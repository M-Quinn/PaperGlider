using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class WorldSpawner : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] [Tooltip("2D array size / 0,0 is bottom left")] private int gridSize;
    [SerializeField] [Tooltip("The object that the world tiles will live under")] private Transform worldParent;
    
    [Header("Ground Settings")]
    [SerializeField] [Tooltip("The prefab for the world tiles")] private GameObject groundPrefab;
    [SerializeField] [Tooltip("These will be assigned in their order")] private Material[] tileMaterials;
    
    [Header("Obstacle Settings")]
    [SerializeField] GameObject obstaclePrefab; //TODO: add a min - max setting for tiles and an array for different obstacles

    private Dictionary<Vector2Int, Transform> tileMap = new Dictionary<Vector2Int, Transform>();

    
    Vector2Int gridSizeVector;
    private float spacingX;
    private float spacingY;

    private SpriteRenderer spriteRenderer;
    private Vector3 localMinBounds;
    private Vector3 localMaxBounds;
    
    

    private void Awake()
    {
        gridSizeVector = new Vector2Int(gridSize, gridSize);
        
        if (groundPrefab == null)
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
        Renderer planeRenderer = groundPrefab.GetComponent<Renderer>();
        if (planeRenderer != null)
        {
            spacingX = planeRenderer.bounds.size.x;
            spacingY = planeRenderer.bounds.size.y;
            
            SpawnGrid();
        }
        else
        {
            Debug.LogError("Plane prefab does not have a Renderer component to determine size. Please manually set the spacing or ensure the prefab has a Renderer.");
            return;
        }
    }


    /// <summary>
    /// Creates a 2D grid with 0,0 at the bottom left and 1,1 at top right
    /// </summary>
    private void SpawnGrid()
    {
        float totalWidth = (gridSizeVector.x - 1) * spacingX;
        float totalHeight = (gridSizeVector.y - 1) * spacingY;
        Vector3 centerOffset = new Vector3(-totalWidth / 2f, -totalHeight / 2f, 0f);

        int materialIndex = 0;

        for (int y = 0; y < gridSizeVector.x; y++)
        {
            for (int x = 0; x < gridSizeVector.y; x++)
            {
                Vector3 spawnPosition = new Vector3(x * spacingX, y * spacingY, 0f) + centerOffset + worldParent.position;
                GameObject tileInstance = Instantiate(groundPrefab, spawnPosition, Quaternion.identity, worldParent);

                spriteRenderer = tileInstance.GetComponent<SpriteRenderer>();
                
                localMinBounds = tileInstance.transform.InverseTransformPoint(spriteRenderer.bounds.min);
                localMaxBounds = tileInstance.transform.InverseTransformPoint(spriteRenderer.bounds.max);

                SpawnObstacles(tileInstance);

                if (spriteRenderer != null)
                {
                    spriteRenderer.material = tileMaterials[materialIndex % tileMaterials.Length];
                }
                else
                {
                    Debug.LogWarning("Instantiated plane does not have a Renderer!");
                }

                materialIndex++;
                
                Vector2Int tileCoord = new Vector2Int(x, y);
                
                tileMap.Add(tileCoord, tileInstance.transform);
                tileInstance.name = $"Tile_{tileCoord.y}_{tileCoord.x}";
            }
        }
    }

    /// <summary>
    /// The logic for spawning obstacles
    /// </summary>
    /// <param name="tileInstance"></param>
    private void SpawnObstacles(GameObject tileInstance)
    {
        Vector3 obstacleSpawnPosition = new Vector3(
            Random.Range(localMinBounds.x, localMaxBounds.x),
            Random.Range(localMinBounds.y, localMaxBounds.y),
            -1f);
                
        GameObject obstacleInstance = Instantiate(obstaclePrefab, Vector3.zero, Quaternion.identity);
                
        obstacleInstance.transform.SetParent(tileInstance.transform);
        obstacleInstance.transform.localPosition = obstacleSpawnPosition;
    }

    public Transform GetTile(Vector2Int tileCoord)
    {
        if (tileMap.ContainsKey(tileCoord))
        {
            return tileMap[tileCoord];
        }
        return null; 
    }

    public Dictionary<Vector2Int, Transform> GetTileMap()
    {
        return tileMap;
    }

    public Vector2Int GetWorldSizeInTiles()
    {
        return gridSizeVector;
    }

    public float GetTileSize()
    {
        return spacingX;// it's fine since it's a square
    }
}