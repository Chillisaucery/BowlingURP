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

 
    private void Start()
    {
        StartCoroutine(JiggleTextColor(pressEnterText, colorChangeInterval, pressEnterColor1, pressEnterColor2));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
