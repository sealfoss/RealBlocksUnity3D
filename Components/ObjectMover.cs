using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMover : MonoBehaviour {

    public int moveMode = 1;
    public bool twoHanded = false;
    private Transform moveTarget = null;
    public bool grabbed = false;
    IObjectController objectController { get { return GetComponent<IObjectController>(); } }
    
    private HandComponent grabbedBy = null;
    private Transform interactionPoint = null;
    public Transform grabHandle = null;

    // physics move stuff
    //public float maxVelocity = 5000.0f;
    public float maxGrabDistance = 0.3f;
    private Rigidbody thisRigidbody { get { return GetComponent<Rigidbody>(); } }

    // lerp move stuff
    public float moveSpeed = 1.0f;
    public float rotationSpeed = 1.0f;

    // Use this for initialization
    void Start()
    {
        thisRigidbody.maxAngularVelocity = Mathf.Infinity;
        interactionPoint = new GameObject().transform;
        interactionPoint.SetParent(this.transform);
        interactionPoint.name = this.name + " Interaction Point";
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (grabbed)
        {
            MoveObject();
        }
    }

    public void SetMoveMode(int newMode)
    {
        if (objectController != null)
        {
            switch (newMode)
            {
                case 0:
                    objectController.DestroyRigidbody();
                    break;

                case 1:
                    this.transform.SetParent(null);
                    objectController.InitializeRigidbody();
                    objectController.EnablePhysics();
                    objectController.EnableCollision();
                    break;

                case 2:
                    objectController.InitializeRigidbody();
                    objectController.KillPhysics();
                    objectController.KillCollision();
                    break;

                default:
                    break;
            }
        }

        moveMode = newMode;
    }

    public void Grab(HandComponent grabbing)
    {
        grabbedBy = grabbing;
        Transform grabPoint;

        if(grabHandle) { grabPoint = grabHandle; }
        else { grabPoint = grabbedBy.transform; }

        switch (moveMode)
        {
            case 0:
                // attached
                break;

            case 1:
                GrabPhysics(grabPoint);
                break;

            case 2:
                GrabLerp(grabPoint);
                break;

            default:
                break;
        }

        grabbed = true;
        //Debug.Log("Object: " + this.name + " , Grabbed By: " + grabbedBy.name);
    }

    public void MoveObject()
    {

        switch (moveMode)
        {
            case 0:
                // attached
                break;

            case 1:
                MovePhysics();
                break;

            case 2:
                MoveLerp();
                break;

            default:
                break;
        }
    }

    public void Release()
    {
        //Debug.Log("Object: " + this.name + " has called Release()");

        switch (moveMode)
        {
            case 0:
                // attached
                break;

            case 1:
                ReleasePhysics();
                break;

            case 2:
                ReleaseLerp();
                break;

            default:
                break;
        }

        grabbed = false;
        grabbedBy = null;
        SetMoveMode(1);
    }


    public void GrabPhysics(Transform grabPoint)
    {
        interactionPoint.SetPositionAndRotation(grabPoint.position, grabPoint.rotation);
        moveTarget = grabbedBy.transform;
        grabbed = true;
    }

    public void ReleasePhysics()
    {
        moveTarget = null;
        grabbed = false;
    }

    public void MovePhysics()
    {
        float angle;
        Vector3 axis;
        Vector3 posDelta;
        Quaternion rotDelta;

        posDelta = moveTarget.position - interactionPoint.position;
        thisRigidbody.velocity = posDelta * (grabbedBy.GetVelocityFactor() / thisRigidbody.mass) * Time.fixedDeltaTime;
        //thisRigidbody.velocity = Vector3.ClampMagnitude(thisRigidbody.velocity, maxVelocity);

        rotDelta = moveTarget.rotation * Quaternion.Inverse(interactionPoint.rotation);
        rotDelta.ToAngleAxis(out angle, out axis);

        if (angle > 180) { angle -= 360; }
            
        if (angle != 0 && axis != Vector3.zero)
        {
            thisRigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * (grabbedBy.GetAngularVelocityFactor() / thisRigidbody.mass);
        }
    }

    public void GrabLerp(Transform grabPoint)
    {
        // do nothing
    }

    public void ReleaseLerp()
    {
        SetMoveMode(1);
    }

    public void MoveLerp()
    {
        // do nothing
    }

    public void SetTarget(Transform newTarget)
    {
        moveTarget = newTarget;
    }

    public Transform GetTarget()
    {
        return moveTarget;
    }

    public Transform GetTransform()
    {
        return this.transform;
    }

    public IObjectController GetObjectController()
    {
        return this.GetComponent<IObjectController>();
    }

    public bool GetGrabbed()
    {
        return grabbed;
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public bool GetTwoHanded()
    {
        return twoHanded;
    }

    public HandComponent GetGrabbedBy()
    {
        return grabbedBy;
    }
}
