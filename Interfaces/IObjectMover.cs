using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectMover
{
    void SetTarget(Transform newTarget);
    Transform GetTarget();
    void MoveObject();
    void Grab(HandComponent grabbing);
    void Release();
    Transform GetTransform();
    IObjectController GetObjectController();
    bool GetGrabbed();
    GameObject GetGameObject();
    bool GetTwoHanded();
    HandComponent GetGrabbedBy();
    void SetMoveMode(int newMode);
}
