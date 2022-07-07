using UnityEngine;
using System.Collections.Generic;

namespace TerraUnity.Runtime
{
    public class WorldToolsParams : MonoBehaviour
    {
#if UNITY_EDITOR
        // Serializable Parameters
        [Range(1, 1000)] public int brushRadius = 30;
        [Range(1f, 100f)] public float brushDensity = 100f;
        [Range(0f, 100f)] public float[] exclusion;
        [Range(0f, 100f)] public float[] exclusionGPU;
        [Range(0f, 100f)] public float[] exclusionGrass;
        public bool isolateLayer = true;
        public int liveSyncState = 0;
        public int undoMode = 0;
        public bool hideAllLayers = false;
        //public bool hideAllGPU = false;
        //public bool hideAllGrass = false;
        public List<bool> cachedStatesGPU;
        public List<bool> cachedStatesGrass;
        public bool sectionToggle1 = false;
        public bool sectionToggle2 = false;
        public bool sectionToggleTexturesFiltering = true;
        public bool sectionToggleLayersFiltering = true;
        public bool sectionToggleSettings = true;
        //public TScatterLayer.MaskData[] maskData;
        public int layerMask = ~0;
        public bool includeGrassLayerFiltering = false;

        // Generic Parameters
        public static Event e;
        public static bool sectionToggleMaskEdit = true;
        public static int maskPreviewResolution = 256;
        public static bool isEditMode = false;
        public static bool painting = false;
        public static bool erasing = false;
        public static bool maskIsDirty = false;
        public static int isolatedIndexGPU = -1; // -1 means no isolated layers, -2 means no isolated layers in any other layer types
        public static int isolatedIndexGrass = -1; // -1 means no isolated layers, -2 means no isolated layers in any other layer types
        public static Rect lastRect;
        public static TerrainLayer[] terrainLayers;
        public static int dataWidthBrush = 2048;
        public static int dataHeightBrush = 2048;
        public static int brushPixels;
        public static float pixelSizeBrush;
        public static Vector3 centerPos;
        public static bool globalMode;
        public static bool isGPULayer;
        public static bool isGrassLayer;
        public static float lastEditorUpdateTime;
        public static bool applySyncing = false;
        public static float syncDelay = 1f;
        public static bool syncAllGPU = false;
        public static bool syncAllGrass = false;
        public static Color enabledColor = Color.white;
        public static Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.25f);
        public static Color editableColor = new Color(0.9f, 1f, 0.9f, 1);
        public static Color liveSyncColor = new Color(0.8f, 1f, 0.8f, 1);
        public static Color bypassColor = new Color(0.85f, 0.85f, 0.85f, 0.85f);
        public static string[] enableDisableSelection = new string[] { "ENABLE", "DISABLE" };
        //public static string[] effectiveSelection = new string[] { "EDITABLE", "LOCKED" };
        public static string[] onOffSelection = new string[] { "ON", "OFF" };
        public static string editingText = "Press  \"E\"  to Enter Edit Mode\nPress  \"Escape\"  to Exit Edit Mode\n\nHold  \"Left Click\"  to paint\nHold  \"Shift + Left Click\"  to erase\nHold  \"Shift + Scroll Wheel\"  to change brush size\nPress  \"Control + Z\"  &  \"Control + Y\"  to Undo/Redo";
        public static Texture2D maskImage;
        public static Texture2D maskImageTemp;

        // Layer Parameters
        public static List<Vector2> paintedPixels;
        public static List<Vector2>[] paintedPixelsGPU;
        public static List<Vector2>[] paintedPixelsGrass;

        //public Texture2D _mask;
        //public float maskPreviewResolution = 256;
        //public bool isPreviewMask = false;
#endif
    }
}

