using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using System.IO;

[InitializeOnLoad]
public static class PostInstall
{
    const string InstallFlag = "UIAnimaBehaviorTree_Installed";

    static PostInstall()
    {
        if (EditorPrefs.GetBool(InstallFlag))
            return;

        var package = PackageInfo.FindForAssembly(typeof(PostInstall).Assembly);
        if (package == null)
            return;

        string source = Path.Combine(package.resolvedPath, "Runtime");
        string dest = Path.Combine("Assets", "BehaviorTree");

        if (Directory.Exists(source) && !Directory.Exists(dest))
        {
            CopyDirectory(source, dest);
            AssetDatabase.Refresh();

            EditorPrefs.SetBool(InstallFlag, true);

            Debug.Log("UIAnimaBehaviorTree 已安装到 Assets/BehaviorTree");
        }
    }

    static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            if (file.EndsWith(".meta")) continue;

            string destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
    }
}