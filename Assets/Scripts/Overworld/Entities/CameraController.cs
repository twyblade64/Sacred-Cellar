using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Character-following camera with a little smoothing.
/// </summary>
public class CameraController : MonoBehaviour {
    public float smoothing;

    private Vector3 startPos;
    private PlayerController player;

	void Start () {
        startPos = transform.position;
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        transform.position += player.transform.position;
	}
	
	void LateUpdate () {
        if (Time.timeScale > 0) {
            Vector3 dist = player.transform.position;
            transform.position = Vector3.Lerp(transform.position, startPos + dist, smoothing);
        }
    }
}
