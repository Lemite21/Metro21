// Assets/Scripts/Inventory/Item.cs
using UnityEngine;

public enum ItemType
{
    Weapon,
    Armor,
    Consumable, // еда/медикаменты
    Misc,        // предметы
    Ammo         // 🔹 НОВЫЙ ТИП: патроны
}

public enum ArmorType
{
    None,
    Helmet,
    Chest,
    Legs
}

public enum WeaponType
{
    Any,        // любое оружие
    Pistol      // только пистолеты
}

// 🔹 НОВЫЙ ENUM ДЛЯ ТИПОВ ПАТРОНОВ
public enum AmmoType
{
    None,
    Pistol,
    Shotgun,
    Rifle,
    Explosive
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public bool isStackable = false;
    public int maxStackSize = 1;
    public int currentStack = 1;

    public ItemType type;

    [SerializeField] private ArmorType armorType = ArmorType.None;
    public ArmorType GetArmorType() => armorType;

    // Только для оружия
    [SerializeField] private WeaponType weaponType = WeaponType.Any;
    public WeaponType GetWeaponType() => weaponType;

    // Удобный метод: можно ли надеть?
    public bool IsEquippable => type == ItemType.Weapon || type == ItemType.Armor;

    // 🔹 НОВЫЕ ПОЛЯ ДЛЯ ТОРГОВЛИ И РЕМОНТА
    [Header("Trade Settings")]
    public int buyPrice = 100;
    public int sellPrice = 50;

    [Header("Durability Settings")]
    public bool hasDurability = false;
    [Range(0, 100)] public float currentDurability = 100f;
    [Range(0, 100)] public float maxDurability = 100f;

    // 🔹 НОВЫЕ ПОЛЯ ДЛЯ СИСТЕМЫ БОЯ - ОРУЖИЕ
    [Header("Weapon Combat Settings")]
    public int minDamage = 0;
    public int maxDamage = 0;
    public AmmoType ammoType = AmmoType.None;
    public int maxAmmo = 0;
    public int currentAmmo = 0;

    // 🔹 НОВЫЕ ПОЛЯ ДЛЯ СИСТЕМЫ БОЯ - БРОНЯ
    [Header("Armor Combat Settings")]
    public int armorValue = 0;

    // 🔹 НОВЫЕ ПОЛЯ ДЛЯ ПРЕДМЕТОВ ПОТРЕБЛЕНИЯ
    [Header("Consumable Effects")]
    public int healthRestore = 0;
    public int foodRestore = 0;
    public int waterRestore = 0;
    public int radiationRemove = 0;
    public int energyRestore = 0;

    // 🔹 НОВЫЕ ПОЛЯ ДЛЯ ПАТРОНОВ
    [Header("Ammo Settings")]
    public int ammoAmount = 0; // количество патронов в стаке

    // Свойство для проверки, требует ли ремонта
    public bool NeedsRepair => hasDurability && currentDurability < maxDurability;

    // 🔹 ОБНОВЛЕННЫЙ МЕТОД ДЛЯ ИСПОЛЬЗОВАНИЯ ПРЕДМЕТА (оружие в бою)
    public void UseInCombat()
    {
        if (!hasDurability) return;

        float durabilityLoss = Random.Range(1f, 3f);
        currentDurability = Mathf.Max(0, currentDurability - durabilityLoss);

        // 🔹 ТРАТИМ ПАТРОНЫ ЕСЛИ ЭТО ОРУЖИЕ
        if (type == ItemType.Weapon && currentAmmo > 0)
        {
            currentAmmo--;
        }
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ПЕРЕЗАРЯДКИ ОРУЖИЯ
    public bool ReloadWeapon(int availableAmmo)
    {
        if (type != ItemType.Weapon || ammoType == AmmoType.None)
            return false;

        int ammoNeeded = maxAmmo - currentAmmo;

        if (ammoNeeded <= 0)
            return false; // уже полностью заряжено

        int ammoToUse = Mathf.Min(ammoNeeded, availableAmmo);
        currentAmmo += ammoToUse;

        return ammoToUse > 0;
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ПРОВЕРКИ ПУСТОГО ОРУЖИЯ
    public bool IsWeaponEmpty()
    {
        return type == ItemType.Weapon && currentAmmo <= 0;
    }

    // Метод для получения урона броней
    public void TakeDamage()
    {
        if (!hasDurability) return;

        float durabilityLoss = Random.Range(1f, 3f);
        currentDurability = Mathf.Max(0, currentDurability - durabilityLoss);
    }

    // Метод для ремонта
    public void Repair()
    {
        currentDurability = maxDurability;
    }

    // Метод для получения цены ремонта
    public int GetRepairCost()
    {
        if (!hasDurability) return 0;

        float conditionPercent = (currentDurability / maxDurability) * 100f;

        if (conditionPercent >= 80f) return 800;
        else if (conditionPercent >= 50f) return 1500;
        else if (conditionPercent >= 25f) return 5000;
        else return 10000;
    }

    // Метод для получения шанса клина (для оружия)
    public float GetJammingChance()
    {
        if (!hasDurability) return 0f;

        float conditionPercent = (currentDurability / maxDurability) * 100f;

        if (conditionPercent <= 0f) return 1f;      // 100%
        else if (conditionPercent <= 25f) return 0.5f;   // 50%
        else if (conditionPercent <= 50f) return 0.15f;  // 15%
        else if (conditionPercent <= 80f) return 0.05f;  // 5%
        else return 0f;                                // 0%
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ИСПОЛЬЗОВАНИЯ ПРЕДМЕТОВ ПОТРЕБЛЕНИЯ
    public void UseConsumable(PlayerStats playerStats)
    {
        if (type != ItemType.Consumable) return;

        if (healthRestore > 0) playerStats.RestoreHealth(healthRestore);
        if (foodRestore > 0) playerStats.ChangeFood(foodRestore);
        if (waterRestore > 0) playerStats.ChangeWater(waterRestore);
        if (radiationRemove > 0) playerStats.ChangeRadiation(-radiationRemove);
        if (energyRestore > 0) playerStats.ChangeEnergy(energyRestore);
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ПОЛУЧЕНИЯ УРОНА ОРУЖИЯ
    public int GetWeaponDamage()
    {
        if (type != ItemType.Weapon) return 0;
        return Random.Range(minDamage, maxDamage + 1);
    }
}