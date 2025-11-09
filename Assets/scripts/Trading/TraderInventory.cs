using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TraderItem
{
    public Item item;
    public int quantity = 1; // 🔹 НОВОЕ ПОЛЕ: количество которое дается при покупке
}

[CreateAssetMenu(fileName = "NewTraderInventory", menuName = "Trading/Trader Inventory")]
public class TraderInventory : ScriptableObject
{
    public List<TraderItem> availableItems = new List<TraderItem>(); // 🔹 ИЗМЕНИЛИ ТИП
}