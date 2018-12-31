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

    public async void GenerateSoundClip()
    {
        audioSource.clip = await SfxrSynthesizer.GenerateSound(soundAsset, CancellationToken.None);
    }

    public void GenerateSoundClipAndPlay(ulong delay = 0UL)
    {
        GenerateSoundClip();
        audioSource.Play(delay);
    }
}
