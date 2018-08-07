using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttachable {

    bool AttachTo(GameObject otherObject, IAttachmentPoint attachmentPoint); // returns true if attachment is succesful, false if otherwise
    void Detach();
}
