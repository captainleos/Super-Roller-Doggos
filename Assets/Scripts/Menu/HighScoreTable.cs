using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HighScoreTable : MonoBehaviour
{
    // High Score table is based on Code Monkey's High Score Table with Saving and Loading tutorial https://www.youtube.com/watch?v=iAbaqGYdnyI&ab_channel=CodeMonkey

    private Transform entryContainer;
    private Transform entryTemplate;
    private List<HighScoreEntry> highScoreEntryList;
    private List<Transform> highScoreEntryTransformList;

    [SerializeField] private float templateHeight = 20f;

    private void Awake()
    {
        entryContainer = transform.Find("highscoreEntryContainer");
        entryTemplate = entryContainer.Find("highscoreEntryTemplate");
        entryTemplate.gameObject.SetActive(false);

        string jsonString = PlayerPrefs.GetString("highScoreTable");
        HighScores highScores = JsonUtility.FromJson<HighScores>(jsonString);

        try
        {
            Debug.Log(highScores.highScoreEntryList);
        }
        catch
        {
            // Writing a new first entry
            highScoreEntryList = new List<HighScoreEntry>()
            {
                new HighScoreEntry{score = 0, name = "AAA"}
            };

            string json = JsonUtility.ToJson(highScoreEntryList);
            PlayerPrefs.SetString("highScoreTable", json);
            PlayerPrefs.Save();

            jsonString = PlayerPrefs.GetString("highScoreTable");
            highScores = JsonUtility.FromJson<HighScores>(jsonString);

            Debug.Log(PlayerPrefs.GetString("highScoreTable"));
            Debug.Log(highScores.highScoreEntryList);
        }

        highScoreEntryList = highScores.highScoreEntryList;

        highScoreEntryTransformList = new List<Transform>();
        foreach (HighScoreEntry highScoreEntry in highScoreEntryList)
        {
            CreateHighScoreEntryTransform(highScoreEntry, entryContainer, highScoreEntryTransformList);
        }
    }

    //private void CheckIfTableExists(HighScores highScores, string jsonString)
    //{
        
    //}

    private void CreateHighScoreEntryTransform(HighScoreEntry highScoreEntry, Transform container, List<Transform> transformList)
    {
        Transform entryTransform = Instantiate(entryTemplate, container);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -templateHeight * transformList.Count);
        entryTransform.gameObject.SetActive(true);

        int rank = transformList.Count + 1;
        string rankString;
        switch (rank)
        {
            default: rankString = rank + "th"; break;
            case 1: rankString = "1st"; break;
            case 2: rankString = "2nd"; break;
            case 3: rankString = "3rd"; break;
        }

        entryTransform.Find("posText").GetComponent<TextMeshProUGUI>().text = rankString;

        int score = highScoreEntry.score;

        entryTransform.Find("scoreText").GetComponent<TextMeshProUGUI>().text = score.ToString();

        string name = highScoreEntry.name;

        entryTransform.Find("nameText").GetComponent<TextMeshProUGUI>().text = name;

        transformList.Add(entryTransform);
    }

    public void AddHighScoreEntry(int score, string name) 
    {
        // Create high score entry
        HighScoreEntry highScoreEntry = new HighScoreEntry { score = score, name = name };
        
        // Load saved high scores
        string jsonString = PlayerPrefs.GetString("highScoreTable");
        HighScores highScores = JsonUtility.FromJson<HighScores>(jsonString);

        // Add new entry to high scores
        highScores.highScoreEntryList.Add(highScoreEntry);

        // Sort entry list by Score
        for (int i = 0; i < highScores.highScoreEntryList.Count; i++)
        {
            for (int j = i + 1; j < highScores.highScoreEntryList.Count; j++)
            {
                if (highScores.highScoreEntryList[j].score > highScores.highScoreEntryList[i].score)
                {
                    // Swap
                    HighScoreEntry tmp = highScores.highScoreEntryList[i];
                    highScores.highScoreEntryList[i] = highScores.highScoreEntryList[j];
                    highScores.highScoreEntryList[j] = tmp;
                }
            }
        }

        // Remove overflow
        if (highScores.highScoreEntryList.Count > 10)
        {
            highScores.highScoreEntryList.RemoveAt(10);
        }

        // Save updated high scores
        string json = JsonUtility.ToJson(highScores);
        PlayerPrefs.SetString("highScoreTable", json);
        PlayerPrefs.Save();
    }

    // Warning, deletes all entries.
    private void ClearHighScoreTable()
    {
        // Load saved high scores
        string jsonString = PlayerPrefs.GetString("highScoreTable");
        HighScores highScores = JsonUtility.FromJson<HighScores>(jsonString);

        // Delete all entries
        int amount = highScores.highScoreEntryList.Count;
        for (int i = 0; i < amount; i++)
        {
            highScores.highScoreEntryList.RemoveAt(0);
        }

        // Save cleared table
        string json = JsonUtility.ToJson(highScores);
        PlayerPrefs.SetString("highScoreTable", json);
        PlayerPrefs.Save();
    }

    // Unsure about this bit, maybe class is required for saving of list?
    private class HighScores
    {
        public List<HighScoreEntry> highScoreEntryList;
    }

    // Represents a single High Score entry
    [System.Serializable]
    private class HighScoreEntry
    {
        public int score;
        public string name;
    }
}
