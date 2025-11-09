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

    // 🔹 МЕТОД ДЛЯ ПРОВЕРКИ ПУСТОГО ОРУЖИЯ
    public bool IsWeaponEmpty(Item weapon)
    {
        if (weapon == null || weapon.type != ItemType.Weapon)
            return true;

        return weapon.currentAmmo <= 0;
    }

    // 🔹 МЕТОД ДЛЯ ПРОВЕРКИ НУЖНА ЛИ ПЕРЕЗАРЯДКА
    // 🔹 УЛУЧШЕННЫЙ МЕТОД ДЛЯ ПРОВЕРКИ ПЕРЕЗАРЯДКИ КОНКРЕТНОГО ОРУЖИЯ
    public bool NeedsReload(Item weapon)
    {
        if (weapon == null || weapon.type != ItemType.Weapon || weapon.ammoType == AmmoType.None)
        {
            Debug.Log($"❌ {weapon?.itemName} - не оружие или нет типа патронов");
            return false;
        }

        // 🔹 ОРУЖИЕ НУЖДАЕТСЯ В ПЕРЕЗАРЯДКЕ ЕСЛИ:
        // 1. В нем не максимальное количество патронов
        // 2. И в инвентаре есть патроны для него
        bool notFull = weapon.currentAmmo < weapon.maxAmmo;
        bool hasAmmo = GetAmmoCount(weapon.ammoType) > 0;

        Debug.Log($"🔍 {weapon.itemName}: патроны {weapon.currentAmmo}/{weapon.maxAmmo}, неполное: {notFull}, есть патроны: {hasAmmo}");

        return notFull && hasAmmo;
    }

    public bool AddItem(Item item)
    {
        if (item == null) return false;

        // 🔹 ДЛЯ ПАТРОНОВ - ОСОБЕННАЯ ЛОГИКА
        if (item.type == ItemType.Ammo)
        {
            return AddAmmo(item);
        }

        if (item.isStackable)
        {
            foreach (var invItem in items)
            {
                // 🔹 ИСПРАВЛЕНО: сравниваем по itemName а не по ссылке
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
            // 🔹 СОЗДАЕМ КОПИЮ ПРЕДМЕТА
            Item newItem = Instantiate(item);
            items.Add(new InventoryItem { item = newItem, count = 1 });
            return true;
        }
        return false;
    }

    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ДЛЯ ДОБАВЛЕНИЯ ПАТРОНОВ
    private bool AddAmmo(Item ammoItem)
    {
        // Ищем патроны того же типа и того же названия
        foreach (var invItem in items)
        {
            if (invItem.item.type == ItemType.Ammo &&
                invItem.item.ammoType == ammoItem.ammoType &&
                invItem.item.itemName == ammoItem.itemName && // 🔹 ДОБАВИЛИ ПРОВЕРКУ НАЗВАНИЯ
                invItem.count < invItem.item.maxStackSize)
            {
                invItem.count++;
                return true;
            }
        }

        // Создаем новые патроны
        if (items.Count < maxSlots)
        {
            Item newAmmo = Instantiate(ammoItem);
            items.Add(new InventoryItem { item = newAmmo, count = 1 });
            return true;
        }
        return false;
    }

    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ДЛЯ ПОТРЕБЛЕНИЯ ПАТРОНОВ
    public bool ConsumeAmmo(AmmoType ammoType, int amount)
    {
        if (amount <= 0) return true;

        // Ищем ВСЕ патроны нужного типа
        List<InventoryItem> ammoItems = new List<InventoryItem>();
        foreach (var invItem in items)
        {
            if (invItem.item.type == ItemType.Ammo &&
                invItem.item.ammoType == ammoType)
            {
                ammoItems.Add(invItem);
            }
        }

        // Если патронов вообще нет
        if (ammoItems.Count == 0)
        {
            Debug.Log($"Нет патронов типа {ammoType} в инвентаре");
            return false;
        }

        // 🔹 ПРОВЕРЯЕМ ОБЩЕЕ КОЛИЧЕСТВО ПАТРОНОВ
        int totalAmmo = 0;
        foreach (var ammoItem in ammoItems)
        {
            totalAmmo += ammoItem.count;
        }

        if (totalAmmo < amount)
        {
            Debug.Log($"Недостаточно патронов: нужно {amount}, есть {totalAmmo}");
            return false;
        }

        // 🔹 ПОТРЕБЛЯЕМ ПАТРОНЫ ИЗ РАЗНЫХ СТАКОВ
        int totalConsumed = 0;
        foreach (var ammoItem in ammoItems)
        {
            if (totalConsumed >= amount) break;

            int canConsume = Mathf.Min(amount - totalConsumed, ammoItem.count);
            ammoItem.count -= canConsume;
            totalConsumed += canConsume;

            Debug.Log($"Потрачено {canConsume} патронов из стака {ammoItem.item.itemName}");
        }

        // 🔹 УДАЛЯЕМ ПУСТЫЕ СТАКИ
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].item.type == ItemType.Ammo && items[i].count <= 0)
            {
                Debug.Log($"Удален пустой стак патронов: {items[i].item.itemName}");
                items.RemoveAt(i);
            }
        }

        Debug.Log($"Всего потрачено патронов: {totalConsumed}/{amount}");
        return totalConsumed >= amount;
    }

    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ДЛЯ ПОЛУЧЕНИЯ КОЛИЧЕСТВА ПАТРОНОВ
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

    // 🔹 УЛУЧШЕННЫЙ МЕТОД ДЛЯ ПЕРЕЗАРЯДКИ ОРУЖИЯ
    public bool ReloadWeapon(Item weapon)
    {
        if (weapon.type != ItemType.Weapon || weapon.ammoType == AmmoType.None)
            return false;

        // 🔹 СКОЛЬКО ПАТРОНОВ НУЖНО ДО ЗАРЯЖЕНИЯ ДО МАКСИМУМА
        int ammoNeeded = weapon.maxAmmo - weapon.currentAmmo;

        // Если оружие уже полностью заряжено - ничего не делаем
        if (ammoNeeded <= 0)
        {
            Debug.Log($"{weapon.itemName} уже полностью заряжено");
            return false;
        }

        // 🔹 СКОЛЬКО ПАТРОНОВ ЕСТЬ В ИНВЕНТАРЕ
        int availableAmmo = GetAmmoCount(weapon.ammoType);

        // Если патронов нет - не заряжаем
        if (availableAmmo <= 0)
        {
            Debug.Log($"Нет патронов {weapon.ammoType} для {weapon.itemName}");
            return false;
        }

        // 🔹 СКОЛЬКО ПАТРОНОВ МОЖЕМ ИСПОЛЬЗОВАТЬ (не больше чем нужно и не больше чем есть)
        int ammoToUse = Mathf.Min(ammoNeeded, availableAmmo);

        // Потребляем патроны из инвентаря
        bool ammoConsumed = ConsumeAmmo(weapon.ammoType, ammoToUse);

        if (ammoConsumed)
        {
            // Заряжаем оружие
            weapon.currentAmmo += ammoToUse;
            Debug.Log($"✅ {weapon.itemName} заряжено: {weapon.currentAmmo}/{weapon.maxAmmo} (использовано {ammoToUse} патронов)");
            return true;
        }

        return false;
    }

    public void UseItem(Item item)
    {
        if (item == null) return;

        if (item.type == ItemType.Consumable)
        {
            item.UseConsumable(playerStats);
            Debug.Log($"Использовано: {item.itemName}");
            RemoveItem(item);
        }
        else
        {
            Debug.Log($"Нельзя использовать: {item.itemName}");
        }
    }

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
                return true;
            }
        }
        Debug.Log("⚠️ Предмет не найден в инвентаре: " + item.itemName);
        return false;
    }

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

    public void ClearAllItems()
    {
        items.Clear();
        Debug.Log("🗑️ Все предметы удалены из инвентаря");
    }
}