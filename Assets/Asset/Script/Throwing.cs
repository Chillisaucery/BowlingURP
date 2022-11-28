using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GlobalMethods;

public class Throwing : MonoBehaviour
{
    [SerializeField]
    Rigidbody ballRB;

    [SerializeField]
    List<Transform> pinTransforms = new List<Transform>();

    [SerializeField]
    Slider forceSlider;

    [SerializeField]
    RectTransform forceCanvas;

    [SerializeField]
    Animator ballAnimator;

    [SerializeField]
    float   moveSpeed = 20f, 
            moveBound = 3f, 
            angleChangeSpeed = 5f, 
            minAngle = -0.3f, 
            maxAngle = 0.3f, 
            maxThrowingForce = 20f, 
            forceChangeSpeed = 20f,
            resetDelay = 3f;

    [SerializeField]
    Vector3 initialThrowingDirection = new Vector3(0, 0, 0.2f).normalized;


    Score score;

    float throwingForce = 0;
    bool isForceChargingUp = true;
    Vector3 throwingDirection = Vector3.zero;

    bool isThrown = false;

    Vector3 initialBallPos;
    List<(Vector3 pos, Vector3 eulerAngle)> initialPinTransforms = new List<(Vector3 pos, Vector3 eulerAngle)>();

    public delegate void ThrowEvent();
    public ThrowEvent OnThrow;
    public ThrowEvent OnReset;



    private void Start()
    {
        throwingDirection = initialThrowingDirection;

        score = FindObjectOfType<Score>();

        initialBallPos = ballRB.transform.position;

        initialPinTransforms.Clear();

        foreach (Transform pinTransform in pinTransforms)
        {
            initialPinTransforms.Add((pinTransform.position, pinTransform.eulerAngles));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isThrown)
        {
            //Change ball position
            if (Input.GetKey(KeyCode.A))
                ballRB.transform.position += new Vector3(0, 0, moveSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.D))
                ballRB.transform.position += new Vector3(0, 0, -moveSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.W))
                ballRB.transform.position += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
            if (Input.GetKey(KeyCode.S))
                ballRB.transform.position += new Vector3(-moveSpeed * Time.deltaTime, 0, 0);

            ballRB.transform.position = new Vector3(
                Mathf.Clamp(ballRB.transform.position.x, initialBallPos.x -moveBound, initialBallPos.x + moveBound),
                ballRB.transform.position.y,
                Mathf.Clamp(ballRB.transform.position.z, initialBallPos.z - moveBound, initialBallPos.z + moveBound)
            );

            //Change throwing angle
            if (Input.GetKey(KeyCode.LeftArrow))
                throwingDirection += new Vector3(0,0, angleChangeSpeed * Time.deltaTime);
            else if (Input.GetKey(KeyCode.RightArrow))
                throwingDirection += new Vector3(0,0, -angleChangeSpeed * Time.deltaTime);

            throwingDirection = new Vector3 (0,0,Mathf.Clamp(throwingDirection.z, minAngle, maxAngle));
            RotateForceCanvas(forceCanvas, throwingDirection);
        }

        //Throw
        if (Input.GetKey(KeyCode.UpArrow) && !isThrown)
        {
            //Decide the force charging up or down
            if (throwingForce > maxThrowingForce + forceChangeSpeed * Time.deltaTime)
                isForceChargingUp = false;
            else if (throwingForce < 0 - forceChangeSpeed * Time.deltaTime)
                isForceChargingUp = true;

            //Charge the force
            if (isForceChargingUp)
            {
                throwingForce += forceChangeSpeed * Time.deltaTime;
            }
            else if (!isForceChargingUp)
            {
                throwingForce -= forceChangeSpeed * Time.deltaTime;
            }

            forceSlider.value = throwingForce / maxThrowingForce;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow) && !isThrown)
        {
            isThrown = true;
            forceSlider.gameObject.SetActive(false);

            ballAnimator.Play("Anticipation");
            ballRB.useGravity = false;
            Invoke(nameof(ThrownBall), 1);
        }

        //Reset if press space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetThrowing(score.ShouldHardReset);
        }
    }

    public void ResetThrowing(bool shouldHardReset)
    {
        StopAllCoroutines();

        ballRB.transform.position = initialBallPos;
        ballRB.velocity = new Vector3(0, 0, 0);
        ballRB.angularVelocity = new Vector3(0, 0, 0);

        throwingForce = 0;
        throwingDirection = initialThrowingDirection;

        for (int i = 0; i < pinTransforms.Count; i++)
        {
            if (shouldHardReset)
            {
                pinTransforms[i].gameObject.SetActive(true);
                pinTransforms[i].GetComponent<Pin>().ResetFallen();
            }
            else
            {
                if (pinTransforms[i].GetComponent<Pin>().IsFallen)
                    pinTransforms[i].gameObject.SetActive(false);
            }

            pinTransforms[i].position = initialPinTransforms[i].pos;
            pinTransforms[i].eulerAngles = initialPinTransforms[i].eulerAngle;
            pinTransforms[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            pinTransforms[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        isThrown = false;
        forceSlider.gameObject.SetActive(true);
        forceSlider.value = throwingForce / maxThrowingForce;

        if (OnReset != null)
            OnReset.Invoke();
    }

    private void RotateForceCanvas(RectTransform forceCanvas, Vector3 throwingDirection)
    {
        throwingDirection = (throwingDirection + new Vector3(1, 0, 0)).normalized;
        
        forceCanvas.eulerAngles = new Vector3(
            forceCanvas.eulerAngles.x, 
            Mathf.Atan2(throwingDirection.x, throwingDirection.z) * Mathf.Rad2Deg -90, 
            forceCanvas.eulerAngles.z);
    }

    private void ThrownBall()
    {
        ballRB.useGravity = true;
        ballRB.AddForce((throwingDirection + new Vector3(1,0,0)).normalized * throwingForce);

        ballAnimator.Play("Idle");

        StartCoroutine(DelayedInvoke(resetDelay, () => ResetThrowing(score.ShouldHardReset)));

        if (OnThrow != null)
            OnThrow.Invoke();
    }
}
