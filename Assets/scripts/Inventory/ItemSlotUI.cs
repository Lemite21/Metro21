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
            outline.effectColor = Color.yellow;
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

        // Находим TraderManager
        TraderManager traderManager = FindFirstObjectByType<TraderManager>();
        if (traderManager != null && traderManager.sellPanel != null && traderManager.sellPanel.activeSelf)
        {
            // Если открыта панель продажи - выделяем/снимаем выделение
            traderManager.ToggleItemSelection(item);
            UpdateSelectionHighlight();
        }
        else
        {
            // Иначе показываем описание предмета (обычное поведение)
            inventoryUI.ShowItemDescription(item);
        }
    }

    // 🔹 ОБНОВЛЕНИЕ ПОДСВЕТКИ ВЫДЕЛЕНИЯ
    private void UpdateSelectionHighlight()
    {
        if (item == null) return;

        TraderManager traderManager = FindFirstObjectByType<TraderManager>();
        if (traderManager != null && traderManager.sellPanel != null && traderManager.sellPanel.activeSelf)
        {
            Image slotImage = GetComponent<Image>();
            if (traderManager.IsItemSelected(item))
            {
                // 🔹 СИЛЬНАЯ ПОДСВЕТКА
                slotImage.color = new Color(1f, 0.9f, 0.4f, 1f); // Ярко-желтый
                if (outline != null) outline.enabled = true;
            }
            else
            {
                // Обычный вид
                slotImage.color = Color.white;
                if (outline != null) outline.enabled = false;
            }
        }
        else
        {
            // Если панель продажи закрыта - обычный вид
            Image slotImage = GetComponent<Image>();
            slotImage.color = Color.white;
            if (outline != null) outline.enabled = false;
        }
    }
}