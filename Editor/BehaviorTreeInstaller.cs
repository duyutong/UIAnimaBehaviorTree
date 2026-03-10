using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using System.IO;

[InitializeOnLoad]
public static class BehaviorTreeInstaller
{
    // 防止重复迁移
    private const string InstallFlag = "UIAnimaBehaviorTree_Installed";

    static BehaviorTreeInstaller()
    {
        if (EditorPrefs.GetBool(InstallFlag))
            return;

        // 获取当前脚本所在的 Package
        var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BehaviorTreeInstaller).Assembly);
        if (package == null)
            return;

        string sourceDir = Path.Combine(package.resolvedPath, "BehaviorTree");
        string destDir = Path.Combine(Application.dataPath, "BehaviorTree");

        if (!Directory.Exists(sourceDir))
            return;

        Debug.Log("开始迁移 BehaviorTree 到 Assets...");

        CopyDirectory(sourceDir, destDir);

        // 删除 Package 内源码
        try
        {
            Directory.Delete(sourceDir, true);

            string metaFile = sourceDir + ".meta";
            if (File.Exists(metaFile))
                File.Delete(metaFile);
        }
        catch
        {
            Debug.LogWarning("Package 内源文件删除失败（只读缓存或权限问题），不影响使用。");
        }

        AssetDatabase.Refresh();

        EditorPrefs.SetBool(InstallFlag, true);

        Debug.Log("BehaviorTree 已迁移到 Assets/BehaviorTree");
    }

    private static void CopyDirectory(string source, string dest)
    {
        if (!Directory.Exists(dest))
            Directory.CreateDirectory(dest);

        // 复制所有文件
        foreach (var file in Directory.GetFiles(source))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(dest, fileName);

            File.Copy(file, destFile, true);

            // 复制.meta
            string metaFile = file + ".meta";
            if (File.Exists(metaFile))
            {
                File.Copy(metaFile, destFile + ".meta", true);
            }
        }

        // 递归复制子目录
        foreach (var dir in Directory.GetDirectories(source))
        {
            string dirName = Path.GetFileName(dir);
            string destSubDir = Path.Combine(dest, dirName);

            CopyDirectory(dir, destSubDir);
        }
    }
}