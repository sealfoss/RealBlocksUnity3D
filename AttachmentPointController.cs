using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentPointController : MonoBehaviour {

    // public variables for configuring the attachment point
    public string attachmentTypeName = "";
    public float attachDistance = 0.01f;
    public float disengagementDistance = 0.1f;

    // trigger stuff
    private InteractiveObjectController owningObject;
    private InteractiveObjectController attachingObject;
    private ManipulatorController attachingController;
    private AttachmentTriggerController attachingTrigger;
    private bool activated;
    private bool attachStop;

    //tracking stuff
    public Transform controllerTracker;
    public Transform trackerOffset;
    public Transform objectGuide;
    public float transitionMoveSpeed = 1;
    public float transitionRotateSpeed = 1;
    private Quaternion inlineRotation;

	// Use this for initialization
	void Start () {
        owningObject = GetComponentInParent<InteractiveObjectController>();
    }
	
	// Update is called once per frame
	void FixedUpdate () {
		if(activated)
        {
            TrackController();
        }
	}

    private bool DetectDeactivation()
    {
        bool deactivate = false;

        if(!attachingObject.GetGrabbed())
        {
            deactivate = true;
        }
        else if(owningObject.grabbable)
        {
            if(!owningObject.GetGrabbed() && !owningObject.GetAttached())
            {
                deactivate = true;
            }
        }

        return deactivate;
    }

    private void TrackController()
    {
        if (DetectDeactivation())
        {
            Deactivate();
        }
        else
        {
            controllerTracker.position = attachingController.transform.position;
            controllerTracker.localPosition = new Vector3(0, controllerTracker.localPosition.y, 0);
            //controllerTracker.localRotation = Quaternion.Euler(new Vector3(0, attachingController.transform.localEulerAngles.z, 0)); // to control twist, if you want it.
            //Debug.Log("guide local y: " + objectGuide.localPosition.y);
            TransitionGuide();
            
            if(!attachStop && objectGuide.localPosition.y <= attachDistance)
            {
                attachStop = true;
                AttachObject();
            }
            else if(attachStop && objectGuide.localPosition.y > attachDistance)
            {
                attachStop = false;
            }
            else if(objectGuide.localPosition.y >= disengagementDistance)
            {
                Deactivate();
            }
        }
    }

    private float GetDistance(Transform a, Transform b)
    {
        return Vector3.Magnitude(a.position - b.position);
    }

    private void SetInlineRotation()
    {
        float y = objectGuide.localEulerAngles.y;
        float mod = y % 90;
        float inline = y - mod;

        if(mod >= 45)
        {
            inline += 90;
        }

        inlineRotation = Quaternion.Euler(new Vector3(0, inline, 0));
    }

    private void TransitionGuide()
    {
        objectGuide.position = Vector3.Lerp(objectGuide.transform.position, trackerOffset.position, Time.deltaTime * transitionMoveSpeed);
        //objectGuide.rotation = Quaternion.Lerp(objectGuide.rotation, trackerOffset.rotation, Time.deltaTime * transitionRotateSpeed); // if not using inline rotation.
        objectGuide.localRotation = Quaternion.Lerp(objectGuide.localRotation, inlineRotation, Time.deltaTime * transitionRotateSpeed);
    }
    
    

    public void Activate(InteractiveObjectController activatingObject, AttachmentTriggerController activatingTrigger)
    {
        attachingObject = activatingObject;
        attachingController = attachingObject.GetAttachedController();
        attachingObject.SetActivated(true);
        owningObject.SetActivated(true);

        if (activatingTrigger != null)
        {
            attachingTrigger = activatingTrigger;
        }
        
        controllerTracker.position = attachingController.transform.position;
        controllerTracker.localPosition = new Vector3(0, controllerTracker.localPosition.y, 0);

        trackerOffset.position = attachingObject.transform.position;
        trackerOffset.localPosition = new Vector3(0, trackerOffset.localPosition.y, 0);

        objectGuide.position = attachingTrigger.transform.position;
        objectGuide.rotation = attachingTrigger.transform.rotation;
        SetInlineRotation();

        attachingObject.transform.SetParent(objectGuide);
        attachingObject.moveMode = 0;
        attachingObject.PhysicsOff();

        activated = true;
        //Debug.Log("ACTIVATED!");
    }

    private void Deactivate()
    {
        attachingObject.transform.SetParent(null);
        attachingObject.PhysicsOn();
        attachingObject.moveMode = 1;

        attachingController.ReleaseObject();
        attachingController.GrabObject(attachingObject);

        owningObject.SetActivated(false);
        attachingObject.SetActivated(false);

        attachingObject = null;
        attachingController = null;

        activated = false;
        attachStop = false;
        //Debug.Log("DEACTIVATED!");
    }

    private void AttachObject()
    {
        objectGuide.position = this.transform.position;
        objectGuide.localRotation = inlineRotation;
        
        attachingObject.SetActivated(false);
        owningObject.SetActivated(false);
        activated = false;

        attachingController.ReleaseObject();
        attachingObject.AttachTo(this.owningObject, this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(owningObject && !owningObject.GetActivated() && (!owningObject.grabbable || owningObject.GetGrabbed() || owningObject.GetAttached()))
        {
            AttachmentTriggerController otherTrigger = other.GetComponent<AttachmentTriggerController>();

            if (otherTrigger && otherTrigger.attachmentTypeName == this.attachmentTypeName
                && !otherTrigger.GetOwningObject().GetActivated() && otherTrigger.GetOwningObject().GetGrabbed())
            {
                //Debug.Log("Attachment detected!");
                Activate(otherTrigger.GetOwningObject(), otherTrigger);
            }
        }
    }


    // getters

    public Transform GetGuide()
    {
        return objectGuide;
    }
}
