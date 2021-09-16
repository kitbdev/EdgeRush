using System.Collections.Generic;
using UnityEngine;
using static PatternSO;

public class PatternRunner : MonoBehaviour {
    /*
    bullet attack patterns

    for each phase repetition
        phase.duration is waited for
        phase subpatterns is executed 

    subpatterns
        ends with bullet type which creates the bullets
        all other types are modifiers to the bullet's spawn state - position, angle, speed, ang speed, acceleration, ang accel
        each modifier operates on every previous spawn state
            and often creates more spawn states
        line
            creates n bullets displaced in a line
        arc
            creates n bullets in an arc
        ring
            creates a full arc (in a circle)

    */
    [System.Serializable]
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

        public Vector2 dir => AngToDir(angle);

        public static Vector2 AngToDir(float angle) {
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        public Vector3 posWorld => position;
        public Quaternion angWorld => Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
        public override string ToString() {
            return $"{position} {angle}deg sp{speed} {angSpeed}deg";
        }
    }

    public PatternSO patternSO;
    public Transform[] spawnPoints;
    [SerializeField] AudioManager.AudioSettings shootAudio;
    public bool isPlayer = false;
    public Transform player;

    [SerializeField, ReadOnly] int phaseIndex = 0;
    [SerializeField, ReadOnly] int curRepetition = 0;
    [SerializeField, ReadOnly] bool didSound = false;
    [SerializeField, ReadOnly] float lastRepetitionStartTime;
    [SerializeField, ReadOnly] float lastPhaseStartTime;
    [SerializeField, ReadOnly] float patternExecuteStartTime = 0;
    [SerializeField, ReadOnly] bool initialDelayComplete;
    [SerializeField] BulletInitState emitterState = BulletInitState.origin;

    BulletManager bulletManager;
    AudioManager audioManager;

    private void Awake() {
        bulletManager = BulletManager.Instance;
        audioManager = AudioManager.Instance;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void ProcessPattern() {
        if (patternSO == null || patternSO.patternPhases.Length == 0) return;
        if (patternExecuteStartTime <= 0) patternExecuteStartTime = Time.time;
        if (!initialDelayComplete) initialDelayComplete = (patternSO.delayDuration <= 0 || Time.time > patternExecuteStartTime + patternSO.delayDuration);

        PatternSO.PatternPhase phase = patternSO.patternPhases[phaseIndex];
        emitterState.position += emitterState.dir * emitterState.speed * Time.deltaTime;
        emitterState.angle += emitterState.angSpeed * Time.deltaTime;
        if (phase.skip) {
            GoToNextPhase();
            return;
        }
        // wait until we can execute
        bool delayComplete = (phase.delayDuration <= 0 || Time.time > lastPhaseStartTime + phase.delayDuration);
        bool waitComplete = (phase.duration <= 0 || Time.time > lastRepetitionStartTime + phase.duration);
        if (initialDelayComplete && delayComplete && waitComplete) {
            ExecutePhase(phase);
            curRepetition++;
            lastRepetitionStartTime = Time.time;
        }
        if (curRepetition >= phase.repetitions) {
            GoToNextPhase();
        }
    }
    void GoToNextPhase() {
        // go to next pattern phase
        phaseIndex++;
        lastPhaseStartTime = Time.time;
        didSound = false;
        if (phaseIndex >= patternSO.patternPhases.Length) {
            // completed all phases, reset
            phaseIndex = 0;
            // reset emitter state
            emitterState = BulletInitState.origin;
        }
        curRepetition = 0;
    }
    public List<BulletInitState> initStates = new List<BulletInitState>();
    void ExecutePhase(PatternPhase phase) {
        initStates.Clear();
        initStates.Add(BulletInitState.origin);
        for (int i = 0; i < phase.bulletPatterns.Length; i++) {
            List<BulletInitState> newStates = new List<BulletInitState>();
            foreach (var initState in initStates) {
                newStates.AddRange(ApplyPattern(phase.bulletPatterns[i], initState));
            }
            initStates = newStates;
        }
        if (phase.spawnBullet) {
            foreach (var initState in initStates) {
                // if (float.NaN.Equals(initState.position.x)) {
                //     Debug.LogWarning("Invalid position NaN!");
                //     break;
                // }
                SpawnBullet(phase.bulletSpawnSettings, initState);
            }
        }
    }
    void SpawnBullet(BulletSpawnSettings bulletSpawnSettings, BulletInitState initState) {
        if (bulletSpawnSettings == null) {
            return;
        }
        // Debug.Log("spawning at " + initState.position);
        bulletManager.Shoot(bulletSpawnSettings, initState, spawnPoints, isPlayer);
        if (!didSound && shootAudio != null) {
            didSound = true;
            shootAudio.position = transform.position;
            audioManager.PlaySfx(shootAudio);
        }
    }
    List<BulletInitState> ApplyPattern(SubPattern subPattern, BulletInitState initState) {
        if (subPattern == null) {
            return initStates;
        }
        // Debug.Log("Applying pattern " + subPattern.patternType.ToString());
        List<BulletInitState> newStates = new List<BulletInitState>();
        switch (subPattern.patternType) {
            case SubPattern.PatternType.none:
                // do nothing
                newStates.Add(initState);
                return newStates;
            // break;
            case SubPattern.PatternType.target:
                BulletInitState nstate = new BulletInitState(initState);
                if (player == null) {
                    if (Application.isPlaying) {
                        Debug.LogWarning(name + " has no player!");
                    }
                    newStates.Add(nstate);
                    break;
                }
                Vector3 toPlayer = player.position - spawnPoints[0].position;
                toPlayer.Normalize();
                nstate.angle = Vector2.SignedAngle(Vector2.down, (Vector2)toPlayer) * Mathf.Deg2Rad;
                newStates.Add(nstate);
                break;
            case SubPattern.PatternType.randomize:
                nstate = new BulletInitState(initState);
                float initrandangoffset = Random.Range(subPattern.initRandomAngleOffsetMin, subPattern.initRandomAngleOffsetMax);
                nstate.angle += initrandangoffset;
                newStates.Add(nstate);
                break;
            case SubPattern.PatternType.emitter:
                // adjust cross subpatterns emitter settings
                // ?emitterState.position
                emitterState.angle += subPattern.modifier.addAng;
                emitterState.speed = subPattern.modifier.addSpeed;
                emitterState.angSpeed = subPattern.modifier.addAngSpeed;
                newStates.Add(initState);
                return newStates;
            // break;
            case SubPattern.PatternType.single:
                // make a single subpattern
                newStates.Add(initState);
                break;
            case SubPattern.PatternType.line:
                // make a line of subpatterns
                float half = (subPattern.spawnAmount - 1) * subPattern.spacing / 2f;
                for (int i = 0; i < subPattern.spawnAmount; i++) {
                    nstate = new BulletInitState(initState);
                    float dist = i * subPattern.spacing - half;
                    float alignAng = initState.angle + Mathf.Deg2Rad * subPattern.alignmentDegree;
                    // todo adjust angle?
                    nstate.position += dist * BulletInitState.AngToDir(alignAng);
                    newStates.Add(nstate);
                }
                break;
            case SubPattern.PatternType.arc:
                // make a arc of subpatterns
                float angDist = subPattern.angleDist;
                float halfang = subPattern.angleDist / 2f;
                if (subPattern.spawnAmount > 1) {
                    for (int i = 0; i < subPattern.spawnAmount; i++) {
                        float angdeg = subPattern.startAngle + i * 1f / (subPattern.spawnAmount - 1) * subPattern.angleDist;
                        angdeg -= halfang;
                        // angdeg += 180;
                        float ang = Mathf.Deg2Rad * angdeg + initState.angle;
                        nstate = new BulletInitState(initState);
                        if (subPattern.angleOut) {
                            nstate.angle = ang + Mathf.PI / 2f + Mathf.Deg2Rad * subPattern.angleTurn;
                        }
                        nstate.position += subPattern.radius * BulletInitState.AngToDir(ang);
                        newStates.Add(nstate);
                    }
                }
                break;
            case SubPattern.PatternType.ring:
                // make a ring of subpatterns
                halfang = (subPattern.spawnAmount - 1) * (Mathf.PI * 2f) / 2f;
                // todo fix odd spawn amount
                if (subPattern.spawnAmount > 0) {
                    for (int i = 0; i < subPattern.spawnAmount; i++) {
                        float ang = i * Mathf.PI * 2 / (subPattern.spawnAmount);
                        ang += subPattern.angleOffset * Mathf.Deg2Rad;
                        ang += initState.angle;
                        nstate = new BulletInitState(initState);
                        if (subPattern.angleOut) {
                            nstate.angle = ang + Mathf.PI / 2f + Mathf.Deg2Rad * subPattern.angleTurn;
                        }
                        nstate.position += subPattern.radius * BulletInitState.AngToDir(ang);
                        newStates.Add(nstate);
                    }
                }
                break;
        }
        var mod = subPattern.modifier;
        for (int i = 0; i < newStates.Count; i++) {
            // int index = i-offsets.c
            if (mod.setAccelb) newStates[i].acceleration = mod.setAccel;
            if (mod.setAngAccelb) newStates[i].angAcceleration = mod.setAngAccel * Mathf.Deg2Rad;
            newStates[i].angle += mod.addAng + mod.addAngByIndex * i;
            newStates[i].speed += mod.addSpeed + mod.addSpeedByIndex * i;
            newStates[i].angSpeed += (mod.addAngSpeed + mod.addAngSpeedByIndex * i) * Mathf.Deg2Rad;
        }
        return newStates;
    }
    public void ForEachInitState(System.Action<BulletInitState> action) {
        if (patternSO == null) return;
        foreach (var phase in patternSO.patternPhases) {
            if (phase.skip) {
                continue;
            }
            List<BulletInitState> offsets = new List<BulletInitState>();
            offsets.Add(BulletInitState.origin);
            foreach (var subpattern in phase.bulletPatterns) {
                List<BulletInitState> noffsets = new List<BulletInitState>();
                foreach (var offset in offsets) {
                    noffsets.AddRange(ApplyPattern(subpattern, offset));
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
        foreach (var patternPhase in patternSO.patternPhases) {
            totalDuration += patternPhase.duration * patternPhase.repetitions;
            // patterndur.numBullets = 0;
            // totalNumBullets += patterndur.numBullets;
            if (patternPhase == null || patternPhase.bulletPatterns == null) {
                continue;
            }
            for (int i = 0; i < patternPhase.bulletPatterns.Length; i++) {
                SubPattern subpattern = patternPhase.bulletPatterns[i];
                subpattern.Validate(i + ". ");
            }
        }
        ForEachInitState(state => {
            bulletsPerLoop++;
        });
    }
    private void OnDrawGizmosSelected() {
        if (patternSO == null) return;
        foreach (var phase in patternSO.patternPhases) {
            List<BulletInitState> offsets = new List<BulletInitState>();
            offsets.Add(BulletInitState.origin);
            foreach (var subpattern in phase.bulletPatterns) {
                List<BulletInitState> noffsets = new List<BulletInitState>();
                foreach (var offset in offsets) {
                    noffsets.AddRange(ApplyPattern(subpattern, offset));
                }
                offsets = noffsets;
            }
            Gizmos.color = Color.black;
            Vector3 basePos = spawnPoints[0].position;
            foreach (var offset in offsets) {
                if (float.NaN.Equals(offset.position.x)) {
                    Debug.LogWarning("Invalid position NaN!");
                    break;
                }
                Gizmos.DrawLine(basePos + offset.posWorld, basePos + offset.posWorld + offset.angWorld * Vector3.up);
            }
        }
        Gizmos.color = Color.white;
    }
}