#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace TerraUnity.Edittime
{

    public static class TProjectSettings
    {

        //public static bool IsReplaceAndUpdate
        //{
        //    set
        //    {
        //        if (value)
        //            SetRegKeyInt("IsReplaceAndUpdate", 1);
        //        else
        //            SetRegKeyInt("IsReplaceAndUpdate", 0);
        //    }
        //    get
        //    {
        //        if (PlayerPrefs.HasKey("IsReplaceAndUpdate"))
        //        {
        //            if (GetRegKeyInt("LastTeamMessageNum") == 0)
        //                return false;
        //            else
        //                return true;
        //        }
        //        else
        //        {
        //            SetRegKeyInt("LastTeamMessageNum", 1);
        //            return true;
        //        }
        //    }
        //}

        public static bool DebugLogSystem
        {
            set
            {
                if (value)
                    SetRegKeyInt("DebugLogSystem", 1);
                else
                    SetRegKeyInt("DebugLogSystem", 0);
            }
            get
            {
#if TERRAWORLD_DEBUG
                return true ;
#else
                if (PlayerPrefs.HasKey("DebugLogSystem"))
                {
                    if (GetRegKeyInt("DebugLogSystem") == 0) return false; else return true;
                }
                else
                {
                    SetRegKeyInt("DebugLogSystem", 1);
                    return false;
                }
#endif
            }
        }


        public static bool ErrorLog
        {
            set
            {
                if (value)
                    SetRegKeyInt("ErrorLog", 1);
                else
                    SetRegKeyInt("ErrorLog", 0);
            }
            get
            {
#if TERRAWORLD_DEBUG
                return true ;
#else
                if (PlayerPrefs.HasKey("ErrorLog"))
                {
                    if (GetRegKeyInt("ErrorLog") == 0) return false; else return true;
                }
                else
                {
                    SetRegKeyInt("ErrorLog", 0);
                    return false;
                }

#endif
            }
        }

        public static bool FeedbackSystem
        {
            set
            {
                if (value)
                    SetRegKeyInt("FeedbackSystem", 1);
                else
                    SetRegKeyInt("FeedbackSystem", 0);
#if TERRAWORLD_PRO
                TTerraWorld.FeedbackSystem = value;
#endif
            }
            get
            {
                if (PlayerPrefs.HasKey("FeedbackSystem"))
                {
                    if (GetRegKeyInt("FeedbackSystem") == 0) return false; else return true;
                }
                else
                {
                    SetRegKeyInt("FeedbackSystem", 1);
                    return true;
                }
            }
        }

        public static bool NewGraphSystem
        {
            set
            {
                if (value)
                {
                    BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
                    BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
                    string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
                    if (!defineSymbols.Contains("TERRAWORLD_XPRO"))
                        defineSymbols = defineSymbols + ";TERRAWORLD_XPRO;";
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols);
                }
                else
                {
                    BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
                    BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
                    string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
                    if (defineSymbols.Contains("TERRAWORLD_XPRO"))
                        defineSymbols = defineSymbols.Replace("TERRAWORLD_XPRO", "");
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols);
                }
            }
            get
            {
                BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
                BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
                if (defineSymbols.Contains("TERRAWORLD_XPRO"))
                    return true;
                else
                    return false;
            }
        }

        public static bool CacheData
        {
            set
            {
                if (value)
                    SetRegKeyInt("CacheData", 1);
                else
                    SetRegKeyInt("CacheData", 0);
            }
            get
            {
                if (PlayerPrefs.HasKey("CacheData"))
                {
                    if (GetRegKeyInt("CacheData") == 0) return false; else return true;
                }
                else
                {
                    SetRegKeyInt("CacheData", 1);
                    return true;
                }
            }
        }


        public static int PreviewResolution
        {
            set
            {
                if (value > 0)
                    SetRegKeyInt("PreviewResolution", value);
            }
            get
            {
                if (PlayerPrefs.HasKey("PreviewResolution"))
                {

                    return GetRegKeyInt("PreviewResolution");
                }
                else
                {
                    SetRegKeyInt("PreviewResolution", 128);
                    return 128;
                }
            }
        }

        public static int SceneViewGUI
        {
            get
            {
                if (PlayerPrefs.HasKey("SceneViewGUI"))
                    return GetRegKeyInt("SceneViewGUI");
                else
                {
                    SetRegKeyInt("SceneViewGUI", 0);
                    return 0;
                }
            }
            set
            {
                SetRegKeyInt("SceneViewGUI", value);
            }
        }

        public static string ActiveTemplatePath
        {
            get
            {
                if (PlayerPrefs.HasKey("ActiveTemplatePath"))
                    return PlayerPrefs.GetString("ActiveTemplatePath");
                else
                {
                    return null;
                }
            }
            set
            {
                PlayerPrefs.SetString("ActiveTemplatePath",value);
            }
        }


        private static bool SetRegKeyInt(string key, int value)
        {
            try
            {
                PlayerPrefs.SetInt(key, value);
                PlayerPrefs.Save();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static DateTime LastTimeTemplateSyncCalled
        {
            get
            {
                DateTime result = DateTime.MinValue;
                try
                {
                    string lastTerrWorldRunTime = PlayerPrefs.GetString("LastTimeTemplateSyncCalled");
                    PlayerPrefs.SetString("LastTimeTemplateSyncCalled", DateTime.Now.ToString());
                    DateTime.TryParse(lastTerrWorldRunTime, out result);
                    return result;
                }
                catch
                {
                    return result;
                }
            }
        }

        public static DateTime LastTimeUpdateWindowShowed
        {
            get
            {
                DateTime result = DateTime.MinValue;
                try
                {
                    string lastTerrWorldRunTime = PlayerPrefs.GetString("LastTimeUpdateWindowShowed");
                    PlayerPrefs.SetString("LastTimeUpdateWindowShowed", DateTime.Now.ToString());
                    DateTime.TryParse(lastTerrWorldRunTime, out result);
                    return result;
                }
                catch
                {
                    return result;
                }
            }
        }

        private static int GetRegKeyInt(string key)
        {
            try
            {
                if (UnityEngine.PlayerPrefs.HasKey(key)) return UnityEngine.PlayerPrefs.GetInt(key);
                else return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static bool IsInstalled
        {
            get
            {
                if (GetRegKeyInt("TWInstalled") == 1) return true;
                else return false;
            }
        }

        public static int LastTeamMessageNum
        {
            set
            {
                SetRegKeyInt("LastTeamMessageNum", value);
            }
            get
            {
                if (GetRegKeyInt("LastTeamMessageNum") != 0) return GetRegKeyInt("LastTeamMessageNum");
                else
                {
                    SetRegKeyInt("LastTeamMessageNum", 0);
                    return 0;
                }
            }
        }

        public static void InstallationCompleted()
        {
            SetRegKeyInt("TWInstalled", 1);
            SetRegKeyInt("TWVERSION", TVersionController.Version);
        }

        public static bool IsInstallationCompleted()
        {
            if (GetInstalledVersion() == TVersionController.Version)
                return true;
            else
                return false;
        }

        public static int GetInstalledVersion()
        {
            int result = 0;

            try
            {
                result = GetRegKeyInt("TWVERSION");
                return result;
            }
            catch
            {
                result = 0;
            }

            return result;
        }

        public static bool IsIntroWindowShowed()
        {
            try
            {
                if (TVersionController.Version == GetRegKeyInt("IntroWindowShowed"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void SetIntroWindowShowed()
        {
            SetRegKeyInt("IntroWindowShowed", TVersionController.Version);
        }

    }
}
#endif

