using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Small floating number effect controller used to display attack damage.
/// It is pooled so it can be created easely and consumes less resources.
/// </summary>
public class FloatingNumberController : MonoBehaviour, IPoolable {
    // TextMeshPro component reference used to display the number
    [SerializeField]
    private TextMeshProUGUI textMesh;

    // The value to display
    public int number;

    // Time that the number shows
    public float floatTime;

    // Float time progress
    private float floatProgress;

    // Height that the number moves to
    public float height;

    // Starting height
    private float startHeight;

    // Pool information
    private bool alive;
    private bool inPool;

    // Color of the damage number
    public Color textColor;

    void Start() {
        transform.rotation = Camera.main.transform.rotation;
        if (!InPool)
            Init();
    }

    /// <summary>
    /// Elevate the number and progress float time
    /// </summary>
    void Update () {
		if (Time.timeScale > 0  && alive) {
            floatProgress += Time.deltaTime;
            Vector3 pos = transform.position;
            float p = floatProgress / floatTime;
            pos.y = Mathf.Lerp(startHeight, startHeight + height, Mathf.Pow(p, .25f));
            transform.position = pos;
            if (p > .75f)
                textMesh.gameObject.SetActive(!textMesh.gameObject.activeInHierarchy);
            if (floatProgress >= floatTime) {
                Clear();
            }
        }
	}

    public bool Alive {
        get {
            return alive;
        }
    }

    public bool InPool {
        get {
            return inPool;
        }
        set {
            inPool = value;
        }
    }

    public GameObject PoolGameObject {
        get {
            return gameObject;
        }
    }

    public void Clear() {
        if (InPool) {
            alive = false;
            gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
        }
    }

    public void Init() {
        floatProgress = 0;
        startHeight = transform.position.y;
        gameObject.SetActive(true);
        textMesh.SetText(number.ToString());
        textMesh.gameObject.SetActive(true);
        textMesh.color = textColor;
        alive = true;
    }
}
