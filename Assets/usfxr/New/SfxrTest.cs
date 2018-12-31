using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxrTest : MonoBehaviour
{
    private SfxrSoundPlayer player;
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<SfxrSoundPlayer>();
    }
 
    private void OnGUI()
    {
        if (GUILayout.Button("Generate and Play"))
        {
            player.GenerateSoundClipAndPlay();
        }
    }
}
