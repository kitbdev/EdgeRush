using System.Collections.Generic;
using UnityEngine;

public class PatternRunner : MonoBehaviour {
    public PatternSO patternSO;
    [SerializeField, ReadOnly] int index = 0;
    [SerializeField, ReadOnly] int curRepetition = 0;
    [SerializeField, ReadOnly] float lastTime;
    public Transform[] spawnPoints;
    public bool isPlayer = false;

    public void ProcessPattern() {
        PatternSO.PatternDur patternDur = patternSO.subPatternDurs[index];
        // wait until next
        if (patternDur.duration <= 0 || Time.time > lastTime + patternDur.duration) {
            CreateSubPattern(patternDur.subPatterns);
            curRepetition++;
            lastTime = Time.time;
        }
        if (curRepetition >= patternDur.repetitions) {
            // go to next pattern
            index++;
            if (index >= patternSO.subPatternDurs.Length) {
                index = 0;
            }
            curRepetition = 0;
        }
    }
    public class SubpatternPlace {
        public Vector2 position;
        public float angle;
        public static SubpatternPlace origin = new SubpatternPlace() {
            position = Vector2.zero, angle = 0,
        };

        private SubpatternPlace() { }
        public SubpatternPlace(SubpatternPlace copy) {
            this.position = copy.position;
            this.angle = copy.angle;
        }

        public Vector3 posWorld => position;
        public Quaternion angWorld => Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
        public override string ToString() {
            return position + "," + angle;
        }

    }
    void CreateSubPattern(SubPatternSO[] subPatterns) {
        List<SubpatternPlace> offsets = new List<SubpatternPlace>();
        offsets.Add(SubpatternPlace.origin);
        for (int i = 0; i < subPatterns.Length; i++) {
            List<SubpatternPlace> noffsets = new List<SubpatternPlace>();
            foreach (var offset in offsets) {
                noffsets.AddRange(SpawnSubPattern(subPatterns[i], offset));
            }
            offsets = noffsets;
        }
    }
    List<SubpatternPlace> SpawnSubPattern(SubPatternSO subPattern, SubpatternPlace place) {
        List<SubpatternPlace> offsets = new List<SubpatternPlace>();
        if (subPattern == null) {
            return offsets;
        }
        switch (subPattern.patternType) {
            case SubPatternSO.PatternType.none:
                // do nothing
                break;
            case SubPatternSO.PatternType.bullet:
                if (subPattern.bulletSpawnSettings == null) {
                    break;
                }
                BulletManager.Instance.Shoot(subPattern.bulletSpawnSettings, place, spawnPoints, isPlayer);
                // for (int i = 0; i < subPattern.numSubPatterns; i++) {
                // float initang = subPattern.bulletInitAngle + place.angle;
                // Vector2 pos = place.position;
                // }
                break;
            case SubPatternSO.PatternType.randomize:
                SubpatternPlace nplace = new SubpatternPlace(place);
                float initrandangoffset = Random.Range(subPattern.initRandomAngleOffsetMin, subPattern.initRandomAngleOffsetMax);
                nplace.angle += initrandangoffset;
                offsets.Add(nplace);
                break;
            case SubPatternSO.PatternType.single:
                // make a single subpattern
                offsets.Add(place);
                break;
            case SubPatternSO.PatternType.line:
                // make a line of subpatterns
                float half = (subPattern.numSubPatterns - 1) * subPattern.spacing / 2f;
                for (int i = 0; i < subPattern.numSubPatterns; i++) {
                    nplace = new SubpatternPlace(place);
                    float dist = i * subPattern.spacing - half;
                    float alignAng = place.angle + Mathf.Deg2Rad * subPattern.alignmentDegree;
                    // todo adjust angle?
                    nplace.position += dist * new Vector2(Mathf.Cos(alignAng), Mathf.Sin(alignAng));
                    offsets.Add(nplace);
                }
                break;
            case SubPatternSO.PatternType.arc:
                // make a arc of subpatterns
                for (int i = 0; i < subPattern.numSubPatterns; i++) {
                    float angdeg = subPattern.minAngle + i * 1f / subPattern.numSubPatterns * (subPattern.maxAngle - subPattern.minAngle);
                    float ang = Mathf.Deg2Rad * angdeg;
                    nplace = new SubpatternPlace(place);
                    nplace.angle = ang;
                    nplace.position += subPattern.radius * new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                    offsets.Add(nplace);
                }
                break;
            case SubPatternSO.PatternType.ring:
                // make a ring of subpatterns
                for (int i = 0; i < subPattern.numSubPatterns; i++) {
                    float ang = i * Mathf.PI * 2 / subPattern.numSubPatterns;
                    nplace = new SubpatternPlace(place);
                    nplace.angle += ang;
                    nplace.position += subPattern.radius * new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                    offsets.Add(nplace);
                }
                break;
        }
        return offsets;
    }
}