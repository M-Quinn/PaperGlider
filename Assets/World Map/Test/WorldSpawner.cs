using UnityEngine;
using System.Collections.Generic;

public class WorldSpawner : MonoBehaviour
{
    [SerializeField] private GameObject planePrefab;
    [SerializeField] private Material[] tileMaterials;
    [SerializeField] private Transform worldParent;

    private Dictionary<Vector2Int, Transform> tileMap = new Dictionary<Vector2Int, Transform>();
    
    private int gridSizeX = 5; //5x5
    private int gridSizeY = 5;
    private float spacingX;
    private float spacingY;

    void Start()
    {
        if (planePrefab == null)
        {
            Debug.LogError("Plane Prefab is not assigned!");
            return;
        }

        if (worldParent == null)
        {
            Debug.LogError("World Parent is not assigned!");
            return;
        }

        // Attempt to get the size from the plane's Renderer bounds
        Renderer planeRenderer = planePrefab.GetComponent<Renderer>();
        if (planeRenderer != null)
        {
            spacingX = planeRenderer.bounds.size.x;
            spacingY = planeRenderer.bounds.size.y;
            Debug.Log($"Tile Size X: {spacingX}, Tile Size Z: {spacingY}");
            SpawnGrid();
        }
        else
        {
            Debug.LogError("Plane prefab does not have a Renderer component to determine size. Please manually set the spacing or ensure the prefab has a Renderer.");
        }
    }

    private void SpawnGrid()
    {
        float totalWidth = (gridSizeX - 1) * spacingX;
        float totalHeight = (gridSizeY - 1) * spacingY;
        Vector3 centerOffset = new Vector3(-totalWidth / 2f, -totalHeight / 2f, 0f);

        int materialIndex = 0;

        for (int y = 0; y < gridSizeX; y++)
        {
            for (int x = 0; x < gridSizeY; x++)
            {
                Vector3 spawnPosition = new Vector3(x * spacingX, y * spacingY, 0f) + centerOffset + worldParent.position;
                GameObject tileInstance = Instantiate(planePrefab, spawnPosition, Quaternion.Euler(90f,90f,-90f), worldParent);

                Renderer renderer = tileInstance.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = tileMaterials[materialIndex % tileMaterials.Length];
                }
                else
                {
                    Debug.LogWarning("Instantiated plane does not have a Renderer!");
                }

                materialIndex++;
                
                Vector2Int tileCoord = new Vector2Int(x, y);
                
                tileMap.Add(tileCoord, tileInstance.transform);
                tileInstance.name = $"Tile_{tileCoord.x}_{tileCoord.y}";
            }
        }
    }
    
    public Transform GetTile(Vector2Int tileCoord)
    {
        if (tileMap.ContainsKey(tileCoord))
        {
            return tileMap[tileCoord];
        }
        return null; // Or handle the case where the tile doesn't exist
    }

    public Dictionary<Vector2Int, Transform> GetTileMap()
    {
        return tileMap;
    }

    public Vector2Int GetWorldSizeInTiles()
    {
        return new Vector2Int(gridSizeX, gridSizeY);
    }

    public float GetTileSize()
    {
        return spacingX;// its fine since it's a square
    }
}