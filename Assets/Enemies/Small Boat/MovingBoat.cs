using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingBoat : MonoBehaviour
{
    [Header("Movement")]
    public Transform player;
    public float desiredDistance = 5f;
    public float moveSpeed = 3f;
    public float obstacleAvoidanceRange = 3f;
    public float castRadius = 1f;
    public LayerMask obstacleLayer;
    public float avoidTurnAngle = 90f;
    public float rotationSpeed = 200f;

    [Header("World Tile Tracking")]
    [Tooltip("Assign the WorldSpawner to access the tile map and grid info.")]
    public WorldSpawner worldSpawner;
    [Tooltip("How often (in frames) to check for tile changes.")]
    public int checkTileEveryNFrames = 10;
    [SerializeField] private float tileDetectionRadius = 0.1f;
    [SerializeField] private LayerMask tileLayer;

    private Rigidbody2D rb;
    private Vector2Int currentTileCoord;
    private Transform currentTileTransform;
    private Vector2 currentDirection;
    private Transform parentTransform;

    private int frameCounter = 0;
    private float tileSize;
    private Vector2Int worldSize;
    private Dictionary<Vector2Int, Transform> tileMap;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = false;

        if (worldSpawner != null)
        {
            tileMap = worldSpawner.GetTileMap();
            tileSize = worldSpawner.GetTileSize();
            worldSize = worldSpawner.GetWorldSizeInTiles();
        }
        else
        {
            Debug.LogError("WorldSpawner reference not set on MovingBoat.");
        }
    }

    void FixedUpdate()
    {
        if (player == null || tileMap == null) return;

        HandleMovement();

        frameCounter++;
        if (frameCounter >= checkTileEveryNFrames)
        {
            DetectTile();
            frameCounter = 0;
        }
    }

    private void HandleMovement()
    {
        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > desiredDistance)
        {
            Vector2 desiredDirection = toPlayer.normalized;

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, castRadius, desiredDirection, obstacleAvoidanceRange, obstacleLayer);
            if (hit)
            {
                
                float proximity = 1f - (hit.distance / obstacleAvoidanceRange); // 0 (far) to 1 (very close)
                float dynamicTurnAngle = Mathf.Lerp(5f, avoidTurnAngle, proximity);
                
                Vector2 leftDir = Quaternion.Euler(0, 0, dynamicTurnAngle) * desiredDirection;
                Vector2 rightDir = Quaternion.Euler(0, 0, -dynamicTurnAngle) * desiredDirection;

                bool leftClear = !Physics2D.Raycast(transform.position, leftDir, obstacleAvoidanceRange, obstacleLayer);
                bool rightClear = !Physics2D.Raycast(transform.position, rightDir, obstacleAvoidanceRange, obstacleLayer);

                if (leftClear)
                    desiredDirection = leftDir;
                else if (rightClear)
                    desiredDirection = rightDir;
                else
                {
                    rb.velocity = Vector2.zero;
                    return;
                }
                currentDirection = desiredDirection;
            }

            currentDirection = Vector2.Lerp(currentDirection, desiredDirection, Time.fixedDeltaTime * rotationSpeed/10).normalized;
            
            float step = moveSpeed * Time.deltaTime;
            transform.Translate(currentDirection * step, Space.World);

            // Rotation using transform
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg - 90f;
            float currentZ = transform.eulerAngles.z;
            float smoothZ = Mathf.LerpAngle(currentZ, angle, rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, smoothZ);
        }
    }

    private void DetectTile()
    {
        Collider2D tile = Physics2D.OverlapCircle(transform.position, tileDetectionRadius, tileLayer);
        if (tile != null)
        {
            if (tile.transform == parentTransform)
                return;
            transform.SetParent(null);
            transform.SetParent(tile.transform);
            parentTransform = tile.transform;
            Debug.Log("Standing on tile: " + tile.name);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Vector2 direction = (player.position - transform.position).normalized;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + direction * obstacleAvoidanceRange);
    }
}
