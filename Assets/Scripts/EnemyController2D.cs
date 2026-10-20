using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))]
public class EnemyController2D : MonoBehaviour
{
    public enum EnemyType
    {
        Rat,
        Bat,
        Chest
    }

    [Header("Enemy Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Rat;

    [Header("Health")]
    [SerializeField] private int maxHealth = 50;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private bool isFlying = false;

    [Header("Combat")]
    [SerializeField] private float attackRange = 1.25f;
    [SerializeField] private float verticalAttackTolerance = 1.3f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int attack1Damage = 10;
    [SerializeField] private int attack2Damage = 20;
    [SerializeField] private bool useSecondAttack = false;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5.5f;
    [SerializeField] private float knockbackDuration = 0.25f;
    [SerializeField] private float knockbackDeceleration = 16f;
    [SerializeField, Range(1f, 3f)]
    private float maximumDamageKnockbackMultiplier = 1.5f;

    [Header("Damage Flash")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.red;

    [Header("Death")]
    [SerializeField] private float deathDespawnDelay = 2.5f;

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Attack Sound Effects")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip attack2Sound;

    [SerializeField, Range(0f, 1f)]
    private float attackVolume = 1f;

    [Header("Death Sound Effect")]
    [SerializeField] private AudioClip deathSound;

    [SerializeField, Range(0f, 1f)]
    private float deathVolume = 1f;

    [Header("References")]
    [SerializeField] private SkeletonController2D player;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Collider2D enemyCollider;

    private int currentHealth;

    private float startingX;
    private float patrolDirection = 1f;
    private float lastAttackTime = -999f;

    private Vector2 desiredVelocity;

    private float knockbackVelocityX;
    private float knockbackTimer;

    private bool isDead;
    private bool playerInRange;
    private bool nextChestAttackIsSecond;

    private Color originalSpriteColor = Color.white;
    private Coroutine damageFlashCoroutine;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int Attack1Hash =
        Animator.StringToHash("Attack1");

    private static readonly int Attack2Hash =
        Animator.StringToHash("Attack2");

    private static readonly int TakeHitHash =
        Animator.StringToHash("TakeHit");

    private static readonly int DieHash =
        Animator.StringToHash("Die");


    // --------------------------------------------------
    // INITIALIZATION
    // --------------------------------------------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (player == null)
            player = FindFirstObjectByType<SkeletonController2D>();

        startingX = transform.position.x;
        currentHealth = maxHealth;

        nextChestAttackIsSecond = false;

        rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        if (spriteRenderer != null)
        {
            originalSpriteColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning(
                name + " has no SpriteRenderer. Damage flash is unavailable.",
                this
            );
        }

        if (animator == null)
        {
            Debug.LogWarning(
                name + " has no Animator assigned or found.",
                this
            );
        }

        if (isFlying)
            rb.gravityScale = 0f;
    }


    // --------------------------------------------------
    // MAIN UPDATE
    // --------------------------------------------------

    private void Update()
    {
        if (isDead)
            return;

        // While being knocked back, temporarily suspend
        // chasing and patrolling so the AI cannot override it.
        if (knockbackTimer > 0f)
        {
            desiredVelocity = Vector2.zero;
            UpdateAnimation();
            return;
        }

        if (player == null)
        {
            player = FindFirstObjectByType<SkeletonController2D>();

            if (player == null)
            {
                desiredVelocity = Vector2.zero;
                UpdateAnimation();
                return;
            }
        }

        if (player.IsDead())
        {
            desiredVelocity = Vector2.zero;
            StopMovement();
            UpdateAnimation();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.transform.position
        );

        playerInRange = distance <= detectionRange;

        if (playerInRange)
            ChaseAndAttack();
        else
            Patrol();

        UpdateAnimation();
    }


    // --------------------------------------------------
    // PHYSICS MOVEMENT
    // --------------------------------------------------

    private void FixedUpdate()
    {
        if (isDead)
            return;

        if (knockbackTimer > 0f)
        {
            // Apply knockback directly to the Rigidbody2D.
            // AI movement is suspended during this period.
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

        if (isFlying)
        {
            rb.linearVelocity = desiredVelocity;
        }
        else
        {
            rb.linearVelocity = new Vector2(
                desiredVelocity.x,
                rb.linearVelocity.y
            );
        }
    }


    // --------------------------------------------------
    // PATROLLING
    // --------------------------------------------------

    private void Patrol()
    {
        float currentX = transform.position.x;

        if (currentX >= startingX + patrolDistance)
            patrolDirection = -1f;
        else if (currentX <= startingX - patrolDistance)
            patrolDirection = 1f;

        desiredVelocity = new Vector2(
            patrolDirection * patrolSpeed,
            0f
        );

        FaceDirection(patrolDirection);
    }


    // --------------------------------------------------
    // CHASING AND ATTACKING
    // --------------------------------------------------

    private void ChaseAndAttack()
    {
        Vector2 difference =
            (Vector2)player.transform.position -
            (Vector2)transform.position;

        if (difference.x < -0.01f)
            FaceDirection(-1f);
        else if (difference.x > 0.01f)
            FaceDirection(1f);

        if (IsPlayerInAttackRange())
        {
            StopMovement();

            if (Time.time >= lastAttackTime + attackCooldown)
                PerformAttack();

            return;
        }

        if (isFlying)
        {
            desiredVelocity = difference.normalized * chaseSpeed;
        }
        else
        {
            float horizontalDirection = Mathf.Sign(difference.x);

            desiredVelocity = new Vector2(
                horizontalDirection * chaseSpeed,
                0f
            );
        }
    }


    // --------------------------------------------------
    // ATTACK RANGE
    // --------------------------------------------------

    private bool IsPlayerInAttackRange()
    {
        if (player == null || player.IsDead())
            return false;

        Collider2D playerCollider =
            player.GetComponent<Collider2D>();

        if (playerCollider == null || enemyCollider == null)
        {
            Vector2 difference =
                (Vector2)player.transform.position -
                (Vector2)transform.position;

            if (isFlying)
                return difference.magnitude <= attackRange;

            return Mathf.Abs(difference.x) <= attackRange &&
                   Mathf.Abs(difference.y) <= verticalAttackTolerance;
        }

        Bounds enemyBounds = enemyCollider.bounds;
        Bounds playerBounds = playerCollider.bounds;

        float horizontalGap = Mathf.Max(
            0f,
            Mathf.Max(
                playerBounds.min.x - enemyBounds.max.x,
                enemyBounds.min.x - playerBounds.max.x
            )
        );

        float verticalGap = Mathf.Max(
            0f,
            Mathf.Max(
                playerBounds.min.y - enemyBounds.max.y,
                enemyBounds.min.y - playerBounds.max.y
            )
        );

        if (isFlying)
        {
            float separation = Mathf.Sqrt(
                horizontalGap * horizontalGap +
                verticalGap * verticalGap
            );

            return separation <= attackRange;
        }

        return horizontalGap <= attackRange &&
               verticalGap <= verticalAttackTolerance;
    }


    // --------------------------------------------------
    // MOVEMENT HELPERS
    // --------------------------------------------------

    private void StopMovement()
    {
        desiredVelocity = Vector2.zero;

        if (knockbackTimer > 0f)
            return;

        if (isFlying)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }
    }


    private void FaceDirection(float direction)
    {
        if (spriteRenderer == null ||
            Mathf.Abs(direction) < 0.01f)
        {
            return;
        }

        spriteRenderer.flipX = direction < 0f;
    }


    // --------------------------------------------------
    // ATTACKING
    // --------------------------------------------------

    private void PerformAttack()
    {
        if (player == null || isDead || player.IsDead())
            return;

        if (!IsPlayerInAttackRange())
            return;

        lastAttackTime = Time.time;

        int damage;
        AudioClip selectedAttackSound;

        if (enemyType == EnemyType.Chest && useSecondAttack)
        {
            if (nextChestAttackIsSecond)
            {
                damage = attack2Damage;

                selectedAttackSound =
                    attack2Sound != null
                    ? attack2Sound
                    : attackSound;

                TriggerAnimation("Attack2", Attack2Hash);
            }
            else
            {
                damage = attack1Damage;
                selectedAttackSound = attackSound;

                TriggerAnimation("Attack1", Attack1Hash);
            }

            nextChestAttackIsSecond = !nextChestAttackIsSecond;
        }
        else
        {
            damage = attack1Damage;
            selectedAttackSound = attackSound;

            TriggerAnimation("Attack1", Attack1Hash);
        }

        PlaySound(selectedAttackSound, attackVolume);

        Debug.Log(
            name + " is attacking the skeleton. Damage: " + damage
        );

        // Pass the attacker's position so the skeleton can
        // calculate the correct direction for knockback.
        player.TakeDamage(damage, transform.position);

        Debug.Log(
            name + " dealt " + damage +
            " damage to the skeleton. Player health: " +
            player.GetCurrentHealth() + "/" +
            player.GetMaxHealth()
        );
    }


    // --------------------------------------------------
    // AUDIO
    // --------------------------------------------------

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        if (audioSource == null)
        {
            Debug.LogWarning(
                name + " is missing its AudioSource component.",
                this
            );

            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }


    // --------------------------------------------------
    // TAKING DAMAGE
    // --------------------------------------------------

    public void TakeDamage(int damage)
    {
        if (isDead || damage <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        Debug.Log(
            enemyType + " took " + damage +
            " damage. Health: " + currentHealth +
            "/" + maxHealth
        );

        StartDamageFlash();

        TriggerAnimation("TakeHit", TakeHitHash);

        ApplyKnockback(damage);

        if (currentHealth <= 0)
            Die();
    }


    // --------------------------------------------------
    // DAMAGE FLASH
    // --------------------------------------------------

    private void StartDamageFlash()
    {
        if (spriteRenderer == null)
            return;

        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = null;
        }

        damageFlashCoroutine = StartCoroutine(FlashDamageColor());
    }


    private IEnumerator FlashDamageColor()
    {
        if (spriteRenderer == null)
            yield break;

        spriteRenderer.color = damageFlashColor;

        yield return new WaitForSeconds(damageFlashDuration);

        if (spriteRenderer != null)
            spriteRenderer.color = originalSpriteColor;

        damageFlashCoroutine = null;
    }


    // --------------------------------------------------
    // KNOCKBACK
    // --------------------------------------------------

    private void ApplyKnockback(int damage)
    {
        if (rb == null || isDead)
            return;

        float direction = 1f;

        if (player != null)
        {
            float difference =
                transform.position.x - player.transform.position.x;

            if (Mathf.Abs(difference) > 0.01f)
            {
                // Push the enemy away from the player.
                direction = Mathf.Sign(difference);
            }
            else if (spriteRenderer != null)
            {
                // If they overlap horizontally, use the
                // enemy's facing direction as a fallback.
                direction = spriteRenderer.flipX ? 1f : -1f;
            }
        }

        float damageMultiplier = Mathf.Clamp(
            damage / (float)Mathf.Max(1, attack1Damage),
            1f,
            maximumDamageKnockbackMultiplier
        );

        knockbackVelocityX =
            direction * knockbackForce * damageMultiplier;

        knockbackTimer = knockbackDuration;

        // Clear any previous horizontal motion before
        // the knockback velocity is applied in FixedUpdate.
        rb.linearVelocity = new Vector2(
            0f,
            rb.linearVelocity.y
        );

        Debug.Log(
            name + " received knockback. Direction: " +
            direction + ", force: " + knockbackVelocityX
        );
    }


    // --------------------------------------------------
    // DEATH
    // --------------------------------------------------

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        desiredVelocity = Vector2.zero;
        knockbackVelocityX = 0f;
        knockbackTimer = 0f;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }

        // Keep the collider enabled so the enemy continues
        // colliding with the floor during its death animation.

        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalSpriteColor;

        if (animator != null)
        {
            ResetTriggerIfPresent("Attack1");
            ResetTriggerIfPresent("Attack2");
            ResetTriggerIfPresent("TakeHit");

            TriggerAnimation("Die", DieHash);
        }

        PlaySound(deathSound, deathVolume);

        Debug.Log(enemyType + " defeated!");

        float destroyDelay = deathDespawnDelay;

        if (deathSound != null)
        {
            destroyDelay = Mathf.Max(
                destroyDelay,
                deathSound.length
            );
        }

        Destroy(gameObject, destroyDelay);
    }


    private void ResetTriggerIfPresent(string parameterName)
    {
        if (animator == null)
            return;

        if (HasParameter(
            parameterName,
            AnimatorControllerParameterType.Trigger))
        {
            // Reset by the validated parameter name.
            animator.ResetTrigger(parameterName);
        }
    }


    // --------------------------------------------------
    // ANIMATIONS
    // --------------------------------------------------

    private void UpdateAnimation()
    {
        if (animator == null || isDead || rb == null)
            return;

        if (HasParameter(
            "Speed",
            AnimatorControllerParameterType.Float))
        {
            float animationSpeed = Mathf.Abs(rb.linearVelocity.x);

            if (isFlying)
                animationSpeed = rb.linearVelocity.magnitude;

            animator.SetFloat(SpeedHash, animationSpeed);
        }
    }


    private void TriggerAnimation(string parameterName, int hash)
    {
        if (animator == null)
            return;

        if (HasParameter(
            parameterName,
            AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(hash);

            Debug.Log(
                name + " triggered animation parameter: " +
                parameterName
            );
        }
        else
        {
            Debug.LogWarning(
                name + "'s Animator is missing the Trigger parameter '" +
                parameterName + "'. Check the Animator Controller.",
                this
            );
        }
    }


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
    // HEALTH ACCESSORS
    // --------------------------------------------------

    public int GetCurrentHealth()
    {
        return currentHealth;
    }


    public int GetMaxHealth()
    {
        return maxHealth;
    }


    public bool IsDead()
    {
        return isDead;
    }


    // --------------------------------------------------
    // DEBUG VISUALIZATION
    // --------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        float centerX = Application.isPlaying
            ? startingX
            : transform.position.x;

        Gizmos.color = Color.green;

        Gizmos.DrawLine(
            new Vector3(
                centerX - patrolDistance,
                transform.position.y,
                transform.position.z
            ),
            new Vector3(
                centerX + patrolDistance,
                transform.position.y,
                transform.position.z
            )
        );
    }
}
