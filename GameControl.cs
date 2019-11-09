using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.IO;

public static class GameControl
{
    // global constants
    public static readonly string STORY_FILE_NAME = "Story.txt";
    public static readonly string INSTRUCTIONS_FILE_NAME = "Instructions.txt";

    public static readonly int NUM_ROOMS = 30;
    public static readonly int STARTING_ROOM = 0;
    public static readonly int NUM_NEIGHBORS_PER_ROOM = 6;
    public static readonly int TOTAL_COINS = 100;
    public static readonly int TIME_LIMIT = 300;
    public static readonly int SECONDS_IN_MINUTE = 60;
    public static readonly int MIN_DOUBLE_DIGIT_NUMBER = 10;
    public static readonly int NUM_TRIVIA_QUESTIONS = 3;
    public static readonly int WUMPUS_NUM_TRIVIA_QUESTIONS = 5;
    public static readonly int NUM_TRIVIA_CHOICES = 4;

    public static readonly string BATS_MESSAGE = "Oh no! You have entered a room with bats.";
    public static readonly string PIT_MESSAGE = "Oh no! You have entered a room with a bottomless pit.";
    public static readonly string WUMPUS_MESSAGE = "Oh no! It's the Wumpus! You must answer 3 out of 5 of its trivia questions to survive.";
    public static readonly string ESCAPED_PIT_MESSAGE = "Congratulations! You escaped the pit.";
    public static readonly string ESCAPED_WUMPUS_MESSAGE = "Congratulations! You escaped the Wumpus - for now.";

    public static readonly string BOUGHT_ARROWS_MESSAGE = "Two arrows have been added to your inventory.";
    public static readonly string ARROWS_FAILED_TRIVIA_MESSAGE = "Unfortunately, you may not purchase arrows at this time.";
    public static readonly string SECRET_FAILED_TRIVIA_MESSAGE = "Unfortunately, you may not purchase a secret at this time.";
    public static readonly string LACKS_COINS_ARROWS_MESSAGE = "You do not have enough coins to buy arrows.";
    public static readonly string LACKS_COINS_TRIVIA_MESSAGE = "You do not have enough coins to buy a secret.";
    public static readonly string ARROW_MISSED_MESSAGE = "Oops! Your arrow did not pass through a tunnel.";
    public static readonly string ARROW_WRONG_ROOM_MESSAGE = "The Wumpus was not in that room.";

    public static readonly string QUIT_GAME_MESSAGE = "Are you sure you want to quit this game? Your progress will not be saved.";
    public static readonly string[] END_GAME_MESSAGES = {"Game Over. You have fallen into a bottomless pit.", "Game Over. You have been defeated by the Wumpus.",
                                            "Game Over. You have run out of arrows.", "Game Over. You have run out of coins.", "Game Over. You have run out of time.",
                                            "Congratulations! Trebek has transformed back into the fantastic Jeopardy! host we know him to be. His fans hail you as their hero." };
    public static readonly int MENU_SCENE_NUMBER = 0;
    public static readonly int GAME_SCENE_NUMBER = 1;
    public static readonly int HIGH_SCORES_SCENE_NUMBER = 2;

    // instance variables
    private static string playerName;
    private static GameLocations _GameLocations;
    private static Player _Player;
    private static HighScores _HighScore;
    private static Trivia _Trivia;
    private static System.Random rand;
    private static string reasonForTrivia;

    // called every time user presses "Play Game" button
    // string name: string representing current player's name
    public static void NewGame(string name)
    {
        rand = new System.Random();
        _GameLocations = new GameLocations(NUM_ROOMS, NUM_NEIGHBORS_PER_ROOM, STARTING_ROOM, TOTAL_COINS, rand, rand.Next(2) == 0 ? true : false);
        _Trivia = new Trivia();
        _Player = new Player(TOTAL_COINS);
        playerName = name;

        SceneManager.LoadScene(GAME_SCENE_NUMBER);
    }

    // called when player enter new room
    // updates coins, turns
    public static void NewRoom(int roomNumber)
    {
        _Player.AddGoldCoins(_GameLocations.Update(roomNumber));
        _Player.NewTurn();
        _GameLocations.MoveWumpus();
    }

    // change screens

    public static void DisplayMenu()
    {
        SceneManager.LoadScene(MENU_SCENE_NUMBER);
    }

    public static void DisplayHighScores()
    {
        SceneManager.LoadScene(HIGH_SCORES_SCENE_NUMBER);
    }

    public static void Quit() // quit application
    {
        Application.Quit();
    }

    // UI elements

    // shows GameObject obj on screen
    public static void Show(GameObject obj)
    {
        obj.SetActive(true);
        obj.transform.SetAsFirstSibling();
    }

    // hides GameObject obj
    public static void Hide(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetAsLastSibling();
    }

    // allows Button button to be clicked
    public static void EnableButton(Button button)
    {
        button.enabled = true;
        button.image.color = button.colors.normalColor;
    }

    // disables Button button
    public static void DisableButton(Button button)
    {
        button.enabled = false;
        button.image.color = button.colors.disabledColor;
    }

    // changes text of label to integer amount;
    public static void ChangeLabel(int amount, Text label)
    {
        label.text = amount.ToString();
    }

    // updates time label based on seconds remaining
    public static void ChangeTime(int totalSeconds, Text label)
    {
        int minutes = totalSeconds / SECONDS_IN_MINUTE;
        int seconds = totalSeconds % SECONDS_IN_MINUTE;

        string timeStr = "0" + minutes + ":";
        if (seconds < MIN_DOUBLE_DIGIT_NUMBER)
        {
            timeStr += "0";
        }
        timeStr += seconds;

        label.text = timeStr;
    }

    // sets transparency level of Image image to float alpha
    public static void SetAlpha(Image image, float alpha)
    {
        Color curColor = image.color;
        curColor.a = alpha;
        image.color = curColor;
    }

    // sets transparency level of Text label to float alpha
    public static void SetAlpha(Text label, float alpha)
    {
        Color curColor = label.color;
        curColor.a = alpha;
        label.color = curColor;
    }

    // toggles sound on and off and updates sound icon on screen
    public static void ToggleMusic(AudioSource music, Button[] soundButtons, Sprite[] soundSprites)
    {
        foreach (Button button in soundButtons)
        {
            button.image.sprite = soundSprites[(int)Math.Round(music.volume)];
        }
        music.volume = 1.0f - music.volume;
    }

    // prevents player from moving or rotating screen with mouse/arrow keys
    public static void FreezePlayerControls(GameObject movement, GameObject rotation)
    {
        movement.GetComponent<CharacterController>().SetGameInSession(false);
        rotation.GetComponent<CameraController>().SetGameInSession(false);
    }

    // unfreezes player controls (mouse/arrow keys)
    public static void UnfreezePlayerControls(GameObject movement, GameObject rotation)
    {
        movement.GetComponent<CharacterController>().SetGameInSession(true);
        rotation.GetComponent<CameraController>().SetGameInSession(true);
    }

    // accessor methods

    // returns random room number
    public static int GetRandomRoom()
    {
        return rand.Next(NUM_ROOMS);
    }

    // returns GameLocations object
    public static GameLocations GetGameLocations()
    {
        return _GameLocations;
    }

    // returns Player object
    public static Player GetPlayer()
    {
        return _Player;
    }

    // returns HighScores object
    public static HighScores GetHighScore()
    {
        if (_HighScore == null)
        {
            _HighScore = new HighScores();
        }
        return _HighScore;
    }

    // returns Trivia object
    public static Trivia GetTrivia()
    {
        return _Trivia;
    }

    // returns current player name
    public static string GetCurrentPlayerName()
    {
        return playerName;
    }

    // returns storyline description as list of strings
    public static List<string> GetStory()
    {
        try
        {
            List<string> story = new List<string>();
            StreamReader storyReader = new StreamReader(STORY_FILE_NAME);
            while (!storyReader.EndOfStream)
            {
                story.Add(storyReader.ReadLine());
            }

            return story;
        }
        catch (FileNotFoundException exception) // handle error
        {
            Console.WriteLine("File Missing", exception);
            throw new FileNotFoundException(@"[file missing]", exception);
        }
    }

    // returns instructions as list of strings (one for each "page" of instructions)
    public static List<string> GetInstructions()
    {
        try
        {
            List<string> instructions = new List<string>();
            StreamReader instructionsReader = new StreamReader(INSTRUCTIONS_FILE_NAME);

            int currentIndex = -1;
            while (!instructionsReader.EndOfStream)
            {
                string line = instructionsReader.ReadLine();
                if (line.StartsWith("Instructions")) // new page
                {
                    instructions.Add(line);
                    currentIndex++;
                }
                else // continue page
                {
                    instructions[currentIndex] += "\n" + line;
                }
            }

            return instructions;
        }
        catch (FileNotFoundException exception) // handle error
        {
            Console.WriteLine("File Missing", exception);
            throw new FileNotFoundException(@"[file missing]", exception);
        }
    }
}
