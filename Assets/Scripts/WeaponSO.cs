using UnityEngine;

/// <summary>
/// holds data for shooting bullets in a single pattern
/// </summary>
[CreateAssetMenu(fileName = "WeaponSO", menuName = "EdgeRush/WeaponSO", order = 0)]
public class WeaponSO : ScriptableObject {

    public GameObject bulletPrefab;
    public AudioManager.AudioSettings shootAudio;
    public int modelIndex = 0;
    public bool hasUnlimitedAmmo = false;
    [Min(0f)]
    public float shootCooldownDur = 0.2f;
    [Min(0f)]
    public float shootHoldCooldownDur = 0.2f;
    public float damage = 1;
    public BulletSpawnSettings bulletSpawnSettings;
}