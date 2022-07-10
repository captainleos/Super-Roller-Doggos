using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(1000)]
public class MenuManager : MonoBehaviour
{
    [Header("Title menu")]
    [SerializeField] private GameObject logo;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject highScoresButton;
    [SerializeField] private GameObject creditsButton;
    [SerializeField] private GameObject quitButton;

    [Header("Select doggo")]
    [SerializeField] private GameObject selectYourDoggo;
    [SerializeField] private GameObject doggos;
    [SerializeField] private GameObject prevButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject selectButton;
    private CurrentDoggo currentDoggo;

    [Header("High Scores")]
    [SerializeField] private GameObject highScoresList;

    [Header("Credits")]
    [SerializeField] private GameObject creditsBG;
    [SerializeField] private GameObject creditsMusic;
    [SerializeField] private GameObject creditsSounds;
    [SerializeField] private GameObject creditsOther;

    [Header("Universal")]
    [SerializeField] private GameObject backButton;

    void Start()
    {
        currentDoggo = doggos.GetComponent<CurrentDoggo>();
    }

    public void StartButton()
    {
        logo.SetActive(false);
        startButton.SetActive(false);
        highScoresButton.SetActive(false);
        creditsButton.SetActive(false);
        quitButton.SetActive(false);

        selectYourDoggo.SetActive(true);
        doggos.SetActive(true);
        prevButton.SetActive(true);
        nextButton.SetActive(true);
        selectButton.SetActive(true);

        backButton.SetActive(true);
    }

    public void NextButton()
    {
        currentDoggo.selectedDoggo = currentDoggo.selectedDoggo + 1;
        if (currentDoggo.selectedDoggo == currentDoggo.doggos.Count)
        {
            currentDoggo.selectedDoggo = 0;
        }
        currentDoggo.ShowSelectedDoggo(currentDoggo.selectedDoggo);
        MainManager.Instance.selectedDoggo = currentDoggo.selectedDoggo;
    }

    public void PrevButton()
    {
        currentDoggo.selectedDoggo = currentDoggo.selectedDoggo - 1;
        if (currentDoggo.selectedDoggo < 0)
        {
            currentDoggo.selectedDoggo = currentDoggo.doggos.Count - 1;
        }
        currentDoggo.ShowSelectedDoggo(currentDoggo.selectedDoggo);
        MainManager.Instance.selectedDoggo = currentDoggo.selectedDoggo;
    }

    public void SelectButton()
    {
        SceneManager.LoadScene(1);
    }

    public void HighScoresButton()
    {
        logo.SetActive(false);
        startButton.SetActive(false);
        highScoresButton.SetActive(false);
        creditsButton.SetActive(false);
        quitButton.SetActive(false);

        highScoresList.SetActive(true);

        backButton.SetActive(true);
    }

    public void CreditsButton()
    {
        logo.SetActive(false);
        startButton.SetActive(false);
        highScoresButton.SetActive(false);
        creditsButton.SetActive(false);
        quitButton.SetActive(false);

        creditsBG.SetActive(true);
        creditsMusic.SetActive(true);
        creditsSounds.SetActive(true);
        creditsOther.SetActive(true);

        backButton.SetActive(true);
    }

    public void BackButton()
    {
        logo.SetActive(true);
        startButton.SetActive(true);
        highScoresButton.SetActive(true);
        creditsButton.SetActive(true);
        quitButton.SetActive(true);

        selectYourDoggo.SetActive(false);
        doggos.SetActive(false);
        prevButton.SetActive(false);
        nextButton.SetActive(false);
        selectButton.SetActive(false);

        highScoresList.SetActive(false);

        creditsBG.SetActive(false);
        creditsMusic.SetActive(false);
        creditsSounds.SetActive(false);
        creditsOther.SetActive(false);

        backButton.SetActive(false);
    }

    public void Exit()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit(); // original code to quit Unity player
#endif
    }
}
