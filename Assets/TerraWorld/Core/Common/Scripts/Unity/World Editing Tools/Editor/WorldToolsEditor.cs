#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace TerraUnity.Runtime
{
    [CustomEditor(typeof(WorldTools))]
    [CanEditMultipleObjects]
    public class WorldToolsEditor : TBrushEditor
    {
        public static bool refresh;

        public void InitLists(bool forced = false)
        {
            // Layers list
            if
            (
                forced ||
                script.GPULayers == null ||
                script.GPULayers.Count == 0
            )
            {
                script.GPULayers = new List<TScatterParams>();

                foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                    if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                        if (go.GetComponent<TScatterParams>() != null)
                            script.GPULayers.Add(go.GetComponent<TScatterParams>());
            }

            if
            (
                forced ||
                script.grassLayers == null ||
                script.grassLayers.Count == 0
            )
            {
                script.grassLayers = new List<GrassLayer>();

                foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                    if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                        if (go.GetComponent<GrassLayer>() != null)
                            script.grassLayers.Add(go.GetComponent<GrassLayer>());
            }

            // Editable states
            if
            (
                script.editableGPU == null ||
                script.editableGPU.Count == 0 ||
                script.editableGPU.Count != script.GPULayers.Count
            )
            {
                if (script.editableGPU == null) script.editableGPU = new List<bool>();

                if (script.GPULayers != null && script.GPULayers.Count > 0 && script.GPULayers[0] != null)
                    for (int i = 0; i < script.GPULayers.Count; i++)
                        if (i > script.editableGPU.Count - 1)
                            script.editableGPU.Add(true);
            }

            if
            (
                script.editableGrass == null ||
                script.editableGrass.Count == 0 ||
                script.editableGrass.Count != script.grassLayers.Count
            )
            {
                script.editableGrass = new List<bool>();

                if (script.grassLayers != null && script.grassLayers.Count > 0 && script.grassLayers[0] != null)
                    for (int i = 0; i < script.grassLayers.Count; i++)
                        if (i > script.editableGrass.Count - 1)
                            script.editableGrass.Add(true);
            }

            // Data list
            if
            (
                forced ||
                script.maskDataListGPU == null ||
                script.maskDataListGPU.Count == 0 ||
                script.maskDataListGPU[0] == null ||
                script.maskDataListGPU.Count != script.GPULayers.Count
            )
            {
                script.maskDataListGPU = new List<TScatterLayer.MaskData[]>();

                if (script.GPULayers != null && script.GPULayers.Count > 0 && script.GPULayers[0] != null)
                    for (int i = 0; i < script.GPULayers.Count; i++)
                        script.maskDataListGPU.Add(script.GPULayers[i].maskData);
            }

            if
            (
                forced ||
                script.exclusionOpacitiesListGPU == null ||
                script.exclusionOpacitiesListGPU.Count == 0 ||
                script.exclusionOpacitiesListGPU[0] == null ||
                script.exclusionOpacitiesListGPU.Count != script.GPULayers.Count
            )
            {
                script.exclusionOpacitiesListGPU = new List<float[]>();
                int terrainLayersCount = script.Terrain.terrainData.terrainLayers.Length;

                if (script.GPULayers != null && script.GPULayers.Count > 0 && script.GPULayers[0] != null)
                    for (int i = 0; i < script.GPULayers.Count; i++)
                    {
                        if (script.GPULayers[i].exclusionOpacities == null || script.GPULayers[i].exclusionOpacities.Length == 0)
                        {
                            script.GPULayers[i].exclusionOpacities = new float[terrainLayersCount];

                            for (int j = 0; j < script.GPULayers[i].exclusionOpacities.Length; j++)
                                script.GPULayers[i].exclusionOpacities[j] = 1f;
                        }
                        else if (script.GPULayers[i].exclusionOpacities.Length < terrainLayersCount)
                        {
                            List<float> list = new List<float>(script.GPULayers[i].exclusionOpacities);

                            for (int j = script.GPULayers[i].exclusionOpacities.Length; j < terrainLayersCount; j++)
                                list.Add(1f);

                            script.GPULayers[i].exclusionOpacities = list.ToArray();
                        }
                        else if (script.GPULayers[i].exclusionOpacities.Length > terrainLayersCount)
                        {
                            List<float> list = new List<float>(script.GPULayers[i].exclusionOpacities);

                            for (int j = terrainLayersCount; j < script.GPULayers[i].exclusionOpacities.Length; j++)
                                list.Remove(script.GPULayers[i].exclusionOpacities[j]);

                            script.GPULayers[i].exclusionOpacities = list.ToArray();
                        }

                        script.exclusionOpacitiesListGPU.Add(script.GPULayers[i].exclusionOpacities);
                    }
            }

            if
            (
                forced ||
                script.maskDataListGrass == null ||
                script.maskDataListGrass.Count == 0 ||
                script.maskDataListGrass[0] == null ||
                script.maskDataListGrass.Count != script.grassLayers.Count
            )
            {
                script.maskDataListGrass = new List<TScatterLayer.MaskData[]>();

                foreach (GrassLayer p in script.grassLayers)
                    if (p != null && p.MGP != null && p.MGP.maskData != null)
                        script.maskDataListGrass.Add(p.MGP.maskData);
            }

            if
            (
                forced ||
                script.exclusionOpacitiesListGrass == null ||
                script.exclusionOpacitiesListGrass.Count == 0 ||
                script.exclusionOpacitiesListGrass[0] == null ||
                script.exclusionOpacitiesListGrass.Count != script.grassLayers.Count
            )
            {
                script.exclusionOpacitiesListGrass = new List<float[]>();
                int terrainLayersCount = script.Terrain.terrainData.terrainLayers.Length;

                foreach (GrassLayer p in script.grassLayers)
                    if (p != null && p.MGP != null && p.MGP.exclusionOpacities != null)
                    {
                        if (p.MGP.exclusionOpacities == null || p.MGP.exclusionOpacities.Length == 0)
                        {
                            p.MGP.exclusionOpacities = new float[terrainLayersCount];

                            for (int j = 0; j < p.MGP.exclusionOpacities.Length; j++)
                                p.MGP.exclusionOpacities[j] = 1f;
                        }
                        else if (p.MGP.exclusionOpacities.Length < terrainLayersCount)
                        {
                            List<float> list = new List<float>(p.MGP.exclusionOpacities);

                            for (int j = p.MGP.exclusionOpacities.Length; j < terrainLayersCount; j++)
                                list.Add(1f);

                            p.MGP.exclusionOpacities = list.ToArray();
                        }
                        else if (p.MGP.exclusionOpacities.Length > terrainLayersCount)
                        {
                            List<float> list = new List<float>(p.MGP.exclusionOpacities);

                            for (int j = terrainLayersCount; j < p.MGP.exclusionOpacities.Length; j++)
                                list.Remove(p.MGP.exclusionOpacities[j]);

                            p.MGP.exclusionOpacities = list.ToArray();
                        }

                        script.exclusionOpacitiesListGrass.Add(p.MGP.exclusionOpacities);
                    }
            }
        }

        public override void OnInspectorGUI()
        {
            if (script.Terrain == null) return;
            serializedObject.Update();

            if (refresh)
            {
                InitLists(true);
                refresh = false;
            }
            else
                InitLists();

            MaskEditorGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif

