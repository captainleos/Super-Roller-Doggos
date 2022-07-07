/*
#if UNITY_EDITOR
#if !TERRAWORLD_XPRO
        using UnityEditor;
        namespace TerraUnity.Edittime
        {
            [InitializeOnLoad]
            public class xNodeModuleModifier
            {
                static xNodeModuleModifier()
                {
                    BuildTarget bt = EditorUserBuildSettings.activeBuildTarget;
                    BuildTargetGroup btg = BuildPipeline.GetBuildTargetGroup(bt);
                    string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
                    if (!defineSymbols.Contains("TERRAWORLD_XPRO")) defineSymbols = defineSymbols + ";TERRAWORLD_XPRO;";
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, defineSymbols);
                }
            }
        }
#endif
#endif
*/
