using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public class TerrainActions : MonoBehaviour
    {
        private Terrain terrain;
        private int xResolution;
        private int zResolution;
        private Vector3 terrainSize;

        // For Debugging
        private Bounds objectBounds;

        void Start()
        {
            terrain = Terrain.activeTerrain;
            xResolution = terrain.terrainData.heightmapResolution;
            zResolution = terrain.terrainData.heightmapResolution;
            terrainSize = terrain.terrainData.size;
        }

        public float GetHeightDiff(GameObject gameObject)
        {
            // Need y to be lowest bounds of object
            Vector3 min = gameObject.GetComponent<Renderer>().bounds.min;
            return min.y - terrain.SampleHeight(min);
        }

        // Moves the terrain height 
        public void ModifyTerrainHeight(GameObject gameObject, float heightChange)
        {
            // Need pos to be smallest corner of object
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                Debug.LogError("GameObject has no renderer! " + gameObject.name);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            // Map the newGameObject to terrain heightmap coordinates
            Terrain terrain = Terrain.activeTerrain;
            Vector3 objectTerrainPos = GetRelativeTerrainPositionFromPos(bounds.min);
            Vector3Int objectSizeInTUs = GetTUsFromWUs(bounds.size);

            // Height will be a float between 0-1 relative to the total height of the terrain
            float newHeight = (terrain.SampleHeight(bounds.min) + heightChange - terrain.GetPosition().y) / terrainSize.y;

            // Change terrain to match the cube
            float[,] heights = terrain.terrainData.GetHeights((int)objectTerrainPos.x - 1, (int)objectTerrainPos.z - 1, objectSizeInTUs.x + 3, objectSizeInTUs.z + 3);

            // Change heights to be the same height as the newGameObject - uses the max height
            for (int i = 0; i < heights.GetLength(0); i++)
            {
                for (int j = 0; j < heights.GetLength(1); j++)
                {
                    if (i == 0 || j == 0 || i == heights.GetLength(0) - 1 || j == heights.GetLength(1) - 1)
                        heights[i, j] = newHeight - (newHeight - heights[i, j]) / 2;
                    else
                        heights[i, j] = newHeight;
                }
            }

            terrain.terrainData.SetHeights((int)objectTerrainPos.x - 1, (int)objectTerrainPos.z - 1, heights);

            // Fix nav mesh
            TotalAIManager.manager.UpdateAllNavMeshes();

        }

        public float MaxSlope(Bounds bounds)
        {
            Vector3 normalizedGhostMin = GetNormalizedPositionRelativeToTerrain(bounds.min);
            Vector3 normalizedGhostMax = GetNormalizedPositionRelativeToTerrain(bounds.max);

            float maxSlope = 0f;
            for (int i = 1; i <= 5; i++)
            {
                float x = normalizedGhostMin.x + (normalizedGhostMax.x - normalizedGhostMin.x) * (i / 6f);
                for (int j = 1; j <= 5; j++)
                {
                    float z = normalizedGhostMin.z + (normalizedGhostMax.z - normalizedGhostMin.z) * (j / 6f);
                    float slope = terrain.terrainData.GetSteepness(x, z);
                    //Debug.Log("Slope at (" + x + "," + z + ") = " + slope);
                    if (slope > maxSlope)
                        maxSlope = slope;
                }
            }
            
            return maxSlope;
        }

        public void MoveObjectAboveTerrain(GameObject gameObject)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                Debug.LogError("GameObject has no renderer! " + gameObject.name);

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            float[] heightDiffs = GetHeightDiffs(bounds);

            // For debugging
            objectBounds = bounds;

            // Want new min height diff to be a small positive number
            float heightAdjustment = .025f - heightDiffs[0];
            //Debug.Log("Min Diff = " + heightDiffs[0] + " - Height Adjustment = " + heightAdjustment);
            gameObject.transform.Translate(0, heightAdjustment, 0);
        }

        public float[] GetHeightDiffs(Bounds bounds)
        {
            // For debugging
            objectBounds = bounds;

            // TODO: Do a grid check with number of checks depending on footprint size of object
            float heightDiffAtCenter = bounds.min.y - terrain.SampleHeight(bounds.center);
            float heightDiffAtMin = bounds.min.y - terrain.SampleHeight(bounds.min);
            float heightDiffAtMax = bounds.min.y - terrain.SampleHeight(bounds.max);
            float heightDiffAtMinXMaxZ = bounds.min.y - terrain.SampleHeight(new Vector3(bounds.min.x, 0, bounds.max.z));
            float heightDiffAtMaxXMinZ = bounds.min.y - terrain.SampleHeight(new Vector3(bounds.max.x, 0, bounds.min.z));

            //Debug.Log("C = " + heightDiffAtCenter + " Min = " + heightDiffAtMin + " - Max = " + heightDiffAtMax + " MinXMaxZ = " + heightDiffAtMinXMaxZ + " - MaxXMinZ = " + heightDiffAtMaxXMinZ);

            float minDiff = Mathf.Min(new float[] { heightDiffAtCenter, heightDiffAtMin, heightDiffAtMax, heightDiffAtMinXMaxZ, heightDiffAtMaxXMinZ });
            float maxDiff = Mathf.Max(new float[] { heightDiffAtCenter, heightDiffAtMin, heightDiffAtMax, heightDiffAtMinXMaxZ, heightDiffAtMaxXMinZ });

            return new float[] { minDiff, maxDiff };
        }


        // Given a point returns the center bottom spot on the building grid
        public Vector3 FindGridLocation(Vector3 point, float height)
        {
            Vector3 terrainPosition = terrain.GetPosition();

            // Find point in heightmap coordinates
            int heightMapX = (int)((point.x - terrainPosition.x) / terrainSize.x * xResolution);
            int heightMapZ = (int)((point.z - terrainPosition.z) / terrainSize.z * zResolution);

            float worldX = terrainSize.x * (heightMapX / (float) xResolution) + terrainPosition.x;
            float worldZ = terrainSize.z * (heightMapZ / (float) zResolution) + terrainPosition.z;

            return new Vector3(worldX, height, worldZ);
        }

        private float FindGridHeight(float worldHeight)
        {
            Vector3 terrainPosition = terrain.GetPosition();

            int heightMapY = (int)((worldHeight - terrainPosition.y) / terrainSize.y * terrain.terrainData.heightmapResolution);
            return terrainSize.y * (heightMapY / (float) terrain.terrainData.heightmapResolution) + terrainPosition.y;
        }

        private Vector3 GetNormalizedPositionRelativeToTerrain(Vector3 pos)
        {
            Vector3 tempCoord = (pos - terrain.gameObject.transform.position);
            Vector3 coord;
            coord.x = tempCoord.x / terrainSize.x;
            coord.y = tempCoord.y / terrainSize.y;
            coord.z = tempCoord.z / terrainSize.z;
            return coord;
        }

        private Vector3 GetRelativeTerrainPositionFromPos(Vector3 pos)
        {
            Vector3 coord = GetNormalizedPositionRelativeToTerrain(pos);
            return new Vector3((coord.x * xResolution), 0, (coord.z * zResolution));
        }

        // Pass in a length in the x axis of the world - returns the length of that in heightmap width
        private int GetRelativeTerrainWidthFromWorldX(float worldX)
        {
            return (int)(worldX / terrainSize.x * xResolution);
        }

        // Pass in a length in the z axis of the world - returns the length of that in heightmap height
        private int GetRelativeTerrainHeightFromWorldZ(float worldZ)
        {
            return (int)(worldZ / terrainSize.z * zResolution);
        }

        // Pass in a world size (i.e. Bounds.size) and get back the size in Terrain Units
        private Vector3Int GetTUsFromWUs(Vector3 worldSize)
        {
            return new Vector3Int((int)(worldSize.x / terrainSize.x * xResolution),
                                  0,
                                  (int)(worldSize.z / terrainSize.z * zResolution));
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(objectBounds.center, objectBounds.size);
        }
    }
}