using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternSO", menuName = "EdgeRush/PatternSO", order = 0)]
public class PatternSO : ScriptableObject {

    [System.Serializable]
    public class PatternPhase {
        [SerializeField, HideInInspector] string title = "phase";
        public void Validate(string prefix = "") {
            if (repetitions <= 0) {
                repetitions = 1;
            }
            totalDuration = duration * repetitions;

            title = prefix + "phase";
            if (repetitions > 1) {
                title += "*" + repetitions;
            }
            if (subPatterns == null || subPatterns.Length == 0) {
                if (duration > 0) {
                    title += " wait " + duration;
                } else {
                    title += " empty!";
                }
            } else {
                if (subPatterns.Any(sp => sp.patternType == SubPatternSO.PatternType.bullet)) {
                    title += " shoot";
                } else {
                    title += " (no bullet!)";
                }
            }
            if (skip) {
                title += " (skipped)";
            }
        }
        public bool skip = false;
        [Min(0f)]
        public float duration = 0;
        [Min(1)]
        public int repetitions = 1;
        public SubPatternSO[] subPatterns;
        [SerializeField, ReadOnly] float totalDuration;
    }
    public PatternPhase[] subPatternPhases;
    private void OnValidate() {
        for (int i = 0; i < subPatternPhases.Length; i++) {
            subPatternPhases[i].Validate(i + ". ");
        }
    }

}