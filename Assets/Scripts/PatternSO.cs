using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternSO", menuName = "EdgeRush/PatternSO", order = 0)]
public class PatternSO : ScriptableObject {

    [System.Serializable]
    public class PatternPhase {
        [SerializeField, HideInInspector] string title = "phase";
        [HideInInspector, SerializeField] bool initialized = false;

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
        public bool spawnBullet = false;
        [ConditionalHide(nameof(spawnBullet))]
        public BulletSpawnSettings bulletSpawnSettings = new BulletSpawnSettings();
        public SubPattern[] bulletPatterns = new SubPattern[0];
        [SerializeField, ReadOnly] float _phaseDuration;

        void Initialize() {
            bulletSpawnSettings = new BulletSpawnSettings();
            initialized = true;
        }

        public float phaseDuration => _phaseDuration;

        public void Validate(string prefix = "") {
            if (repetitions <= 0) {
                repetitions = 1;
            }
            _phaseDuration = duration * repetitions + delayDuration;

            title = prefix + "phase";
            if (repetitions > 1) {
                title += "*" + repetitions;
            }
            if (bulletPatterns == null || bulletPatterns.Length == 0) {
                if (_phaseDuration > 0) {
                    title += " wait " + _phaseDuration;
                } else if (spawnBullet) {
                    title += " shoot";
                } else {
                    title += " empty";
                }
            } else {
                if (spawnBullet) {
                    title += " shoot";
                }
            }
            if (skip) {
                title += " (skipped)";
            }
            for (int i = 0; i < bulletPatterns.Length; i++) {
                bulletPatterns[i].Validate(i + ". ");
            }
            if (!initialized) {
                Initialize();
            }
        }
    }

    [HideInInspector]
    [SerializeField] GameObject defaultBulletPrefab;
    // [HideInInspector]
    // [SerializeField] BulletSpawnSettings;


    [Tooltip("Duration to wait once before going through the pattern")]
    [Min(0f)]
    public float delayDuration = 0;
    public PatternPhase[] patternPhases = new PatternPhase[0];
    [SerializeField, ReadOnly] float totalDuration;

    private void OnValidate() {
        totalDuration = delayDuration;
        for (int i = 0; i < patternPhases.Length; i++) {
            patternPhases[i].Validate(i + ". ");
            totalDuration += patternPhases[i].phaseDuration;
            if (patternPhases[i].spawnBullet && patternPhases[i].bulletSpawnSettings.prefab == null) {
                patternPhases[i].bulletSpawnSettings.prefab = defaultBulletPrefab;
            }
        }
    }
}