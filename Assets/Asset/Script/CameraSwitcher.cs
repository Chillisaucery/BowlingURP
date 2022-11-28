using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [SerializeField]
    Transform ball, leadPin;

    [SerializeField]
    Camera camDefault, camMidway, camNear, camFar;

    [SerializeField, Range(0f, 2f)]
    float defaultBound = 0.9f,
            midwayBound = 0.6f,
            camNearBound = 0.05f,
            camFarBound = 1.2f;

    float initialDistance = 0;
    float currentDistance = 0;

    Vector3 initialBallPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        initialDistance = (ball.position - leadPin.position).magnitude;
        initialBallPos = ball.position;
    }

    void FixedUpdate()
    {
        currentDistance = (ball.position - leadPin.position).magnitude;

        if (currentDistance >= defaultBound * initialDistance)
        {
            SwitchCamera(camDefault);
        }
        else if (currentDistance >= midwayBound * initialDistance)
        {
            SwitchCamera(camMidway);
        }
        else
        {
            SwitchCamera(camNear);
        }

        //Debug.Log((ball.position - initialBallPos).magnitude / initialDistance);

        if ((ball.position - initialBallPos).magnitude >= camFarBound * initialDistance)
        {
            SwitchCamera(camFar);
        }
    }

    private void SwitchCamera(Camera activeCamera)
    {
        camDefault.depth = 0;
        camMidway.depth = 0;
        camNear.depth = 0;
        camFar.depth = 0;

        activeCamera.depth = 1;
    }
}
