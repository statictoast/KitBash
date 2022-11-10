using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// instead of having a timer, these FXs must have RequestEnd called in order for them to stop
public class FXGameManagedLogic : FXLogic
{
    bool m_requestedToEnd;

    public override void FXStart()
    {
        base.FXStart();
        m_requestedToEnd = false;
    }

    public override bool ShouldEnd()
    {
        return m_requestedToEnd;
    }

    public override void RequestEnd()
    {
        m_requestedToEnd = true;
    }
}
