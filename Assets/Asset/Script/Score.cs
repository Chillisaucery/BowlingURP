using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    public bool ShouldHardReset { get => (subRound == 2) || scoreThisRound >= 10; }

    ScoreProfile scoreProfile;
    int scoreThisRound = 0;
    int subRound = 1;
    Color initTextColor;



    private void OnEnable()
    {
        if (throwing == null)
            throwing = FindObjectOfType<Throwing>();

        throwing.OnReset += OnReset;
    }

    private void OnDisable()
    {
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

        roundTextMesh.text = scoreProfile.Round.ToString();
    }

    private void ChangeBallColor()
    {
        if (scoreProfile.PlayerID == PlayerID.Player1 && ballMesh.material != ballMaterial1)
        {
            ballMesh.material = ballMaterial1;
            forceSliderImage.color = redColor;
        }
        else if (scoreProfile.PlayerID == PlayerID.Player2 && ballMesh.material != ballMaterial2)
        {
            ballMesh.material = ballMaterial2;
            forceSliderImage.color = blueColor;
        }
    }

    public void OnPinFall()
    {
        scoreProfile.AddScore(1);
        scoreThisRound++;
    }

    public void OnReset()
    {
        subRound++;

        //Hard Reset
        if ((subRound >= 3) || scoreThisRound >= 10)
        {
            scoreProfile.ChangePlayer();
            ChangeTextColor();

            subRound = 1;
            scoreThisRound = 0;
        }
    }

    private void ChangeTextColor()
    {
        player1PlaceHolder.color = initTextColor;
        player2PlaceHolder.color = initTextColor;

        player1Score.color = initTextColor;
        player2Score.color = initTextColor;

        if (scoreProfile.PlayerID == PlayerID.Player1)
        {
            player1PlaceHolder.color = Color.white;
            player1Score.color = Color.white;
        }
        else if (scoreProfile.PlayerID == PlayerID.Player2)
        {
            player2PlaceHolder.color = Color.white;
            player2Score.color = Color.white;
        }
    }
}

public class ScoreProfile
{
    int player1Score = 0;
    int player2Score = 0;
    int round = 1;
    PlayerID playerID = PlayerID.Player1;

    public ScoreProfile()
    {
        
    }

    public int Player1Score { get => player1Score; private set => player1Score = value; }
    public int Player2Score { get => player2Score; private set => player2Score = value; }
    public int Round { get => round; private set => round = value; }
    public PlayerID PlayerID { get => playerID; private set => playerID = value; }

    public void AddScore(int value)
    {
        switch (PlayerID)
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

    public void ChangePlayer()
    {
        if (PlayerID == PlayerID.Player1)
        {
            PlayerID = PlayerID.Player2;
        }
        else if (PlayerID == PlayerID.Player2)
        {
            PlayerID = PlayerID.Player1;
            Round++;
        }
        else if (PlayerID == PlayerID.None)
        {
            PlayerID = PlayerID.Player1;
        }
    }
}

public enum PlayerID
{
    Player1,
    Player2,
    None
}
