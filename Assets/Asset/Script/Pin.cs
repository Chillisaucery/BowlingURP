using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pin : MonoBehaviour
{
    [SerializeField, Tooltip("If the top point yPos reach below this, the pin falled")]
    float fallThresold = 0.25f;

    Score score;
    bool isFallen = false;

    public bool IsFallen { get => isFallen; private set => isFallen = value; }

    private void Start()
    {
        score = FindObjectOfType<Score>();

        IsFallen = false;
    }

    void OnTriggerEnter(Collider other)
    {
       if (other.tag == "Ground" && !IsFallen)
       {
            score.OnPinFall();
            IsFallen = true;
       } 
    }

    public void ResetFallen()
    {
        IsFallen=false;
    }
}
