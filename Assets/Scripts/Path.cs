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
        public Transform moveToTransform;
        public Vector2 moveToOffset = Vector2.zero;
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
            title = prefix;
            if (moveToTransform != null) {
                title += $"move to {moveToTransform.name} {GetMoveToPoint()}";
            } else if (curve != null) {
                title += curve.name;
            }
        }
        public Vector2 GetMoveToPoint() {
            return (Vector2)moveToTransform.position + moveToOffset;
        }
        public Vector2 GetLastCurvePoint() {
            if (curve.pointCount == 0) {
                return Vector2.zero;
            }
            return curve[curve.pointCount - 1].position;
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
        Vector2? lastPoint = null;
        for (int i = 0; i < pathCurves.Count; i++) {
            PathCurve pathCurve = pathCurves[i];
            if (pathCurve.moveToTransform != null) {
                Vector2 moveToPoint = pathCurve.GetMoveToPoint();
                if (lastPoint == null) {
                    lastPoint = moveToPoint;
                }
                pathCurve.distance = Vector2.Distance(moveToPoint, (Vector2)lastPoint);
                lastPoint = moveToPoint;
            } else if (pathCurve.curve != null) {
                pathCurve.distance = pathCurve.curve.length;
                lastPoint = pathCurve.GetLastCurvePoint();
            } else {
                pathCurve.distance = 0;
            }
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

    public Sequence FollowPath(Rigidbody2D rb, float moveSpeed = -1) => FollowPath(rb, Vector2.zero, moveSpeed);
    public Sequence FollowPath(Rigidbody2D rb, Vector2 pathOffset, float moveSpeed = -1) {
        // todo? paths can be used by multiple users?
        // StopPath();
        if (moveSpeed > 0) {
            this.moveSpeed = moveSpeed;
        }
        CalcDistances();
        pathseq = DOTween.Sequence();
        Vector2? startPos = null;
        // Vector2 lastEndPoint = Vector2.zero;
        for (int i = 0; i < pathCurves.Count; i++) {
            // if (i > 0) {
            //     // link to prev path
            //     var connectTween = rb.DOMove(lastEndPoint, 0.01f);
            //     connectTween.SetEase(Ease.Linear);
            //     pathseq.Append(connectTween);
            // }
            PathCurve pathCurve = pathCurves[i];
            if (pathCurve.moveToTransform != null) {
                Vector2 point = pathCurve.GetMoveToPoint() + pathOffset;
                float dur = pathCurve.duration;
                var moveTween = rb.DOMove(point, dur);
                if (startPos == null) startPos = point;
                pathseq.Append(moveTween);
            } else {
                Vector2[] points = pathCurve.GetPoints();
                points = points.ToList().ConvertAll<Vector2>(v => v + pathOffset).ToArray();
                // lastEndPoint = points[points.Length - 3];
                float dur = pathCurve.duration;
                var pathtween = rb.DOPath(points, dur, PathType.CubicBezier, PathMode.Sidescroller2D);
                if (startPos == null) startPos = points[0];
                // todo? incremental loops ignore first move
                pathtween.SetLoops(pathCurve.loops, pathCurve.loopType);
                pathtween.SetEase(pathCurve.easeType);
                pathtween.SetDelay(pathCurve.delay);
                // pathtween.SetSpeedBased(true);
                pathseq.Append(pathtween);
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
    Color drawColor = new Color(1, 0.5f, 0);
    private void OnDrawGizmosSelected() {
        Gizmos.color = drawColor * Color.gray;
        Vector2 lastEndPoint = Vector2.zero;
        for (int i = 0; i < pathCurves.Count; i++) {
            PathCurve pathCurve = pathCurves[i];
            if (pathCurve.moveToTransform != null) {
                Vector2 point = pathCurve.GetMoveToPoint();
                if (i != 0) Gizmos.DrawLine(lastEndPoint, point);
                lastEndPoint = point;
            } else {
                Vector2[] points = pathCurve.GetPoints();
                if (i != 0) Gizmos.DrawLine(lastEndPoint, points[0]);
                pathCurve.curve.drawColor = drawColor;
                // lastEndPoint = points[points.Length - 3];
                // draw curve repetitions
                BezierCurve curve = pathCurve.curve;
                Vector3 baseoffset = curve[curve.pointCount - 1].localPosition - curve[0].localPosition;
                // Vector3 baseoffset = curve[curve.pointCount - 1].position - (Vector3)lastEndPoint;
                baseoffset += (curve[0].position - (Vector3)lastEndPoint);
                Vector3 offset = Vector3.zero;
                for (int l = 0; l < pathCurve.loops - 1; l++) {
                    Gizmos.DrawLine(curve[curve.pointCount - 1].position + offset, curve[0].position + offset + baseoffset);
                    offset += baseoffset;
                    if (curve.pointCount > 1) {
                        for (int p = 0; p < curve.pointCount - 1; p++) {
                            BezierCurve.DrawCurve(curve[p], curve[p + 1], curve.resolution, offset);
                        }
                        if (curve.close) BezierCurve.DrawCurve(curve[curve.pointCount - 1], curve[0], curve.resolution, offset);
                    }
                }
                lastEndPoint = curve[curve.pointCount - 1].position;
                lastEndPoint += (Vector2)offset;
            }
        }
        Gizmos.color = Color.white;
    }

}