using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class SkeletonController2D : MonoBehaviour
{
    public bool isPossessed = true;

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

    [Header("Audio")]
    [Tooltip("Sound played when the skeleton performs Attack 1.")]
    [SerializeField] private AudioClip attack1Sound;

    [Tooltip("Sound played when the skeleton performs Attack 2.")]
    [SerializeField] private AudioClip attack2Sound;

    [Tooltip("Sound played when the skeleton successfully jumps.")]
    [SerializeField] private AudioClip jumpSound;

    [Tooltip("Sound played when the skeleton takes non-fatal damage.")]
    [SerializeField] private AudioClip takeDamageSound;

    [Tooltip("Sound played when the skeleton dies.")]
    [SerializeField] private AudioClip dieSound;

    [Tooltip("Optional sound played when the skeleton first raises its shield.")]
    [SerializeField] private AudioClip shieldSound;

    [Range(0f, 1f)]
    [SerializeField] private float soundEffectVolume = 1f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("Combat")]
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private int attack1Damage = 10;
    [SerializeField] private int attack2Damage = 20;

    [Header("Attack Detection")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.85f;
    [SerializeField] private float attackForwardOffset = 0.65f;
    [SerializeField] private float attackVerticalOffset = 0.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5.5f;
    [SerializeField] private float knockbackDuration = 0.25f;
    [SerializeField] private float knockbackDeceleration = 16f;

    private Rigidbody2D rb;
    private Collider2D skeletonCollider;
    private AudioSource audioSource;

    private float horizontalInput;
    private bool isGrounded;
    private bool isRunning;
    private bool isShielding;
    private bool isDead;

    private int currentHealth;
    private float lastAttackTime;
    private float jumpGroundLockTimer;

    private float knockbackVelocityX;
    private float knockbackTimer;

    private const float JumpGroundLockDuration = 0.15f;

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
        audioSource = GetComponent<AudioSource>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;

        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            Debug.LogWarning(
                "Skeleton Rigidbody2D should use Dynamic Body Type.",
                this
            );
        }

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Audio clips will play only when requested by this script.
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;

            // Use 2D audio so sound volume does not depend on
            // the distance between the skeleton and the camera.
            audioSource.spatialBlend = 0f;
        }
    }


    // --------------------------------------------------
    // AUDIO
    // --------------------------------------------------

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(
            clip,
            soundEffectVolume
        );
    }


    // --------------------------------------------------
    // UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (isDead)
            return;

        if (jumpGroundLockTimer > 0f)
            jumpGroundLockTimer -= Time.deltaTime;

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
            return;

        // Knockback takes priority over regular movement.
        if (knockbackTimer > 0f)
        {
            rb.linearVelocity = new Vector2(
                knockbackVelocityX,
                rb.linearVelocity.y
            );

            knockbackVelocityX = Mathf.MoveTowards(
                knockbackVelocityX,
                0f,
                knockbackDeceleration * Time.fixedDeltaTime
            );

            knockbackTimer -= Time.fixedDeltaTime;

            if (knockbackTimer <= 0f)
            {
                knockbackTimer = 0f;
                knockbackVelocityX = 0f;
            }

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
        if (skeletonCollider == null || jumpGroundLockTimer > 0f)
        {
            isGrounded = false;
            return;
        }

        Bounds bounds = skeletonCollider.bounds;

        Vector2 checkPosition = new Vector2(
            bounds.center.x,
            bounds.min.y - 0.02f
        );

        Vector2 checkSize = new Vector2(
            Mathf.Max(0.05f, bounds.size.x * groundCheckWidth),
            groundCheckDistance
        );

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

        if (Input.GetKey(KeyCode.A))
            horizontalInput = -1f;

        if (Input.GetKey(KeyCode.D))
            horizontalInput = 1f;

        isRunning =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        if (spriteRenderer != null)
        {
            if (horizontalInput < 0f)
                spriteRenderer.flipX = true;
            else if (horizontalInput > 0f)
                spriteRenderer.flipX = false;
        }
    }


    // --------------------------------------------------
    // JUMPING
    // --------------------------------------------------

    private void HandleJump()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        if (!isGrounded)
        {
            Debug.Log("Jump blocked: skeleton is not grounded.");
            return;
        }

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        isGrounded = false;
        jumpGroundLockTimer = JumpGroundLockDuration;

        if (animator != null &&
            HasParameter(
                "Jump",
                AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(JumpHash);
        }

        // Play only when the jump is successful.
        PlaySound(jumpSound);
    }


    // --------------------------------------------------
    // COMBAT INPUT
    // --------------------------------------------------

    private void HandleCombatInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
            Attack1();

        if (Input.GetKeyDown(KeyCode.E))
            Attack2();
    }


    // --------------------------------------------------
    // ATTACK 1
    // --------------------------------------------------

    public void Attack1()
    {
        if (isDead || isShielding)
            return;

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (animator != null &&
            HasParameter(
                "Attack1",
                AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(Attack1Hash);
        }

        // Play the attack sound when the attack is accepted,
        // whether or not an enemy is within range.
        PlaySound(attack1Sound);

        Debug.Log("Skeleton Attack 1! Damage: " + attack1Damage);

        DamageEnemiesInRange(attack1Damage);
    }


    // --------------------------------------------------
    // ATTACK 2
    // --------------------------------------------------

    public void Attack2()
    {
        if (isDead || isShielding)
            return;

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        if (animator != null &&
            HasParameter(
                "Attack2",
                AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(Attack2Hash);
        }

        // Play the attack sound when the attack is accepted.
        PlaySound(attack2Sound);

        Debug.Log("Skeleton Attack 2! Damage: " + attack2Damage);

        DamageEnemiesInRange(attack2Damage);
    }


    // --------------------------------------------------
    // ENEMY HIT DETECTION
    // --------------------------------------------------

    private void DamageEnemiesInRange(int damage)
    {
        Vector2 hitPosition = GetAttackPosition();

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            hitPosition,
            attackRange,
            enemyLayer
        );

        if (hits.Length == 0)
        {
            Debug.Log(
                "Attack missed. No enemies detected. " +
                "Check Attack Point, Attack Range, and Enemy Layer."
            );

            return;
        }

        HashSet<EnemyController2D> damagedEnemies =
            new HashSet<EnemyController2D>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            EnemyController2D enemy =
                hit.GetComponentInParent<EnemyController2D>();

            if (enemy == null || enemy.IsDead())
                continue;

            if (!damagedEnemies.Add(enemy))
                continue;

            enemy.TakeDamage(damage);

            Debug.Log(
                "Skeleton hit " + enemy.name +
                " for " + damage + " damage."
            );
        }
    }


    private Vector2 GetAttackPosition()
    {
        if (attackPoint != null)
            return attackPoint.position;

        float facingDirection = 1f;

        if (spriteRenderer != null && spriteRenderer.flipX)
            facingDirection = -1f;

        return (Vector2)transform.position +
               new Vector2(
                   facingDirection * attackForwardOffset,
                   attackVerticalOffset
               );
    }


    // --------------------------------------------------
    // SHIELD
    // --------------------------------------------------

    private void HandleShield()
    {
        // Remember the previous state so the sound plays once
        // when the shield is raised, not every frame it is held.
        bool wasShielding = isShielding;

        isShielding = Input.GetKey(KeyCode.S);

        if (animator != null &&
            HasParameter(
                "IsShielding",
                AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(IsShieldingHash, isShielding);
        }

        // Play the optional shield sound only when shielding starts.
        if (isShielding && !wasShielding)
        {
            PlaySound(shieldSound);
        }
    }


    // --------------------------------------------------
    // HEALTH AND DAMAGE
    // --------------------------------------------------

    // Retains compatibility with existing calls that only
    // provide damage and do not provide an attacker's position.
    public void TakeDamage(int damage)
    {
        float facingDirection = 1f;

        if (spriteRenderer != null && spriteRenderer.flipX)
            facingDirection = -1f;

        Vector2 estimatedSourcePosition =
            (Vector2)transform.position -
            new Vector2(facingDirection, 0f);

        TakeDamage(damage, estimatedSourcePosition);
    }


    // Called by an enemy that knows its own position.
    public void TakeDamage(int damage, Vector2 sourcePosition)
    {
        if (isDead || damage <= 0)
            return;

        // Shielding blocks both damage and knockback.
        if (isShielding)
        {
            Debug.Log("Skeleton blocked the attack!");
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log(
            "Skeleton took " + damage +
            " damage. Health: " + currentHealth +
            "/" + maxHealth
        );

        if (animator != null &&
            HasParameter(
                "TakeHit",
                AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(TakeHitHash);
        }

        ApplyKnockback(sourcePosition);

        if (currentHealth <= 0)
        {
            // Die() plays the death sound for a fatal hit.
            // This avoids playing the regular damage sound
            // at the same time as the death sound.
            Die();
        }
        else
        {
            // Play the regular damage sound for non-fatal hits.
            PlaySound(takeDamageSound);
        }
    }


    private void ApplyKnockback(Vector2 sourcePosition)
    {
        if (rb == null || isDead)
            return;

        float difference =
            transform.position.x - sourcePosition.x;

        float direction;

        if (Mathf.Abs(difference) > 0.01f)
        {
            // Push the skeleton away from the attacker.
            direction = Mathf.Sign(difference);
        }
        else
        {
            // If both characters occupy nearly the same X
            // position, use the skeleton's facing direction.
            direction =
                spriteRenderer != null && spriteRenderer.flipX
                ? -1f
                : 1f;
        }

        knockbackVelocityX = direction * knockbackForce;
        knockbackTimer = knockbackDuration;

        // Clear the old horizontal velocity.
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );

        Debug.Log(
            "Skeleton received knockback. Direction: " +
            direction + ", force: " + knockbackVelocityX
        );
    }


    // --------------------------------------------------
    // HEALING
    // --------------------------------------------------

    public void Heal(int amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + amount
        );

        Debug.Log(
            "Skeleton healed. Health: " +
            currentHealth + "/" + maxHealth
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
    // DEATH STATUS
    // --------------------------------------------------

    public bool IsDead()
    {
        return isDead;
    }


    // --------------------------------------------------
    // DEATH
    // --------------------------------------------------

    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        horizontalInput = 0f;
        isShielding = false;

        knockbackVelocityX = 0f;
        knockbackTimer = 0f;

        rb.linearVelocity = Vector2.zero;

        if (animator != null &&
            HasParameter(
                "Die",
                AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(DieHash);
        }

        // Play the death sound once.
        PlaySound(dieSound);

        Debug.Log("Skeleton has died!");
    }


    // --------------------------------------------------
    // ANIMATIONS
    // --------------------------------------------------

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        if (HasParameter(
            "Speed",
            AnimatorControllerParameterType.Float))
        {
            float animationSpeed =
                Mathf.Abs(horizontalInput) *
                (isRunning ? runSpeed : walkSpeed);

            // Reflect actual horizontal motion during knockback
            // so the Speed parameter doesn't report normal
            // walking while the character is being pushed.
            if (knockbackTimer > 0f && rb != null)
                animationSpeed = Mathf.Abs(rb.linearVelocity.x);

            animator.SetFloat(SpeedHash, animationSpeed);
        }

        if (HasParameter(
            "IsGrounded",
            AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(IsGroundedHash, isGrounded);
        }

        if (HasParameter(
            "IsRunning",
            AnimatorControllerParameterType.Bool))
        {
            animator.SetBool(IsRunningHash, isRunning);
        }
    }


    // --------------------------------------------------
    // ANIMATOR PARAMETER VALIDATION
    // --------------------------------------------------

    private bool HasParameter(
        string parameterName,
        AnimatorControllerParameterType parameterType)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
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
    // GIZMOS
    // --------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
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
            Gizmos.DrawWireCube(checkPosition, checkSize);
        }

        Vector2 hitPosition = GetAttackPosition();

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitPosition, attackRange);
    }
}