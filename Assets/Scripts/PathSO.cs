using UnityEngine;

[CreateAssetMenu(fileName = "PathSO", menuName = "EdgeRush/PathSO", order = 0)]
public class PathSO : ScriptableObject {
    [System.NonSerialized]
    public Path path;
}