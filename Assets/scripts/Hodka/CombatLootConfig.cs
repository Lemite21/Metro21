using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CombatLootConfig", menuName = "Combat/Loot Configuration")]
public class CombatLootConfig : ScriptableObject
{
    [System.Serializable]
    public class EnemyLoot
    {
        public CombatSystem.EnemyType enemyType;
        public List<CombatSystem.LootItem> lootTable;
    }

    public List<EnemyLoot> enemyLootTables = new List<EnemyLoot>();

    public List<CombatSystem.LootItem> GetLootTable(CombatSystem.EnemyType enemyType)
    {
        foreach (var enemyLoot in enemyLootTables)
        {
            if (enemyLoot.enemyType == enemyType)
                return enemyLoot.lootTable;
        }
        return new List<CombatSystem.LootItem>();
    }
}