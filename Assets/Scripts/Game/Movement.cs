using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f;  // Ταχύτητα κίνησης του κάδου
    public float boundaryX = 8f;  // Όριο για την κίνηση του κάδου (αριστερά/δεξιά)

    void Awake()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isQuizActive)
            return;
        float moveInput = Input.GetAxis("Horizontal"); 

        Vector3 newPosition = transform.position + Vector3.right * moveInput * moveSpeed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, -boundaryX, boundaryX);


        transform.position = newPosition;
    }

    private void OnGameStateChanged(GameState newGameState)
    {
        enabled = newGameState == GameState.Gameplay;
    }
}
