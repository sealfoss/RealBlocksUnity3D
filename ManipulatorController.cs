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
    private InteractiveObjectController grabbedObject; // The object this manipulator is currently in control of
    bool left; // whether this is the left controller
    public float transitionRate = 10.0f;
    public float transitionThreshold = 0.1f;
    private Transform moveTarget; // where you're moving the grabbed object to.
    private Transform transitionGuide; // moves the grabbed object when transitioning

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
        grabStop = false;
        transitionGuide = new GameObject().transform;
        transitionGuide.name = "Transition Guide" + controllerIndex;
    }

    // Update is called once per frame
    private void Update()
    {
        // controller pad stuff
        if (controller.padPressed)
        {
            //Debug.Log(device.GetAxis().x + " " + device.GetAxis().y);
            character.MoveBody(device.GetAxis().x, device.GetAxis().y);
        }
    }

    void FixedUpdate () {
        
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
        else
        {
            if (grabbedObject)
            {
                ReleaseObject();
            }

            if (grabStop)
            {
                grabStop = false;
            }
            
            // check if there are any available objects.
            FindClosestAvailableObject();
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

       
        
    }


    // helpers

    private void SetGrabbed(InteractiveObjectController newGrabbed)
    {
        grabbedObject = newGrabbed;
    }

    private void SetClosest(InteractiveObjectController newClosest)
    {
        closestObject = newClosest;

        if(closestObject)
        {
            closestObject.SelectThisObject(this);
        }
    }

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

                SetClosest(newClosestObject);
            }
        }
        else if (closestObject)
        {
            SetClosest(null);
        }
    }


    public void GrabObject(InteractiveObjectController objectToGrab)
    {
        if (objectToGrab)
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

            // if the object to grab is already grab, tell the other hand to let go
            if (objectToGrab.grabbed)
            {
                objectToGrab.attachedController.ReleaseObject();
            }
            
            SetInteractionPoint(objectToGrab); // if the object has a grab handle, grab it by that. otherwise, grab it by the position the controller is current in relative to the object
            SetMoveTarget(this.transform); // set the move target to the grabbing hand
            SetGrabbed(objectToGrab);
            grabbedObject.Grab(this);
        }
    }

   



    private void MoveGrabbedObject()
    {
        if(grabbedObject)
        {
            switch (grabbedObject.GetMoveMode())
            {
                case 0:
                    // do nothing
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
        posDelta = moveTarget.position - interactionPoint.position;
        //set the grabbed object's velocity so that it is constantly moving towards where it should be as a grabbed object

        rBody.velocity = posDelta * grabbedObject.GetCalculatedVelocityFactor() * Time.fixedDeltaTime;
        

        rotDelta = moveTarget.rotation * Quaternion.Inverse(interactionPoint.rotation);
        rotDelta.ToAngleAxis(out angle, out axis);

        if (angle > 180)
        {
            angle -= 360;
        }

        //make sure the object isn't already in the appropriate rotation
        if (angle != 0 && axis != Vector3.zero)
        {
            //set the object's angular velocity so that the object is rotating towards where it should be as a grabbed object
            Vector3 angularVelocity = (Time.fixedDeltaTime * angle * axis) * grabbedObject.GetCalculatedRotationFactor();

            // need to check to make sure angular and reg velocity aren't infinity or some other non valid value.

            rBody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * grabbedObject.GetCalculatedRotationFactor();
        }
    }


    private void MoveTransition()
    {
        transitionGuide.position = Vector3.Lerp(transitionGuide.position, moveTarget.position, Time.deltaTime * transitionRate);
        transitionGuide.rotation = Quaternion.Lerp(transitionGuide.rotation, moveTarget.rotation, Time.deltaTime * transitionRate);
    }
    


    public void SetInteractionPoint(InteractiveObjectController objectToGrab)
    {
        Transform newPoint;

        if (objectToGrab.grabHandle)
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

    public void SetTransitionGuide(Transform guidePoint)
    {
        if (guidePoint)
        {
            transitionGuide.SetPositionAndRotation(guidePoint.position, guidePoint.rotation);

            if (grabbedObject)
            {
                grabbedObject.transform.SetParent(transitionGuide);
            }
        }
        else
        {
            if (grabbedObject)
            {
                grabbedObject.transform.SetParent(null);
            }
        }
    }




    public void ReleaseObject()
    {
        if (grabbedObject)
        {
            grabbedObject.Release();

            SetGrabbed(null);
        }
    }




    private float GetDistance(Transform a, Transform b)
    {
        return Vector3.Magnitude(a.position - b.position);
    }




    private void OnTriggerEnter(Collider other)
    {
        InteractiveObjectController otherObject = other.GetComponent<InteractiveObjectController>();
        
        if(otherObject && otherObject.grabbable && !availableObjects.Contains(otherObject))
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

    public Transform GetTransitionGuide()
    {
        return transitionGuide;
    }


    // setters

    public void SetMoveTarget(Transform newTarget)
    {
        moveTarget = newTarget;
    }
    
}
