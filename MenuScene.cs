using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScene : MonoBehaviour
{
    // instance variables
    public Canvas nameCanvas;
    public Canvas creditsCanvas;
    public Button menuToName;
    public Button menuToHighScores;
    public Button menuToInstructions;
    public Button quitButton;
    public Button menuToCredits;
    public Button nameCanvasToMenu;
    public Button playGame;
    public Button creditsCanvasToMenu;
    public InputField input;

    // instructions canvas
    public Canvas instructionsCanvas;
    public Button instructionsBackButton;
    public Button instructionsNextButton;
    public Text instructionsText;
    private int instructionsPageNumber;

    // music
    public Sprite[] soundSprites;
    public AudioSource music;
    public Button[] soundButtons;

    // Start is called before the first frame update
    private void Start()
    {
        HideNameCanvas();
        HideCreditsCanvas();

        // button handlers

        // main screen
        menuToName.onClick.AddListener(ShowNameCanvas);
        menuToHighScores.onClick.AddListener(GameControl.DisplayHighScores);
        menuToInstructions.onClick.AddListener(Instructions);
        quitButton.onClick.AddListener(GameControl.Quit);
        menuToCredits.onClick.AddListener(ShowCreditsCanvas);

        // entering name screen
        nameCanvasToMenu.onClick.AddListener(HideNameCanvas);
        creditsCanvasToMenu.onClick.AddListener(HideCreditsCanvas);
        playGame.onClick.AddListener(NewGame);
        input.onValueChanged.AddListener(UpdatePlayButtonStatus);

        // credits screen
        creditsCanvasToMenu.onClick.AddListener(HideCreditsCanvas);

        foreach (Button button in soundButtons)
        {
            button.onClick.AddListener(ToggleMusic);
        }

        GameControl.DisableButton(playGame);
    }

    // updates whether or not player can start game based on
    // whether or not input field contains a name
    // string text: text in input field
    private void UpdatePlayButtonStatus(string text)
    {
        if (text.Length > 0) GameControl.EnableButton(playGame);
        else GameControl.DisableButton(playGame);
    }

    // when user presses "Play Game" button
    private void NewGame() { GameControl.NewGame(input.text); }

    // asks for name
    private void ShowNameCanvas() { GameControl.Show(nameCanvas.gameObject); }

    // back to menu
    private void HideNameCanvas() { GameControl.Hide(nameCanvas.gameObject); }

    // shows credits
    private void ShowCreditsCanvas() { GameControl.Show(creditsCanvas.gameObject); }

    // back to menu
    private void HideCreditsCanvas() { GameControl.Hide(creditsCanvas.gameObject); }

    // when user clicks sound icon
    private void ToggleMusic() { GameControl.ToggleMusic(music, soundButtons, soundSprites); }

    // when user clicks "Instructions" button
    private void Instructions()
    {
        GameControl.Show(instructionsCanvas.gameObject);
        instructionsPageNumber = 0;
        ChangeInstructionsPage();
    }

    // sets up a new page of the instructions with
    // the appropriate button handlers
    private void ChangeInstructionsPage()
    {
        List<string> instructions = GameControl.GetInstructions();
        instructionsBackButton.onClick.RemoveAllListeners();
        instructionsNextButton.onClick.RemoveAllListeners();

        // first page: back leads back to menu
        if (instructionsPageNumber == 0) instructionsBackButton.onClick.AddListener(HideInstructionsCanvas);
        else instructionsBackButton.onClick.AddListener(ShowPreviousInstructionsPage);

        // last page: next leads back to menu
        if (instructionsPageNumber == instructions.Count - 1)
        {
            instructionsNextButton.GetComponentInChildren<Text>().text = "Done";
            instructionsNextButton.onClick.AddListener(HideInstructionsCanvas);
        }
        else
        {
            instructionsNextButton.GetComponentInChildren<Text>().text = "Next";
            instructionsNextButton.onClick.AddListener(ShowNextInstructionsPage);
        }

        instructionsText.text = instructions[instructionsPageNumber];
    }

    // shows previous instructions page
    private void ShowPreviousInstructionsPage()
    {
        instructionsPageNumber--;
        ChangeInstructionsPage();
    }

    // shows next instructions page
    private void ShowNextInstructionsPage()
    {
        instructionsPageNumber++;
        ChangeInstructionsPage();
    }

    // goes from instructions canvas back to main menu
    private void HideInstructionsCanvas() { GameControl.Hide(instructionsCanvas.gameObject); }

}