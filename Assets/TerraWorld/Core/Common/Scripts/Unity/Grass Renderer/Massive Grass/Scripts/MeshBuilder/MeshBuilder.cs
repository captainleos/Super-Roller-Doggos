using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Mewlist.MassiveGrass
{
    public class MeshBuilder
    {
        private IMeshBuilder builder;

        private async Task<Element[]> GenerateElements(Terrain terrain, Rect rect, MassiveGrassProfile profile,
            int haltonOffset)
        {
            var context = SynchronizationContext.Current;
            var terrainPos = terrain.transform.position;
            var terrainSize = terrain.terrainData.size.x;
            var terrainXZPos = new Vector2(terrainPos.x, terrainPos.z);
            var localRect = new Rect(rect.min - terrainXZPos, rect.size);
            var localNormalizedRect = new Rect(localRect.position / terrainSize, localRect.size / terrainSize);

            var haltons = new Vector2[profile.AmountPerBlock];
            var normalizedPositions = new Vector2[profile.AmountPerBlock];
            var heights = new float[profile.AmountPerBlock];
            var normals = new Vector3[profile.AmountPerBlock];
            //            var list = new List<Element>(profile.AmountPerBlock);
            var list = new Element[profile.AmountPerBlock];

            var done = false;
            var range = Enumerable.Range(0, profile.AmountPerBlock);
            for (var i = 0; i < list.Length; i++)
                list[i] = default(Element);

            await Task.Run(() =>
            {
                for (var i = 0; i < profile.AmountPerBlock; i++)
                {
                    haltons[i] = new Vector2(
                        HaltonSequence.Base2(i + haltonOffset + profile.Seed),
                        HaltonSequence.Base3(i + haltonOffset + profile.Seed));
                    normalizedPositions[i] = localNormalizedRect.min + haltons[i] * localNormalizedRect.size;
                }
            });

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            await Task.Run(async () =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                context.Post(_ =>
                {
                    // TW_Tweaks
                    if (normalizedPositions.Length != profile.AmountPerBlock) return;
                    // TW_Tweaks

                    for (var i = 0; i < profile.AmountPerBlock; i++)
                    {
                        if (terrain == null) break;
                        Vector2 normalizedPosition = normalizedPositions[i];

                        // TW_Tweaks
                        if (profile.layerBasedPlacement)
                        {
                            Ray ray = new Ray(new Vector3((normalizedPosition.x * terrainSize) - (terrainSize / 2f), 100000, (normalizedPosition.y * terrainSize) - (terrainSize / 2f)), Vector3.down);
                            RaycastHit hit;

                            if (TerraUnity.Runtime.Raycasts.RaycastNonAllocSorted(ray, profile.bypassWater, profile.underWater, out hit, profile.unityLayerMask))
                            {
                                heights[i] = hit.point.y;
                                //normals[i] = Quaternion.FromToRotation(Vector3.up, hit.normal + Vector3.zero).eulerAngles;

                                //heights[i] = terrain.terrainData.GetInterpolatedHeight(normalizedPosition.x, normalizedPosition.y) + terrain.transform.position.y;
                                normals[i] = terrain.terrainData.GetInterpolatedNormal(normalizedPosition.x, normalizedPosition.y);
                            }
                            else
                            {
                                heights[i] = -10000;
                                normals[i] = Vector3.one;
                            }
                        }
                        else
                        {
                            heights[i] = terrain.terrainData.GetInterpolatedHeight(normalizedPosition.x, normalizedPosition.y) + terrain.transform.position.y;
                            normals[i] = terrain.terrainData.GetInterpolatedNormal(normalizedPosition.x, normalizedPosition.y);
                        }
                        // TW_Tweaks
                    }

                    done = true;
                }, null);
            });

            while (!done) await Task.Delay(1);

            await Task.Run(() =>
            {
                for (var i = 0; i < profile.AmountPerBlock; i++)
                {
                    var haltonPos = haltons[i];
                    var position = haltonPos * rect.size + rect.min;
                    var normalizedPosition = localNormalizedRect.min + haltons[i] * localNormalizedRect.size;
                    list[i] = new Element(
                        i,
                        new Vector3(position.x, heights[i], position.y),
                        normalizedPosition,
                        normals[i]);
                }
            });

            return list;
        }

        public async Task<MeshData> BuildMeshData(Terrain terrain, IReadOnlyCollection<Texture2D> alphaMaps, MassiveGrassGrid.CellIndex index, Rect rect, MassiveGrassProfile profile)
        {
            var elements = await GenerateElements(terrain, rect, profile, index.hash % 50000);
            builder = builder ?? profile.CreateBuilder();
            return await builder.BuildMeshData(terrain, alphaMaps, profile, elements);
        }

        public void BuildMesh(Mesh mesh, MeshData meshData)
        {
            builder.BuildMesh(mesh, meshData);
        }
    }
}

