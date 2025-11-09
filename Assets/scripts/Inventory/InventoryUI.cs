using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject inventoryPanel;
    public GameObject itemDescriptionPanel;
    public Transform itemSlotsParent;
    public GameObject itemSlotPrefab;

    [Header("Inventory Buttons")]
    public Button inventoryButton; // 🔹 Обычная кнопка инвентаря
    public Button disabledInventoryButton; // 🔹 Серая/неактивная кнопка
    public Button closeAllUIButton; // 🔹 КНОПКА ЗАКРЫТИЯ ВСЕГО UI

    [Header("References")]
    public InventorySystem inventory;
    public GameObject equipmentPanel;

    private List<GameObject> slotInstances = new List<GameObject>();
    private Item currentlyShownItem = null;

    public bool IsLocked { get; private set; } = false;

    void Start()
    {
        inventory = FindFirstObjectByType<InventorySystem>();
        if (inventory == null)
        {
            Debug.LogError("InventorySystem не найден!");
        }

        // 🔹 УБЕДИМСЯ ЧТО КНОПКИ В ПРАВИЛЬНОМ СОСТОЯНИИ ПРИ СТАРТЕ
        SetInventoryButtonState(false); // Бой не активен
    }

    public void ToggleInventory()
    {
        if (IsLocked)
        {
            Debug.Log("Инвентарь нельзя открыть - идет бой!");
            return;
        }

        bool willOpen = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(willOpen);
        equipmentPanel.SetActive(willOpen);

        if (willOpen)
        {
            RefreshUI();
            var equipUI = FindFirstObjectByType<EquipmentUI>();
            equipUI?.RefreshUI();
        }
        else
        {
            HideDescriptionPanel();
        }
    }

    // 🔹 НОВЫЙ МЕТОД ДЛЯ ЗАКРЫТИЯ ВСЕГО UI
    public void CloseAllUI()
    {
        Debug.Log("🔒 Закрытие всего UI...");

        // Закрываем инвентарь если открыт
        if (inventoryPanel != null && inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(false);
            equipmentPanel.SetActive(false);
            HideDescriptionPanel();
        }

        // Закрываем другие системы
        CloseOtherUI();

        Debug.Log("✅ Весь UI закрыт");
    }

    // 🔹 МЕТОД ДЛЯ ЗАКРЫТИЯ ДРУГИХ UI СИСТЕМ
    private void CloseOtherUI()
    {
        // Закрываем торговые панели
        TraderManager traderManager = FindFirstObjectByType<TraderManager>();
        if (traderManager != null)
        {
            traderManager.CloseAllTraderPanels();
        }

        // Закрываем панель ремонта
        RepairSystem repairSystem = FindFirstObjectByType<RepairSystem>();
        if (repairSystem != null)
        {
            repairSystem.CloseRepairPanel();
        }
    }

    // 🔹 ОСНОВНОЙ МЕТОД ДЛЯ УПРАВЛЕНИЯ КНОПКАМИ ИНВЕНТАРЯ
    public void SetInventoryButtonState(bool combatActive)
    {
        IsLocked = combatActive;

        if (inventoryButton != null && disabledInventoryButton != null)
        {
            if (combatActive)
            {
                // 🔹 БОЙ АКТИВЕН - показываем серую кнопку, скрываем обычную
                inventoryButton.gameObject.SetActive(false);
                disabledInventoryButton.gameObject.SetActive(true);

                // 🔹 ЗАКРЫВАЕМ ИНВЕНТАРЬ ЕСЛИ ОН БЫЛ ОТКРЫТ
                if (inventoryPanel != null && inventoryPanel.activeSelf)
                {
                    inventoryPanel.SetActive(false);
                    if (equipmentPanel != null) equipmentPanel.SetActive(false);
                    HideDescriptionPanel();
                }

                Debug.Log("🎒 Кнопка инвентаря: серая (бой активен)");
            }
            else
            {
                // 🔹 БОЙ НЕ АКТИВЕН - показываем обычную кнопку, скрываем серую
                inventoryButton.gameObject.SetActive(true);
                disabledInventoryButton.gameObject.SetActive(false);
                Debug.Log("🎒 Кнопка инвентаря: обычная (бой завершен)");
            }
        }
    }

    // 🔹 ДЛЯ СОВМЕСТИМОСТИ СО СТАРЫМИ СКРИПТАМИ
    public void LockInventory()
    {
        IsLocked = true;

        // Закрываем все панели инвентаря
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
        if (equipmentPanel != null)
            equipmentPanel.SetActive(false);
        if (itemDescriptionPanel != null)
            itemDescriptionPanel.SetActive(false);

        HideDescriptionPanel();

        // Скрываем все кнопки инвентаря
        HideAllInventoryButtons();

        Debug.Log("✅ Инвентарь заблокирован - все кнопки скрыты");
    }

    public void UnlockInventory()
    {
        IsLocked = false;

        // Показываем все кнопки инвентаря
        ShowAllInventoryButtons();

        Debug.Log("✅ Инвентарь разблокирован - кнопки показаны");
    }

    private void HideAllInventoryButtons()
    {
        // Скрываем основную кнопку если она назначена
        if (inventoryButton != null)
        {
            inventoryButton.gameObject.SetActive(false);
        }

        // Скрываем серую кнопку если она видима
        if (disabledInventoryButton != null)
        {
            disabledInventoryButton.gameObject.SetActive(false);
        }
    }

    private void ShowAllInventoryButtons()
    {
        // Показываем обычную кнопку
        if (inventoryButton != null)
        {
            inventoryButton.gameObject.SetActive(true);
        }

        // Скрываем серую кнопку
        if (disabledInventoryButton != null)
        {
            disabledInventoryButton.gameObject.SetActive(false);
        }
    }

    public void RefreshUI()
    {
        if (inventory == null) return;
        if (itemSlotPrefab == null) return;
        if (itemSlotsParent == null) return;

        // Очистка
        foreach (var go in slotInstances)
            if (go != null) Destroy(go);
        slotInstances.Clear();

        // Создание слотов
        foreach (var invItem in inventory.items)
        {
            if (invItem.item == null) continue;

            GameObject slotGo = Instantiate(itemSlotPrefab, itemSlotsParent);
            ItemSlotUI slot = slotGo.GetComponent<ItemSlotUI>();
            if (slot != null)
            {
                slot.Setup(invItem.item, invItem.count, this);
            }

            slotInstances.Add(slotGo);
        }
    }

    public void ShowItemDescription(Item item)
    {
        if (IsLocked) return;

        if (itemDescriptionPanel == null || inventory == null) return;

        if (currentlyShownItem == item && itemDescriptionPanel.activeSelf)
        {
            HideDescriptionPanel();
            return;
        }

        currentlyShownItem = item;
        itemDescriptionPanel.SetActive(true);
        itemDescriptionPanel.GetComponent<ItemDescriptionPanel>().Setup(item, inventory, this);
    }

    public void HideDescriptionPanel()
    {
        if (itemDescriptionPanel != null)
        {
            itemDescriptionPanel.SetActive(false);
            currentlyShownItem = null;
        }
    }

    // 🔹 СКРЫТЬ ПАНЕЛИ ИНВЕНТАРЯ
    public void HideInventoryPanels()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (equipmentPanel != null) equipmentPanel.SetActive(false);
        HideDescriptionPanel();

        Debug.Log("Панели инвентаря скрыты после ремонта");
    }

    // 🔹 СКРЫТЬ ВСЕ ПАНЕЛИ ИНВЕНТАРЯ (для RepairSystem)
    public void HideAllInventoryPanels()
    {
        HideInventoryPanels();
    }
}