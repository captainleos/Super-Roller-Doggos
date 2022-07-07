#if UNITY_EDITOR
    #if TW_TEMPLATES
    #else
        using UnityEditor;
        namespace TerraUnity.Edittime
        {
            [InitializeOnLoad]
            public class TemplatesModuleModifier
            {
                static TemplatesModuleModifier()
                {
                    BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
                    BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
                    string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
                    if (!defineSymbols.Contains("TW_TEMPLATES")) defineSymbols = defineSymbols + ";TW_TEMPLATES;";
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols);
                }
            }
        }
    #endif
#endif

