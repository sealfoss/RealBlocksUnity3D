using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObjectController : MonoBehaviour {

    public Vector3 spawnPosition;

    // grabbing stuff
    public HashSet<ManipulatorController> availableControllers = new HashSet<ManipulatorController>();
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
    public AttachmentPointController parentAttachmentPoint; // the attachment point used by the object to attach to its parentObject
    public int attachmentHeight;
    public HashSet<InteractiveObjectController> parentObjects = new HashSet<InteractiveObjectController>(); // objects this object is attached to
    //public List<InteractiveObjectController> parentObjects = new List<InteractiveObjectController>();
    public HashSet<InteractiveObjectController> attachedObjects = new HashSet<InteractiveObjectController>(); // objects attached to this object
    public AttachmentTriggerController[] triggers;
    public AttachmentPointController[] attachmentPoints;
    public float collisionDetachForce = 100.0f;
    public float breakMomentum = 10.0f;
    public CollisionTrackerController attachmentTracker;

    // grabbed move stuff
    public float mass;
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
        spawnPosition = this.transform.position;

        if (triggers.Length == 0) { triggers = GetComponentsInChildren<AttachmentTriggerController>(); }
        if (attachmentPoints.Length == 0) { attachmentPoints = GetComponentsInChildren<AttachmentPointController>(); }

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
        //DetectHighlight();
    }

    // helpers

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 collisionVelocity = collision.relativeVelocity;
        float collisonMagnitude = collisionVelocity.magnitude;

        if(collisonMagnitude >= collisionDetachForce)
        {
            Detach();
            parentAttachmentPoint.Deactivate();
        }
    }

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
        this.attachedObjects.Remove(objectToRemove);

        if (this != objectToRemove && objectToRemove.attachedObjects.Count > 0)
        {
            foreach (InteractiveObjectController obj in objectToRemove.attachedObjects)
            {
                this.attachedObjects.Remove(obj);
            }
        }

        if (parentObject)
        {
            parentObject.RemoveObjects(objectToRemove);
        }
    }


    public void AttachTo(InteractiveObjectController otherObject, AttachmentPointController attachmentPoint)
    {
        parentObject = otherObject;
        parentAttachmentPoint = attachmentPoint;
        //parentObject.AddObjects(this);
        Reroot(FindRoot(parentObject));
        parentObject.AddObjects(this);
        Destroy(this.GetComponent<Rigidbody>());
        this.GetComponent<Collider>().isTrigger = false;
        //parentObject.AddMass(mass);
        attached = true;
        //Debug.Log(this.name + " attached to " + parentObject.name + ", with root object " + rootObject.name);
        SetParents();
    }

    public void Detach()
    {
        SetAttached();
        //Reroot(FindRoot(parentObject));

        this.transform.SetParent(parentAttachmentPoint.GetGuide()); // reparent to attachment point guide
        this.gameObject.AddComponent<Rigidbody>();
        rigidBody = GetComponent<Rigidbody>(); // reset the local variable for the object's rigid body
        rigidBody.mass = mass; // set the rigid body's mass to the stored value
        //parentObject.RemoveObjects(this); // remove this object from the parent object's attached objects group
        //parentObject.SubtractMass(mass);
        ClearParents();
        rootObject = null; // we're detaching here, there is no more root object (this object is root)
        parentObject = null; // we're detaching here, there is no more parent object (this object is its own parent)
        Reroot(this); // reroot all of this object's attached objects to this object
        attached = false; // this object is no longer attached

        AttachmentPointController temp = parentAttachmentPoint; // temp reference for the attachment point that will be deleted once out of scope
        parentAttachmentPoint = null; // get rid of the attachment point global
        temp.Activate(temp.GetAttachingTrigger());  // reactivate the attachment point
    }

    private HashSet<InteractiveObjectController> SetAttached()
    {
        attachedObjects.Clear();
        attachedObjects = attachmentTracker.GetAttachedObjects();

        foreach (InteractiveObjectController attached in attachedObjects)
        {
            attached.transform.SetParent(this.transform);
        }
        
        /**

        foreach (AttachmentPointController attachmentPoint in attachmentPoints)
        {

            AttachmentTriggerController trigger = attachmentPoint.GetOverlappingTrigger();

            if (trigger)
            {
                InteractiveObjectController attached = trigger.GetOwningObject();

                if (attached && attached.attached && !attachedObjects.Contains(attached))
                {
                    attachedObjects.Add(attached);
                    HashSet<InteractiveObjectController> newObjects = attached.SetAttached();

                    foreach (InteractiveObjectController newObject in newObjects)
                    {
                        if (!attachedObjects.Contains(newObject))
                        {
                            attachedObjects.Add(newObject);
                        }
                    }
                }
            }
        }

        **/

        return attachedObjects;
        
    }

    private void SetParents()
    {
        //parentObjects.Clear();

        foreach(AttachmentTriggerController trigger in triggers)
        {
            AttachmentPointController attachmentPoint = trigger.GetOverlappingAttachmentPoint();

            if(attachmentPoint)
            {
                InteractiveObjectController newParent = attachmentPoint.GetOwningObject();

                if(newParent && (newParent.grabbed || newParent.attached) && !parentObjects.Contains(newParent))
                {
                    newParent.AddObjects(this);
                    newParent.AddMass(mass);
                    parentObjects.Add(newParent);

                    //Debug.Log("Object: " + this.name + " adding object: " + attachmentPoint.GetOwningObject().name + " as a parent object.");
                    //Debug.Log("Parent objects count: " + parentObjects.Count);
                }
            }
        }

        if(attachedObjects.Count > 0)
        {
            foreach(InteractiveObjectController attached in attachedObjects)
            {
                attached.SetParents();
            }
        }

        //SayParents();
    }

    private void SayParents()
    {
        string names = "Parents of block " + this.name + ": ";

        foreach (InteractiveObjectController parent in parentObjects)
        {
            names += parent.name + ", ";
        }

        Debug.Log(names);
    }

    private void ClearParents()
    {
        Debug.Log("Clearing parents: ");
        SayParents();

        foreach(InteractiveObjectController parent in parentObjects)
        {
            parent.RemoveObjects(this);
            parent.SubtractMass(mass);
        }
        
        parentObjects.Clear();
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


    public void IgnoreCollision(Collider objectToIgnore, bool ignoreStatus)
    {
        Physics.IgnoreCollision(this.GetComponent<Collider>(), objectToIgnore, ignoreStatus);

        if (rootObject && rootObject.attachedObjects.Count > 0)
        {
            foreach(InteractiveObjectController attached in rootObject.attachedObjects)
            {
                Physics.IgnoreCollision(attached.GetComponent<Collider>(), objectToIgnore, ignoreStatus);
                //attached.IgnoreCollision(objectToIgnore, ignoreStatus);
            }
        }
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
