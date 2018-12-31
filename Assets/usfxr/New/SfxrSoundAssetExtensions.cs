using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SfxrSoundAssetExtensions
{
	public static void GeneratePickupCoin(this SfxrSoundAsset asset) {
		asset.ResetParams();

		asset.startFrequency = 0.4f + GetRandom() * 0.5f;

		asset.sustainTime = GetRandom() * 0.1f;
		asset.decayTime = 0.1f + GetRandom() * 0.4f;
		asset.sustainPunch = 0.3f + GetRandom() * 0.3f;

		if (GetRandomBool()) {
			asset.changeSpeed = 0.5f + GetRandom() * 0.2f;
			int cnum = (int)(GetRandom()*7f) + 1;
			int cden = cnum + (int)(GetRandom()*7f) + 2;
			asset.changeAmount = (float)cnum / (float)cden;
		}
	}
	
	public static void GenerateLaserShoot(this SfxrSoundAsset asset) {
		asset.ResetParams();

		asset.waveType = (SfxrSoundAsset.WaveType)(GetRandom() * 3);
		if (asset.waveType == SfxrSoundAsset.WaveType.Sine && GetRandomBool()) 
			asset.waveType = (SfxrSoundAsset.WaveType)(GetRandom() * 2f);

		asset.startFrequency = 0.5f + GetRandom() * 0.5f;
		asset.minFrequency = asset.startFrequency - 0.2f - GetRandom() * 0.6f;
		if (asset.minFrequency < 0.2f) asset.minFrequency = 0.2f;

		asset.slide = -0.15f - GetRandom() * 0.2f;

		if (GetRandom() < 0.33f) {
			asset.startFrequency = 0.3f + GetRandom() * 0.6f;
			asset.minFrequency = GetRandom() * 0.1f;
			asset.slide = -0.35f - GetRandom() * 0.3f;
		}

		if (GetRandomBool()) {
			asset.squareDuty = GetRandom() * 0.5f;
			asset.dutySweep = GetRandom() * 0.2f;
		} else {
			asset.squareDuty = 0.4f + GetRandom() * 0.5f;
			asset.dutySweep = -GetRandom() * 0.7f;
		}

		asset.sustainTime = 0.1f + GetRandom() * 0.2f;
		asset.decayTime = GetRandom() * 0.4f;
		if (GetRandomBool()) asset.sustainPunch = GetRandom() * 0.3f;

		if (GetRandom() < 0.33f) {
			asset.phaserOffset = GetRandom() * 0.2f;
			asset.phaserSweep = -GetRandom() * 0.2f;
		}

		if (GetRandomBool()) asset.hpFilterCutoff = GetRandom() * 0.3f;
	}
	
	public static void GenerateExplosion(this SfxrSoundAsset asset) {
		asset.ResetParams();

		asset.waveType =  SfxrSoundAsset.WaveType.Noise;

		if (GetRandomBool()) {
			asset.startFrequency = 0.1f + GetRandom() * 0.4f;
			asset.slide = -0.1f + GetRandom() * 0.4f;
		} else {
			asset.startFrequency = 0.2f + GetRandom() * 0.7f;
			asset.slide = -0.2f - GetRandom() * 0.2f;
		}

		asset.startFrequency *= asset.startFrequency;

		if (GetRandom() < 0.2f) asset.slide = 0.0f;
		if (GetRandom() < 0.33f) asset.repeatSpeed = 0.3f + GetRandom() * 0.5f;

		asset.sustainTime = 0.1f + GetRandom() * 0.3f;
		asset.decayTime = GetRandom() * 0.5f;
		asset.sustainPunch = 0.2f + GetRandom() * 0.6f;

		if (GetRandomBool()) {
			asset.phaserOffset = -0.3f + GetRandom() * 0.9f;
			asset.phaserSweep = -GetRandom() * 0.3f;
		}

		if (GetRandom() < 0.33f) {
			asset.changeSpeed = 0.6f + GetRandom() * 0.3f;
			asset.changeAmount = 0.8f - GetRandom() * 1.6f;
		}
	}
	
	public static void GeneratePowerup(this SfxrSoundAsset asset) {
		asset.ResetParams();

		if (GetRandomBool()) {
			asset.waveType = SfxrSoundAsset.WaveType.Sawtooth;
		} else {
			asset.squareDuty = GetRandom() * 0.6f;
		}

		if (GetRandomBool()) {
			asset.startFrequency = 0.2f + GetRandom() * 0.3f;
			asset.slide = 0.1f + GetRandom() * 0.4f;
			asset.repeatSpeed = 0.4f + GetRandom() * 0.4f;
		} else {
			asset.startFrequency = 0.2f + GetRandom() * 0.3f;
			asset.slide = 0.05f + GetRandom() * 0.2f;

			if (GetRandomBool()) {
				asset.vibratoDepth = GetRandom() * 0.7f;
				asset.vibratoSpeed = GetRandom() * 0.6f;
			}
		}

		asset.sustainTime = GetRandom() * 0.4f;
		asset.decayTime = 0.1f + GetRandom() * 0.4f;
	}
	
	public static void GenerateHitHurt(this SfxrSoundAsset asset) {
		asset.ResetParams();

		asset.waveType = (SfxrSoundAsset.WaveType)(GetRandom() * 3f);
		if (asset.waveType == SfxrSoundAsset.WaveType.Sine) {
			asset.waveType = SfxrSoundAsset.WaveType.Noise;
		} else if (asset.waveType == SfxrSoundAsset.WaveType.Square) {
			asset.squareDuty = GetRandom() * 0.6f;
		}

		asset.startFrequency = 0.2f + GetRandom() * 0.6f;
		asset.slide = -0.3f - GetRandom() * 0.4f;

		asset.sustainTime = GetRandom() * 0.1f;
		asset.decayTime = 0.1f + GetRandom() * 0.2f;

		if (GetRandomBool()) asset.hpFilterCutoff = GetRandom() * 0.3f;
	}
	
	public static void GenerateBlipSelect(this SfxrSoundAsset asset) {
		asset.ResetParams();

		asset.waveType = (SfxrSoundAsset.WaveType)(GetRandom() * 2f);
		if (asset.waveType == SfxrSoundAsset.WaveType.Square) asset.squareDuty = GetRandom() * 0.6f;

		asset.startFrequency = 0.2f + GetRandom() * 0.4f;

		asset.sustainTime = 0.1f + GetRandom() * 0.1f;
		asset.decayTime = GetRandom() * 0.2f;
		asset.hpFilterCutoff = 0.1f;
	}

    public static void GenerateJump(this SfxrSoundAsset asset) {
        asset.ResetParams();

        asset.waveType = SfxrSoundAsset.WaveType.Square;
        asset. squareDuty = GetRandom() * 0.6f;
        asset. startFrequency = 0.3f + GetRandom() * 0.3f;
        asset. slide = 0.1f + GetRandom() * 0.2f;

        asset.sustainTime = 0.1f + GetRandom() * 0.3f;
        asset.decayTime = 0.1f + GetRandom() * 0.2f;

        if (GetRandomBool()) asset.hpFilterCutoff = GetRandom() * 0.3f;
        if (GetRandomBool()) asset.lpFilterCutoff = 1.0f - GetRandom() * 0.6f;
    }

    public static void Randomize(this SfxrSoundAsset asset) {
	    asset.ResetParams();

	    asset.waveType = (SfxrSoundAsset.WaveType)(GetRandom() * 9f);

	    asset.attackTime				= Mathf.Pow(GetRandom() * 2f - 1f, 4);
	    asset.sustainTime			= Mathf.Pow(GetRandom() * 2f - 1f, 2);
	    asset.sustainPunch			= Mathf.Pow(GetRandom() * 0.8f, 2);
	    asset.decayTime				= GetRandom();

	    asset.startFrequency			= (GetRandomBool()) ? Mathf.Pow(GetRandom() * 2f - 1f, 2) : (Mathf.Pow(GetRandom() * 0.5f, 3) + 0.5f);
	    asset.minFrequency			= 0.0f;

	    asset.slide					= Mathf.Pow(GetRandom() * 2f - 1f, 3);
	    asset.deltaSlide				= Mathf.Pow(GetRandom() * 2f - 1f, 3);

	    asset.vibratoDepth			= Mathf.Pow(GetRandom() * 2f - 1f, 3);
	    asset.vibratoSpeed			= GetRandom() * 2f - 1f;

	    asset.changeAmount			= GetRandom() * 2f - 1f;
	    asset.changeSpeed			= GetRandom() * 2f - 1f;

	    asset.squareDuty				= GetRandom() * 2f - 1f;
	    asset.dutySweep				= Mathf.Pow(GetRandom() * 2f - 1f, 3);

	    asset.repeatSpeed			= GetRandom() * 2f - 1f;

	    asset.phaserOffset			= Mathf.Pow(GetRandom() * 2f - 1f, 3);
	    asset.phaserSweep			= Mathf.Pow(GetRandom() * 2f - 1f, 3);

	    asset.lpFilterCutoff			= 1f - Mathf.Pow(GetRandom(), 3);
	    asset.lpFilterCutoffSweep	= Mathf.Pow(GetRandom() * 2f - 1f, 3);
	    asset.lpFilterResonance		= GetRandom() * 2f - 1f;

	    asset.hpFilterCutoff			= Mathf.Pow(GetRandom(), 5);
	    asset.hpFilterCutoffSweep	= Mathf.Pow(GetRandom() * 2f - 1f, 5);

	    if (asset.attackTime + asset.sustainTime + asset.decayTime < 0.2f) {
		    asset.sustainTime		= 0.2f + GetRandom() * 0.3f;
		    asset.decayTime			= 0.2f + GetRandom() * 0.3f;
	    }

	    if ((asset.startFrequency > 0.7f && asset.slide > 0.2) || (asset.startFrequency < 0.2 && asset.slide < -0.05)) {
		    asset.slide				= -asset.slide;
	    }

	    if (asset.lpFilterCutoff < 0.1f && asset.lpFilterCutoffSweep < -0.05f) {
		    asset.lpFilterCutoffSweep = -asset.lpFilterCutoffSweep;
	    }

	    // From BFXR
	    asset.changeRepeat			= GetRandom();
	    asset.changeAmount2			= GetRandom() * 2f - 1f;
	    asset.changeSpeed2			= GetRandom();

	    asset.compressionAmount		= GetRandom();

	    asset.overtones				= GetRandom();
	    asset.overtoneFalloff		= GetRandom();

	    asset.bitCrush				= GetRandom();
	    asset.bitCrushSweep			= GetRandom() * 2f - 1f;
    }

	public static void Mutate(this SfxrSoundAsset asset, float mutation = 0.05f) {
		if (GetRandomBool()) asset.startFrequency			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.minFrequency			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.slide					+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.deltaSlide				+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.squareDuty				+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.dutySweep				+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.vibratoDepth			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.vibratoSpeed			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.attackTime				+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.sustainTime			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.decayTime				+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.sustainPunch			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.lpFilterCutoff			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.lpFilterCutoffSweep	+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.lpFilterResonance		+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.hpFilterCutoff			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.hpFilterCutoffSweep	+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.phaserOffset			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.phaserSweep			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.repeatSpeed			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.changeSpeed			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.changeAmount			+= GetRandom() * mutation * 2f - mutation;

		// From BFXR
		if (GetRandomBool()) asset.changeRepeat			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.changeAmount2			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.changeSpeed2			+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.compressionAmount		+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.overtones				+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.overtoneFalloff		+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.bitCrush				+= GetRandom() * mutation * 2f - mutation;
		if (GetRandomBool()) asset.bitCrushSweep			+= GetRandom() * mutation * 2f - mutation;

		asset.EnforceRangeAttributes();
	}

	private static void ResetParams(this SfxrSoundAsset asset) {
		asset.waveType =  SfxrSoundAsset.WaveType.Square;
		asset.startFrequency			= 0.3f;
		asset.minFrequency			= 0.0f;
		asset.slide					= 0.0f;
		asset.deltaSlide				= 0.0f;
		asset.squareDuty				= 0.0f;
		asset.dutySweep				= 0.0f;

		asset.vibratoDepth			= 0.0f;
		asset.vibratoSpeed			= 0.0f;

		asset.attackTime				= 0.0f;
		asset.sustainTime			    = 0.3f;
		asset.decayTime				= 0.4f;
		asset.sustainPunch			= 0.0f;

		asset.lpFilterResonance		= 0.0f;
		asset.lpFilterCutoff			= 1.0f;
		asset.lpFilterCutoffSweep	    = 0.0f;
		asset.hpFilterCutoff			= 0.0f;
		asset.hpFilterCutoffSweep	    = 0.0f;

		asset.phaserOffset			= 0.0f;
		asset.phaserSweep			    = 0.0f;

		asset.repeatSpeed			    = 0.0f;

		asset.changeSpeed			    = 0.0f;
		asset.changeAmount			= 0.0f;

		// From BFXR
		asset.changeRepeat			= 0.0f;
		asset.changeAmount2			= 0.0f;
		asset.changeSpeed2			= 0.0f;

		asset.compressionAmount		= 0.3f;

		asset.overtones				= 0.0f;
		asset.overtoneFalloff		    = 0.0f;

		asset.bitCrush				= 0.0f;
		asset.bitCrushSweep			= 0.0f;
	}

	private static void EnforceRangeAttributes(this SfxrSoundAsset asset)
	{
		var fields = typeof(SfxrSoundAsset).GetFields();
		foreach (var fieldInfo in fields)
		{
			var attr = (RangeAttribute)fieldInfo.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault();
			if (attr != null)
			{
				fieldInfo.SetValue(asset, Mathf.Clamp((float)fieldInfo.GetValue(asset), attr.min, attr.max));
			}
		}
	}

    private static float GetRandom() {
        return UnityEngine.Random.value % 1;
    }

    private static bool GetRandomBool() {
        return UnityEngine.Random.value > 0.5f;
    }
}
