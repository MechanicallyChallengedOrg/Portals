using UnityEngine;

public static class Globals {

    /// <summary>
    /// How wide the portal is in its relative horizontal direction
    /// </summary>
    public static float PortalWidth = 2f;
    
    /// <summary>
    /// How tall the portal is in its relative vertical direction
    /// </summary>
    public static float PortalHeight = 3.5f;
    
    /// <summary>
    /// The scale of the portal as a Vector3.
    /// </summary>
    public static Vector3 PortalScale => new Vector3(PortalHeight, PortalWidth, 1f);

    /// <summary>
    /// The width radius of the portal.
    /// </summary>
    public static float PortalWidthRadius => PortalWidth / 2f;

    /// <summary>
    /// The height radius of the portal.
    /// </summary>
    public static float PortalHeightRadius => PortalHeight / 2f;

    /// <summary>
    /// How fast a portal projectile travels when fired from the portal gun.
    /// </summary>
    public static float PortalGunSpeed = 20f;

    /// <summary>
    /// How quickly a portal opens or closes.
    /// </summary>
    public static float PortalBloopSpeed = 25f;

    /// <summary>
    /// How long a portal ball "lives" before destroying itself if it doesn't hit a surface.
    /// Prevents portal balls from flying off into infinity, also used to limit range of portal gun.
    /// </summary>
    public static float PortalBallLifetime = 5f;

    /// <summary>
    /// Value used to determine how many raycasts are used when checking to see if a potential portal
    /// position is valid. The higher the number, the more raycasts are used, which provides a more accurate
    /// idea of whether a position is valid but is less performant. A smaller number is more performant but
    /// has a higher chance of missing an obstacle that would interfere with the portal.
    /// </summary>
    public static int PortalSpaceCheckFidelity = 2;

}
