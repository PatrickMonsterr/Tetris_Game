using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tetris.Core
{
    /// <summary>
    /// Specifies the player side.
    /// </summary>
    public enum PlayerType
    {
        Left,
        Right
    }

    /// <summary>
    /// Manages the core Tetris game logic for one player.
    /// Responsible for spawning falling blocks, tracking the score,
    /// and clearing full lines.
    /// </summary>
    public class TetrisManager : MonoBehaviour
    {
        /// <summary>
        /// Event triggered when the next block prefab changes.
        /// </summary>
        public static System.Action<PlayerType, GameObject> OnNextBlockPrefabChanged;

        /// <summary>
        /// Event triggered when the score changes.
        /// </summary>
        public static System.Action<PlayerType, int> OnScoreChanged;

        /// <summary>
        /// Event triggered when a player's game is over.
        /// </summary>
        public static System.Action<PlayerType> OnGameOver;

        /// <summary>
        /// Event triggered when both players have lost.
        /// </summary>
        public static System.Action OnBothPlayersLost;

        /// <summary>
        /// The score of the left player.
        /// </summary>
        public static int LeftScore { get; private set; }

        /// <summary>
        /// The score of the right player.
        /// </summary>
        public static int RightScore { get; private set; }

        /// <summary>
        /// Indicates whether the left player's game is over.
        /// </summary>
        public static bool LeftGameOver { get; private set; }

        /// <summary>
        /// Indicates whether the right player's game is over.
        /// </summary>
        public static bool RightGameOver { get; private set; }

        #region Inspector Fields

        [Header("Boundary Settings")]
        [Tooltip("Left boundary for play area.")]
        public Transform LeftBoundary;

        [Tooltip("Right boundary for play area.")]
        public Transform RightBoundary;

        [Header("Player Settings")]
        [Tooltip("Select the player side (Left or Right).")]
        public PlayerType PlayerSide;

        [Header("Block Settings")]
        [Tooltip("Spawn point where new blocks will appear.")]
        public Transform SpawnPoint;

        [Tooltip("List of block prefabs to spawn (2-3 types are enough).")]
        public GameObject[] BlockPrefabs;

        #endregion

        // Private fields
        private GameObject _nextBlockPrefab;

        /// <summary>
        /// Initializes game state.
        /// </summary>
        private void Start()
        {
            LeftGameOver = false;
            RightGameOver = false;
            LeftScore = 0;
            RightScore = 0;
            PrepareNextBlock();
            SpawnBlock();
        }

        /// <summary>
        /// Spawns a falling block at the spawn point and prepares the next block.
        /// If a block already occupies the spawn point, the game is considered over for that player.
        /// </summary>
        public void SpawnBlock()
        {
            if ((PlayerSide == PlayerType.Left && LeftGameOver) ||
                (PlayerSide == PlayerType.Right && RightGameOver))
            {
                return;
            }

            if (_nextBlockPrefab != null && SpawnPoint != null)
            {
                Collider2D hit = Physics2D.OverlapBox(
                    SpawnPoint.position,
                    new Vector2(0.3f, 0.3f),
                    0f,
                    LayerMask.GetMask("Default")
                );

                string ownTag = PlayerSide == PlayerType.Left ? "Block" : "RightBlock";
                if (hit != null && hit.CompareTag(ownTag))
                {
                    if (PlayerSide == PlayerType.Left)
                    {
                        LeftGameOver = true;
                    }
                    else
                    {
                        RightGameOver = true;
                    }

                    OnGameOver?.Invoke(PlayerSide);

                    if (LeftGameOver && RightGameOver)
                    {
                        OnBothPlayersLost?.Invoke();
                    }
                    return;
                }

                GameObject blockInstance = Instantiate(_nextBlockPrefab, SpawnPoint.position, Quaternion.identity);
                var inputHandler = blockInstance.GetComponent<BlockInputHandler>();
                if (inputHandler != null)
                {
                    inputHandler.Spawner = this;
                }
            }

            PrepareNextBlock();
        }

        /// <summary>
        /// Randomly selects a block prefab from the list to be the next block.
        /// Triggers an event to update the UI preview.
        /// </summary>
        private void PrepareNextBlock()
        {
            if (BlockPrefabs != null && BlockPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, BlockPrefabs.Length);
                _nextBlockPrefab = BlockPrefabs[randomIndex];
                OnNextBlockPrefabChanged?.Invoke(PlayerSide, _nextBlockPrefab);
            }
        }

        /// <summary>
        /// Gets the sprite for the next block (for UI preview).
        /// Assumes that the prefab has a SpriteRenderer in one of its children.
        /// </summary>
        public Sprite NextBlockSprite
        {
            get
            {
                if (_nextBlockPrefab != null)
                {
                    var spriteRenderer = _nextBlockPrefab.GetComponentInChildren<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        return spriteRenderer.sprite;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Adds points to the player's score and triggers the score update event.
        /// </summary>
        /// <param name="points">The number of points to add.</param>
        public void AddScore(int points)
        {
            if (PlayerSide == PlayerType.Left)
            {
                LeftScore += points;
            }
            else
            {
                RightScore += points;
            }
            OnScoreChanged?.Invoke(PlayerSide, PlayerSide == PlayerType.Left ? LeftScore : RightScore);
        }

        /// <summary>
        /// Checks for and clears full lines of blocks.
        /// Blocks are cleared when the number of blocks in a line reaches the required threshold.
        /// </summary>
        public void CheckAndClearLines()
        {
            float blockHeight = 0.32f;
            int requiredBlocks = 12;
            string tagToUse = PlayerSide == PlayerType.Left ? "Block" : "RightBlock";

            var allBlocks = GameObject.FindGameObjectsWithTag(tagToUse)
                .Where(block =>
                    block.transform.position.x >= LeftBoundary.position.x &&
                    block.transform.position.x <= RightBoundary.position.x)
                .ToList();

            if (allBlocks.Count == 0)
            {
                return;
            }

            Dictionary<int, List<GameObject>> lines = new Dictionary<int, List<GameObject>>();
            foreach (var block in allBlocks)
            {
                int lineNumber = Mathf.RoundToInt(block.transform.position.y / blockHeight);
                if (!lines.ContainsKey(lineNumber))
                {
                    lines.Add(lineNumber, new List<GameObject>());
                }
                lines[lineNumber].Add(block);
            }

            List<int> fullLines = lines
                .Where(entry => entry.Value.Count >= requiredBlocks)
                .Select(entry => entry.Key)
                .ToList();

            if (fullLines.Count == 0)
            {
                return;
            }

            // Disable colliders for blocks in full lines.
            foreach (int line in fullLines)
            {
                foreach (var block in lines[line])
                {
                    Collider2D collider = block.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        collider.enabled = false;
                    }
                }
            }

            // Calculate new positions for blocks above the cleared lines.
            Dictionary<GameObject, Vector2> blocksToMove = new Dictionary<GameObject, Vector2>();
            foreach (var block in allBlocks)
            {
                if (block == null)
                {
                    continue;
                }
                int currentLine = Mathf.RoundToInt(block.transform.position.y / blockHeight);
                if (!fullLines.Contains(currentLine))
                {
                    int linesBelow = fullLines.Count(line => line < currentLine);
                    if (linesBelow > 0)
                    {
                        float newY = block.transform.position.y - (linesBelow * blockHeight);
                        blocksToMove.Add(block, new Vector2(block.transform.position.x, newY));
                    }
                }
            }

            // Destroy blocks in the full lines.
            foreach (int line in fullLines)
            {
                foreach (var block in lines[line])
                {
                    Destroy(block);
                }
            }

            // Move remaining blocks downward.
            foreach (var kvp in blocksToMove)
            {
                GameObject block = kvp.Key;
                Vector2 newPos = kvp.Value;
                if (block != null)
                {
                    Collider2D collider = block.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                    Rigidbody2D rb = block.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.bodyType = RigidbodyType2D.Dynamic;
                    }
                    block.transform.position = newPos;
                }
            }

            Physics2D.SyncTransforms();
            AddScore(100 * fullLines.Count);
        }
    }
}
