// Assets/Scripts/Inventory/EquipmentUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentUI : MonoBehaviour
{
    // Слоты (иконки + текст)
    public Image helmetIcon; public TMP_Text helmetText;
    public Image chestIcon; public TMP_Text chestText;
    public Image legsIcon; public TMP_Text legsText;
    public Image wpn1Icon; public TMP_Text wpn1Text;
    public Image wpn2Icon; public TMP_Text wpn2Text;

    private EquipmentSystem equipment;
    private InventoryUI inventoryUI;

    void Start()
    {
        equipment = FindFirstObjectByType<EquipmentSystem>();
        inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (equipment == null) Debug.LogError("EquipmentSystem не найден!");
        if (inventoryUI == null) Debug.LogError("InventoryUI не найден!");
    }

    public void RefreshUI()
    {
        if (equipment == null) return;
        UpdateSlot(helmetIcon, helmetText, equipment.helmet);
        UpdateSlot(chestIcon, chestText, equipment.chest);
        UpdateSlot(legsIcon, legsText, equipment.legs);
        UpdateSlot(wpn1Icon, wpn1Text, equipment.weaponMain);
        UpdateSlot(wpn2Icon, wpn2Text, equipment.weaponSecondary);
    }

    void UpdateSlot(Image icon, TMP_Text text, EquipmentSlot slot)
    {
        if (slot.item != null)
        {
            icon.sprite = slot.item.icon;
            icon.enabled = true;
            text.text = slot.item.itemName;
        }
        else
        {
            icon.enabled = false;
            text.text = "";
        }
    }

    // Методы для кликов по слотам (назначать в инспекторе!)
    public void OnClickHelmet() => ShowItem(equipment.helmet.item);
    public void OnClickChest() => ShowItem(equipment.chest.item);
    public void OnClickLegs() => ShowItem(equipment.legs.item);
    public void OnClickWpn1() => ShowItem(equipment.weaponMain.item);
    public void OnClickWpn2() => ShowItem(equipment.weaponSecondary.item);

    void ShowItem(Item item)
    {
        if (item == null) return;
        inventoryUI.ShowItemDescription(item);
    }
}