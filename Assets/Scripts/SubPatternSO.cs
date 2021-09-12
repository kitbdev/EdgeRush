﻿using UnityEngine;

[CreateAssetMenu(fileName = "SubPatternSO", menuName = "EdgeRush/SubPatternSO", order = 0)]
public class SubPatternSO : ScriptableObject {

    public enum PatternType {
        bullet,
        single,
        line,
        arc,
        ring,
        randomize,
        target,
        none,
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

    // [ConditionalHide(nameof(patternType), (int)PatternType.bullet)]
    // public bool followPlayer = false;
    [ConditionalHide(nameof(patternType), (int)PatternType.bullet)]
    public BulletSpawnSettings bulletSpawnSettings;

    // public SubPatternSO subpattern;


    public void PatternLogic() {

    }
}