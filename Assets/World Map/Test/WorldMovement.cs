using System.Collections.Generic;
using UnityEngine;

public class WorldMovement : MonoBehaviour
{
    [SerializeField] private float worldMoveSpeed = 5f;
    [SerializeField] private Transform worldParent; // Assign your "WorldTiles" parent here
    private Camera mainCamera; 
    [SerializeField] private float cullDistance = 20f; // Distance from player to disable tiles

    private Vector2Int playerTileCoord = Vector2Int.zero; // Keep track of what tile the player is on
    [SerializeField] private WorldSpawner worldSpawner;

    private void Start()
    {
        if (worldParent == null)
        {
            Debug.LogError("World Parent is not assigned!");
            return;
        }

        mainCamera = Camera.main;
    }

    private void Update()
    {
        RotatePlayerTowardsMouse();

        
        MoveWorld();

        
        UpdateActiveTiles();
    }

    private void RotatePlayerTowardsMouse()
    {
        if (mainCamera == null) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.transform.position.z));
        Vector2 direction = mouseWorldPosition - transform.position;
        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void MoveWorld()
    {
        Vector2 moveOffset = -transform.right * worldMoveSpeed * Time.deltaTime;
        worldParent.position += (Vector3)moveOffset;

        // Update player tile coordinate
        playerTileCoord = WorldToTileCoordinates(transform.position);
        
        WrapTiles();
    }

    private Vector2Int WorldToTileCoordinates(Vector3 worldPosition)
    {
        Vector3 worldCenter = worldParent.position;
        float halfWorldWidth = (worldSpawner.GetWorldSizeInTiles().x * worldSpawner.GetTileSize()) / 2f;
        float halfWorldHeight = (worldSpawner.GetWorldSizeInTiles().y * worldSpawner.GetTileSize()) / 2f;

        // Calculate tile coordinates, handling wrapping.
        int x = Mathf.FloorToInt((worldPosition.x - worldCenter.x + halfWorldWidth) / worldSpawner.GetTileSize());
        int y = Mathf.FloorToInt((worldPosition.z - worldCenter.z + halfWorldHeight) / worldSpawner.GetTileSize());

        // Wrap the coordinates
        x = (x % worldSpawner.GetWorldSizeInTiles().x + worldSpawner.GetWorldSizeInTiles().x) % worldSpawner.GetWorldSizeInTiles().x;
        y = (y % worldSpawner.GetWorldSizeInTiles().y + worldSpawner.GetWorldSizeInTiles().y) % worldSpawner.GetWorldSizeInTiles().y;

        return new Vector2Int(x, y);
    }

    private void UpdateActiveTiles()
    {
        Dictionary<Vector2Int, Transform> tileMap = worldSpawner.GetTileMap();
        Vector2Int worldSizeInTiles = worldSpawner.GetWorldSizeInTiles();

        for (int x = 0; x < worldSizeInTiles.x; x++)
        {
            for (int y = 0; y < worldSizeInTiles.y; y++)
            {
                Vector2Int tileCoord = new Vector2Int(x, y);
                Transform tileTransform = tileMap[tileCoord];
                if (tileTransform != null)
                {
                    float distance = Vector3.Distance(transform.position, tileTransform.position);
                    tileTransform.gameObject.SetActive(distance <= cullDistance);
                }
            }
        }
    }
    private void WrapTiles()
    {
        Dictionary<Vector2Int, Transform> tileMap = worldSpawner.GetTileMap();
        Vector2Int gridSize = worldSpawner.GetWorldSizeInTiles();
        float tileSize = worldSpawner.GetTileSize();
        Vector3 playerPos = transform.position;

        foreach (var kvp in tileMap)
        {
            Vector2Int tileCoord = kvp.Key;
            Transform tileTransform = kvp.Value;

            Vector3 tilePos = tileTransform.position;
            Vector3 offset = tilePos - playerPos;

            int moveX = 0, moveY = 0;

            // Wrap horizontally
            if (offset.x > tileSize * gridSize.x / 2f)
                moveX = -gridSize.x;
            else if (offset.x < -tileSize * gridSize.x / 2f)
                moveX = gridSize.x;

            // Wrap vertically
            if (offset.y > tileSize * gridSize.y / 2f)
                moveY = -gridSize.y;
            else if (offset.y < -tileSize * gridSize.y / 2f)
                moveY = gridSize.y;

            if (moveX != 0 || moveY != 0)
            {
                tileTransform.position += new Vector3(moveX * tileSize, moveY * tileSize, 0f);
            }
        }
    }
}
