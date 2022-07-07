using UnityEditor;

[InitializeOnLoad]
class WebGLThreads
{
    static WebGLThreads()
    {
#if UNITY_2019_1_OR_NEWER
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.threadsSupport = true;
        PlayerSettings.WebGL.memorySize = 2048;
#endif
    }
}

