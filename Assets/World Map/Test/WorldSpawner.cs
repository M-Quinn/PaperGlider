using UnityEngine;
using System.Collections.Generic;

public class WorldSpawner : MonoBehaviour
{
    [SerializeField] private GameObject planePrefab;
    [SerializeField] private Material[] tileMaterials;
    [SerializeField] private Transform worldParent;

    private int gridSizeX = 3;
    private int gridSizeY = 3;
    private float spacingX;
    private float spacingY;

    void Start()
    {
        if (planePrefab == null)
        {
            Debug.LogError("Plane Prefab is not assigned!");
            return;
        }

        if (tileMaterials == null || tileMaterials.Length < gridSizeX * gridSizeY)
        {
            Debug.LogError("Not enough materials assigned in tileMaterials array!");
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
            }
        }
    }
}