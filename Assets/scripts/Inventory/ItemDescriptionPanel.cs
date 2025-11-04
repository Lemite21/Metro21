using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescriptionPanel : MonoBehaviour
{
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public Button useButton;
    public Button deleteButton;
    public Button equipButton;

    private Item currentItem;
    private InventorySystem inventory;
    private InventoryUI ui;
    private EquipmentSystem equipment;
    private PlayerStats playerStats;

    public void Setup(Item item, InventorySystem inv, InventoryUI inventoryUI)
    {
        currentItem = item;
        inventory = inv;
        ui = inventoryUI;
        equipment = FindFirstObjectByType<EquipmentSystem>();
        playerStats = FindFirstObjectByType<PlayerStats>();

        // Обновляем UI
        itemIcon.sprite = item.icon;
        itemIcon.enabled = item.icon != null;
        itemNameText.text = item.itemName;
        itemDescriptionText.text = GetEnhancedDescription(item);

        // Скрываем все кнопки
        useButton.gameObject.SetActive(false);
        deleteButton.gameObject.SetActive(true);
        equipButton.gameObject.SetActive(false);

        // 🔹 ОБНОВЛЕННАЯ ЛОГИКА КНОПОК
        if (item.IsEquippable)
        {
            // Для оружия/брони — показываем "Надеть" или "Снять"
            equipButton.gameObject.SetActive(true);
            equipButton.onClick.RemoveAllListeners();
            equipButton.GetComponentInChildren<TMP_Text>().text = equipment.IsEquipped(item) ? "Снять" : "Надеть";

            if (equipment.IsEquipped(item))
                equipButton.onClick.AddListener(UnequipItem);
            else
                equipButton.onClick.AddListener(EquipItem);
        }
        else if (item.type == ItemType.Consumable)
        {
            // Для еды/медикаментов — показываем "Использовать"
            useButton.gameObject.SetActive(true);
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(UseItem);
        }
        else if (item.type == ItemType.Ammo)
        {
            // Для патронов — показываем информацию
            useButton.gameObject.SetActive(false);
        }
        else
        {
            // Misc — просто описание
            useButton.gameObject.SetActive(false);
        }

        // Кнопка удаления
        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(DeleteItem);
    }

    // 🔹 УЛУЧШЕННОЕ ОПИСАНИЕ ПРЕДМЕТА
    string GetEnhancedDescription(Item item)
    {
        string description = item.description + "\n\n";

        if (item.type == ItemType.Weapon)
        {
            description += $"⚔️ Урон: {item.minDamage}-{item.maxDamage}\n";
            description += $"🎯 Патроны: {item.ammoType} ({item.currentAmmo}/{item.maxAmmo})\n";
            description += $"🔧 Прочность: {item.currentDurability:F0}%\n";
        }
        else if (item.type == ItemType.Armor)
        {
            description += $"🛡️ Защита: +{item.armorValue} HP\n";
            description += $"🔧 Прочность: {item.currentDurability:F0}%\n";
        }
        else if (item.type == ItemType.Consumable)
        {
            if (item.healthRestore > 0) description += $"❤️ Здоровье: +{item.healthRestore}\n";
            if (item.foodRestore > 0) description += $"🍖 Еда: +{item.foodRestore}\n";
            if (item.waterRestore > 0) description += $"💧 Вода: +{item.waterRestore}\n";
            if (item.radiationRemove > 0) description += $"☢️ Радиация: -{item.radiationRemove}\n";
            if (item.energyRestore > 0) description += $"⚡ Энергия: +{item.energyRestore}\n";
        }
        else if (item.type == ItemType.Ammo)
        {
            description += $"🎯 Тип: {item.ammoType}\n";
            description += $"🔫 Количество: {item.ammoAmount} патронов\n";
        }

        description += $"💰 Цена продажи: {item.sellPrice} руб";

        return description;
    }

    void UseItem()
    {
        inventory.UseItem(currentItem);
        ui.RefreshUI();
        gameObject.SetActive(false);
    }

    void DeleteItem()
    {
        Debug.Log("Удаляю: " + currentItem.itemName);
        inventory.RemoveItem(currentItem);
        ui.RefreshUI();
        gameObject.SetActive(false);
    }

    void EquipItem()
    {
        if (equipment.TryEquip(currentItem))
        {
            ui.RefreshUI();
            var equipUI = FindFirstObjectByType<EquipmentUI>();
            equipUI?.RefreshUI();
            gameObject.SetActive(false);
        }
    }

    void UnequipItem()
    {
        // Определяем, из какого слота снимать
        if (equipment.helmet.item == currentItem) equipment.Unequip(equipment.helmet);
        else if (equipment.chest.item == currentItem) equipment.Unequip(equipment.chest);
        else if (equipment.legs.item == currentItem) equipment.Unequip(equipment.legs);
        else if (equipment.weaponMain.item == currentItem) equipment.Unequip(equipment.weaponMain);
        else if (equipment.weaponSecondary.item == currentItem) equipment.Unequip(equipment.weaponSecondary);

        ui.RefreshUI();
        var equipUI = FindFirstObjectByType<EquipmentUI>();
        equipUI?.RefreshUI();
        gameObject.SetActive(false);
    }
}