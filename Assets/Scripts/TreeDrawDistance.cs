using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TreeDrawDistance : MonoBehaviour
{
    public float distance;
    public Terrain terrain;
    void Update()
    {
        terrain.treeDistance = distance;
    }
}
