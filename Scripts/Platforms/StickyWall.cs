using UnityEngine;

public class StickyWall : Platform
{

    protected override void ProcessOnCollisionEnter(Collision2D collision)
    {
        base.ProcessOnCollisionEnter(collision);

        StickyWallCollisionEvent newEvent = new StickyWallCollisionEvent(true);
        EventManager.Instance.TriggerEvent(Events.EVENT_STICKY_WALL_COLLISION, newEvent);
    }

    protected override void ProcessOnCollisionExit(Collision2D collision)
    {
        base.ProcessOnCollisionExit(collision);

        StickyWallCollisionEvent newEvent = new StickyWallCollisionEvent(false);
        EventManager.Instance.TriggerEvent(Events.EVENT_STICKY_WALL_COLLISION, newEvent);
    }
}
