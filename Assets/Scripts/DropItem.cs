using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour {

    [Header("Drop rates")]
    [Min(0)]
    public int numCoinsToDrop = 1;
    public EnemyAI.DropRate[] dropRates = new EnemyAI.DropRate[0];

    [ContextMenu("NormalizeDropRates")]
    void NormalizeDropRates() {
        float total = 0;
        foreach (var droprate in dropRates) {
            total += droprate.chance;
        }
        foreach (var droprate in dropRates) {
            droprate.chance /= total;
        }
    }

    public void DropAnItem() {
        // drop coins
        LevelManager.Instance.DropCoins(numCoinsToDrop, transform.position);
        // chance to drop weapon
        NormalizeDropRates();
        float val = Random.value;
        float acc = 0;
        foreach (var droprate in dropRates) {
            if (val >= acc && val < acc + droprate.chance) {
                if (droprate.weaponType != null) {
                    // is null for nothing
                    // drop this item
                    LevelManager.Instance.DropWeapon(droprate.weaponType, transform.position, droprate.ammo);
                }
                break;
            }
            acc += droprate.chance;
        }
    }
}