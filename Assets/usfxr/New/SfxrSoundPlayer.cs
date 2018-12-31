using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxrSoundPlayer : MonoBehaviour
{
    public SfxrSoundAsset soundAsset;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public async void GenerateSoundClipAsync()
    {
        audioSource.clip = await SfxrSynthesizer.GenerateSoundAsync(soundAsset, CancellationToken.None);
    }

    public void GenerateSoundClip()
    {
        audioSource.clip = SfxrSynthesizer.GenerateSoundJob(soundAsset);
    }

    public void GenerateSoundClipAndPlay(ulong delay = 0UL)
    {
        GenerateSoundClip();
        audioSource.Play(delay);
    }
}
