using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoTCPBehaviour : MonoBehaviour
{
    protected void OnEnable()
    {
        Client.Instance.OnReceiveMessage += OnReceiveTCPMessage;
    }

    protected void OnDisable()
    {
        Client.Instance.OnReceiveMessage -= OnReceiveTCPMessage;
    }

    protected abstract void OnReceiveTCPMessage();

    /// <summary>
    /// Put this somewhere in the code to handle the TCP Messages
    /// </summary>
    protected abstract void HandleTCPMessage();
}
