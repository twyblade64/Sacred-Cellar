using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

/// <summary>
/// Small class for testing Unity adds.
/// </summary>
public class AddsTestController : MonoBehaviour {

    public void ShowRewardedVideo() {
        SoundManager.GetInstance().PauseMusic();
        ShowOptions options = new ShowOptions();
        options.resultCallback = HandleShowResult;
        Advertisement.Show("rewardedVideo", options);
    }

    void HandleShowResult(ShowResult result) {
        /*if (result == ShowResult.Finished) {
            Debug.Log("Video completed - Offer a reward to the player");

        } else if (result == ShowResult.Skipped) {
            Debug.LogWarning("Video was skipped - Do NOT reward the player");

        } else if (result == ShowResult.Failed) {
            Debug.LogError("Video failed to show");
        }*/

        // I wasn't planning of big monetization plans for the game anyway
        SceneManager.GetInstance().ChangeScene("Main Menu Scene", true);
    }
}
