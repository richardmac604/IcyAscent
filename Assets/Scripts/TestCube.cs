using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestCube : MonoBehaviour {
    private float rayDistance = 2f;
    public LayerMask easyClimbLayer;
    [SerializeField] GameObject HingeObject;
    private float speed = 100f;
    private bool rayHit;
    private HingeJoint joint;

    private void Update() {
         RaycastHit hitPoint = new RaycastHit();
         rayHit = Input.GetMouseButton(0) && Physics.Raycast(gameObject.transform.position, transform.forward, out hitPoint, rayDistance, easyClimbLayer);
        if (rayHit) {
            Debug.Log("RayHit");
            if (joint == null) {
            Debug.Log("CreatingJoint");
                // Create a joint for the left hand and anchor it to the hit point on the wall
                joint = gameObject.AddComponent<HingeJoint>();
                joint.axis = Vector3.forward;
                //joint.connectedBody = rayHit.transform.gameObject.GetComponent<Rigidbody>();  // No connected body, fixed to the wall
                joint.autoConfigureConnectedAnchor = true;
                //joint.anchor = hitPoint.point;  // Anchor at the hand's current position
                joint.connectedAnchor = hitPoint.point;  // Hand is anchored to the hit point on the wall
            }

            Debug.Log("Holding mouse down");
            float horizontalInput = Input.GetAxis("Mouse X") * speed;
            horizontalInput *= Time.deltaTime;

            Debug.Log(horizontalInput);

            HingeObject.GetComponent<Rigidbody>().AddForce(horizontalInput, 0f,0f);
        } else if (Input.GetMouseButtonUp(0)) {
            Destroy(HingeObject.GetComponent<HingeJoint>());
        }
    }


}
