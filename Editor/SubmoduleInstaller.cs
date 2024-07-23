using System;
using System.Diagnostics;
using System.IO;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SubmoduleInstaller
{
    public void Clone(string targetDirectory, string repositoryUrl)
    {
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        Process process = new Process();

        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = $"submodule add {repositoryUrl}";
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.WorkingDirectory = targetDirectory;

        try
        {
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Debug.Log($"Error on trying to clone: {error}");
                return;
            }

            Debug.Log("Repository cloned successfully.");
            Debug.Log($"Output: {output}");
            
            if (!string.IsNullOrEmpty(error))
            {
                Debug.Log($"Warnings/Information: {error}");
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"An error occurred: {ex.Message}");
        }
    }

    public void AddToPackage(string targetDirectory, string repositoryUrl)
    {
        try
        {
            string folderNameFromUrl = GetFolderNameFromUrl(repositoryUrl);
            string localPath = Path.Combine(targetDirectory, folderNameFromUrl);

            if (Directory.Exists(localPath))
            {
                string packagePath = $"file:{GetRelativePath(Application.dataPath, targetDirectory)}/{folderNameFromUrl}";
                AddRequest request = Client.Add(packagePath);
                
                while (!request.IsCompleted)
                {
                    //Loading
                }

                if (request.Status == StatusCode.Success)
                {
                    Debug.Log("Package added successfully!");
                    return;
                }

                Debug.LogError($"Failed to add package: {request.Error.message}");
                
                return;
            }

            Debug.LogError("Package folder path does not exist.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while adding trying to add the package: {ex.Message}");
        }
    }

    private string GetFolderNameFromUrl(string repositoryUrl)
    {
        Uri uri = new Uri(repositoryUrl);
        string folderName = Path.GetFileNameWithoutExtension(uri.LocalPath);
        return folderName;
    }
    
    private string GetRelativePath(string basePath, string targetPath)
    {
        // Ensure paths end with a directory separator
        if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            basePath += Path.DirectorySeparatorChar;
        }

        Uri baseUri = new Uri(basePath);
        Uri targetUri = new Uri(targetPath);

        Uri relativeUri = baseUri.MakeRelativeUri(targetUri);

        // Replace forward slashes with backslashes if on Windows
        string relativePath = Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);

        return relativePath;
    }
}