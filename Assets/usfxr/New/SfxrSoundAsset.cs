/*
* Copyright 2010 Thomas Vian
* Copyright 2013 Zeh Fernando
* Copyright 2018 Brahim Hadriche
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using UnityEngine;

[CreateAssetMenu(menuName = "Sfxr/Sfxr Sound Asset")]
public class SfxrSoundAsset : ScriptableObject
{
	public enum WaveType : uint {
		Square = 0,
		Sawtooth = 1,
		Sine = 2,
		Noise = 3,
		Triangle = 4,
		PinkNoise = 5,
		Tan = 6,
		Whistle = 7,
		Breaker = 8
	}

	// Properties
	public WaveType	waveType				= 0;	// Shape of wave to generate (see enum WaveType)

	[Range(0f, 1f)]
	public float	masterVolume			= 0.5f;	// Overall volume of the sound (0 to 1)

	[Range(0f, 1f)]
	public float	attackTime				= 0.0f;	// Length of the volume envelope attack (0 to 1)
	[Range(0f, 1f)]
	public float	sustainTime			= 0.0f;	// Length of the volume envelope sustain (0 to 1)
	[Range(0f, 1f)]
	public float	sustainPunch			= 0.0f;	// Tilts the sustain envelope for more 'pop' (0 to 1)
	[Range(0f, 1f)]
	public float	decayTime				= 0.0f;	// Length of the volume envelope decay (yes, I know it's called release) (0 to 1)

	[Range(0f, 1f)]
	public float	startFrequency			= 0.0f;	// Base note of the sound (0 to 1)
	[Range(0f, 1f)]
	public float	minFrequency			= 0.0f;	// If sliding, the sound will stop at this frequency, to prevent really low notes (0 to 1)

	[Range(-1f, 1f)]
	public float	slide					= 0.0f;	// Slides the note up or down (-1 to 1)
	[Range(-1f, 1f)]
	public float	deltaSlide				= 0.0f;	// Accelerates the slide (-1 to 1)

	[Range(0f, 1f)]
	public float	vibratoDepth			= 0.0f;	// Strength of the vibrato effect (0 to 1)
	[Range(0f, 1f)]
	public float	vibratoSpeed			= 0.0f;	// Speed of the vibrato effect (i.e. frequency) (0 to 1)

	[Range(-1f, 1f)]
	public float	changeAmount			= 0.0f;	// Shift in note, either up or down (-1 to 1)
	[Range(0f, 1f)]
	public float	changeSpeed			= 0.0f;	// How fast the note shift happens (only happens once) (0 to 1)

	[Range(0f, 1f)]
	public float	squareDuty				= 0.0f;	// Controls the ratio between the up and down states of the square wave, changing the tibre (0 to 1)
	[Range(-1f, 1f)]
	public float	dutySweep				= 0.0f;	// Sweeps the duty up or down (-1 to 1)

	[Range(0f, 1f)]
	public float	repeatSpeed			= 0.0f;	// Speed of the note repeating - certain variables are reset each time (0 to 1)

	[Range(-1f, 1f)]
	public float	phaserOffset			= 0.0f;	// Offsets a second copy of the wave by a small phase, changing the tibre (-1 to 1)
	[Range(-1f, 1f)]
	public float	phaserSweep			= 0.0f;	// Sweeps the phase up or down (-1 to 1)

	[Range(0f, 1f)]
	public float	lpFilterCutoff			= 0.0f;	// Frequency at which the low-pass filter starts attenuating higher frequencies (0 to 1)
	[Range(-1f, 1f)]
	public float	lpFilterCutoffSweep	= 0.0f;	// Sweeps the low-pass cutoff up or down (-1 to 1)
	[Range(0f, 1f)]
	public float	lpFilterResonance		= 0.0f;	// Changes the attenuation rate for the low-pass filter, changing the timbre (0 to 1)

	[Range(0f, 1f)]
	public float	hpFilterCutoff			= 0.0f;	// Frequency at which the high-pass filter starts attenuating lower frequencies (0 to 1)
	[Range(-1f, 1f)]
	public float	hpFilterCutoffSweep	= 0.0f;	// Sweeps the high-pass cutoff up or down (-1 to 1)

	// From BFXR
	[Range(0f, 1f)]
	public float	changeRepeat			= 0.0f;	// Pitch Jump Repeat Speed: larger Values means more pitch jumps, which can be useful for arpeggiation (0 to 1)
	[Range(-1f, 1f)]
	public float	changeAmount2			= 0.0f;	// Shift in note, either up or down (-1 to 1)
	[Range(0f, 1f)]
	public float	changeSpeed2			= 0.0f;	// How fast the note shift happens (only happens once) (0 to 1)

	[Range(0f, 1f)]
	public float	compressionAmount		= 0.0f;	// Compression: pushes amplitudes together into a narrower range to make them stand out more. Very good for sound effects, where you want them to stick out against background music (0 to 1)

	[Range(0f, 1f)]
	public float	overtones				= 0.0f;	// Harmonics: overlays copies of the waveform with copies and multiples of its frequency. Good for bulking out or otherwise enriching the texture of the sounds (warning: this is the number 1 cause of usfxr slowdown!) (0 to 1)
	[Range(0f, 1f)]
	public float	overtoneFalloff		= 0.0f;	// Harmonics falloff: the rate at which higher overtones should decay (0 to 1)

	[Range(0f, 1f)]
	public float	bitCrush				= 0.0f;	// Bit crush: resamples the audio at a lower frequency (0 to 1)
	[Range(-1f, 1f)]
	public float	bitCrushSweep			= 0.0f;	// Bit crush sweep: sweeps the Bit Crush filter up or down (-1 to 1)

}
