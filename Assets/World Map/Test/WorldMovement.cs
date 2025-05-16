using System.Collections.Generic;
using UnityEngine;

public class WorldMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float worldMoveSpeed = 5f;
    [SerializeField] private Transform worldParent;

    [Header("World Settings")] 
    [SerializeField] [Tooltip("The number of frames to skip for updating the world | Increasing this may increase performance by removing world checks")]
    private int frameDelay = 0;
    [SerializeField] [Tooltip("Tiles within this range will be displayed. Tiles outside this range will be culled.")]
    private float cullDistance = 35f; 

    [SerializeField] private WorldSpawner worldSpawner;

    private Camera mainCamera; 
    
    int frameCounter = -1;
    private Vector2Int worldSize;
    private float tileSize;
    private Dictionary<Vector2Int, Transform> tileMap;

    private void Start()
    {
        if (worldParent == null)
        {
            Debug.LogError("World Parent is not assigned!");
            return;
        }

        if (worldSpawner)
        {
            worldSize = worldSpawner.GetWorldSizeInTiles();
            tileSize = worldSpawner.GetTileSize();
            tileMap = worldSpawner.GetTileMap();
        }
        else
        {
            Debug.LogError("Missing WorldSpawner reference!");
            return;
        }

        mainCamera = Camera.main;
    }

    private void Update()
    {
        frameCounter++;
        RotatePlayerTowardsMouse();

        MoveWorld();

        if (frameCounter >= frameDelay)
        {
            ManageTiles();
            frameCounter = 0;
        }
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
        Vector2 moveOffset = -transform.right * (worldMoveSpeed * Time.deltaTime);
        worldParent.position += (Vector3)moveOffset;
    }
    
    private void ManageTiles()
    {
        foreach (var kvp in tileMap)
        {
            Transform tileTransform = kvp.Value;

            if (tileTransform == null)
                return;
            
            CullTiles(tileTransform);

            WrapTiles(tileTransform, transform.position);
            
        }
    }
    
    private void CullTiles(Transform tileTransform)
    {
        float distance = Vector3.Distance(transform.position, tileTransform.position);
        tileTransform.gameObject.SetActive(distance <= cullDistance);
    }
    
    private void WrapTiles(Transform tileTransform, Vector3 playerPos)
    {
        Vector3 tilePos = tileTransform.position;
        Vector3 offset = tilePos - playerPos;

        int moveX = 0, moveY = 0;
            
        if (offset.x > tileSize * worldSize.x / 2f)
            moveX = -worldSize.x;
        else if (offset.x < -tileSize * worldSize.x / 2f)
            moveX = worldSize.x;
            
        if (offset.y > tileSize * worldSize.y / 2f)
            moveY = -worldSize.y;
        else if (offset.y < -tileSize * worldSize.y / 2f)
            moveY = worldSize.y;

        if (moveX != 0 || moveY != 0)
        {
            tileTransform.position += new Vector3(moveX * tileSize, moveY * tileSize, 0f);
        }
    }
}
