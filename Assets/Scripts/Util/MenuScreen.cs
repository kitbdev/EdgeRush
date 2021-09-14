using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
[RequireComponent(typeof(CanvasGroup))]
public class MenuScreen : MonoBehaviour {

    [SerializeField] Selectable selectOnShow;
    [SerializeField] bool showOnStart = false;
    [SerializeField] bool resetPosOnAwake = true;
    [SerializeField] string animTag = "Shown";

    [ReadOnly] public bool isShown = false;

    protected CanvasGroup canvasGroup;
    protected Animator anim;

    private void Awake() {
        Setup();
        if (resetPosOnAwake) {
            ResetPosition();
        }
    }
    void Setup() {
        anim = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
    }
    private void OnEnable() {
        SetVisible(showOnStart, true);
    }
    // private void Start() {
    // }
    public void ToggleShow() {
        SetVisible(!isShown);
    }
    [ContextMenu("Show")]
    public void Show() {
        SetVisible(true);
    }
    [ContextMenu("Hide")]
    public void Hide() {
        SetVisible(false);
    }
    [ContextMenu("Reset pos")]
    public void ResetPosition() {
        var rt = transform as RectTransform;
        rt.localPosition = Vector3.zero;
    }
    public void ForceSetVisible(bool visible = true) {
        SetVisible(visible, true);
    }
    public void SetVisible(bool visible = true, bool forceSet = false) {
        if (!forceSet && isShown == visible) {
            return;
        }
        isShown = visible;
        if (isShown) {
            transform.SetAsLastSibling();
            selectOnShow?.Select();
        }
        if (!Application.isPlaying) {
            Setup();
        }
        if (anim) {
            anim.SetBool(animTag, isShown);
        }
        if (canvasGroup) {
            // todo lerp option (with unscaledtime)
            canvasGroup.alpha = isShown ? 1f : 0f;
            canvasGroup.interactable = isShown;
            canvasGroup.blocksRaycasts = isShown;
        }
    }
}