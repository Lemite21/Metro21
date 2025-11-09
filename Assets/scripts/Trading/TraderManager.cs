using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TraderManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject buyPanel;
    public GameObject sellPanel;
    public GameObject itemDescriptionPanel;

    [Header("Inventory Panels")]
    public GameObject inventoryPanel;    // Панель инвентаря игрока
    public GameObject equipmentPanel;    // Панель экипировки игрока

    [Header("Buy Panel Components")]
    public Transform traderItemsGrid;    // Контейнер для кнопок предметов торговца
    public GameObject traderItemButtonPrefab; // Префаб кнопки предмета

    [Header("Item Description UI")]
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;
    public TMP_Text itemPriceText;
    public TMP_Text itemQuantityText; // 🔹 НОВОЕ: текст количества
    public Button buyButton;
    public Button cancelButton;

    [Header("Sell Panel UI")]
    public TMP_Text totalSellValueText;
    public Button confirmSellButton;

    [Header("Trader Inventory")]
    public TraderInventory traderInventory;

    [Header("References")]
    private PlayerWallet playerWallet;
    private InventorySystem playerInventory;
    private InventoryUI inventoryUI;

    private Item selectedItem; // Выбранный предмет для покупки
    private List<Item> selectedItems = new List<Item>(); // Для продажи
    private int totalSellValue = 0;

    void Start()
    {
        playerWallet = FindFirstObjectByType<PlayerWallet>();
        playerInventory = FindFirstObjectByType<InventorySystem>();
        inventoryUI = FindFirstObjectByType<InventoryUI>();

        // Скрываем все панели при старте
        if (buyPanel != null) buyPanel.SetActive(false);
        if (sellPanel != null) sellPanel.SetActive(false);
        if (itemDescriptionPanel != null) itemDescriptionPanel.SetActive(false);

        // Настраиваем кнопки
        if (buyButton != null) buyButton.onClick.AddListener(BuyItem);
        if (cancelButton != null) cancelButton.onClick.AddListener(HideItemDescription);
        if (confirmSellButton != null) confirmSellButton.onClick.AddListener(ConfirmSell);
    }

    // ========== СИСТЕМА ПОКУПКИ ==========

    public void ToggleBuyPanel()
    {
        if (buyPanel != null)
        {
            bool willOpen = !buyPanel.activeSelf;
            buyPanel.SetActive(willOpen);

            if (willOpen)
            {
                // Закрываем продажу при открытии покупки
                if (sellPanel != null) sellPanel.SetActive(false);
                HideItemDescription();
                ClearSelection(); // Сбрасываем выделение продажи
                CreateTraderItemButtons(); // Создаем кнопки предметов
                Debug.Log("✅ Панель покупки открыта");
            }
            else
            {
                Debug.Log("✅ Панель покупки закрыта");
            }
        }
    }

    // 🔹 СОЗДАНИЕ КНОПОК ПРЕДМЕТОВ ТОРГОВЦА
    void CreateTraderItemButtons()
    {
        if (traderItemsGrid == null || traderItemButtonPrefab == null || traderInventory == null) return;

        // Очищаем старые кнопки
        foreach (Transform child in traderItemsGrid)
            Destroy(child.gameObject);

        // Создаем кнопки для каждого предмета торговца
        foreach (TraderItem traderItem in traderInventory.availableItems)
        {
            GameObject buttonGO = Instantiate(traderItemButtonPrefab, traderItemsGrid);
            TraderItemButton button = buttonGO.GetComponent<TraderItemButton>();

            if (button != null)
            {
                button.Setup(traderItem.item, this);
            }
        }

        Debug.Log($"✅ Создано {traderInventory.availableItems.Count} кнопок предметов");
    }

    // 🔹 ПОКАЗАТЬ ОПИСАНИЕ ПРЕДМЕТА ДЛЯ ПОКУПКИ
    public void ShowBuyItemDescription(Item item)
    {
        selectedItem = item;

        if (itemDescriptionPanel != null && item != null)
        {
            // 🔹 ПОЛУЧАЕМ КОЛИЧЕСТВО ИЗ TRADER INVENTORY
            int quantity = GetItemQuantityFromTrader(item);

            // 🔹 ЦЕНА ОСТАЕТСЯ ОДНА И ТА ЖЕ, НЕ УМНОЖАЕМ НА КОЛИЧЕСТВО
            int price = item.buyPrice;

            // Заполняем информацию о предмете
            itemIcon.sprite = item.icon;
            itemIcon.enabled = item.icon != null;
            itemNameText.text = item.itemName;
            itemDescriptionText.text = item.description;
            itemPriceText.text = $"Цена: {price} руб";

            // 🔹 ПОКАЗЫВАЕМ КОЛИЧЕСТВО
            if (itemQuantityText != null)
            {
                if (quantity > 1)
                {
                    itemQuantityText.text = $"Количество: {quantity} шт.";
                    itemQuantityText.gameObject.SetActive(true);
                }
                else
                {
                    itemQuantityText.gameObject.SetActive(false);
                }
            }

            // Настраиваем кнопку "Купить"
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyItem);

            // Проверяем хватает ли денег (цена не умножается на количество)
            bool canAfford = playerWallet.HasEnoughMoney(price);
            buyButton.interactable = canAfford;

            // Меняем текст кнопки в зависимости от возможности покупки
            TMP_Text buyButtonText = buyButton.GetComponentInChildren<TMP_Text>();
            if (buyButtonText != null)
            {
                if (canAfford)
                {
                    buyButtonText.text = "Купить";
                    buyButtonText.color = Color.white;
                }
                else
                {
                    buyButtonText.text = "Нехватает денег";
                    buyButtonText.color = Color.gray;
                }
            }

            // Настраиваем кнопку "Отмена"
            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(HideItemDescription);

                TMP_Text cancelButtonText = cancelButton.GetComponentInChildren<TMP_Text>();
                if (cancelButtonText != null)
                {
                    cancelButtonText.text = "ОТМЕНА";
                }
            }

            itemDescriptionPanel.SetActive(true);
            Debug.Log($"✅ Показано описание предмета: {item.itemName} (x{quantity} за {price} руб)");
        }
    }

    // 🔹 СКРЫТЬ ОПИСАНИЕ ПРЕДМЕТА
    public void HideItemDescription()
    {
        if (itemDescriptionPanel != null)
        {
            itemDescriptionPanel.SetActive(false);
            selectedItem = null;
            Debug.Log("✅ Описание предмета скрыто");
        }
    }

    // 🔹 ОБНОВЛЕННЫЙ МЕТОД ПОКУПКИ С УЧЕТОМ КОЛИЧЕСТВА
    void BuyItem()
    {
        if (selectedItem == null)
        {
            Debug.Log("❌ Не выбран предмет для покупки");
            return;
        }

        // 🔹 ПОЛУЧАЕМ КОЛИЧЕСТВО ИЗ TRADER INVENTORY
        int quantity = GetItemQuantityFromTrader(selectedItem);

        // 🔹 ЦЕНА ОСТАЕТСЯ ОДНА И ТА ЖЕ, НЕ УМНОЖАЕМ НА КОЛИЧЕСТВО
        int price = selectedItem.buyPrice;

        if (!playerWallet.HasEnoughMoney(price))
        {
            Debug.Log("❌ Недостаточно денег для покупки");
            return;
        }

        // 🔹 ДОБАВЛЯЕМ УКАЗАННОЕ КОЛИЧЕСТВО ПРЕДМЕТОВ
        bool allItemsAdded = true;
        int itemsAdded = 0;

        for (int i = 0; i < quantity; i++)
        {
            if (AddItemToInventory(selectedItem))
            {
                itemsAdded++;
            }
            else
            {
                allItemsAdded = false;
                break;
            }
        }

        if (itemsAdded > 0)
        {
            // 🔹 СПИСЫВАЕМ СТАНДАРТНУЮ ЦЕНУ (НЕ УМНОЖЕННУЮ НА КОЛИЧЕСТВО)
            playerWallet.SpendMoney(price);

            if (allItemsAdded)
            {
                Debug.Log($"✅ Куплено {quantity} шт. {selectedItem.itemName} за {price} руб");
            }
            else
            {
                Debug.Log($"⚠️ Куплено {itemsAdded} из {quantity} шт. {selectedItem.itemName} за {price} руб (не хватило места)");
            }
        }
        else
        {
            Debug.Log("❌ Не хватило места в инвентаре для покупки");
            ShowInventoryFullMessage();
            return;
        }

        // Обновляем UI
        HideItemDescription();

        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }

        Debug.Log($"🎉 Успешная покупка! Осталось денег: {playerWallet.CurrentMoney} руб");
    }

    // 🔹 ПОЛУЧИТЬ КОЛИЧЕСТВО ПРЕДМЕТА ИЗ TRADER INVENTORY
    private int GetItemQuantityFromTrader(Item item)
    {
        foreach (TraderItem traderItem in traderInventory.availableItems)
        {
            if (traderItem.item == item)
            {
                return traderItem.quantity;
            }
        }
        return 1; // по умолчанию 1
    }

    // 🔹 МЕТОД ДЛЯ ДОБАВЛЕНИЯ ПРЕДМЕТА В ИНВЕНТАРЬ
    private bool AddItemToInventory(Item item)
    {
        // Пытаемся добавить к существующему стаку
        if (item.isStackable)
        {
            foreach (InventoryItem invItem in playerInventory.items)
            {
                if (invItem.item.itemName == item.itemName &&
                    invItem.item.isStackable &&
                    invItem.count < invItem.item.maxStackSize)
                {
                    invItem.count++;
                    return true;
                }
            }
        }

        // Создаем новый предмет в инвентаре
        Item itemCopy = Instantiate(item);
        return playerInventory.AddItem(itemCopy);
    }

    // 🔹 СООБЩЕНИЕ О ПЕРЕПОЛНЕННОМ ИНВЕНТАРЕ
    private void ShowInventoryFullMessage()
    {
        TMP_Text buyButtonText = buyButton.GetComponentInChildren<TMP_Text>();
        if (buyButtonText != null)
        {
            buyButtonText.text = "НЕТ МЕСТА В ИНВЕНТАРЕ";
            buyButtonText.color = Color.red;
            buyButton.interactable = false;
        }
    }

    // ========== СИСТЕМА ПРОДАЖИ ==========

    public void ToggleSellPanel()
    {
        if (sellPanel != null)
        {
            bool willOpen = !sellPanel.activeSelf;
            sellPanel.SetActive(willOpen);

            if (willOpen)
            {
                // Закрываем покупку при открытии продажи
                if (buyPanel != null) buyPanel.SetActive(false);
                HideItemDescription();
                ClearSelection();
                UpdateSellTotal();
                Debug.Log("✅ Панель продажи открыта - можно выделять предметы");
            }
            else
            {
                // При закрытии панели продажи - сбрасываем выделение
                ClearSelection();
                Debug.Log("✅ Панель продажи закрыта, выделение сброшено");
            }
        }
    }

    // 🔹 ВЫДЕЛЕНИЕ/СНЯТИЕ ВЫДЕЛЕНИЯ ПРЕДМЕТА
    public void ToggleItemSelection(Item item)
    {
        if (item == null) return;

        if (selectedItems.Contains(item))
        {
            // Убираем из выделения
            selectedItems.Remove(item);
            totalSellValue -= item.sellPrice;
            Debug.Log($"❌ Снято выделение: {item.itemName}");
        }
        else
        {
            // Добавляем в выделение
            selectedItems.Add(item);
            totalSellValue += item.sellPrice;
            Debug.Log($"✅ Выделено для продажи: {item.itemName} (+{item.sellPrice} руб)");
        }

        UpdateSellTotal();
        UpdateInventoryHighlight();
    }

    // 🔹 УЛУЧШЕННАЯ ПОДСВЕТКА ВЫДЕЛЕННЫХ ПРЕДМЕТОВ
    private void UpdateInventoryHighlight()
    {
        if (inventoryUI != null && inventoryUI.itemSlotsParent != null)
        {
            // Проходим по всем слотам инвентаря
            foreach (Transform slotTransform in inventoryUI.itemSlotsParent)
            {
                ItemSlotUI slot = slotTransform.GetComponent<ItemSlotUI>();
                if (slot != null)
                {
                    Item item = slot.GetItem();
                    Image slotImage = slotTransform.GetComponent<Image>();
                    Outline outline = slotTransform.GetComponent<Outline>();

                    // Добавляем Outline если его нет
                    if (outline == null)
                    {
                        outline = slotTransform.gameObject.AddComponent<Outline>();
                        outline.effectColor = Color.yellow;
                        outline.effectDistance = new Vector2(3, 3);
                        outline.enabled = false;
                    }

                    if (item != null && selectedItems.Contains(item))
                    {
                        // 🔹 СИЛЬНАЯ ПОДСВЕТКА ВЫБРАННОГО ПРЕДМЕТА
                        slotImage.color = new Color(1f, 0.9f, 0.4f, 1f); // Ярко-желтый
                        outline.enabled = true; // Контур
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

    void UpdateSellTotal()
    {
        if (totalSellValueText != null)
            totalSellValueText.text = $"Сумма продажи: {totalSellValue} руб";

        confirmSellButton.interactable = totalSellValue > 0;
    }

    // 🔹 УЛУЧШЕННОЕ ПОДТВЕРЖДЕНИЕ ПРОДАЖИ
    void ConfirmSell()
    {
        if (selectedItems.Count == 0)
        {
            Debug.Log("❌ Нет выделенных предметов для продажи");
            return;
        }

        Debug.Log($"🔄 Начинаем продажу {selectedItems.Count} предметов...");

        // Удаляем выделенные предметы из инвентаря (по 1 штуке каждого)
        foreach (Item item in selectedItems)
        {
            playerInventory.RemoveItem(item);
            Debug.Log($"✅ Продан: {item.itemName}");
        }

        // Добавляем деньги
        playerWallet.AddMoney(totalSellValue);
        Debug.Log($"💰 Получено: {totalSellValue} руб");

        // Показываем итог
        Debug.Log($"🎉 Продано {selectedItems.Count} предметов на сумму {totalSellValue} руб");

        // 🔹 ЗАКРЫВАЕМ ОБЕ ПАНЕЛИ ИНВЕНТАРЯ ПОСЛЕ ПРОДАЖИ
        CloseInventoryPanels();

        ClearSelection();
    }

    // 🔹 МЕТОД ДЛЯ ЗАКРЫТИЯ ПАНЕЛЕЙ ИНВЕНТАРЯ
    private void CloseInventoryPanels()
    {
        // 🔹 ПРЯМОЕ УПРАВЛЕНИЕ ПАНЕЛЯМИ ЧЕРЕЗ ИНСПЕКТОР
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
            Debug.Log("✅ Закрыта панель инвентаря");
        }

        if (equipmentPanel != null)
        {
            equipmentPanel.SetActive(false);
            Debug.Log("✅ Закрыта панель экипировки");
        }

        // 🔹 ДОПОЛНИТЕЛЬНО: ЗАКРЫВАЕМ ОПИСАНИЕ ПРЕДМЕТА ЕСЛИ ОТКРЫТО
        if (itemDescriptionPanel != null && itemDescriptionPanel.activeSelf)
        {
            itemDescriptionPanel.SetActive(false);
            Debug.Log("✅ Закрыто описание предмета");
        }
    }

    // 🔹 СБРОС ВЫДЕЛЕНИЯ
    void ClearSelection()
    {
        selectedItems.Clear();
        totalSellValue = 0;
        UpdateSellTotal();
        UpdateInventoryHighlight();
        Debug.Log("🧹 Выделение сброшено");
    }

    // 🔹 ПРОВЕРКА: ВЫДЕЛЕН ЛИ ПРЕДМЕТ
    public bool IsItemSelected(Item item)
    {
        return selectedItems.Contains(item);
    }

    public void CloseAllTraderPanels()
    {
        if (buyPanel != null) buyPanel.SetActive(false);
        if (sellPanel != null)
        {
            sellPanel.SetActive(false);
            ClearSelection();
        }
        if (itemDescriptionPanel != null) itemDescriptionPanel.SetActive(false);
    }
}