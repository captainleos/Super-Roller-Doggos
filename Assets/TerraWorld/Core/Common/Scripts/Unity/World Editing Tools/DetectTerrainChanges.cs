#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using TerraUnity.UI;
using System;

namespace TerraUnity.Runtime
{
    [ExecuteInEditMode]
    public class DetectTerrainChanges : MonoBehaviour
    {
        public static bool liveSync = true;
        private const string terrainPaintingStr = "Terrain Paint - Texture";
        private Event e;
        private float lastEditorUpdateTime;
        private float syncDelay = 0.5f;
        private static Vector3 mousePosWorld;
        private Material terrainMaterial;


        // Painting Texture Events
        //-------------------------------------------------------------------------------------------------------

        private void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        private void OnDestroy()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
        }

        private void OnSceneGUI(SceneView scnView)
        {
            DetectPaintingChanges();
        }

        private void DetectPaintingChanges ()
        {
            e = Event.current;
            if (!liveSync) return;

            if (Undo.GetCurrentGroupName() == terrainPaintingStr)
            {
                if (e.isMouse && e.button == 0 && e.type == EventType.MouseUp)
                {
                    lastEditorUpdateTime = Time.realtimeSinceStartup;
                    EditorApplication.update += OnEditorUpdate;
                }
            }
        }

        private void OnEditorUpdate()
        {
            if (Time.realtimeSinceStartup - lastEditorUpdateTime > syncDelay)
            {
                Undo.SetCurrentGroupName("TerraWorld - Painting Detected");
                SyncLayersWithTerrain(e.mousePosition);
                EditorApplication.update -= OnEditorUpdate;
            }
        }


        // Painting Heightmap Events
        //-------------------------------------------------------------------------------------------------------

        void OnTerrainChanged(TerrainChangedFlags flags)
        {
            if (!liveSync) return;

            // Sync layers only when heightmap is painted and update painted regions
            if (e != null && flags != 0 && flags != TerrainChangedFlags.Heightmap)
                if (TerrainChangedFlags.DelayedHeightmapUpdate != 0 | TerrainChangedFlags.FlushEverythingImmediately != 0)
                    SyncLayersWithTerrain(e.mousePosition);

            // Sync layers everywhere when terrain heightmap is updated
            if (flags != 0)
                if (flags == TerrainChangedFlags.Heightmap)
                    SyncLayersWithTerrain();
        }


        // Sync TerraWorld Layers with Terrain Changes
        //-------------------------------------------------------------------------------------------------------

        public static void SyncLayerWithTerrain(Vector2 mousePosition, TScatterLayer.MaskData[] filter, int progressID = -1)
        {
            if (Application.isPlaying) return;

            try
            {
                if (!Raycasts.GetMouseWorldPosition(mousePosition, out mousePosWorld)) return;

                // Update GPU Instance Layers
                TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>();

                if (GPULayers != null && GPULayers.Length > 0 && GPULayers[0] != null)
                {
                    //TODO: Detect number of neighbor cells according to brush size
                    int neighborCells = 1;
                    int index, row, col;
                    int indexStart = -neighborCells;
                    int indexEnd = neighborCells + 1;

                    foreach (TScatterParams g in GPULayers)
                    {
                        if (g.maskData != filter) continue;
                        List<int> patchIndices = new List<int>();

                        if (g.GetPatchesRowCol(new Vector2(mousePosWorld.x, mousePosWorld.z), out index, out row, out col))
                            for (int i = indexStart; i < indexEnd; i++)
                                for (int j = indexStart; j < indexEnd; j++)
                                    if (g.GetPatchesIndex(row + i, col + j, out index))
                                        patchIndices.Add(index);

                        g.SyncLayer(patchIndices, false);
                    }
                }

                // Update Grass Layers
                List<GrassLayer> GrassLayers = new List<GrassLayer>();

                foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                    if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                        if (go.GetComponent<GrassLayer>() != null)
                            GrassLayers.Add(go.GetComponent<GrassLayer>());

                if (GrassLayers != null && GrassLayers.Count > 0 && GrassLayers[0] != null)
                {
                    for (int i = 0; i < GrassLayers.Count; i++)
                    {
                        if (GrassLayers[i].MGP.maskData != filter) continue;
                        GrassLayers[i].UpdateLayer();
                    }
                }

                //MassiveGrass[] GrassLayers = FindObjectsOfType<MassiveGrass>();
                //
                //if (GrassLayers != null && GrassLayers.Length > 0 && GrassLayers[0] != null)
                //    foreach (MassiveGrass g in GrassLayers)
                //        foreach (MassiveGrassProfile p in g.profiles)
                //        {
                //            if (p.maskData != filter) continue;
                //            g.Refresh();
                //        }

                SceneView.RepaintAll();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
#if TERRAWORLD_DEBUG
                catch (Exception e)
                {
                    throw e;
                }
#else
            catch { }
#endif

            TProgressBar.RemoveProgressBar(progressID);
        }

        public static void SyncLayersWithTerrain (Vector2 mousePosition, bool affectGPULayers = true, bool affectGrassLayers = true, List<TScatterLayer.MaskData[]> filters = null, int progressID = -1)
        {
            if (Application.isPlaying) return;
            if (!affectGPULayers && !affectGrassLayers) return;

            try
            {
                if (!Raycasts.GetMouseWorldPosition(mousePosition, out mousePosWorld)) return;

                // Update GPU Instance Layers
                if (affectGPULayers)
                {
                    TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>();

                    if (GPULayers != null && GPULayers.Length > 0 && GPULayers[0] != null)
                    {
                        //TODO: Detect number of neighbor cells according to affected pixels in current filter mask
                        int neighborCells = 1;
                        int index, row, col;
                        int indexStart = -neighborCells;
                        int indexEnd = neighborCells + 1;

                        foreach (TScatterParams g in GPULayers)
                        {
                            if (filters != null && !filters.Contains(g.maskData)) continue;
                            List<int> patchIndices = new List<int>();

                            if (g.GetPatchesRowCol(new Vector2(mousePosWorld.x, mousePosWorld.z), out index, out row, out col))
                                for (int i = indexStart; i < indexEnd; i++)
                                    for (int j = indexStart; j < indexEnd; j++)
                                        if (g.GetPatchesIndex((row + i), (col + j), out index))
                                            patchIndices.Add(index);

                            g.SyncLayer(patchIndices, false);
                        }
                    }
                }

                if (affectGrassLayers)
                {
                    // Update Grass Layers
                    List<GrassLayer> GrassLayers = new List<GrassLayer>();

                    foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                        if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                            if (go.GetComponent<GrassLayer>() != null)
                                GrassLayers.Add(go.GetComponent<GrassLayer>());

                    if (GrassLayers != null && GrassLayers.Count > 0 && GrassLayers[0] != null)
                    {
                        for (int i = 0; i < GrassLayers.Count; i++)
                        {
                            if (filters != null)
                            {
                                if (filters.Contains(GrassLayers[i].MGP.maskData))
                                {
                                    GrassLayers[i].UpdateLayer();
                                    break;
                                }
                            }
                            else
                                GrassLayers[i].UpdateLayer();
                        }
                    }

                    //MassiveGrass[] GrassLayers = FindObjectsOfType<MassiveGrass>();
                    //
                    //if (GrassLayers != null && GrassLayers.Length > 0 && GrassLayers[0] != null)
                    //    foreach (MassiveGrass g in GrassLayers)
                    //    {
                    //        if (filters != null)
                    //        {
                    //            foreach (MassiveGrassProfile p in g.profiles)
                    //                if (filters.Contains(p.maskData))
                    //                {
                    //                    g.Refresh();
                    //                    break;
                    //                }
                    //        }
                    //        else
                    //            g.Refresh();
                    //    }
                }

                SceneView.RepaintAll();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
#if TERRAWORLD_DEBUG
                catch (Exception e)
                {
                    throw e;
                }
#else
            catch { }
#endif

            TProgressBar.RemoveProgressBar(progressID);
        }

        public static void SyncLayersWithTerrain (bool syncGPU = true, bool syncGrass = true, List<TScatterLayer.MaskData[]> filters = null, int progressID = -1)
        {
            if (Application.isPlaying) return;
            if (!syncGPU && !syncGrass) return;

            // Update GPU Instance Layers
            if (syncGPU)
            {
                TScatterParams[] GPULayers = FindObjectsOfType<TScatterParams>();

                if (GPULayers != null && GPULayers.Length > 0 && GPULayers[0] != null)
                {
                    foreach (TScatterParams g in GPULayers)
                    {
                        if (filters != null && !filters.Contains(g.maskData)) continue;
                        g.UpdateLayer();
                    }
                }
            }

            // Update Grass Layers
            if (syncGrass)
            {
                List<GrassLayer> GrassLayers = new List<GrassLayer>();

                foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
                    if (go != null && go.hideFlags != HideFlags.NotEditable && go.hideFlags != HideFlags.HideAndDontSave && go.scene.IsValid())
                        if (go.GetComponent<GrassLayer>() != null)
                            GrassLayers.Add(go.GetComponent<GrassLayer>());

                if (GrassLayers != null && GrassLayers.Count > 0 && GrassLayers[0] != null)
                {
                    for (int i = 0; i < GrassLayers.Count; i++)
                    {
                        if (filters != null)
                        {
                            if (filters.Contains(GrassLayers[i].MGP.maskData))
                            {
                                GrassLayers[i].UpdateLayer();
                                break;
                            }
                        }
                        else
                            GrassLayers[i].UpdateLayer();
                    }
                }

                //MassiveGrass[] GrassLayers = FindObjectsOfType<MassiveGrass>();
                //
                //if (GrassLayers != null && GrassLayers.Length > 0 && GrassLayers[0] != null)
                //    foreach (MassiveGrass g in GrassLayers)
                //    {
                //        if (filters != null)
                //        {
                //            foreach (MassiveGrassProfile p in g.profiles)
                //                if (filters.Contains(p.maskData))
                //                {
                //                    g.Refresh();
                //                    break;
                //                }
                //        }
                //        else
                //            g.Refresh();
                //    }
            }

            SceneView.RepaintAll();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            TProgressBar.RemoveProgressBar(progressID);
        }
    }
}
#endif

