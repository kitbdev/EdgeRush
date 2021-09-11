using UnityEngine;

[CreateAssetMenu(fileName = "PatternSO", menuName = "EdgeRush/PatternSO", order = 0)]
public class PatternSO : ScriptableObject {

    [System.Serializable]
    public class PatternDur {
        [Min(0f)]
        public float duration = 0;
        [Min(1)]
        public float repetitions = 1;
        public SubPatternSO[] subPatterns;
    }
    public PatternDur[] subPatternDurs;
}