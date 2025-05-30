using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCompleteUIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;

    public Image[] stars;
    public Sprite fullStar;
    public Sprite emptyStar;

    public void SetStats(int level, int score, int starsEarned)
    {
        levelText.text = "Level " + level;
        scoreText.text = "Score: " + score;

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].sprite = (i < starsEarned) ? fullStar : emptyStar;
        }
    }
}
