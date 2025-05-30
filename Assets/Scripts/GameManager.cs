using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int correctTrashCollected = 0;
    public LevelSettingsDatabase settingsDatabase;
    private LevelSettings currentLevelSettings;


    public int score = 0;
    public int lives = 4;
    public int level = 1;
    public int stars = 0;
    public int points = 10;
    public bool isQuizActive = false;


    public Image[] heartImages;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public Image[] armorIcons;
    public Sprite fullArmorSprite;
    public Sprite emptyArmorSprite;

    public GameObject levelCompletePopup;
    public LevelCompleteUIController levelCompleteUI;

    public GameObject bin;

    public Text scoreText;
    public Text livesText;

    private int currentArmor = 2;

    [Header("Quiz Reference")]
    public QuizManager quizManager; // Ανέθεσέ το από το Inspector

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        level = SaveSystem.CurrentLevel;
        currentLevelSettings = settingsDatabase.GetSettings(level);

        Debug.Log($"Level {currentLevelSettings} requires {currentLevelSettings.requiredTrash} trash.");
        StartCoroutine(InitializeAfterFrame());
    }

    IEnumerator InitializeAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        SetLevel(level);
    }

    // --- LEVEL FLOW ---

    public void OnLevelCompleted()
    {
        Time.timeScale = 0f;
        //Set Stats in Pop up (dialog)
        int starsEarned = CalculateStars();
        levelCompleteUI.SetStats(level, score, starsEarned);
        //Save the stars
        SaveSystem.SaveStars(level, starsEarned);

        levelCompletePopup.SetActive(true);
    }

    public int CalculateStars()
    {
        if (lives == 3)
            return 3;
        else if (lives == 2)
            return 2;
        else if (lives == 1)
            return 1;
        else
            return 0;
    }

    public void StartNextLevel()
    {
        levelCompletePopup?.SetActive(false);



        level++;

        if (quizManager != null)
        {          
            quizManager.ShowQuestionForCurrentLevel(); // Το Quiz θα καλέσει ContinueGame() μετά
            Time.timeScale = 1f;
        }
        else
        {
            GameManager.Instance.isQuizActive = false;
            ContinueGame();
        }
    }

    public void ContinueGame()
    {
        quizManager.EnableAllAnswerButtons();
        currentLevelSettings = settingsDatabase.GetSettings(level);
        SetupLevel();
    }

    public void SetupLevel()
    {
        SaveSystem.CurrentLevel = level;
        Debug.Log("Starting Level " + level);

        // Φέρνουμε τα σωστά settings από τη βάση
        currentLevelSettings = settingsDatabase.GetSettings(level);
        if (currentLevelSettings == null)
        {
            Debug.LogError($"LevelSettings not found for level {level}");
            return;
        }

        correctTrashCollected = 0;

        SetBinType(currentLevelSettings.correctTrashType);
        ResetArmor();
        UpdateUI();
        Time.timeScale = 1f;
    }

    public void SetLevel(int newLevel)
    {
        level = newLevel;
        SaveSystem.CurrentLevel = level;

        lives = SaveSystem.GetLives(0);
        score = SaveSystem.GetScore(level);
        stars = SaveSystem.GetStars(level);

        SetupLevel();
    }

    // --- SCORE + LIVES ---

    public void AddScore(int amount)
    {
        score += amount;

        if (score >= level * points)
        {
            //score = 0;
            OnLevelCompleted(); // Εμφάνιση popup
            GameManager.Instance.isQuizActive = true;
            // Optionally Show Quiz
            Invoke(nameof(ShowQuiz), 0.5f);
        }

        UpdateUI();
    }

    public void LoseLife()
    {
        if (currentArmor > 0)
        {
            currentArmor--;
            UpdateArmorUI();
            return;
        }

        lives--;

        if (lives <= 0)
        {
            GameOver();
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

    public void ResetGame()
    {
        if(level == 1)
        {
            score = 0;
            lives = 4;
            level = 1;
        } 
        else 
        {
            score = (level - 1) * 50;
        }
        ResetArmor();
        SetupLevel();
        levelCompletePopup?.SetActive(false);
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
        // TODO: Game Over UI ή reset
    }

    // --- BIN / LEVEL LOGIC ---

    public void SetBinType(TrashItem.TrashType type)
    {
        Bin binScript = bin.GetComponent<Bin>();
        if (binScript == null) return;

        binScript.SetBinType(type);
    }

    TrashItem.TrashType GetTrashTypeForCurrentLevel()
    {
        switch (level)
        {
            case 1: return TrashItem.TrashType.Paper;
            case 2: return TrashItem.TrashType.Plastic;
            case 3: return TrashItem.TrashType.Glass;
            case 4: return TrashItem.TrashType.Aluminum;
            case 5: return TrashItem.TrashType.Organic;
            default:
                return (TrashItem.TrashType)Random.Range(0, 5);
        }
    }

    // --- UI ---

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
        SaveSystem.SaveLives(level, lives);
    }

    void UpdateArmorUI()
    {
        for (int i = 0; i < armorIcons.Length; i++)
        {
            armorIcons[i].sprite = (i < currentArmor) ? fullArmorSprite : emptyArmorSprite;
        }
    }

    public void ResetArmor()
    {
        currentArmor = 2;
        UpdateArmorUI();
    }

    void ShowQuiz()
    {
        if (quizManager != null)
        {
            PlayPause();

            quizManager.ShowQuestionForCurrentLevel();
        }
    }

    public void PlayPause()
    {
        GameState currentGameState = GameStateManager.Instance.CurrentGameState;
        GameState newGameState = currentGameState == GameState.Gameplay
            ? GameState.Paused
            : GameState.Gameplay;

        GameStateManager.Instance.SetState(newGameState);
    }

    public void OnCorrectTrashCollected()
    {
        correctTrashCollected++;
        score += 10;

        if (correctTrashCollected >= currentLevelSettings.requiredTrash)
        {
            OnLevelCompleted();
            isQuizActive = true;
            Invoke(nameof(ShowQuiz), 0.5f);
        }

        UpdateUI();
    }
}
