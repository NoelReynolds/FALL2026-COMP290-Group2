using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SkeletonController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.12f;
    [SerializeField] private float groundCheckWidth = 0.8f;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int attack1Damage = 10;
    [SerializeField] private int attack2Damage = 20;

    private Rigidbody2D rb;
    private Collider2D skeletonCollider;

    private float horizontalInput;
    private bool isGrounded;
    private bool isRunning;
    private bool isShielding;
    private bool isDead;

    private int currentHealth;
    private float lastAttackTime;
    private float jumpGroundLockTimer;

    private const float JumpGroundLockDuration = 0.15f;

    // Animator parameter hashes.
    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int IsGroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int IsRunningHash =
        Animator.StringToHash("IsRunning");

    private static readonly int IsShieldingHash =
        Animator.StringToHash("IsShielding");

    private static readonly int Attack1Hash =
        Animator.StringToHash("Attack1");

    private static readonly int Attack2Hash =
        Animator.StringToHash("Attack2");

    private static readonly int TakeHitHash =
        Animator.StringToHash("TakeHit");

    private static readonly int DieHash =
        Animator.StringToHash("Die");

    private static readonly int JumpHash =
        Animator.StringToHash("Jump");


    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        skeletonCollider = GetComponent<Collider2D>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        currentHealth = maxHealth;

        if (skeletonCollider == null)
        {
            Debug.LogError(
                "Skeleton needs a Collider2D component!"
            );
        }

        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            Debug.LogWarning(
                "Skeleton Rigidbody2D should use Dynamic Body Type."
            );
        }
    }


    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (jumpGroundLockTimer > 0f)
        {
            jumpGroundLockTimer -= Time.deltaTime;
        }

        CheckGrounded();

        HandleMovementInput();
        HandleJump();
        HandleCombatInput();
        HandleShield();

        UpdateAnimations();
    }


    // --------------------------------------------------
    // PHYSICS UPDATE
    // --------------------------------------------------

    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        rb.linearVelocity = new Vector2(
            horizontalInput * currentSpeed,
            rb.linearVelocity.y
        );
    }


    // --------------------------------------------------
    // GROUND DETECTION
    // --------------------------------------------------

    private void CheckGrounded()
    {
        if (skeletonCollider == null)
        {
            isGrounded = false;
            return;
        }

        if (jumpGroundLockTimer > 0f)
        {
            isGrounded = false;
            return;
        }

        Bounds bounds = skeletonCollider.bounds;

        // Position the ground check immediately below the feet.
        Vector2 checkPosition = new Vector2(
            bounds.center.x,
            bounds.min.y - 0.02f
        );

        // Make the check slightly narrower than the collider.
        Vector2 checkSize = new Vector2(
            Mathf.Max(0.05f, bounds.size.x * groundCheckWidth),
            groundCheckDistance
        );

        // Detect colliders on the selected ground layer.
        Collider2D groundCollider = Physics2D.OverlapBox(
            checkPosition,
            checkSize,
            0f,
            groundLayer
        );

        isGrounded = groundCollider != null;
    }


    // --------------------------------------------------
    // MOVEMENT
    // --------------------------------------------------

    private void HandleMovementInput()
    {
        horizontalInput = 0f;

        // A = Move left.
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }

        // D = Move right.
        if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }

        // Either Shift key = Run.
        isRunning =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        // Flip the sprite to face the movement direction.
        if (spriteRenderer != null)
        {
            if (horizontalInput < 0f)
            {
                spriteRenderer.flipX = true;
            }
            else if (horizontalInput > 0f)
            {
                spriteRenderer.flipX = false;
            }
        }
    }


    // --------------------------------------------------
    // JUMPING
    // --------------------------------------------------

    private void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }

        Debug.Log(
            "Space pressed. Grounded: " + isGrounded
        );

        if (!isGrounded)
        {
            Debug.LogWarning(
                "Jump blocked: skeleton is not grounded. " +
                "Check the Ground Layer and ground detection rectangle."
            );

            return;
        }

        // Apply upward velocity immediately.
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        // Prevent immediate ground re-detection.
        isGrounded = false;
        jumpGroundLockTimer = JumpGroundLockDuration;

        // Play the jump animation if the parameter exists.
        if (animator != null &&
            HasParameter(
                animator,
                "Jump",
                AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(JumpHash);
        }

        Debug.Log(
            "JUMP SUCCESSFUL! Vertical velocity: " +
            rb.linearVelocity.y
        );
    }


    // --------------------------------------------------
    // COMBAT INPUT
    // --------------------------------------------------

    private void HandleCombatInput()
    {
        // W = Attack 1.
        if (Input.GetKeyDown(KeyCode.W))
        {
            Attack1();
        }

        // E = Attack 2.
        if (Input.GetKeyDown(KeyCode.E))
        {
            Attack2();
        }
    }


    public void Attack1()
    {
        if (isDead || isShielding)
        {
            return;
        }

        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        if (animator != null &&
            HasParameter(
                animator,
                "Attack1",
                AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(Attack1Hash);
        }

        Debug.Log(
            "Skeleton used Attack 1! Damage: " + attack1Damage
        );
    }


    public void Attack2()
    {
        if (isDead || isShielding)
        {
            return;
        }

        if (Time.time < lastAttackTime + attackCooldown)
        {
            return;
        }

        lastAttackTime = Time.time;

        if (animator != null &&
            HasParameter(
                animator,
                "Attack2",
                AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(Attack2Hash);
        }

        Debug.Log(
            "Skeleton used Attack 2! Damage: " + attack2Damage
        );
    }


    // --------------------------------------------------
    // SHIELD
    // --------------------------------------------------

    private void HandleShield()
    {
        // Hold S to activate the shield.
        isShielding = Input.GetKey(KeyCode.S);

        if (animator != null &&
            HasParameter(
                animator,
                "IsShielding",
                AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(IsShieldingHash, isShielding);
        }
    }


    // --------------------------------------------------
    // HEALTH AND DAMAGE
    // --------------------------------------------------

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
        {
            return;
        }

        if (isShielding)
        {
            Debug.Log("Skeleton blocked the attack!");
            return;
        }

        currentHealth = Mathf.Max(
            0,
            currentHealth - damage
        );

        Debug.Log(
            "Skeleton took " + damage +
            " damage. Health: " + currentHealth
        );

        if (animator != null &&
            HasParameter(
                animator,
                "TakeHit",
                AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(TakeHitHash);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + amount
        );

        Debug.Log(
            "Skeleton healed. Health: " + currentHealth
        );
    }


    public int GetCurrentHealth()
    {
        return currentHealth;
    }


    public int GetMaxHealth()
    {
        return maxHealth;
    }


    // --------------------------------------------------
    // DEATH
    // --------------------------------------------------

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        horizontalInput = 0f;
        isShielding = false;

        rb.linearVelocity = Vector2.zero;

        if (animator != null &&
            HasParameter(
                animator,
                "Die",
                AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(DieHash);
        }

        Debug.Log("Skeleton has died!");
    }


    // --------------------------------------------------
    // ANIMATIONS
    // --------------------------------------------------

    private void UpdateAnimations()
    {
        if (animator == null)
        {
            return;
        }

        if (HasParameter(
            animator,
            "Speed",
            AnimatorControllerParameterType.Float))
        {
            float animationSpeed =
                Mathf.Abs(horizontalInput) *
                (isRunning ? runSpeed : walkSpeed);

            animator.SetFloat(
                SpeedHash,
                animationSpeed
            );
        }

        if (HasParameter(
            animator,
            "IsGrounded",
            AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(
                IsGroundedHash,
                isGrounded
            );
        }

        if (HasParameter(
            animator,
            "IsRunning",
            AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(
                IsRunningHash,
                isRunning
            );
        }
    }


    // --------------------------------------------------
    // ANIMATOR PARAMETER VALIDATION
    // --------------------------------------------------

    private bool HasParameter(
        Animator targetAnimator,
        string parameterName,
        AnimatorControllerParameterType parameterType)
    {
        foreach (
            AnimatorControllerParameter parameter
            in targetAnimator.parameters)
        {
            if (parameter.name == parameterName &&
                parameter.type == parameterType)
            {
                return true;
            }
        }

        return false;
    }


    // --------------------------------------------------
    // VISUALIZE GROUND DETECTION
    // --------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col == null)
        {
            return;
        }

        Bounds bounds = col.bounds;

        Vector2 checkPosition = new Vector2(
            bounds.center.x,
            bounds.min.y - 0.02f
        );

        Vector2 checkSize = new Vector2(
            Mathf.Max(0.05f, bounds.size.x * groundCheckWidth),
            groundCheckDistance
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(
            checkPosition,
            checkSize
        );
    }
}
