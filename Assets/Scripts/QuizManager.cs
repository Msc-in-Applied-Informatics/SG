using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class QuizManager : MonoBehaviour
{
    public enum QuizCategory { Paper, Plastic, Glass, Aluminum, Organic, Electronics, Battery };

    [Header("UI References")]
    public GameObject quizPanel;
    public Text questionText;
    public Button[] answerButtons;
    public Text feedbackText;

    [Header("Answer Colors")]
    public Color normalColor = Color.white;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public Color highlightCorrectColor = new Color(1f, 0.5f, 0f);
    public Color selectedOutlineColor = Color.yellow;

    private List<QuizQuestion> questions = new List<QuizQuestion>();
    private QuizQuestion currentQuestion;
    private bool answerSelected = false;

    [System.Serializable]
    public class QuizQuestion
    {
        public string question;
        public string[] answers;
        public int correctAnswerIndex;
        public QuizCategory category;
    }

    [System.Serializable]
    public class QuizQuestionJSON
    {
        public string question;
        public string[] answers;
        public int correctAnswerIndex;
        public string category;
    }

    // ==================== INIT ====================

    void Start()
    {
        LoadQuestionsFromJSON();
    }

    public void LoadQuestionsFromJSON()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "quiz_questions.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            QuizQuestionJSON[] rawQuestions = JsonHelper.FromJsonArray<QuizQuestionJSON>(json);

            foreach (var raw in rawQuestions)
            {
                QuizQuestion q = new QuizQuestion
                {
                    question = raw.question,
                    answers = raw.answers,
                    correctAnswerIndex = raw.correctAnswerIndex,
                    category = (QuizCategory)System.Enum.Parse(typeof(QuizCategory), raw.category)
                };

                questions.Add(q);
            }
        }
        else
        {
            Debug.LogError("Quiz JSON not found at path: " + path);
        }
    }

    // ==================== SHOW QUIZ ====================

    public void ShowQuestionForCurrentLevel()
    {
        if (GameManager.Instance == null) return;

        List<QuizQuestion> matching;

        // Μέχρι το Level 6: βάσει του τύπου σκουπιδιού
        if (GameManager.Instance.level <= 6)
        {
            TrashItem.TrashType currentType = GameManager.Instance.currentLevelSettings.correctTrashType;
            string categoryName = currentType.ToString();

            if (System.Enum.TryParse(categoryName, out QuizCategory parsedCategory))
            {
                matching = questions.FindAll(q => q.category == parsedCategory);
            }
            else
            {
                Debug.LogWarning($"TrashType '{currentType}' could not be matched to QuizCategory");
                matching = new List<QuizQuestion>();
            }
        }
        // Από το Level 7 και πάνω: οποιοδήποτε διαθέσιμο quiz (random)
        else
        {
            matching = new List<QuizQuestion>(questions);
        }

        //
        if (matching.Count == 0)
        {
            GameManager.Instance.isQuizActive = false;
            quizPanel.SetActive(false);
            GameManager.Instance.ContinueGame();
            return;
        }

        currentQuestion = matching[Random.Range(0, matching.Count)];
        DisplayQuestion(currentQuestion);
    }


    void DisplayQuestion(QuizQuestion q)
    {
        quizPanel.SetActive(true);
        feedbackText.text = "";
        questionText.text = q.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int capturedIndex = i;

            answerButtons[i].GetComponentInChildren<Text>().text = q.answers[i];
            ResetButtonVisuals(answerButtons[i]);

            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(capturedIndex));
        }
    }

    void ResetButtonVisuals(Button button)
    {
        if (button.TryGetComponent(out Image img)) img.color = normalColor;
        if (button.TryGetComponent(out Outline outline)) outline.enabled = false;
    }

    // ==================== ANSWER LOGIC ====================

    void OnAnswerSelected(int index)
    {
        if (answerSelected) return;
        answerSelected = true;

        Button selected = answerButtons[index];
        HighlightSelectedButton(selected);

        bool isCorrect = index == currentQuestion.correctAnswerIndex;
        //feedbackText.text = isCorrect ? "Correct! +1 Life" : "Wrong!";

       
        StartCoroutine(ShowAnswerResult(index, isCorrect));
        StartCoroutine(HideAndContinueAfterDelay(4f, isCorrect));
    }

    void HighlightSelectedButton(Button selected)
    {
        foreach (var btn in answerButtons)
        {
            if (btn.TryGetComponent(out Outline o)) o.enabled = false;
        }

        if (selected.TryGetComponent(out Outline outline))
        {
            outline.effectColor = selectedOutlineColor;
            outline.enabled = true;
        }
    }

    IEnumerator ShowAnswerResult(int selectedIndex, bool isCorrect)
    {
        yield return new WaitForSeconds(2f);

        if (isCorrect)
        {
            SetButtonColor(selectedIndex, correctColor);
        }
        else
        {
            SetButtonColor(selectedIndex, wrongColor);
            SetButtonColor(currentQuestion.correctAnswerIndex, highlightCorrectColor);
        }
        AudioManager.Instance.PlaySound(isCorrect ? AudioManager.Instance.correctAnswerClip : AudioManager.Instance.wrongAnswerClip);
    }

    void SetButtonColor(int index, Color color)
    {
        if (index < 0 || index >= answerButtons.Length) return;
        if (answerButtons[index].TryGetComponent(out Image img)) img.color = color;
    }

    IEnumerator HideAndContinueAfterDelay(float delay, bool isCorrect)
    {
        yield return new WaitForSecondsRealtime(delay);
        HideAndContinue(isCorrect);
    }

    void HideAndContinue(bool isCorrect)
    {
        quizPanel.SetActive(false);

        if (isCorrect)
        {
            questions.Remove(currentQuestion);
            GameManager.Instance.GrantArmor();
        }

        GameManager.Instance.isQuizActive = false;
        GameManager.Instance.PlayPause(false, "");
        GameManager.Instance.ContinueGame();
    }

    // ==================== BUTTON STATE ====================

    public void EnableAllAnswerButtons()
    {
        answerSelected = false;
        foreach (Button btn in answerButtons)
        {
            btn.interactable = true;
        }
    }

    private void DisableAllAnswerButtons()
    {
        foreach (Button btn in answerButtons)
        {
            btn.interactable = false;
        }
    }
}
