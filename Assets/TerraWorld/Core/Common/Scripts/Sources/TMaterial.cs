#if TERRAWORLD_PRO
#if UNITY_EDITOR
using UnityEngine;
using System.IO;

namespace TerraUnity.Edittime
{
    public class TMaterial : TObjects
    {
        private Material _cachedMaterial = null;

        public TMaterial ():base(AssetTypes.Material)
        {

        }

        public override string GenerateAssetPath()
        {
            return TTerraWorld.WorkDirectoryLocalPath + "Asset_" + ID + ".mat";  
        }

        public override void SaveObject(string path)
        {
            _ObjPath = path;
            if (File.Exists(_ObjPath)) return;
            string fullPath = Path.GetFullPath(path);
            string projectPath = fullPath.Substring(fullPath.LastIndexOf("Assets"));

            if (_cachedMaterial != null)
            {
            }
        }
    }
}
#endif
#endif
