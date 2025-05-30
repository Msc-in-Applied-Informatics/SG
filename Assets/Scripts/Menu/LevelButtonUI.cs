using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButtonUI : MonoBehaviour
{
    public int levelIndex;

    public Text levelText;
    public Image[] starImages;
    public Sprite fullStar;
    public Sprite emptyStar;
    public GameObject lockPanel;
    public Button button;

    void Start()
    {
        levelText.text = levelIndex.ToString();

        bool unlocked = SaveSystem.IsLevelUnlocked(levelIndex);
        int stars = SaveSystem.GetStars(levelIndex);

        button.interactable = unlocked;
        lockPanel.SetActive(!unlocked);

        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].sprite = i < stars ? fullStar : emptyStar;
        }

        if (unlocked)
        {
            button.onClick.AddListener(() =>
            {
                SaveSystem.CurrentLevel = levelIndex;
                UnityEngine.SceneManagement.SceneManager.LoadScene("GamePlay");
            });
        }
    }
}
