#if (TERRAWORLD_PRO || TERRAWORLD_LITE)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using TerraUnity.UI;

namespace TerraUnity.Edittime
{
    public class TemplateMaker : EditorWindow
    {
    //    [MenuItem("Tools/TerraUnity/Graph Package Maker", false, 3)]
        static void Init()
        {
            TemplateMaker window = (TemplateMaker)GetWindow(typeof(TemplateMaker));
            window.position = new Rect(5, 135, 480, 800);
            window.titleContent = new GUIContent("Graph Package Maker", "Graph Package Maker");
        }

        static string aboutText;
        static Object templateGraph;
        static string packageName = "My Graph";
        static List<string> assetPaths;
        //static string graphicsPath;
        //static string materialsPath;
        //static string texturesPath;
        //static string resourcesPathManager;

        private void OnEnable()
        {
            aboutText = "GRAPH EXPORTER" + "\n" +
                "Ver. " + "1.0";

            //#if TERRAWORLD_PRO
            //            graphicsPath = "Assets/TerraWorld/Core/Graphics/Pro";
            //            materialsPath = "Assets/TerraWorld/Core/Resources/Materials";
            //            texturesPath = "Assets/TerraWorld/Core/Resources/Textures";
            //#else
            //            graphicsPath = "Assets/TerraWorld/Core/Graphics/Lite";
            //            materialsPath = "Assets/TerraWorld/Core/Resources/Materials";
            //            texturesPath = "Assets/TerraWorld/Core/Resources/Textures";
            //#endif
            //            resourcesPathManager = "Assets/TerraWorld/Core/Common/Scripts/Sources/TResourcesManager.cs";
            assetPaths = new List<string>();
        }

        void OnGUI()
        {
            GUIStyle style = new GUIStyle(EditorStyles.toolbarButton); style.fixedHeight = 40;
            EditorGUI.BeginChangeCheck();
            templateGraph = THelpersUI.GUI_ObjectField(new GUIContent("GRAPH", "Insert TerraWorld's graph file in xml or twg format"), templateGraph, null, null, 40);
            if (EditorGUI.EndChangeCheck()) CheckGraphValidity();
            packageName = THelpersUI.GUI_TextField(new GUIContent("PACKAGE NAME", "Type Package name for the export"), packageName);

            GUILayout.Space(30);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(new GUIContent("GENERATE PACKAGE", "Generates unitypackage file from inserted TerraWorld graph"), style))
            {
                CreateTempelatePackage(templateGraph, packageName);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            THelpersUI.GUI_HelpBoxInfo(aboutText, 20);
        }

        private static void CheckGraphValidity()
        {
            if (templateGraph == null) return;
            string extension = Path.GetExtension(AssetDatabase.GetAssetPath(templateGraph));

            if (extension != ".xml" && extension != ".twg")
            {
                EditorUtility.DisplayDialog("TERRAWORLD","INVALID FORMAT : Insert a valid TerraWorld graph file in .xml or .twg format!", "Ok");
                templateGraph = null;
            }
        }

        public static void CreateTempelatePackage(Object graphToExport, string exportName)
        {
            if (graphToExport == null)
            {
                EditorUtility.DisplayDialog("TERRAWORLD","GRAPH NOT INSERTED : Insert a TerraWorld graph file in .xml or .twg format into the GRAPH slot first!", "Ok");
                return;
            }
            else
            {
                int progressID = TProgressBar.StartProgressBar("TERRAWORLD", "CREATING GRAPH PACKAGE", TProgressBar.ProgressOptionsList.Indefinite, false);
                TProgressBar.DisplayProgressBar("TERRAWORLD", "CREATING GRAPH PACKAGE", 0.5f, progressID);
                InitResources(exportName);
                AddCoreResources(graphToExport);
                AddGraphResources(graphToExport);
                ExportPackage(exportName);
                TProgressBar.RemoveProgressBar(progressID);
            }
        }

        private static void InitResources(string exportName)
        {
            if (string.IsNullOrEmpty(exportName)) exportName = "My Graph";
            assetPaths = new List<string>();
        }

        private static void AddCoreResources(Object graphToExport)
        {
            //string FolderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(graphToExport));
            //string[] dbFiles = Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories);
            //
            //foreach (string db in dbFiles)
            //{
            //    assetPaths.Add(db);
            //}

            assetPaths.Add(AssetDatabase.GetAssetPath(graphToExport));
            //assetPaths.Add(graphicsPath);
            //assetPaths.Add(materialsPath);
            //assetPaths.Add(texturesPath);
            //assetPaths.Add(resourcesPathManager);
        }

        private static void AddGraphResources(Object graphToExport)
        {
            List<TGraph> graphList = TTerraWorld.WorldGraph.LoadGraphList(AssetDatabase.GetAssetPath(graphToExport));

            for (int i = 0; i < graphList.Count; i++)
                for (int j = 0; j < graphList[i].nodes.Count; j++)
                {
                    List<string> nodeResources = graphList[i].nodes[j].GetResourcePaths();
                    if (nodeResources != null && nodeResources.Count > 0) assetPaths.AddRange(nodeResources);
                }
        }

        private static void ExportPackage(string exportName)
        {
            if (assetPaths == null || assetPaths.Count == 0) return;
            AssetDatabase.ExportPackage(assetPaths.ToArray(), exportName + ".unitypackage", ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Interactive);
        }
    }
}
#endif

