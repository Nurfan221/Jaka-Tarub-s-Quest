using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour // Follow plyaer with damping
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new(0, 0, -10);
    [SerializeField] float damping;

    Vector3 velocity = Vector3.zero;
    
    private void LateUpdate()
    {
        Vector3 movePos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, movePos, ref velocity, damping);
    }
}
