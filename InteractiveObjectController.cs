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
    public int attachmentHeight;

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
        DetectHighlight();
    }

    // helpers

    private void DetectHighlight()
    {
        if(grabbed)
        {
            if(!highlight.highlightGrabbed || !highlight.highlightOn)
            {
                highlight.SetHighlightGrabbed(true);
            }
        }
        else if(availableControllers.Count > 0)
        {
            HashSet<ManipulatorController> toRemove = new HashSet<ManipulatorController>();

            foreach(ManipulatorController manipulator in availableControllers)
            {
                if(manipulator.GetClosest() != this)
                {
                    toRemove.Add(manipulator);
                }
            }

            if(toRemove.Count > 0)
            {
                foreach(ManipulatorController manipuator in toRemove)
                {
                    availableControllers.Remove(manipuator);
                }
            }

            if(availableControllers.Count > 0)
            {
                if(!highlight.highlightSelected || !highlight.highlightOn)
                {
                    highlight.SetHighlightSelected(true);
                }
            }
            else
            {
                if(highlight.highlightOn)
                {
                    highlight.SetHighlightOff(true);
                }
            }
        }
        else if(highlight.highlightOn)
        {
            highlight.SetHighlightOff(true);
        }

        if(parentObject && parentObject.highlight.highlightOn)
        {
            if(parentObject.highlight.highlightSelected && (!highlight.highlightOn || highlight.highlightGrabbed))
            {
                highlight.SetHighlightSelected(true);
            }
            else if (parentObject.highlight.highlightGrabbed && (!highlight.highlightOn || highlight.highlightSelected))
            {
                highlight.SetHighlightGrabbed(true);
            }
        }
    }

    public void SelectThisObject(ManipulatorController controller)
    {
        availableControllers.Add(controller);
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

    public void AddObjects(InteractiveObjectController objectToAdd)
    {
        attachedObjects.Add(objectToAdd);

        if (objectToAdd.attachedObjects.Count > 0)
        {
            foreach (InteractiveObjectController obj in objectToAdd.attachedObjects)
            {
                attachedObjects.Add(obj);
            }
        }

        if(parentObject)
        {
            parentObject.AddObjects(objectToAdd);
        }
    }

    public void RemoveObjects(InteractiveObjectController objectToRemove)
    {
        attachedObjects.Remove(objectToRemove);

        if (objectToRemove.attachedObjects.Count > 0)
        {
            foreach (InteractiveObjectController obj in objectToRemove.attachedObjects)
            {
                attachedObjects.Remove(obj);
            }
        }

        if(parentObject)
        {
            parentObject.RemoveObjects(objectToRemove);
        }
    }


    public void AttachTo(InteractiveObjectController otherObject, AttachmentPointController attachmentPoint)
    {
        parentObject = otherObject;
        parentAttachmentPoint = attachmentPoint;
        parentObject.AddObjects(this);
        Reroot(FindRoot(parentObject));
        parentObject.AddObjects(this);
        Destroy(this.GetComponent<Rigidbody>());
        this.GetComponent<Collider>().isTrigger = false;
        parentObject.AddMass(mass);
        attached = true;
        //Debug.Log(this.name + " attached to " + parentObject.name + ", with root object " + rootObject.name);
    }

    public void Detach()
    {
        this.transform.SetParent(parentAttachmentPoint.GetGuide()); // reparent to attachment point guide
        this.gameObject.AddComponent<Rigidbody>();
        rigidBody = GetComponent<Rigidbody>(); // reset the local variable for the object's rigid body
        rigidBody.mass = mass; // set the rigid body's mass to the stored value
        parentObject.RemoveObjects(this); // remove this object from the parent object's attached objects group
        parentObject.SubtractMass(mass);

        rootObject = null; // we're detaching here, there is no more root object (this object is root)
        parentObject = null; // we're detaching here, there is no more parent object (this object is its own parent)
        Reroot(this); // reroot all of this object's attached objects to this object
        attached = false; // this object is no longer attached

        AttachmentPointController temp = parentAttachmentPoint; // temp reference for the attachment point that will be deleted once out of scope
        parentAttachmentPoint = null; // get rid of the attachment point global
        temp.Activate(temp.GetAttachingTrigger());  // reactivate the attachment point
    }

    private InteractiveObjectController FindRoot(InteractiveObjectController otherObject)
    {
        InteractiveObjectController newRoot = null;

        if (otherObject.rootObject)
        {
            newRoot = parentObject.rootObject;
        }
        else if (otherObject.grabbable)
        {
            newRoot = parentObject;
        }
        else
        {
            newRoot = this;
        }

        return newRoot;
    }

    private void Reroot(InteractiveObjectController newRoot)
    {
        this.rootObject = newRoot;

        if (rootObject != this)
        {
            this.transform.SetParent(rootObject.transform);
        }

        if (attachedObjects.Count > 0)
        {
            foreach (InteractiveObjectController attached in attachedObjects)
            {
                attached.Reroot(rootObject);
            }
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

    public void AddMass(float massToAdd)
    {
        mass += massToAdd;

        if(GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().mass = mass;
        }

        CalculateMovementFactors();

        if (parentObject)
        {
            parentObject.AddMass(massToAdd);
        }
    }

    public void SubtractMass(float massToSubract)
    {
        mass -= massToSubract;

        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().mass = mass;
        }

        CalculateMovementFactors();

        if (parentObject)
        {
            parentObject.SubtractMass(massToSubract);
        }
    }

    public void ResetCenterOfMass()
    {
        InteractiveObjectController objectToReset;

        if (rootObject)
        {
            objectToReset = rootObject;
        }
        else
        {
            objectToReset = this;
        }

        if (objectToReset.GetComponent<Rigidbody>())
        {
            objectToReset.GetComponent<Rigidbody>().ResetCenterOfMass();
        }
    }
    
}
