using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    [SerializeField]
    float colorChangeInterval = 1f;

    [SerializeField]
    TMP_Text pressEnterText;

    [SerializeField]
    Color pressEnterColor1, pressEnterColor2;

    [SerializeField]
    GameObject profileEnter;



    //Private variables
    SceneState sceneState = SceneState.Start;

 
    private void Start()
    {
        StartCoroutine(JiggleTextColor(pressEnterText, colorChangeInterval, pressEnterColor1, pressEnterColor2));

        profileEnter.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            //Change State
            switch (sceneState)
            {
                case SceneState.Start:
                    sceneState = SceneState.EnterData;
                    break;
                case SceneState.EnterData:
                    sceneState = SceneState.End;
                    break;
                case SceneState.End:
                    break;
            }

            OnSwitchState();
        }
    }

    private void OnSwitchState()
    {
        switch (sceneState)
        {
            case SceneState.Start:
                break;
            case SceneState.EnterData:
                profileEnter.SetActive(true);
                pressEnterText.text = "Press Enter again to start";
                break;
            case SceneState.End:
                if (PlayerNetworkProfile.Instance.CanJoinGame)
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                break;
        }

    }

    IEnumerator JiggleTextColor(TMP_Text textMesh, float interval, Color color1, Color color2)
    {
        float timeLeft;

        Color startingColor = color1;
        Color targetColor = color2;

        while (true)
        {
            timeLeft = interval;
            textMesh.color = startingColor;

            while (timeLeft > 0)
            {
                yield return null;

                timeLeft -= Time.deltaTime;

                textMesh.color = Color.Lerp(startingColor, targetColor, timeLeft / interval);
            }

            (startingColor, targetColor) = (targetColor, startingColor);
        }
    }
}

enum SceneState
{
    Start,
    EnterData,
    End
}
