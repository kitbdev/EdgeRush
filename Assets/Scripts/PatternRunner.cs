using System.Collections.Generic;
using UnityEngine;

public class PatternRunner : MonoBehaviour {
    public PatternSO patternSO;
    [SerializeField, ReadOnly] int index = 0;
    [SerializeField, ReadOnly] int curRepetition = 0;
    [SerializeField, ReadOnly] float lastTime;
    public Transform[] spawnPoints;
    public bool isPlayer = false;
    public Transform player;
    BulletInitState emitterState = BulletInitState.origin;

    public void ProcessPattern() {
        if (patternSO == null || patternSO.subPatternPhases.Length == 0) return;
        PatternSO.PatternPhase patternDur = patternSO.subPatternPhases[index];
        // wait until next
        if (patternDur.duration <= 0 || Time.time > lastTime + patternDur.duration) {
            CreateSubPattern(patternDur.subPatterns);
            curRepetition++;
            lastTime = Time.time;
        }
        if (curRepetition >= patternDur.repetitions) {
            // go to next pattern
            index++;
            if (index >= patternSO.subPatternPhases.Length) {
                index = 0;
            }
            curRepetition = 0;
        }
    }
    public class BulletInitState {
        public Vector2 position = Vector2.zero;
        public float angle = 0;
        public float speed = 0;
        public float angSpeed = 0;
        public float acceleration = 0;
        public float angAcceleration = 0;

        public static BulletInitState origin = new BulletInitState() {
            position = Vector2.zero, angle = 0,
        };

        private BulletInitState() { }
        public BulletInitState(BulletInitState copy) {
            this.position = copy.position;
            this.angle = copy.angle;
            this.speed = copy.speed;
            this.angSpeed = copy.angSpeed;
            this.acceleration = copy.acceleration;
            this.angAcceleration = copy.angAcceleration;
        }

        public Vector3 posWorld => position;
        public Quaternion angWorld => Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
        public override string ToString() {
            return position + "," + angle;
        }

    }
    void CreateSubPattern(SubPatternSO[] subPatterns) {
        List<BulletInitState> offsets = new List<BulletInitState>();
        offsets.Add(BulletInitState.origin);
        for (int i = 0; i < subPatterns.Length; i++) {
            List<BulletInitState> noffsets = new List<BulletInitState>();
            foreach (var offset in offsets) {
                noffsets.AddRange(SpawnSubPattern(subPatterns[i], offset));
            }
            offsets = noffsets;
        }
    }
    List<BulletInitState> SpawnSubPattern(SubPatternSO subPattern, BulletInitState place) {
        List<BulletInitState> offsets = new List<BulletInitState>();
        if (subPattern == null) {
            return offsets;
        }
        switch (subPattern.patternType) {
            case SubPatternSO.PatternType.none:
                // do nothing
                offsets.Add(place);
                return offsets;
            // break;
            case SubPatternSO.PatternType.bullet:
                if (subPattern.bulletSpawnSettings == null) {
                    break;
                }
                BulletManager.Instance.Shoot(subPattern.bulletSpawnSettings, place, spawnPoints, isPlayer);
                // for (int i = 0; i < subPattern.numSubPatterns; i++) {
                // float initang = subPattern.bulletInitAngle + place.angle;
                // Vector2 pos = place.position;
                // }
                break;
            case SubPatternSO.PatternType.target:
                BulletInitState nplace = new BulletInitState(place);
                if (player == null) {
                    Debug.LogWarning(name + " has no player!");
                    offsets.Add(nplace);
                    break;
                }
                Vector3 toPlayer = player.position - spawnPoints[0].position;
                toPlayer.Normalize();
                nplace.angle = Vector2.SignedAngle(Vector2.down, (Vector2)toPlayer) * Mathf.Deg2Rad;
                offsets.Add(nplace);
                break;
            case SubPatternSO.PatternType.randomize:
                nplace = new BulletInitState(place);
                float initrandangoffset = Random.Range(subPattern.initRandomAngleOffsetMin, subPattern.initRandomAngleOffsetMax);
                nplace.angle += initrandangoffset;
                offsets.Add(nplace);
                break;
            case SubPatternSO.PatternType.emitter:
                // adjust cross subpatterns emitter settings
                // todo
                // ?emitterstate speed?
                // emitterState.position
                // emitterState.angle
                offsets.Add(place);
                return offsets;
            // break;
            case SubPatternSO.PatternType.single:
                // make a single subpattern
                offsets.Add(place);
                break;
            case SubPatternSO.PatternType.line:
                // make a line of subpatterns
                float half = (subPattern.numSubPatterns - 1) * subPattern.spacing / 2f;
                for (int i = 0; i < subPattern.numSubPatterns; i++) {
                    nplace = new BulletInitState(place);
                    float dist = i * subPattern.spacing - half;
                    float alignAng = place.angle + Mathf.Deg2Rad * subPattern.alignmentDegree;
                    // todo adjust angle?
                    nplace.position += dist * new Vector2(Mathf.Cos(alignAng), Mathf.Sin(alignAng));
                    offsets.Add(nplace);
                }
                break;
            case SubPatternSO.PatternType.arc:
                // make a arc of subpatterns
                float angDist = subPattern.angleDist;
                float halfang = subPattern.angleDist / 2f;
                for (int i = 0; i < subPattern.numSubPatterns; i++) {
                    float angdeg = subPattern.startAngle + i * 1f / (subPattern.numSubPatterns - 1) * subPattern.angleDist;
                    angdeg -= halfang;
                    // angdeg += 180;
                    float ang = Mathf.Deg2Rad * angdeg + place.angle;
                    nplace = new BulletInitState(place);
                    if (subPattern.angleOut) {
                        nplace.angle = ang + Mathf.PI / 2f + Mathf.Deg2Rad * subPattern.angleTurn;
                    }
                    nplace.position += subPattern.radius * new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                    offsets.Add(nplace);
                }
                break;
            case SubPatternSO.PatternType.ring:
                // make a ring of subpatterns
                halfang = (subPattern.numSubPatterns - 1) * (Mathf.PI * 2f) / 2f;
                for (int i = 0; i < subPattern.numSubPatterns; i++) {
                    float ang = i * Mathf.PI * 2 / (subPattern.numSubPatterns - 1);
                    ang += subPattern.angleOffset * Mathf.Deg2Rad;
                    ang += place.angle;
                    nplace = new BulletInitState(place);
                    if (subPattern.angleOut) {
                        nplace.angle = ang + Mathf.PI / 2f + Mathf.Deg2Rad * subPattern.angleTurn;
                    }
                    nplace.position += subPattern.radius * new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                    offsets.Add(nplace);
                }
                break;
        }
        var mod = subPattern.modifier;
        for (int i = 0; i < offsets.Count; i++) {
            // int index = i-offsets.c
            if (mod.setAccelb) offsets[i].acceleration = mod.setAccel;
            if (mod.setAngAccelb) offsets[i].angAcceleration = mod.setAngAccel * Mathf.Deg2Rad;
            offsets[i].angle += mod.addAng + mod.addAngByIndex * i;
            offsets[i].speed += mod.addSpeed + mod.addSpeedByIndex * i;
            offsets[i].angSpeed += (mod.addAngSpeed + mod.addAngSpeedByIndex * i) * Mathf.Deg2Rad;
        }
        return offsets;
    }
    public void ForEachInitState(System.Action<BulletInitState> action) {
        if (patternSO == null) return;
        foreach (var subdirs in patternSO.subPatternPhases) {
            List<BulletInitState> offsets = new List<BulletInitState>();
            offsets.Add(BulletInitState.origin);
            foreach (var subpattern in subdirs.subPatterns) {
                List<BulletInitState> noffsets = new List<BulletInitState>();
                foreach (var offset in offsets) {
                    if (subpattern.patternType == SubPatternSO.PatternType.bullet) {
                        noffsets = offsets;
                        continue;
                    }
                    noffsets.AddRange(SpawnSubPattern(subpattern, offset));
                }
                offsets = noffsets;
            }
            foreach (var offset in offsets) {
                action.Invoke(offset);
            }
        }
    }
    [Space]
    [SerializeField, ReadOnly] float totalDuration = 0;
    [SerializeField, ReadOnly] int bulletsPerLoop = 0;

    private void OnValidate() {
        totalDuration = 0;
        bulletsPerLoop = 0;
        if (!patternSO) return;
        foreach (var patternPhase in patternSO.subPatternPhases) {
            totalDuration += patternPhase.duration * patternPhase.repetitions;
            // patterndur.numBullets = 0;
            // totalNumBullets += patterndur.numBullets;
            for (int i = 0; i < patternPhase.subPatterns.Length; i++) {
                SubPatternSO subpattern = patternPhase.subPatterns[i];
                subpattern.Validate(i + ". ");
            }
        }
        ForEachInitState(state => {
            bulletsPerLoop++;
        });
    }
    private void OnDrawGizmosSelected() {
        if (patternSO == null) return;
        foreach (var phase in patternSO.subPatternPhases) {
            List<BulletInitState> offsets = new List<BulletInitState>();
            offsets.Add(BulletInitState.origin);
            foreach (var subpattern in phase.subPatterns) {
                List<BulletInitState> noffsets = new List<BulletInitState>();
                foreach (var offset in offsets) {
                    if (subpattern.patternType == SubPatternSO.PatternType.bullet) {
                        noffsets = offsets;
                        continue;
                    }
                    noffsets.AddRange(SpawnSubPattern(subpattern, offset));
                }
                offsets = noffsets;
                // if (subpattern.patternType == SubPatternSO.PatternType.arc) {

                // }
            }
            Gizmos.color = Color.black;
            Vector3 basePos = spawnPoints[0].position;
            foreach (var offset in offsets) {
                Gizmos.DrawLine(basePos + offset.posWorld, basePos + offset.posWorld + offset.angWorld * Vector3.up);
            }
        }
        Gizmos.color = Color.white;
    }
}