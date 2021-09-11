using UnityEngine;

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
    public float launchForce = 5;

    [ReadOnly, SerializeField] int bulletPrefabTypeId = -1;

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

    public void Shoot(Transform[] shootPoints, bool isPlayer = false) {
        if (shootPoints.Length != numShootPoints) {
            Debug.LogWarning("invalid number of shootpoints for " + name);
            return;
        }
        if (bulletPrefabTypeId == -1) {
            GetId();
        }
        // Debug.Log("Shooting " + name);
        GameObject[] gos = BulletManager.Instance.pool.Get(bulletPrefabTypeId, numShootPoints);
        for (int i = 0; i < numShootPoints; i++) {
            gos[i].transform.position = shootPoints[i].position;
            gos[i].transform.rotation = shootPoints[i].rotation;
            Vector2 forw = gos[i].transform.up;
            gos[i].GetComponent<Rigidbody2D>().AddForce(forw * launchForce, ForceMode2D.Impulse);
            Layer curLayer = isPlayer ? BulletManager.Instance.playerBulletLayer : BulletManager.Instance.enemyBulletLayer;
            curLayer.SetLayerAllChildren(gos[i]);
        }
    }
}