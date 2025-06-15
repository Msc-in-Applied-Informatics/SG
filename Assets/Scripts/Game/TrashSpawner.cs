using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    public GameObject[] trashPrefabs;
    public float spawnRate = 2f;
    public float spawnHeight = 6f;
    public float spawnRangeX = 5f;

    private Coroutine spawnCoroutine;
    private float elapsedTime = 0f;
    private float totalLevelTime = 60f;  

    void Awake()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    void Start()
    {
        totalLevelTime = GameManager.Instance.currentLevelSettings.maxTime;

        if (GameStateManager.Instance.CurrentGameState == GameState.Gameplay)
            StartSpawning();
    }

    void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Gameplay)
        {
            Time.timeScale = 1f;
            StartSpawning();
        }
        else
            StopSpawning();
    }

    private void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            elapsedTime = 0f; // reset timer
            spawnRate = GetSpawnRateForLevel(GameManager.Instance.level);
            spawnCoroutine = StartCoroutine(SpawnLoop());
        }
    }

    private void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        float timer = 0f;

        while (true)
        {
            while (timer < spawnRate)
            {
                timer += Time.deltaTime;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            timer = 0f;
            SpawnTrash();
        }
    }

    private void SpawnTrash()
    {
        int currentLevel = GameManager.Instance.level;
        TrashItem.TrashType correctType = GameManager.Instance.currentLevelSettings.correctTrashType;

        // Initialize Prefabs
        List<GameObject> correctPrefabs = new List<GameObject>();
        List<GameObject> otherPrefabs = new List<GameObject>();

        HashSet<TrashItem.TrashType> allowedTypes = new HashSet<TrashItem.TrashType> { correctType };

        if (currentLevel > 6)
        {
            int maxExtraTypes = Mathf.Min((currentLevel - 6), System.Enum.GetValues(typeof(TrashItem.TrashType)).Length - 1);

            foreach (TrashItem.TrashType type in System.Enum.GetValues(typeof(TrashItem.TrashType)))
            {
                if (type != correctType && allowedTypes.Count <= maxExtraTypes)
                    allowedTypes.Add(type);
            }
        }

        // Γεμίζουμε τις λίστες correct / others
        foreach (GameObject prefab in trashPrefabs)
        {
            TrashItem item = prefab.GetComponent<TrashItem>();
            if (item == null || !allowedTypes.Contains(item.trashType))
                continue;

            if (item.trashType == correctType)
                correctPrefabs.Add(prefab);
            else
                otherPrefabs.Add(prefab);
        }

        // Αν δεν έχουμε σκουπίδια, δεν κάνουμε spawn
        if (correctPrefabs.Count == 0 && otherPrefabs.Count == 0) return;

        float spawnX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector2 spawnPosition = new Vector2(spawnX, spawnHeight);

        // Υπολογισμός πιθανότητας εμφάνισης σωστού σκουπιδιού
        float probabilityCorrect = GetCorrectTrashProbability();

        GameObject prefabToSpawn;

        if (Random.value < probabilityCorrect && correctPrefabs.Count > 0)
        {
            prefabToSpawn = correctPrefabs[Random.Range(0, correctPrefabs.Count)];
        }
        else if (otherPrefabs.Count > 0)
        {
            prefabToSpawn = otherPrefabs[Random.Range(0, otherPrefabs.Count)];
        }
        else
        {
            prefabToSpawn = correctPrefabs[Random.Range(0, correctPrefabs.Count)];
        }

        GameObject spawned = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        TrashItem spawnItem = spawned.GetComponent<TrashItem>();
        if (spawnItem != null)
        {
            GameManager.Instance.OnTrashSpawned(spawnItem.trashType);
        }

        Rigidbody2D rb = spawned.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.gravityScale = 1f;
    }

    private float GetCorrectTrashProbability()
    {
         float startProb = 0.3f;
        float endProb = 0.65f;

        float t = Mathf.Clamp01(elapsedTime / totalLevelTime);
        return Mathf.SmoothStep(startProb, endProb, t);
    }

    private float GetSpawnRateForLevel(int level)
    {
        if (level <= 5) return 2.0f;
        if (level <= 10) return 1.6f;
        if (level <= 15) return 1.3f;
        if (level <= 20) return 1.1f;
        return 1.0f;
    }
}
