#if UNITY_EDITOR

/*
    _____  _____  _____  _____  ______
        |  _____ |      |      |  ___|
        |  _____ |      |      |     |
    
     U       N       I       T      Y
                                         
    
    TerraUnity Co. - Earth Simulation Tools - 2020
    
    http://terraunity.com
    info@terraunity.com
    
    TerraWorld's Core Project Unpacker
    
*/


using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System;
using System.IO;
using System.Reflection;
//using Ionic.Zip;
using UnityEngine.SceneManagement;

namespace TerraUnity.Edittime
{
    enum UnpackingStage
    {
        FirstTime,
        Updated,
        PostProcessInProgress,
        PostProcessDone,
        AddingLayersInProgress,
        AddingLayersDone,
        PlayerSettingsInProgress,
        PlayerSettingsDone,
        SetDefinesInProgress,
        SetDefinesDone,
        DecompressContentInProgress,
        DecompressContentDone,
        MoveDLLsForMacLinuxInProgress,
        MoveDLLsForMacLinuxDone,
        Manually,
        Finalize,
        FinalizingDone
    }

    [InitializeOnLoad]
    public class ProjectUnpacker : EditorWindow
    {
        [MenuItem("Tools/TerraUnity/Reimport TerraWorld", false, 1000)]
        static void Init()
        {
            ResetProjectSetupWindowShowedForVersion();
            EditorApplication.update += InitWindow;
        }

#if !TERRAWORLD_DEBUG
        [MenuItem("Tools/TerraUnity/Enable Development Mode", false, 1000)]
        static void EnableEditingModifier()
        {
            BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            if (!defineSymbols.Contains("TERRAWORLD_DEBUG")) defineSymbols = defineSymbols + ";TERRAWORLD_DEBUG;";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols);
        }
#else
        [MenuItem("Tools/TerraUnity/Disable Development Mode", false, 1000)]
        static void DisableEditingModifier()
        {
            BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            if (defineSymbols.Contains("TERRAWORLD_DEBUG")) defineSymbols = defineSymbols.Replace("TERRAWORLD_DEBUG", "");
            if (defineSymbols.Contains("TERRAWORLD_XPRO")) defineSymbols = defineSymbols.Replace("TERRAWORLD_XPRO", "");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols);
        }

#endif

        //static int PackageVersion = 2211;
        private static ProjectUnpacker window;
        private static Vector2 windowSize = new Vector2(540, 275);
        private static int bypassframe = 0;
        private static ListRequest listRequestPP;
        private static AddRequest addRequestPP;
        private static GameObject sceneParent;
        private static GameObject[] sceneObjects;
        private static Texture2D[] introImages;
        private static string statusString = "Initializing";
        private static float statusPercentage = 0.1f;
        private static DateTime lastTimePictureShows = new DateTime(2010, 6, 12, 18, 32, 0);
        private static int lastTimePictureIndex = 0;
        private const string packageNamePostProcessing = "com.unity.postprocessing";
        private const string proVersionDefineSymbol = "TERRAWORLD_PRO";
        private static UnpackingStage unpackingStage = UnpackingStage.FirstTime;
        private static bool isError;

        // TerraWorld Version Definition
        //-------------------------------------------------------------------------------------------------------
        private static bool isProVersionDefined;


        static ProjectUnpacker()
        {
            EditorApplication.update += InitWindow;
        }

        private static void InitWindow()
        {
            EditorApplication.update -= InitWindow;

            if (!CheckIfProjectSetupWindowShowedForVersion())
            {
                SetProjectSetupWindowShowedForVersion();

                if (EditorUtility.DisplayDialog("TERRAWORLD", "TerraWorld is going to import the package and set needed settings automatically.", "OK, Continue", "I will install the package manually!"))
                    SetProjectSetupStage(UnpackingStage.FirstTime);
                else
                {
                    SetProjectSetupStage(UnpackingStage.Manually);
                    EditorUtility.DisplayDialog("TERRAWORLD", "For manual installation please follow the installation steps described in \"INSTALLATION STEPS\" folder \n \n Also, You can reimport Terraworld from the menu any time. (Tools->TerraWorld->Reimport)", "OK");
                    TProjectSettings.InstallationCompleted();
                    return;
                }
            }

            if (GetProjectSetupStage() != UnpackingStage.Updated)
                if (GetProjectSetupStage() != UnpackingStage.Manually)
                {
                    bypassframe = 0;
                    updateIntroImages();
                    window = GetWindow<ProjectUnpacker>(true, "INITIALIZING TERRAWORLD PACKAGE ...", true);

                    window.position = new Rect
                        (
                            (Screen.currentResolution.width / 2) - (windowSize.x / 2),
                            (Screen.currentResolution.height / 2) - (windowSize.y / 2) - 150,
                            windowSize.x,
                            windowSize.y
                        );

                    window.minSize = new Vector2(windowSize.x, windowSize.y);
                    window.maxSize = new Vector2(windowSize.x, windowSize.y);
                    window.titleContent = new GUIContent("TerraWorld Auto Installer", "Unpacks TerraWorld content and reset settings if needed");
                }
        }

        public void OnGUI()
        {
            if (isError) CloseWindow();
            if (introImages == null) updateIntroImages();
            bypassframe++;
            TimeSpan travelTime = DateTime.Now - lastTimePictureShows;
            bool trigger = false;

            if (introImages != null && introImages.Length > 0)
            {
                if (travelTime.TotalSeconds > 4)
                {
                    lastTimePictureShows = DateTime.Now;
                    lastTimePictureIndex++;
                    lastTimePictureIndex %= introImages.Length;
                    trigger = true;
                }

                GUI.DrawTexture(new Rect(0, 0, 540, 255), introImages[lastTimePictureIndex], ScaleMode.ScaleAndCrop);
            }

            GUILayout.Space(300);

            //EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.HelpBox(new GUIContent("IMPORTING CONTENT", "Unpacking Core Content for project setup"), true);
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.EndHorizontal();
            //GUILayout.Space(5);

            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.x = 15;
            lastRect.y += 250;
            lastRect.width = this.position.width - 30;
            lastRect.height = 18;
            EditorGUI.ProgressBar(lastRect, statusPercentage, statusString);

            if (bypassframe > 100000)
            {
                if (EditorUtility.DisplayDialog("TERRAWORLD", "Process is taking more than usual! Do you want to continue ?", "Yes", "I will install package manually"))
                    bypassframe = 0;
                else
                {
                    AssetDatabase.Refresh();
                    this.Close();
                }
            }

            switch (unpackingStage)
            {
                case UnpackingStage.Updated:
                    TProjectSettings.InstallationCompleted();
                    CloseWindow();
                    break;
                case UnpackingStage.FirstTime:
                    statusPercentage = 0.1f;
                    statusString = "Initializing";
                    break;
                case UnpackingStage.PostProcessInProgress:
                    statusPercentage = 0.2f;
                    statusString = "Installing Post Processing";
                    break;
                case UnpackingStage.PostProcessDone:
                    statusPercentage = 0.2f;
                    statusString = "Installing Post Processing Done";
                    break;
                case UnpackingStage.AddingLayersInProgress:
                    statusPercentage = 0.3f;
                    statusString = "Adding Pre-Defined Layers";
                    break;
                case UnpackingStage.AddingLayersDone:
                    statusPercentage = 0.3f;
                    statusString = "Pre-Defined Layers Done";
                    break;
                case UnpackingStage.PlayerSettingsInProgress:
                    statusPercentage = 0.4f;
                    statusString = "Setting Up Player Settings";
                    break;
                case UnpackingStage.PlayerSettingsDone:
                    statusPercentage = 0.4f;
                    statusString = "Setting Up Player Settings Done";
                    break;
                case UnpackingStage.DecompressContentInProgress:
                    statusPercentage = 0.5f;
                    statusString = "Importing Core Contents";
                    break;
                case UnpackingStage.DecompressContentDone:
                    statusPercentage = 0.5f;
                    statusString = "Importing Core Contents Done";
                    break;
                case UnpackingStage.MoveDLLsForMacLinuxInProgress:
                    statusPercentage = 0.8f;
                    statusString = "Importing Libraries";
                    break;
                case UnpackingStage.MoveDLLsForMacLinuxDone:
                    statusPercentage = 0.8f;
                    statusString = "Importing Libraries Done";
                    break;
                case UnpackingStage.SetDefinesInProgress:
                    statusPercentage = 0.9f;
                    statusString = "Setting Up TerraWorld Defines";
                    break;
                case UnpackingStage.SetDefinesDone:
                    statusPercentage = 0.9f;
                    statusString = "Setting Up TerraWorld Defines Done";
                    break;
                default:
                    break;
            }

            Repaint();

            if (trigger)
                DoActione();
        }

        private static bool DoActione()
        {
            bool ChangedToNextAction = false;
            unpackingStage = GetProjectSetupStage();

#if TERRAWORLD_DEBUG
            // DONOT REMOVE THIS LOG IN RELEASE
            Debug.Log(unpackingStage.ToString());
#endif
            switch (unpackingStage)
            {
                case UnpackingStage.FirstTime:
                    ChangedToNextAction = true;
                    SetProjectSetupStage(UnpackingStage.PostProcessInProgress);
                    PostProcessingInstalled();
                    break;
                case UnpackingStage.PostProcessDone:
                    ChangedToNextAction = true;
                    SetProjectSetupStage(UnpackingStage.AddingLayersInProgress);
                    AddTerraWorldLayers();
                    break;
                case UnpackingStage.AddingLayersDone:
                    ChangedToNextAction = true;
                    SetProjectSetupStage(UnpackingStage.PlayerSettingsInProgress);
                    SetProjectSettings();
                    break;
                case UnpackingStage.PlayerSettingsDone:
                    ChangedToNextAction = true;
                    SetProjectSetupStage(UnpackingStage.DecompressContentInProgress);
                    DecompressContent();
                    break;
                case UnpackingStage.DecompressContentDone:
                    ChangedToNextAction = true;
                    SetProjectSetupStage(UnpackingStage.MoveDLLsForMacLinuxInProgress);
                    MoveDLLsForMacLinux();
                    break;
                case UnpackingStage.MoveDLLsForMacLinuxDone:
                    ChangedToNextAction = true;
                    SetProjectSetupStage(UnpackingStage.SetDefinesInProgress);
                    SetDefines();
                    break;
                case UnpackingStage.SetDefinesDone:
                    ChangedToNextAction = true;
                    SetProjectSetupStage(UnpackingStage.Finalize);
                    FinalizeInstallation();
                    break;
                case UnpackingStage.FinalizingDone:
                    ChangedToNextAction = true;
                    SetProjectSetupStage(UnpackingStage.Updated);
                    break;
                case UnpackingStage.Updated:
                    break;
                default:
                    break;
            }

            return ChangedToNextAction;
        }

        private static bool CheckIfProjectSetupWindowShowedForVersion()
        {
            bool result = false;
            if (PlayerPrefs.GetInt("Terra_ProjectUnpackerWindowShowedForVersion") == TVersionController.Version) result = true;
            return result;
        }

        private static void SetProjectSetupWindowShowedForVersion()
        {
            PlayerPrefs.SetInt("Terra_ProjectUnpackerWindowShowedForVersion", TVersionController.Version);
            PlayerPrefs.Save();
        }

        private static void ResetProjectSetupWindowShowedForVersion()
        {
            PlayerPrefs.SetInt("Terra_ProjectUnpackerWindowShowedForVersion", 0);
            PlayerPrefs.Save();
        }

        private static void SetProjectSetupStage(UnpackingStage unpackingStage)
        {
            PlayerPrefs.SetString("Terra_ProjectUnpackerStage_" + TVersionController.Version.ToString(), unpackingStage.ToString());
            PlayerPrefs.Save();
        }

        private static UnpackingStage GetProjectSetupStage()
        {
            UnpackingStage unpackingStage;

            try
            {
                string unpackingStageString = PlayerPrefs.GetString("Terra_ProjectUnpackerStage_" + TVersionController.Version.ToString());
                unpackingStage = (UnpackingStage)Enum.Parse(typeof(UnpackingStage), unpackingStageString, true);
            }
            catch (Exception)
            {
                unpackingStage = UnpackingStage.FirstTime;
            }

            return unpackingStage;
        }


        private static void PostProcessingInstalled()
        {
            listRequestPP = Client.List();
            EditorApplication.update += CheckPackageInstalledPP;
        }

        private static void CheckPackageInstalledPP()
        {
            bool isPostProcessingInstalled = false;

            if (listRequestPP.IsCompleted)
            {
                EditorApplication.update -= CheckPackageInstalledPP;

                if (listRequestPP.Status == StatusCode.Success)
                {
                    foreach (var package in listRequestPP.Result)
                        if (package.name.Equals(packageNamePostProcessing))
                        {
                            SetProjectSetupStage(UnpackingStage.PostProcessDone);
                            isPostProcessingInstalled = true;
                        }
                }
                else if (listRequestPP.Status >= StatusCode.Failure)
                {
                    isError = true;
                    Debug.Log(listRequestPP.Error.message);
                }

                if (!isPostProcessingInstalled)
                    InstallPostProcessing();
            }
        }

        private static void SetProjectSettings()
        {
            BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);

            if (PlayerSettings.GetApiCompatibilityLevel(btg) != ApiCompatibilityLevel.NET_4_6)
                PlayerSettings.SetApiCompatibilityLevel(btg, ApiCompatibilityLevel.NET_4_6);

#if !UNITY_2019_2_OR_NEWER
            if (PlayerSettings.scriptingRuntimeVersion != ScriptingRuntimeVersion.Latest)
                PlayerSettings.scriptingRuntimeVersion = ScriptingRuntimeVersion.Latest;
#endif

            if (!PlayerSettings.allowUnsafeCode)
                PlayerSettings.allowUnsafeCode = true;

            if (PlayerSettings.colorSpace != ColorSpace.Linear)
                if (EditorUtility.DisplayDialog("TERRAWORLD", "For better visual experience, TerraWorld is going to change the \" Color Space\" setting to \"Linear\" mode. This option can be changed any time from the \"Project Settings\" later. \n Do you want to change it now?", "Yes", "No"))
                    PlayerSettings.colorSpace = ColorSpace.Linear;

            SetProjectSetupStage(UnpackingStage.PlayerSettingsDone);
        }

        private static void AddTerraWorldLayers()
        {
            SerializedObject layerSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = layerSettings.FindProperty("layers");
            string[] newLayers = new string[8] { "PostProcessing", "Terra_Vegetation", "Terra_Geometry", "Player", "Target", "Weapon", "Projectile", "PlayerCollisionOnly" };
            int counter = 0;
            int i = 8;

            for (i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(i);

                if (layer.stringValue == "")
                {
                    bool elementAlreadyExists = false;

                    for (int j = 8; j < layers.arraySize; j++)
                    {
                        if (layers.GetArrayElementAtIndex(j).stringValue == newLayers[counter])
                            elementAlreadyExists = true;
                    }

                    if (!elementAlreadyExists)
                    {
                        layer.stringValue = newLayers[counter];
                        layerSettings.ApplyModifiedProperties();
                    }
                    else
                        i--;

                    counter++;
                }

                if (counter == newLayers.Length) break;
            }

            layerSettings.Update();
            SetProjectSetupStage(UnpackingStage.AddingLayersDone);
        }

        private static void InstallPostProcessing()
        {
            addRequestPP = Client.Add(packageNamePostProcessing);
            EditorApplication.update += InstallPackagePP;
        }

        private static void InstallPackagePP()
        {
            if (addRequestPP.IsCompleted)
            {
                EditorApplication.update -= InstallPackagePP;

                if (addRequestPP.Status == StatusCode.Success)
                {
                    SetProjectSetupStage(UnpackingStage.PostProcessDone);
                    Debug.Log("Installed: " + addRequestPP.Result.packageId);
                }
                else if (addRequestPP.Status >= StatusCode.Failure)
                {
                    isError = true;
                    Debug.Log(addRequestPP.Error.message);
                }
            }
        }

        /// <summary>
        /// Helper to see if the specified BuildTarget is installed in the editor.
        /// </summary>
        private static bool IsPlatformInstalled(BuildTarget buildTarget)
        {
            Type moduleManager = Type.GetType("UnityEditor.Modules.ModuleManager, UnityEditor.dll");
            MethodInfo isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", BindingFlags.Static | BindingFlags.NonPublic);
            return (bool)isPlatformSupportLoaded.Invoke(null, new object[] { (string)getTargetStringFromBuildTarget.Invoke(null, new object[] { buildTarget }) });
        }

        private static void RemoveOldPreprocessors()
        {
            BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);

            if (!defineSymbols.Contains(proVersionDefineSymbol))
                PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols + ";" + proVersionDefineSymbol);
        }

        private static void SetDefines()
        {
            //foreach (BuildTarget bt in Enum.GetValues(typeof(BuildTarget)))
            //{
            //    if (IsPlatformInstalled(bt))
            //    {
            //        BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
            //
            //        string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            //        if (!defineSymbols.Contains(proVersionDefineSymbol))
            //            PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols + ";" + proVersionDefineSymbol);
            //    }
            //}

            BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);

            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            if (!defineSymbols.Contains(proVersionDefineSymbol))
                PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols + ";" + proVersionDefineSymbol);

            //defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            //if (!defineSymbols.Contains(templatesDefineSymbol))
            //    PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols + ";" + templatesDefineSymbol);

            SetProjectSetupStage(UnpackingStage.SetDefinesDone);
        }

        private static void FinalizeInstallation()
        {
            AssetDatabase.StartAssetEditing();
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            
            foreach (string assetPath in allAssetPaths)
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath(assetPath, typeof(MonoScript)) as MonoScript;
                if (script != null)
                {
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    break;
                }
            }

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();

            SetProjectSetupStage(UnpackingStage.FinalizingDone);
        }

        private static void DecompressContent()
        {
          //  string[] dbFiles = Directory.GetFiles(Application.dataPath, "*." + TVersionController.Version, SearchOption.AllDirectories);
          //
          //  try
          //  {
          //      //DisableScene();
          //
          //      foreach (string db in dbFiles)
          //      {
          //          using (ZipFile zip = new ZipFile(db))
          //          {
          //              string dirName = Path.GetDirectoryName(db) + "/" + Path.GetFileNameWithoutExtension(db);
          //
          //              if (Directory.Exists(dirName))
          //                  Directory.Delete(dirName, true);
          //
          //              zip.ExtractAll(Path.GetDirectoryName(db), ExtractExistingFileAction.OverwriteSilently);
          //
          //          }
          //      }
          //
          //      //EnableScene();
                SetProjectSetupStage(UnpackingStage.DecompressContentDone);
          //  }
          //  catch (Exception e)
          //  {
          //      Debug.Log(e.Message);
          //      isError = true;
          //  }
          //  finally
          //  {
          //      //EnableScene();
          //  }
        }

        private static void updateIntroImages()
        {
            string ImagesPath = Application.dataPath + "/TerraWorld/Installer/IntroImages";
            string[] imageFiles = Directory.GetFiles(ImagesPath, "*.png", SearchOption.TopDirectoryOnly);
            introImages = new Texture2D[imageFiles.Length];

            try
            {
                for (int i = 0; i < imageFiles.Length; i++)
                {
                    string imageLocalPath = imageFiles[i].Replace(Application.dataPath, "Assets");
                    introImages[i] = AssetDatabase.LoadAssetAtPath(imageLocalPath, typeof(Texture2D)) as Texture2D;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                isError = true;
            }
            finally
            {

            }
        }

        public static string ReplaceLastOccurrence(string source, string find, string replace, bool trim = true)
        {
            string result = source;
            int place = source.LastIndexOf(find);
            if (place == -1) return result;
            if (trim) result = source.Remove(place).Insert(place, replace);
            else result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        private static void MoveDLLsForMacLinux()
        {
            try
            {
#if UNITY_EDITOR_OSX
                // Edit Unity's default mono config file
                string configPathSrc = EditorApplication.applicationContentsPath
                + Path.DirectorySeparatorChar + "MonoBleedingEdge/etc/mono/config";

                string configPathDst = EditorApplication.applicationContentsPath
                + Path.DirectorySeparatorChar + "MonoBleedingEdge/etc/mono/config_edited";

                string line = null;
                string skippingString = "gdiplus";

                using (StreamReader reader = new StreamReader(configPathSrc))
                {
                    using (StreamWriter writer = new StreamWriter(configPathDst))
                    {
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains(skippingString)) continue;
                            writer.WriteLine(line);
                        }
                    }
                }
            
                File.Delete(configPathSrc);
                File.Move(configPathDst, configPathSrc);

                //// Move provided dylib files to the project root
                //string libgdiplusPath = Application.dataPath
                //+ Path.DirectorySeparatorChar + "TerraWorld"
                //+ Path.DirectorySeparatorChar + "_INSTALLATION STEPS"
                //+ Path.DirectorySeparatorChar + "libgdiplus.dyli_";
                //
                //string libLerc64Path = Application.dataPath
                //+ Path.DirectorySeparatorChar + "TerraWorld"
                //+ Path.DirectorySeparatorChar + "_INSTALLATION STEPS"
                //+ Path.DirectorySeparatorChar + "libLerc64.dyli_";
                //
                //string libgdiplusDstPath = ReplaceLastOccurrence(Application.dataPath, "Assets", "libgdiplus.dylib");
                //string libLerc64DstPath = ReplaceLastOccurrence(Application.dataPath, "Assets", "libLerc64.dylib");
                //
                //File.Copy(libgdiplusPath, libgdiplusDstPath, true);
                //File.Copy(libLerc64Path, libLerc64DstPath, true);
                //
                //libgdiplusDstPath = ReplaceLastOccurrence(Application.dataPath, "Assets", "Assets/Plugins/libgdiplus.dylib");
                //libLerc64DstPath = ReplaceLastOccurrence(Application.dataPath, "Assets", "Assets/Plugins/libLerc64.dylib");
                //
                //File.Copy(libgdiplusPath, libgdiplusDstPath, true);
                //File.Copy(libLerc64Path, libLerc64DstPath, true);
#endif

#if UNITY_EDITOR_LINUX

                EditorUtility.DisplayDialog("TERRAWORLD", "TERRAWORLD is based on \"MONO Liberary\". Please be aware that the complete Mono Library should be installed on your computer to get Terraword working.", "Ok") ;

                // Edit Unity's default mono config file
                string configPathSrc = EditorApplication.applicationContentsPath
                + Path.DirectorySeparatorChar + "MonoBleedingEdge/etc/mono/config";

                string configPathbkp = EditorApplication.applicationContentsPath
                + Path.DirectorySeparatorChar + "MonoBleedingEdge/etc/mono/config_Backup_Terraworld";

                string configPathDst = EditorApplication.applicationContentsPath
                + Path.DirectorySeparatorChar + "MonoBleedingEdge/etc/mono/config_edited";

                if (!File.Exists(configPathbkp))
                {
                    string line = null;
                    string skippingString = "gdiplus";

                    Debug.Log("TERRAWORLD : " + configPathSrc + " edited. A backup copy has been created beside the orginal file.");



                    using (StreamReader reader = new StreamReader(configPathSrc))
                    {
                        using (StreamWriter writer = new StreamWriter(configPathDst))
                        {
                            while ((line = reader.ReadLine()) != null)
                            {
                                if (line.Contains(skippingString)) continue;
                                writer.WriteLine(line);
                            }
                        }
                    }

                    File.Copy(configPathSrc, configPathbkp);
                    File.Delete(configPathSrc);
                    File.Move(configPathDst, configPathSrc);
                }

                //// Move provided so files to the project root
                //string libgdiplusPath = Application.dataPath
                //+ Path.DirectorySeparatorChar + "TerraWorld"
                //+ Path.DirectorySeparatorChar + "_INSTALLATION STEPS"
                //+ Path.DirectorySeparatorChar + "libgdiplus.s_";
                //
                //string libLerc64Path = Application.dataPath
                //+ Path.DirectorySeparatorChar + "TerraWorld"
                //+ Path.DirectorySeparatorChar + "_INSTALLATION STEPS"
                //+ Path.DirectorySeparatorChar + "Lerc64.s_";
                //
                //string libgdiplusDstPath = ReplaceLastOccurrence(Application.dataPath, "Assets", "libgdiplus.so");
                //string libLerc64DstPath = ReplaceLastOccurrence(Application.dataPath, "Assets", "Lerc64.so");
                //
                //File.Copy(libgdiplusPath, libgdiplusDstPath, true);
                //File.Copy(libLerc64Path, libLerc64DstPath, true);
                //
                //libgdiplusDstPath = ReplaceLastOccurrence(Application.dataPath, "Assets", "Assets/Plugins/libgdiplus.so");
                //libLerc64DstPath = ReplaceLastOccurrence(Application.dataPath, "Assets", "Assets/Plugins/Lerc64.so");
                //
                //File.Copy(libgdiplusPath, libgdiplusDstPath, true);
                //File.Copy(libLerc64Path, libLerc64DstPath, true);
#endif
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                SetProjectSetupStage(UnpackingStage.MoveDLLsForMacLinuxDone);
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void CloseWindow()
        {
            AssetDatabase.Refresh();
            this.Close();
        }

        public static void DisableScene()
        {
            sceneParent = new GameObject("Template Scene Parent");
            Scene scene = SceneManager.GetActiveScene();
            sceneObjects = scene.GetRootGameObjects();
            for (int i = 0; i < sceneObjects.Length; i++) sceneObjects[i].transform.parent = sceneParent.transform;
            sceneParent.SetActive(false);
            ForceUpdateScene(10);
        }

        public static void EnableScene()
        {
            for (int i = 0; i < sceneObjects.Length; i++) if (sceneObjects[i] != null) sceneObjects[i].transform.parent = null;
            if (sceneParent != null) MonoBehaviour.DestroyImmediate(sceneParent);
        }

        private static void ForceUpdateScene(int iterations)
        {
            for (int i = 0; i < iterations; i++) if (SceneView.lastActiveSceneView != null) SceneView.RepaintAll();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        //private static void OnImportPackageStarted (string packagename)
        //{
        //    AssetDatabase.importPackageStarted -= OnImportPackageStarted;
        //    DisableScene();
        //    AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
        //}
        //
        //private static void OnImportPackageCompleted (string packagename)
        //{
        //    AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
        //    EnableScene();
        //}
    }
}
#endif

