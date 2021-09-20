using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-55)]
public class EffectiveVelocity : MonoBehaviour {

    [ReadOnly] public Vector2 effectiveVelocity = Vector2.zero;//> (Vector2)transform.position - lastpos;
    [ReadOnly] public Vector2 lastpos = Vector2.zero;

    private void Update() {
        Vector2 pos = (Vector2)transform.position;
        effectiveVelocity = (pos - lastpos) / Time.deltaTime;
        lastpos = pos;
    }
}