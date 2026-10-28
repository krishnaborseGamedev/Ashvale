using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Put this on an empty GameObject called "GameEndUI".
/// ShowWin()  is called by the key.
/// ShowLose() is called by PlayerHealth when the player dies.
/// It shows the right panel and freezes the game.
/// </summary>
public class GameEndUI : MonoBehaviour
{
    // Lets other scripts find this one easily.
    public static GameEndUI Instance { get; private set; }

    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private float showDelay = 1f;   // seconds to wait before the panel appears

    private bool _ended;   // makes sure the game can only end one time

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Make sure the game starts running normally (also after Restart).
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void ShowWin()
    {
        if (_ended) return;
        _ended = true;
        StartCoroutine(EndGame(winPanel));
    }

    public void ShowLose()
    {
        if (_ended) return;
        _ended = true;
        StartCoroutine(EndGame(losePanel));
    }

    private IEnumerator EndGame(GameObject panel)
    {
        yield return new WaitForSecondsRealtime(showDelay);

        if (panel != null) panel.SetActive(true);

        // Freeze the game and give the mouse back so buttons can be clicked.
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ---- Called by the buttons ----

    public void RestartGame()
    {
        Time.timeScale = 1f;   // unfreeze BEFORE reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}