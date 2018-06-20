using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Controller for all player input logic
/// </summary>
public class InputController : MonoBehaviour {
    // Enum for the area selection of the pads
    private enum SelectionZone { Up, Left, Down, Right, Center, None }

    // Command (Attack) pad reference
    public GameObject cmdPad;

    // Movement pad reference
    public GameObject movePad;

    // Movement pad sections
    public Image uiMovePadUp;
    public Image uiMovePadDown;
    public Image uiMovePadLeft;
    public Image uiMovePadRight;
    public Image uiMovePadCenter;
    public Image uiMovePadReticle;
    private Collider2D uiMovePadUpCollider;
    private Collider2D uiMovePadLeftCollider;
    private Collider2D uiMovePadDownCollider;
    private Collider2D uiMovePadRightCollider;
    private Collider2D uiMovePadCenterCollider;

    // Command pad sections
    public Image uiCmdPadUp;
    public Image uiCmdPadDown;
    public Image uiCmdPadLeft;
    public Image uiCmdPadRight;
    public Image uiCmdPadCenter;
    private Collider2D uiCmdPadUpCollider;
    private Collider2D uiCmdPadLeftCollider;
    private Collider2D uiCmdPadDownCollider;
    private Collider2D uiCmdPadRightCollider;

    // Thereshold time for the dodge action
    public float tapTimeThereshold;

    // Thereshold time for slide action
    public float slideTimeThereshold;

    // Thereshold drag distance for slide action
    public float slideDistanceThereshold;

    // Registry for the lastest tap time
    private float latestTapTime;

    // Registry for the lastest tap button
    private int latestTapDirection;

    // Current movement zone being pressed
    private SelectionZone currentZone;

    // Previous movement zone being pressed
    private SelectionZone previousZone;

    // Time at which the previous zone was left
    private float previousZoneLeftTime;

    // Current movement selection direction
    private int selectedDir = -1;

    // Player reference
    private PlayerController player;

    // Pressed button colorize
    public Color activatedColor = new Color(1f, 1f, 1f, 1f);

    // Non-pressed button colorize
    public Color deactivatedColor = new Color(1f, 1f, 1f, 0.5f);

    // Center for the reticle of the movement pad
    private Vector3 uiMovePadReticleCenter;

    // Target position for the reticle of the movement pad
    private Vector3 uiMovePadReticleTargetPos;

    // Reticle position smooth factor
    public float uiMovePadReticleSmooth = .9f;


    /// <summary>
    /// Initiate references and set base UI colors
    /// </summary>
    void Awake() {
        uiMovePadUpCollider = uiMovePadUp.GetComponent<Collider2D>();
        uiMovePadLeftCollider = uiMovePadLeft.GetComponent<Collider2D>();
        uiMovePadDownCollider = uiMovePadDown.GetComponent<Collider2D>();
        uiMovePadRightCollider = uiMovePadRight.GetComponent<Collider2D>();
        uiMovePadCenterCollider = uiMovePadCenter.GetComponent<Collider2D>();
        uiCmdPadUpCollider = uiCmdPadUp.GetComponent<Collider2D>();
        uiCmdPadLeftCollider = uiCmdPadLeft.GetComponent<Collider2D>();
        uiCmdPadDownCollider = uiCmdPadDown.GetComponent<Collider2D>();
        uiCmdPadRightCollider = uiCmdPadRight.GetComponent<Collider2D>();

        uiMovePadUp.color = deactivatedColor;
        uiMovePadLeft.color = deactivatedColor;
        uiMovePadDown.color = deactivatedColor;
        uiMovePadRight.color = deactivatedColor;
        uiMovePadCenter.color = deactivatedColor;
        uiCmdPadUp.color = deactivatedColor;
        uiCmdPadLeft.color = deactivatedColor;
        uiCmdPadDown.color = deactivatedColor;
        uiCmdPadRight.color = deactivatedColor;
        uiCmdPadCenter.color = deactivatedColor;
    }

    /// <summary>
    /// Get player reference and startup UI
    /// </summary>
    void Start() {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        UpdateControlScheme();
        uiMovePadReticleCenter = uiMovePadReticle.rectTransform.position;
        uiMovePadReticleTargetPos = uiMovePadReticleCenter;
    }

    /// <summary>
    /// Keyboard controls for debugging: WASD for attack in specified directions
    /// </summary>
    void Update() {
        if (Time.timeScale > 0) {
            // Execute normal UI movement
            if (selectedDir != -1)
                player.MoveWalk(selectedDir);

            // Keyboard Attack
            if (Input.GetKeyDown(KeyCode.W))
                OnButtonPress(0);
            if (Input.GetKeyDown(KeyCode.A))
                OnButtonPress(1);
            if (Input.GetKeyDown(KeyCode.S))
                OnButtonPress(2);
            if (Input.GetKeyDown(KeyCode.D))
                OnButtonPress(3);

            uiMovePadReticle.rectTransform.position = Vector3.Lerp(uiMovePadReticle.rectTransform.position, uiMovePadReticleTargetPos, uiMovePadReticleSmooth);
        }
    }

    public void OnUIBeginDrag(EventTrigger trigger, PointerEventData data) {}

    /// <summary>
    /// Check Drag event data
    /// </summary>
    public void OnUIDrag(EventTrigger trigger, PointerEventData data) {
        // Check which pad was draged
        if (trigger.gameObject == cmdPad) {
            // Command pad, show pads pressed
            if (uiCmdPadUpCollider.OverlapPoint(data.position)) { ColorUI(false, 0); return; }
            if (uiCmdPadLeftCollider.OverlapPoint(data.position)) { ColorUI(false, 1); return; }
            if (uiCmdPadDownCollider.OverlapPoint(data.position)) { ColorUI(false, 2); return; }
            if (uiCmdPadRightCollider.OverlapPoint(data.position)) { ColorUI(false, 3); return; }
        } else {
            // Movement pad
            // Update reticle
            Vector2 relDist = data.position - (Vector2)uiMovePadReticleCenter;
            relDist = relDist.normalized * (Mathf.Min(relDist.magnitude, 100 * uiMovePadReticle.canvas.scaleFactor));
            uiMovePadReticleTargetPos = relDist + (Vector2)uiMovePadReticleCenter;

            // Check which selection zone / button the cursor is in
            SelectionZone sz = SelectionZone.None;
            if (uiMovePadCenterCollider.OverlapPoint(data.position))
                sz = SelectionZone.Center;
            else if (uiMovePadUpCollider.OverlapPoint(data.position))
                sz = SelectionZone.Up;
            else if (uiMovePadLeftCollider.OverlapPoint(data.position))
                sz = SelectionZone.Left;
            else if (uiMovePadDownCollider.OverlapPoint(data.position))
                sz = SelectionZone.Down;
            else if (uiMovePadRightCollider.OverlapPoint(data.position))
                sz = SelectionZone.Right;

            // Update currently selection direction
            switch (sz) {
                case SelectionZone.Up:
                    selectedDir = 0;
                    ColorUI(true, 0);
                    break;
                case SelectionZone.Left:
                    selectedDir = 1;
                    ColorUI(true, 1);
                    break;
                case SelectionZone.Down:
                    selectedDir = 2;
                    ColorUI(true, 2);
                    break;
                case SelectionZone.Right:
                    selectedDir = 3;
                    ColorUI(true, 3);
                    break;
                case SelectionZone.Center:
                    selectedDir = -1;
                    ColorUI(true, 4);
                    break;
            }

            // Update previous selection zone
            if (sz != currentZone && sz != SelectionZone.None) {
                previousZone = currentZone;
                currentZone = sz;
                previousZoneLeftTime = Time.time;
            }
        }
    }

    public void OnUIEndDrag(EventTrigger trigger, PointerEventData data) { }

    public void OnUIPointerDown(EventTrigger trigger, PointerEventData data) {
        if (trigger.gameObject == cmdPad) {
            // Command Pad, execute attacks
            if (uiCmdPadUpCollider.OverlapPoint(data.position)) {
                OnButtonPress(0);
                ColorUI(false, 0);
                return;
            }
            if (uiCmdPadLeftCollider.OverlapPoint(data.position)) {
                OnButtonPress(1);
                ColorUI(false, 1);
                return;
            }
            if (uiCmdPadDownCollider.OverlapPoint(data.position)) {
                OnButtonPress(2);
                ColorUI(false, 2);
                return;
            }
            if (uiCmdPadRightCollider.OverlapPoint(data.position)) {
                OnButtonPress(3);
                ColorUI(false, 3);
                return;
            }
        } else {
            // Movement pad
            // Check over which selection zone/button the pointer was pressed
            uiMovePadReticleTargetPos = data.position;
            SelectionZone sz = SelectionZone.None;
            if (uiMovePadCenterCollider.OverlapPoint(data.position))
                sz = SelectionZone.Center;
            else if (uiMovePadUpCollider.OverlapPoint(data.position))
                sz = SelectionZone.Up;
            else if (uiMovePadLeftCollider.OverlapPoint(data.position))
                sz = SelectionZone.Left;
            else if (uiMovePadDownCollider.OverlapPoint(data.position))
                sz = SelectionZone.Down;
            else if (uiMovePadRightCollider.OverlapPoint(data.position))
                sz = SelectionZone.Right;

            // Colorize button
            ColorUI(true, (int)sz);

            // Execute dodge if the button tapped twice within thereshold
            switch (sz) {
                case SelectionZone.Up:
                    selectedDir = 0;
                    if (latestTapDirection == selectedDir) {
                        if (Time.time - latestTapTime < tapTimeThereshold) {
                            player.MoveDodge(selectedDir);
                        }
                    }
                    latestTapDirection = selectedDir;
                    latestTapTime = Time.time;
                    break;
                case SelectionZone.Left:
                    selectedDir = 1;
                    if (latestTapDirection == selectedDir) {
                        if (Time.time - latestTapTime < tapTimeThereshold) {
                            player.MoveDodge(selectedDir);
                        }
                    }
                    latestTapDirection = selectedDir;
                    latestTapTime = Time.time;
                    break;
                case SelectionZone.Down:
                    selectedDir = 2;
                    if (latestTapDirection == selectedDir) {
                        if (Time.time - latestTapTime < tapTimeThereshold) {
                            player.MoveDodge(selectedDir);
                        }
                    }
                    latestTapDirection = selectedDir;
                    latestTapTime = Time.time;
                    break;
                case SelectionZone.Right:
                    selectedDir = 3;
                    if (latestTapDirection == selectedDir) {
                        if (Time.time - latestTapTime < tapTimeThereshold) {
                            player.MoveDodge(selectedDir);
                        }
                    }
                    latestTapDirection = selectedDir;
                    latestTapTime = Time.time;
                    break;
            }

            // Update current movement zone
            currentZone = sz;
        }
    }

    /// <summary>
    /// Release pointer event
    /// </summary>
    /// <param name="trigger">Source trigger object</param>
    /// <param name="data">Data of the event</param>
    public void OnUIPointerUp(EventTrigger trigger, PointerEventData data) {
        // Is the command pad?
        if (trigger.gameObject == cmdPad) {
            // Just remove press graphics
            ColorUI(false, -1);
            return;
        } else {
            // Movement pad
            uiMovePadReticleTargetPos = uiMovePadReticleCenter;
            ColorUI(true, -1);

            // Check for possible slide events
            if (previousZone == SelectionZone.Center && (Time.time - previousZoneLeftTime) < slideTimeThereshold) {
                switch (currentZone) {
                    case SelectionZone.Up: player.MoveSlide(0); break;
                    case SelectionZone.Left: player.MoveSlide(1); break;
                    case SelectionZone.Down: player.MoveSlide(2); break;
                    case SelectionZone.Right: player.MoveSlide(3); break;
                }
            }

            if (uiMovePadCenterCollider.OverlapPoint(data.position))
                player.StopSlide();

            selectedDir = -1;
            previousZone = SelectionZone.None;
            previousZoneLeftTime = 0;
        }
    }

    /// <summary>
    /// Attack in the specified direction
    /// </summary>
    /// <param name="id">Direction of attack in the 4 integer direction notation</param>
    public void OnButtonPress(int id) {
        player.Attack(id);
    }

    /// <summary>
    /// Colorize UI buttons
    /// </summary>
    /// <param name="isMovementPad"> Movement or Command pad</param>
    /// <param name="pressed"> Pressed button</param>
    private void ColorUI(bool isMovementPad, int pressed) {
        if (isMovementPad) {
            uiMovePadUp.color = deactivatedColor;
            uiMovePadLeft.color = deactivatedColor;
            uiMovePadDown.color = deactivatedColor;
            uiMovePadRight.color = deactivatedColor;
            uiMovePadCenter.color = deactivatedColor;
            switch (pressed) {
                case 0:
                    uiMovePadUp.color = activatedColor;
                    break;
                case 1:
                    uiMovePadLeft.color = activatedColor;
                    break;
                case 2:
                    uiMovePadDown.color = activatedColor;
                    break;
                case 3:
                    uiMovePadRight.color = activatedColor;
                    break;
                case 4:
                    uiMovePadCenter.color = activatedColor;
                    break;
            }
        } else {
            uiCmdPadUp.color = deactivatedColor;
            uiCmdPadLeft.color = deactivatedColor;
            uiCmdPadDown.color = deactivatedColor;
            uiCmdPadRight.color = deactivatedColor;
            uiCmdPadCenter.color = deactivatedColor;
            switch (pressed) {
                case 0:
                    uiCmdPadUp.color = activatedColor;
                    break;
                case 1:
                    uiCmdPadLeft.color = activatedColor;
                    break;
                case 2:
                    uiCmdPadDown.color = activatedColor;
                    break;
                case 3:
                    uiCmdPadRight.color = activatedColor;
                    break;
                case 4:
                    uiCmdPadCenter.color = activatedColor;
                    break;
            }
        }
    }

    /// <summary>
    /// Specify the control schene between three different modes
    /// </summary>
    /// <param name="mode">The control scheme [0-2]</param>
    public void SetControlScheme(int mode) {
        PersistanceManager.GetInstance().controlScheme = mode;
        UpdateControlScheme();
    }

    /// <summary>
    /// Update UI position of the controls to reflect the control scheme
    /// </summary>
    public void UpdateControlScheme() {
        switch (PersistanceManager.GetInstance().controlScheme) {
            case 0: {
                    cmdPad.transform.rotation = Quaternion.identity;
                    movePad.transform.rotation = Quaternion.identity;
                }
                break;
            case 2: {
                    Quaternion rot = Quaternion.AngleAxis(45, Vector3.forward);
                    cmdPad.transform.rotation = rot;
                    movePad.transform.rotation = rot;
                }
                break;
            case 1: {
                    Quaternion rot = Quaternion.AngleAxis(-45, Vector3.forward);
                    cmdPad.transform.rotation = rot;
                    movePad.transform.rotation = rot;
                }
                break;
        }
    }
}
