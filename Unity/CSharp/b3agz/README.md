# Mechanically Challenged Slopes and Loops

## Overview
This project was a little truncated due to entering an overlapping game jam, but I'm happy with the result. No revolutionary methods were used, with popular videos like the one by [Sebastian Lague](https://www.youtube.com/watch?v=cWpFZbjtSQg) being referenced during the development process.

The Portals work by placing a relatively positioned camera behind the portal and drawing what it sees onto the texture of the other portal. Transportation is handled determining the moment a teleportable object crosses the portal threshold and moving them to the relative position at the other portal.

Additional aspects include inverting the velocity of physics objects, ensuring that portals cannot be be placed partially inside walls, and creating a "mirror" object at the other portal so that objects are visible on both sides of the portal.

I have built this project to work with Unity physics objects out of the box. Any object with a Rigidbody attached that is being moved purely by physics should work with these portals (once they are tagged as teleportable) without any additional scripting or setup.

## Known Bugs/Issues
    Given the nature of the challenge, there are a few bugs that I decided to leave alone rather than spending more time on the project.

### Mirror Rotation
    In certain situations, the relative rotation of the "mirrored" objects gets messed up, breaking the illusion of portal mechanics. This seems to happen most often when the portals are opposite each other but one is horizontally aligned and one is vertically aligned.

### Portal Camera Obstructions
    Because the portal's visual is created by a camera, it is possible for objects to come between the camera and the portal, which are then drawn onto the texture of the other portal. The solution to this problem is to modify the portal camera's near-clipping plane so that it starts where the portal is and only draws things on the other side. However, in Unity you can only easily modify the distance of the near-clipping plane, not the rotation. Modifying the rotation involves using matrices, but I was unable to figure this out in the time I allotted to the challenge.

### Shoddy Character Controller
    I wanted to set up this project to work with any Unity physics object "out of the box", and I believe I achieved that. I did not get around to writing a character controller to work with this system, so the character controller in use is a simple physics-based affair that is... not good. It works with the portals, but if this was a game I would be rewriting the character controller.

## How to Play

Use WASD to move around and the mouse to look around. Press space to jump. The left and right mouse buttons will fire portals, and pressing B will fire a "test ball" that can bounce around the room and go through portals.

## How to Use

There is a running build in the "Build" folder. If you just want to test the project, I recommend using that. You can also open the project and run the "Main" scene.

To use portals in your project, simply drag the two portal prefabs and the player prefab into your scene. Regarding setup, the two portals need a reference to each other in the inspector, and any object that can pass through a portal needs to be tagged as `Teleportable`. Finally, tag any surface you want to allow portals to be placed on as `PortalSurface`.

Remember, the portals will work out of the box with regular physics objects, but any objects with code that adjusts position or rotation will need to modified to work with the portals.