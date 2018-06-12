using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentPointController : MonoBehaviour {

    // public variables for configuring the attachment point
    public string attachmentTypeName = "";

    // trigger stuff
    private InteractiveObjectController owningObject { get { return GetComponentInParent<InteractiveObjectController>(); ; } }
    public InteractiveObjectController attachingObject;
    public ManipulatorController attachingController;
    public AttachmentTriggerController attachingTrigger;
    public bool activated;

    //tracking stuff
    public Transform controllerTracker;
    public Transform trackerOffset;
    public Transform objectGuide;
    public float transitionMoveSpeed = 1;
    public float transitionRotateSpeed = 1;

    // new stuff
    public bool attachStop;
    public float attachStopReset = 0.025f;
    public float attachPositionThreshold = 0.1f;
    public float attachRotationThreshold = 1.0f;
    public float attachDistance = 0.01f;
    public float disengagementDistance = 0.09f;


	// Use this for initialization
	void Start () {
        //owningObject = GetComponentInParent<InteractiveObjectController>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
		if(activated)
        {
            TrackController();
            TransitionMoveObject();

            if (DetectDeactivation())
            {
                Deactivate();
            }
            else if(DetectAttach())
            {
                AttachObject();
            }
        }
	}



    private void TrackController()
    {
        controllerTracker.position = attachingController.transform.position;
        controllerTracker.localPosition = new Vector3(0, controllerTracker.localPosition.y, 0);
    }



    private void TransitionMoveObject()
    {
        objectGuide.position = Vector3.Lerp(objectGuide.position, trackerOffset.position, transitionMoveSpeed * Time.deltaTime);
        objectGuide.rotation = Quaternion.Lerp(objectGuide.rotation, trackerOffset.rotation, transitionRotateSpeed * Time.deltaTime);

        if (objectGuide.localPosition.y < attachDistance)
        {
            objectGuide.localPosition = new Vector3(objectGuide.localPosition.x, attachDistance, objectGuide.localPosition.z);
        }
    }


    private bool DetectDeactivation()
    {
        bool deactivate = false;
        // detach if...

        if (!attachingObject.GetGrabbed()) // if the attaching object isn't grabbed
        {
            deactivate = true;
        }
        else if (owningObject.grabbable) // if the owning object is grabbable, and isn't currently grabbed or attached
        {
            if (!owningObject.GetGrabbed() && !owningObject.GetAttached())
            {
                deactivate = true;
            }
        }

        if (GetDistance(attachingTrigger.transform, this.transform) > disengagementDistance) // or if the attaching object is beyond the disengagement distance
        {
            deactivate = true;
        }

        return deactivate;
    }



    private bool DetectAttach()
    {
        bool attach = false;

        if (attachStop)
        {
            if(objectGuide.localPosition.y >= attachStopReset)
            {
                attachStop = false;
            }
        }
        else
        {
            if (objectGuide.localPosition.y <= attachDistance)
            {
                if (GetDistance(objectGuide, trackerOffset) < attachPositionThreshold
                    && Quaternion.Angle(objectGuide.rotation, trackerOffset.rotation) < attachRotationThreshold)
                {
                    attach = true;
                }
            }
        }

        return attach;
    }
    


    public void Activate(AttachmentTriggerController trigger)
    {
        //Debug.Log("Activating!");

        attachingTrigger = trigger;
        attachingObject = GetAttachingObject(attachingTrigger);
        attachingController = GetAttachingController(attachingTrigger);

        if(attachingController && attachingObject && attachingController)
        {
            Physics.IgnoreCollision(owningObject.GetComponent<Collider>(), attachingObject.GetComponent<Collider>(), true);
            attachingObject.SetMoveMode(0);

            SetInitialTrackingPositions();
            attachingObject.transform.SetParent(objectGuide);

            attachingObject.busy = true;
            owningObject.busy = true;
            activated = true;
        }

        
    }
    


    public void ReActivate()
    {
        //Debug.Log("ReActivating!");

        owningObject.busy = true;
        activated = true;
        attachingObject.busy = true;

        Physics.IgnoreCollision(owningObject.GetComponent<Collider>(), attachingObject.GetComponent<Collider>(), true);
        attachingObject.SetMoveMode(0);

        SetInitialTrackingPositions();
        attachingObject.transform.SetParent(objectGuide);
    }


    private void Deactivate()
    {
        //Debug.Log("Deactivating!");

        attachingObject.busy = false;
        owningObject.busy = false;
        activated = false;
        attachStop = false;
        attachingObject.SetMoveMode(1);
        Physics.IgnoreCollision(owningObject.GetComponent<Collider>(), attachingObject.GetComponent<Collider>(), false);
    }



    private void AttachObject()
    {
        Debug.Log("Attaching!");
        activated = false;
        attachingController.GetTransitionGuide().SetPositionAndRotation(this.transform.position, this.transform.rotation);
        objectGuide.SetPositionAndRotation(this.transform.position, trackerOffset.rotation);
        owningObject.busy = false;
        attachingObject.busy = false;
        attachingObject.AttachTo(owningObject, this);
        attachingController.ReleaseObject();
        attachStop = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!activated && owningObject && !owningObject.busy)
        {
            AttachmentTriggerController otherTrigger = other.GetComponent<AttachmentTriggerController>();

            if (otherTrigger && otherTrigger.attachmentTypeName == this.attachmentTypeName
                && otherTrigger.GetOwningObject() && !owningObject.attachedObjects.Contains(otherTrigger.GetOwningObject()) 
                && otherTrigger.CheckReadyToAttach())
            {
                Activate(otherTrigger);
            }
        }
    }


    // getters

    private InteractiveObjectController GetAttachingObject(AttachmentTriggerController trigger)
    {
        InteractiveObjectController attaching = trigger.GetOwningObject();

        if (attaching.rootObject)
        {
            Debug.Log("Setting attaching object to object root!.");
            attaching = attaching.rootObject;
        }

        return attaching;
    }

    private ManipulatorController GetAttachingController(AttachmentTriggerController trigger)
    {
        ManipulatorController controller = null;
        InteractiveObjectController triggerOwner = trigger.GetOwningObject();

        if(triggerOwner.attached)
        {
            controller = triggerOwner.rootObject.GetAttachedController();
        }
        else
        {
            controller = triggerOwner.GetAttachedController();
        }

        return controller;
    }

    public AttachmentTriggerController GetAttachingTrigger()
    {
        return attachingTrigger;
    }

    public Transform GetGuide()
    {
        return objectGuide;
    }

    private float GetDistance(Transform a, Transform b)
    {
        return Vector3.Magnitude(a.position - b.position);
    }
    


    private Quaternion GetLocalInlineRotation(Transform otherObject)
    {
        float y = otherObject.localEulerAngles.y;
        float mod = y % 90;
        float inline = y - mod;

        if (mod >= 45)
        {
            inline += 90;
        }

        return Quaternion.Euler(new Vector3(0, inline, 0));
    }


    private void SetInitialTrackingPositions()
    {
        controllerTracker.position = attachingController.transform.position;
        controllerTracker.localPosition = new Vector3(0, controllerTracker.localPosition.y, 0);
        objectGuide.SetPositionAndRotation(attachingTrigger.transform.position, attachingTrigger.transform.rotation);
        trackerOffset.position = attachingObject.transform.position;
        trackerOffset.localPosition = new Vector3(0, trackerOffset.localPosition.y, 0);
        trackerOffset.localRotation = GetLocalInlineRotation(objectGuide);
    }




    // old stuff, probably useless

}
