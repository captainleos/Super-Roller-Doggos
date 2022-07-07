#if UNITY_EDITOR
using UnityEditor;
using System;
using System.IO;

namespace TerraUnity.Edittime
{
    public enum AssetTypes
    {
        Material,
        Image
    }

    public abstract class TObjects 
    {
        private static int Counter;
        protected AssetTypes _assetType;

        protected string _ObjPath;
        public string ObjectPath
        {
            get { return OnGetAssetPath(); }
            set { OnSetAssetAtPath(value); }
        }

        private string _ID;
        public string ID { get => _ID; }

        public TObjects (AssetTypes assetType)
        {
            _assetType = assetType;
            Random rand = new Random((int)DateTime.Now.Ticks);
            _ID = (rand.Next() + Counter++).ToString();
        }

        public abstract string GenerateAssetPath();

        public string OnGetAssetPath()
        {
            if (string.IsNullOrEmpty(_ObjPath))
                _ObjPath = GenerateAssetPath();

            SaveObject(_ObjPath);

            return _ObjPath;
        }

        public abstract void SaveObject(string path);

        public virtual void OnSetAssetAtPath(string path)
        {
            _ObjPath = path;
        }

        public UnityEngine.Object GetObject()
        {
            return AssetDatabase.LoadMainAssetAtPath(ObjectPath);
        }

        public bool SetObject(UnityEngine.Object _obj)
        {
            try
            {
                if (File.Exists(_ObjPath)) AssetDatabase.DeleteAsset(_ObjPath);
                AssetDatabase.CreateAsset(_obj, _ObjPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
#endif

