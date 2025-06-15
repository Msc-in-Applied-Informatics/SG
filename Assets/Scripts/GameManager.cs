using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Settings & References")]
    public LevelSettingsDatabase settingsDatabase;
    public LevelSettings currentLevelSettings;
    public GameObject bin;
    public QuizManager quizManager;

    [Header("UI")]
    public Text timerText;
    public Text scoreText;
    public Text livesText;
    public GameObject levelCompletePopup;
    public LevelCompleteUIController levelCompleteUI;
    public GameObject gameOverPanel;
    public GameObject gameMenuPanel;
    public Image[] heartImages;
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public Image[] armorIcons;
    public Sprite fullArmorSprite;
    public Sprite emptyArmorSprite;

    [Header("Trash Progress UI")]
    public Slider trashProgressSlider;
    public Image trashTypeIcon;
    public Sprite paperIcon, plasticIcon, glassIcon, aluminumIcon, organicIcon, electricIcon, battery;
    public Text counterTotalTrash;

    [Header("Gameplay Values")]
    public int score = 0;
    public int lives = 3;
    public int level = 1;
    public int stars = 0;
    public int points = 10;

    [Header("Wait for Regen Life")]
    public Text lifeTimerText;
    public Button retryButton;
    public GameObject gameTimeRegenLifePanel;
    private bool waitingForLife = false;

    [Header("End Game")]
    public GameObject youWinPanel;

    private int currentArmor = 0;
    private int correctTrashCollected = 0;
    private int wrongTrashCollected = 0;
    private int totalTrashSpawned = 0;
    private int totalCorrectTrashSpawned = 0;
    private float timeRemaining;
    private bool isTimerRunning = false;
    private bool gameIsOver = false;
    public bool isQuizActive = false;
    private bool shouldShowQuizAfterLevelComplete = false;
    //private bool isRetryingLevel = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    void Start()
    {
        AudioManager.Instance.PlayMusic(AudioManager.Instance.backgroundMusicClip);
        if (!CheckLifeRegen()) return;
        level = SaveSystem.CurrentLevel;
        currentLevelSettings = settingsDatabase.GetSettings(level);
        StartCoroutine(InitializeAfterFrame());
    }

    IEnumerator InitializeAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        SetLevel(level);
    }

    void Update()
    {
        if (isTimerRunning && !isQuizActive && !waitingForLife)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();

            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                isTimerRunning = false;
                GameOver();
            }
        }

        ChangeLifeTimer();
    }

    void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newState)
    {
        isTimerRunning = (newState == GameState.Gameplay);
        if (gameMenuPanel != null && !gameIsOver)
            gameMenuPanel.SetActive(newState == GameState.Paused);
    }

    public void PlayPause(bool start, string mode)
    {
        GameState current = GameStateManager.Instance.CurrentGameState;
        GameState target = mode == "Sleep"
            ? GameState.Sleep
            : start ? GameState.Gameplay : (current == GameState.Gameplay ? GameState.Paused : GameState.Gameplay);

        GameStateManager.Instance.SetState(target);
        isTimerRunning = (target == GameState.Gameplay);
    }

    public void SetLevel(int newLevel)
    {   
        level = newLevel;
        SaveSystem.CurrentLevel = level;

        //lives = SaveSystem.GetLives(level);
        lives = SaveSystem.GetGlobalLives();
        score = 0;
        currentArmor = SaveSystem.GetGlobalArmor();
        stars = SaveSystem.GetStars(level);
        SetupLevel();
    }

    public void SetupLevel()
    {
        currentLevelSettings = settingsDatabase.GetSettings(level);
        if (currentLevelSettings == null)
        {
            Debug.LogError($"LevelSettings not found for level {level}");
            return;
        }
        lives = SaveSystem.GetGlobalLives();

        correctTrashCollected = 0;
        wrongTrashCollected = 0;
        totalTrashSpawned = 0;
        totalCorrectTrashSpawned = 0;
        //if (!isRetryingLevel)
        //    SetupBalancedStats();

        SetBinType(currentLevelSettings.correctTrashType);
        UpdateTrashIcon(currentLevelSettings.correctTrashType);

        trashProgressSlider.maxValue = currentLevelSettings.requiredTrash;
        trashProgressSlider.value = 0;
        counterTotalTrash.text = currentLevelSettings.requiredTrash.ToString();

        timeRemaining = currentLevelSettings.maxTime;
        isTimerRunning = true;
        shouldShowQuizAfterLevelComplete = false;
        Time.timeScale = 1f;
        PlayPause(true, "");

        UpdateUI();
        //isRetryingLevel = false;
    }

    void SetupBalancedStats()
    {
        if (level <= 5) { lives = 3; }
        else if (level <= 10) { lives = 3; }
        else if (level <= 15) { lives = 2; }
        else { lives = 2;}
    }

    public void ContinueGame()
    {
        level++;

        if (level > 20)
        {
            ShowYouWin();
            return;
        }

        score = 0;
        quizManager?.EnableAllAnswerButtons();
        SetupLevel();
    }

    public void ShowYouWin()
    {
        isTimerRunning = false;
        PlayPause(false, "Sleep");
        Time.timeScale = 0f;
        AudioManager.Instance.PlaySound(AudioManager.Instance.victoryClip);
        if (youWinPanel != null)
            youWinPanel.SetActive(true);
    }

    public void RetryLevel()
    {
        //isRetryingLevel = true;
        if (!CheckLifeRegen()) return;
        gameIsOver = false;
        Time.timeScale = 1f;
        gameOverPanel?.SetActive(false);
        ResetGame();
    }

    public void ResetGame()
    {
        score = 0;
        SetupLevel();
        levelCompletePopup?.SetActive(false);
        shouldShowQuizAfterLevelComplete = false;
    }

    public void StartNextLevel()
    {
        levelCompletePopup?.SetActive(false);

        if (shouldShowQuizAfterLevelComplete && quizManager != null)
        {
            shouldShowQuizAfterLevelComplete = false;
            quizManager.ShowQuestionForCurrentLevel();
            Time.timeScale = 1f;
        }
        else
        {
            isQuizActive = false;
            ContinueGame();
        }
    }

    public void GoToMainMenu()
    {
        AudioManager.Instance.StopMusic();
        gameTimeRegenLifePanel.gameObject.SetActive(false);
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        PlayPause(false, "");
    }

    public void Logout()
    {
        gameTimeRegenLifePanel.gameObject.SetActive(false);
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
        PlayPause(false, "");
    }

    public void OnLevelCompleted()
    {
        //Play sound
        AudioManager.Instance.PlaySound(AudioManager.Instance.levelCompleteClip);

        isTimerRunning = false;
        Time.timeScale = 0f;
        PlayPause(false, "Sleep");

        int starsEarned = CalculateStars();
        levelCompleteUI.SetStats(level, score, starsEarned);
        SaveSystem.SaveStars(level, starsEarned);
        levelCompletePopup.SetActive(true);
        shouldShowQuizAfterLevelComplete = true;
    }

    public int CalculateStars()
    {
        if (totalCorrectTrashSpawned == 0) return 0;

        int missedCorrect = totalCorrectTrashSpawned - correctTrashCollected;
        int totalConsidered = correctTrashCollected + wrongTrashCollected + missedCorrect;

        float accuracy = (float)correctTrashCollected / totalConsidered;

        Debug.Log($"Correct: {correctTrashCollected}, Wrong: {wrongTrashCollected}, Missed: {missedCorrect}, Total: {totalConsidered}, Accuracy: {accuracy}");

        if (accuracy >= 0.85f) return 3;
        else if (accuracy >= 0.60f) return 2;
        else if (accuracy >= 0.35f) return 1;
        else return 0;
    }

    void GameOver()
    {
        //Play Sound
        AudioManager.Instance.PlaySound(AudioManager.Instance.gameOverClip);

        SaveSystem.SaveGlobalArmor(currentArmor);
        isTimerRunning = false;
        gameIsOver = true;
        timerText.text = "Time: 00:00";
        PlayPause(false, "");
        if(CheckLifeRegen())
            gameOverPanel?.SetActive(true);
    }

    public void OnTrashSpawned(TrashItem.TrashType spawnedType)
    {
        totalTrashSpawned++;

        if (spawnedType == currentLevelSettings.correctTrashType)
        {
            totalCorrectTrashSpawned++;
        }
    }

    public void OnCorrectTrashCollected()
    {
        correctTrashCollected++;
        score += 10;
        trashProgressSlider.value = correctTrashCollected;
        //Play sound
        AudioManager.Instance.PlaySound(AudioManager.Instance.trashCollectedClip);

        if (correctTrashCollected >= currentLevelSettings.requiredTrash)
            OnLevelCompleted();

        UpdateUI();
    }

    public void OnWrongTrashCollected()
    {
        wrongTrashCollected++;
        AudioManager.Instance.PlaySound(AudioManager.Instance.wrongTrashCollectedClip);
        LoseLife();
        UpdateUI();
    }

    public void SetBinType(TrashItem.TrashType type)
    {
        Bin binScript = bin.GetComponent<Bin>();
        if (binScript != null)
            binScript.SetBinType(type);
    }

    void UpdateTrashIcon(TrashItem.TrashType type)
    {
        switch (type)
        {
            case TrashItem.TrashType.Paper: trashTypeIcon.sprite = paperIcon; break;
            case TrashItem.TrashType.Plastic: trashTypeIcon.sprite = plasticIcon; break;
            case TrashItem.TrashType.Glass: trashTypeIcon.sprite = glassIcon; break;
            case TrashItem.TrashType.Aluminum: trashTypeIcon.sprite = aluminumIcon; break;
            case TrashItem.TrashType.Organic: trashTypeIcon.sprite = organicIcon; break;
            case TrashItem.TrashType.Electronics: trashTypeIcon.sprite = electricIcon; break;
            case TrashItem.TrashType.Battery: trashTypeIcon.sprite = battery; break;
        }
    }

    public void LoseLife()
    {
        if (currentArmor > 0)
        {
            currentArmor--;
            SaveSystem.SaveGlobalArmor(currentArmor);
            UpdateArmorUI();
            return;
        }

        lives--;
        //Play sound
        //AudioManager.Instance.PlaySound(AudioManager.Instance.lifeLostClip);
        if (lives <= 0)
        {
            lives = 0;
            SaveSystem.SaveGlobalLives(lives);

            // Αποθήκευση χρόνου που μπορεί να ξαναπαίξει (5 λεπτά αργότερα)
            System.DateTime regenTime = System.DateTime.Now.AddMinutes(5);
            SaveSystem.SaveNextLifeRegenTime(regenTime);

            GameOver();
            return;
        }

        UpdateUI();
    }

    public void AddLife()
    {
        if (lives < heartImages.Length)
        {
            lives++;
            UpdateUI();
        }
    }

    public void GrantArmor()
    {
        if (currentArmor < armorIcons.Length)
        {
            currentArmor++;
            SaveSystem.SaveGlobalArmor(currentArmor);
            UpdateArmorUI();
        }
    }

    public void ResetArmor()
    {
        UpdateArmorUI();
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        livesText.text = "Lives: " + lives;
        UpdateHearts();
        UpdateArmorUI();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            heartImages[i].sprite = (i < lives) ? fullHeart : emptyHeart;
        }

        SaveSystem.SaveScore(level, score);
        SaveSystem.SaveGlobalLives(lives);
        //SaveSystem.SaveLives(level, lives);
    }

    void UpdateArmorUI()
    {
        for (int i = 0; i < armorIcons.Length; i++)
        {
            armorIcons[i].sprite = (i < currentArmor) ? fullArmorSprite : emptyArmorSprite;
        }
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    public void RegenAndStart()
    {
        gameTimeRegenLifePanel.gameObject.SetActive(false);
        SetLevel(level);
    }

    // Check Life 
    bool CheckLifeRegen()
    {
        if (SaveSystem.GetGlobalLives() > 0)
        {
            return true;
        }

        System.DateTime regenTime = SaveSystem.GetNextLifeRegenTime();

        if (System.DateTime.Now >= regenTime){
            SaveSystem.SaveGlobalLives(3);
            gameTimeRegenLifePanel.gameObject.SetActive(false);
            retryButton.interactable = true;
            return true;
        }
        else{
            waitingForLife = true;
            isTimerRunning = false;
            //Time.timeScale = 0f;
            PlayPause(false, "Sleep");
            retryButton.interactable = false;
            gameTimeRegenLifePanel.gameObject.SetActive(true);
            return false;
        }
    }

    void ChangeLifeTimer()
    {
        if (waitingForLife)
        {
            System.TimeSpan remaining = SaveSystem.GetNextLifeRegenTime() - System.DateTime.Now;

            if (remaining.TotalSeconds <= 0)
            {
                SaveSystem.SaveGlobalLives(3);
                waitingForLife = false;
                //lifeTimerText.gameObject.SetActive(false);
                retryButton.interactable = true;
            }
            else
            {
                lifeTimerText.text = $"Next lives in {remaining.Minutes:00}:{remaining.Seconds:00}";
            }
        }
    }
}
