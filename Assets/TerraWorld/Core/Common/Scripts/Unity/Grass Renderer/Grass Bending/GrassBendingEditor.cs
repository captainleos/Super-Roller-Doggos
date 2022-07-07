using UnityEngine;
using GrassBending;

[ExecuteAlways]
public class GrassBendingEditor : MonoBehaviour
{
    private void Update ()
    {
        if (!Application.isPlaying)
            GrassBendingManager.ProcessBenders();
    }
}

