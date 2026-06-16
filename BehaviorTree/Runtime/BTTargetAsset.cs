using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;

[Serializable]
public class BTTargetAsset
{
    public UnityEngine.Object target = null;
    public string assetPath;

    [NonSerialized]
    public BTRuntime runtime;

    [NonSerialized]
    private Action<bool> loadFinish;

    public void SetTarget(Action<bool> _loadFinish = null)
    {
        if(string.IsNullOrEmpty(assetPath)) { _loadFinish?.Invoke(true); return; }
        if (target != null) { _loadFinish?.Invoke(true); return; }

        Type type = GetObjectType();
#if UNITY_EDITOR
        target = AssetDatabase.LoadAssetAtPath(assetPath, type);
        _loadFinish?.Invoke(true);
#else
        loadFinish = _loadFinish;
        Addressables.LoadAssetAsync<UnityEngine.Object>(assetPath).Completed += OnLoadDone;
#endif
    }
    private Type GetObjectType()
    {
        string ext = System.IO.Path.GetExtension(assetPath).ToLower();
        return ext switch
        {
            ".prefab" => typeof(GameObject),
            ".asset" => typeof(ScriptableObject),
            ".unity" => typeof(SceneAsset),
            ".mat" => typeof(Material),
            ".png" or ".jpg" or ".jpeg" or ".tga" or ".bmp" or ".psd" => typeof(Texture2D),
            ".wav" or ".mp3" or ".ogg" or ".aiff" or ".flac" => typeof(AudioClip),
            ".anim" => typeof(AnimationClip),
            ".playable" => typeof(PlayableAsset),
            ".controller" => typeof(RuntimeAnimatorController),
            ".ttf" or ".otf" => typeof(Font),
            ".txtDice" or ".json" or ".xml" => typeof(TextAsset),
            _ => typeof(GameObject)
        };
    }
    private void OnLoadDone(AsyncOperationHandle<UnityEngine.Object> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded) target = handle.Result;
        else Debug.LogError("资源加载失败：" + assetPath);

        loadFinish?.Invoke(handle.Status == AsyncOperationStatus.Succeeded);
        loadFinish = null;
    }
    public void SerializeSelf()
    {
#if UNITY_EDITOR
        assetPath = null;
        if (target is null) { target = null; return; }
        assetPath = AssetDatabase.GetAssetPath(target);
        target = null;
#endif
    }
}
