using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachmentTrigger : MonoBehaviour {
    public string attachmentType = "block";
    public IObjectController owningObject { get { return GetComponentInParent<IObjectController>(); } }
    private Vector3 positionOffset { get { return -1 * this.transform.localPosition; } }
    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {

    }
    

    public IObjectController GetOwningObject()
    {
        return owningObject.GetRootObject();
    }

    public Vector3 GetPositionOffset()
    {
        return positionOffset;
    }
}
