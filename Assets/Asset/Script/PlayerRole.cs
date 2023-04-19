using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static GlobalMethods;

public class PlayerRole : MonoTCPBehaviour
{
    [SerializeField]
    Color player1Color, player2Color, observerColor;

    [SerializeField]
    Image playerRoleImage;

    int playerCode = -1, oldPlayerCode = -1;

    public int PlayerCode { get => playerCode; private set => playerCode = value; }



    private void Start()
    {
        Client.Instance.SendTCPMessage(START_GAME_PREFIX);
    }

    void Update()
    {
        HandleTCPMessage();
    }

    protected override void HandleTCPMessage()
    {
        if (PlayerCode == oldPlayerCode) { return; }    //Do nothing because the code doesn't change

        switch (PlayerCode)
        {
            case 0:
                playerRoleImage.color = player1Color;
                break;
            case 1:
                playerRoleImage.color = player2Color;
                break;
            default:
                playerRoleImage.color = observerColor;
                break;
        }

        oldPlayerCode = PlayerCode;
    }

    protected override void OnReceiveTCPMessage()
    {
        if (Client.Instance.LatestMsg.StartsWith(PLAYER_CODE_PREFIX))
        {
            if (Client.Instance.LatestMsg.Contains("0"))
                PlayerCode = 0;
            else if (Client.Instance.LatestMsg.Contains("1"))
                PlayerCode = 1;
            else PlayerCode = 2;
        }
    }
}
