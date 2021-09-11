using UnityEngine;

[CreateAssetMenu(fileName = "SubPatternSO", menuName = "EdgeRush/SubPatternSO", order = 0)]
public class SubPatternSO : ScriptableObject {

    public enum PatternType {
        bullet,
        single,
        line,
        arc,
        ring,
        randomize,
        none,
    }
    public PatternType patternType;
    [ConditionalHide(nameof(patternType), false, (int)PatternType.single)]
    public int numSubPatterns = 1;
    // [ConditionalHide(nameof(patternType), false, (int)PatternType.bullet)]
    // [ConditionalHide(nameof(patternType), (int)PatternType.bullet)]
    // public GameObject bulletPrefab;
    [ConditionalHide(nameof(patternType), (int)PatternType.line)]
    public float spacing = 5;
    [ConditionalHide(nameof(patternType), (int)PatternType.arc)]
    // [Range(-360, 360)]
    public float minAngle = -30;
    [ConditionalHide(nameof(patternType), (int)PatternType.arc)]
    public float maxAngle = 30;
    [ConditionalHide(nameof(patternType), (int)PatternType.ring)]
    public float radius = 1;
    [ConditionalHide(nameof(patternType), (int)PatternType.ring)]
    public float angleSpread = 15;
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