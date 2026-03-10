using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public static class BehaviorTreeInstaller
{
    const string InstallFlag = "UIAnimaBehaviorTree_Installed";

    static BehaviorTreeInstaller()
    {
        if (EditorPrefs.GetBool(InstallFlag))
            return;

        var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BehaviorTreeInstaller).Assembly);
        if (package == null)
            return;

        string sourceDir = Path.Combine(package.resolvedPath, "BehaviorTree");
        string destDir = Path.Combine(Application.dataPath, "BehaviorTree");

        if (!Directory.Exists(sourceDir))
            return;

        Debug.Log("开始迁移 BehaviorTree 到 Assets...");

        CopyDirectory(sourceDir, destDir);

        try
        {
            Directory.Delete(sourceDir, true);

            string meta = sourceDir + ".meta";
            if (File.Exists(meta))
                File.Delete(meta);
        }
        catch
        {
            Debug.LogWarning("无法删除 Package 内源码（可能是只读缓存），但不影响使用。");
        }

        AssetDatabase.Refresh();

        EditorPrefs.SetBool(InstallFlag, true);

        Debug.Log("BehaviorTree 已迁移到 Assets/BehaviorTree");
    }

    static void CopyDirectory(string source, string dest)
    {
        if (!Directory.Exists(dest))
            Directory.CreateDirectory(dest);

        foreach (var file in Directory.GetFiles(source))
        {
            string name = Path.GetFileName(file);
            string destFile = Path.Combine(dest, name);

            File.Copy(file, destFile, true);

            string meta = file + ".meta";
            if (File.Exists(meta))
                File.Copy(meta, destFile + ".meta", true);
        }

        foreach (var dir in Directory.GetDirectories(source))
        {
            string name = Path.GetFileName(dir);
            string destSub = Path.Combine(dest, name);

            CopyDirectory(dir, destSub);
        }
    }
}