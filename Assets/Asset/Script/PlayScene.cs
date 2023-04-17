using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PausePlay : MonoBehaviour
{
    [SerializeField]
    GameObject pauseCanvas;

    bool isPaused = false;



    private void Start()
    {
        pauseCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            pauseCanvas.SetActive(true);

            Time.timeScale = 0f;
            isPaused = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused)
        {
            pauseCanvas.SetActive(false);

            Time.timeScale = 1f;
            isPaused = false;
        }

        if (Input.GetKeyDown(KeyCode.Return) && isPaused)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }

        HandleLosingFocus();
    }

    /// <summary>
    /// When losing focus (when the player alt tab), pause the game.
    /// </summary>
    private void HandleLosingFocus()
    {
        if (!isPaused && !Application.isFocused)
            isPaused = false;
    }
}
