using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRCharacterController : MonoBehaviour {

    public CharacterController playerCharacter;
    public float playerMoveSpeed = 1.0f;
    public float playerRotateSpeed = 1.0f;
    Transform groundChecker;
    public float groundDistance = 0.1f;
    public Transform testTransform;
    public float movementThreshold = 0.1f;
    public Vector3 targetPosition;
    public float resyncSpeed = 10;
    public float resyncThreshold = 0.5f;
    public float stepSpeed = 1.0f;

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
        groundChecker = new GameObject().transform;
        //bodyCollider.radius = bodyRadius;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        //SetBodyCollision();

        MoveCharacter();
        CheckVerticalAllignment();
        CheckCharacterPositionSync();
    }
    
    public void MoveBody(float x, float y)
    {
        float rotateSpeed = Time.fixedDeltaTime * 10;
        Vector3 forward = new Vector3(headTransform.transform.forward.x, 0, headTransform.transform.forward.z);
        this.transform.Translate(forward * Time.fixedDeltaTime * y * moveSpeedMultiplier, Space.World);
        this.transform.RotateAround(headTransform.position, new Vector3(0, x, 0), rotateSpeed * rotateSpeedMultiplier);
    }

    public void SetBodyCollision()
    {
        //bodyCollider.height = headTransform.localPosition.y;
        Vector3 bodyPosition = new Vector3(headTransform.localPosition.x, headTransform.localPosition.y / 2, headTransform.localPosition.z);
    }

    public Transform GetHeadTransform()
    {
        return headTransform;
    }

    private void MoveCharacter()
    {
        Quaternion targetRotation = Quaternion.Euler(0, headTransform.rotation.eulerAngles.y, 0);
        playerCharacter.transform.rotation = Quaternion.Lerp(playerCharacter.transform.rotation, targetRotation, playerRotateSpeed);

        Vector3 targetPosition = new Vector3(headTransform.position.x, playerCharacter.transform.position.y, headTransform.position.z);
        Vector3 direction = targetPosition - playerCharacter.transform.position;

        if (direction.magnitude >= movementThreshold)
        {
            Vector3 moveTo;
            direction.Normalize();
            //float y = headTransform.position.y - playerCharacter.GetComponent<CapsuleCollider>().height / 2;

            if (playerCharacter.isGrounded) //(CheckGrounded())
            {
                moveTo = direction * playerMoveSpeed;
            }
            else
            {
                moveTo = new Vector3(0, Physics.gravity.y, 0);
            }

            playerCharacter.Move(moveTo * Time.deltaTime);
        }
    }

    private bool CheckGrounded()
    {
        int layerMask = 1 << 11;
        groundChecker.position = new Vector3(headTransform.position.x, this.transform.position.y, headTransform.position.z);
        return Physics.CheckSphere(groundChecker.position, groundDistance, layerMask, QueryTriggerInteraction.Ignore);
    }
  
    private void CheckCharacterPositionSync()
    {
        Vector3 thisPosition = this.transform.position;
        Vector3 headPosition = headTransform.position;
        Vector3 charPosition = new Vector3(playerCharacter.transform.position.x, headPosition.y, playerCharacter.transform.position.z);
        Vector3 headCharDiff = charPosition - headPosition;
        float headCharDist = headCharDiff.magnitude;
            // target - current
        if (headCharDist > 0.25)
        {
            Debug.Log("Sync distance: " + headCharDist);
            Vector3 newPosition = this.transform.position + headCharDiff;
            this.transform.position = Vector3.Lerp(this.transform.position, newPosition, Time.deltaTime * resyncSpeed);
        }

    }

    private void CheckVerticalAllignment()
    {
        //float y = playerCharacter.transform.position.y - (playerCharacter.GetComponent<CapsuleCollider>().height / 2);
        float y = playerCharacter.transform.position.y - playerCharacter.height / 2;

        if(this.transform.position.y != y)
        {
            //this.transform.position = new Vector3(this.transform.position.x, y, this.transform.position.z);
            Vector3 newPosition = new Vector3(this.transform.position.x, y, this.transform.position.z);
            this.transform.position = Vector3.Lerp(this.transform.position, newPosition, Time.fixedDeltaTime * stepSpeed);
        }
    }
}
