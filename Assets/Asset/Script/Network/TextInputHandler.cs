using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static GlobalMethods;

public class TextInputHandler : MonoBehaviour
{
    [SerializeField]
    TMP_Text nameTextMesh, roomIDTextMesh;


    #region Mono Behaviour
    private void OnEnable()
    {
        Client.Instance.OnReceiveMessage += OnReceiveMessage;
    }

    private void OnDisable()
    {
        Client.Instance.OnReceiveMessage -= OnReceiveMessage;
    }
    #endregion

    #region Handle Receiving Message
    private void OnReceiveMessage()
    {
        HandleNameMsg();
        HandleRoomIDMsg();
    }

    private void HandleRoomIDMsg()
    {
        if (Client.Instance.LatestMsg.StartsWith(ROOM_PREFIX))
        {
            string room = Client.Instance.LatestMsg.Substring(5).Trim();
            Debug.Log("room " + room);

            PlayerNetworkProfile.Instance.Room = room;
        }
    }

    private void HandleNameMsg()
    {
        //Debug.Log("Handling Name Msg" + Client.Instance.LatestMsg);
        if (Client.Instance.LatestMsg.StartsWith(NAME_PREFIX))
        {
            string name = Client.Instance.LatestMsg.Substring(5).Trim();
            Debug.Log("name " + name);

            PlayerNetworkProfile.Instance.Name = name;
        }
    }
    #endregion

    #region Sending Message
    public void OnNameFinish()
    {
        //If the name is changed, send a tcp message
        if (PlayerNetworkProfile.Instance.Name != ClearString(nameTextMesh.text))
            Client.Instance.SendTCPMessage(NAME_PREFIX + ClearString(nameTextMesh.text));
    }

    public void OnRoomIDFinish()
    {
        //Exit the old room, then enter the new room
        //Client.Instance.SendTCPMessage(ROOM_PREFIX + "EXIT");
        Client.Instance.SendTCPMessage(ROOM_PREFIX + ClearString(roomIDTextMesh.text));
    }
    #endregion
}
