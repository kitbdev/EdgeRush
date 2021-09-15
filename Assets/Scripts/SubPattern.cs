using UnityEngine;

[System.Serializable]
public class SubPattern {
    [SerializeField, HideInInspector] string title = "subpattern";
    [HideInInspector, SerializeField] bool initialized = false;
    public void Validate(string prefix = "") {
        title = prefix + patternType.ToString();
        if (patternType != PatternType.single) {
            title += " " + spawnAmount;
        }
        if (radius < 0) radius = 0;
        if (spacing < 0) spacing = 0;
        if (!initialized) {
            Initialize();
        }
    }
    void Initialize() {
        spawnAmount = 1;
        spacing = 0.5f;
        radius = 0.2f;
        startAngle = -90;
        angleDist = 30;
        initialized = true;
    }
    public enum PatternType {
        none,
        single,
        line,
        arc,
        ring,
        randomize,
        target,
        emitter,
    }
    public PatternType patternType;
    // [ConditionalHide(nameof(patternType), false, (int)PatternType.single)]
    [Min(1)]
    public int spawnAmount = 1;

    // [Min(0)]
    [ConditionalHide(nameof(patternType), (int)PatternType.line)]
    public float spacing = 0.5f;
    [ConditionalHide(nameof(patternType), (int)PatternType.line)]
    public float alignmentDegree = 0;

    [ConditionalHide(nameof(patternType), (int)PatternType.ring, (int)PatternType.arc)]
    // [Min(0)]
    public float radius = 0.2f;

    [ConditionalHide(nameof(patternType), (int)PatternType.arc)]
    // [Range(-360, 360)]
    public float startAngle = -90;
    [ConditionalHide(nameof(patternType), (int)PatternType.arc)]
    public float angleDist = 30;

    [ConditionalHide(nameof(patternType), (int)PatternType.ring)]
    public float angleOffset = 0;
    [ConditionalHide(nameof(patternType), (int)PatternType.ring, (int)PatternType.arc)]
    public float angleTurn = 0;
    [ConditionalHide(nameof(patternType), (int)PatternType.ring, (int)PatternType.arc)]
    public bool angleOut = false;

    [ConditionalHide(nameof(patternType), (int)PatternType.randomize)]
    public float initRandomAngleOffsetMin = -10;
    [ConditionalHide(nameof(patternType), (int)PatternType.randomize)]
    public float initRandomAngleOffsetMax = 10;

    // [Space]
    // [ConditionalHide(nameof(patternType), false, (int)PatternType.none)]
    public InitStateMod modifier = new InitStateMod();
    [System.Serializable]
    public class InitStateMod {
        public bool setAccelb = false;
        [ConditionalHide(nameof(setAccelb), true)]
        public float setAccel = 0;
        public bool setAngAccelb = false;
        [ConditionalHide(nameof(setAngAccelb), true)]
        public float setAngAccel = 0;
        // todo? set ang, pos, directly option

        public float addAng = 0;
        [ConditionalHide(nameof(SubPattern.patternType), (int)SubPattern.PatternType.line,
                                                        (int)SubPattern.PatternType.arc,
                                                        (int)SubPattern.PatternType.ring)]
        public float addAngByIndex = 0;
        public float addSpeed = 0;
        [ConditionalHide(nameof(SubPattern.patternType), (int)SubPattern.PatternType.line,
                                                        (int)SubPattern.PatternType.arc,
                                                        (int)SubPattern.PatternType.ring)]
        public float addSpeedByIndex = 0;
        public float addAngSpeed = 0;
        [ConditionalHide(nameof(SubPattern.patternType), (int)SubPattern.PatternType.line,
                                                        (int)SubPattern.PatternType.arc,
                                                        (int)SubPattern.PatternType.ring)]
        public float addAngSpeedByIndex = 0;
    }
    // [ConditionalHide(nameof(patternType), (int)PatternType.bullet)]
    // public bool followPlayer = false;
    // [ConditionalHide(nameof(patternType), (int)PatternType.bullet)]
    // public BulletSpawnSettings bulletSpawnSettings;

    // public SubPatternSO subpattern;
}