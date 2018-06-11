using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulatorController : MonoBehaviour {
    // Steam VR input stuff
    private SteamVR_TrackedController controller;
    private int controllerIndex;
    private SteamVR_TrackedObject trackedObject;
    private SteamVR_Controller.Device device { get { return SteamVR_Controller.Input((int)trackedObject.index); } }

    // Stuff for grabbing objects
    private HashSet<InteractiveObjectController> availableObjects;
    private InteractiveObjectController closestObject;
    private Transform interactionPoint;
    private Transform referencePoint;
    private InteractiveObjectController grabbedObject; // The object this manipulator is currently in control of
    bool left; // whether this is the left controller
    public float transitionRate = 10.0f;
    public float transitionThreshold = 0.1f;

    // input bools to prevent constant grabbing, trigger pulls, etc...
    bool grabStop;
    bool triggerStop;

    public VRCharacterController character; // The character controller for character that owns this controller


    // Use this for initialization
    void Start () {
        trackedObject = this.GetComponent<SteamVR_TrackedObject>();
        controller = this.GetComponent<SteamVR_TrackedController>();
        controllerIndex = (int)this.GetComponent<SteamVR_TrackedObject>().index;
        left = (this.name.Equals("Controller (left)"));
        grabbedObject = null;
        availableObjects = new HashSet<InteractiveObjectController>();
        interactionPoint = new GameObject().transform;
        interactionPoint.gameObject.name = "Interaction Point " + controllerIndex;
        referencePoint = new GameObject().transform;
        referencePoint.gameObject.name = "Reference Point " + controllerIndex;
        grabStop = false;
    }
	
	// Update is called once per frame
	void FixedUpdate () {

        // check if there are any available objects.
        FindClosestAvailableObject();
        
        // grip button stuff
        if (controller.gripped)
        {
            if (grabbedObject)
            {
                MoveGrabbedObject();
            }
            else if (!grabStop)
            {
                GrabObject(closestObject);
                grabStop = true;
            }
        }
        else if(grabbedObject)
        {
            ReleaseObject();
            grabStop = false;
        }
        else if(grabStop)
        {
            grabStop = false;
        }

        // trigger stuff
        if(controller.triggerPressed && !triggerStop)
        {
            if(grabbedObject)
            {
                if(grabbedObject.GetAttached())
                {
                    grabbedObject.Detach();
                }
            }

            triggerStop = true;
        }
        else if(triggerStop)
        {
            triggerStop = false;
        }

        // controller pad stuff
        if (controller.padPressed)
        {
            //Debug.Log(device.GetAxis().x + " " + device.GetAxis().y);
            character.MoveBody(device.GetAxis().x, device.GetAxis().y);
        }
        
    }


    // helpers



    private void FindClosestAvailableObject()
    {
        if (availableObjects.Count > 0)
        {
            float closestDistance = float.MaxValue;
            InteractiveObjectController newClosestObject = null;

            foreach (InteractiveObjectController item in availableObjects)
            {
                if(!item) { break; } // check for null pointer nonsense from unity


                float currentDistance = (item.transform.position - transform.position).sqrMagnitude;

                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    newClosestObject = item;
                }
                
                closestObject = newClosestObject;
            }
        }
        else if (closestObject)
        {
            closestObject = null;
        }
    }
    


    public void GrabObject(InteractiveObjectController objectToGrab)
    {
        if (objectToGrab)
        {
            // if the object is already grabbed, have the hand that's grabbing it release its hold, and grab it with this hand
            if (objectToGrab.GetGrabbed())
            {
                objectToGrab.GetAttachedController().ReleaseObject();
            }

            switch (objectToGrab.moveMode)
            {
                case 0:
                    break;

                case 1:
                    GrabPhysics(objectToGrab);
                    break;

                case 2:
                    GrabTransition(objectToGrab);
                    break;

                default:
                    Debug.LogError("Bad MoveMode on object to grab!");
                    break;
                    
            }

            objectToGrab.Grab(this);
            grabbedObject = objectToGrab;
        }
    }

    public void GrabPhysics(InteractiveObjectController objectToGrab)
    {
        // if the object is attached, check if the root is grabbable. If root is grabbable, but hasn't been grabbed, grab it now with this hand
        if (objectToGrab.GetAttached())
        {
            InteractiveObjectController root = objectToGrab.GetRootObject();

            if (root.grabbable && !root.grabbed && !root.attached)
            {
                objectToGrab = root;
            }
        }

        // if the object has a grab handle, grab it by that
        SetInteractionPoint(objectToGrab);
    }

   


    public void SetInteractionPoint(InteractiveObjectController objectToGrab)
    {
        Transform newPoint;

        if(objectToGrab.grabHandle)
        {
            objectToGrab.transform.rotation = Quaternion.Lerp(this.transform.rotation, objectToGrab.grabHandle.rotation, Time.deltaTime * 1);
            newPoint = objectToGrab.grabHandle;
        }
        else
        {
            newPoint = this.transform;
        }

        interactionPoint.SetParent(null);
        interactionPoint.position = newPoint.position;
        interactionPoint.rotation = newPoint.rotation;
        interactionPoint.SetParent(objectToGrab.transform);
    }

    private void GrabTransition(InteractiveObjectController objectToGrab)
    {
        SetInteractionPoint(objectToGrab);
        SetReferencePoint(objectToGrab);
        objectToGrab.PhysicsOff();
    }

    private void SetReferencePoint(InteractiveObjectController objectToGrab)
    {
        referencePoint.position = interactionPoint.position;
        referencePoint.rotation = interactionPoint.rotation;
        objectToGrab.transform.SetParent(referencePoint);
    }

    private void MoveTransition(Transform target)
    {
        if (GetDistance(referencePoint, this.transform) <= transitionThreshold)
        {
            referencePoint.position = this.transform.position;
            referencePoint.rotation = this.transform.rotation;
            grabbedObject.transform.SetParent(null);
            grabbedObject.PhysicsOn();
            grabbedObject.moveMode = 1;
            GrabObject(grabbedObject);
        }
        else
        {
            referencePoint.position = Vector3.Lerp(referencePoint.position, target.position, Time.deltaTime * transitionRate);
            referencePoint.rotation = Quaternion.Lerp(referencePoint.rotation, target.rotation, Time.deltaTime * transitionRate);
        }
    }

    private float GetDistance(Transform a, Transform b)
    {
        return Vector3.Magnitude(a.position - b.position);
    }



    public void ReleaseObject()
    {
        if (grabbedObject)
        {
            grabbedObject.PhysicsOn();
            grabbedObject.moveMode = 1;
            grabbedObject.Release();
            grabbedObject = null;
        }
    }



    private void MoveGrabbedObject()
    {
        if(grabbedObject)
        {
            switch (grabbedObject.moveMode)
            {
                case 0:
                    MoveAttached();
                    break;

                case 1:
                    MovePhysics();
                    break;

                default:
                    Debug.LogError("Invalid move mode on grabbed object");
                    break;
            }
        }
    }



    private void MoveAttached()
    {
        // if the root object controlling the grabbed object is released, grab it with this hand
        InteractiveObjectController root = grabbedObject.GetRootObject();

        if(root && root.grabbable && !root.GetGrabbed())
        {
            ReleaseObject();
            GrabObject(root);
        }
    }



    private void MovePhysics()
    {
        //CheckInteractionPointPosition();

        float angle;
        Vector3 axis;
        Vector3 posDelta;
        Quaternion rotDelta;
        Rigidbody rBody = grabbedObject.GetRigidbody();

        //establish the direction the grabbed object needs to be moving in, in order to place it "in" the grabbing manipulator
        posDelta = this.transform.position - interactionPoint.position;
        //set the grabbed object's velocity so that it is constantly moving towards where it should be as a grabbed object

        rBody.velocity = posDelta * grabbedObject.GetCalculatedVelocityFactor() * Time.fixedDeltaTime;
        

        rotDelta = this.transform.rotation * Quaternion.Inverse(interactionPoint.rotation);
        rotDelta.ToAngleAxis(out angle, out axis);

        if (angle > 180)
        {
            angle -= 360;
        }

        //make sure the object isn't already in the appropriate rotation
        if (angle != 0 && axis != Vector3.zero)
        {
            //set the object's angular velocity so that the object is rotating towards where it should be as a grabbed object
            rBody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * grabbedObject.GetCalculatedRotationFactor();
        }
    }



    private void CheckInteractionPointPosition()
    {
        float speed = 10;

        if(Vector3.Magnitude(interactionPoint.position - referencePoint.position) > grabbedObject.gripPlayDistance)
        {
            interactionPoint.position = Vector3.Lerp(interactionPoint.position, referencePoint.position, Time.deltaTime * speed);
            interactionPoint.rotation = Quaternion.Lerp(interactionPoint.rotation, referencePoint.rotation, Time.deltaTime * speed);
        }
    }
    

    private void OnTriggerEnter(Collider other)
    {
        InteractiveObjectController otherObject = other.GetComponent<InteractiveObjectController>();
        
        if(otherObject && otherObject.grabbable)
        {
            availableObjects.Add(otherObject);
        }
    }



    private void OnTriggerExit(Collider other)
    {
        InteractiveObjectController otherObject = other.GetComponent<InteractiveObjectController>();

        if (otherObject)
        {
            availableObjects.Remove(otherObject);
        }
    }



    // getters



    public int GetControllerIndex()
    {
        return controllerIndex;
    }
    
    public InteractiveObjectController GetGrabbedObject()
    {
        return grabbedObject;
    }

    public Transform GetInteractionPoint()
    {
        return interactionPoint;
    }

    public InteractiveObjectController GetClosest()
    {
        return closestObject;
    }

    public bool IsAvailableToController(InteractiveObjectController objectToCheck)
    {
        return availableObjects.Contains(objectToCheck);
    }


    // setters
}
