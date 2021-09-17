using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimPlayer : MonoBehaviour {

    public string boolName = "Play";
    Animator anim;

    private void Awake() {
        anim = GetComponent<Animator>();
    }

    public void Play() {
        anim.SetBool(boolName, true);
    }
    public void Stop() {
        anim.SetBool(boolName, false);
    }
}