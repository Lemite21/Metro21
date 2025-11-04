using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TraderItemButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image itemIcon;
    public TMP_Text itemNameText;
    public TMP_Text priceText;

    private Item item;
    private TraderManager traderManager;

    public void Setup(Item itemToSell, TraderManager manager)
    {
        item = itemToSell;
        traderManager = manager;

        // Заполняем UI
        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (priceText != null)
            priceText.text = $"{item.buyPrice} руб";

        // Назначаем обработчик клика
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }

        Debug.Log($"✅ Создана кнопка предмета: {item.itemName}");
    }

    void OnButtonClick()
    {
        if (item != null && traderManager != null)
        {
            traderManager.ShowBuyItemDescription(item);
        }
        else
        {
            Debug.LogError("❌ TraderItemButton: item или traderManager не назначены!");
        }
        Debug.Log("Кнопка купить нажата");
    }
}