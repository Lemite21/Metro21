// Assets/Scripts/Economy/PlayerWallet.cs
using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    [SerializeField] private int currentMoney = 1000;
    [SerializeField] private TMP_Text moneyText;

    public int CurrentMoney => currentMoney;

    void Start()
    {
        UpdateMoneyDisplay();
    }

    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }

    public bool SpendMoney(int amount)
    {
        if (!HasEnoughMoney(amount)) return false;

        currentMoney -= amount;
        UpdateMoneyDisplay();
        return true;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyDisplay();
    }

    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
            moneyText.text = $"{currentMoney} руб";
    }
}