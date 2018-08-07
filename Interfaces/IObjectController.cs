using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectController
{
    void Activate();
    void Deactivate();
    void KillPhysics();
    void EnablePhysics();
    void KillCollision();
    void EnableCollision();
    void IgnoreCollision(Collider otherCollider, bool ignore);
    void InitializeRigidbody();
    void DestroyRigidbody();
    IObjectController GetRootObject();
    ObjectMover GetObjectMover();
    GameObject GetGameObject();
    void SetBusy(bool newBusy);
    bool GetBusy(); // giggity.
    bool AttachTo(GameObject otherObject, IAttachmentPoint attachmentPoint);
    void Detach();
    bool GetAttached();
    float GetAttachmentGrabOffset();
}
