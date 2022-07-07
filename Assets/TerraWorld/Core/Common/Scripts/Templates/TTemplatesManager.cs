#if UNITY_EDITOR
#if TERRAWORLD_PRO
#if TW_TEMPLATES
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace TerraUnity.Edittime
{
    public enum TemplateManagerStatus
    {
        Update,
        FetchingFolders,
        Downloading,
        Importing,
        Idle,
        Error
    }

    public static class TTemplatesManager
    {
//#if TERRAWORLD_PRO
//        private static string templatesURL = "http://terraunity.com/TerraWorldTemplates/Pro/";
//#else
//        private static string templatesURL = "http://terraunity.com/TerraWorldTemplates/Lite/";
//#endif

       // private static string templateProjectPath;
        private static bool needsReRendering = false;
        private static TemplateManagerStatus status = TemplateManagerStatus.Idle;
        //private static float templatesSyncProgress;
        private static GameObject sceneParent;
        private static GameObject[] sceneObjects;
        //private static int _templatesSyncProgress = 0;
        private static int _tryAttepmt = 0;
        private static TCacheService cacheService;
        private static string remoteURL = "https://terraunity.com/TerraWorldTemplates/TemplatesPro02.xml";
        private static string LocalFolder = TAddresses.templatesPath;
        private static string _templatePathAbsolute;
        public static List<string> _ignoreList ;
        public static string notificationMessage = null;

        public static float PackageDownloadProgress { get => GetPackageDownloadProgress(); }
        public static float TemplatesSyncProgress { get => GettemplatesSyncProgress();  }
        public static TemplateManagerStatus Status { get => status; }
        public static bool NeedsReRendering { get => needsReRendering; set => needsReRendering = value; }

        public static void SetStatusToUpdate()
        {
            status = TemplateManagerStatus.Update;
        }

        public static float GettemplatesSyncProgress()
        {
            float result = 0f;
            if (cacheService != null) result = cacheService.Progress;
            return result;
        }

        public static float GetPackageDownloadProgress()
        {
            float result = 0f;

            if (status == TemplateManagerStatus.Downloading)
                if (cacheService != null)
                    result = cacheService.Progress;

            return result;
        }

        public static void SyncTemplatesFromServer()
        {
            status = TemplateManagerStatus.FetchingFolders;
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                ASyncTemplatesFoldersFromServer();
        }

        public static void ASyncTemplatesFoldersFromServer()
        {
            cacheService = new TCacheService();
            cacheService.OnSyncComplete += SyncTemplatesFoldersFromServerCompleted;
            cacheService.RemoveOldFiles = false;
            string FullTemplatePath = TAddresses.projectPath + TAddresses.templatesPath_Pro;
            cacheService.ASyncFolder(FullTemplatePath, remoteURL);
        }

        public static void SyncTemplatesFoldersFromServerCompleted(TCacheService cacheService)
        {
            SyncTemplatesFromServerCompleted(cacheService.Exception, cacheService.CacheFolder);
        }

        // This function is on a background thread, so any Unity API calls will cause errors
        private static void SyncTemplatesFromServerCompleted(Exception e , string path)
        {
            //if (e == null && !string.IsNullOrEmpty(retrievePath) && retrievePath.EndsWith(".png")) needsReRendering = true;

            if (e != null)
            {
                if (e.Message.Contains("ConnectFailure") || e.Message.Contains("NameResolutionFailure"))
                {
                    //templatesSyncProgress = 1;
                    notificationMessage = "Seems like you are offline, check your internet connection and try again later!\n\nTemplates will not be synced from TERRA servers!";
                }
                else
                    notificationMessage = e.Message + "\n\nTemplates will not be synced from TERRA servers!";
            }
            else
                notificationMessage = "";

            status = TemplateManagerStatus.Idle;
            needsReRendering = true;
        }

        public static void TryDownloadTemplatePackage(string templatePathAbsolute)
        {
            if (!string.IsNullOrEmpty (templatePathAbsolute))
            {
                _templatePathAbsolute = templatePathAbsolute;
                _tryAttepmt = 0;
            }
            else
            {
                _tryAttepmt++;
            }

            string[] _linkFiles = Directory.GetFiles(_templatePathAbsolute, "*.link", SearchOption.AllDirectories);
            if (_linkFiles.Length == 0) RunPackageInOfflineMode(_templatePathAbsolute);
            status = TemplateManagerStatus.Downloading;
            string templatePathLocal = _templatePathAbsolute.Replace(Path.GetFullPath(TAddresses.templatesPath_Pro), "");
            bool TryAttemptedIsOk = DownloadPackage2(_linkFiles[_linkFiles.Length-1], TAddresses.templatesPath_Pro_Cache + templatePathLocal, _tryAttepmt);
            if (!TryAttemptedIsOk) RunPackageInOfflineMode(_templatePathAbsolute); ;
        }

        public static bool DownloadPackage2(string linkFile, string savePath, int tryAttempt)
        {
            try
            {
                if (savePath == null) return false;
                string[] lines = File.ReadAllLines(linkFile);
                if (tryAttempt > lines.Length-1) throw new Exception("TryAttemptError");
                string packageURLDropBox = lines[tryAttempt];
                string fileName = savePath + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(linkFile) + ".unitypackage";
                cacheService = new TCacheService();
                cacheService.RemoveOldFiles = true;
                cacheService.OnSyncComplete += SyncPackageFromServerCompleted;
                cacheService.ASyncFile(fileName, packageURLDropBox);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SyncPackageFromServerCompleted(TCacheService cacheService)
        {
            status = TemplateManagerStatus.Idle;

            if (cacheService.Exception == null && cacheService.SyncedFiles.Count > 0)
            {
                string fileName = cacheService.SyncedFiles[0].LocalFullPath;
                DownloadTemplatePackageCompleted2(fileName);
            }
            else
                TryDownloadTemplatePackage("");
        }

        private static void DownloadTemplatePackageCompleted2(string retrievePath)
        {
            if (string.IsNullOrEmpty(retrievePath)) throw new Exception("Retrieve path missed");
            retrievePath = retrievePath.Replace("\\", "/");
            if (!File.Exists(retrievePath)) throw new Exception("Cache file not found ("+retrievePath+")");
            //            string NormilizedCachetemplateLocalPath = TCacheService.NormalizePath(TAddresses.templatesPath_Pro_Cache);
            string NormilizedCachetemplateLocalPath = TAddresses.templatesPath_Pro_Cache.Replace("\\","/");
            //string NormilizedCachetemplateLocalPath2 = ChageBackSlashtoSlash(TAddresses.templatesPath_Pro_Cache);
            //string templateLocalPath = Path.GetDirectoryName(retrievePath.Replace(NormilizedCachetemplateLocalPath, ""));
            string templateLocalPath = retrievePath.Replace(NormilizedCachetemplateLocalPath, "");
            //templateLocalPath = retrievePath.Replace(NormilizedCachetemplateLocalPath2, "");
            templateLocalPath = TAddresses.templatesPath_Pro + templateLocalPath ;
            templateLocalPath = Path.GetFullPath(templateLocalPath);
            templateLocalPath = Path.GetDirectoryName(templateLocalPath);
           // TProjectSettings.ActiveTemplatePath = Path.GetFullPath(TAddresses.templatesPath_Pro + templateLocalPath);
            TProjectSettings.ActiveTemplatePath = Path.GetFullPath(templateLocalPath);
            status = TemplateManagerStatus.Importing;
            ImportTemplatePackageNEW(retrievePath);
        }

        public static void RunPackageInOfflineMode(string templatePathAbsolute)
        {
            TProjectSettings.ActiveTemplatePath = templatePathAbsolute;
            RunTemplateNEW();
        }

        private static void DownloadTemplatePackageCompleted(Exception e, string retrievePath)
        {
            if (e == null && !string.IsNullOrEmpty(retrievePath) && retrievePath.EndsWith(".unitypackage"))
            {
                status = TemplateManagerStatus.Importing;
                ImportTemplatePackageNEW(retrievePath);
            }

            if (e != null)
            {
                // Checks if package is already downloaded and exists in system's cache folder even though there
                // was no connection made to servers or encountered errors during data capturing
                if (!string.IsNullOrEmpty(retrievePath))
                {
                    string templateFolderPath = TAddresses.templatesPath_Pro_Cache + retrievePath;

                    if (Directory.Exists(templateFolderPath))
                    {
                        string[] packages = Directory.GetFiles(templateFolderPath, "*.unitypackage", SearchOption.TopDirectoryOnly);

                        if (packages.Length > 0 && !string.IsNullOrEmpty(packages[0]))
                        {
                            status = TemplateManagerStatus.Importing;
                            ImportTemplatePackageNEW(packages[0]);
                        }
                        else
                        {
                            if (e.Message.Contains("ConnectFailure") || e.Message.Contains("NameResolutionFailure"))
                            {
                                e.Data.Add("TW", "Seems like you are offline!\n\nTemplates cannot be downloaded from TERRA servers!");
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
                            e.Data.Add("TW", "Seems like you are offline!\n\nTemplates cannot be downloaded from TERRA servers!");
                            TDebug.LogErrorToUnityUI(e);
                        }
                        else
                            TDebug.LogErrorToUnityUI(e);
                    }
                }
            }

            status = TemplateManagerStatus.Idle;
        }

        private static void ImportTemplatePackageNEW(string path)
        {
            UnityEngine.Debug.Log("ImportTemplatePackageNEW : " + path);
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                DisableScene();
                AssetDatabase.ImportPackage(path, false);
                AssetDatabase.importPackageCompleted += OnImportPackageCompletedNEW;
            }
            catch
            {
                EnableScene();
                status = TemplateManagerStatus.Error;
                EditorUtility.DisplayDialog("IMPORT ERROR", "There was a problem importing unitypackage from cache folder!\n\nPlease try again!", "OK");
            }
        }

        private static void OnImportPackageCompletedNEW(string packagename)
        {
            AssetDatabase.importPackageCompleted -= OnImportPackageCompletedNEW;
            RunTemplateNEW();
        }

        private static void RunTemplateNEW()
        {
            try
            {
                if (string.IsNullOrEmpty(TProjectSettings.ActiveTemplatePath)) throw new Exception("ActiveTemplate Path is missed.");
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                string[] TWG = Directory.GetFiles(TProjectSettings.ActiveTemplatePath, "*.twg", SearchOption.TopDirectoryOnly);
                if (TWG.Length > 0 && !string.IsNullOrEmpty(TWG[TWG.Length-1]))
                {
                    Exception exception = null;
                    TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Template");
                    TTerraWorld.FeedbackEvent(EventCategory.Templates, EventAction.Uses, Path.GetFileNameWithoutExtension(TWG[TWG.Length-1]));
                    TTerraWorld.TemplateName = Path.GetFileNameWithoutExtension(TWG[TWG.Length - 1]);
                    bool needsRegeneration = TTerraWorld.LoadTemplate(TWG[TWG.Length - 1], out exception);
                    if (exception != null) TDebug.LogErrorToUnityUI(exception);
                    TTerraworldGenerator.RunLoadedGraph(needsRegeneration);
                }
                else
                    EditorUtility.DisplayDialog("TERRAWORLD", "TEMPLATE UNAVAILABLE : The template is not avilable. Please sync templates or download it from templates forum.", "OK");

                status = TemplateManagerStatus.Idle;
            }
            catch (Exception e)
            {
                e.Data.Add("TW", "GRAPH ERROR.");
                TDebug.LogErrorToUnityUI(e);
                status = TemplateManagerStatus.Error;
            }
            finally
            {
                EnableScene();
            }
        }

        //public static void CancelDownload()
        //{
        //    ServerSync.CancelDownload();
        //}

        public static void CancelDownload2()
        {
            cacheService?.Cancel();
        }

        public static void DisableScene()
        {
            sceneParent = new GameObject("Template Scene Parent");
            Scene scene = SceneManager.GetActiveScene();
            sceneObjects = scene.GetRootGameObjects();

            for (int i = 0; i < sceneObjects.Length; i++)
            {
                // Skip gameobjects including camera to bypass Camera.Main errors when Unity can't find them on queries
                Component[] components = sceneObjects[i].GetComponentsInChildren(typeof(Camera), true);
                if (components != null && components.Length > 0) continue;

                sceneObjects[i].transform.parent = sceneParent.transform;
            }

            sceneParent.SetActive(false);
            ForceUpdateScene(10);
        }

        public static void EnableScene()
        {
            if (sceneObjects == null || sceneObjects.Length == 0 || sceneObjects[0] == null) return;
            for (int i = 0; i < sceneObjects.Length; i++) if (sceneObjects[i] != null) sceneObjects[i].transform.parent = null;
            if (sceneParent != null) MonoBehaviour.DestroyImmediate(sceneParent);
        }

        private static void ForceUpdateScene(int iterations)
        {
            for (int i = 0; i < iterations; i++) if (SceneView.lastActiveSceneView != null) SceneView.RepaintAll();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        //private static void DownloadTemplatePackageCompleted(Exception e, string retrievePath, bool isNew)
        //{
        //    status = TemplateManagerStatus.Importing;
        //
        //    //TODO: Expose the following line in UI later
        //    //bool forceImport = false;
        //
        //    //if (IsNew )
        //       ImportTemplatePackageNEW(retrievePath);
        //    //else
        //    //   ImportTemplatePackageEXISTING(retrievePath, forceImport);
        //}
        //
        //private static void ImportTemplatePackageEXISTING(string path, bool forceImport)
        //{
        //    try
        //    {
        //        string templateLocalPath = path.Replace(TAddresses.templatesPath_Pro_Cache, "");
        //        templateProjectPath = Path.GetFullPath(TAddresses.templatesPath_Pro + templateLocalPath);
        //
        //        if (forceImport)
        //        {
        //            string[] TUP = Directory.GetFiles(path, "*.unitypackage", SearchOption.TopDirectoryOnly);
        //
        //            if (TUP.Length > 0 && !string.IsNullOrEmpty(TUP[0]))
        //            {
        //                AssetDatabase.ImportPackage(TUP[0], false);
        //                AssetDatabase.importPackageCompleted += OnImportPackageCompletedEXISTING;
        //            }
        //        }
        //        else
        //            RunTemplateEXISTING(path);
        //    }
        //    catch
        //    {
        //        status = TemplateManagerStatus.Error;
        //        EditorUtility.DisplayDialog("IMPORT ERROR", "There was a problem importing unitypackage from cache folder!\n\nPlease try again!", "OK");
        //    }
        //    finally
        //    {
        //        status = TemplateManagerStatus.Idle;
        //    }
        //}
        //
        //private static void RunTemplateEXISTING(string path)
        //{
        //    try
        //    {
        //        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        //        needsRegeneration = false;
        //        string[] TWG = Directory.GetFiles(templateProjectPath, "*.twg", SearchOption.TopDirectoryOnly);
        //
        //        if (TWG.Length == 0)
        //        {
        //            string[] TUPCache = Directory.GetFiles(path, "*.unitypackage", SearchOption.AllDirectories);
        //
        //            if (TUPCache.Length > 0 && !string.IsNullOrEmpty(TUPCache[0]))
        //            {
        //                AssetDatabase.ImportPackage(TUPCache[0], false);
        //                AssetDatabase.importPackageCompleted += OnImportPackageCompletedFORCED;
        //            }
        //        }
        //        else
        //            RunTemplateFORCED();
        //    }
        //    catch
        //    {
        //        status = TemplateManagerStatus.Error;
        //        EditorUtility.DisplayDialog("GRAPH LOAD ERROR", "There was a problem loading graph from imported package!\n\nPlease try again!", "OK");
        //    }
        //}
        //
        //private static void RunTemplateFORCED()
        //{
        //    try
        //    {
        //        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        //        string[] TWG = Directory.GetFiles(templateProjectPath, "*.twg", SearchOption.AllDirectories);
        //
        //        if (TWG.Length > 0 && !string.IsNullOrEmpty(TWG[0]))
        //        {
        //            TTerraWorld.FeedbackEvent(EventCategory.UX, EventAction.Click, "Template");
        //            TTerraWorld.FeedbackEvent(EventCategory.Templates, EventAction.Uses, Path.GetFileNameWithoutExtension(TWG[0]));
        //            TTerraWorld.TemplateName = Path.GetFileNameWithoutExtension(TWG[0]);
        //            needsRegeneration = TTerraWorldGraph.LoadTemplate(TWG[0]);
        //        }
        //        else
        //            EditorUtility.DisplayDialog("GRAPH UNAVAILABLE", "The graph file is not avilable in the selected template folder!", "OK");
        //
        //        status = TemplateManagerStatus.Idle;
        //    }
        //    catch
        //    {
        //        status = TemplateManagerStatus.Error;
        //        EditorUtility.DisplayDialog("GRAPH LOAD ERROR", "There was a problem loading graph from imported package!\n\nPlease try again!", "OK");
        //    }
        //}
        //
        //private static void OnImportPackageCompletedEXISTING(string packagename)
        //{
        //    RunTemplateEXISTING(templateProjectPath);
        //}
        //
        //private static void OnImportPackageCompletedFORCED(string packagename)
        //{
        //    RunTemplateFORCED();
        //}
    }
}
#endif
#endif
#endif

