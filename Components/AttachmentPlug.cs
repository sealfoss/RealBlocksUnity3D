using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentPlug : MonoBehaviour, IAttachmentPoint {
    private IObjectController owningObject { get { return GetComponentInParent<IObjectController>(); } }
    public string attachmentType = "block";
    private AttachmentTrigger attachedObjectTrigger;
    private IObjectController attachedObject;
    private ObjectMover attachedObjectMover;
    private HandComponent attachedObjectGrabbedBy;
    public Transform moveGuide;
    private Vector3 movePosition = Vector3.zero;
    private float moveObjectOffeset = 0.0f;
    private bool activated = false;
    private bool tracking = false;
    public float deactivationDistance = 0.12f;
    public float attachStopResetDistance = 0.01f;
    public float attachDistance = 0.002f;
    private bool attachStop = false;
    public float attachingMoveSpeed = 10.0f;
    public float attachingRotateSpeed = 10.0f;
    private Quaternion inlineRot = Quaternion.identity;

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        
        if (tracking) { Track(); }
        else
        {

        }
    }

    public void Track()
    {
        float distance = Vector3.Magnitude(this.transform.position - attachedObjectMover.transform.position);
        Debug.Log("Track Distance: " + distance);

        if (DetectDeactivate()) { Deactivate(); }
        else
        {
            Vector3 local = transform.InverseTransformPoint(attachedObjectGrabbedBy.transform.position);
            local = new Vector3(0, local.y, 0);
            local = Vector3.ClampMagnitude(local, local.magnitude - attachedObject.GetAttachmentGrabOffset());
            Vector3 world = transform.TransformPoint(local);
            moveGuide.position = world;
            // move the rotation
            //Quaternion moveRotation = Quaternion.FromToRotation(this.transform.position, attachedObjectTrigger.transform.position);
            //attachedObjectMover.transform.rotation = Quaternion.Lerp(attachedObjectMover.transform.rotation, moveRotation, attachingRotateSpeed * Time.fixedDeltaTime);
            moveGuide.rotation = inlineRot;

        }
        
        if (DetectAttach()) { Attach(); }
    }

    public void Activate(AttachmentTrigger trigger)
    {
        activated = true;

        if (trigger != null)
        {
            attachedObjectTrigger = trigger;
            attachedObject = trigger.GetOwningObject();
            attachedObjectMover = attachedObject.GetObjectMover();
            attachedObjectGrabbedBy = attachedObjectMover.GetGrabbedBy();
        }

        if(attachedObject != null && attachedObjectMover && attachedObjectGrabbedBy)
        {
            attachedObject.SetBusy(true);
            inlineRot = GetLocalInlineRotation(attachedObjectMover.transform);
            attachedObjectGrabbedBy.Release();
            //attachedObjectMover.SetMoveMode(2);
            //attachedObjectGrabbedBy.Grab(attachedObjectMover);
            moveGuide.SetParent(this.transform);
            moveGuide.SetPositionAndRotation(attachedObjectTrigger.transform.position, attachedObjectTrigger.transform.rotation);
            attachedObject.KillCollision();
            attachedObject.KillPhysics();
            attachedObject.IgnoreCollision(owningObject.GetGameObject().GetComponent<BoxCollider>(), true);
            attachedObjectMover.transform.localRotation = inlineRot;
            //moveObjectOffeset = Vector3.Magnitude(attachedObjectMover.transform.position - attachedObjectGrabbedBy.transform.position);
            attachedObjectMover.transform.SetParent(moveGuide);
            
            //attachedObjectMover.transform.SetParent(moveGuide);
            tracking = true;
            owningObject.SetBusy(true);
        }
        else
        {
            Deactivate();
        }
    }


    private bool DetectDeactivate()
    {
        bool deactivate = false;

        if(!attachedObjectTrigger)
        {
            deactivate = true;
            //Debug.Log("No trigger!");
        }

        if(attachedObject == null)
        {
            deactivate = true;
            //Debug.Log("No attached object!");
        }

        if (!attachedObjectMover)
        {
            deactivate = true;
            //Debug.Log("No mover!");
        }

        if (!attachedObjectGrabbedBy)
        {
            deactivate = true;
            //Debug.Log("No grabbed by!");
        }

        if (!attachedObjectMover.GetGrabbed())
        {
            deactivate = true;
            //Debug.Log("Not grabbed!");
        }

        if (Vector3.Magnitude(attachedObjectMover.transform.position - this.transform.position) > deactivationDistance)
        {
            deactivate = true;
            //Debug.Log("beyond distance!");
        }

        deactivate = false;
        return deactivate;
    }


    public void Deactivate()
    {
        tracking = false;
        activated = false;
        attachedObject.SetBusy(false);
        attachedObjectMover.transform.SetParent(null);
        attachedObjectMover.SetMoveMode(1);
        attachedObject.IgnoreCollision(this.GetComponent<BoxCollider>(), false);
        attachedObjectGrabbedBy.Grab(attachedObjectMover);
        attachedObjectTrigger = null;
        Debug.Log("Deactivated!");
        owningObject.SetBusy(false);
    }


    private bool DetectAttach()
    {
        bool attach = false;

        if (attachStop)
        {
            if (Vector3.Magnitude(attachedObjectMover.transform.position - this.transform.position) > attachStopResetDistance)
            { attachStop = false; }
        }
        else if (Vector3.Magnitude(attachedObjectMover.transform.position - this.transform.position) <= attachDistance)
        { attach = true; }

        attach = false;
        return attach;
    }


    public void Attach()
    {
        attachedObjectMover.transform.position = this.transform.position + attachedObjectTrigger.GetPositionOffset();
        attachedObjectMover.transform.rotation = inlineRot;
        attachedObjectMover.SetMoveMode(0);
        attachedObject.AttachTo(owningObject.GetGameObject(), this);
        tracking = false;
        owningObject.SetBusy(false);
    }


    

    private void OnTriggerEnter(Collider other)
    {
        if (!activated && owningObject != null && !owningObject.GetBusy() && (owningObject.GetObjectMover().GetGrabbed() || owningObject.GetAttached()))
        {
            AttachmentTrigger trigger = other.GetComponent<AttachmentTrigger>();
            if (trigger) { Debug.Log("Plug: " + this.gameObject.name + ", collided with trigger: " + trigger.gameObject.name); }

            if (trigger && !trigger.GetOwningObject().GetBusy() 
                && trigger.attachmentType == this.attachmentType && trigger.GetOwningObject().GetObjectMover().GetGrabbed())
            {
                Debug.Log("Activating plug" + this.name + " via trigger: " + trigger.name);
                Activate(trigger);
            }
        }
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

    // getters

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }



    public void DrawLineMatrix(Transform otherTrans)
    {
        Vector3 world = otherTrans.position;
        Vector3 local = transform.InverseTransformPoint(world);
        local = new Vector3(0, local.y, 0);
        world = transform.TransformPoint(local);
        Debug.DrawLine(this.transform.position, world, Color.yellow);
    }
}
