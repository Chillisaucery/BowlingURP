using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GlobalMethods;

public class Throwing : MonoBehaviour
{
    [Header("Throwing")]
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

    [SerializeField, Range(0, 1)]
    float forceRandomness = 0.5f;

    //Score
    Score score;

    //Throwing
    float throwingStrength = 0;
    bool isForceChargingUp = true;
    Vector3 throwingDirection = Vector3.zero;

    //State variables of throwing
    bool isThrown = false;
    Vector3 throwingForce = Vector3.zero;
    public Vector3 ThrowingForce { get => throwingForce; private set => throwingForce = value; }

    //Initial 
    Vector3 initialBallPos;
    List<(Vector3 pos, Vector3 eulerAngle)> initialPinTransforms = new List<(Vector3 pos, Vector3 eulerAngle)>();

    //Network variables
    Vector3 networkForce = Vector3.zero, oldNetworkForce = Vector3.zero;
    Vector3 networkPosition = Vector3.zero, oldNetworkPosition = Vector3.zero;

    //Events
    public delegate void ThrowEvent();
    public ThrowEvent OnThrow;
    public ThrowEvent OnReset;



    #region Static methods
    public static Vector3 ToVector3(SerializableVector3 sVector3) => new Vector3 (sVector3.x, sVector3.y, sVector3.z);
    #endregion


    #region Mono Behaviour
    private void Start()
    {
        throwingDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-forceRandomness, forceRandomness)).normalized;

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
        HandleTCPMessages();

        if (!isThrown)
        {
            //Change ball positionString
            ChangeBallPosition();

            //Change throwing angle
            if (Input.GetKey(KeyCode.LeftArrow))
                throwingDirection += new Vector3(0, 0, angleChangeSpeed * Time.deltaTime);
            else if (Input.GetKey(KeyCode.RightArrow))
                throwingDirection += new Vector3(0, 0, -angleChangeSpeed * Time.deltaTime);

            throwingDirection = new Vector3(0, 0, Mathf.Clamp(throwingDirection.z, minAngle, maxAngle));
            RotateForceCanvas(forceCanvas, throwingDirection);
        }

        //Throw
        if (Input.GetKey(KeyCode.UpArrow) && !isThrown)
        {
            //Decide the vector3 charging up or down
            if (throwingStrength > maxThrowingForce + forceChangeSpeed * Time.deltaTime)
                isForceChargingUp = false;
            else if (throwingStrength < 0 - forceChangeSpeed * Time.deltaTime)
                isForceChargingUp = true;

            //Charge the vector3
            if (isForceChargingUp)
            {
                throwingStrength += forceChangeSpeed * Time.deltaTime;
            }
            else if (!isForceChargingUp)
            {
                throwingStrength -= forceChangeSpeed * Time.deltaTime;
            }

            forceSlider.value = throwingStrength / maxThrowingForce;
        }
        else if (Input.GetKeyUp(KeyCode.UpArrow) && !isThrown)
        {
            isThrown = true;
            forceSlider.gameObject.SetActive(false);

            ballAnimator.Play("Anticipation");
            ballRB.useGravity = false;

            StartCoroutine(DelayedInvoke(1, ThrowBall));
        }

        //Reset if press space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetThrowing(score.ShouldHardReset);
        }
    }

    private void OnEnable()
    {
        Client.Instance.OnReceiveMessage += OnReceiveMessage;
    }

    private void OnDisable()
    {
        Client.Instance.OnReceiveMessage -= OnReceiveMessage;
    }
    #endregion



    #region On Receive TCP Message, and Handle them (this is 2 different parts)
    private void OnReceiveMessage()
    {
        OnForceMsg();
        OnPositionMsg();
    }

    private void OnPositionMsg()
    {
        if (Client.Instance.LatestMsg.StartsWith("/position"))
        {
            string positionString = Client.Instance.LatestMsg.Substring(9).Trim();
            Debug.Log("position " + positionString);

            networkPosition = ToVector3(JsonConvert.DeserializeObject<SerializableVector3>(positionString));
        }
    }

    private void OnForceMsg()
    {
        if (Client.Instance.LatestMsg.StartsWith("/force"))
        {
            string forceString = Client.Instance.LatestMsg.Substring(6).Trim();
            Debug.Log("force " + forceString);

            networkForce = ToVector3(JsonConvert.DeserializeObject<SerializableVector3>(forceString));
        }
    }

    private void HandleTCPMessages()
    {
        //Handle position
        if (networkPosition != oldNetworkPosition)
        {
            ballRB.position = networkPosition;
            oldNetworkPosition = networkPosition;
        }

        //Handle force
        if (networkForce != oldNetworkForce)
        {
            ThrowBall(networkForce);
            oldNetworkForce = networkForce; 
        }
    }
    #endregion



    #region Throwing
    private void ChangeBallPosition()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            if (Input.GetKey(KeyCode.A))
                ballRB.transform.position += new Vector3(0, 0, moveSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.D))
                ballRB.transform.position += new Vector3(0, 0, -moveSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.W))
                ballRB.transform.position += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
            if (Input.GetKey(KeyCode.S))
                ballRB.transform.position += new Vector3(-moveSpeed * Time.deltaTime, 0, 0);

            ballRB.transform.position = new Vector3(
                Mathf.Clamp(ballRB.transform.position.x, initialBallPos.x - moveBound, initialBallPos.x + moveBound),
                ballRB.transform.position.y,
                Mathf.Clamp(ballRB.transform.position.z, initialBallPos.z - moveBound, initialBallPos.z + moveBound)
                );

            var ballPosJson = JsonConvert.SerializeObject(new SerializableVector3(ballRB.position));
            Client.Instance.SendTCPMessage("/position " + ballPosJson);
        }
    }

    public void ResetThrowing(bool shouldHardReset)
    {
        StopAllCoroutines();

        ballRB.transform.position = initialBallPos;
        ballRB.velocity = new Vector3(0, 0, 0);
        ballRB.angularVelocity = new Vector3(0, 0, 0);

        throwingStrength = 0;
        throwingDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-forceRandomness, forceRandomness)).normalized;

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
        forceSlider.value = throwingStrength / maxThrowingForce;

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

    private void ThrowBall()
    {
        ballRB.useGravity = true;

        throwingForce = (throwingDirection + new Vector3(1, 0, 0)).normalized * throwingStrength;
        ThrowBall(throwingForce);

        // Serialize the dictionary to a JSON string
        var throwingForceJson = JsonConvert.SerializeObject(new SerializableVector3(throwingForce));

        Client.Instance.SendTCPMessage("/force " + throwingForceJson);

        if (OnThrow != null)
            OnThrow.Invoke();
    }

    private void ThrowBall(Vector3 throwingForce)
    {
        ballRB.AddForce(throwingForce);

        ballAnimator.Play("Idle");

        StartCoroutine(DelayedInvoke(resetDelay, () => ResetThrowing(score.ShouldHardReset)));
    }
    #endregion
}

[Serializable]
public class SerializableVector3{
    public float x=0, y=0, z=0;
    public SerializableVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
}
