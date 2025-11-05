using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TMP_Text countText;

    private Item item;
    private InventoryUI inventoryUI;
    private Outline outline;

    void Start()
    {
        // Добавляем Outline для подсветки
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.effectColor = Color.blue; // Синий для ремонта
            outline.effectDistance = new Vector2(3, 3);
            outline.enabled = false;
        }
    }

    public void Setup(Item item, int count, InventoryUI ui)
    {
        this.item = item;
        this.inventoryUI = ui;

        if (item != null)
        {
            icon.sprite = item.icon;
            icon.enabled = item.icon != null;
            countText.text = (item.isStackable && count > 1) ? count.ToString() : "";

            // 🔹 ОБНОВЛЯЕМ ПОДСВЕТКУ ПРИ СОЗДАНИИ СЛОТА
            UpdateSelectionHighlight();
        }
        else
        {
            icon.enabled = false;
            countText.text = "";
        }
    }

    public Item GetItem()
    {
        return item;
    }

    // 🔹 ОБРАБОТКА КЛИКА ПО СЛОТУ
    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null) return;

        // Находим RepairSystem
        RepairSystem repairSystem = FindFirstObjectByType<RepairSystem>();
        if (repairSystem != null && repairSystem.repairPanel != null && repairSystem.repairPanel.activeSelf)
        {
            // 🔹 ЕСЛИ ОТКРЫТА ПАНЕЛЬ РЕМОНТА - ВЫБИРАЕМ ПРЕДМЕТ ДЛЯ РЕМОНТА
            repairSystem.SelectItemForRepair(item);
            UpdateSelectionHighlight();
        }
        else
        {
            // Иначе обычное поведение (описание или продажа)
            TraderManager traderManager = FindFirstObjectByType<TraderManager>();
            if (traderManager != null && traderManager.sellPanel != null && traderManager.sellPanel.activeSelf)
            {
                traderManager.ToggleItemSelection(item);
            }
            else
            {
                inventoryUI.ShowItemDescription(item);
            }
        }
    }

    // 🔹 ОБНОВЛЕНИЕ ПОДСВЕТКИ ВЫДЕЛЕНИЯ
    private void UpdateSelectionHighlight()
    {
        if (item == null) return;

        RepairSystem repairSystem = FindFirstObjectByType<RepairSystem>();
        TraderManager traderManager = FindFirstObjectByType<TraderManager>();

        Image slotImage = GetComponent<Image>();

        if (repairSystem != null && repairSystem.repairPanel != null && repairSystem.repairPanel.activeSelf)
        {
            // 🔹 РЕЖИМ РЕМОНТА - СИНЯЯ ПОДСВЕТКА
            if (repairSystem.IsItemSelected(item))
            {
                slotImage.color = new Color(0.7f, 0.8f, 1f, 1f); // Светло-синий
                if (outline != null) outline.enabled = true;
            }
            else
            {
                slotImage.color = Color.white;
                if (outline != null) outline.enabled = false;
            }
        }
        else if (traderManager != null && traderManager.sellPanel != null && traderManager.sellPanel.activeSelf)
        {
            // Режим продажи (старая логика)
            if (traderManager.IsItemSelected(item))
            {
                slotImage.color = new Color(1f, 0.9f, 0.4f, 1f); // Желтый
                if (outline != null) outline.enabled = true;
            }
            else
            {
                slotImage.color = Color.white;
                if (outline != null) outline.enabled = false;
            }
        }
        else
        {
            // Обычный режим
            slotImage.color = Color.white;
            if (outline != null) outline.enabled = false;
        }
    }
}