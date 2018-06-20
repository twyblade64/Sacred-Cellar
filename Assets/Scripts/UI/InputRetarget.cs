using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Trasnfer trigger information to the InputCnotroller
/// </summary>
public class InputRetarget : EventTrigger {
    [SerializeField]
    public InputController target;

    void Awake() {
        target = GetComponentInParent<InputController>();
    }

    public override void OnBeginDrag(PointerEventData data) {
        target.OnUIBeginDrag(this, data);
    }

    public override void OnDrag(PointerEventData data) {
        target.OnUIDrag(this, data);
    }

    public override void OnEndDrag(PointerEventData data) {
        target.OnUIEndDrag(this, data);
    }

    public override void OnPointerUp(PointerEventData data) {
        target.OnUIPointerUp(this, data);
    }

    public override void OnPointerDown(PointerEventData data) {
        target.OnUIPointerDown(this, data);
    }

    public override void OnCancel(BaseEventData data) {}
    public override void OnDeselect(BaseEventData data) {}
    public override void OnDrop(PointerEventData data) {}
    public override void OnInitializePotentialDrag(PointerEventData data) {}
    public override void OnMove(AxisEventData data) {}
    public override void OnPointerClick(PointerEventData data) {}
    public override void OnPointerEnter(PointerEventData data) {}
    public override void OnPointerExit(PointerEventData data) {}
    public override void OnScroll(PointerEventData data) {}
    public override void OnSelect(BaseEventData data) {}
    public override void OnSubmit(BaseEventData data) {}
    public override void OnUpdateSelected(BaseEventData data) {}
}
