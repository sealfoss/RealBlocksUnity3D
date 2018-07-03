using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WreckingBallController : MonoBehaviour {

    Rigidbody rb;

    // Use this for initialization
    void Start () {
        Rigidbody rb = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {

    }

    private void OnTriggerEnter(Collider other)
    {
        InteractiveObjectController otherObject = other.GetComponent<InteractiveObjectController>();

        if (otherObject && otherObject.attached && otherObject.breakMomentum <= GetMomentum())
        {
            AttachmentPointController otherAttachment = otherObject.parentAttachmentPoint;
            otherObject.Detach();
            otherAttachment.Deactivate();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        InteractiveObjectController otherObject = collision.gameObject.GetComponent<InteractiveObjectController>();

        if(otherObject && otherObject.attached)
        {
            AttachmentPointController otherAttachment = otherObject.parentAttachmentPoint;
            otherObject.Detach();
            otherAttachment.Deactivate();
        }
    }

    public float GetMomentum()
    {
        float momentum = 0.0f;
        Vector3 velocity = this.GetComponent<Rigidbody>().velocity;
        float speed = velocity.magnitude;
        float mass = this.GetComponent<Rigidbody>().mass;
        momentum = speed * mass;
        return momentum;
    }
}
