using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct SfxrJob : IJobParallelFor
{
    public struct PinkNumber : IDisposable {
        // Properties
        private int max_key;
        private int key;
        private NativeArray<uint> white_values;
        private uint range;

        // Temp
        private float rangeBy5;
        private int last_key;
        private int diff;

        public static PinkNumber Create()
        {
            var pinkNumber = new PinkNumber();
            pinkNumber.max_key = 0x1f; // Five bits set
            pinkNumber.range = 128;
            pinkNumber.rangeBy5 = (float)pinkNumber.range / 5f;
            pinkNumber.key = 0;
            pinkNumber.white_values = new NativeArray<uint>(5, Allocator.Persistent);
            for (var i = 0; i < 5; i++)
            {
                pinkNumber.white_values[i] = (uint)(getRandom() * pinkNumber.rangeBy5);
            }

            return pinkNumber;
        }

        public float getNextValue() {
            // Returns a number between -1 and 1
            last_key = key;

            key++;
            if (key > max_key) key = 0;

            // Exclusive-Or previous value with current value. This gives
            // a list of bits that have changed.
            diff = last_key ^ key;
            uint sum = 0;
            for (var i  = 0; i < 5; i++) {
                // If bit changed get new random number for corresponding
                // white_value
                if ((diff & (1 << i)) > 0) white_values[i] = (uint)(getRandom() * rangeBy5);;
                sum += white_values[i];
            }
            return sum / 64f - 1f;
        }

        public void Dispose()
        {
            white_values.Dispose();
        }
    };

    private const int LO_RES_NOISE_PERIOD = 8; // Should be < 32

    // Synth properies
    private bool _finished; // If the sound has finished

    private float _masterVolume; // masterVolume * masterVolume (for quick calculations)

    private uint _waveType; // Shape of wave to generate (see enum WaveType)

    private float _envelopeVolume; // Current volume of the envelope
    private int _envelopeStage; // Current stage of the envelope (attack, sustain, decay, end)
    private float _envelopeTime; // Current time through current enelope stage
    private float _envelopeLength; // Length of the current envelope stage
    private float _envelopeLength0; // Length of the attack stage
    private float _envelopeLength1; // Length of the sustain stage
    private float _envelopeLength2; // Length of the decay stage
    private float _envelopeOverLength0; // 1 / _envelopeLength0 (for quick calculations)
    private float _envelopeOverLength1; // 1 / _envelopeLength1 (for quick calculations)
    private float _envelopeOverLength2; // 1 / _envelopeLength2 (for quick calculations)
    public uint _envelopeFullLength; // Full length of the volume envelop (and therefore sound)

    private float _sustainPunch; // The punch factor (louder at begining of sustain)

    private int _phase; // Phase through the wave
    private float _pos; // Phase expresed as a Number from 0-1, used for fast sin approx
    private float _period; // Period of the wave
    private float _periodTemp; // Period modified by vibrato
    private int _periodTempInt; // Period modified by vibrato (as an Int)
    private float _maxPeriod; // Maximum period before sound stops (from minFrequency)

    private float _slide; // Note slide
    private float _deltaSlide; // Change in slide
    private float _minFrequency; // Minimum frequency before stopping

    private float _vibratoPhase; // Phase through the vibrato sine wave
    private float _vibratoSpeed; // Speed at which the vibrato phase moves
    private float _vibratoAmplitude; // Amount to change the period of the wave by at the peak of the vibrato wave

    private float _changeAmount; // Amount to change the note by
    private int _changeTime; // Counter for the note change
    private int _changeLimit; // Once the time reaches this limit, the note changes

    private float _squareDuty; // Offset of center switching point in the square wave
    private float _dutySweep; // Amount to change the duty by

//	private int			_repeatTime;						// Counter for the repeats
    private int _repeatLimit; // Once the time reaches this limit, some of the variables are reset

    private bool _phaser; // If the phaser is active
    private float _phaserOffset; // Phase offset for phaser effect
    private float _phaserDeltaOffset; // Change in phase offset
    private int _phaserInt; // Integer phaser offset, for bit maths
    private int _phaserPos; // Position through the phaser buffer
    private NativeArray<float> _phaserBuffer; // Buffer of wave values used to create the out of phase second wave

    private bool _filters; // If the filters are active
    private float _lpFilterPos; // Adjusted wave position after low-pass filter
    private float _lpFilterOldPos; // Previous low-pass wave position
    private float _lpFilterDeltaPos; // Change in low-pass wave position, as allowed by the cutoff and damping
    private float _lpFilterCutoff; // Cutoff multiplier which adjusts the amount the wave position can move
    private float _lpFilterDeltaCutoff; // Speed of the low-pass cutoff multiplier
    private float _lpFilterDamping; // Damping muliplier which restricts how fast the wave position can move
    private bool _lpFilterOn; // If the low pass filter is active

    private float _hpFilterPos; // Adjusted wave position after high-pass filter
    private float _hpFilterCutoff; // Cutoff multiplier which adjusts the amount the wave position can move
    private float _hpFilterDeltaCutoff; // Speed of the high-pass cutoff multiplier

    // From BFXR
    private float _changePeriod;
    private int _changePeriodTime;

    private bool _changeReached;

    private float _changeAmount2; // Amount to change the note by
    private int _changeTime2; // Counter for the note change
    private int _changeLimit2; // Once the time reaches this limit, the note changes
    private bool _changeReached2;

    private int _overtones; // Minimum frequency before stopping
    private float _overtoneFalloff; // Minimum frequency before stopping

    private float _bitcrushFreq; // Inversely proportional to the number of samples to skip
    private float _bitcrushFreqSweep; // Change of the above
    private float _bitcrushPhase; // Samples when this > 1
    private float _bitcrushLast; // Last sample value

    private float _compressionFactor;

    // Pre-calculated data
    private NativeArray<float> _noiseBuffer; // Buffer of random values used to generate noise
    private NativeArray<float> _pinkNoiseBuffer; // Buffer of random values used to generate pink noise
    private PinkNumber _pinkNumber; // Used to generate pink noise
    private NativeArray<float> _loResNoiseBuffer; // Buffer of random values used to generate Tan waveform

    // Temp
    private float _superSample; // Actual sample writen to the wave
    private float _sample; // Sub-sample calculated 8 times per actual sample, averaged out to get the super sample
    private float _sample2; // Used in other calculations
    private float amp; // Used in other calculations

    public NativeArray<float> buffer;

    public void Execute(int i)
    {
        if (_finished) return;

        // Repeats every _repeatLimit times, partially resetting the sound parameters
        if (_repeatLimit != 0 && (i - 1) % _repeatLimit == 0)
        {
            //Reset(false);
        }

        _changePeriodTime++;
        if (_changePeriodTime >= _changePeriod)
        {
            _changeTime = 0;
            _changeTime2 = 0;
            _changePeriodTime = 0;
            if (_changeReached)
            {
                _period /= _changeAmount;
                _changeReached = false;
            }

            if (_changeReached2)
            {
                _period /= _changeAmount2;
                _changeReached2 = false;
            }
        }

        // If _changeLimit is reached, shifts the pitch
        if (!_changeReached)
        {
            if (++_changeTime >= _changeLimit)
            {
                _changeReached = true;
                _period *= _changeAmount;
            }
        }

        // If _changeLimit is reached, shifts the pitch
        if (!_changeReached2)
        {
            if (++_changeTime2 >= _changeLimit2)
            {
                _changeReached2 = true;
                _period *= _changeAmount2;
            }
        }

        // Acccelerate and apply slide
        _slide += _deltaSlide;
        _period *= _slide;

        // Checks for frequency getting too low, and stops the sound if a minFrequency was set
        if (_period > _maxPeriod)
        {
            _period = _maxPeriod;
            if (_minFrequency > 0) _finished = true;
        }

        _periodTemp = _period;

        // Applies the vibrato effect
        if (_vibratoAmplitude > 0)
        {
            _vibratoPhase += _vibratoSpeed;
            _periodTemp = _period * (1.0f + math.sin(_vibratoPhase) * _vibratoAmplitude);
        }

        _periodTempInt = (int) _periodTemp;
        if (_periodTemp < 8) _periodTemp = _periodTempInt = 8;

        // Sweeps the square duty
        if (_waveType == 0)
        {
            _squareDuty = math.clamp(_squareDuty + _dutySweep, 0f, 0.5f);
        }

        // Moves through the different stages of the volume envelope
        if (++_envelopeTime > _envelopeLength)
        {
            _envelopeTime = 0;

            switch (++_envelopeStage)
            {
                case 1:
                    _envelopeLength = _envelopeLength1;
                    break;
                case 2:
                    _envelopeLength = _envelopeLength2;
                    break;
            }
        }

        // Sets the volume based on the position in the envelope
        switch (_envelopeStage)
        {
            case 0:
                _envelopeVolume = _envelopeTime * _envelopeOverLength0;
                break;
            case 1:
                _envelopeVolume = 1.0f + (1.0f - _envelopeTime * _envelopeOverLength1) * 2.0f * _sustainPunch;
                break;
            case 2:
                _envelopeVolume = 1.0f - _envelopeTime * _envelopeOverLength2;
                break;
            case 3:
                _envelopeVolume = 0.0f;
                _finished = true;
                break;
        }

        // Moves the phaser offset
        if (_phaser)
        {
            _phaserOffset += _phaserDeltaOffset;
            _phaserInt = (int) _phaserOffset;
            if (_phaserInt < 0)
            {
                _phaserInt = -_phaserInt;
            }
            else if (_phaserInt > 1023)
            {
                _phaserInt = 1023;
            }
        }

        // Moves the high-pass filter cutoff
        if (_filters && _hpFilterDeltaCutoff != 0)
        {
            _hpFilterCutoff *= _hpFilterDeltaCutoff;
            if (_hpFilterCutoff < 0.00001f)
            {
                _hpFilterCutoff = 0.00001f;
            }
            else if (_hpFilterCutoff > 0.1f)
            {
                _hpFilterCutoff = 0.1f;
            }
        }

        _superSample = 0;
        for (var j = 0; j < 8; j++)
        {
            // Cycles through the period
            _phase++;
            if (_phase >= _periodTempInt)
            {
                _phase = _phase % _periodTempInt;

                // Generates new random noise for this period
                if (_waveType == 3)
                {
                    // Noise
                    for (var n = 0; n < 32; n++) _noiseBuffer[n] = getRandom() * 2.0f - 1.0f;
                }
                else if (_waveType == 5)
                {
                    // Pink noise
                    for (var n = 0; n < 32; n++) _pinkNoiseBuffer[n] = _pinkNumber.getNextValue();
                }
                else if (_waveType == 6)
                {
                    // Tan
                    for (var n = 0; n < 32; n++)
                        _loResNoiseBuffer[n] = ((n % LO_RES_NOISE_PERIOD) == 0)
                            ? getRandom() * 2.0f - 1.0f
                            : _loResNoiseBuffer[n - 1];
                }
            }

            _sample = 0;
            var sampleTotal = 0f;
            var overtoneStrength = 1f;

            for (var k = 0; k <= _overtones; k++)
            {
                var tempPhase = (float) ((_phase * (k + 1))) % _periodTemp;

                // Gets the sample from the oscillator
                switch (_waveType)
                {
                    case 0:
                        // Square
                        _sample = ((tempPhase / _periodTemp) < _squareDuty) ? 0.5f : -0.5f;
                        break;
                    case 1:
                        // Sawtooth
                        _sample = 1.0f - (tempPhase / _periodTemp) * 2.0f;
                        break;
                    case 2:
                        // Sine: fast and accurate approx
                        _pos = tempPhase / _periodTemp;
                        _pos = _pos > 0.5f ? (_pos - 1.0f) * 6.28318531f : _pos * 6.28318531f;
                        _sample = _pos < 0
                            ? 1.27323954f * _pos + 0.405284735f * _pos * _pos
                            : 1.27323954f * _pos - 0.405284735f * _pos * _pos;
                        _sample = _sample < 0
                            ? 0.225f * (_sample * -_sample - _sample) + _sample
                            : 0.225f * (_sample * _sample - _sample) + _sample;
                        break;
                    case 3:
                        // Noise
                        _sample = _noiseBuffer[(int) (tempPhase * 32f / _periodTempInt) % 32];
                        break;
                    case 4:
                        // Triangle
                        _sample = math.abs(1f - (tempPhase / _periodTemp) * 2f) - 1f;
                        break;
                    case 5:
                        // Pink noise
                        _sample = _pinkNoiseBuffer[(int) (tempPhase * 32f / _periodTempInt) % 32];
                        break;
                    case 6:
                        // Tan
                        _sample = (float) math.tan(math.PI * tempPhase / _periodTemp);
                        break;
                    case 7:
                        // Whistle
                        // Sine wave code
                        _pos = tempPhase / _periodTemp;
                        _pos = _pos > 0.5f ? (_pos - 1.0f) * 6.28318531f : _pos * 6.28318531f;
                        _sample = _pos < 0
                            ? 1.27323954f * _pos + 0.405284735f * _pos * _pos
                            : 1.27323954f * _pos - 0.405284735f * _pos * _pos;
                        _sample = 0.75f * (_sample < 0
                                      ? 0.225f * (_sample * -_sample - _sample) + _sample
                                      : 0.225f * (_sample * _sample - _sample) + _sample);
                        // Then whistle (essentially an overtone with frequencyx20 and amplitude0.25
                        _pos = ((tempPhase * 20f) % _periodTemp) / _periodTemp;
                        _pos = _pos > 0.5f ? (_pos - 1.0f) * 6.28318531f : _pos * 6.28318531f;
                        _sample2 = _pos < 0
                            ? 1.27323954f * _pos + .405284735f * _pos * _pos
                            : 1.27323954f * _pos - 0.405284735f * _pos * _pos;
                        _sample += 0.25f * (_sample2 < 0
                                       ? .225f * (_sample2 * -_sample2 - _sample2) + _sample2
                                       : .225f * (_sample2 * _sample2 - _sample2) + _sample2);
                        break;
                    case 8:
                        // Breaker
                        amp = tempPhase / _periodTemp;
                        _sample = math.abs(1f - amp * amp * 2f) - 1f;
                        break;
                }

                sampleTotal += overtoneStrength * _sample;
                overtoneStrength *= (1f - _overtoneFalloff);
            }

            _sample = sampleTotal;

            // Applies the low and high pass filters
            if (_filters)
            {
                _lpFilterOldPos = _lpFilterPos;
                _lpFilterCutoff *= _lpFilterDeltaCutoff;
                if (_lpFilterCutoff < 0.0)
                {
                    _lpFilterCutoff = 0.0f;
                }
                else if (_lpFilterCutoff > 0.1)
                {
                    _lpFilterCutoff = 0.1f;
                }

                if (_lpFilterOn)
                {
                    _lpFilterDeltaPos += (_sample - _lpFilterPos) * _lpFilterCutoff;
                    _lpFilterDeltaPos *= _lpFilterDamping;
                }
                else
                {
                    _lpFilterPos = _sample;
                    _lpFilterDeltaPos = 0.0f;
                }

                _lpFilterPos += _lpFilterDeltaPos;

                _hpFilterPos += _lpFilterPos - _lpFilterOldPos;
                _hpFilterPos *= 1.0f - _hpFilterCutoff;
                _sample = _hpFilterPos;
            }

            // Applies the phaser effect
            if (_phaser)
            {
                _phaserBuffer[_phaserPos & 1023] = _sample;
                _sample += _phaserBuffer[(_phaserPos - _phaserInt + 1024) & 1023];
                _phaserPos = (_phaserPos + 1) & 1023;
            }

            _superSample += _sample;
        }

        // Averages out the super samples and applies volumes
        _superSample = _masterVolume * _envelopeVolume * _superSample * 0.125f;

        // Bit crush
        _bitcrushPhase += _bitcrushFreq;
        if (_bitcrushPhase > 1f)
        {
            _bitcrushPhase = 0;
            _bitcrushLast = _superSample;
        }

        _bitcrushFreq = math.clamp(_bitcrushFreq + _bitcrushFreqSweep, 0, 1);

        _superSample = _bitcrushLast;

        // Compressor
        _superSample = math.sign(_superSample) * math.pow(math.abs(_superSample), _compressionFactor);

        // BFXR leftover:
        //if (_muted) {
        //	_superSample = 0;
        //}

        // Clipping if too loud
        _superSample = math.clamp(_superSample, -1, 1);

        // Writes value to list, ignoring left/right sound channels (this is applied when filtering the audio later)
        buffer[i] = _superSample;
    }

    private void Reset(SfxrSoundAsset soundAsset, bool __totalReset)
    {
        _period = 100.0f / (soundAsset.startFrequency * soundAsset.startFrequency + 0.001f);
        _maxPeriod = 100.0f / (soundAsset.minFrequency * soundAsset.minFrequency + 0.001f);

        _slide = 1.0f - soundAsset.slide * soundAsset.slide * soundAsset.slide * 0.01f;
        _deltaSlide = -soundAsset.deltaSlide * soundAsset.deltaSlide * soundAsset.deltaSlide * 0.000001f;

        if (soundAsset.waveType == 0)
        {
            _squareDuty = 0.5f - soundAsset.squareDuty * 0.5f;
            _dutySweep = -soundAsset.dutySweep * 0.00005f;
        }

        _changePeriod = (((1f - soundAsset.changeRepeat) + 0.1f) / 1.1f) * 20000f + 32f;
        _changePeriodTime = 0;

        if (soundAsset.changeAmount > 0.0)
        {
            _changeAmount = 1.0f - soundAsset.changeAmount * soundAsset.changeAmount * 0.9f;
        }
        else
        {
            _changeAmount = 1.0f + soundAsset.changeAmount * soundAsset.changeAmount * 10.0f;
        }

        _changeTime = 0;
        _changeReached = false;

        if (soundAsset.changeSpeed == 1.0f)
        {
            _changeLimit = 0;
        }
        else
        {
            _changeLimit = (int) ((1f - soundAsset.changeSpeed) * (1f - soundAsset.changeSpeed) * 20000f + 32f);
        }

        if (soundAsset.changeAmount2 > 0f)
        {
            _changeAmount2 = 1f - soundAsset.changeAmount2 * soundAsset.changeAmount2 * 0.9f;
        }
        else
        {
            _changeAmount2 = 1f + soundAsset.changeAmount2 * soundAsset.changeAmount2 * 10f;
        }

        _changeTime2 = 0;
        _changeReached2 = false;

        if (soundAsset.changeSpeed2 == 1.0f)
        {
            _changeLimit2 = 0;
        }
        else
        {
            _changeLimit2 = (int) ((1f - soundAsset.changeSpeed2) * (1f - soundAsset.changeSpeed2) * 20000f + 32f);
        }

        _changeLimit = (int) (_changeLimit * ((1f - soundAsset.changeRepeat + 0.1f) / 1.1f));
        _changeLimit2 = (int) (_changeLimit2 * ((1f - soundAsset.changeRepeat + 0.1f) / 1.1f));

        if (__totalReset)
        {
            _masterVolume = soundAsset.masterVolume * soundAsset.masterVolume;

            _waveType = (uint) soundAsset.waveType;

            if (soundAsset.sustainTime < 0.01) soundAsset.sustainTime = 0.01f;

            float totalTime = soundAsset.attackTime + soundAsset.sustainTime + soundAsset.decayTime;
            if (totalTime < 0.18f)
            {
                float multiplier = 0.18f / totalTime;
                soundAsset.attackTime *= multiplier;
                soundAsset.sustainTime *= multiplier;
                soundAsset.decayTime *= multiplier;
            }

            _sustainPunch = soundAsset.sustainPunch;

            _phase = 0;

            _overtones = (int) (soundAsset.overtones * 10f);
            _overtoneFalloff = soundAsset.overtoneFalloff;

            _minFrequency = soundAsset.minFrequency;

            _bitcrushFreq = 1f - math.pow(soundAsset.bitCrush, 1f / 3f);
            _bitcrushFreqSweep = -soundAsset.bitCrushSweep * 0.000015f;
            _bitcrushPhase = 0;
            _bitcrushLast = 0;

            _compressionFactor = 1f / (1f + 4f * soundAsset.compressionAmount);

            _filters = soundAsset.lpFilterCutoff != 1.0 || soundAsset.hpFilterCutoff != 0.0;

            _lpFilterPos = 0.0f;
            _lpFilterDeltaPos = 0.0f;
            _lpFilterCutoff = soundAsset.lpFilterCutoff * soundAsset.lpFilterCutoff * soundAsset.lpFilterCutoff * 0.1f;
            _lpFilterDeltaCutoff = 1.0f + soundAsset.lpFilterCutoffSweep * 0.0001f;
            _lpFilterDamping = 5.0f / (1.0f + soundAsset.lpFilterResonance * soundAsset.lpFilterResonance * 20.0f) *
                               (0.01f + _lpFilterCutoff);
            if (_lpFilterDamping > 0.8f) _lpFilterDamping = 0.8f;
            _lpFilterDamping = 1.0f - _lpFilterDamping;
            _lpFilterOn = soundAsset.lpFilterCutoff != 1.0f;

            _hpFilterPos = 0.0f;
            _hpFilterCutoff = soundAsset.hpFilterCutoff * soundAsset.hpFilterCutoff * 0.1f;
            _hpFilterDeltaCutoff = 1.0f + soundAsset.hpFilterCutoffSweep * 0.0003f;

            _vibratoPhase = 0.0f;
            _vibratoSpeed = soundAsset.vibratoSpeed * soundAsset.vibratoSpeed * 0.01f;
            _vibratoAmplitude = soundAsset.vibratoDepth * 0.5f;

            _envelopeVolume = 0.0f;
            _envelopeStage = 0;
            _envelopeTime = 0;
            _envelopeLength0 = soundAsset.attackTime * soundAsset.attackTime * 100000.0f;
            _envelopeLength1 = soundAsset.sustainTime * soundAsset.sustainTime * 100000.0f;
            _envelopeLength2 = soundAsset.decayTime * soundAsset.decayTime * 100000.0f + 10f;
            _envelopeLength = _envelopeLength0;
            _envelopeFullLength = (uint) (_envelopeLength0 + _envelopeLength1 + _envelopeLength2);

            _envelopeOverLength0 = 1.0f / _envelopeLength0;
            _envelopeOverLength1 = 1.0f / _envelopeLength1;
            _envelopeOverLength2 = 1.0f / _envelopeLength2;

            _phaser = soundAsset.phaserOffset != 0.0f || soundAsset.phaserSweep != 0.0f;

            _phaserOffset = soundAsset.phaserOffset * soundAsset.phaserOffset * 1020.0f;
            if (soundAsset.phaserOffset < 0.0f) _phaserOffset = -_phaserOffset;
            _phaserDeltaOffset = soundAsset.phaserSweep * soundAsset.phaserSweep * soundAsset.phaserSweep * 0.2f;
            _phaserPos = 0;

            int i;
            for (i = 0; i < 1024; i++) _phaserBuffer[i] = 0.0f;
            for (i = 0; i < 32; i++) _noiseBuffer[i] = getRandom() * 2.0f - 1.0f;
            for (i = 0; i < 32; i++) _pinkNoiseBuffer[i] = _pinkNumber.getNextValue();
            for (i = 0; i < 32; i++)
                _loResNoiseBuffer[i] = ((i % LO_RES_NOISE_PERIOD) == 0)
                    ? getRandom() * 2.0f - 1.0f
                    : _loResNoiseBuffer[i - 1];

            if (soundAsset.repeatSpeed == 0.0)
            {
                _repeatLimit = 0;
            }
            else
            {
                _repeatLimit = (int) ((1.0 - soundAsset.repeatSpeed) * (1.0 - soundAsset.repeatSpeed) * 20000) + 32;
            }
        }
    }

    private static float getRandom()
    {
        // We can't use Unity's Random.value because it cannot be called from a separate thread
        // (We get the error "get_value can only be called from the main thread" when this is called to generate the sound data)
        return (float) (randomGenerator.NextDouble() % 1);
    }

    private static System.Random randomGenerator = new System.Random(); // Used to generate random numbers safely

    public static AudioClip ScheduleAndExecute(SfxrSoundAsset soundAsset)
    {
        var job = new SfxrJob();
        
        job._pinkNumber = PinkNumber.Create();
        job._phaserBuffer  = new NativeArray<float>(1024, Allocator.Persistent);
        job._noiseBuffer  = new NativeArray<float>(32, Allocator.Persistent);
        job._pinkNoiseBuffer  = new NativeArray<float>(32, Allocator.Persistent);
        job._loResNoiseBuffer  = new NativeArray<float>(32, Allocator.Persistent);
        
        job.Reset(soundAsset, true);

        job.buffer = new NativeArray<float>((int)job._envelopeFullLength, Allocator.Persistent);
        
        JobHandle jobHandle = job.Schedule(job.buffer.Length, 1);
        jobHandle.Complete();
        
        var clip = AudioClip.Create("SfxrSoundClip", (int)job._envelopeFullLength , 1, 44100, false);

        clip.SetData(job.buffer.ToArray(), 0);
        
        job.buffer.Dispose();
        
        job._pinkNumber.Dispose();
        job._phaserBuffer.Dispose();
        job._noiseBuffer.Dispose();
        job._pinkNoiseBuffer.Dispose();
        job._loResNoiseBuffer.Dispose();
        
        return clip;

    }
    
}