﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentTriggerController : MonoBehaviour {
    private InteractiveObjectController owningObject;
    private bool activated;
    private AttachmentPointController pairedAttachmentPoint;
    public string attachmentTypeName = "";
    private AttachmentPointController overlappingAttachmentPoint = null;

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

    public bool CheckReadyToAttach()
    {
        bool ready = false;

        if (!owningObject.busy)
        {
            if (owningObject.grabbed)
            {
                ready = true;
            }
            else if (owningObject.rootObject)
            {
                if (owningObject.rootObject.grabbed && !owningObject.rootObject.busy)
                {
                    //ready = true;
                }
            }
        }

        return ready;
    }

    private void OnTriggerEnter(Collider other)
    {
        AttachmentPointController attachmentPoint = other.GetComponent<AttachmentPointController>();

        if(attachmentPoint)
        {
            overlappingAttachmentPoint = attachmentPoint;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        AttachmentPointController attachmentPoint = other.GetComponent<AttachmentPointController>();

        if (attachmentPoint && attachmentPoint == overlappingAttachmentPoint)
        {
            overlappingAttachmentPoint = null;
        }
    }

    public AttachmentPointController GetOverlappingAttachmentPoint()
    {
        return overlappingAttachmentPoint;
    }
}
