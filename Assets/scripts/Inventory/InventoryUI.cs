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
    public Button inventoryButton;

    [Header("References")]
    public InventorySystem inventory;

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
    }

    public GameObject equipmentPanel;

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

        // Дополнительно ищем кнопки по имени (исправленная версия)
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Inventory") ||
                obj.name.Contains("инвентар") ||
                obj.name.Contains("Инвентар"))
            {
                obj.SetActive(false);
            }
        }
    }

    private void ShowAllInventoryButtons()
    {
        // Показываем основную кнопку если она назначена
        if (inventoryButton != null)
        {
            inventoryButton.gameObject.SetActive(true);
        }

        // Дополнительно ищем кнопки по имени (исправленная версия)
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Inventory") ||
                obj.name.Contains("инвентар") ||
                obj.name.Contains("Инвентар"))
            {
                obj.SetActive(true);
            }
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

}