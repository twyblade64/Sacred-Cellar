using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton class used for scene transition handling
/// </summary>
public class SceneManager : MonoBehaviour {
    // Static reference
    private static SceneManager sceneManager;

    // Animator containing scene transition
    private Animator animator;

    // Scene to be loaded between two scenes
    public string bufferScene;

    // Scene to load
    private string sceneToLoad;

    // Started async-scene load flag
    private bool loadingScene = false;

    // Finished async-scene load flag
    private bool sceneLoaded = false;


    public bool CanPause {
        get {
            return animator.GetCurrentAnimatorStateInfo(0).IsName("OnGame") && !loadingScene;
        }
    }

    /// <summary>
    /// Init singleton references
    /// </summary>
    public void Awake() {
        if (sceneManager == null) {
            sceneManager = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            if (sceneManager != this) {
                Destroy(gameObject);
            }
        }
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Singleton acces
    /// </summary>
    /// <returns>Instance of SceneManager class</returns>
    public static SceneManager GetInstance() {
        return sceneManager;
    }

    /// <summary>
    /// Check for async load finish
    /// </summary>
    public void Update() {
        if (loadingScene) {
            if (sceneLoaded) {
                loadingScene = false;
                sceneLoaded = false;
            }
        }
    }

    /// <summary>
    /// Change to another scene
    /// </summary>
    /// <param name="sceneName">The name of the scene to change to</param>
    /// <param name="asyncLoad">Enable asynchronous loading. Also executes scene transition.</param>
    public void ChangeScene(string sceneName, bool asyncLoad) {
        if (!loadingScene) {
            if (!asyncLoad)
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            else {
                loadingScene = true;
                sceneToLoad = sceneName;
                animator.SetTrigger("Load Scene");
            }
        }
    }

    /// <summary>
    /// One parameter ChangeScene method. Changes to another scene with asynchronous load enabled.
    /// </summary>
    /// <param name="sceneName">The name of the scene to change to</param>
    public void ChangeScene(string sceneName) {
        ChangeScene(sceneName, true);
    }

    /// <summary>
    /// Coroutine for scene loading.
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadNewScene() {
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneToLoad);
        while (!async.isDone)
            yield return null;

        sceneLoaded = true;
        animator.SetTrigger("Scene Loaded");
    }

    /// <summary>
    /// Start loading asynchronously the next scene
    /// </summary>
    public void StartSceneLoad() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(bufferScene);
        StartCoroutine(LoadNewScene());
    }

    /// <summary>
    /// Exit the game
    /// </summary>
    public void ExitApp() {
        Application.Quit();
    }
}
