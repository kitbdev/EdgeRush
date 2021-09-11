using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour {

    [SerializeField] float speed = 1;
    [SerializeField] float height = 10;

    private void Update() {
        transform.position += Vector3.down * speed * Time.deltaTime;
        // todo loop
        if (transform.position.y < -height) {
            transform.position = Vector3.zero;
        }
        // todo different screen sizes?
    }
}