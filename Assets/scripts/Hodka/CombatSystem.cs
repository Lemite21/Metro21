using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CombatSystem : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject combatPanel;
    public Image combatBackground;
    public Button attackButton;
    public Button secondaryWeaponButton;
    public Button escapeButton;
    public Button reloadButton;
    public Button coverButton;
    public TMP_Text combatLog;
    public TMP_Text enemyHealthText;

    [Header("Enemy Settings")]
    public Sprite mutantBackground;
    public Sprite banditBackground;

    [System.Serializable]
    public class EnemySettings
    {
        public int health;
        public int minDamage;
        public int maxDamage;
        public List<LootItem> lootTable;
    }

    [System.Serializable]
    public class LootItem
    {
        public Item item;
        public int minQuantity = 1;
        public int maxQuantity = 1;
        [Range(0, 100)] public float dropChance = 100f;
    }

    public EnemySettings mutantSettings;
    public EnemySettings banditSettings;

    [Header("References")]
    public InventoryUI inventoryUI; // 🔹 ДОБАВЬ ЭТУ ССЫЛКУ!
    private PlayerStats playerStats;
    private EquipmentSystem equipment;
    private InventorySystem inventory;
    private StationManager stationManager;
    private HodkaManager hodkaManager;

    private bool combatActive = false;
    private bool playerTurn = true;
    private EnemyType currentEnemyType;
    private int currentEnemyHealth;
    private EnemySettings currentEnemySettings;

    public enum EnemyType { Mutant, Bandit }

    void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        equipment = FindFirstObjectByType<EquipmentSystem>();
        inventory = FindFirstObjectByType<InventorySystem>();
        stationManager = FindFirstObjectByType<StationManager>();
        hodkaManager = FindFirstObjectByType<HodkaManager>();

        // 🔹 ЕСЛИ ССЫЛКА НЕ НАЗНАЧЕНА - НАЙДЕМ АВТОМАТИЧЕСКИ
        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
            if (inventoryUI != null)
            {
                Debug.Log("✅ InventoryUI найден автоматически");
            }
            else
            {
                Debug.LogError("❌ InventoryUI не найден!");
            }
        }

        combatPanel.SetActive(false);

        // Настройка кнопок
        attackButton.onClick.AddListener(OnAttack);
        secondaryWeaponButton.onClick.AddListener(OnSecondaryWeapon);
        escapeButton.onClick.AddListener(OnEscape);
        reloadButton.onClick.AddListener(OnReload);
        coverButton.onClick.AddListener(OnCover);
    }

    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД НАЧАЛА БОЯ
    public void StartCombat(EnemyType enemyType)
    {
        Debug.Log("⚔️ НАЧАЛО БОЯ - МЕНЯЕМ КНОПКУ ИНВЕНТАРЯ");

        combatActive = true;
        playerTurn = true;
        currentEnemyType = enemyType;

        // 🔹 МЕНЯЕМ КНОПКУ ИНВЕНТАРЯ НА СЕРУЮ (НЕ УДАЛЯЕМ ИНВЕНТАРЬ!)
        if (inventoryUI != null)
        {
            inventoryUI.SetInventoryButtonState(true); // Бой активен
        }
        else
        {
            Debug.LogError("❌ InventoryUI не найден для смены кнопки!");
        }

        // Настраиваем врага
        switch (enemyType)
        {
            case EnemyType.Mutant:
                combatBackground.sprite = mutantBackground;
                currentEnemySettings = mutantSettings;
                break;
            case EnemyType.Bandit:
                combatBackground.sprite = banditBackground;
                currentEnemySettings = banditSettings;
                break;
        }

        currentEnemyHealth = currentEnemySettings.health;

        // Показываем панель боя
        combatPanel.SetActive(true);

        UpdateCombatUI();
        AddCombatLog($"Начало боя с {enemyType}");

        // Скрываем кнопки ходки
        if (hodkaManager != null)
        {
            hodkaManager.HideJourneyButtons();
        }
    }

    void UpdateCombatUI()
    {
        // Показываем только число здоровья врага
        if (enemyHealthText != null)
            enemyHealthText.text = $"{currentEnemyHealth}";

        // Проверяем доступность оружия
        bool hasMainWeapon = equipment.weaponMain.item != null && !inventory.IsWeaponEmpty(equipment.weaponMain.item);
        bool hasSecondaryWeapon = equipment.weaponSecondary.item != null && !inventory.IsWeaponEmpty(equipment.weaponSecondary.item);

        // 🔹 ИСПОЛЬЗУЕМ НОВЫЙ МЕНЕДЖЕР ДЛЯ ПРОВЕРКИ ПЕРЕЗАРЯДКИ
        ReloadManager reloadManager = FindFirstObjectByType<ReloadManager>();
        bool needsReload = reloadManager != null && reloadManager.CanReloadAnyWeapon();

        // 🔹 ДОБАВИМ ДЕТАЛЬНУЮ ОТЛАДКУ
        Debug.Log($"Проверка перезарядки: доступно = {needsReload}");
        if (reloadManager != null)
        {
            Debug.Log($"Статус перезарядки: {reloadManager.GetReloadStatus()}");
        }

        // Блокируем кнопки во время хода противника
        bool buttonsInteractable = combatActive && playerTurn;

        attackButton.interactable = hasMainWeapon && playerStats.energy >= 30 && buttonsInteractable;
        secondaryWeaponButton.interactable = hasSecondaryWeapon && playerStats.energy >= 15 && buttonsInteractable;
        reloadButton.interactable = needsReload && playerStats.energy >= 10 && buttonsInteractable;
        escapeButton.interactable = buttonsInteractable;
        coverButton.interactable = buttonsInteractable;

        UpdateWeaponButtonTexts();
    }

    void UpdateWeaponButtonTexts()
    {
        if (equipment.weaponMain.item != null)
        {
            TMP_Text text = attackButton.GetComponentInChildren<TMP_Text>();
            text.text = $"Атака ({equipment.weaponMain.item.currentAmmo}/{equipment.weaponMain.item.maxAmmo})";
        }

        if (equipment.weaponSecondary.item != null)
        {
            TMP_Text text = secondaryWeaponButton.GetComponentInChildren<TMP_Text>();
            text.text = $"Доп. оружие ({equipment.weaponSecondary.item.currentAmmo}/{equipment.weaponSecondary.item.maxAmmo})";
        }
    }

    // ОБЩИЙ МЕТОД ДЛЯ БЛОКИРОВКИ КНОПОК
    void SetButtonsInteractable(bool interactable)
    {
        attackButton.interactable = interactable;
        secondaryWeaponButton.interactable = interactable;
        escapeButton.interactable = interactable;
        reloadButton.interactable = interactable;
        coverButton.interactable = interactable;
    }

    // КНОПКА: Атаковать (основное оружие)
    public void OnAttack()
    {
        if (!combatActive || !playerTurn || equipment.weaponMain.item == null) return;
        StartCoroutine(PerformAttackSequence(equipment.weaponMain.item, 30, "основным оружием"));
    }

    // КНОПКА: Доп. оружие
    public void OnSecondaryWeapon()
    {
        if (!combatActive || !playerTurn || equipment.weaponSecondary.item == null) return;
        StartCoroutine(PerformAttackSequence(equipment.weaponSecondary.item, 15, "дополнительным оружием"));
    }

    // ПОСЛЕДОВАТЕЛЬНОСТЬ АТАКИ С ЗАДЕРЖКАМИ
    // 🔹 ОБНОВЛЕННЫЙ МЕТОД АТАКИ С ОЧЕРЕДЬЮ
    IEnumerator PerformAttackSequence(Item weapon, int energyCost, string weaponType)
    {
        playerTurn = false;
        SetButtonsInteractable(false);

        // Проверка клина
        if (CheckJamming(weapon))
        {
            AddCombatLog($"{weapon.itemName} заклинило!");
            playerStats.ChangeEnergy(-energyCost);
            yield return new WaitForSeconds(2f);
            StartCoroutine(EnemyTurnSequence());
            yield break;
        }

        // Проверка патронов
        if (weapon.IsWeaponEmpty())
        {
            AddCombatLog($"{weapon.itemName} пусто!");
            playerTurn = true;
            SetButtonsInteractable(true);
            yield break;
        }

        // 🔹 СТРЕЛЬБА ОЧЕРЕДЬЮ ДЛЯ АВТОМАТОВ
        int shotsFired = 1;
        int totalDamage = 0;

        if (weapon.shotsPerAttack > 1)
        {
            // Автомат - стреляем очередью
            shotsFired = weapon.ShootBurst();
            for (int i = 0; i < shotsFired; i++)
            {
                totalDamage += Random.Range(weapon.minDamage, weapon.maxDamage + 1);
            }
            AddCombatLog($"Вы выпустили очередь из {shotsFired} патронов!");
        }
        else
        {
            // Одиночный выстрел
            shotsFired = 1;
            totalDamage = weapon.GetWeaponDamage();
            weapon.UseInCombat(); // Старый метод для одиночных выстрелов
        }

        // Наносим урон
        currentEnemyHealth -= totalDamage;

        // Тратим энергию
        playerStats.ChangeEnergy(-energyCost);

        AddCombatLog($"Вы атаковали {weaponType} и нанесли {totalDamage} урона ({shotsFired} выстрелов)");

        UpdateCombatUI();

        // Проверяем победу
        if (currentEnemyHealth <= 0)
        {
            yield return new WaitForSeconds(2f);
            EndCombat(true);
            yield break;
        }

        yield return new WaitForSeconds(2f);
        StartCoroutine(EnemyTurnSequence());
    }

    // КНОПКА: Побег 50%
    public void OnEscape()
    {
        if (!combatActive || !playerTurn) return;
        StartCoroutine(PerformEscapeSequence());
    }

    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ПОБЕГА
    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ПОБЕГА
    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ПОБЕГА
    IEnumerator PerformEscapeSequence()
    {
        playerTurn = false;
        SetButtonsInteractable(false);

        bool escaped = Random.Range(0, 2) == 0;

        if (escaped)
        {
            AddCombatLog("Вы успешно сбежали");
            yield return new WaitForSeconds(2f);

            // 🔹 ВОЗВРАЩАЕМ ОБЫЧНУЮ КНОПКУ ИНВЕНТАРЯ (ЗАМЕНИЛИ UnlockInventory)
            if (inventoryUI != null)
            {
                inventoryUI.SetInventoryButtonState(false);
            }

            // 🔹 ВОЗВРАТ В ХОДКУ ПРИ УСПЕШНОМ ПОБЕГЕ
            if (hodkaManager != null)
            {
                hodkaManager.ReturnToJourneyAfterEscape();
            }
            combatPanel.SetActive(false);
        }
        else
        {
            AddCombatLog("Побег не удался");
            yield return new WaitForSeconds(2f);
            StartCoroutine(EnemyTurnSequence());
        }
    }



    // КНОПКА: Перезарядка
    public void OnReload()
    {
        if (!combatActive || !playerTurn) return;
        StartCoroutine(PerformReloadSequence());
    }

    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ПЕРЕЗАРЯДКИ
    // 🔹 УЛУЧШЕННЫЙ МЕТОД ПЕРЕЗАРЯДКИ В БОЮ
    // 🔹 ПОЛНЫЙ МЕТОД ПЕРЕЗАРЯДКИ С ОБОИМИ СЛОТАМИ
    // 🔹 УЛУЧШЕННЫЙ МЕТОД ПЕРЕЗАРЯДКИ
    IEnumerator PerformReloadSequence()
    {
        playerTurn = false;
        SetButtonsInteractable(false);

        AddCombatLog("Перезарядка...");
        playerStats.ChangeEnergy(-10);

        // 🔹 ИСПОЛЬЗУЕМ НОВЫЙ МЕНЕДЖЕР ПЕРЕЗАРЯДКИ
        ReloadManager reloadManager = FindFirstObjectByType<ReloadManager>();
        if (reloadManager == null)
        {
            reloadManager = gameObject.AddComponent<ReloadManager>();
        }

        var reloadResult = reloadManager.ReloadAllWeapons();

        // 🔹 ФОРМИРУЕМ СООБЩЕНИЕ О РЕЗУЛЬТАТЕ
        if (reloadResult.anyWeaponReloaded)
        {
            AddCombatLog("Перезарядка выполнена!");

            if (reloadResult.mainWeaponReloaded)
                AddCombatLog("Основное оружие перезаряжено");

            if (reloadResult.secondaryWeaponReloaded)
                AddCombatLog("Доп. оружие перезаряжено");
        }
        else
        {
            AddCombatLog("Не удалось перезарядить оружие. Проверьте патроны.");
        }

        // 🔹 ОБНОВЛЯЕМ UI
        UpdateCombatUI();

        yield return new WaitForSeconds(2f);
        StartCoroutine(EnemyTurnSequence());
    }

    // КНОПКА: В укрытие
    public void OnCover()
    {
        if (!combatActive || !playerTurn) return;
        StartCoroutine(PerformCoverSequence());
    }

    IEnumerator PerformCoverSequence()
    {
        playerTurn = false;
        SetButtonsInteractable(false);

        AddCombatLog("Вы укрылись");
        playerStats.ChangeEnergy(60);

        yield return new WaitForSeconds(2f);
        StartCoroutine(EnemyTurnSequence());
    }

    // ХОД ПРОТИВНИКА С ЗАДЕРЖКОЙ
    IEnumerator EnemyTurnSequence()
    {
        AddCombatLog($"Ход {currentEnemyType}");
        yield return new WaitForSeconds(2f);

        // 25% шанс промаха врага
        if (Random.Range(0, 100) < 25)
        {
            AddCombatLog($"{currentEnemyType} промахнулся");
        }
        else
        {
            int damage = Random.Range(currentEnemySettings.minDamage, currentEnemySettings.maxDamage + 1);
            playerStats.TakeDamage(damage);
            AddCombatLog($"{currentEnemyType} атаковал и нанес {damage} урона");

            // Проверяем смерть игрока
            if (!playerStats.IsAlive())
            {
                PlayerDeath();
                yield break;
            }
        }

        playerTurn = true;
        UpdateCombatUI();
    }

    bool CheckJamming(Item weapon)
    {
        if (!weapon.hasDurability) return false;
        float jamChance = weapon.GetJammingChance();
        return Random.Range(0f, 1f) < jamChance;
    }

    // В метод EndCombat добавляем разблокировку:
    // В метод EndCombat добавляем разблокировку:
    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ЗАВЕРШЕНИЯ БОЯ
    void EndCombat(bool victory)
    {
        Debug.Log("🏁 КОНЕЦ БОЯ - ВОЗВРАЩАЕМ КНОПКУ ИНВЕНТАРЯ");

        combatActive = false;

        // 🔹 ВОЗВРАЩАЕМ ОБЫЧНУЮ КНОПКУ ИНВЕНТАРЯ
        if (inventoryUI != null)
        {
            inventoryUI.SetInventoryButtonState(false); // Бой завершен
        }

        if (victory)
        {
            AddCombatLog("Победа! Получена добыча");
            GiveLoot();
        }

        // Возвращаем к обычному режиму через 2 секунды
        StartCoroutine(ReturnToJourneyMode());
    }

    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ВОЗВРАТА В ХОДКУ
    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ВОЗВРАТА В ХОДКУ
    IEnumerator ReturnToJourneyMode()
    {
        yield return new WaitForSeconds(2f);

        // 🔹 ВОЗВРАТ В ХОДКУ ЧЕРЕЗ HODKA MANAGER
        if (hodkaManager != null)
        {
            hodkaManager.EndCombatAndReturnToJourney();
        }

        // 🔹 ВОЗВРАЩАЕМ ОБЫЧНУЮ КНОПКУ ИНВЕНТАРЯ (ЗАМЕНИЛИ UnlockInventory)
        if (inventoryUI != null)
        {
            inventoryUI.SetInventoryButtonState(false); // Бой завершен
        }

        combatPanel.SetActive(false);
    }

    // В метод PlayerDeath тоже добавляем разблокировку:
    void PlayerDeath()
    {
        AddCombatLog("Вы погибли");

        // 🔹 РАЗБЛОКИРОВКА ИНВЕНТАРЯ ПРИ СМЕРТИ
        if (inventoryUI != null)
        {
            inventoryUI.UnlockInventory();
        }

        // Удаление всех предметов
        inventory.items.Clear();

        // Сброс экипировки
        equipment.helmet.item = null;
        equipment.chest.item = null;
        equipment.legs.item = null;
        equipment.weaponMain.item = null;
        equipment.weaponSecondary.item = null;

        // Обновление UI
        if (inventoryUI != null) inventoryUI.RefreshUI();
        var equipUI = FindFirstObjectByType<EquipmentUI>();
        if (equipUI != null) equipUI.RefreshUI();

        StartCoroutine(ReturnToStationAfterDeath());
    }

    // ВОЗВРАТ НА СТАНЦИЮ ПОСЛЕ СМЕРТИ
    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД СМЕРТИ
    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД СМЕРТИ
    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД СМЕРТИ
    IEnumerator ReturnToStationAfterDeath()
    {
        yield return new WaitForSeconds(2f);

        // 🔹 ВОЗВРАЩАЕМ ОБЫЧНУЮ КНОПКУ ИНВЕНТАРЯ (ЗАМЕНИЛИ UnlockInventory)
        if (inventoryUI != null)
        {
            inventoryUI.SetInventoryButtonState(false);
        }

        combatPanel.SetActive(false);

        // 🔹 ВОЗВРАТ НА СТАНЦИЮ ЧЕРЕЗ STATION MANAGER
        if (stationManager != null)
        {
            stationManager.ReturnToStationAfterDeath();
        }
    }

    void GiveLoot()
    {
        foreach (var lootItem in currentEnemySettings.lootTable)
        {
            if (Random.Range(0f, 100f) <= lootItem.dropChance)
            {
                int quantity = Random.Range(lootItem.minQuantity, lootItem.maxQuantity + 1);

                for (int i = 0; i < quantity; i++)
                {
                    Item itemCopy = Instantiate(lootItem.item);
                    inventory.AddItem(itemCopy);
                }

                AddCombatLog($"Получено: {lootItem.item.itemName} x{quantity}");
            }
        }

        if (inventoryUI != null) inventoryUI.RefreshUI();
    }

    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ДЛЯ COMBAT LOG
    // 🔹 ИСПРАВЛЕННЫЙ МЕТОД ДЛЯ COMBAT LOG
    void AddCombatLog(string message)
    {
        if (combatLog != null)
        {
            // Ограничиваем длину текста чтобы не выходил за границы
            string newText = combatLog.text;

            // Добавляем новое сообщение
            if (string.IsNullOrEmpty(newText))
            {
                newText = message;
            }
            else
            {
                newText = message + "\n" + newText;
            }

            // Ограничиваем количество строк (например, 6 строк)
            string[] lines = newText.Split('\n');
            if (lines.Length > 6)
            {
                // Берем только последние 6 строк
                System.Text.StringBuilder limitedText = new System.Text.StringBuilder();
                for (int i = 0; i < 6; i++)
                {
                    if (i > 0) limitedText.Append("\n");
                    limitedText.Append(lines[i]);
                }
                newText = limitedText.ToString();
            }

            combatLog.text = newText;
        }
        Debug.Log($"Combat: {message}");
    }
}