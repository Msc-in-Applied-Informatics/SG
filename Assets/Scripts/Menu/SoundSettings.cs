using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{

    public Slider soundSlider;
    public Image soundIcon;

    public Sprite muteIcon;
    public Sprite lowIcon;
    public Sprite midIcon;
    public Sprite highIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soundSlider.onValueChanged.AddListener(UpdateSound);
        soundSlider.value = AudioListener.volume;
        UpdateSound(soundSlider.value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateSound(float value)
    {
        AudioListener.volume = value;

        if (value == 0)
        {
            soundIcon.sprite = muteIcon;
        }
        else if (value <= 0.3f)
        {
            soundIcon.sprite = lowIcon;
        }
        else if (value <= 0.7f)
        {
            soundIcon.sprite = midIcon;
        }
        else
        {
            soundIcon.sprite = highIcon;
        }
    }
}
