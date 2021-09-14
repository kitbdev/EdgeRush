using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour {

    [SerializeField] float speed = 1;
    [SerializeField] float height = 10;
    public Material[] scrollMats;

    private void Update() {
        foreach (var scrollMat in scrollMats) {
            if (!scrollMat) return;
            var offset = scrollMat.mainTextureOffset;
            offset.y += speed * Time.unscaledDeltaTime;
            if (offset.y > 1) {
                offset.y -= 1;
            }
            scrollMat.mainTextureOffset = offset;
        }
        // transform.position += Vector3.down * speed * Time.deltaTime;
        // // todo loop
        // if (transform.position.y < -height) {
        //     transform.position = Vector3.zero;
        // }
        // todo different screen sizes?
    }
    public void ResetScrolls() {
        foreach (var scrollMat in scrollMats) {
            if (!scrollMat) return;
            var offset = scrollMat.mainTextureOffset;
            offset.y = 0;
            scrollMat.mainTextureOffset = offset;
        }
    }
}