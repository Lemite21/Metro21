using UnityEngine;
using UnityEngine.UI;

public class StationManager : MonoBehaviour
{
    [Header("Main Station UI")]
    public GameObject stationBackground;
    public Button tradeButton;
    public Button journeyButton;
    public Button questsButton;

    [Header("Journey Mode UI")]
    public GameObject journeyBackground;
    public Button completeJourneyButton;
    public Button continueJourneyButton;

    [Header("Trade Mode UI")]
    public GameObject tradeSelectionPanel;
    public Button traderButton;
    public Button repairmanButton;
    public Button backFromTradeButton;

    [Header("Trader UI")]
    public GameObject traderBackground;
    public Button buyButton;
    public Button sellButton;
    public Button backFromTraderButton;

    [Header("Repairman UI")]
    public GameObject repairmanBackground;
    public Button repairButton;
    public Button backFromRepairmanButton;

    [Header("Manager References")]
    public TraderManager traderManager;
    public RepairSystem repairSystem;
    public HodkaManager hodkaManager;
    public CombatSystem combatSystem; // 🔹 Добавил ссылку на CombatSystem

    private void Start()
    {
        SetupMainStation();
    }

    public void SetupMainStation()
    {
        // Показываем станцию
        SetActiveState(stationBackground, true);
        SetActiveState(tradeButton.gameObject, true);
        SetActiveState(journeyButton.gameObject, true);
        SetActiveState(questsButton.gameObject, true);

        // Скрываем все остальное
        SetActiveState(journeyBackground, false);
        SetActiveState(tradeSelectionPanel, false);
        SetActiveState(traderBackground, false);
        SetActiveState(repairmanBackground, false);

        // 🔹 Убедимся что combat panel скрыт
        if (combatSystem != null && combatSystem.combatPanel != null)
            combatSystem.combatPanel.SetActive(false);

        if (traderManager != null) traderManager.CloseAllTraderPanels();
        if (repairSystem != null) repairSystem.CloseRepairPanel();

        // Назначаем обработчики кнопок
        tradeButton.onClick.RemoveAllListeners();
        tradeButton.onClick.AddListener(EnterTradeMode);

        journeyButton.onClick.RemoveAllListeners();
        journeyButton.onClick.AddListener(EnterJourneyMode);
    }

    public void EnterJourneyMode()
    {
        // Скрываем станцию
        SetActiveState(stationBackground, false);
        SetActiveState(tradeButton.gameObject, false);
        SetActiveState(journeyButton.gameObject, false);
        SetActiveState(questsButton.gameObject, false);

        // Показываем ходку
        SetActiveState(journeyBackground, true);
        SetActiveState(completeJourneyButton.gameObject, true);
        SetActiveState(continueJourneyButton.gameObject, true);

        // Назначаем обработчики кнопок ходки
        completeJourneyButton.onClick.RemoveAllListeners();
        completeJourneyButton.onClick.AddListener(ReturnToStation);

        continueJourneyButton.onClick.RemoveAllListeners();
        continueJourneyButton.onClick.AddListener(OnContinueJourney);

        // 🔹 УВЕДОМЛЯЕМ HODKA MANAGER О ВХОДЕ В РЕЖИМ ХОДКИ
        if (hodkaManager != null)
        {
            hodkaManager.OnEnterJourneyMode();
        }

        Debug.Log("🚶 Режим ходки активирован");
    }

    void OnContinueJourney()
    {
        if (hodkaManager != null)
        {
            hodkaManager.OnContinueJourney();
        }
        else
        {
            Debug.LogError("HodkaManager не назначен!");
        }
    }

    public void ReturnToStation()
    {
        // Скрываем ходку
        SetActiveState(journeyBackground, false);
        SetActiveState(completeJourneyButton.gameObject, false);
        SetActiveState(continueJourneyButton.gameObject, false);

        // 🔹 Убедимся что combat panel скрыт
        if (combatSystem != null && combatSystem.combatPanel != null)
            combatSystem.combatPanel.SetActive(false);

        // Показываем станцию
        SetupMainStation();

        // Восстанавливаем энергию на базе
        var playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.RestoreAtBase();
        }

        Debug.Log("🏠 Возврат на станцию");
    }

    // 🔹 ВОЗВРАТ НА СТАНЦИЮ ПОСЛЕ СМЕРТИ
    public void ReturnToStationAfterDeath()
    {
        // Скрываем combat panel
        if (combatSystem != null && combatSystem.combatPanel != null)
            combatSystem.combatPanel.SetActive(false);

        // Скрываем ходку
        SetActiveState(journeyBackground, false);
        SetActiveState(completeJourneyButton.gameObject, false);
        SetActiveState(continueJourneyButton.gameObject, false);

        // Показываем станцию
        SetupMainStation();

        // Восстанавливаем здоровье после смерти
        var playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.currentHealth = playerStats.GetMaxHealthWithArmor();
            playerStats.RestoreAtBase();
            playerStats.UpdateUI();
        }

        Debug.Log("💀 Возврат на станцию после смерти");
    }

    // 🔹 ВОЗВРАТ В ХОДКУ ИЗ БОЯ
    public void ReturnToJourneyFromCombat()
    {
        EnterJourneyMode();
    }

    public void EnterTradeMode()
    {
        SetActiveState(tradeButton.gameObject, false);
        SetActiveState(journeyButton.gameObject, false);
        SetActiveState(questsButton.gameObject, false);

        SetActiveState(tradeSelectionPanel, true);

        traderButton.onClick.RemoveAllListeners();
        traderButton.onClick.AddListener(EnterTraderMode);

        repairmanButton.onClick.RemoveAllListeners();
        repairmanButton.onClick.AddListener(EnterRepairmanMode);

        backFromTradeButton.onClick.RemoveAllListeners();
        backFromTradeButton.onClick.AddListener(SetupMainStation);
    }

    public void EnterTraderMode()
    {
        SetActiveState(tradeSelectionPanel, false);
        SetActiveState(repairmanBackground, false);
        if (repairSystem != null) repairSystem.CloseRepairPanel();

        SetActiveState(traderBackground, true);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(traderManager.ToggleBuyPanel);

        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(traderManager.ToggleSellPanel);

        backFromTraderButton.onClick.RemoveAllListeners();
        backFromTraderButton.onClick.AddListener(ExitTraderMode);
    }

    public void ExitTraderMode()
    {
        SetActiveState(traderBackground, false);
        if (traderManager != null) traderManager.CloseAllTraderPanels();

        SetActiveState(tradeSelectionPanel, true);
    }

    public void EnterRepairmanMode()
    {
        SetActiveState(tradeSelectionPanel, false);
        SetActiveState(traderBackground, false);
        if (traderManager != null) traderManager.CloseAllTraderPanels();

        SetActiveState(repairmanBackground, true);

        // 🔹 НАСТРАИВАЕМ КНОПКУ РЕМОНТА
        repairButton.onClick.RemoveAllListeners();
        repairButton.onClick.AddListener(repairSystem.ShowRepairPanel);

        backFromRepairmanButton.onClick.RemoveAllListeners();
        backFromRepairmanButton.onClick.AddListener(ExitRepairmanMode);

        Debug.Log("Ремонтник: кнопка настроена");
    }

    public void ExitRepairmanMode()
    {
        SetActiveState(repairmanBackground, false);
        if (repairSystem != null) repairSystem.CloseRepairPanel();

        SetActiveState(tradeSelectionPanel, true);
    }

    void SetActiveState(GameObject obj, bool state)
    {
        if (obj != null) obj.SetActive(state);
    }
}