using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObjectController : MonoBehaviour {

    // grabbing stuff
    public HashSet<ManipulatorController> availableControllers; 
    public ManipulatorController attachedController; // the manipulator currently in control of this object
    private Rigidbody rigidBody;
    public bool grabbable = true;

    public bool grabbed; // lets the system know this object is grabbed
    public bool busy; // lest the system know this object is in the middle of attaching or detaching, and shouldn't trigger anything else
    public ObjectHighlightController highlight;

    // attachment stuff
    public bool attached;  // lets the system know this object is attached to another
    public InteractiveObjectController rootObject; // controlling object of all objects attached to each other
    public InteractiveObjectController parentObject; // object this object is attached to
    public HashSet<InteractiveObjectController> attachedObjects;
    private AttachmentPointController parentAttachmentPoint; // the attachment point used by the object to attach to its parentObject
    //public float attachDistance = 0.01f;
    //public float disengagementDistance = 0.1f;

    // grabbed move stuff
    private float mass;
    private int moveMode = 1; // 0 for attached, 1 for physics move, 2 for transition move, -1 for dead (i guess?)
    public float velocityFactor = 2000;
    private float calculatedVelocityFactor;
    public float rotationFactor = 80;
    private float calculatedRotationFactor;
    public Transform grabHandle = null;
    public float maxGrabDistance = 10;
    public float gripPlayDistance = 0.1f;



    // Use this for initialization
    void Start()
    {
        availableControllers = new HashSet<ManipulatorController>();
        attachedObjects = new HashSet<InteractiveObjectController>();
        attachedController = null;
        this.rigidBody = GetComponent<Rigidbody>();
        mass = rigidBody.mass;

        if (grabbable)
        {    
            CalculateMovementFactors();
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (grabbed || highlight.highlightOn || availableControllers.Count > 0)
        {
            CheckHighlight();
        }

        if (grabbed)
        {
            //Debug.Log("Controller Distance: " + Vector3.Magnitude(this.transform.position - attachedController.transform.position));

            if (!attachedController.IsAvailableToController(this) && CheckTooFarFromController())
            {
                //attachedController.ReleaseObject();
            }
        }
    }

    // helpers

    private void CheckHighlight()
    {
        if(grabbed)
        {
            if (!highlight.highlightOn)
            {
                highlight.SetHighlightOn(false);
            }

            if(!highlight.highlightGrabbed)
            {
                highlight.SetHighlightGrabbed(false);
            }
        }
        else
        {
            if (!highlight.highlightSelected)
            {
                highlight.SetHighlightSelected(false);
            }

            bool selected = false;

            foreach (ManipulatorController controller in availableControllers)
            {
                if(controller.GetClosest() == this)
                {
                    selected = true;
                    break;
                }
            }

            if(selected)
            {
                if (!highlight.highlightOn)
                {
                    highlight.SetHighlightOn(false);
                }
            }
            else
            {
                if(highlight.highlightOn)
                {
                    highlight.SetHighlightOff(false);
                }
            }
        }
    }


    private bool CheckTooFarFromController()
    {
        if(Vector3.Magnitude(this.transform.position - attachedController.transform.position) >= maxGrabDistance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void CalculateMovementFactors()
    {
        calculatedVelocityFactor = velocityFactor / mass;
        calculatedRotationFactor = rotationFactor / mass;
    }

    public void Grab(ManipulatorController controller)
    {
        attachedController = controller;
        grabbed = true;
    }

    public void Release()
    {
        attachedController = null;
        grabbed = false;
    }

    public void AttachTo(InteractiveObjectController otherObject, AttachmentPointController attachmentPoint)
    {
        parentObject = otherObject;
        parentObject.attachedObjects.Add(this);
        parentAttachmentPoint = attachmentPoint;
        
        if(parentObject.rootObject)
        {
            rootObject = parentObject.rootObject;
        }
        else if(parentObject.grabbable)
        {
            rootObject = parentObject;
        }
        else
        {
            rootObject = this;
        }
        
        if(attachedObjects.Count > 0)
        {
            Reroot(rootObject);
        }

        Destroy(this.GetComponent<Rigidbody>());
        this.GetComponent<Collider>().isTrigger = false;
        this.transform.SetParent(rootObject.transform);

        if(parentObject.grabbable)
        {
            parentObject.AddMass(this.mass);
        }
        

        attached = true;
        Debug.Log(this.name + " attached to " + parentObject.name + ", with root object " + rootObject.name);
    }

    public void Detach()
    {
        this.transform.SetParent(parentAttachmentPoint.GetGuide()); // reparent to attachment point guide
        this.gameObject.AddComponent<Rigidbody>();
        rigidBody = GetComponent<Rigidbody>(); // reset the local variable for the object's rigid body
        rigidBody.mass = mass; // set the rigid body's mass to the stored value

        if (parentObject.grabbable)
        {
            parentObject.SubtractMass(mass); // remove this object's mass from the mass of the parent (and its parents)
        }

        parentObject.attachedObjects.Remove(this); // remove this object from the parent object's attached objects group
        rootObject = null; // we're detaching here, there is no more root object (this object is root)
        parentObject = null; // we're detaching here, there is no more parent object (this object is its own parent)
        Reroot(this); // reroot all of this object's attached objects to this object
        attached = false; // this object is no longer attached
        AttachmentPointController temp = parentAttachmentPoint; // temp reference for the attachment point that will be deleted once out of scope
        parentAttachmentPoint = null; // get rid of the attachment point global
        Debug.Log("DETATCH!");
        temp.Activate(temp.GetAttachingTrigger());  // reactivate the attachment point
    }

    private void Reroot(InteractiveObjectController newRoot)
    {
        foreach(InteractiveObjectController item in attachedObjects)
        {
            item.rootObject = newRoot;

            if(item.attachedObjects.Count > 0)
            {
                item.Reroot(newRoot);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ManipulatorController otherController = other.GetComponent<ManipulatorController>();

        if(otherController)
        {
            availableControllers.Add(otherController);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ManipulatorController otherController = other.GetComponent<ManipulatorController>();

        if (otherController)
        {
            availableControllers.Remove(otherController);
        }
    }

    public void SetMoveMode(int mode)
    {
        switch (mode)
        {
            case 0:
                if (rigidBody) { rigidBody.isKinematic = true; }
                break;

            case 1:
                rigidBody.isKinematic = false;
                this.transform.SetParent(null);
                break;

            default:
                Debug.Log("ERROR: " + this.name + " set to invalid move mode.");
                break;
        }

        moveMode = mode;
    }


    // getters

    public ManipulatorController GetAttachedController()
    {
        return attachedController;
    }

    public Rigidbody GetRigidbody()
    {
        return rigidBody;
    }

    public bool GetGrabbed()
    {
        return grabbed;
    }

    public InteractiveObjectController GetParentObject()
    {
        return parentObject;
    }

    public bool GetAttached()
    {
        return attached;
    }
    

    public InteractiveObjectController GetRootObject()
    {
        return rootObject;
    }

    public float GetMass()
    {
        return mass;
    }

    public float GetCalculatedVelocityFactor()
    {
        return calculatedVelocityFactor;
    }

    public float GetCalculatedRotationFactor()
    {
        return calculatedRotationFactor;
    }

    public HashSet<InteractiveObjectController> GetAttachedObjects()
    {
        return attachedObjects;
    }

    public int GetMoveMode()
    {
        return moveMode;
    }

    // setters
    

    public void SetMass(float newMass)
    {
        rigidBody.mass = newMass;
        mass = rigidBody.mass;
        CalculateMovementFactors();   
    }

    public void AddMass(float massToAdd)
    {
        mass += massToAdd;

        if(parentObject)
        {
            parentObject.AddMass(mass);
        }
        else if(GetComponent<Rigidbody>())
        {
            rigidBody.mass = mass;
        }

        CalculateMovementFactors();
    }

    public void SubtractMass(float massToSubract)
    {
        mass -= massToSubract;

        if(parentObject)
        {
            parentObject.SubtractMass(mass);
        }
        else if(GetComponent<Rigidbody>())
        {
            rigidBody.mass = mass;
        }

        CalculateMovementFactors();
    }
}
