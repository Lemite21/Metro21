using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JourneySystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject journeyPanel;
    public Button completeJourneyButton;
    public Button continueJourneyButton;
    public TMP_Text locationText;

    [Header("Journey Settings")]
    public int minFoodCost = 1;
    public int maxFoodCost = 10;
    public int minWaterCost = 5;
    public int maxWaterCost = 15;

    [Header("Event Chances")]
    [Range(0, 100)] public float combatChance = 10f;
    [Range(0, 100)] public float lootChance = 15f;
    [Range(0, 100)] public float stashChance = 5f;

    [Header("References")]
    private PlayerStats playerStats;
    private CombatSystem combatSystem;
    private int currentLocation = 0;

    void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        combatSystem = FindFirstObjectByType<CombatSystem>();

        completeJourneyButton.onClick.AddListener(OnCompleteJourney);
        continueJourneyButton.onClick.AddListener(OnContinueJourney);

        UpdateLocationText();
    }

    public void OnContinueJourney()
    {
        if (!playerStats.CanContinueJourney())
        {
            Debug.Log("Нельзя продолжать: закончилась еда или вода!");
            return;
        }

        // Тратим ресурсы
        int foodCost = Random.Range(minFoodCost, maxFoodCost + 1);
        int waterCost = Random.Range(minWaterCost, maxWaterCost + 1);

        playerStats.ChangeFood(-foodCost);
        playerStats.ChangeWater(-waterCost);

        currentLocation++;
        UpdateLocationText();

        // Случайное событие
        float eventRoll = Random.Range(0f, 100f);

        if (eventRoll < combatChance) // Бой
        {
            CombatSystem.EnemyType enemyType = Random.Range(0, 2) == 0 ?
                CombatSystem.EnemyType.Mutant : CombatSystem.EnemyType.Bandit;
            combatSystem.StartCombat(enemyType);
        }
        else if (eventRoll < combatChance + lootChance) // Лут
        {
            GiveRandomLoot();
        }
        else if (eventRoll < combatChance + lootChance + stashChance) // Тайник
        {
            GiveStashLoot();
        }
        // Остальное - ничего не происходит
    }

    public void OnCompleteJourney()
    {
        playerStats.RestoreAtBase();
        currentLocation = 0;
        UpdateLocationText();
        Debug.Log("Возвращение на базу");
    }

    void GiveRandomLoot()
    {
        // Здесь можно добавить логику случайного лута
        Debug.Log("Найден случайный лут!");
    }

    void GiveStashLoot()
    {
        // Здесь можно добавить логику тайника
        Debug.Log("Найден тайник!");
    }

    void UpdateLocationText()
    {
        if (locationText != null)
            locationText.text = $"Локация: {currentLocation}";
    }
}