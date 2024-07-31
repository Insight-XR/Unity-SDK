#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public class CustomBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.Android)
        {
            string packageManifestPath = "Packages/com.insightxr.insightxrreplaytool/Runtime/Android/AndroidManifest.xml";
            string destinationPath = "Assets/Plugins/Android/AndroidManifest.xml";

            if (!Directory.Exists("Assets/Plugins/Android"))
            {
                Directory.CreateDirectory("Assets/Plugins/Android");
            }

            File.Copy(packageManifestPath, destinationPath, true);

            Debug.Log($"Copied {packageManifestPath} to {destinationPath}");
        }
    }
}
#endif
