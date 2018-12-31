using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(SfxrSoundAsset))]
public class SfxrSoundAssetEditor : Editor
{
    private SfxrSoundAsset soundAsset;

    private AudioClip clip;
    private Editor clipEditor;
    
    private static Type tyAudioClipInspector;
    private static Type tyAudioUtil;
    private static MethodInfo miPlayClip;

    private AudioSource audioSource;
    
    static SfxrSoundAssetEditor()
    {
        
        Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;

        tyAudioClipInspector = unityEditorAssembly.GetType("UnityEditor.AudioClipInspector");
        tyAudioUtil = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
        miPlayClip = tyAudioUtil.GetMethod("PlayClip",
            BindingFlags.Static | BindingFlags.Public, null, new[] {typeof(AudioClip)}, null);
    }

    private static readonly Dictionary<string, Action<SfxrSoundAsset>> generationActions =
        new Dictionary<string, Action<SfxrSoundAsset>>
        {
            {"Pickup/Coin", asset => asset.GeneratePickupCoin()},
            {"Laser/Shoot", asset => asset.GenerateLaserShoot()},
            {"Explosion", asset => asset.GenerateExplosion()},
            {"Powerup", asset => asset.GeneratePowerup()},
            {"Hit/Hurt", asset => asset.GenerateHitHurt()},
            {"Jump", asset => asset.GenerateJump()},
            {"Blip/Select", asset => asset.GenerateBlipSelect()},
            {"", null}, //Space
            {"Randomize", asset => asset.Randomize()},
            {"Mutation", asset => asset.Mutate()},
        };

    private void OnEnable()
    {
        soundAsset = (SfxrSoundAsset) target;
        CreateCachedEditor(clip, tyAudioClipInspector, ref clipEditor);
        
        var go = new GameObject("Audio Preview", typeof(AudioSource));
        go.hideFlags = HideFlags.HideAndDontSave;
        audioSource = go.GetComponent<AudioSource>();
        audioSource.spatialize = false;
    }

    private void OnDisable()
    {
        if (audioSource != null)
        {
            DestroyImmediate(audioSource.gameObject);
        }
    }

    public override async void OnInspectorGUI()
    {
        bool differGenerate = false;
        bool differPlay = false;
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        foreach (var generationAction in generationActions)
        {
            if (string.IsNullOrEmpty(generationAction.Key))
            {
                EditorGUILayout.Space();
                continue;
            }

            if (GUILayout.Button(generationAction.Key))
            {
                Undo.RegisterCompleteObjectUndo(this, generationAction.Key);
                generationAction.Value(soundAsset);
                EditorUtility.SetDirty(soundAsset);
                AssetDatabase.SaveAssets();
                differGenerate = differPlay = true;
            }
        }
         
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate"))
        {
            differGenerate = true;
        }

        if (GUILayout.Button("Play"))
        {
            differPlay = true;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();

        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();
        
        if (EditorGUI.EndChangeCheck())
        {
            differGenerate = differPlay = true;
        }
        
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        if (differGenerate)
        {
            GenerateSound();
            if (differPlay)
            {
                PlayClip();
            }

            return;
            
            
            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();
            generateSoundTask = GenerateSoundAsync(tokenSource.Token);
            await generateSoundTask;
            if (generateSoundTask.IsCompleted && !generateSoundTask.IsCanceled)
            {
                Repaint();
            
                if (differPlay)
                {
                    PlayClip();
                }        

            }
        }
    }

    private CancellationTokenSource tokenSource;

    private Task generateSoundTask;
    
    private async Task GenerateSoundAsync(CancellationToken token){           
        clip = await SfxrSynthesizer.GenerateSoundAsync(soundAsset, token);
        CreateCachedEditor(clip, tyAudioClipInspector, ref clipEditor);
    }
    
    private void GenerateSound(){           
        clip = SfxrSynthesizer.GenerateSoundJob(soundAsset);
        CreateCachedEditor(clip, tyAudioClipInspector, ref clipEditor);
    }

    public override bool HasPreviewGUI()
    {
        return clipEditor != null;
    }

    public override void OnPreviewSettings()
    {
        clipEditor.OnPreviewSettings();
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        clipEditor.OnPreviewGUI(r, background);
    }

    public override void DrawPreview(Rect previewArea)
    {
        clipEditor.DrawPreview(previewArea);
    }

    private void PlayClip()
    {
//        miPlayClip.Invoke(null, new object[] {clip});
        if (audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

}