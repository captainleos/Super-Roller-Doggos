#if UNITY_EDITOR
using UnityEditor;
using System;
using System.IO;
using UnityEngine.SceneManagement;
using TerraUnity.Runtime;

namespace TerraUnity.Edittime
{
    public class TAddresses
    {
        #region Public Fields

        
        private static string _persistentDataPath = UnityEngine.Application.persistentDataPath.Remove(UnityEngine.Application.persistentDataPath.LastIndexOf("/")) + "/";

        public static string projectPath
        {
            get
            {
                string _dataPath = UnityEngine.Application.dataPath + "/";
                return _dataPath.Replace("/Assets", "");
            }
        }

        public static string TerraWorldPath
        {
            get
            {
                return "Assets/" + "TerraWorld/";
            }
        }

        public static string corePath
        {
            get
            {
                return TerraWorldPath + "Core/";
            }
        }

        public static string helpPath
        {
            get
            {
                return TerraWorldPath + "Help/";
            }
        }

        public static string macInstructionsPath
        {
            get
            {
                return TerraWorldPath + "_INSTALLATION STEPS/";
            }
        }

        public static string GetWorkDirectoryPath(int ID)
        {
            string _path = TerraWorldPath + "World Resources/Scenes/" + ID + "/";
            CreateDirectory(_path);
            return _path;
        }

      //  public static string generatedTerrainsPath (string subDirectory)
      //  {
      //      string path = WorkDirectoryPath + subDirectory + "/";
      //      CreateDirectory(path);
      //      return path;
      //  }

        public static string presetsPath
        {
            get
            {
                string path = corePath + "Presets/";
                return path;
            }
        }

        public static string templatesPath
        {
            get
            {
                string path = TerraWorldPath + "Templates";
                CreateDirectory(path);
                return path;
            }
        }

        public static string templatesPath_Pro
        {
            get
            {
                string path = TerraWorldPath + "Templates/Pro/";
                CreateDirectory(path);
                return path;
            }
        }

        public static string templatesPath_Pro_FullPath
        {
            get
            {
                string _dataPath = UnityEngine.Application.dataPath + "/";
                string path = _dataPath + templatesPath_Pro;
                return path;
            }
        }

        public static string templatesPath_Lite_Mobile
        {
            get
            {
                string path = TerraWorldPath + "Templates/Lite/Mobile/";
                return path;
            }
        }

        public static string templatesPath_Pro_Cache
        {
            get
            {
                string path = _persistentDataPath + "TerraUnityCo/TerraWorld/Cache/Templates/Pro/";
                CreateDirectory(path);
                return path;
            }
        }

        public static string scenesPath_Pro_Cache
        {
            get
            {
                string path = _persistentDataPath + "TerraUnityCo/TerraWorld/Cache/Scenes/Pro/";
                CreateDirectory(path);
                return path;
            }
        }

        public static string systemTempPath
        {
            get
            {
                return Path.GetTempPath() + "TerraWorld/";
            }
        }

        public static string externalPath
        {
            get
            {
                return projectPath + "TerraWorld/";
            }
        }

        public static string cachePath
        {
            get
            {
                return externalPath + "Cache/";
            }
        }

        public static string cachePathElevation
        {
            get
            {
                return cachePath + "Elevation Data/";
            }
        }

        public static string cachePathImagery
        {
            get
            {
                return cachePath + "Imagery Data/";
            }
        }

        public static string cachePathLandcover
        {
            get
            {
                return cachePath + "Landcover Data/";
            }
        }

        public static string cachePathServers
        {
            get
            {
                return cachePath + "Cached Servers/";
            }
        }

        public static string serverPathElevation
        {
            get
            {
                return cachePathServers + "/Elevation/";
            }
        }

        public static string serverPathImagery
        {
            get
            {
                return cachePathServers + "/Imagery/";
            }
        }
        public static string serverPathInfo
        {
            get
            {
                return cachePathServers + "/Info/";
            }
        }

        public static string downloadDate
        {
            get
            {
                return DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss/");
            }
        }

        private static void CreateDirectory (string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        #endregion
    }
}
#endif

