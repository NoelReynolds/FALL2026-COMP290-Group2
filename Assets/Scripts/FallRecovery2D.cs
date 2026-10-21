using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FallRecovery2D : MonoBehaviour
{
    [Header("Fall Recovery")]
    [SerializeField] private float fallTimeLimit = 5f;

    [Header("Fall Damage")]
    [SerializeField] private int fallDamage = 20;

    private Rigidbody2D rb;
    private SkeletonController2D player;

    private float fallTimer;
    private Vector2 fallStartPosition;

    private bool isFalling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<SkeletonController2D>();
    }

    private void FixedUpdate()
    {
        // Stop processing if the skeleton is dead.
        if (player != null && player.IsDead())
        {
            return;
        }

        // Detect when the skeleton starts descending.
        if (rb.linearVelocity.y < -0.1f)
        {
            // Save the position only once at the beginning of the fall.
            if (!isFalling)
            {
                isFalling = true;
                fallTimer = 0f;
                fallStartPosition = rb.position;

                Debug.Log("Skeleton started falling.");
            }

            // Count how long the skeleton has been falling.
            fallTimer += Time.fixedDeltaTime;

            // Recover after five seconds.
            if (fallTimer >= fallTimeLimit)
            {
                RecoverFromFall();
            }
        }
        else
        {
            // Reset when the skeleton stops descending.
            isFalling = false;
            fallTimer = 0f;
        }
    }

    private void RecoverFromFall()
    {
        RespawnPoint2D[] respawnPoints =
            FindObjectsByType<RespawnPoint2D>(
                FindObjectsSortMode.None
            );

        if (respawnPoints.Length == 0)
        {
            Debug.LogWarning(
                "FallRecovery2D: No respawn points exist in the scene."
            );

            // Avoid repeating the recovery attempt every physics frame.
            fallTimer = 0f;
            isFalling = false;
            return;
        }

        // Find the respawn point closest to where the fall began.
        RespawnPoint2D closestPoint = null;
        float closestDistance = Mathf.Infinity;

        foreach (RespawnPoint2D point in respawnPoints)
        {
            if (point == null)
            {
                continue;
            }

            float distance = Vector2.Distance(
                fallStartPosition,
                point.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }

        if (closestPoint == null)
        {
            fallTimer = 0f;
            isFalling = false;
            return;
        }

        // Teleport the skeleton to the selected respawn point.
        Vector2 respawnPosition = closestPoint.transform.position;

        rb.position = respawnPosition;
        transform.position = respawnPosition;

        // Stop all movement.
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // Reset the fall timer.
        fallTimer = 0f;
        isFalling = false;

        Debug.Log(
            "Skeleton respawned at: " + closestPoint.name
        );

        // Apply fall damage.
        if (player != null)
        {
            player.TakeDamage(fallDamage);
        }
    }
}