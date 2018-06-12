using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour {
    //public List<AttachmentPointController> attachmentPoints = new List<AttachmentPointController>();
    public AttachmentPointController[] attachmentPoints;
    public HashSet<AttachmentPointController> activatedAttachmentPoints = new HashSet<AttachmentPointController>();
    private InteractiveObjectController owningObject;

	// Use this for initialization
	void Start ()
    {
        owningObject = this.GetComponent<InteractiveObjectController>();
    }
	
	// Update is called once per frame
	void Update () {
	}

    // helpers
    

    private bool CheckForAttachment()
    {
        bool attachReady = false;

        // i split this up just for readibility's sake
        if(owningObject.grabbed)
        {
            Debug.Log("Not attach ready");
            attachReady = true;
        }
        else if(owningObject.rootObject && (owningObject.rootObject.grabbed || owningObject.rootObject.attached))
        {
            attachReady = true;
        }
        
        return attachReady;
    }

    private void GetAttachmentPoints()
    {
        attachmentPoints = GetComponentsInChildren<AttachmentPointController>();
    }
}
