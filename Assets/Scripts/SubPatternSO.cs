﻿using UnityEngine;

[CreateAssetMenu(fileName = "SubPatternSO", menuName = "EdgeRush/SubPatternSO", order = 0)]
public class SubPatternSO : ScriptableObject {

    [SerializeField, HideInInspector] string title = "subpattern";
    public void Validate(string prefix = "") {
        title = prefix + patternType.ToString();
        if (patternType != PatternType.single && patternType != PatternType.bullet) {
            title += " " + numSubPatterns;
        }
    }

    public enum PatternType {
        none,
        bullet,
        single,
        line,
        arc,
        ring,
        randomize,
        target,
        emitter,
    }
    public PatternType patternType;
    [ConditionalHide(nameof(patternType), false, (int)PatternType.single, (int)PatternType.bullet)]
    public int numSubPatterns = 1;

    [ConditionalHide(nameof(patternType), (int)PatternType.line)]
    [Min(0)]
    public float spacing = 5;
    [ConditionalHide(nameof(patternType), (int)PatternType.line)]
    public float alignmentDegree = 0;

    [ConditionalHide(nameof(patternType), (int)PatternType.ring, (int)PatternType.arc)]
    [Min(0)]
    public float radius = 1;

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
    public float initRandomAngleOffsetMin = 0;
    [ConditionalHide(nameof(patternType), (int)PatternType.randomize)]
    public float initRandomAngleOffsetMax = 0;

    [Space]
    [ConditionalHide(nameof(patternType), false, (int)PatternType.bullet, (int)PatternType.none)]
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
        [ConditionalHide(nameof(SubPatternSO.patternType), (int)SubPatternSO.PatternType.line,
                                                        (int)SubPatternSO.PatternType.arc,
                                                        (int)SubPatternSO.PatternType.ring)]
        public float addAngByIndex = 0;
        public float addSpeed = 0;
        [ConditionalHide(nameof(SubPatternSO.patternType), (int)SubPatternSO.PatternType.line,
                                                        (int)SubPatternSO.PatternType.arc,
                                                        (int)SubPatternSO.PatternType.ring)]
        public float addSpeedByIndex = 0;
        public float addAngSpeed = 0;
        [ConditionalHide(nameof(SubPatternSO.patternType), (int)SubPatternSO.PatternType.line,
                                                        (int)SubPatternSO.PatternType.arc,
                                                        (int)SubPatternSO.PatternType.ring)]
        public float addAngSpeedByIndex = 0;
    }
    // [ConditionalHide(nameof(patternType), (int)PatternType.bullet)]
    // public bool followPlayer = false;
    [ConditionalHide(nameof(patternType), (int)PatternType.bullet)]
    public BulletSpawnSettings bulletSpawnSettings;

    // public SubPatternSO subpattern;
}