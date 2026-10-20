using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent (typeof(SpriteRenderer))]
[RequireComponent (typeof(BoxCollider2D))]
public class Player : MonoBehaviour
{
    public bool isPossessed; // We can use this to check if player can complete level
    private new Rigidbody2D rigidbody;
    private SpriteRenderer sprite;
    [SerializeField] private Vector2 movementInput;
    #region SkeletonStats

    #endregion
    #region GhostStats
    public float ghostSpeed = 10f;
    #endregion
    public enum PlayerState
    {
        Skeleton,
        Ghost,
        Human
    }
    [SerializeField] private PlayerState activeState;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        ChangeState(PlayerState.Skeleton);
        isPossessed = true;
    }

    void Update()
    {
        movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space)) ChangeState(PlayerState.Ghost);
        else if (Input.GetKeyDown(KeyCode.KeypadEnter)) ChangeState(PlayerState.Skeleton);

        if (rigidbody.linearVelocityX < 0) sprite.flipX = true;
        else if (rigidbody.linearVelocityX > 0) sprite.flipX = false;

        if (activeState == PlayerState.Ghost && (movementInput.x != 0 || movementInput.y != 0)) GhostMovement(movementInput);
        else if (activeState == PlayerState.Ghost && (movementInput.x == 0 || movementInput.y == 0)) rigidbody.linearVelocity = Vector2.zero;
        else if (activeState != PlayerState.Ghost && (movementInput.x != 0 || movementInput.y != 0)) SkeletonMovement(movementInput);
        else if (activeState != PlayerState.Ghost && (movementInput.x == 0 || movementInput.y == 0)) /* Area for velocity resetting if needed, otherwise remove */;
    }

    public void ChangeState(PlayerState newState)
    {
        if (activeState == newState) return;
        activeState = newState;
        switch (newState)
        {
            case PlayerState.Skeleton:
                rigidbody.gravityScale = 1;
                rigidbody.excludeLayers = LayerMask.GetMask("SkeleIgnore");
                isPossessed = true; // Set to false if this form can't beat the level
                break;
            case PlayerState.Ghost:
                rigidbody.gravityScale = 0;
                rigidbody.excludeLayers = LayerMask.GetMask("GhostIgnore");
                isPossessed = false;
                break;
            case PlayerState.Human:
                rigidbody.gravityScale = 1;
                rigidbody.excludeLayers = LayerMask.NameToLayer("");
                isPossessed = true;
                break;
            default:
                ChangeState(PlayerState.Skeleton);
                break;
        }
    }

    public void GhostMovement(Vector2 movement)
    {
        rigidbody.linearVelocity = (movement * ghostSpeed);
    }

    public void SkeletonMovement(Vector2 movement)
    {

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("SoulFragment"))
        {
            GameManager.AddSouls();
            Destroy(collision.gameObject);
        }
    }
}
