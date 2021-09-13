using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class Path : MonoBehaviour {

    [System.Serializable]
    public class PathCurve {
        [SerializeField, HideInInspector] string title = "pathcurve";
        // todo move to from anywhere option
        // public bool moveTo = false;
        // public Transform moveToTransform;
        public BezierCurve curve;
        [Min(-1)]
        public int loops = 1;
        [Min(0)]
        public float delay = 0;
        public LoopType loopType = LoopType.Restart;
        public Ease easeType = Ease.Linear;
        // [SerializeField] Ease easeTypeCloseRestart = Ease.InOutCubic;

        [ReadOnly] public float distance;
        [ReadOnly] public float duration;
        [ReadOnly] public float distanceTotal;
        [ReadOnly] public float durationTotal;

        public void Validate(string prefix = "") {
            title = prefix + curve.name;
        }

        public Vector2[] GetPoints() {
            List<Vector2> points = new List<Vector2>();

            if (curve.pointCount <= 1) {
                return points.ToArray();
            }
            Vector2 lastHandle2;
            if (curve.close) {
                lastHandle2 = curve[curve.pointCount - 1].globalHandle2;
            } else {
                lastHandle2 = curve[0].globalHandle1;// + curve[0].handle1;
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
        for (int i = 0; i < pathCurves.Count; i++) {
            pathCurves[i].Validate(i + ". ");
        }
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
            pathCurve.durationTotal = pathCurve.duration * pathCurve.loops + pathCurve.delay;
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

    public Sequence FollowPath(Rigidbody2D rb, Vector2 pathOffset) {
        StopPath();
        CalcDistances();
        pathseq = DOTween.Sequence();
        Vector3? startPos = null;
        Vector2 lastEndPoint = Vector2.zero;
        for (int i = 0; i < pathCurves.Count; i++) {
            // if (i > 0) {
            //     // link to prev path
            //     var connectTween = rb.DOMove(lastEndPoint, 0.01f);
            //     connectTween.SetEase(Ease.Linear);
            //     pathseq.Append(connectTween);
            // }
            PathCurve pathCurve = pathCurves[i];
            Vector2[] points = pathCurve.GetPoints();
            points = points.ToList().ConvertAll<Vector2>(v => v + pathOffset).ToArray();
            lastEndPoint = points[points.Length - 3];
            float dur = pathCurve.duration;
            var pathtween = rb.DOPath(points, dur, PathType.CubicBezier, PathMode.Sidescroller2D);
            if (startPos == null) startPos = points[0];
            pathtween.SetLoops(pathCurve.loops, pathCurve.loopType);
            pathtween.SetEase(pathCurve.easeType);
            pathtween.SetDelay(pathCurve.delay);
            // pathtween.SetSpeedBased(true);
            pathseq.Append(pathtween);
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

    private void OnDrawGizmosSelected() {
        // for (int i = 1; i < pathCurves.Count; i++) {
        //     var lastcurve = pathCurves[i - 1].curve;
        //     var curve = pathCurves[i].curve;
        //     BezierCurve.DrawCurve(lastcurve[lastcurve.pointCount - 1], curve[0], 30);
        // }
    }

}