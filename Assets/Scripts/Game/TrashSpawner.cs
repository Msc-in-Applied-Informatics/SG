using System.Collections;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    public GameObject[] trashPrefabs;
    public float spawnRate = 2f;
    public float spawnHeight = 6f;
    public float spawnRangeX = 5f;

    private Coroutine spawnCoroutine;

    void Awake()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    void Start()
    {
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
            StartSpawning();
        else
            StopSpawning();
    }

    private void StartSpawning()
    {
        if (spawnCoroutine == null)
            spawnCoroutine = StartCoroutine(SpawnLoop());
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
        while (true)
        {
            SpawnTrash();
            yield return new WaitForSeconds(spawnRate);
        }
    }

    private void SpawnTrash()
    {
        if (trashPrefabs.Length == 0) return;

        float spawnX = Random.Range(-spawnRangeX, spawnRangeX);
        Vector2 spawnPosition = new Vector2(spawnX, spawnHeight);

        int index = Random.Range(0, trashPrefabs.Length);
        GameObject trash = Instantiate(trashPrefabs[index], spawnPosition, Quaternion.identity);

        Rigidbody2D rb = trash.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.gravityScale = 1f;
    }
}
