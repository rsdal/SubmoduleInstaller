using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SubmoduleInstallerWindow : EditorWindow
{
    private const string FolderPathKey = "SelectedFolderPath";
    private string folderPathText;
    private string gitUrlText;

    private static SubmoduleInstaller submoduleInstaller;  
    
    [MenuItem("Window/Submodule Installer Window")]
    public static void ShowExample()
    {
        SubmoduleInstallerWindow window = GetWindow<SubmoduleInstallerWindow>();
        window.titleContent = new GUIContent("Submodule installer window");
        window.minSize = new Vector2(600, 400);
        
        submoduleInstaller = new SubmoduleInstaller();
    }

    public void CreateGUI()
    {
        folderPathText = EditorPrefs.GetString(FolderPathKey, "");

        VisualElement root = rootVisualElement;

        TextField folderPathField = new TextField("Selected Folder Path");
        folderPathField.value = folderPathText;
        folderPathField.isReadOnly = true;

        void ClickEvent()
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", "", "");
            
            if (string.IsNullOrEmpty(selectedPath)) return;
            
            folderPathText = selectedPath;
            folderPathField.value = folderPathText;
            EditorPrefs.SetString(FolderPathKey, folderPathText);
        }

        Button selectFolderButton = new Button(ClickEvent)
        {
            text = "Select Folder"
        };

        TextField gitUrl = new TextField("Git Repository URL");
        gitUrl.RegisterValueChangedCallback(evt => gitUrlText = evt.newValue);

        Button enterButton = new Button(() =>
        {
            EditorUtility.DisplayProgressBar("Cloning", "Cloning repository...", 0);
            submoduleInstaller.Clone(folderPathText, gitUrlText);
            EditorUtility.ClearProgressBar();
            submoduleInstaller.AddToPackage(folderPathText, gitUrlText);
        })
        {
            text = "Clone and Add to Package"
        };

        root.Add(folderPathField);
        root.Add(selectFolderButton);
        root.Add(gitUrl);
        root.Add(enterButton);
    }
}
