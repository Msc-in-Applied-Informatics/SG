using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class QuizManager : MonoBehaviour
{
    public enum QuizCategory { Paper, Plastic, Glass, Aluminum, Organic }

    public GameObject quizPanel;
    public Text questionText;
    public Button[] answerButtons;
    public Text feedbackText;

    [Header("Answer Colors")]
    public Color normalColor = Color.white;
    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    public Color highlightCorrectColor = new Color(1f, 0.5f, 0f); // Orange
    public Color selectedOutlineColor = Color.yellow;

    private List<QuizQuestion> questions = new List<QuizQuestion>();
    private QuizQuestion currentQuestion;

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

    public void ShowQuestionForCurrentLevel()
    {
        if (GameManager.Instance == null) return;

        QuizCategory currentCategory = (QuizCategory)(GameManager.Instance.level - 2);
        List<QuizQuestion> matching = questions.FindAll(q => q.category == currentCategory);

        if (matching.Count == 0)
        {
            GameManager.Instance.isQuizActive = false;  
            GameManager.Instance.ContinueGame();      
            return;
        }

        currentQuestion = matching[Random.Range(0, matching.Count)];
        questions.Remove(currentQuestion);
        DisplayQuestion(currentQuestion);
    }

    void DisplayQuestion(QuizQuestion q)
    {
        quizPanel.SetActive(true);
        feedbackText.text = "";
        questionText.text = q.question;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int captured = i;

            // Set text
            answerButtons[i].GetComponentInChildren<Text>().text = q.answers[i];

            // Reset visuals
            Image img = answerButtons[i].GetComponent<Image>();
            if (img != null) img.color = normalColor; // Όπως έχεις ορίσει

            // Disable outline initially
            Outline outline = answerButtons[i].GetComponent<Outline>();
            if (outline != null) outline.enabled = false;

            // Reset animation
            Animator anim = answerButtons[i].GetComponent<Animator>();
            if (anim != null)
            {
                anim.Rebind();
                anim.Update(0f);
            }

            // Set listener
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(captured));
        }
    }

    private bool answerSelected = false;

    void OnAnswerSelected(int index)
    {
        if (answerSelected) return; // μπλοκάρει επόμενα κλικ

        answerSelected = true;

        Button selected = answerButtons[index];

        //DisableAllAnswerButtons();
        // Παίξε το scale animation
        Animator anim = selected.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Rebind();                  // Reset
            anim.Update(0f);
            anim.ResetTrigger("PlayClick"); // Reset για σιγουριά
            anim.SetTrigger("PlayClick");   // Ενεργοποίηση
        }

        // Απενεργοποίησε όλα τα outlines
        foreach (var btn in answerButtons)
        {
            Outline o = btn.GetComponent<Outline>();
            if (o != null) o.enabled = false;
        }

        // Ενεργοποίησε outline μόνο στο επιλεγμένο κουμπί
        Outline selectedOutline = selected.GetComponent<Outline>();
        if (selectedOutline != null)
        {
            selectedOutline.effectColor = selectedOutlineColor;
            selectedOutline.enabled = true;
        }

        // Έλεγχος απάντησης
        bool isCorrect = index == currentQuestion.correctAnswerIndex;
        feedbackText.text = isCorrect ? "Correct! +1 Life" : "Wrong!";

        // Έγχρωμος τονισμός (καθυστέρηση για animation)
        StartCoroutine(ShowAnswerResult(index, isCorrect));

        if (isCorrect)
        {
            GameManager.Instance.AddLife();
        }

        StartCoroutine(HideAndContinueAfterDelay(8f));
    }

    //Disable click after selected
    private void DisableAllAnswerButtons()
    {
        foreach (Button btn in answerButtons)
        {
            btn.interactable = false;
        }
    }

    //Enable click after selected
    public void EnableAllAnswerButtons()
    {
        answerSelected = false;
        foreach (Button btn in answerButtons)
        {
            btn.interactable = true;
        }
    }



    void HideAndContinue()
    {
        quizPanel.SetActive(false);
        GameManager.Instance.isQuizActive = false;
        GameManager.Instance.PlayPause();
        GameManager.Instance.ContinueGame();
    }

    IEnumerator ShowAnswerResult(int selectedIndex, bool isCorrect)
    {
        yield return new WaitForSeconds(2f); // περιμένουμε να παιχτεί πρώτα το animation

        if (isCorrect)
        {
            Image img = answerButtons[selectedIndex].GetComponent<Image>();
            if (img != null) img.color = correctColor;
        }
        else
        {
            Image selectedImg = answerButtons[selectedIndex].GetComponent<Image>();
            if (selectedImg != null) selectedImg.color = wrongColor;

            Image correctImg = answerButtons[currentQuestion.correctAnswerIndex].GetComponent<Image>();
            if (correctImg != null) correctImg.color = highlightCorrectColor;
        }
    }

    IEnumerator HideAndContinueAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(4f);
        HideAndContinue();
    }

}
