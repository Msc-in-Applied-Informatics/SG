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
        // Παίρνουμε την είσοδο του χρήστη για την κίνηση του κάδου (π.χ. πλήκτρα A/D ή βελάκια)
        float moveInput = Input.GetAxis("Horizontal"); // Αυτό το Input παίρνει τις εντολές για κίνηση αριστερά/δεξιά

        // Υπολογίζουμε την νέα θέση του κάδου με βάση το Input και την ταχύτητα
        Vector3 newPosition = transform.position + Vector3.right * moveInput * moveSpeed * Time.deltaTime;

        // Διασφαλίζουμε ότι ο κάδος δεν θα ξεπεράσει τα όρια (Boundary)
        newPosition.x = Mathf.Clamp(newPosition.x, -boundaryX, boundaryX);

        // Εφαρμόζουμε την νέα θέση στον κάδο
        transform.position = newPosition;
    }

    private void OnGameStateChanged(GameState newGameState)
    {
        enabled = newGameState == GameState.Gameplay;
    }
}
