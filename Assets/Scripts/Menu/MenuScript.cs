using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Reset Saved data
        //SaveSystem.ClearAllData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(SceneEnum.GamePlay.ToString());
    }

    public void OpenLevel()
    {
        SceneManager.LoadSceneAsync(SceneEnum.LevelSelect.ToString());
    }

    public void OpenMenu()
    {
        SceneManager.LoadSceneAsync(SceneEnum.MainMenu.ToString());
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenStory()
    {
        Debug.Log("Open Story Panel");
    }
}
