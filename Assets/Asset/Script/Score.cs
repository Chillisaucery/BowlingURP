using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static GlobalMethods;

public class Score : MonoBehaviour
{
    [SerializeField]
    TMP_Text player1Score, player2Score, player1PlaceHolder, player2PlaceHolder, roundTextMesh;

    [SerializeField]
    MeshRenderer ballMesh;

    [SerializeField]
    Material ballMaterial1, ballMaterial2;

    [SerializeField]
    Image forceSliderImage;

    [SerializeField]
    Color redColor, blueColor;

    Throwing throwing = null;
    PlayerRole playerRole;

    public bool ShouldHardReset { get => (subRound == 2) || scoreThisRound >= 10; }
    public bool IsInTurn { get =>
            (scoreProfile.CurrentPlayer == PlayerID.Player1 && playerRole.PlayerCode == 0) ||
            (scoreProfile.CurrentPlayer == PlayerID.Player2 && playerRole.PlayerCode == 1);
        }

    ScoreProfile scoreProfile;
    int scoreThisRound = 0;
    int subRound = 1;
    Color initTextColor;

    int[] standingPins = null;
    int[] oldStandingPins = null;

    bool shouldHardResetPins = false;



    private void OnEnable()
    {
        Client.Instance.OnReceiveMessage += OnReceiveTCPMessage;

        if (throwing == null)
            throwing = FindObjectOfType<Throwing>();

        if (playerRole == null)
            playerRole = FindObjectOfType<PlayerRole>();

        throwing.OnReset += OnReset;
    }

    private void OnDisable()
    {
        Client.Instance.OnReceiveMessage += OnReceiveTCPMessage;

        throwing.OnReset -= OnReset;
    }



    private void Start()
    {
        scoreProfile = new ScoreProfile();

        initTextColor = player1Score.color;
    }

    private void Update()
    {
        player1Score.text = scoreProfile.Player1Score.ToString();
        player2Score.text = scoreProfile.Player2Score.ToString();

        ChangeTextColor();
        ChangeBallColor();

        HandleTCPMessages();

        roundTextMesh.text = scoreProfile.Round.ToString();
    }

    private void HandleTCPMessages()
    {
        //Handle reseting pins
        if (standingPins != oldStandingPins)
        {
            throwing.ResetPins(standingPins);
            oldStandingPins = standingPins;
        }

        if (shouldHardResetPins)
        {
            throwing.ResetPins(true);   //Hard reset if it goes into new player
            shouldHardResetPins = false;
        }
    }

    private void ChangeBallColor()
    {
        if (scoreProfile.CurrentPlayer == PlayerID.Player1 && ballMesh.material != ballMaterial1)
        {
            ballMesh.material = ballMaterial1;
            forceSliderImage.color = redColor;
        }
        else if (scoreProfile.CurrentPlayer == PlayerID.Player2 && ballMesh.material != ballMaterial2)
        {
            ballMesh.material = ballMaterial2;
            forceSliderImage.color = blueColor;
        }
    }

    public void OnPinFall()
    {
        if (!IsInTurn)
            return;

        scoreProfile.AddScore(1);
        scoreThisRound++;
    }

    public void OnReset()
    {
        //This TCP call has to be above
        if (IsInTurn)
        {
            Client.Instance.SendTCPMessage(RESET_THROW_PREFIX + scoreProfile.CurrentScore);

            string pinMsg = JsonConvert.SerializeObject(throwing.StandingPins);
            Client.Instance.SendTCPMessage(RESET_PINS_PREFIX + pinMsg);

            subRound++;
        }
        else
        {
            subRound = 1;
            scoreThisRound = 0;
        }

        //Hard Reset
        if ((subRound >= 3) || scoreThisRound >= 10)
        {
            scoreProfile.ChangePlayer();
            subRound = 1;
            scoreThisRound = 0;

            Client.Instance.SendTCPMessage(CHANGE_PLAYER_PREFIX + scoreProfile.CurrentPlayer);
            Client.Instance.SendTCPMessage(CHANGE_ROUND_PREFIX + scoreProfile.Round);
        }
    }

    private void ChangeTextColor()
    {
        player1PlaceHolder.color = initTextColor;
        player2PlaceHolder.color = initTextColor;

        player1Score.color = initTextColor;
        player2Score.color = initTextColor;

        if (scoreProfile.CurrentPlayer == PlayerID.Player1)
        {
            player1PlaceHolder.color = Color.white;
            player1Score.color = Color.white;
        }
        else if (scoreProfile.CurrentPlayer == PlayerID.Player2)
        {
            player2PlaceHolder.color = Color.white;
            player2Score.color = Color.white;
        }
    }

    protected void OnReceiveTCPMessage()
    {
        if (Client.Instance.LatestMsg.StartsWith(RESET_THROW_PREFIX))
        {
            string scoreString = Client.Instance.LatestMsg.Substring(RESET_THROW_PREFIX.Length).Trim();

            int score = Int32.Parse(scoreString);
            scoreProfile.UpdateScore(score);
        }

        if (Client.Instance.LatestMsg.StartsWith(CHANGE_PLAYER_PREFIX) ||
            Client.Instance.LatestMsg.StartsWith(CHANGE_ROUND_PREFIX))
        {
            if (Client.Instance.LatestMsg.StartsWith(CHANGE_PLAYER_PREFIX))
            {
                if (Client.Instance.LatestMsg.Contains("1"))
                    scoreProfile.CurrentPlayer = PlayerID.Player1;
                else if (Client.Instance.LatestMsg.Contains("2"))
                    scoreProfile.CurrentPlayer = PlayerID.Player2;

                shouldHardResetPins = true;
            }
            if (Client.Instance.LatestMsg.StartsWith(CHANGE_ROUND_PREFIX))
            {
                string roundString = Client.Instance.LatestMsg.Substring(CHANGE_ROUND_PREFIX.Length).Trim();

                int round = Int32.Parse(roundString);
                scoreProfile.Round = round;
            }

            subRound = 1;
            scoreThisRound = 0;
        }

        if (Client.Instance.LatestMsg.StartsWith(RESET_PINS_PREFIX))
        {
            string pinsString = Client.Instance.LatestMsg.Substring(RESET_PINS_PREFIX.Length).Trim();

            standingPins = JsonConvert.DeserializeObject<int[]>(pinsString);
        }
    }
}

public class ScoreProfile
{
    int player1Score = 0;
    int player2Score = 0;
    int round = 1;
    PlayerID playerID = PlayerID.Player1;

    public ScoreProfile() { }

    public int Player1Score { get => player1Score; private set => player1Score = value; }
    public int Player2Score { get => player2Score; private set => player2Score = value; }
    public int Round { get => round; set => round = value; }
    public PlayerID CurrentPlayer { get => playerID; set => playerID = value; }
    public int CurrentScore { get
        {
            if (playerID == PlayerID.Player1) { return player1Score; }
            if (playerID == PlayerID.Player2) { return player2Score; }
            return 0;
        }
    }

    public void AddScore(int value)
    {
        switch (CurrentPlayer)
        {
            case (PlayerID.Player1):
                Player1Score += value;
                break;
            case (PlayerID.Player2):
                Player2Score += value;
                break;
            default:
                break;
        }
    }

    public void UpdateScore(int value)
    {
        switch (CurrentPlayer)
        {
            case (PlayerID.Player1):
                Player1Score = value;
                break;
            case (PlayerID.Player2):
                Player2Score = value;
                break;
            default:
                break;
        }
    }


    public void ChangePlayer()
    {
        if (CurrentPlayer == PlayerID.Player1)
        {
            CurrentPlayer = PlayerID.Player2;
        }
        else if (CurrentPlayer == PlayerID.Player2)
        {
            CurrentPlayer = PlayerID.Player1;
            Round++;
        }
        else if (CurrentPlayer == PlayerID.None)
        {
            CurrentPlayer = PlayerID.Player1;
        }
    }
}

public enum PlayerID
{
    Player1,
    Player2,
    None
}
