#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using System.IO;
using TerraUnity.UI;

namespace TerraUnity.Edittime
{
    public enum SceneDownloaderStatus
    {
        Initializing,
        Downloading,
        Importing,
        Idle,
        Error,
        Aborted
    }

    [ExecuteInEditMode]
    public class DownloadDemoScene : MonoBehaviour
    {
#pragma warning disable CS0414 // Add readonly modifier
        public string demoScenePath = "";
        public string sceneURL = "";
        public Vector3 initialCameraPosition = new Vector3(0f, 0f, 0f);
        public Vector3 initialCameraRotation = new Vector3(0f, 0f, 0f);

        public static SceneDownloaderStatus status = SceneDownloaderStatus.Idle;
        private static string _sceneURL;
        private static string cachePath;
        private static int progressId;
        private static string title = "DOWNLOADING SCENE";
        private static string info = "Downloading package, please wait...";
        //private string demoSceneDownloadPath = "Assets/TerraWorld/Scenes/Pro/Demo Scene Download.unity";
#pragma warning restore CS0414 // Add readonly modifier

#if TERRAWORLD_PRO
        private void OnEnable()
        {
            Initialize();
            EditorApplication.update += OnEditorUpdate;
        }

        void OnDisable()
        {
            TProgressBar.RemoveProgressBar(progressId);
            EditorApplication.update -= OnEditorUpdate;
        }

        protected virtual void OnEditorUpdate()
        {
            Update();
        }

        private void Update ()
        {
            if (Application.isPlaying) return;
            
            if (status == SceneDownloaderStatus.Initializing)
            {
                if (!AssetDatabase.IsValidFolder(Path.GetDirectoryName(demoScenePath)) || AssetDatabase.LoadAssetAtPath(demoScenePath, typeof(UnityEngine.Object)) == null)
                {
                    if (EditorUtility.DisplayDialog("TERRAWORLD", "DOWNLOAD DEMO SCENE\n\nDemo scene should be downloaded from the internet (It could be hundreds of megabytes in size but will be cached for later usage)\n\nDo you want to proceed and import the package into project?", "Yes", "No"))
                        DownloadScenePackage();
                }
            }
            
            if (status == SceneDownloaderStatus.Downloading)
            {
                TProgressBar.DisplayCancelableProgressBar("TERRAWORLD", "Downloading scene package, please wait...", ServerSync.PackageDownloadProgress, progressId, CancelDownload);
                if (EditorUtility.DisplayCancelableProgressBar("TERRAWORLD", "Downloading scene package, please wait...", ServerSync.PackageDownloadProgress)) CancelDownload();
            }
            
            if (status != SceneDownloaderStatus.Downloading && !EditorApplication.isCompiling)
                LoadScene();
        }

        private void Initialize()
        {
            if (Application.isPlaying) return;
            status = SceneDownloaderStatus.Initializing;
        }

        private void DownloadScenePackage()
        {
            progressId = TProgressBar.StartCancelableProgressBar(title, info, TProgressBar.ProgressOptionsList.Managed, true, CancelDownload);
            string pathLocal = ""; // This is the root of "Scenes/Pro" folder in project
            cachePath = TAddresses.scenesPath_Pro_Cache;
            status = SceneDownloaderStatus.Downloading;

#if TERRAWORLD_PRO
            _sceneURL = sceneURL;
#else
            _sceneURL = "http://terraunity.com/TerraWorldScenes/Lite/";
#endif

            if (!string.IsNullOrEmpty(_sceneURL))
                ServerSync.DownloadPackage(_sceneURL, pathLocal, cachePath, DownloadPackageCompleted);
            else
            {
                status = SceneDownloaderStatus.Error;
                Debug.LogError("Can not find scene on TERRA servers or in project!");
            }
        }

        private static void DownloadPackageCompleted(Exception e, string retrievePath)
        {
            TProgressBar.RemoveProgressBar(progressId);

            if (e == null && !string.IsNullOrEmpty(retrievePath) && retrievePath.EndsWith(".unitypackage"))
            {
                status = SceneDownloaderStatus.Importing;
                ImportPackage(retrievePath);
            }

            if (e != null)
            {
                // Checks if package is already downloaded and exists in system's cache folder even though there
                // was no connection made to servers or encountered errors during data capturing
                if (!string.IsNullOrEmpty(retrievePath))
                {
                    string folderPath = cachePath + retrievePath;

                    if (Directory.Exists(folderPath))
                    {
                        string[] packages = Directory.GetFiles(folderPath, "*.unitypackage", SearchOption.TopDirectoryOnly);

                        if (packages.Length > 0 && !string.IsNullOrEmpty(packages[0]))
                        {
                            status = SceneDownloaderStatus.Importing;
                            ImportPackage(packages[0]);
                        }
                        else
                        {
                            if (e.Message.Contains("ConnectFailure") || e.Message.Contains("NameResolutionFailure"))
                            {
                                e.Data.Add("TW", "Seems like you are offline!\n\nPackage cannot be downloaded from TERRA servers!");
                                TDebug.LogErrorToUnityUI(e);
                            }
                            else
                                TDebug.LogErrorToUnityUI(e);
                        }
                    }
                    else
                    {
                        if (e.Message.Contains("ConnectFailure") || e.Message.Contains("NameResolutionFailure"))
                        {
                            e.Data.Add("TW", "Seems like you are offline!\n\nPackage cannot be downloaded from TERRA servers!");
                            TDebug.LogErrorToUnityUI(e);
                        }
                        else
                            TDebug.LogErrorToUnityUI(e);
                    }
                }
                else
                    TDebug.LogErrorToUnityUI(e);
            }
        }

        private static void ImportPackage(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return;
                TProgressBar.RemoveProgressBar(progressId);
                AssetDatabase.ImportPackage(path, false);
            }
            catch
            {
                status = SceneDownloaderStatus.Error;
                EditorUtility.DisplayDialog("TERRAWORLD", "IMPORT ERROR : There was a problem importing unitypackage from cache folder!\n\nPlease try again!", "OK");
            }
        }

        private void LoadScene()
        {
            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene == SceneManager.GetSceneByPath(demoScenePath)) return;

                if (AssetDatabase.IsValidFolder(Path.GetDirectoryName(demoScenePath)))
                {
                    if (AssetDatabase.LoadAssetAtPath(demoScenePath, typeof(UnityEngine.Object)) != null)
                    {
                        EditorSceneManager.OpenScene(demoScenePath);
                        SceneView sceneView = SceneView.lastActiveSceneView;

                        if (sceneView != null)
                        {
                            sceneView.orthographic = false;
                            Quaternion rotation = Quaternion.Euler(initialCameraRotation);
                            //Vector3 forwardOffset = rotation * Vector3.forward * 150f;
                            //Vector3 upwardOffset = rotation * Vector3.up * 10f;
                            //sceneView.LookAt(cameraPosition + forwardOffset + upwardOffset, rotation, 150);
                            sceneView.LookAt(initialCameraPosition, rotation, 10);
                        }
                    }
                    else
                        Debug.Log("Demo Scene Not Found!");
                }

                status = SceneDownloaderStatus.Idle;
            }
            catch (Exception e)
            {
                status = SceneDownloaderStatus.Error;
                TDebug.LogErrorToUnityUI(e);
            }
        }

        public static bool CancelDownload()
        {
            TProgressBar.RemoveProgressBar(progressId);
            status = SceneDownloaderStatus.Aborted;
            ServerSync.CancelDownload();

            return true;
        }
#endif
    }
}
#endif

