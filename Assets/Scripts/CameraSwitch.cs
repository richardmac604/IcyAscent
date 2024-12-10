using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour {
    public CinemachineVirtualCamera virtualCamera;
    public Transform playerTransform;

    private float switchHeight = 10f;
    private bool switched = false;

    void Update() {
        if (!switched && playerTransform.position.y >= switchHeight) {
            virtualCamera.Priority = 10;
            switched = true;
        }
    }
}
