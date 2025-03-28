using Tetris.Core;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles block input for horizontal movement and falling, with collision checks.
/// The block moves horizontally by a fixed step and falls using physics.
/// </summary>
public class BlockInputHandler : MonoBehaviour
{
    #region Inspector Fields

    [Header("Collision Settings")]
    [Tooltip("Size of the block for collision checks")]
    [SerializeField] private Vector2 _collisionCheckSize = new Vector2(0.3f, 0.3f);

    [Header("Movement Settings")]
    [Tooltip("Interpolation speed for smooth horizontal movement.")]
    [SerializeField] private float _moveSpeed = 10f;

    [Header("Falling Settings")]
    [Tooltip("Falling speed in units per second.")]
    [SerializeField] private float _fallSpeed = 2f;

    #endregion

    // Horizontal movement step (constant value)
    private float _moveStep = 0.32f;

    // Input actions wrapper
    private InputActions _inputActions;
    // Target position for smooth interpolation
    private Vector3 _targetPosition;
    // Flags for tracking player side and input subscription status
    private bool _isRightPlayer = false;
    private bool _isSubscribed = false;
    // Flag indicating whether the block has landed
    private bool _landed = false;

    // Reference to the Tetris manager (spawner)
    public TetrisManager Spawner;

    #region MonoBehaviour Methods

    /// <summary>
    /// Initializes input actions and sets the initial target position.
    /// </summary>
    private void Awake()
    {
        _inputActions = new InputActions();
        _targetPosition = transform.position;
    }

    /// <summary>
    /// Enables input actions.
    /// </summary>
    private void OnEnable() => _inputActions.Enable();

    /// <summary>
    /// Subscribes to the correct move input based on the player's side.
    /// </summary>
    private void Start()
    {
        if (Spawner != null && Spawner.PlayerSide == PlayerType.Right)
        {
            _isRightPlayer = true;
            _inputActions.Controls.MoveRights.performed += HandleMove;
        }
        else
        {
            _isRightPlayer = false;
            _inputActions.Controls.Move.performed += HandleMove;
        }
        _isSubscribed = true;
    }

    /// <summary>
    /// Unsubscribes from input events and disables input actions.
    /// </summary>
    private void OnDisable()
    {
        if (_isSubscribed)
        {
            if (_isRightPlayer)
            {
                _inputActions.Controls.MoveRights.performed -= HandleMove;
            }
            else
            {
                _inputActions.Controls.Move.performed -= HandleMove;
            }
        }
        _inputActions.Disable();
    }

    /// <summary>
    /// Smoothly interpolates the block to the target horizontal position and applies falling movement.
    /// </summary>
    private void Update()
    {
        // Smooth horizontal movement
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.Lerp(transform.position.x, _targetPosition.x, Time.deltaTime * _moveSpeed);
        transform.position = new Vector3(newPosition.x, transform.position.y, transform.position.z);

        // Apply falling movement using physics if the block hasn't landed
        if (!_landed)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -_fallSpeed);
            }
        }
    }

    /// <summary>
    /// Draws a gizmo to visualize the target position and collider bounds in the editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(_targetPosition, collider.bounds.size);
        }
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// Handles horizontal move input.
    /// Moves the block left or right by a fixed step if no collision with a wall is detected.
    /// </summary>
    /// <param name="context">Input action callback context.</param>
    private void HandleMove(InputAction.CallbackContext context)
    {
        if (_landed)
        {
            return;
        }

        Vector2 moveInput = context.ReadValue<Vector2>();
        int moveDirectionX = (int)moveInput.x; // -1 for left, +1 for right

        if (moveDirectionX != 0)
        {
            Vector3 newPos = _targetPosition + new Vector3(moveDirectionX * _moveStep, 0f, 0f);

            // Use the block's collider size for collision checking
            Collider2D blockCollider = GetComponent<Collider2D>();
            Vector2 checkSize = blockCollider.bounds.size * 0.9f;

            Collider2D hit = Physics2D.OverlapBox(newPos, checkSize, 0f, LayerMask.GetMask("Default"));

            // Update target position if no wall is hit
            if (hit == null || !hit.CompareTag("Wall"))
            {
                _targetPosition = newPos;
            }
        }
    }

    #endregion

    #region Collision Handling

    /// <summary>
    /// Called when the block collides with another collider.
    /// Stops falling and snaps the block to the grid if it collides with Ground, Cube, or RightBlock.
    /// Then notifies the spawner.
    /// </summary>
    /// <param name="collision">Collision data.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_landed && (collision.gameObject.CompareTag("Ground") ||
                         collision.gameObject.CompareTag("Cube") ||
                         collision.gameObject.CompareTag("RightBlock")))
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            float snapY = Mathf.Round(transform.position.y * 100f) / 100f;
            transform.position = new Vector3(transform.position.x, snapY, transform.position.z);

            _landed = true;
            Physics2D.SyncTransforms();
            Invoke(nameof(NotifySpawner), 0.1f);
        }
    }

    /// <summary>
    /// Notifies the spawner that the block has landed, triggering line clearing and block spawning.
    /// </summary>
    private void NotifySpawner()
    {
        if (Spawner != null &&
            !((Spawner.PlayerSide == PlayerType.Left && TetrisManager.LeftGameOver) ||
              (Spawner.PlayerSide == PlayerType.Right && TetrisManager.RightGameOver)))
        {
            Spawner.CheckAndClearLines();
            Spawner.SpawnBlock();
        }
    }

    #endregion
}
