using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RepairSystem : MonoBehaviour
{
    [Header("Repair UI")]
    public GameObject repairPanel;
    public Image repairSlotIcon;
    public TMP_Text itemNameText;
    public TMP_Text conditionText;
    public TMP_Text repairCostText;
    public Button repairButton;
    public Button cancelButton;

    [Header("References")]
    private PlayerWallet playerWallet;
    private InventorySystem inventory;
    private InventoryUI inventoryUI;
    private Item selectedRepairItem;

    void Start()
    {
        playerWallet = FindFirstObjectByType<PlayerWallet>();
        inventory = FindFirstObjectByType<InventorySystem>();
        inventoryUI = FindFirstObjectByType<InventoryUI>();

        if (repairPanel != null) repairPanel.SetActive(false);

        // Настраиваем кнопки
        repairButton.onClick.RemoveAllListeners();
        repairButton.onClick.AddListener(RepairItem);

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(CancelRepair);

        // Изначально кнопка ремонта неактивна
        repairButton.interactable = false;
    }

    // 🔹 ОТКРЫТЬ ПАНЕЛЬ РЕМОНТА
    public void ShowRepairPanel()
    {
        if (repairPanel != null)
        {
            repairPanel.SetActive(true);
            ClearSelection();
            Debug.Log("Панель ремонта открыта - можно выбирать предметы");
        }
        else
        {
            Debug.LogError("RepairPanel не назначен в инспекторе!");
        }
    }

    // 🔹 ВЫБРАТЬ ПРЕДМЕТ ДЛЯ РЕМОНТА
    public void SelectItemForRepair(Item item)
    {
        if (item == null) return;

        // 🔹 ПРОВЕРЯЕМ МОЖНО ЛИ РЕМОНТИРОВАТЬ ЭТОТ ПРЕДМЕТ
        if (!CanRepairItem(item))
        {
            Debug.Log($"Этот предмет нельзя починить: {item.itemName}");
            return;
        }

        // 🔹 СНИМАЕМ ПРЕДЫДУЩЕЕ ВЫДЕЛЕНИЕ
        ClearSelection();

        // 🔹 ВЫБИРАЕМ НОВЫЙ ПРЕДМЕТ
        selectedRepairItem = item;
        UpdateRepairUI();

        Debug.Log($"Выбран для ремонта: {item.itemName}");
    }

    // 🔹 ПРОВЕРКА МОЖНО ЛИ РЕМОНТИРОВАТЬ ПРЕДМЕТ
    private bool CanRepairItem(Item item)
    {
        if (item == null) return false;

        // Можно ремонтировать только оружие и броню с прочностью
        bool canRepair = (item.type == ItemType.Weapon || item.type == ItemType.Armor) &&
                        item.hasDurability;

        Debug.Log($"Проверка предмета {item.itemName}: type={item.type}, hasDurability={item.hasDurability}, canRepair={canRepair}");

        return canRepair;
    }

    // 🔹 ОБНОВИТЬ UI РЕМОНТА
    private void UpdateRepairUI()
    {
        if (selectedRepairItem == null) return;

        // Обновляем информацию о предмете
        repairSlotIcon.sprite = selectedRepairItem.icon;
        repairSlotIcon.enabled = true;
        itemNameText.text = selectedRepairItem.itemName;

        // Рассчитываем стоимость и состояние
        int repairCost = selectedRepairItem.GetRepairCost();
        float conditionPercent = (selectedRepairItem.currentDurability / selectedRepairItem.maxDurability) * 100f;

        conditionText.text = $"Состояние: {conditionPercent:F0}%";
        repairCostText.text = $"Цена ремонта: {repairCost} руб";

        // 🔹 АКТИВИРУЕМ КНОПКУ ТОЛЬКО ЕСЛИ ПРЕДМЕТ НУЖДАЕТСЯ В РЕМОНТЕ И ХВАТАЕТ ДЕНЕГ
        bool canRepair = selectedRepairItem.NeedsRepair && playerWallet.HasEnoughMoney(repairCost);
        repairButton.interactable = canRepair;

        Debug.Log($"Обновлена информация: состояние {conditionPercent:F0}%, цена {repairCost}, можно чинить: {canRepair}");
    }

    // 🔹 ПОЧИНИТЬ ПРЕДМЕТ
    private void RepairItem()
    {
        if (selectedRepairItem == null || !selectedRepairItem.NeedsRepair)
        {
            Debug.Log("Нечего чинить или предмет не требует ремонта");
            return;
        }

        int repairCost = selectedRepairItem.GetRepairCost();

        if (playerWallet.SpendMoney(repairCost))
        {
            // Восстанавливаем прочность до максимума
            selectedRepairItem.Repair();

            Debug.Log($"✅ Отремонтировано: {selectedRepairItem.itemName} за {repairCost} руб");

            // 🔹 СКРЫВАЕМ ИНВЕНТАРЬ ПОСЛЕ РЕМОНТА
            if (inventoryUI != null)
            {
                inventoryUI.HideInventoryPanels();
            }

            // 🔹 ЗАКРЫВАЕМ ПАНЕЛЬ РЕМОНТА
            CloseRepairPanel();

            // Обновляем инвентарь
            if (inventoryUI != null) inventoryUI.RefreshUI();
        }
        else
        {
            Debug.Log("Недостаточно денег для ремонта");
        }
    }

    // 🔹 ОТМЕНА РЕМОНТА
    private void CancelRepair()
    {
        ClearSelection();
        CloseRepairPanel();
    }

    // 🔹 ОЧИСТИТЬ ВЫБОР
    private void ClearSelection()
    {
        selectedRepairItem = null;

        if (repairSlotIcon != null) repairSlotIcon.enabled = false;
        if (itemNameText != null) itemNameText.text = "Выберите предмет";
        if (conditionText != null) conditionText.text = "Состояние: -";
        if (repairCostText != null) repairCostText.text = "Цена ремонта: -";
        if (repairButton != null) repairButton.interactable = false;

        // 🔹 СНИМАЕМ ВЫДЕЛЕНИЕ С ПРЕДМЕТОВ В ИНВЕНТАРЕ
        UpdateInventoryHighlight();
    }

    // 🔹 ПРОВЕРИТЬ ВЫБРАН ЛИ ПРЕДМЕТ
    public bool IsItemSelected(Item item)
    {
        return selectedRepairItem == item;
    }

    // 🔹 ОБНОВИТЬ ПОДСВЕТКУ В ИНВЕНТАРЕ
    private void UpdateInventoryHighlight()
    {
        if (inventoryUI != null && inventoryUI.itemSlotsParent != null)
        {
            foreach (Transform slotTransform in inventoryUI.itemSlotsParent)
            {
                ItemSlotUI slot = slotTransform.GetComponent<ItemSlotUI>();
                if (slot != null)
                {
                    Item item = slot.GetItem();
                    Image slotImage = slotTransform.GetComponent<Image>();
                    Outline outline = slotTransform.GetComponent<Outline>();

                    if (outline == null)
                    {
                        outline = slotTransform.gameObject.AddComponent<Outline>();
                        outline.effectColor = Color.blue; // 🔹 СИНИЙ ЦВЕТ ДЛЯ РЕМОНТА
                        outline.effectDistance = new Vector2(3, 3);
                        outline.enabled = false;
                    }

                    if (item != null && IsItemSelected(item))
                    {
                        // 🔹 ПОДСВЕТКА ВЫБРАННОГО ПРЕДМЕТА
                        slotImage.color = new Color(0.7f, 0.8f, 1f, 1f); // Светло-синий
                        outline.enabled = true;
                    }
                    else
                    {
                        // Обычный вид
                        slotImage.color = Color.white;
                        if (outline != null) outline.enabled = false;
                    }
                }
            }
        }
    }

    // 🔹 ЗАКРЫТЬ ПАНЕЛЬ РЕМОНТА
    public void CloseRepairPanel()
    {
        if (repairPanel != null)
        {
            repairPanel.SetActive(false);
            ClearSelection();
            Debug.Log("Панель ремонта закрыта");
        }
    }
}