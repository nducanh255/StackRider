using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private float smoothTime;
    private Vector3 offset;
    private Vector3 speed;

    private void Awake() {
        offset = this.transform.position - target.transform.position;
        speed = Vector3.zero;
    }

    private void LateUpdate() {
        Vector3 targetPos = target.transform.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref speed, smoothTime);
    }
}
