using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PathRunHandler : MonoBehaviour {
    public Sequence sequence;

    public void KillSequence() {
        sequence.Kill();
    }
}