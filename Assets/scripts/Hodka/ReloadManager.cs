using UnityEngine;
using System.Collections.Generic;

public class ReloadManager : MonoBehaviour
{
    private InventorySystem inventory;
    private EquipmentSystem equipment;
    private PlayerStats playerStats;

    void Start()
    {
        inventory = FindFirstObjectByType<InventorySystem>();
        equipment = FindFirstObjectByType<EquipmentSystem>();
        playerStats = FindFirstObjectByType<PlayerStats>();
    }

    // 🔹 ОСНОВНОЙ МЕТОД ПЕРЕЗАРЯДКИ ВСЕГО ОРУЖИЯ
    public ReloadResult ReloadAllWeapons()
    {
        var result = new ReloadResult();

        // 🔹 ПЕРЕЗАРЯЖАЕМ ОСНОВНОЕ ОРУЖИЕ
        if (equipment.weaponMain.item != null && equipment.weaponMain.item.type == ItemType.Weapon)
        {
            var weapon = equipment.weaponMain.item;
            result.mainWeaponReloaded = ReloadSingleWeapon(weapon, "Основное оружие");
        }

        // 🔹 ПЕРЕЗАРЯЖАЕМ ДОПОЛНИТЕЛЬНОЕ ОРУЖИЕ
        if (equipment.weaponSecondary.item != null && equipment.weaponSecondary.item.type == ItemType.Weapon)
        {
            var weapon = equipment.weaponSecondary.item;
            result.secondaryWeaponReloaded = ReloadSingleWeapon(weapon, "Доп. оружие");
        }

        return result;
    }

    // 🔹 ПЕРЕЗАРЯДКА ОДНОГО ОРУЖИЯ
    private bool ReloadSingleWeapon(Item weapon, string weaponName)
    {
        if (weapon.type != ItemType.Weapon || weapon.ammoType == AmmoType.None)
            return false;

        // Проверяем нужно ли перезаряжать
        int ammoNeeded = weapon.maxAmmo - weapon.currentAmmo;
        if (ammoNeeded <= 0)
        {
            Debug.Log($"{weaponName} {weapon.itemName} уже полностью заряжено");
            return false;
        }

        // Проверяем есть ли патроны
        int availableAmmo = inventory.GetAmmoCount(weapon.ammoType);
        if (availableAmmo <= 0)
        {
            Debug.Log($"Нет патронов {weapon.ammoType} для {weaponName}");
            return false;
        }

        // 🔹 РАСЧЕТ СКОЛЬКО ПАТРОНОВ МОЖЕМ ИСПОЛЬЗОВАТЬ
        int ammoToUse = Mathf.Min(ammoNeeded, availableAmmo);

        // Потребляем патроны из инвентаря
        bool ammoConsumed = inventory.ConsumeAmmo(weapon.ammoType, ammoToUse);

        if (ammoConsumed)
        {
            int oldAmmo = weapon.currentAmmo;
            weapon.currentAmmo += ammoToUse;
            Debug.Log($"✅ {weaponName} {weapon.itemName} заряжено: {oldAmmo} → {weapon.currentAmmo}/{weapon.maxAmmo} (+{ammoToUse})");
            return true;
        }

        return false;
    }

    // 🔹 ПРОВЕРКА ДОСТУПНОСТИ ПЕРЕЗАРЯДКИ
    public bool CanReloadAnyWeapon()
    {
        if (equipment.weaponMain.item != null && CanReloadWeapon(equipment.weaponMain.item))
            return true;

        if (equipment.weaponSecondary.item != null && CanReloadWeapon(equipment.weaponSecondary.item))
            return true;

        return false;
    }

    // 🔹 ПРОВЕРКА МОЖНО ЛИ ПЕРЕЗАРЯДИТЬ КОНКРЕТНОЕ ОРУЖИЕ
    private bool CanReloadWeapon(Item weapon)
    {
        if (weapon.type != ItemType.Weapon || weapon.ammoType == AmmoType.None)
            return false;

        bool needsReload = weapon.currentAmmo < weapon.maxAmmo;
        bool hasAmmo = inventory.GetAmmoCount(weapon.ammoType) > 0;

        return needsReload && hasAmmo;
    }

    // 🔹 ПОЛУЧЕНИЕ СТАТУСА ПЕРЕЗАРЯДКИ ДЛЯ UI
    public string GetReloadStatus()
    {
        string status = "";

        if (equipment.weaponMain.item != null)
        {
            var weapon = equipment.weaponMain.item;
            status += $"Основное: {weapon.currentAmmo}/{weapon.maxAmmo}";
            if (CanReloadWeapon(weapon)) status += " [Нужна перезарядка]";
            status += "\n";
        }

        if (equipment.weaponSecondary.item != null)
        {
            var weapon = equipment.weaponSecondary.item;
            status += $"Доп. оружие: {weapon.currentAmmo}/{weapon.maxAmmo}";
            if (CanReloadWeapon(weapon)) status += " [Нужна перезарядка]";
        }

        return status;
    }
}

// 🔹 СТРУКТУРА ДЛЯ РЕЗУЛЬТАТОВ ПЕРЕЗАРЯДКИ
public struct ReloadResult
{
    public bool mainWeaponReloaded;
    public bool secondaryWeaponReloaded;
    public bool anyWeaponReloaded => mainWeaponReloaded || secondaryWeaponReloaded;
}