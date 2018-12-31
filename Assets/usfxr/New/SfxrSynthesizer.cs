using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
 
public class SfxrSynthesizer 
{
    public static async Task<AudioClip> GenerateSoundAsync(SfxrSoundAsset asset, CancellationToken cancellationToken)
    {
        var synth = new SfxrSynth();
        synth.parameters = asset;
        synth.Reset(true);
        
        var clip = AudioClip.Create("SfxrSoundClip", (int) synth._envelopeFullLength, 1, 44100, false);

        float[] audioData = new float[clip.samples];
        clip.GetData(audioData, 0);
        await Task.Run(() => { synth.SynthWave(audioData, 0, (uint) audioData.Length); }, cancellationToken);

        clip.SetData(audioData, 0);

        return clip;
    }
    
    public static AudioClip GenerateSoundJob(SfxrSoundAsset asset)
    {
        return SfxrJob.ScheduleAndExecute(asset);
    }

}
