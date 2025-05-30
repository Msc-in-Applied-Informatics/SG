using UnityEngine;

public class TrashObject : MonoBehaviour
{
    void Awake()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        OnGameStateChanged(GameStateManager.Instance.CurrentGameState); // ’μεση εφαρμογή
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newState)
    {
        // Σημείωση: Δεν μπορείς να κάνεις SetActive(false) στον εαυτό σου από μέσα σου.
        // Πρέπει να ελέγχεις από έξω (π.χ. από manager ή parent).

        // Λύση: κάνε απλά ενεργοποίηση/απενεργοποίηση κάποιου child, renderer ή συμπεριφοράς.
        // Εδώ κάνουμε disable τα scripts ή renderers

        bool isActive = (newState == GameState.Gameplay);

        foreach (var comp in GetComponents<MonoBehaviour>())
        {
            if (comp != this)
                comp.enabled = isActive;
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.simulated = isActive;

        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = isActive;

        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
            renderer.enabled = isActive;
    }
}
