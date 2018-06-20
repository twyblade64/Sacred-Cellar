using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for the shop interface
/// </summary>
public class ShopController : MonoBehaviour {
    public int selectedItemIndex;
    public GameObject[] imagesItems;
    public GameObject imageSoldout;
    public TextMeshProUGUI textArticleName;
    public TextMeshProUGUI textArticleDescription;
    public TextMeshProUGUI textArticlePrice;

    public AudioClip selectClip;
    public AudioClip purchaseClip;
    public AudioClip errorClip;

	/// <summary>
    /// Initialize sounds and setup shop UI
    /// </summary>
	void Start () {
        SoundManager.GetInstance().Load(selectClip, 1);
        SoundManager.GetInstance().Load(purchaseClip, 1);
        SoundManager.GetInstance().Load(errorClip, 1);
        UpdateShop();
    }

    /// <summary>
    /// Show the next item in the shop catalogue
    /// </summary>
    public void ShowNextItem() {
        SoundManager.GetInstance().Play(selectClip);
        selectedItemIndex = (selectedItemIndex + 1) % imagesItems.Length;
        UpdateShop();
    }

    /// <summary>
    /// Show the previous item in the shop catalogue
    /// </summary>
    public void ShowPreviousItem() {
        SoundManager.GetInstance().Play(selectClip);
        selectedItemIndex = (selectedItemIndex + imagesItems.Length - 1) % imagesItems.Length;
        UpdateShop();
    }

    /// <summary>
    /// Update shop UI to reflect the current selected item, aviability and price
    /// </summary>
    public void UpdateShop() {
        PersistanceManager pm = PersistanceManager.GetInstance();

        // Deactivate all items images
        for (int i = 0; i < imagesItems.Length; i++) {
            imagesItems[i].SetActive(false);
        }

        // Show selected item image and description
        imagesItems[selectedItemIndex].SetActive(true);
        switch(selectedItemIndex) {
            case 0: // Refill health
                textArticleName.SetText("Health Potion");
                textArticleDescription.SetText("110% organic remedy. Don't ask for the 10%");
                textArticlePrice.SetText("x2");
                imageSoldout.SetActive(false);
                break;
            case 1: // Health upgrade
                textArticleName.SetText("Health Upgrade");
                textArticleDescription.SetText("Found it floating somewhere and decided to sell it.");
                textArticlePrice.SetText("x" + GetNextLevelPrice(pm.playerStatHealth));
                imageSoldout.SetActive(pm.playerStatHealth >= 5);
                break;
            case 2: // Strength upgrade
                textArticleName.SetText("Attack Upgrade");
                textArticleDescription.SetText("The best defense is not having who to fight you.");
                textArticlePrice.SetText("x" + GetNextLevelPrice(pm.playerStatStrength));
                imageSoldout.SetActive(pm.playerStatStrength >= 5);
                break;
            case 3: // Speed upgrade
                textArticleName.SetText("Speed Upgrade");
                textArticleDescription.SetText("Bebrage imported from far lands. It surely makes you go faster.");
                textArticlePrice.SetText("x" + GetNextLevelPrice(pm.playerStatSpeed));
                imageSoldout.SetActive(pm.playerStatSpeed >= 5);
                break;
            case 4: // Tenacity upgrade
                textArticleName.SetText("Tenacity Upgrade");
                textArticleDescription.SetText("Pants ready for rough jobs. Not fit for square bodies.");
                textArticlePrice.SetText("x" + GetNextLevelPrice(pm.playerStatTenacity));
                imageSoldout.SetActive(pm.playerStatTenacity >= 5);
                break;
            case 5: // Luck upgrade
                textArticleName.SetText("Luck Upgrade");
                textArticleDescription.SetText("It's fake, but has to be useful for something, right?");
                textArticlePrice.SetText("x"+GetNextLevelPrice(pm.playerStatLuck));
                imageSoldout.SetActive(pm.playerStatLuck >= 5);
                break;
        }
    }

    /// <summary>
    /// Buy the selected item
    /// </summary>
    public void BuySelectedItem() {
        PersistanceManager pm = PersistanceManager.GetInstance();
        PlayerController player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        switch(selectedItemIndex) {
            case 0: // Refill health
                if (player.hitpoints < player.hitpointsMax && player.coins >= 2) {
                    player.AddCoins(-2);
                    player.AddHealth(1);
                    SoundManager.GetInstance().Play(purchaseClip);
                } else {
                    SoundManager.GetInstance().Play(errorClip);
                }
                break;
            case 1: // Health upgrade
                if (pm.playerStatHealth < 5 && player.coins >= GetNextLevelPrice (pm.playerStatHealth)) {
                    player.AddCoins(-GetNextLevelPrice(pm.playerStatHealth));
                    pm.playerStatHealth++;
                    player.UpdateStats();
                    player.AddHealth(1);
                    SoundManager.GetInstance().Play(purchaseClip);
                } else {
                    SoundManager.GetInstance().Play(errorClip);
                }
                break;
            case 2: // Strength upgrade
                if (pm.playerStatStrength < 5 && player.coins >= GetNextLevelPrice(pm.playerStatStrength)) {
                    player.AddCoins(-GetNextLevelPrice(pm.playerStatStrength));
                    pm.playerStatStrength++;
                    player.UpdateStats();
                    SoundManager.GetInstance().Play(purchaseClip);
                } else {
                    SoundManager.GetInstance().Play(errorClip);
                }
                break;
            case 3: // Speed upgrade
                if (pm.playerStatSpeed < 5 && player.coins >= GetNextLevelPrice(pm.playerStatSpeed)) {
                    player.AddCoins(-GetNextLevelPrice(pm.playerStatSpeed));
                    pm.playerStatSpeed++;
                    player.UpdateStats();
                    SoundManager.GetInstance().Play(purchaseClip);
                } else {
                    SoundManager.GetInstance().Play(errorClip);
                }
                break;
            case 4: // Tenacity upgrade
                if (pm.playerStatTenacity < 5 && player.coins >= GetNextLevelPrice(pm.playerStatTenacity)) {
                    player.AddCoins(-GetNextLevelPrice(pm.playerStatTenacity));
                    pm.playerStatTenacity++;
                    player.UpdateStats();
                    SoundManager.GetInstance().Play(purchaseClip);
                } else {
                    SoundManager.GetInstance().Play(errorClip);
                }
                break;
            case 5: // Luck upgrade
                if (pm.playerStatLuck < 5 && player.coins >= GetNextLevelPrice(pm.playerStatLuck)) {
                    player.AddCoins(-GetNextLevelPrice(pm.playerStatLuck));
                    pm.playerStatLuck++;
                    player.UpdateStats();
                    SoundManager.GetInstance().Play(purchaseClip);
                } else {
                    SoundManager.GetInstance().Play(errorClip);
                }
                break;
        }
        UpdateShop();
    }

    /// <summary>
    /// Get the prices of the items
    /// </summary>
    private int GetNextLevelPrice(int currentLevel) {
        switch (currentLevel) {
            case 0:
                return 2;
            case 1:
                return 4;
            case 2:
                return 8;
            case 3:
                return 16;
            case 4:
                return 32;
            default:
                return 0;
        }
    }
}
