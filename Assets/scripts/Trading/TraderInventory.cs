// Assets/Scripts/Trading/TraderInventory.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewTraderInventory", menuName = "Trading/Trader Inventory")]
public class TraderInventory : ScriptableObject
{
    public List<Item> availableItems = new List<Item>();
}