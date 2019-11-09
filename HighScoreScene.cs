using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighScoreScene : MonoBehaviour
{
    public Button menuButton;
    public Button clearButton;
    public Text namesLabel;
    public Text scoresLabel;

    // music
    public Sprite[] soundSprites;
    public AudioSource music;
    public Button[] soundButtons;

    // Start is called before the first frame update
    private void Start()
    {
        // button handlers
        menuButton.onClick.AddListener(GameControl.DisplayMenu);
        clearButton.onClick.AddListener(ClearHighScores);

        foreach (Button button in soundButtons)
        {
            button.onClick.AddListener(ToggleMusic);
        }

        UpdateHighScores();
    }

    // resets high scores
    private void ClearHighScores()
    {
        GameControl.GetHighScore().ResetHighScores();
        UpdateHighScores(); // change on screen
    }

    // displays current high scores to screen
    private void UpdateHighScores()
    {
        List<string> names = GameControl.GetHighScore().GetHighScoreNames();
        List<int> scores = GameControl.GetHighScore().GetHighScores();

        // convert to strings and update on screen
        string namesStr = "";
        foreach (string name in names)
        {
            namesStr += name + "\n";
        }
        namesLabel.text = namesStr;

        string scoresStr = "";
        foreach (int score in scores)
        {
            scoresStr += score + "\n";
        }
        scoresLabel.text = scoresStr;
    }

    // when user clicks on sound icon
    private void ToggleMusic()
    {
        GameControl.ToggleMusic(music, soundButtons, soundSprites);
    }
}
