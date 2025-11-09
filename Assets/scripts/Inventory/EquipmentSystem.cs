using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EquipmentSlot
{
    public Item item;
    public string slotName;
}

public class EquipmentSystem : MonoBehaviour
{
    public EquipmentSlot helmet = new EquipmentSlot() { slotName = "Шлем" };
    public EquipmentSlot chest = new EquipmentSlot() { slotName = "Нагрудник" };
    public EquipmentSlot legs = new EquipmentSlot() { slotName = "Штаны" };
    public EquipmentSlot weaponMain = new EquipmentSlot() { slotName = "Основное оружие" };
    public EquipmentSlot weaponSecondary = new EquipmentSlot() { slotName = "Пистолет" };

    private InventorySystem inventory;
    private PlayerStats playerStats;

    void Start()
    {
        inventory = FindFirstObjectByType<InventorySystem>();
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (inventory == null)
            Debug.LogError("InventorySystem не найден!");
    }

    // 🔹 ОБНОВЛЕННЫЙ МЕТОД ДЛЯ ЭКИПИРОВКИ
    public bool TryEquip(Item item)
    {
        if (item == null || !item.IsEquippable) return false;

        EquipmentSlot targetSlot = null;

        if (item.type == ItemType.Armor)
        {
            switch (item.GetArmorType())
            {
                case ArmorType.Helmet:
                    targetSlot = helmet;
                    break;
                case ArmorType.Chest:
                    targetSlot = chest;
                    break;
                case ArmorType.Legs:
                    targetSlot = legs;
                    break;
                default:
                    Debug.Log("❌ Неизвестный подтип брони");
                    return false;
            }
        }
        else if (item.type == ItemType.Weapon)
        {
            // Пистолет → второй слот
            if (item.GetWeaponType() == WeaponType.Pistol)
                targetSlot = weaponSecondary;
            else
                targetSlot = weaponMain;
        }

        if (targetSlot == null)
            return false;

        // Если слот уже занят — снимаем предмет
        if (targetSlot.item != null)
        {
            inventory.AddItem(targetSlot.item);
            Debug.Log($"🔄 Заменено: {targetSlot.item.itemName}");
        }

        targetSlot.item = item;
        inventory.RemoveItem(item);

        // 🔹 ОБНОВЛЯЕМ ЗДОРОВЬЕ ПРИ ЭКИПИРОВКЕ БРОНИ
        if (item.type == ItemType.Armor)
        {
            playerStats.UpdateUI(); // Теперь этот метод public
        }

        Debug.Log($"✅ Экипировано: {item.itemName} → {targetSlot.slotName}");
        return true;
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ПОЛУЧЕНИЯ УРОНА ОРУЖИЯ
    public int GetWeaponDamage(bool isMainWeapon)
    {
        Item weapon = isMainWeapon ? weaponMain.item : weaponSecondary.item;
        if (weapon == null) return 0;
        return weapon.GetWeaponDamage();
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ПРОВЕРКИ КЛИНА
    public bool CheckWeaponJamming(bool isMainWeapon)
    {
        Item weapon = isMainWeapon ? weaponMain.item : weaponSecondary.item;
        if (weapon == null || !weapon.hasDurability) return false;

        float jamChance = weapon.GetJammingChance();
        return Random.Range(0f, 1f) < jamChance;
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ИСПОЛЬЗОВАНИЯ ОРУЖИЯ
    public void UseWeapon(bool isMainWeapon)
    {
        Item weapon = isMainWeapon ? weaponMain.item : weaponSecondary.item;
        if (weapon != null)
        {
            weapon.UseInCombat();
        }
    }

    // Снимает предмет из слота
    public void Unequip(EquipmentSlot slot)
    {
        if (slot.item == null) return;
        inventory.AddItem(slot.item);

        // 🔹 ОБНОВЛЯЕМ ЗДОРОВЬЕ ПРИ СНЯТИИ БРОНИ
        if (slot.item.type == ItemType.Armor)
        {
            playerStats.UpdateUI(); // Теперь этот метод public
        }

        slot.item = null;
        Debug.Log($"Снято: {slot.slotName}");
    }

    // Проверяет, экипирован ли предмет
    public bool IsEquipped(Item item)
    {
        return item != null &&
               (helmet.item == item || chest.item == item || legs.item == item ||
                weaponMain.item == item || weaponSecondary.item == item);
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ПРОВЕРКИ ПУСТОГО ОРУЖИЯ
    public bool IsWeaponEmpty(bool isMainWeapon)
    {
        Item weapon = isMainWeapon ? weaponMain.item : weaponSecondary.item;
        return weapon == null || weapon.IsWeaponEmpty();
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ПЕРЕЗАРЯДКИ ОРУЖИЯ
    public bool ReloadWeapon(bool isMainWeapon, int availableAmmo)
    {
        Item weapon = isMainWeapon ? weaponMain.item : weaponSecondary.item;
        if (weapon == null) return false;
        return weapon.ReloadWeapon(availableAmmo);
    }

    // 🔹 МЕТОД ДЛЯ ПОЛУЧЕНИЯ ВСЕГО ОРУЖИЯ ИГРОКА
    public List<Item> GetAllWeapons()
    {
        List<Item> weapons = new List<Item>();

        if (weaponMain.item != null && weaponMain.item.type == ItemType.Weapon)
            weapons.Add(weaponMain.item);

        if (weaponSecondary.item != null && weaponSecondary.item.type == ItemType.Weapon)
            weapons.Add(weaponSecondary.item);

        return weapons;
    }

    // 🔹 МЕТОД ДЛЯ ПРОВЕРКИ - НУЖНА ЛИ ПЕРЕЗАРЯДКА ХОТЯ БЫ ОДНОМУ ОРУЖИЮ
    // 🔹 МЕТОД ДЛЯ ПРОВЕРКИ - НУЖНА ЛИ ПЕРЕЗАРЯДКА ХОТЯ БЫ ОДНОМУ ОРУЖИЮ
    public bool AnyWeaponNeedsReload(InventorySystem inventory)
    {
        if (weaponMain.item != null && weaponMain.item.type == ItemType.Weapon)
        {
            if (inventory.NeedsReload(weaponMain.item))
            {
                Debug.Log($"Основное оружие {weaponMain.item.itemName} нуждается в перезарядке");
                return true;
            }
        }

        if (weaponSecondary.item != null && weaponSecondary.item.type == ItemType.Weapon)
        {
            if (inventory.NeedsReload(weaponSecondary.item))
            {
                Debug.Log($"Дополнительное оружие {weaponSecondary.item.itemName} нуждается в перезарядке");
                return true;
            }
        }

        Debug.Log("Ни одно оружие не нуждается в перезарядке");
        return false;
    }

}