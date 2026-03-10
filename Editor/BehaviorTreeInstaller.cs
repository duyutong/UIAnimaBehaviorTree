using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using System.IO;

[InitializeOnLoad]
public static class BehaviorTreeInstaller
{
    private const string InstallFlag = "UIAnimaBehaviorTree_Installed";

    static BehaviorTreeInstaller()
    {
        Debug.Log("[BTInstaller] 执行 BehaviorTreeInstaller 初始化");

        // 检查是否已安装过
        if (EditorPrefs.GetBool(InstallFlag))
        {
            Debug.Log("[BTInstaller] 已检测到安装标记，跳过迁移");
            return;
        }

        // 获取当前脚本所在的 Package
        var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(BehaviorTreeInstaller).Assembly);
        if (package == null)
        {
            Debug.LogError("[BTInstaller] 找不到 BehaviorTreeInstaller 对应的 PackageInfo");
            return;
        }

        Debug.Log("[BTInstaller] 找到 Package: " + package.name);
        Debug.Log("[BTInstaller] Package 路径: " + package.resolvedPath);

        string sourceDir = Path.Combine(package.resolvedPath, "BehaviorTree");
        string destDir = Path.Combine(Application.dataPath, "BehaviorTree");

        Debug.Log("[BTInstaller] 源路径: " + sourceDir);
        Debug.Log("[BTInstaller] 目标路径: " + destDir);

        if (!Directory.Exists(sourceDir))
        {
            Debug.LogWarning("[BTInstaller] 源路径不存在，迁移终止");
            return;
        }

        Debug.Log("[BTInstaller] 开始迁移 BehaviorTree 到 Assets...");

        try
        {
            CopyDirectory(sourceDir, destDir);
            Debug.Log("[BTInstaller] 复制完成");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[BTInstaller] 复制出错: " + ex.Message);
            return;
        }

        // 尝试删除 Package 内源码
        try
        {
            Directory.Delete(sourceDir, true);

            string metaFile = sourceDir + ".meta";
            if (File.Exists(metaFile))
                File.Delete(metaFile);

            Debug.Log("[BTInstaller] Package 内源文件已尝试删除");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[BTInstaller] 删除 Package 内源文件失败（只读或权限问题），不影响使用: " + ex.Message);
        }

        AssetDatabase.Refresh();

        EditorPrefs.SetBool(InstallFlag, true);

        Debug.Log("[BTInstaller] BehaviorTree 已成功迁移到 Assets/BehaviorTree");
    }

    private static void CopyDirectory(string source, string dest)
    {
        if (!Directory.Exists(dest))
            Directory.CreateDirectory(dest);

        foreach (var file in Directory.GetFiles(source))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(dest, fileName);

            try
            {
                File.Copy(file, destFile, true);
                Debug.Log("[BTInstaller] 复制文件: " + fileName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[BTInstaller] 复制文件失败: " + fileName + " Error: " + ex.Message);
            }

            // 复制 .meta 文件
            string metaFile = file + ".meta";
            if (File.Exists(metaFile))
            {
                try
                {
                    File.Copy(metaFile, destFile + ".meta", true);
                    Debug.Log("[BTInstaller] 复制.meta文件: " + fileName + ".meta");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[BTInstaller] 复制.meta失败: " + fileName + ".meta Error: " + ex.Message);
                }
            }
        }

        foreach (var dir in Directory.GetDirectories(source))
        {
            string dirName = Path.GetFileName(dir);
            string destSubDir = Path.Combine(dest, dirName);

            CopyDirectory(dir, destSubDir);
        }
    }
}