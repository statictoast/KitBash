using UnityEngine;

public abstract class InputModule
{
    public abstract Vector2 GetHeading();
    public abstract Vector2 GetAimingDirection();
    public abstract InputActions GetActions();
}
