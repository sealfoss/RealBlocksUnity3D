using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCharacterController : MonoBehaviour {
    
    public Transform headTransform;
    public Transform positionMaster;
    public float rotateSpeedMultiplier = 1;
    public float moveSpeedMultiplier = 1;
    //public CapsuleCollider bodyCollider;
    public float bodyRadius = 0.8f;

    public int controllerIndex;
    //public GameObject leftHand;
    //public GameObject rightHand;

    // Use this for initialization
    void Start () {
        //bodyCollider.radius = bodyRadius;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        //SetBodyCollision();
    }
    
    public void MoveBody(float x, float y)
    {
        float rotateSpeed = Time.deltaTime * 10;
        Vector3 forward = new Vector3(headTransform.transform.forward.x, 0, headTransform.transform.forward.z);
        this.transform.Translate(forward * Time.deltaTime * y * moveSpeedMultiplier, Space.World);
        this.transform.RotateAround(headTransform.position, new Vector3(0, x, 0), rotateSpeed * rotateSpeedMultiplier);
    }

    public void SetBodyCollision()
    {
        //bodyCollider.height = headTransform.localPosition.y;
        Vector3 bodyPosition = new Vector3(headTransform.localPosition.x, headTransform.localPosition.y / 2, headTransform.localPosition.z);
    }

  
}
