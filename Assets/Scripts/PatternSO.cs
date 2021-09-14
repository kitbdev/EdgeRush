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
            _phaseDuration = duration * repetitions + delayDuration;

            title = prefix + "phase";
            if (repetitions > 1) {
                title += "*" + repetitions;
            }
            if (subPatterns == null || subPatterns.Length == 0) {
                if (_phaseDuration > 0) {
                    title += " wait " + _phaseDuration;
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
        [Tooltip("Ignore this pattern")]
        public bool skip = false;
        [Tooltip("Duration to wait once before firing")]
        [Min(0f)]
        public float delayDuration = 0;
        [Tooltip("Duration to wait before firing for each repetition")]
        [Min(0f)]
        public float duration = 0;
        [Tooltip("Number of times to repeat before going on to the next phase")]
        [Min(1)]
        public int repetitions = 1;
        public SubPatternSO[] subPatterns;
        [SerializeField, ReadOnly] float _phaseDuration;
        public float phaseDuration => _phaseDuration;
    }
    [Tooltip("Duration to wait once before going through the pattern")]
    [Min(0f)]
    public float delayDuration = 0;
    public PatternPhase[] subPatternPhases;
    [SerializeField, ReadOnly] float totalDuration;

    private void OnValidate() {
        totalDuration = delayDuration;
        for (int i = 0; i < subPatternPhases.Length; i++) {
            subPatternPhases[i].Validate(i + ". ");
            totalDuration += subPatternPhases[i].phaseDuration;
        }
    }
}