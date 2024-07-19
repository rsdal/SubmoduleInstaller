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
                string packagePath = "file:" + GetRelativePath(localPath, targetDirectory);
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

    private string GetRelativePath(string absolutePath, string targetDirectory)
    {
        Uri projectUri = new Uri(Application.dataPath);
        Uri folderUri = new Uri(absolutePath);
        Uri relativeUri = projectUri.MakeRelativeUri(folderUri);

        string[] remainingTargetDirectory = targetDirectory.Split('/');
        string[] remainingPathSegments = absolutePath.Split('/');

        int commonPathLength = 0;
        for (int i = remainingPathSegments.Length - 1; i >= 0; i--)
        {
            if (remainingPathSegments[i] == remainingTargetDirectory[^1])
            {
                break;
            }

            commonPathLength++;
        }

        // Convert the relative URI to a relative path
        string relativePath = Uri.UnescapeDataString(relativeUri.ToString());
        relativePath = relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);

        for (int i = 0; i < commonPathLength; i++)
        {
            relativePath = Path.Combine("..", relativePath);
        }

        return relativePath;
    }
}