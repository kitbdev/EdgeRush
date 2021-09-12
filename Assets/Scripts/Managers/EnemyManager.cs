using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager> {

    public List<EnemyAI> activeEnemies = new List<EnemyAI>();

}