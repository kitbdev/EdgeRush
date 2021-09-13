using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour {

    [SerializeField] float speed = 1;
    [SerializeField] float height = 10;
    public Material scrollMat;

    private void Update() {
        if (!scrollMat) return;
        var offset = scrollMat.mainTextureOffset;
        offset.y += speed * Time.deltaTime;
        scrollMat.mainTextureOffset = offset;
        // transform.position += Vector3.down * speed * Time.deltaTime;
        // // todo loop
        // if (transform.position.y < -height) {
        //     transform.position = Vector3.zero;
        // }
        // todo different screen sizes?
    }
}