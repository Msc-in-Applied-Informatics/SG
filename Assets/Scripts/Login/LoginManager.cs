using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;



public class LoginManager : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public Text errorText;

    public void OnLoginButtonClicked()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            errorText.text = "Συμπλήρωσε όλα τα πεδία!";
            return;
        }

        string userKey = "user_" + username;

        // Έλεγχος αν ο χρήστης υπάρχει ήδη
        if (PlayerPrefs.HasKey(userKey))
        {
            string savedPassword = PlayerPrefs.GetString(userKey);
            if (savedPassword == password)
            {
                // Επιτυχής σύνδεση
                PlayerPrefs.SetString("CurrentUser", username);
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                errorText.text = "Λάθος κωδικός!";
            }
        }
        else
        {
            // Δημιουργία νέου χρήστη
            PlayerPrefs.SetString(userKey, password);
            PlayerPrefs.SetString("CurrentUser", username);
            errorText.text = "Νέος χρήστης δημιουργήθηκε!";
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void ResetData()
    {
        //Reset Saved data
        SaveSystem.ClearAllData();
    }
}

