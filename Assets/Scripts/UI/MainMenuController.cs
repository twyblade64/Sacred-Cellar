using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Control the main menu interface and setup the starting player data
/// </summary>
public class MainMenuController : MonoBehaviour {
    public TextMeshProUGUI scoreNumber;

	void Start () {
#if UNITY_STANDALONE_WIN
        Screen.SetResolution(896, 504, false);
#endif
        scoreNumber.SetText(System.String.Format("{0:000000}", PersistanceManager.GetInstance().GetMaxScore()));
        PersistanceManager.GetInstance().ResetPlayerScore();
        PersistanceManager.GetInstance().ResetPlayerStats();
    }
	
    public void SetControlScheme(int mode) {
        PersistanceManager.GetInstance().controlScheme = mode;
    }

    public void ExitGame() {
        Application.Quit();
    }
}
