using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandComponent : MonoBehaviour {
    // steam vr stuff
    private SteamVR_TrackedController trackedController { get { return this.GetComponent<SteamVR_TrackedController>(); } }
    private SteamVR_TrackedObject trackedObject { get { return this.GetComponent<SteamVR_TrackedObject>(); } }
    private SteamVR_Controller.Device device { get { return SteamVR_Controller.Input((int)trackedObject.index); } }
    private bool padClicked = false;


    public VRCharacterController characterVR;
    public bool left;


    // game code stuff
    public float velocityFactor = 1000;
    public float angularVelocityFactor = 120;
    private HashSet<ObjectMover> availableObjects = new HashSet<ObjectMover>();
    private ObjectMover closestObject = null;
    private ObjectMover lastClosestObject = null;
    private ObjectMover grabbedObject = null;
    public Color selectColor = Color.green;
    public Color grabColor = Color.cyan;
    HighlightController grabbedHighlighter { get { return grabbedObject.GetGameObject().GetComponent<HighlightController>(); } }
    HighlightController closestHighlighter { get { return closestObject.GetGameObject().GetComponent<HighlightController>(); } }
    HighlightController lastClosestHighlighter { get { return lastClosestObject.GetGameObject().GetComponent<HighlightController>(); } }


    // Use this for initialization
    void Start () {
		
	}

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update () {

        if (grabbedObject == null)
        {
            if (availableObjects.Count > 0)
            {
                FindClosestObject();
                //Debug.Log("Closest Object to Hand " + this.name + ": " + closestObject.GetGameObject().name);
            }
            else if (closestObject != null)
            {
                if (lastClosestObject != null && !lastClosestObject.GetGrabbed())
                {
                    lastClosestHighlighter.TurnHighlightOff();
                    lastClosestObject = null;
                }

                if (!closestObject.GetGrabbed()) { closestHighlighter.TurnHighlightOff(); }
                closestObject = null;
            }
        }

        if (padClicked) { MoveCharacter(); }
        
    }
    

    // setup

    private void OnEnable()
    {
        trackedController.Gripped += GripButtonClicked;
        trackedController.Ungripped += GripButtonUnclicked;
        trackedController.TriggerClicked += TriggerClicked;
        trackedController.TriggerUnclicked += TriggerUnclicked;
        trackedController.PadClicked += PadClicked;
        trackedController.PadUnclicked += PadUnclicked;
    }

    private void OnDisable()
    {
        trackedController.Gripped -= GripButtonClicked;
        trackedController.Ungripped -= GripButtonUnclicked;
        trackedController.TriggerClicked -= TriggerClicked;
        trackedController.TriggerUnclicked -= TriggerUnclicked;
        trackedController.PadClicked -= PadClicked;
        trackedController.PadUnclicked -= PadUnclicked;
    }

   
    


    // events

    private void OnTriggerEnter(Collider other)
    {
        ObjectMover moveableObject = other.GetComponent<ObjectMover>();

        if(moveableObject != null && !availableObjects.Contains(moveableObject))
        {
            //Debug.Log("Adding object " + other.name + " to Hand " + this.name + " available objects.");
            availableObjects.Add(moveableObject);
        }
        else
        {
            //Debug.Log("No moveable object found.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ObjectMover moveableObject = other.GetComponent<ObjectMover>();

        if (moveableObject != null && moveableObject != grabbedObject)
        {
            availableObjects.Remove(moveableObject);
        }
    }
    
    private void GripButtonClicked(object sender, ClickedEventArgs e)
    {
        if (!grabbedObject) { Grab(closestObject); }
        else { Release(); }
    }

    private void GripButtonUnclicked(object sender, ClickedEventArgs e)
    {
        //Debug.Log("Grip button on hand " + this.name + " has been unclicked.");
        if(grabbedObject != null && !trackedController.gripped)
        {
            //Release(grabbedObject);
        }
    }

    private void TriggerClicked(object sender, ClickedEventArgs e)
    {
        if(grabbedObject != null)
        {
            grabbedObject.GetObjectController().Activate();
        }
    }

    private void TriggerUnclicked(object sender, ClickedEventArgs e)
    {
        if (grabbedObject != null)
        {
            grabbedObject.GetObjectController().Deactivate();
        }
    }

    private void PadClicked(object sender, ClickedEventArgs e)
    {
        padClicked = true;
    }

    private void PadUnclicked(object sender, ClickedEventArgs e)
    {
        padClicked = false;
    }



    // helpers

    public void MoveCharacter()
    {
        if (left)
        {
            characterVR.RotateBody(device.GetAxis().x, device.GetAxis().y);
        }
        else
        {
            characterVR.Strafe(device.GetAxis().x, device.GetAxis().y);
        }
    }

    public void Grab(ObjectMover objectToGrab)
    {
        if (objectToGrab)
        {
            if (objectToGrab.GetGrabbed() && !objectToGrab.GetTwoHanded()) { objectToGrab.GetGrabbedBy().Release(); }

            objectToGrab.Grab(this);
            grabbedObject = objectToGrab;
            grabbedHighlighter.TurnHighlightOn(grabColor);
            //Debug.Log("Grabbing with controller: " + grabbedObject.name);
        }
        else { return; }
    }

    public void Release()
    {
        if (grabbedObject)
        {
            Debug.Log("Hand: " + this.name + " has called Release()");
            HighlightController grabbedHighlighter = grabbedObject.GetGameObject().GetComponent<HighlightController>();
            grabbedHighlighter.TurnHighlightOff();
            grabbedObject.Release();
            grabbedObject = null;
        }
        else { return; }
    }

    private void FindClosestObject()
    {
        lastClosestObject = closestObject;
        float closestDistance = float.MaxValue;

        foreach (ObjectMover moveable in availableObjects)
        {
            float currentDistance = Vector3.Magnitude(this.transform.position - moveable.GetTransform().position);

            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestObject = moveable;
            }
        }

        if(lastClosestObject != null && lastClosestObject != closestObject && !lastClosestObject.GetGrabbed()) { lastClosestHighlighter.TurnHighlightOff(); }
        
        if(!closestHighlighter.GetHighlightOn() || (!closestObject.GetGrabbed() && closestHighlighter.GetHighlightColor() != selectColor))
        {
            closestHighlighter.TurnHighlightOn(selectColor);
        }
    }



    // getters

    public float GetVelocityFactor()
    {
        return velocityFactor;
    }

    public float GetAngularVelocityFactor()
    {
        return angularVelocityFactor;
    }
}
