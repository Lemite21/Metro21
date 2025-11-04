using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("Основные параметры")]
    public int baseMaxHealth = 100;
    public int currentHealth = 100;
    public int energy = 100;
    public int food = 100;
    public int water = 100;
    public int radiation = 0;

    [Header("UI Elements")]
    public TMP_Text healthText;
    public TMP_Text energyText;
    public TMP_Text foodText;
    public TMP_Text waterText;
    public TMP_Text radiationText;

    private EquipmentSystem equipment;
    private InventoryUI inventoryUI;

    void Start()
    {
        equipment = FindFirstObjectByType<EquipmentSystem>();
        inventoryUI = FindFirstObjectByType<InventoryUI>();
        UpdateUI();
    }

    public int GetTotalArmor()
    {
        int totalArmor = 0;
        if (equipment.helmet.item != null) totalArmor += equipment.helmet.item.armorValue;
        if (equipment.chest.item != null) totalArmor += equipment.chest.item.armorValue;
        if (equipment.legs.item != null) totalArmor += equipment.legs.item.armorValue;
        return totalArmor;
    }

    public int GetMaxHealthWithArmor()
    {
        return baseMaxHealth + GetTotalArmor();
    }

    public void TakeDamage(int damage)
    {
        // Наносим урон броне
        if (equipment.helmet.item != null) equipment.helmet.item.TakeDamage();
        if (equipment.chest.item != null) equipment.chest.item.TakeDamage();
        if (equipment.legs.item != null) equipment.legs.item.TakeDamage();

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        // Обновляем UI брони
        if (inventoryUI != null)
        {
            var equipUI = FindFirstObjectByType<EquipmentUI>();
            if (equipUI != null) equipUI.RefreshUI();
        }

        UpdateUI();
    }

    public void RestoreHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth > GetMaxHealthWithArmor()) currentHealth = GetMaxHealthWithArmor();
        UpdateUI();
    }

    public void ChangeEnergy(int amount)
    {
        energy += amount;
        if (energy > 100) energy = 100;
        if (energy < 0) energy = 0;
        UpdateUI();
    }

    public void ChangeFood(int amount)
    {
        food = Mathf.Clamp(food + amount, 0, 100);
        UpdateUI();
    }

    public void ChangeWater(int amount)
    {
        water = Mathf.Clamp(water + amount, 0, 100);
        UpdateUI();
    }

    public void ChangeRadiation(int amount)
    {
        radiation = Mathf.Clamp(radiation + amount, 0, 100);
        UpdateUI();
    }

    public void UpdateUI()
    {
        // 🔹 ИСПРАВЛЕНО: показываем только значения без "/100"
        if (healthText != null) healthText.text = $"{currentHealth}";
        if (energyText != null) energyText.text = $"{energy}";
        if (foodText != null) foodText.text = $"{food}";
        if (waterText != null) waterText.text = $"{water}";
        if (radiationText != null) radiationText.text = $"{radiation}";
    }

    public bool CanContinueJourney()
    {
        return food > 0 && water > 0 && currentHealth > 0;
    }

    public void RestoreAtBase()
    {
        energy = 100;
        UpdateUI();
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}