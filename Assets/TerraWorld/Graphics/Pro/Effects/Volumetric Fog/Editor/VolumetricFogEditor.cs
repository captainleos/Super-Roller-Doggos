#if UNITY_STANDALONE_WIN
using UnityEditor;

namespace TerraUnity.Runtime
{
	[CustomEditor(typeof(VolumetricFog))]
	[CanEditMultipleObjects]
	public class VolumetricFogEditor : Editor
	{
		override public void OnInspectorGUI()
		{
			if (!VolumetricFog.CheckSupport())
			{
				EditorGUILayout.HelpBox(VolumetricFog.GetUnsupportedErrorMessage(), MessageType.Error);
				return;
			}

			DrawDefaultInspector();
		}
	}
}
#endif

