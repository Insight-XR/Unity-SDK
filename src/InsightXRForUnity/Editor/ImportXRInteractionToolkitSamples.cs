#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public class ImportXRInteractionToolkitSamples
{
    static ImportXRInteractionToolkitSamples()
    {
        ImportSamples();
    }

    static void ImportSamples()
    {
        // Define the package and sample asset path
        string packageName = "com.unity.xr.interaction.toolkit";
        string samplePath = "Samples~/Starter Assets";

        // Destination path
        string destinationPath = "Assets/Samples/XR Interaction Toolkit";

        // Check if the sample assets are already imported
        if (Directory.Exists(destinationPath))
        {
            // Debug.Log("XR Interaction Toolkit samples are already imported.");
            return;
        }

        // Create the destination directory if it doesn't exist
        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        // Find the sample asset directory
        string packagePath = Path.Combine("Packages", packageName, samplePath);
        if (!Directory.Exists(packagePath))
        {
            Debug.LogError($"Sample path not found: {packagePath}");
            return;
        }

        try
        {
            // Copy the sample assets to the Assets folder
            CopyDirectory(packagePath, destinationPath);
            AssetDatabase.Refresh();
            Debug.Log("XR Interaction Toolkit samples imported successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to copy sample assets: {ex.Message}");
        }
    }

    static void CopyDirectory(string sourceDir, string destinationDir)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDir);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        Directory.CreateDirectory(destinationDir);

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(temppath, false);
        }

        // Copy subdirectories and their contents to new location.
        foreach (DirectoryInfo subdir in dirs)
        {
            string temppath = Path.Combine(destinationDir, subdir.Name);
            CopyDirectory(subdir.FullName, temppath);
        }
    }
}
#endif
