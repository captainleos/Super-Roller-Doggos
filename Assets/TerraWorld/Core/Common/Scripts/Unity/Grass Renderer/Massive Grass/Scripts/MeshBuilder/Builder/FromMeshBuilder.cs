using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Mewlist.MassiveGrass
{
    public class FromMeshBuilder : IMeshBuilder
    {
        private readonly Stack<MeshData> pool = new Stack<MeshData>();
        private MeshTemplateData templateData;

        public void BuildMesh(Mesh mesh, MeshData meshData)
        {
            //var vertices  = new Vector3[meshData.VertexCount];
            //var normals   = new Vector3[meshData.VertexCount];
            //var triangles = new int[meshData.IndexCount];
            //var colors    = new Color[meshData.VertexCount];
            //var uvs       = new Vector4[meshData.VertexCount];
            //
            //for (var c = 0; c < meshData.VertexCount; c++) vertices[c] = meshData.vertices[c];
            //for (var c = 0; c < meshData.VertexCount; c++) normals[c]  = meshData.normals[c];
            //for (var c = 0; c < meshData.IndexCount; c++) triangles[c] = meshData.triangles[c];
            //for (var c = 0; c < meshData.VertexCount; c++) colors[c]   = meshData.colors[c];
            //for (var c = 0; c < meshData.VertexCount; c++) uvs[c]      = meshData.uvs[c];
            //
            //mesh.Clear();
            //mesh.name = "MassiveGrass Template Mesh";
            //
            //mesh.SetVertices( vertices, 0, meshData.VertexCount);
            //mesh.SetUVs(0, uvs, 0, meshData.VertexCount);
            //mesh.SetNormals(normals, 0, meshData.VertexCount);
            //mesh.SetTriangles(triangles, 0, meshData.IndexCount, 0);
            //mesh.SetColors(colors, 0, meshData.VertexCount);

// TW Tweaks
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> colors = new List<Color>();
            List<Vector4> uvs = new List<Vector4>();
            List<Vector4> uv4s = new List<Vector4>();

            for (var c = 0; c < meshData.VertexCount; c++) vertices.Add(meshData.vertices[c]);
            for (var c = 0; c < meshData.VertexCount; c++) normals.Add(meshData.normals[c]);
            for (var c = 0; c < meshData.IndexCount; c++) triangles.Add(meshData.triangles[c]);
            for (var c = 0; c < meshData.VertexCount; c++) colors.Add(meshData.colors[c]);
            for (var c = 0; c < meshData.VertexCount; c++) uvs.Add(meshData.uvs[c]);
            for (var c = 0; c < meshData.VertexCount; c++) uv4s.Add(meshData.uv4s[c]);

            mesh.Clear();
            mesh.name = "MassiveGrass Template Mesh";

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetUVs(3, uv4s);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.SetColors(colors);

            //Vector2[] uvs = new Vector2[meshData.vertices.Length];
            //for (int c = 0; c < meshData.VertexCount; c++) uvs[c] = meshData.uvs[c];
            //
            //mesh.Clear();
            //mesh.name = "MassiveGrass Template Mesh";
            //
            //mesh.vertices = meshData.vertices;
            //mesh.uv = uvs;
            //mesh.normals = meshData.normals;
            //mesh.triangles = meshData.triangles;
            //mesh.colors = meshData.colors;
// TW Tweaks

            pool.Push(meshData);
        }

        public async Task<MeshData> BuildMeshData(Terrain terrain, IReadOnlyCollection<Texture2D> alphaMaps,
            MassiveGrassProfile profile, Element[] elements)
        {
            if (templateData == null)
            {
                var scale = new Vector3(profile.Scale.x, profile.Scale.y, profile.Scale.x);
                templateData = new MeshTemplateData(profile.Mesh, scale);
            }

            if (pool.Count <= 0)
                pool.Push(new MeshData(profile.AmountPerBlock, templateData.vertexCount, templateData.indecesCount));

            var terrainData = terrain.terrainData;
            var w           = terrainData.alphamapWidth;
            var h           = terrainData.alphamapHeight;

            var meshData = pool.Pop();
            var actualCount = 0;
            var alphas = new float[elements.Length];

// TW Tweaks
            //var layers = new List<int>(profile.TerrainLayers.Length);
            //
            //foreach (var terrainLayer in profile.TerrainLayers)
            //    for (var i = 0; i < terrainData.terrainLayers.Length; i++)
            //        if (terrainData.terrainLayers[i].name == terrainLayer.name)
            //            layers.Add(i);

            for (var i = 0; i < elements.Length; i++)
            {
                var element  = elements[i];
                var alpha    = 0f;

                //Ray ray = new Ray(element.position + (Vector3.up * 100000), Vector3.down);
                //if (!Raycasts.RaycastNonAllocSorted(ray, profile.bypassWater, profile.underWater, profile.unityLayerMask)) continue;

                bool excludedPixel = false;

                for (int k = 0; k < profile.exclusionOpacities.Length; k++)
                {
                    if (profile.exclusionOpacities[k] == 1) continue;
                    int pixelX = Mathf.Clamp((int)(element.normalizedPosition.x * w), 0, w - 1);
                    int pixelY = Mathf.Clamp((int)(element.normalizedPosition.y * h), 0, h - 1);
                    float[,,] alphamap = terrain.terrainData.GetAlphamaps(pixelX, pixelY, 1, 1);

                    if (alphamap[0, 0, k] > profile.exclusionOpacities[k])
                    {
                        excludedPixel = true;
                        break;
                    }
                }

                if (!excludedPixel)
                {
                    //Color pixel = profile.mask.GetPixelBilinear(
                    //            element.normalizedPosition.x,
                    //            element.normalizedPosition.y);
                    //
                    ////alpha = Mathf.Max(alpha, pixel.a);
                    //if (pixel.a > profile.AlphaMapThreshold)
                    //    alpha = pixel.a;

                    int maskPosX = (int)(element.normalizedPosition.x * profile.maskData.Length);
                    int maskPosY = (int)(element.normalizedPosition.y * profile.maskData[0].row.Length);

                    // Neighbor cells to check for masking falloff
                    int neighbors = Mathf.CeilToInt(Mathf.InverseLerp(32f, 4096f, w) * 10f) - Mathf.FloorToInt(w / 1000f);

                    if (profile.maskData[maskPosX].row[maskPosY] > profile.AlphaMapThreshold)
                        alpha = profile.maskData[maskPosX].row[maskPosY];
                    else if (neighbors > 0)
                    {
                        float sum = 0;

                        for (int j = 1; j <= neighbors; j++)
                        {
                            int nextCol = maskPosX + j; if (nextCol > profile.maskData.Length - 1) nextCol = profile.maskData.Length - 1;
                            int prevCol = maskPosX - j; if (prevCol < 0) prevCol = 0;
                            int nextRow = maskPosY + j; if (nextRow > profile.maskData[0].row.Length - 1) nextRow = profile.maskData[0].row.Length - 1;
                            int prevRow = maskPosY - j; if (prevRow < 0) prevRow = 0;
                            float topLeft = profile.maskData[prevCol].row[prevRow];
                            float top = profile.maskData[prevCol].row[maskPosY];
                            float topRight = profile.maskData[prevCol].row[nextRow];
                            float left = profile.maskData[maskPosX].row[prevRow];
                            float right = profile.maskData[maskPosX].row[nextRow];
                            float bottomLeft = profile.maskData[nextCol].row[prevRow];
                            float bottom = profile.maskData[nextCol].row[maskPosY];
                            float bottomRight = profile.maskData[nextCol].row[nextRow];
                            sum += topLeft + top + topRight + left + right + bottomLeft + bottom + bottomRight;
                        }

                        if (sum >= 0)
                        {
                            System.Random random = new System.Random(profile.Seed);
                            alpha = (sum * 0.05f) + (float)random.NextDouble() * 0.65f;
                        }
                    }

                    //foreach (var layer in layers)
                    //{
                    //    var texIndex = layer / 4;
                    //    if (texIndex < alphaMaps.Count)
                    //    {
                    //        var pixel = alphaMaps.ElementAt(layer / 4).GetPixelBilinear(
                    //            element.normalizedPosition.x * (w - 1f) / w,
                    //            element.normalizedPosition.y * (h - 1f) / h);
                    //        switch (layer % 4)
                    //        {
                    //            case 0:
                    //                alpha = Mathf.Max(alpha, pixel.r);
                    //                break;
                    //            case 1:
                    //                alpha = Mathf.Max(alpha, pixel.g);
                    //                break;
                    //            case 2:
                    //                alpha = Mathf.Max(alpha, pixel.b);
                    //                break;
                    //            case 3:
                    //                alpha = Mathf.Max(alpha, pixel.a);
                    //                break;
                    //        }
                    //    }
                    //}
                }

                alphas[i] = alpha;
            }
// TW Tweaks

            await Task.Run(() =>
            {
                for (var i = 0; i < elements.Length; i++)
                {
                    var element  = elements[i];

                    var validHeight = profile.HeightRange.x <= element.position.y &&
                                      element.position.y <= profile.HeightRange.y;

                    if (!validHeight) continue;

                    var alpha = alphas[i];

// TW Tweaks
                    if (profile.DensityFactor > 0)
                    {
                        var rand = 1f - Mathf.Repeat(1f, ParkAndMiller.Get(i));

                        if (alpha > (1f - profile.DensityFactor) * rand * rand)
                            Add(meshData, profile, element, alpha, actualCount++);
                    }
                    else if (alpha > profile.DensityFactor)
                        Add(meshData, profile, element, alpha, actualCount++);

                    //if (alpha >= profile.AlphaMapThreshold)
                    //    Add(meshData, profile, element, alpha, actualCount++);
                    //else
                    //{
                    //    var rand = 1f - Mathf.Repeat(1f, ParkAndMiller.Get(i));
                    //
                    //    if (alpha > (1f - profile.DensityFactor) * rand * rand)
                    //        Add(meshData, profile, element, alpha, actualCount++);
                    //}
// TW Tweaks
                }

                meshData.SetActualCount(
                    templateData.vertexCount * actualCount,
                    templateData.indecesCount * actualCount);
            });

            return meshData;
        }

        private void Add(MeshData meshData, MassiveGrassProfile profile, Element element, float density, int index)
        {
            var vOrigin = index * templateData.vertexCount;
            var iOrigin = index * templateData.indecesCount;
            var rand    = ParkAndMiller.Get(element.index + 1000);

            var normalRot = Quaternion.LookRotation(element.normal);
            var slant     = Quaternion.AngleAxis(profile.Slant * 90f * (rand - 0.5f), Vector3.right);
            var slantWeak = Quaternion.AngleAxis(profile.Slant * 45f * (rand - 0.5f), Vector3.right);
            var upRot = Quaternion.AngleAxis(360f * rand, Vector3.up);
            var idealRot = normalRot *
                           Quaternion.AngleAxis(90f, Vector3.right) *
                           upRot;
            var rot       = idealRot * slant;

//            var scale = profile.Scale * (1 + 0.4f * (rand - 0.5f));
            var scale = Vector3.one * (1 + 0.4f * (rand - 0.5f));
            var rightVec = element.index % 2 == 0 ? rot * Vector3.right : rot * -Vector3.right;
            var upVec = rot * Vector3.up;

            for (var i = 0; i < templateData.vertexCount; i++)
            {
                var vertPos = element.position + Vector3.up * profile.GroundOffset +
                              rot * Vector3.Scale(templateData.scaledVertices[i], scale);
                meshData.vertices[vOrigin + i] = vertPos;
                switch (profile.NormalType)
                {
                    case NormalType.Up:
                        meshData.normals[vOrigin + i] = idealRot * Vector3.up;
                        break;
                    default:
                        meshData.normals[vOrigin + i] = templateData.normals[i];
                        break;
                }

                var uv = templateData.uvs[i];

                var attr = new MassiveGrassProfile.VertAttribute(density, rand, element.position, templateData.vertices[i]);
                var vColorR = profile.GetCustomVertexData(VertexDataType.VertexColorR, attr);
                var vColorG = profile.GetCustomVertexData(VertexDataType.VertexColorG, attr);
                var vColorB = profile.GetCustomVertexData(VertexDataType.VertexColorB, attr);
                var vColorA = profile.GetCustomVertexData(VertexDataType.VertexColorA, attr);
                var uv1Z    = profile.GetCustomVertexData(VertexDataType.UV1Z, attr);
                var uv1W    = profile.GetCustomVertexData(VertexDataType.UV1W, attr);
                var color = new Color(vColorR, vColorG, vColorB, vColorA);

                meshData.uvs[vOrigin + i] = new Vector4(uv.x, uv.y, uv1Z, uv1W);

                // meshData.colors[vOrigin + i] = color;
                // Keep mesh vertex color
                if (templateData.colors.Count <= i) // mesh does not have vertex color
                    meshData.colors[vOrigin + i] = new Color(1f, 1f, 1f, 1f);
                else
                    meshData.colors[vOrigin + i] = templateData.colors[i];

                // Write uv4 to mesh
                if (templateData.uv4s.Count <= i) // mesh does not have vertex color
                    meshData.uv4s[vOrigin + i] = Vector4.zero;
                else
                    meshData.uv4s[vOrigin + i] = templateData.uv4s[i];
            }

            for (var i = 0; i < templateData.indecesCount; i++)
            {
                var vi = vOrigin + templateData.triangles[i];
                if (vi >= vOrigin + templateData.vertexCount)
                {
                    //var a = 100;
                }

                meshData.triangles[iOrigin + i] = vOrigin + templateData.triangles[i];
            }
        }

        private class MeshTemplateData
        {
            public readonly int indecesCount;

            public readonly int vertexCount;
            public readonly List<Color> colors = new List<Color>();
            public readonly List<Vector3> normals = new List<Vector3>();
            public readonly List<Vector3> scaledVertices = new List<Vector3>();
            public readonly List<int> triangles = new List<int>();
            public readonly List<Vector4> uvs = new List<Vector4>();
            public readonly List<Vector4> uv4s = new List<Vector4>();
            public readonly List<Vector3> vertices = new List<Vector3>();

            public MeshTemplateData(Mesh mesh, Vector3 scale)
            {
                mesh.GetVertices(vertices);
                mesh.GetNormals(normals);
                mesh.GetUVs(0, uvs);
                mesh.GetUVs(3, uv4s);
                mesh.GetColors(colors);
                mesh.GetTriangles(triangles, 0);
                vertexCount = vertices.Count;
                indecesCount = triangles.Count;

                scaledVertices = vertices.Select(x => Vector3.Scale(x, scale)).ToList();
            }
        }
    }
}