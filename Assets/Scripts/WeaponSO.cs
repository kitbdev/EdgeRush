using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "EdgeRush/WeaponSO", order = 0)]
public class WeaponSO : ScriptableObject {

    public GameObject bulletPrefab;
    public float shootCooldownDur = 0.2f;
    public float shootHoldCooldownDur = 0.2f;
    public int numShootPoints = 1;
    public float damage = 1;

    public void Shoot() {

    }
}