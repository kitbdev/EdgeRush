using UnityEngine;

/// <summary>
/// holds data for shooting bullets in a single pattern
/// </summary>
[CreateAssetMenu(fileName = "WeaponSO", menuName = "EdgeRush/WeaponSO", order = 0)]
public class WeaponSO : ScriptableObject {

    public GameObject bulletPrefab;
    [Min(0f)]
    public float shootCooldownDur = 0.2f;
    [Min(0f)]
    public float shootHoldCooldownDur = 0.2f;
    [Min(1)]
    public int numShootPoints = 1;
    public float damage = 1;
    public float bulletScale = 1;
    public float launchForce = 5;
    public bool aimAtPlayer = false;
    public float[] aimOffsets = new float[0];
    public int[] shotPointIndexes = new int[0];
    // add random options to startpos?
    // types
    // unaimed (straight)
    // aimed at player
    // angle change over time

    [ReadOnly, SerializeField] int bulletPrefabTypeId = -1;
    public int bulletId {
        get {
            if (bulletPrefabTypeId == -1) {
                GetId();
            }
            return bulletPrefabTypeId;
        }
    }

    private void OnValidate() {
        if (bulletPrefab == null || !Application.isPlaying) {
            bulletPrefabTypeId = -1;
            return;
        }
        GetId();
    }
    void GetId() {
        bulletPrefabTypeId = BulletManager.Instance.pool.GetTypeId(bulletPrefab);
    }
    private void OnEnable() {
        // Debug.Log("weapon awake");
        bulletPrefabTypeId = -1;
    }
}