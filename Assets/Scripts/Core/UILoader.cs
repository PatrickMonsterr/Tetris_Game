using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads the UI scene additively when the game starts.
/// </summary>
public class UILoader : MonoBehaviour
{
    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        // Initialization code can be added here if needed.
    }

    /// <summary>
    /// Called before the first frame update.
    /// Loads the UI scene additively.
    /// </summary>
    private void Start()
    {
        SceneManager.LoadScene("GameUI", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    private void Update()
    {
        // Update logic can be added here if needed.
    }
}
