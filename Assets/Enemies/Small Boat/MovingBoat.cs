using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingBoat : MonoBehaviour
{
    public Transform player;
    public float desiredDistance = 5f;
    public float moveSpeed = 3f;
    public float obstacleAvoidanceRange = 1f;
    public float castRadius = 0.5f;
    public LayerMask obstacleLayer;
    public float avoidTurnAngle = 30f;
    public float rotationSpeed = 200f; // Degrees per second

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = false;
    }

    void FixedUpdate()
    {
        if (player == null)
            return;

        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > desiredDistance)
        {
            Vector2 desiredDirection = toPlayer.normalized;

            RaycastHit2D hit = Physics2D.CircleCast(transform.position, castRadius, desiredDirection, obstacleAvoidanceRange, obstacleLayer);
            // Obstacle detection and avoidance
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
            }

            rb.velocity = desiredDirection * moveSpeed;

            // 🚤 Smooth rotation toward movement direction
            float angle = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg - 90f;
            float smoothedAngle = Mathf.MoveTowardsAngle(rb.rotation, angle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothedAngle);
        }
        else
        {
            rb.velocity = Vector2.zero;
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