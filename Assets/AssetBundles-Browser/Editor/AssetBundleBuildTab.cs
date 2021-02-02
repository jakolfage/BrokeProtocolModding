using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace AssetBundleBrowser
{
    [Serializable]
    internal class AssetBundleBuildTab
    {
        [SerializeField]
        private string outputPath = "AssetBundles";

        private static string CleanPath(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string GetProjectPath => CleanPath(Path.GetFullPath("."));

        private const string dataFile = "Library/AssetBundleBrowserBuild.dat";
        private string GetDataPath => Path.Combine(GetProjectPath, dataFile);

        private const string assetExtension = ".bpa";
        private const string manifestExtension = ".manifest";

        [SerializeField]
        private Vector2 m_ScrollPosition;

        private string GetExtension(BuildTarget buildTarget)
        {
            return "." + (buildTarget == BuildTarget.Android ? RuntimePlatform.Android : RuntimePlatform.WindowsPlayer);
        }

        internal void OnEnable(EditorWindow parent)
        {
            var dataPath = GetDataPath;

            try
            {
                if (File.Exists(dataPath))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream file = File.Open(dataPath, FileMode.Open);
                    outputPath = bf.Deserialize(file) as string;
                    file.Close();
                }
            } catch(Exception e)
            {
                Debug.LogWarning("[AB] Deserialization Error: " + e);
            }
        }

        internal void OnDisable()
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(GetDataPath);

            bf.Serialize(file, outputPath);
            file.Close();
        }

        internal void OnGUI()
        {
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(new GUIContent("Select the Broke Protocol AssetBundles Path"), centeredStyle);

            // basic options
            EditorGUILayout.Space();
            GUILayout.BeginVertical();

            // output path
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Browse", GUILayout.MaxWidth(75f)))
            {
                BrowseForFolder();
            }
 
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // build
            EditorGUILayout.Space();
            if (GUILayout.Button("Build") )
            {
                string tempDirectory = Path.Combine(outputPath, "temp");
                Directory.CreateDirectory(tempDirectory);

                ExecuteBuild(BuildTarget.StandaloneWindows64, tempDirectory, false);
                ExecuteBuild(BuildTarget.Android, tempDirectory, true);

                Directory.Delete(tempDirectory, true);

                GUIUtility.ExitGUI();
            }

            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void ExecuteBuild(BuildTarget buildTarget, string tempDirectory, bool last)
        {
            if (string.IsNullOrEmpty(outputPath))
            {
                BrowseForFolder();
            }

            if (string.IsNullOrEmpty(outputPath)) //in case they hit "cancel" on the open browser
            {
                Debug.LogError("AssetBundle Build: No valid output path for build.");
                return;
            }

            Directory.CreateDirectory(outputPath);

            var buildManifest = BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.AssetBundleStripUnityVersion, buildTarget);
            if (buildManifest == null)
            {
                Debug.Log("Error in build");
                return;
            }

            foreach (var assetBundleName in buildManifest.GetAllAssetBundles())
            {
                string lastFolderName = Path.GetFileName(outputPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                string lastFolderPath = Path.Combine(outputPath, lastFolderName);
                File.Delete(lastFolderPath);
                File.Delete(lastFolderPath + manifestExtension);

                string filepath = Path.Combine(outputPath, assetBundleName);
                File.Delete(filepath + manifestExtension);

                string subdirectory = Path.Combine(tempDirectory, assetBundleName);

                if (!Directory.Exists(subdirectory))
                {
                    Directory.CreateDirectory(subdirectory);
                }

                string assetFile = Path.Combine(subdirectory, assetBundleName + GetExtension(buildTarget));

                if(File.Exists(assetFile))
                {
                    File.Delete(assetFile);
                }

                File.Move(filepath, assetFile);

                //Finished
                if (last)
                {
                    filepath += assetExtension;

                    if(File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }

                    ZipFile.CreateFromDirectory(subdirectory, filepath, System.IO.Compression.CompressionLevel.NoCompression, false);
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        private void BrowseForFolder()
        {
            GUI.FocusControl(null); // Else Text field won't be updated
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", outputPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                outputPath = CleanPath(newPath);
            }
        }
    }
}