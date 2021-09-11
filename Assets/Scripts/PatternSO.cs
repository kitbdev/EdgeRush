using UnityEngine;

[CreateAssetMenu(fileName = "PatternSO", menuName = "EdgeRush/PatternSO", order = 0)]
public class PatternSO : ScriptableObject {

    [System.Serializable]
    public class PatternDur {
        public float duration = 0;
        public float repetitions = 1;
        public SubPatternSO[] subPatterns;
    }
    public PatternDur[] subPatternDurs;
}