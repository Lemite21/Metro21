using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public Item item;
    public int count;
}

public class InventorySystem : MonoBehaviour
{
    public List<InventoryItem> items = new List<InventoryItem>();
    public int maxSlots = 20;

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
    }

    public bool AddItem(Item item)
    {
        if (item == null) return false;

        // Для патронов - особенная логика
        if (item.type == ItemType.Ammo)
        {
            return AddAmmo(item);
        }

        if (item.isStackable)
        {
            foreach (var invItem in items)
            {
                if (invItem.item.itemName == item.itemName &&
                    invItem.item.isStackable &&
                    invItem.count < invItem.item.maxStackSize)
                {
                    invItem.count++;
                    return true;
                }
            }
        }

        if (items.Count < maxSlots)
        {
            items.Add(new InventoryItem { item = item, count = 1 });
            return true;
        }
        return false;
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ДОБАВЛЕНИЯ ПАТРОНОВ
    private bool AddAmmo(Item ammoItem)
    {
        foreach (var invItem in items)
        {
            if (invItem.item.type == ItemType.Ammo &&
                invItem.item.ammoType == ammoItem.ammoType)
            {
                // Увеличиваем количество существующих патронов
                invItem.count += ammoItem.ammoAmount;
                return true;
            }
        }

        // Создаем новые патроны
        if (items.Count < maxSlots)
        {
            Item newAmmo = Instantiate(ammoItem);
            items.Add(new InventoryItem { item = newAmmo, count = newAmmo.ammoAmount });
            return true;
        }
        return false;
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ПОТРЕБЛЕНИЯ ПАТРОНОВ
    public bool ConsumeAmmo(AmmoType ammoType, int amount)
    {
        foreach (var invItem in items)
        {
            if (invItem.item.type == ItemType.Ammo &&
                invItem.item.ammoType == ammoType)
            {
                if (invItem.count >= amount)
                {
                    invItem.count -= amount;
                    // Удаляем если патроны закончились
                    if (invItem.count <= 0)
                    {
                        items.Remove(invItem);
                    }
                    return true;
                }
            }
        }
        return false;
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ПОЛУЧЕНИЯ КОЛИЧЕСТВА ПАТРОНОВ
    public int GetAmmoCount(AmmoType ammoType)
    {
        int total = 0;
        foreach (var invItem in items)
        {
            if (invItem.item.type == ItemType.Ammo &&
                invItem.item.ammoType == ammoType)
            {
                total += invItem.count;
            }
        }
        return total;
    }

    // 🔹 ОБНОВЛЕННЫЙ МЕТОД ДЛЯ ИСПОЛЬЗОВАНИЯ ПРЕДМЕТОВ
    public void UseItem(Item item)
    {
        if (item == null) return;

        if (item.type == ItemType.Consumable)
        {
            // Используем предмет потребления
            item.UseConsumable(playerStats);
            Debug.Log($"Использовано: {item.itemName}");

            // Удаляем предмет после использования
            RemoveItem(item);
        }
        else
        {
            Debug.Log($"Нельзя использовать: {item.itemName}");
        }
    }

    // 🔹 ИЗМЕНЯЕМ МЕТОД RemoveItem - ДОБАВЛЯЕМ ВОЗВРАТ bool
    public bool RemoveItem(Item item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].item != null && items[i].item.itemName == item.itemName)
            {
                if (item.isStackable && items[i].count > 1)
                {
                    items[i].count--;
                }
                else
                {
                    items.RemoveAt(i);
                }
                Debug.Log("✅ Удалён предмет: " + item.itemName);
                return true; // 🔹 ВОЗВРАЩАЕМ true ЕСЛИ УДАЛЕНИЕ УСПЕШНО
            }
        }
        Debug.Log("⚠️ Предмет не найден в инвентаре: " + item.itemName);
        return false; // 🔹 ВОЗВРАЩАЕМ false ЕСЛИ ПРЕДМЕТ НЕ НАЙДЕН
    }

    // 🔹 ДОБАВЛЯЕМ МЕТОД ДЛЯ ПРОВЕРКИ КОЛИЧЕСТВА ПРЕДМЕТОВ
    public int GetItemCount(Item item)
    {
        int count = 0;
        foreach (var invItem in items)
        {
            if (invItem.item != null && invItem.item.itemName == item.itemName)
            {
                count += invItem.count;
            }
        }
        return count;
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ОЧИСТКИ ВСЕХ ПРЕДМЕТОВ (при смерти)
    public void ClearAllItems()
    {
        items.Clear();
        Debug.Log("🗑️ Все предметы удалены из инвентаря");
    }
}