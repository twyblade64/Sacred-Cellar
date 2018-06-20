using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for all UI seen during gameplay
/// Also manages some of the general game logic.
/// </summary>
public class GameUIController : MonoBehaviour {
    // Player reference
    private PlayerController player;

    // Maximum player hitpoints for the healthbar
    private float playerHitpointsMax;

    // Current player hitpoints for the healthbar
    private float playerHitpointsCurrent;

    // Slider used to show the current player health
    public Slider healthbar;

    // Text component to show current player coins
    public TextMeshProUGUI coinText;

    // Text component to show current player score
    public TextMeshProUGUI scoreText;

    // AudioClip for pausing event
    public AudioClip pauseClip;

    // AudioClip for unpausing event
    public AudioClip unpauseClip;

    // Rectangle containing the healthbar
    private RectTransform healthbarRect;

    // Canvas shown when the game is paused
    [SerializeField]
    private Canvas pauseCanvas;

    // Game paused flag
    private bool isPaused;

    // Changing scene flag
    private bool changingScene;

    /// <summary>
    /// Init references and set some application configuration settings
    /// </summary>
    void Awake() {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled to set a target framerate
        Application.targetFrameRate = 30;
        changingScene = false;
        healthbarRect = healthbar.GetComponent<RectTransform>();
    }


    /// <summary>
    /// Find component references and initialize variables
    /// </summary>
    void Start () {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        playerHitpointsCurrent = player.hitpoints;
        playerHitpointsMax = playerHitpointsCurrent;
        healthbar.maxValue = playerHitpointsMax;
        healthbar.minValue = 0;
        coinText.SetText("" + player.coins);
        SoundManager.GetInstance().Load(pauseClip, 1);
        SoundManager.GetInstance().Load(unpauseClip, 1);
    }
	
	/// <summary>
    /// Update UI and check for game state
    /// </summary>
	void Update () {
        // Update healthbar
        if (healthbar) {
            healthbar.value = player.hitpoints;
            healthbar.maxValue = player.hitpointsMax;
            healthbarRect.sizeDelta = new Vector2(player.hitpointsMax*30, healthbarRect.sizeDelta.y);
        }

        // Update coins text
        coinText.SetText("" + player.coins);

        // Update score text
        scoreText.SetText(System.String.Format("{0:000000}", PersistanceManager.GetInstance().playerScore));

        // Check for player death
        if (player.hitpoints <= 0 && !changingScene) {
            // Save stats and go to death screen
            PersistanceManager.GetInstance().ResetPlayerScore();
            PersistanceManager.GetInstance().ResetPlayerStats();
            SceneManager.GetInstance().ChangeScene("Death Screen", true);
            changingScene = true;
        }
    }

    /// <summary>
    /// Pause the game
    /// </summary>
    private void PauseGame() {
        if (SceneManager.GetInstance().CanPause) {
            Debug.Log("Pausing the game...");
            Time.timeScale = 0;
            SoundManager.GetInstance().PauseMusic();
            pauseCanvas.gameObject.SetActive(true);
            isPaused = true;
        }
    }

    /// <summary>
    /// Unpause the game
    /// </summary>
    private void UnPauseGame() {
        Debug.Log("Unpausing game...");
        Time.timeScale = 1;
        SoundManager.GetInstance().UnPauseMusic();
        pauseCanvas.gameObject.SetActive(false);
        isPaused = false;
    }

    /// <summary>
    /// Switch between pause and unpause
    /// </summary>
    public void SwitchPauseGame() {
        if (isPaused) {
            SoundManager.GetInstance().Play(pauseClip);
            UnPauseGame();
        } else {
            SoundManager.GetInstance().Play(unpauseClip);
            PauseGame();
        }
    }

    /// <summary>
    /// Pause game when out of focus
    /// </summary>
    /// <param name="hasFocus"></param>
    void OnApplicationFocus(bool hasFocus) {
        if (!hasFocus && !isPaused)
            PauseGame();

    }

    /// <summary>
    /// Pause game when application pause
    /// </summary>
    /// <param name="pauseStatus"></param>
    void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus && !isPaused)
            PauseGame();
    }

    /// <summary>
    /// Change the control scheme
    /// </summary>
    /// <param name="mode"></param>
    public void SetControlScheme(int mode) {
        PersistanceManager.GetInstance().controlScheme = mode;
    }
}
