using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Tetris.Core;

/// <summary>
/// Manages the UI for the Tetris game, including score updates, next block previews, and game over display.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region Inspector Fields

    [SerializeField] private TextMeshProUGUI leftScoreText;
    [SerializeField] private TextMeshProUGUI rightScoreText;
    [SerializeField] private RectTransform leftNextBlockContainer;
    [SerializeField] private RectTransform rightNextBlockContainer;
    [SerializeField] private TextMeshProUGUI gameOverText;

    #endregion
    #region Private Fields

    private bool _leftPlayerLost;
    private bool _rightPlayerLost;

    #endregion

    #region Event Subscriptions

    private void OnEnable()
    {
        TetrisManager.OnScoreChanged += UpdateScore;
        TetrisManager.OnNextBlockPrefabChanged += UpdateNextBlockPreview;
        TetrisManager.OnGameOver += HandleGameOver;
        TetrisManager.OnBothPlayersLost += ShowFinalResult;
    }

    private void OnDisable()
    {
        TetrisManager.OnScoreChanged -= UpdateScore;
        TetrisManager.OnNextBlockPrefabChanged -= UpdateNextBlockPreview;
        TetrisManager.OnGameOver -= HandleGameOver;
        TetrisManager.OnBothPlayersLost -= ShowFinalResult;
    }
    #endregion
    #region UI Update Methods
    /// <summary>
    /// Updates the score UI for the specified player.
    /// </summary>
    /// <param name="player">The player whose score is updated.</param>
    /// <param name="newScore">The new score value.</param>
    private void UpdateScore(PlayerType player, int newScore)
    {
        if (player == PlayerType.Left)
        {
            leftScoreText.text = $"Left Player: {newScore}";
        }
        else
        {
            rightScoreText.text = $"Right Player: {newScore}";
        }
    }
    /// <summary>
    /// Handles the game over event by setting flags for the player who lost.
    /// </summary>
    /// <param name="playerWhoLost">The player who lost the game.</param>
    private void HandleGameOver(PlayerType playerWhoLost)
    {
        if (playerWhoLost == PlayerType.Left)
        {
            _leftPlayerLost = true;
        }
        else
        {
            _rightPlayerLost = true;
        }
    }

    /// <summary>
    /// Displays the final result (game over screen) based on the players' scores.
    /// </summary>
    private void ShowFinalResult()
    {
        int leftScore = TetrisManager.LeftScore;
        int rightScore = TetrisManager.RightScore;

        string resultText;
        if (leftScore > rightScore)
        {
            resultText = "Left Player Wins!";
        }
        else if (rightScore > leftScore)
        {
            resultText = "Right Player Wins!";
        }
        else
        {
            resultText = "Tie Game!";
        }

        gameOverText.text = $"{resultText}";
        gameOverText.gameObject.SetActive(true);
    }
    #endregion
    #region Next Block Preview

    /// <summary>
    /// Updates the next block preview in the UI for the specified player.
    /// </summary>
    /// <param name="player">The player for which to update the preview.</param>
    /// <param name="nextBlockPrefab">The prefab of the next block.</param>
    private void UpdateNextBlockPreview(PlayerType player, GameObject nextBlockPrefab)
    {
        RectTransform targetContainer = (player == PlayerType.Left) ? leftNextBlockContainer : rightNextBlockContainer;

        foreach (Transform child in targetContainer)
        {
            Destroy(child.gameObject);
        }

        GameObject uiContainer = new GameObject("BlockPreview");
        uiContainer.transform.SetParent(targetContainer, false);
        uiContainer.AddComponent<RectTransform>();

        List<Transform> blockParts = new List<Transform>();
        foreach (Transform child in nextBlockPrefab.transform)
        {
            blockParts.Add(child);
        }

        foreach (Transform part in blockParts)
        {
            GameObject uiPart = new GameObject("BlockPart");
            uiPart.transform.SetParent(uiContainer.transform, false);

            Image image = uiPart.AddComponent<Image>();
            SpriteRenderer partSprite = part.GetComponent<SpriteRenderer>();
            if (partSprite != null)
            {
                image.sprite = partSprite.sprite;
                image.color = partSprite.color;
            }

            RectTransform rect = uiPart.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(part.localPosition.x * 160f, part.localPosition.y * 160f);
            rect.sizeDelta = new Vector2(50f, 50f);
        }

        RectTransform containerRect = uiContainer.GetComponent<RectTransform>();
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.localScale = Vector3.one;
    }
    #endregion
}