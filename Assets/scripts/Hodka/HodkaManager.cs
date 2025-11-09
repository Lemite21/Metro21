using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HodkaManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject journeyBackground;
    public Image journeyBackgroundImage;
    public Sprite normalJourneyBackground; // 🔹 Обычный фон ходки
    public Sprite mutantBackground;        // 🔹 Фон мутанта
    public Sprite banditBackground;        // 🔹 Фон бандита
    public Button completeJourneyButton;
    public Button continueJourneyButton;

    [Header("Combat System")]
    public CombatSystem combatSystem;

    [Header("Event Settings")]
    [Range(0, 100)] public float nothingChance = 50f;
    [Range(0, 100)] public float mutantChance = 20f;
    [Range(0, 100)] public float banditChance = 20f;
    [Range(0, 100)] public float lootChance = 10f;

    [Header("Loot Settings")]
    public List<Item> possibleLoot;

    [Header("References")]
    private InventorySystem inventory;
    private PlayerStats playerStats;
    private StationManager stationManager;

    private void Start()
    {
        inventory = FindFirstObjectByType<InventorySystem>();
        playerStats = FindFirstObjectByType<PlayerStats>();
        stationManager = FindFirstObjectByType<StationManager>();

        if (inventory == null)
        {
            Debug.LogError("InventorySystem not found!");
        }

        // 🔹 Устанавливаем обычный фон при старте
        SetNormalBackground();
    }

    // 🔹 УСТАНОВИТЬ ОБЫЧНЫЙ ФОН ХОДКИ
    public void SetNormalBackground()
    {
        if (journeyBackgroundImage != null && normalJourneyBackground != null)
        {
            journeyBackgroundImage.sprite = normalJourneyBackground;
        }
    }

    // 🔹 УСТАНОВИТЬ ФОН МУТАНТА
    public void SetMutantBackground()
    {
        if (journeyBackgroundImage != null && mutantBackground != null)
        {
            journeyBackgroundImage.sprite = mutantBackground;
        }
    }

    // 🔹 УСТАНОВИТЬ ФОН БАНДИТА
    public void SetBanditBackground()
    {
        if (journeyBackgroundImage != null && banditBackground != null)
        {
            journeyBackgroundImage.sprite = banditBackground;
        }
    }

    // 🔹 СКРЫТЬ КНОПКИ ХОДКИ ПРИ БОЕ
    public void HideJourneyButtons()
    {
        if (completeJourneyButton != null) completeJourneyButton.gameObject.SetActive(false);
        if (continueJourneyButton != null) continueJourneyButton.gameObject.SetActive(false);
    }

    // 🔹 ПОКАЗАТЬ КНОПКИ ХОДКИ
    public void ShowJourneyButtons()
    {
        if (completeJourneyButton != null) completeJourneyButton.gameObject.SetActive(true);
        if (continueJourneyButton != null) continueJourneyButton.gameObject.SetActive(true);
    }

    // 🔹 ВОЗВРАТ В ХОДКУ ПОСЛЕ УСПЕШНОГО ПОБЕГА
    // 🔹 ВОЗВРАТ В ХОДКУ ПОСЛЕ УСПЕШНОГО ПОБЕГА
    // 🔹 ВОЗВРАТ В ХОДКУ ПОСЛЕ УСПЕШНОГО ПОБЕГА
    public void ReturnToJourneyAfterEscape()
    {
        // 🔹 УБЕДИМСЯ ЧТО КНОПКА ИНВЕНТАРЯ В ПРАВИЛЬНОМ СОСТОЯНИИ (ЗАМЕНИЛИ UnlockInventory)
        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.SetInventoryButtonState(false); // Бой завершен
        }

        SetNormalBackground();
        ShowJourneyButtons();
    }

    public void OnContinueJourney()
    {
        if (!playerStats.CanContinueJourney())
        {
            Debug.Log("Нельзя продолжать: закончилась еда или вода!");
            return;
        }

        // Тратим ресурсы
        int foodCost = Random.Range(1, 11);
        int waterCost = Random.Range(5, 16);

        playerStats.ChangeFood(-foodCost);
        playerStats.ChangeWater(-waterCost);

        Debug.Log($"Потрачено: еда -{foodCost}, вода -{waterCost}");

        // Генерация случайного события
        GenerateRandomEvent();
    }

    void GenerateRandomEvent()
    {
        float eventRoll = Random.Range(0f, 100f);

        if (eventRoll < nothingChance)
        {
            HandleNothingEvent();
        }
        else if (eventRoll < nothingChance + mutantChance)
        {
            HandleMutantEvent();
        }
        else if (eventRoll < nothingChance + mutantChance + banditChance)
        {
            HandleBanditEvent();
        }
        else
        {
            HandleBonusLootEvent();
        }
    }

    void HandleNothingEvent()
    {
        Debug.Log("Ничего не произошло.");

        if (Random.Range(0f, 100f) < lootChance)
        {
            GiveRandomLoot();
        }
    }

    void HandleMutantEvent()
    {
        Debug.Log("⚔️ Встреча с мутантом!");
        StartCombat(CombatSystem.EnemyType.Mutant);
    }

    void HandleBanditEvent()
    {
        Debug.Log("⚔️ Встреча с бандитом!");
        StartCombat(CombatSystem.EnemyType.Bandit);
    }

    void HandleBonusLootEvent()
    {
        Debug.Log("🎁 Найден дополнительный лут!");
        GiveRandomLoot();
    }

    void StartCombat(CombatSystem.EnemyType enemyType)
    {
        // 🔹 Устанавливаем фон врага
        switch (enemyType)
        {
            case CombatSystem.EnemyType.Mutant:
                SetMutantBackground();
                break;
            case CombatSystem.EnemyType.Bandit:
                SetBanditBackground();
                break;
        }

        // Скрываем кнопки ходки при начале боя
        HideJourneyButtons();

        if (combatSystem != null)
        {
            combatSystem.StartCombat(enemyType);
        }
        else
        {
            Debug.LogError("CombatSystem not assigned!");
        }
    }

    // Вызывается из CombatSystem когда бой заканчивается
    // Вызывается из CombatSystem когда бой заканчивается
    // Вызывается из CombatSystem когда бой заканчивается
    public void EndCombatAndReturnToJourney()
    {
        // 🔹 УБЕДИМСЯ ЧТО КНОПКА ИНВЕНТАРЯ В ПРАВИЛЬНОМ СОСТОЯНИИ (ЗАМЕНИЛИ UnlockInventory)
        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null)
        {
            inventoryUI.SetInventoryButtonState(false); // Бой завершен
        }

        // 🔹 Возвращаем обычный фон
        SetNormalBackground();

        // Показываем кнопки ходки
        ShowJourneyButtons();
    }

    void GiveRandomLoot()
    {
        if (possibleLoot.Count > 0)
        {
            Item randomItem = possibleLoot[Random.Range(0, possibleLoot.Count)];
            if (inventory.AddItem(randomItem))
            {
                Debug.Log($"🎁 Получен предмет: {randomItem.itemName}");
            }
        }
    }

    public void OnCompleteJourney()
    {
        if (stationManager != null)
        {
            stationManager.ReturnToStation();
        }
    }

    // 🔹 ВОЗВРАТ В ХОДКУ ПРИ ВХОДЕ В РЕЖИМ ПУТЕШЕСТВИЯ
    public void OnEnterJourneyMode()
    {
        SetNormalBackground();
        ShowJourneyButtons();
    }
}