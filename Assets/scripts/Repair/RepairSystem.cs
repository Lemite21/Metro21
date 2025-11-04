using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class RepairSystem : MonoBehaviour, IDropHandler
{
    [Header("Repair UI")]
    public GameObject repairPanel;
    public Image repairSlotIcon;
    public TMP_Text conditionText;
    public TMP_Text repairCostText;
    public Button repairButton;

    [Header("References")]
    private PlayerWallet playerWallet;
    private Item currentRepairItem;

    void Start()
    {
        playerWallet = FindFirstObjectByType<PlayerWallet>();

        if (repairPanel != null) repairPanel.SetActive(false);

        repairButton.onClick.RemoveAllListeners();
        repairButton.onClick.AddListener(RepairItem);
    }

    // ВКЛЮЧАЕТ панель ремонта
    public void ShowRepairPanel()
    {
        if (repairPanel != null)
        {
            repairPanel.SetActive(true);
            ClearRepairSlot();
        }
    }

    // Обработчик перетаскивания предмета в слот
    public void OnDrop(PointerEventData eventData)
    {
        // Получаем перетаскиваемый предмет из системы перетаскивания
        Item draggedItem = GetDraggedItem(eventData);

        if (draggedItem != null && draggedItem.hasDurability)
        {
            SetRepairItem(draggedItem);
        }
    }

    Item GetDraggedItem(PointerEventData eventData)
    {
        // Эта функция зависит от вашей системы перетаскивания
        // Если используете стандартную Unity UI drag&drop:
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject != null)
        {
            ItemSlotUI slot = draggedObject.GetComponent<ItemSlotUI>();
            if (slot != null)
            {
                // Нужно получить Item из slot
                return null; // Замените на вашу логику
            }
        }
        return null;
    }

    void SetRepairItem(Item item)
    {
        currentRepairItem = item;

        // Обновляем UI
        repairSlotIcon.sprite = item.icon;
        repairSlotIcon.enabled = true;

        UpdateRepairInfo();
    }

    void UpdateRepairInfo()
    {
        if (currentRepairItem == null) return;

        int repairCost = currentRepairItem.GetRepairCost();
        float conditionPercent = (currentRepairItem.currentDurability / currentRepairItem.maxDurability) * 100f;

        conditionText.text = $"Состояние: {conditionPercent:F0}%";
        repairCostText.text = $"Цена ремонта: {repairCost} руб";

        repairButton.interactable = currentRepairItem.NeedsRepair && playerWallet.HasEnoughMoney(repairCost);
    }

    void RepairItem()
    {
        if (currentRepairItem == null || !currentRepairItem.NeedsRepair) return;

        int repairCost = currentRepairItem.GetRepairCost();

        if (playerWallet.SpendMoney(repairCost))
        {
            currentRepairItem.Repair();
            Debug.Log($"✅ Отремонтировано: {currentRepairItem.itemName} за {repairCost} руб");
            UpdateRepairInfo();
        }
    }

    void ClearRepairSlot()
    {
        currentRepairItem = null;
        repairSlotIcon.enabled = false;
        conditionText.text = "Состояние: -";
        repairCostText.text = "Цена ремонта: -";
        repairButton.interactable = false;
    }

    // В RepairSystem.cs добавьте этот метод:
    public void CloseRepairPanel()
    {
        if (repairPanel != null)
            repairPanel.SetActive(false);

        // Очищаем слот ремонта если нужно
        ClearRepairSlot();
    }
}