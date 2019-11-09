using System;
using System.Collections.Generic;
using System.IO;

public class HighScores
{
    // constants
    private readonly string HIGH_SCORES_FILE = "HighScores.txt";
    private readonly int NUM_HIGH_SCORES = 10;

    private List<int> overallhighs;
    private List<string> highScoreNames;

    // initialize variables
    public HighScores()
    {
        overallhighs = new List<int>();
        highScoreNames = new List<string>();

        if (File.Exists(@HIGH_SCORES_FILE)) // load scores if file exists
        {
            ReadHighScoreTextFile();
        }
    }

    // Accessors

    // returns high scores
    public List<int> GetHighScores()
    {
        return overallhighs;
    }

    // returns names corresponding to high scores
    public List<string> GetHighScoreNames()
    {
        List<string> actHighScore = highScoreNames;
        actHighScore.Reverse();
        return actHighScore;
    }

    // Mutators

    // write names/scores to file
    public void WriteHighScoreTextFile()
    {
        string[] final_text = OverallHighScorestring(highScoreNames.ToArray(), overallhighs.ToArray());
        File.WriteAllLines(@HIGH_SCORES_FILE, final_text);
    }

    // clear all high scores
    public void ResetHighScores()
    {
        File.Delete(@HIGH_SCORES_FILE);
        highScoreNames = new List<string>();
        overallhighs = new List<int>();
    }

    // Helper methods

    // load names/scores
    private void ReadHighScoreTextFile()
    {
        string[] lines = File.ReadAllLines(@HIGH_SCORES_FILE);
        foreach (string line in lines)
        {
            string[] nameNumSplit = line.Split();
            highScoreNames.Add(nameNumSplit[0]);
            overallhighs.Add(Int32.Parse(nameNumSplit[1]))  ;
        }
        highScoreNames.Reverse();
    }

    // formats high score names and scores for file
    private string[] OverallHighScorestring(string[] highScoreNamesArray, int[] HighScores)
    {
        List<string> textToWrite = new List<string>();
        for (int i = 0; i < HighScores.Length; i++)
        {
            textToWrite.Add(highScoreNamesArray[i] + "\t" + HighScores[i]);
        }
        return textToWrite.ToArray();
    }

    //Is the current game score a high score?
    private bool IsNewOverallHighScore(int currentGameScore)
    {
        if (overallhighs.Count == 0)
        {
            return true;
        }

        int last_index = overallhighs.Count - 1;
        return currentGameScore > overallhighs[last_index];
    }

    //Place the current game score where is needs to be
    // int currentGameScore: new score
    // string currentName: name of player who achieved new score
    public void UpdateHighScores(int currentGameScore, string currentName)
    {
        if (overallhighs.Count < NUM_HIGH_SCORES) // can be added to end of list
        {
            overallhighs.Add(currentGameScore);
            overallhighs.Sort();
            int name_index = overallhighs.IndexOf(currentGameScore);
            highScoreNames.Insert(name_index, currentName);
            overallhighs.Reverse();
        }
        else // verifying/sorting required
        {
            if (IsNewOverallHighScore(currentGameScore))
            {
                overallhighs.Add(currentGameScore);
                overallhighs.Sort();
                overallhighs.RemoveAt(0);
                int current_name_Index = overallhighs.IndexOf(currentGameScore) + 1;
                highScoreNames.Insert(current_name_Index, currentName);
                highScoreNames.RemoveAt(0);
                overallhighs.Reverse();
            }
        }
    }


}