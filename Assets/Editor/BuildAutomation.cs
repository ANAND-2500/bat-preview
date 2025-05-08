using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class BuildAutomation : EditorWindow
{
    private string[] scenePaths;
    private List<string> selectedScenes = new List<string>();
    private string folderPath = "E:/unity/bat preview/Assets/Scenes";

    private enum BuildType { None, Windows, Android }
    private BuildType pendingBuild = BuildType.None;

    [MenuItem("Build/Build Automation")]
    public static void ShowWindow()
    {
        BuildAutomation window = GetWindow<BuildAutomation>("Build Automation");
        window.Show();
    }

    void OnEnable()
    {
        scenePaths = Directory.GetFiles(folderPath, "*.unity");
    }

    void OnGUI()
    {
        GUILayout.Label("Select Scenes to Build", EditorStyles.boldLabel);

        foreach (var scene in scenePaths)
        {
            string sceneName = Path.GetFileName(scene);
            bool isSelected = selectedScenes.Contains(scene);

            if (GUILayout.Toggle(isSelected, sceneName))
            {
                if (!isSelected) selectedScenes.Add(scene);
            }
            else
            {
                selectedScenes.Remove(scene);
            }
        }

        GUILayout.Space(15);

        if (GUILayout.Button("Build Windows"))
        {
            if (selectedScenes.Count > 0)
            {
                pendingBuild = BuildType.Windows;
                AskToChangePlayerSettings();
            }
            else
            {
                Debug.LogError("No scenes selected.");
            }
        }

        if (GUILayout.Button("Build Android"))
        {
            if (selectedScenes.Count > 0)
            {
                pendingBuild = BuildType.Android;
                AskToChangePlayerSettings();
            }
            else
            {
                Debug.LogError("No scenes selected.");
            }
        }

        if (GUILayout.Button("Build All"))
        {
            if (selectedScenes.Count > 0)
            {
                BuildAll(selectedScenes);
            }
            else
            {
                Debug.LogError("No scenes selected.");
            }
        }

        if (pendingBuild != BuildType.None)
        {
            EditorGUILayout.HelpBox("Once done adjusting Player Settings, click below to start build.", MessageType.Info);

            if (GUILayout.Button("Begin Build"))
            {
                if (pendingBuild == BuildType.Windows)
                    BuildWindows(selectedScenes);
                else if (pendingBuild == BuildType.Android)
                    BuildAndroid(selectedScenes);

                pendingBuild = BuildType.None;
            }
        }
    }

    private void AskToChangePlayerSettings()
    {
        if (EditorUtility.DisplayDialog("Player Settings",
            "Do you want to modify Player Settings before building?",
            "Yes", "No"))
        {
            // Open Player Settings inspector
            SettingsService.OpenProjectSettings("Project/Player");
        }
        else
        {
            // Build immediately with default settings
            if (pendingBuild == BuildType.Windows)
            {
                BuildWindows(selectedScenes);
                pendingBuild = BuildType.None;
            }
            else if (pendingBuild == BuildType.Android)
            {
                BuildAndroid(selectedScenes);
                pendingBuild = BuildType.None;
            }
        }
    }

    private static string GetVersion()
    {
        return "1.0." + DateTime.Now.ToString("yyyyMMddHHmmss");
    }

    private void BuildWindows(List<string> scenes)
    {
        string version = GetVersion();
        PlayerSettings.bundleVersion = version;

        foreach (string scene in scenes)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scene);
            string path = $"E:/unity/builds/Windows/{sceneName}_BatShowcase_{version}.exe";

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            BuildPipeline.BuildPlayer(new[] { scene }, path, BuildTarget.StandaloneWindows64, BuildOptions.None);
            Debug.Log($"✅ Windows Build for {sceneName} saved to: {path}");
        }
    }

    private void BuildAndroid(List<string> scenes)
    {
        string version = GetVersion();
        PlayerSettings.bundleVersion = version;

        foreach (string scene in scenes)
        {
            string sceneName = Path.GetFileNameWithoutExtension(scene);
            string path = $"E:/unity/builds/Android/{sceneName}_BatShowcase_{version}.apk";

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            BuildPipeline.BuildPlayer(new[] { scene }, path, BuildTarget.Android, BuildOptions.None);
            Debug.Log($"✅ Android Build for {sceneName} saved to: {path}");
        }
    }

    private void BuildAll(List<string> scenes)
    {
        Debug.Log("🚀 Building for all platforms with default settings...");
        BuildWindows(scenes);
        BuildAndroid(scenes);
        Debug.Log("✅ All builds complete.");
    }
}
