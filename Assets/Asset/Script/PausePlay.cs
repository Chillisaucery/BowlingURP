using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GlobalMethods;

public class PausePlay : MonoTCPBehaviour
{
    [SerializeField]
    GameObject pauseCanvas;

    bool isPaused = false;

    public bool IsPaused { get => isPaused; private set
        {
            isPaused = value;
            Client.Instance.SendTCPMessage(PAUSE_PREFIX + isPaused);
        }
    }

    private void Start()
    {
        pauseCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();

        HandlePausing();

        //HandleLosingFocus();
    }

    private void HandlePausing()
    {
        if (IsPaused) 
        {
            pauseCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            pauseCanvas.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsPaused)
        { 
            IsPaused = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && IsPaused)
        {
            IsPaused = false;
        }

        if (Input.GetKeyDown(KeyCode.Return) && IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// When losing focus (when the player alt tab), pause the game.
    /// </summary>
    private void HandleLosingFocus()
    {
        if (!IsPaused && !Application.isFocused)
            IsPaused = true;
        else if (IsPaused && Application.isFocused)
            IsPaused = false;
    }



    //TCP
    protected override void OnReceiveTCPMessage()
    {
        if (Client.Instance.LatestMsg.StartsWith(PAUSE_PREFIX))
        {
            string pauseTCPString = Client.Instance.LatestMsg.Substring(PAUSE_PREFIX.Length+2).Trim();

            Debug.Log(PAUSE_PREFIX + pauseTCPString);

            if (pauseTCPString.Contains("true"))
                IsPaused = true;
            else if (!pauseTCPString.Contains("false"))
                IsPaused = false;
        }
    }

    protected override void HandleTCPMessage()
    {
        
    }
}
