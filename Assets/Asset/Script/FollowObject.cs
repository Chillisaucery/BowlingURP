using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    [SerializeField]
    Transform followee;

    Vector3 offset = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        offset = followee.position - transform.position;
    }

    private void Update()
    {
        transform.position = followee.position - offset;
    }
}
