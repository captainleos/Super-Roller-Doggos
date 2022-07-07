using UnityEngine;

namespace TerraUnity.Runtime
{
	[RequireComponent(typeof(Camera)), ExecuteAlways, ImageEffectAllowedInSceneView]
	public class Crepuscular : MonoBehaviour
	{
		public Material material;
		public GameObject sun;

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			//if (material == null) material = TResourcesManager.godRaysMaterial;
			//if (sun == null) sun = RenderSettings.sun.gameObject;
			if (material == null || sun == null) return;

			material.SetVector("_LightPos", GetComponent<Camera>().WorldToViewportPoint(transform.position - sun.transform.forward));
			Graphics.Blit(source, destination, material);
		}
	}
}

