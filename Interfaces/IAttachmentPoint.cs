using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttachmentPoint
{
    GameObject GetGameObject();
    void Track();
    void Activate(AttachmentTrigger triggger);
    void Deactivate();
    void Attach();
}
