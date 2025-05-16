using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingBoat : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float desiredDistance = 5f;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float rotationSpeed = 70f;
    
    [Header("Obstacle Avoidance Settings")]
    [SerializeField] [Tooltip("The distance from the object before casting a circle")] float obstacleAvoidanceRange = 3f;
    [SerializeField] [Tooltip("The radius of the circle casted for checking if obstacles are around")] float castRadius = 1f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] float avoidTurnAngle = 110f;

    [FormerlySerializedAs("checkTileEveryNFrames")]
    [Header("World Tile Tracking")]
    [Tooltip("How often (in frames) to check for tile changes.")]
    [SerializeField] int frameDelay = 10;
    [SerializeField] private float tileDetectionRadius = 0.1f;
    [SerializeField] private LayerMask tileLayer;

    [Header("Debugging")]
    [SerializeField] Transform player;
    
    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private Transform parentTransform;
    
    private enum AvoidDirection { None, Left, Right }
    private AvoidDirection lastAvoidDirection  = AvoidDirection.None;

    private int frameCounter = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = false;
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        HandleMovement();

        frameCounter++;
        if (frameCounter >= frameDelay)
        {
            DetectTile();
            frameCounter = 0;
        }
    }

    public void AssignPlayerTransform(Transform playerTransform)
    {
        this.player = playerTransform;
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
            float proximity = 1f - (hit.distance / obstacleAvoidanceRange);
            float dynamicTurnAngle = Mathf.Lerp(5f, avoidTurnAngle, proximity);

            Vector2 leftDir = Quaternion.Euler(0, 0, dynamicTurnAngle) * desiredDirection;
            Vector2 rightDir = Quaternion.Euler(0, 0, -dynamicTurnAngle) * desiredDirection;

            bool leftClear = !Physics2D.CircleCast(transform.position, castRadius, leftDir, obstacleAvoidanceRange * 0.5f, obstacleLayer);
            bool rightClear = !Physics2D.CircleCast(transform.position, castRadius, rightDir, obstacleAvoidanceRange * 0.5f, obstacleLayer);

            if (leftClear && rightClear)
            {
                // Continue in previous direction if valid, else pick one
                if (lastAvoidDirection == AvoidDirection.Right)
                {
                    desiredDirection = rightDir;
                }
                else
                {
                    desiredDirection = leftDir;
                    lastAvoidDirection = AvoidDirection.Left;
                }
            }
            else if (leftClear)
            {
                desiredDirection = leftDir;
                lastAvoidDirection = AvoidDirection.Left;
            }
            else if (rightClear)
            {
                desiredDirection = rightDir;
                lastAvoidDirection = AvoidDirection.Right;
            }
            else
            {
                rb.velocity = Vector2.zero;
                return;
            }
        }
        else
        {
            // Reset if no obstacle in direct path
            lastAvoidDirection = AvoidDirection.None;
        }

        currentDirection = Vector2.Lerp(currentDirection, desiredDirection, Time.fixedDeltaTime * rotationSpeed / 10f).normalized;

        float step = moveSpeed * Time.deltaTime;
        transform.Translate(currentDirection * step, Space.World);

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
        Gizmos.DrawSphere((Vector2)transform.position + direction * obstacleAvoidanceRange, castRadius);
    }
}
