using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentTriggerController : MonoBehaviour {
    private InteractiveObjectController owningObject;
    private bool activated;
    private AttachmentPointController pairedAttachmentPoint;
    public string attachmentTypeName = "";

	// Use this for initialization
	void Start () {
        activated = false;
        owningObject = GetComponentInParent<InteractiveObjectController>();
        pairedAttachmentPoint = null;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // helpers

    public void Activate(AttachmentPointController activatingPoint)
    {
        pairedAttachmentPoint = activatingPoint;
        activated = true;
    }

    public void Deactivate()
    {
        activated = false;
        pairedAttachmentPoint = null;
    }

    // getters

    public InteractiveObjectController GetOwningObject()
    {
        return owningObject;
    }

    public bool GetActivated()
    {
        return activated;
    }

    public AttachmentPointController GetPairedAttachmentPoint()
    {
        return pairedAttachmentPoint;
    }

}
