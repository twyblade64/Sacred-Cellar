using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for a pop-up dialog canvas
/// </summary>
public class DialogCanvasController : MonoBehaviour {
    // Alpha of the canvas, modified by the Animator
    public float generalAlpha;

    // Title text of the dialog
    public string dialogTitle;

    // Body text of the dialog
    [TextArea(15, 20)]
    public string dialogBody;

    // Title TextMeshPro component reference
    public TextMeshProUGUI txtTitle;

    // Body TextMeshPro component reference
    public TextMeshProUGUI txtBody;

    // Animator component reference
    private Animator animator;

    public void Awake() {
        animator = GetComponent<Animator>();
    }

    void Start () {
        txtTitle.SetText(dialogTitle);
        txtBody.SetText(dialogBody);
	}
	
	// Manually update elements alpha
	void Update () {
        if (Time.timeScale > 0) {
            foreach (Image img in GetComponentsInChildren<Image>()) {
                Color col = img.color;
                col.a = generalAlpha;
                img.color = col;
            }
            txtTitle.color = new Color(1f, 1f, 1f, generalAlpha);
            txtBody.color = new Color(1f, 1f, 1f, generalAlpha);
        }
    }

    // Show the dialog canvas
    public void Open() {
        animator.SetBool("isOpen", true);
    }

    // Hide the dialog canvas
    public void Close() {
        animator.SetBool("isOpen", false);
    }
}
