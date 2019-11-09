using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScene : MonoBehaviour
{
    // constants
    private readonly Color DEFAULT_TRIVIA_COLOR = new Color32(255, 255, 255, 255);
    private readonly Color INCORRECT_TRIVIA_COLOR = new Color32(227, 142, 142, 255);
    private readonly Color CORRECT_TRIVIA_COLOR = new Color32(142, 227, 142, 255);
    private readonly Color CURRENT_ROOM_COLOR = new Color32(0, 225, 255, 255);
    private readonly Color NOT_CURRENT_ROOM_COLOR = new Color32(255, 255, 255, 255);

    private readonly float LONG_ANIMATION_DURATION = 3.5f; // seconds
    private readonly float MEDIUM_ANIMATION_DURATION = 3.0f;
    private readonly float DOUBLE_ANIMATION_DURATION = 5.0f;
    private readonly float LONG_ANIMATION_INTERVAL = 0.1f;
    private readonly float MEDIUM_ANIMATION_INTERVAL = 0.05f;
    private readonly float SHORT_ANIMATION_INTERVAL = 0.025f;
    private readonly float ONE_SECOND = 1;
    private readonly float ALPHA_INTERVAL = 0.05f;
    private readonly float[] ARROW_TRANSLATION_CONSTANTS = { 0.1f, 0.2f, -25 };
    private readonly float[] ARROW_INITIAL_VELOCITY = { 7, 15 };
    private readonly Vector3 SCREEN_SCALE = new Vector3(1, 1.15f, 1);

    private readonly string PIT_TRIVIA_STR = "pit";
    private readonly string WUMPUS_TRIVIA_STR = "wumpus";
    private readonly string ARROW_TRIVIA_STR = "arrow";
    private readonly string SECRET_TRIVIA_STR = "secret";

    // cave components
    public GameObject floor;
    public GameObject ceiling;
    public GameObject[] walls;
    public GameObject capsule;
    public GameObject cam;
    public GameObject arrow;
    public Material wallNoTunnelMaterial;
    public Material wallTunnelMaterial;
    public Material floorMaterial;
    public Material darkMaterial;
    public Material lightMaterial;

    // main canvas
    public Button pauseButton;
    public Button mapButton;
    public Button buyArrowsButton;
    public Button buySecretButton;
    public Button shootArrowButton;
    public Image compass;
    public Image fadeImage;

    // music
    public Sprite[] soundSprites;
    public AudioSource music;
    public Button[] soundButtons;

    // information canvas
    public Canvas informationCanvas;
    public Text roomLabel;
    public Text coinsLabel;
    public Text arrowsLabel;
    public Text timeLabel;
    public Text[] informationElements;

    // story canvas
    public Canvas storyCanvas;
    public Text storyText;
    public Button toGameButton;

    // instructions canvas
    public Canvas instructionsCanvas;
    public Button instructionsBackButton;
    public Button instructionsNextButton;
    public Text instructionsText;
    private int instructionsPageNumber;

    // pause canvas
    public Canvas pauseCanvas;
    public Image pausePanel;
    public Button resumeButton;
    public Button menuButton;
    public Button instructionsButton;

    // map canvas
    public Canvas mapCanvas;
    public Button backButton;
    public GameObject roomNumberLabels;

    // trivia canvas
    public Canvas triviaCanvas;
    public Image triviaPanel;
    public Button[] triviaButtons;
    public Button nextTrivia;
    public Text questionTitleLabel;
    public Text questionLabel;
    public Text correctLabel;
    private int currentTriviaQuestion;
    private int totalTriviaQuestions;
    private int correctTriviaQuestions;

    // message canvas
    public Canvas messageCanvas;
    public Button continueButton;
    public Button endScreenMenuButton;
    public Button endScreenScoresButton;
    public Button confirmQuitButton;
    public Button cancelQuitButton;
    public Text messageLabel;
    public Image continuePanel;
    public Image endGamePanel;
    public Image quitGamePanel;
    public Image[] bats;
    public Image wumpus;
    public Image trebek;

    // other
    private List<string> messages;
    private int currentMessageNumber;
    private string reasonForTrivia;
    private int secondsRemaining;
    private bool paused;
    private bool gameOver;

    // Start is called before the first frame update
    private void Start()
    {
        messages = new List<string>();
        currentMessageNumber = -1;
        reasonForTrivia = "";
        secondsRemaining = GameControl.TIME_LIMIT;
        paused = false;
        gameOver = false;
        StartCoroutine(ManageTime());

        // button handlers

        toGameButton.onClick.AddListener(StartGame);
        foreach (Button button in soundButtons) button.onClick.AddListener(ToggleMusic);
        foreach (Button button in triviaButtons) button.onClick.AddListener(() => VerifyTrivia(button.GetComponentInChildren<Text>().text));

        // main canvas
        pauseButton.onClick.AddListener(ShowPauseCanvas);
        mapButton.onClick.AddListener(ShowMapCanvas);
        buyArrowsButton.onClick.AddListener(BuyArrows);
        shootArrowButton.onClick.AddListener(ShowArrow);
        buySecretButton.onClick.AddListener(BuySecret);

        // pause canvas
        resumeButton.onClick.AddListener(HidePauseCanvas);
        menuButton.onClick.AddListener(QuitGame);
        instructionsButton.onClick.AddListener(Instructions);
        backButton.onClick.AddListener(HideMapCanvas);

        // message canvas
        endScreenMenuButton.onClick.AddListener(GameControl.DisplayMenu);
        endScreenScoresButton.onClick.AddListener(GameControl.DisplayHighScores);
        confirmQuitButton.onClick.AddListener(GameControl.DisplayMenu);
        cancelQuitButton.onClick.AddListener(HideMessageCanvas);

        // hide all canvases except main/information ones
        HidePauseCanvas();
        HideMapCanvas();
        HideTriviaCanvas();
        HideMessageCanvas();
        HideCanvas(storyCanvas);
        HideCanvas(instructionsCanvas);
        GameControl.Hide(arrow);

        cam.GetComponent<CameraController>().SetCompass(compass);
        capsule.GetComponent<CharacterController>().SetGameScene(this);

        StartCoroutine(ShowStory());
    }

    // called every frame
    private void Update()
    {
        if (!paused) // if game in session, wait for keyboard shortcuts
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                if (!mapCanvas.gameObject.activeSelf) ShowMapCanvas(); // toggle map
                else HideMapCanvas();
            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) BuyArrows();
            if (Input.GetKeyDown(KeyCode.Alpha2)) ShowArrow();
            if (Input.GetKeyDown(KeyCode.Alpha3)) BuySecret();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!pauseCanvas.gameObject.activeSelf) ShowPauseCanvas(); // toggle pause canvas
            else HidePauseCanvas();
        }

        if (pauseCanvas.gameObject.activeSelf || gameOver) paused = true; // game not in session
        else paused = false; // game in session
    }

    // when user presses sound button
    private void ToggleMusic() { GameControl.ToggleMusic(music, soundButtons, soundSprites); }

    public void StartGame()
    {
        StopCoroutine(ShowStory());
        HideCanvas(storyCanvas);
        NewRoom(GameControl.STARTING_ROOM);
    }

    // when user enters new room
    public void NewRoom(int room)
    {
        GameControl.NewRoom(room);

        // update information on screen
        GameControl.ChangeLabel(room + 1, roomLabel);
        GameControl.ChangeLabel(GameControl.GetPlayer().GetNumberOfGoldCoins(), coinsLabel);
        GameControl.ChangeLabel(GameControl.GetPlayer().GetNumberOfArrows(), arrowsLabel);

        SetTunnels();
        CheckForMessages();
    }

    // makes tunnels between current room and adjacent rooms
    private void SetTunnels()
    {
        int currentRoom = GameControl.GetGameLocations().GetPlayerLocation();
        int[] allAdjacentRooms = GameControl.GetGameLocations().GetCave().GetAllNeighbors(currentRoom);
        List<int> adjacentRooms = GameControl.GetGameLocations().GetCave().GetAdjacentRooms(currentRoom);

        for (int i = 0; i < walls.Length; i++)
        {
            // allow user to walk through wall if room is connected, otherwise make wall a collider
            walls[i].GetComponent<MeshCollider>().enabled = adjacentRooms.Contains(allAdjacentRooms[i % GameControl.NUM_NEIGHBORS_PER_ROOM]);
            walls[i].GetComponent<MeshCollider>().convex = adjacentRooms.Contains(allAdjacentRooms[i % GameControl.NUM_NEIGHBORS_PER_ROOM]);
            walls[i].GetComponent<MeshCollider>().isTrigger = adjacentRooms.Contains(allAdjacentRooms[i % GameControl.NUM_NEIGHBORS_PER_ROOM]);

            // change color of wall based on whether or not it's a tunnel
            if (adjacentRooms.Contains(allAdjacentRooms[i % GameControl.NUM_NEIGHBORS_PER_ROOM])) walls[i].GetComponent<MeshRenderer>().material = wallTunnelMaterial;
            else walls[i].GetComponent<MeshRenderer>().material = wallNoTunnelMaterial;
        }
    }

    // sets which messages user must receive when they enter room
    private void CheckForMessages()
    {
        messages = GameControl.GetGameLocations().GetMessages();

        if (messages.Count > 0)
        {
            currentMessageNumber = 1;
            ShowMessages(); // start showing messages
        }
        else CheckForObstacles(); // go straight to checking for hazards/Wumpus
    }

    // shows user messages (if they found treasure chest or need a warning)
    private void ShowMessages()
    {
        ShowMessageCanvas(messages[currentMessageNumber - 1]);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(HideMessageCanvas);

        if (currentMessageNumber < messages.Count) // not last message
        {
            currentMessageNumber++;
            continueButton.onClick.AddListener(ShowMessages); // continue button leads to next message
        }
        else continueButton.onClick.AddListener(CheckForObstacles); // last message, check for obstacles when continue pressed
    }

    // checks for hazards/Wumpus in current room
    private void CheckForObstacles()
    {
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(HideMessageCanvas);

        if (GameControl.GetGameLocations().BatsInRoom())
        {
            continueButton.onClick.AddListener(HandleBats);
            ShowMessageCanvas(GameControl.BATS_MESSAGE, true);
            ShowSuperBats();
        } else if (GameControl.GetGameLocations().PitInRoom())
        {
            reasonForTrivia = PIT_TRIVIA_STR; // saves reason for user playing trivia for later
            continueButton.onClick.AddListener(DisplayTrivia);
            ShowMessageCanvas(GameControl.PIT_MESSAGE, true);
            StartCoroutine(FallIntoPit()); // starts animation for pit
        } else if (GameControl.GetGameLocations().WumpusInRoom())
        {
            reasonForTrivia = WUMPUS_TRIVIA_STR;
            continueButton.onClick.AddListener(DisplayTrivia);
            ShowMessageCanvas(GameControl.WUMPUS_MESSAGE, true);
            StartCoroutine(FadeIn(wumpus)); // starts animation for Wumpus
        }
    }

    // starts animation for bats
    private void ShowSuperBats()
    {
        foreach (Image bat in bats) { StartCoroutine(FadeIn(bat)); }
        StartCoroutine(RotateScreen());
    }

    // when user presses continue, ends animation for bats
    // and moves player/bat
    private void HandleBats()
    {
        StartCoroutine(FadeInAndOut(fadeImage));
        StartCoroutine(RotateScreen());
        GameControl.GetGameLocations().MoveBat();
        NewRoom(GameControl.GetRandomRoom());
    }

    // handles cases when user finishes playing trivia
    private void FinishedTrivia()
    {
        HideTriviaCanvas();
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(HideMessageCanvas);

        // gives user a coin back for every extra question answered (over minimum of 2 out of 3 or 3 out of 5)
        int addedCoins = GameControl.GetTrivia().ExtraTriviaQuestionsAnswered(totalTriviaQuestions, correctTriviaQuestions);
        GameControl.GetPlayer().AddGoldCoins(addedCoins);
        GameControl.ChangeLabel(GameControl.GetPlayer().GetNumberOfGoldCoins(), coinsLabel);

        if (GameControl.GetTrivia().PassedTrivia(totalTriviaQuestions, correctTriviaQuestions)) // passed trivia
        {
            if (reasonForTrivia == PIT_TRIVIA_STR) // brings user back from pit, congratulates user, moves player to starting room
            {
                StartCoroutine(RiseFromPit());
                ShowMessageCanvas(GameControl.ESCAPED_PIT_MESSAGE, true);
                NewRoom(GameControl.STARTING_ROOM);
            }
            else if (reasonForTrivia == WUMPUS_TRIVIA_STR) // congratulates user, moves Wumpus
            {
                ShowMessageCanvas(GameControl.ESCAPED_WUMPUS_MESSAGE);
                GameControl.GetGameLocations().DefeatedInTrivia();
            }
            else if (reasonForTrivia == ARROW_TRIVIA_STR) // congratulates user, adds arrows to inventory
            {
                ShowMessageCanvas(GameControl.BOUGHT_ARROWS_MESSAGE);
                GameControl.GetPlayer().AddArrows(2);
                GameControl.ChangeLabel(GameControl.GetPlayer().GetNumberOfArrows(), arrowsLabel);
            }
            else // tells user secret
            {
                ShowMessageCanvas(GameControl.GetGameLocations().GetSecret());
            }
        }
        else // failed trivia
        {
            if (reasonForTrivia == PIT_TRIVIA_STR) // user lost game
            {
                EndGame(GameControl.END_GAME_MESSAGES[0], false);
            }
            else if (reasonForTrivia == WUMPUS_TRIVIA_STR) // user lost game
            {
                EndGame(GameControl.END_GAME_MESSAGES[1], false);
            }
            else if (reasonForTrivia == ARROW_TRIVIA_STR) // user not able to buy arrows
            {
                ShowMessageCanvas(GameControl.ARROWS_FAILED_TRIVIA_MESSAGE);
            }
            else // user not able to buy secret
            {
                ShowMessageCanvas(GameControl.SECRET_FAILED_TRIVIA_MESSAGE);
            }
        }

    }

    // when user presses "Buy Arrows" button
    private void BuyArrows()
    {
        if (GameControl.GetPlayer().GetNumberOfGoldCoins() > GameControl.NUM_TRIVIA_QUESTIONS) // can afford to play trivia
        {
            reasonForTrivia = ARROW_TRIVIA_STR;
            DisplayTrivia();
        }
        else ShowMessageCanvas(GameControl.LACKS_COINS_ARROWS_MESSAGE);
    }

    // when user presses "Buy Secret" button
    private void BuySecret()
    {
        if (GameControl.GetPlayer().GetNumberOfGoldCoins() > GameControl.NUM_TRIVIA_QUESTIONS) // can afford to play trivia
        {
            reasonForTrivia = SECRET_TRIVIA_STR;
            DisplayTrivia();
        }
        else ShowMessageCanvas(GameControl.LACKS_COINS_TRIVIA_MESSAGE);
    }

    // when user presses "Shoot Arrow" button
    private void ShowArrow()
    {
        capsule.GetComponent<CharacterController>().SetGameInSession(false); // lock arrow keys

        // set up projectile motion for arrow
        arrow.GetComponent<Rigidbody>().useGravity = false;
        arrow.transform.position = capsule.transform.position + cam.transform.right * ARROW_TRANSLATION_CONSTANTS[0]
                                                                 + cam.transform.up * ARROW_TRANSLATION_CONSTANTS[1];
        arrow.transform.eulerAngles = new Vector3(capsule.transform.eulerAngles.x + ARROW_TRANSLATION_CONSTANTS[2],
                                                 capsule.transform.eulerAngles.y, capsule.transform.eulerAngles.z);
        arrow.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        GameControl.Show(arrow);
        GameControl.Show(pauseCanvas.gameObject);
        GameControl.Hide(pausePanel.gameObject);
        StartCoroutine(ShootArrow());
    }

    // user finished shooting arrow
    // int room: room number arrow was shot into, -1 if not shot into adjacent room
    public void ArrowShot(int room)
    {
        pausePanel.gameObject.SetActive(true);
        capsule.GetComponent<CharacterController>().SetGameInSession(true);

        if (room == GameControl.GetGameLocations().GetWumpusLocation()) EndGame(GameControl.END_GAME_MESSAGES[5], true); // correct room, user wins
        else if (room == -1) // not a valid room
        {
            ShowMessageCanvas(GameControl.ARROW_MISSED_MESSAGE);
            GameControl.GetGameLocations().ShotAttempted();
        }
        else // wrong room
        {
            ShowMessageCanvas(GameControl.ARROW_WRONG_ROOM_MESSAGE);
            GameControl.GetGameLocations().ShotAttempted();
        }

        // update arrows
        GameControl.GetPlayer().SubArrows(1);
        GameControl.ChangeLabel(GameControl.GetPlayer().GetNumberOfArrows(), arrowsLabel);
        if (GameControl.GetPlayer().GetNumberOfArrows() == 0) EndGame(GameControl.END_GAME_MESSAGES[2], false); // user loses, ran out of arrows
    }

    // user either lost or won
    // string message: message to be shown to user explaining loss or victory
    // bool defeatedWumpus: true if user won, false otherwise
    private void EndGame(string message, bool defeatedWumpus)
    {
        gameOver = true;

        // update high scores
        GameControl.GetHighScore().UpdateHighScores(GameControl.GetPlayer().GetScore(defeatedWumpus, secondsRemaining), GameControl.GetCurrentPlayerName());
        GameControl.GetHighScore().WriteHighScoreTextFile();

        ShowMessageCanvas(message + " Your final score was " + GameControl.GetPlayer().GetScore(defeatedWumpus, secondsRemaining) + ".", false, true);

        if (defeatedWumpus) // user won
        {
            GameControl.Show(trebek.gameObject);
            foreach (GameObject wall in walls)
            {
                wall.GetComponent<MeshRenderer>().material = lightMaterial;
            }
            floor.GetComponent<MeshRenderer>().material = lightMaterial;
            ceiling.GetComponent<MeshRenderer>().material = lightMaterial;
        }
    }

    // user pressed "Menu" button in middle of game, confirms whether or not they want to quit
    private void QuitGame() { ShowMessageCanvas(GameControl.QUIT_GAME_MESSAGE, false, false, true); }

    // when user wants to view instructions
    private void Instructions()
    {
        ShowCanvas(instructionsCanvas);
        instructionsPageNumber = 0;
        ChangeInstructionsPage();
    }

    // changes instruction page showing on screen and sets appropriate button handlers
    private void ChangeInstructionsPage()
    {
        List<string> instructions = GameControl.GetInstructions();
        instructionsBackButton.onClick.RemoveAllListeners();
        instructionsNextButton.onClick.RemoveAllListeners();

        if (instructionsPageNumber == 0) instructionsBackButton.onClick.AddListener(HideInstructionsCanvas);
        else instructionsBackButton.onClick.AddListener(ShowPreviousInstructionsPage);

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

    // goes to previous instructions page
    private void ShowPreviousInstructionsPage()
    {
        instructionsPageNumber--;
        ChangeInstructionsPage();
    }

    // goes to next instructions page
    private void ShowNextInstructionsPage()
    {
        instructionsPageNumber++;
        ChangeInstructionsPage();
    }

    // removes instructions from screen
    private void HideInstructionsCanvas() { HideCanvas(instructionsCanvas); }

    // when user presses "Pause" button or hits "P" on keyboard
    private void ShowPauseCanvas() { ShowCanvas(pauseCanvas); }

    // when user presses "Resume" button or hits "P" on keyboard
    private void HidePauseCanvas() { HideCanvas(pauseCanvas); }

    // when user presses "Map" button or hits "M" on keyboard
    private void ShowMapCanvas()
    {
        foreach (Text roomNumLabel in roomNumberLabels.GetComponentsInChildren<Text>())
        {
            Debug.Log(roomNumLabel.text);
            if (roomNumLabel.text == roomLabel.text) roomNumLabel.color = CURRENT_ROOM_COLOR;
            else roomNumLabel.color = NOT_CURRENT_ROOM_COLOR;
        }

        ShowCanvas(mapCanvas);
    }

    // when user presses "Back" button or hits "M" on keyboard
    private void HideMapCanvas() { HideCanvas(mapCanvas); }

    // shows message to user (warnings, updates, etc.)
    private void ShowMessageCanvas(string message, bool waitToContinue=false, bool showEndScreen=false, bool playerQuittingGame=false)
    {
        foreach (Image bat in bats) GameControl.Hide(bat.gameObject);
        GameControl.Hide(wumpus.gameObject);
        GameControl.Hide(trebek.gameObject);
        GameControl.Hide(continuePanel.gameObject);
        GameControl.Hide(endGamePanel.gameObject);
        GameControl.Hide(quitGamePanel.gameObject);
        messageLabel.text = message;
        ShowCanvas(messageCanvas);

        if (waitToContinue) StartCoroutine(ShowContinuePanel()); // animation before "Continue" button shows
        else if (showEndScreen) endGamePanel.gameObject.SetActive(true); // end game
        else if (playerQuittingGame) quitGamePanel.gameObject.SetActive(true); // user trying to quit game
        else continuePanel.gameObject.SetActive(true); // regular message
    }

    // removes message from screen
    private void HideMessageCanvas() { HideCanvas(messageCanvas); }

    // shows trivia question
    private void ShowTriviaCanvas(string question, string choicesStr, string answer, int questionNumber, int totalNumQuestions, int numCorrect)
    {   // update instance variables
        currentTriviaQuestion = questionNumber;
        totalTriviaQuestions = totalNumQuestions;
        correctTriviaQuestions = numCorrect;

        GameControl.Hide(nextTrivia.gameObject); // set up "Continue" button on screen
        nextTrivia.onClick.RemoveAllListeners();
        if (currentTriviaQuestion < totalTriviaQuestions) nextTrivia.onClick.AddListener(NextTrivia);
        else nextTrivia.onClick.AddListener(FinishedTrivia);

        foreach (Button button in triviaButtons) // reset answer choice buttons
        {
            button.enabled = true;
            button.tag = "Untagged";
            button.GetComponent<Image>().color = DEFAULT_TRIVIA_COLOR;
        }

        // update labels
        questionTitleLabel.text = "Question " + questionNumber + " of " + totalNumQuestions;
        questionLabel.text = question;
        correctLabel.text = numCorrect + " Correct";

        string[] choices = new string[GameControl.NUM_TRIVIA_CHOICES];
        string[] words = choicesStr.Split(' ');

        int curChoice = -1; // split up answer choices
        foreach (string word in words) {
            if (word.EndsWith("A.") || word.EndsWith("B.") || word.EndsWith("C.") || word.EndsWith("D.")) curChoice++;
            choices[curChoice] += word + " ";
        }

        for (int i = 0; i < GameControl.NUM_TRIVIA_CHOICES; i++)
        {
            triviaButtons[i].GetComponentInChildren<Text>().text = choices[i];
            if (choices[i].StartsWith(answer)) triviaButtons[i].tag = "Answer"; // mark which one is correct
        }

        foreach (Text element in informationElements) // move room number, coins, time, etc. to trivia canvas
        {
            element.transform.parent.transform.SetParent(triviaPanel.transform);
            element.transform.parent.GetComponent<RectTransform>().localScale = SCREEN_SCALE;
        }

        ShowCanvas(triviaCanvas);
    }

    // remove trivia from screen
    private void HideTriviaCanvas()
    {
        foreach (Text element in informationElements) // move room number, coins, time, etc. back to main screen
        {
            element.transform.parent.transform.SetParent(informationCanvas.transform);
            element.transform.parent.GetComponent<RectTransform>().localScale = SCREEN_SCALE;
        }

        HideCanvas(triviaCanvas);
    }

    // start trivia
    private void DisplayTrivia()
    {
        int numTriviaQuestions = GameControl.NUM_TRIVIA_QUESTIONS;
        if (reasonForTrivia.Equals(WUMPUS_TRIVIA_STR)) numTriviaQuestions = GameControl.WUMPUS_NUM_TRIVIA_QUESTIONS;
        GetTrivia(1, numTriviaQuestions, 0);
    }

    // get trivia question, choices, answer
    private void GetTrivia(int questionNumber, int totalNumQuestions, int numCorrect)
    {
        int qNumber = GameControl.GetTrivia().GetRandom();
        string question = GameControl.GetTrivia().GetQuestion(qNumber);
        string choicesStr = GameControl.GetTrivia().GetChoices(qNumber);
        string answer = GameControl.GetTrivia().GetAnswer(qNumber);

        ShowTriviaCanvas(question, choicesStr, answer, questionNumber, totalNumQuestions, numCorrect);
    }

    // verify user's answer to trivia question
    private void VerifyTrivia(string choice)
    {
        GameControl.Show(nextTrivia.gameObject); // show "Continue" button
        foreach (Button button in triviaButtons)
        {
            button.enabled = false;
            if (button.GetComponentInChildren<Text>().text.Equals(choice) && button.CompareTag("Answer"))
            {   // got question right
                button.GetComponent<Image>().color = CORRECT_TRIVIA_COLOR;
                correctTriviaQuestions++;
                correctLabel.text = correctTriviaQuestions + " Correct";
            }
            else if (button.GetComponentInChildren<Text>().text.Equals(choice))
            {   // mark wrong answer
                button.GetComponent<Image>().color = INCORRECT_TRIVIA_COLOR;
            }
            else if (button.CompareTag("Answer"))
            {   // mark right answer
                button.GetComponent<Image>().color = CORRECT_TRIVIA_COLOR;
            }
        }

        GameControl.GetPlayer().SubGoldCoins(1); // decrement number of coins
        GameControl.ChangeLabel(GameControl.GetPlayer().GetNumberOfGoldCoins(), coinsLabel);
        if (GameControl.GetPlayer().GetNumberOfGoldCoins() == 0) // out of coins
        {
            EndGame(GameControl.END_GAME_MESSAGES[3], false); // game over
        }

        currentTriviaQuestion++;
    }

    // start next trivia question
    private void NextTrivia() { GetTrivia(currentTriviaQuestion, totalTriviaQuestions, correctTriviaQuestions); }

    // shows canvas and freezes player controls
    private void ShowCanvas(Canvas canvas)
    {
        GameControl.Show(canvas.gameObject);
        GameControl.FreezePlayerControls(capsule, cam);
    }

    // hides canvas and unfreezes player controls
    private void HideCanvas(Canvas canvas)
    {
        GameControl.Hide(canvas.gameObject);
        GameControl.UnfreezePlayerControls(capsule, cam);
    }

    // animations

    // storyline description at start of game
    private IEnumerator ShowStory()
    {
        ShowCanvas(storyCanvas);
        List<string> story = GameControl.GetStory();
        for (int i = 0; i < story.Count; i++) // fade pieces of stoyline in and out
        {
            string line = story[i];
            GameControl.SetAlpha(storyText, 0);
            storyText.text = line;
            while (storyText.color.a < 1)
            {
                GameControl.SetAlpha(storyText, storyText.color.a + ALPHA_INTERVAL);
                yield return new WaitForSeconds(SHORT_ANIMATION_INTERVAL);
            }
            yield return new WaitForSeconds(MEDIUM_ANIMATION_DURATION*3);
            if (i != story.Count-1)
            {
                while (storyText.color.a > 0)
                {
                    GameControl.SetAlpha(storyText, storyText.color.a - ALPHA_INTERVAL);
                    yield return new WaitForSeconds(SHORT_ANIMATION_INTERVAL);
                }
            }
        }
        toGameButton.GetComponentInChildren<Text>().text = "Play";
        while (true) // keep showing last storyline page until player presses Skip/Play button
        {
            yield return new WaitForSeconds(MEDIUM_ANIMATION_DURATION);
        }
    }

    // show continue button after animation completed
    private IEnumerator ShowContinuePanel()
    {
        yield return new WaitForSeconds(MEDIUM_ANIMATION_DURATION);
        continuePanel.gameObject.SetActive(true);
    }

    // fades in image
    private IEnumerator FadeIn(Image image)
    {
        GameControl.Show(image.gameObject);
        GameControl.SetAlpha(image, 0);
        while (image.color.a < 1)
        {
            GameControl.SetAlpha(image, image.color.a + ALPHA_INTERVAL);
            yield return new WaitForSeconds(LONG_ANIMATION_INTERVAL);
        }
    }

    // fades image in and out (used for fading to black, and then fading back to normal screen)
    private IEnumerator FadeInAndOut(Image image)
    {
        while (image.color.a < 1)
        {
            GameControl.SetAlpha(image, image.color.a + ALPHA_INTERVAL);
            yield return new WaitForSeconds(MEDIUM_ANIMATION_INTERVAL);
        }
        GameControl.UnfreezePlayerControls(capsule, cam);
        while (image.color.a > 0)
        {
            GameControl.SetAlpha(image, image.color.a - ALPHA_INTERVAL);
            yield return new WaitForSeconds(MEDIUM_ANIMATION_INTERVAL);
        }
    }

    // rotates screen
    private IEnumerator RotateScreen()
    {
        GameControl.FreezePlayerControls(capsule, cam);
        cam.GetComponent<CameraController>().SetIsShowingBats(true);
        yield return new WaitForSeconds(DOUBLE_ANIMATION_DURATION);
        cam.GetComponent<CameraController>().SetIsShowingBats(false);
    }

    // shows player falling into pit
    private IEnumerator FallIntoPit()
    {
        floor.GetComponent<MeshRenderer>().material = darkMaterial;
        capsule.GetComponent<CharacterController>().SetFallingInPit(true);
        yield return new WaitForSeconds(LONG_ANIMATION_DURATION);
        GameControl.FreezePlayerControls(capsule, cam);
        capsule.GetComponent<CharacterController>().SetFallingInPit(false);
    }

    // shows player coming out of pit
    private IEnumerator RiseFromPit()
    {
        capsule.GetComponent<CharacterController>().SetRisingFromPit(true);
        yield return new WaitForSeconds(LONG_ANIMATION_DURATION);
        capsule.GetComponent<CharacterController>().SetRisingFromPit(false);
        GameControl.UnfreezePlayerControls(capsule, cam);
        floor.GetComponent<MeshRenderer>().material = floorMaterial;
    }

    // arrow following projectile motion
    private IEnumerator ShootArrow()
    {
        while (!Input.GetKey(KeyCode.Space)) // until user hits space bar...
        {
            yield return new WaitForSeconds(MEDIUM_ANIMATION_INTERVAL);
        }

        // start projectile motion
        arrow.GetComponent<Rigidbody>().useGravity = true;
        arrow.GetComponent<Rigidbody>().velocity = capsule.transform.up * ARROW_INITIAL_VELOCITY[0]
                                                     + capsule.transform.forward * ARROW_INITIAL_VELOCITY[1];
        capsule.GetComponent<CharacterController>().ArrowWillBeShot();
        yield return new WaitForSeconds(ONE_SECOND); // wait for arrow to land on ground
        HidePauseCanvas();
        GameControl.Hide(arrow);
        if (!capsule.GetComponent<CharacterController>().ArrowShot()) // if arrow didn't reach adjacent room
        {
            ArrowShot(-1);
        }

    }

    // update time on screen
    private IEnumerator ManageTime()
    {
        while (secondsRemaining > 0)
        {
            if (!paused) // not paused, game still in session
            {
                GameControl.ChangeTime(secondsRemaining, timeLabel);
                secondsRemaining--;
            }
            yield return new WaitForSeconds(ONE_SECOND);
        }

        EndGame(GameControl.END_GAME_MESSAGES[4], false); // out of time, player loses
    }
}
