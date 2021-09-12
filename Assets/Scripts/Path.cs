using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class Path : MonoBehaviour {

    [System.Serializable]
    public class PathCurve {
        public BezierCurve curve;
        public int loops = 1;
        public LoopType loopType = LoopType.Restart;
        public Ease easeType = Ease.Linear;
        // [SerializeField] Ease easeTypeCloseRestart = Ease.InOutCubic;

        [ReadOnly] public float distance;
        [ReadOnly] public float duration;
        [ReadOnly] public float distanceTotal;
        [ReadOnly] public float durationTotal;

        public Vector2[] GetPoints() {
            List<Vector2> points = new List<Vector2>();

            if (curve.pointCount <= 1) {
                return points.ToArray();
            }
            Vector2 lastHandle2;
            if (curve.close) {
                lastHandle2 = curve[curve.pointCount - 1].handle2;
            } else {
                lastHandle2 = curve[0].handle1;
            }
            for (int i = 0; i < curve.pointCount; i++) {
                BezierPoint bezierPoint = curve[i];
                points.Add(bezierPoint.position);
                points.Add((Vector2)lastHandle2);
                lastHandle2 = bezierPoint.globalHandle2;
                points.Add(bezierPoint.globalHandle1);
            }
            return points.ToArray();
        }
    }

    [SerializeField] List<PathCurve> pathCurves = new List<PathCurve>();
    [Min(0f)]
    public float moveSpeed = 5;
    [SerializeField] [Min(0f)] float delay = 0;
    [Tooltip("-1 for infinite")]
    [SerializeField] [Min(-1)] int loops = 1;
    [SerializeField] LoopType fullSequenceLoopType = LoopType.Restart;
    [SerializeField] Ease fullSequenceEase = Ease.Linear;

    [SerializeField, ReadOnly] float totalDistance;
    [SerializeField, ReadOnly] float totalDuration;

    Sequence pathseq;

    [Header("Events")]
    UnityEvent sequenceCompleteEvent;
    UnityEvent sequenceLoopEvent;

    private void OnValidate() {
        CalcDistances();
    }
    private void Awake() {
        CalcDistances();
    }
    [ContextMenu("Recalc distances")]
    void CalcDistances() {
        totalDistance = 0;
        totalDuration = 0;
        for (int i = 0; i < pathCurves.Count; i++) {
            PathCurve pathCurve = pathCurves[i];
            pathCurve.distance = pathCurve.curve.length;
            pathCurve.duration = pathCurve.distance / moveSpeed;
            pathCurve.distanceTotal = pathCurve.distance * pathCurve.loops;
            pathCurve.durationTotal = pathCurve.duration * pathCurve.loops;
            totalDistance += pathCurve.distanceTotal;
            totalDuration += pathCurve.durationTotal;
        }
    }
    [ContextMenu("Find Curves")]
    void FindCurves() {
#if UNITY_EDITOR
        UnityEditor.Undo.RecordObject(this, "find curves");
#endif
        var curves = GetComponentsInChildren<BezierCurve>();
        if (pathCurves.Count > curves.Length) {
            pathCurves.RemoveRange(curves.Length, pathCurves.Count - curves.Length);
        }
        for (int i = 0; i < pathCurves.Count; i++) {
            pathCurves[i].curve = curves[i];
        }
        if (pathCurves.Count < curves.Length) {
            pathCurves.AddRange(curves.Skip(pathCurves.Count).ToList().ConvertAll(b => new PathCurve() { curve = b }));
        }
        OnValidate();
    }

    public Sequence FollowPath(Rigidbody2D rb) {
        StopPath();
        CalcDistances();
        pathseq = DOTween.Sequence();
        Vector3? startPos = null;
        for (int i = 0; i < pathCurves.Count; i++) {
            PathCurve pathCurve = pathCurves[i];
            Vector2[] points = pathCurve.GetPoints();
            float dur = pathCurve.duration;
            var pathtween = rb.DOPath(points, dur, PathType.CubicBezier, PathMode.Sidescroller2D);
            if (startPos == null) startPos = points[0];
            pathtween.SetLoops(pathCurve.loops, pathCurve.loopType);
            pathtween.SetEase(pathCurve.easeType);
            // pathtween.SetSpeedBased(true);
            pathseq.Append(pathtween);
            if (i < pathCurves.Count - 1) {
                // link to next path
                // rb.DOMove
            }
        }
        pathseq.PrependInterval(delay);
        // loopType = curve.close ? LoopType.Restart : LoopType.Yoyo;
        // var easeType = curve.close && loopType != LoopType.Yoyo ? easeTypeCloseRestart : easeTypeOpen;
        pathseq.SetLoops(loops, fullSequenceLoopType);
        pathseq.SetEase(fullSequenceEase);
        pathseq.onStepComplete = () => { sequenceLoopEvent?.Invoke(); };
        pathseq.onComplete = () => { sequenceCompleteEvent?.Invoke(); };
        rb.position = (Vector2)startPos;
        pathseq.Play();
        return pathseq;
    }

    public void StopPath() {
        pathseq.Kill();
    }

}