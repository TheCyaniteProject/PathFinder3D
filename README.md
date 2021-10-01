# PathFinder3D
3D A* Pathfinding that doesn't need baked navmeshes and can be used with dynamically created terrain (MapMagic or other)

PathFinder3D (Also check out my original [PathFinder](https://github.com/TheCyaniteProject/PathFinder) for 2D games) is an asset that allows for pathfinding on dynamically created levels. I created this asset so I could have an easy to configure Pathfinding for [MapMagic 2](https://assetstore.unity.com/packages/tools/terrain/mapmagic-2-bundle-178682)

You can center the pathfinding grid on the level (pathfinding across the entire level), the player (pathfinding only near the player), or the agent (pathfinding that follows the agent).

Climbing things is more costly, so agents will prefer to go arround things when possible, but will still climb things to reach the player.

There's a layermask for what you define as 'terrain' (that the agent can move over/on), and one for what you defind as 'obsticle' (that the agent will avoid/go around)

The pathfinding drops tracking nodes down on to the terrain layers, and then enables/disables them if obstructions are in the way.

![Obstruction Example](https://cdn.discordapp.com/attachments/869651649755492352/893189441282269194/unknown.png)
![Grid Following Agent](https://cdn.discordapp.com/attachments/869651649755492352/893211061451386880/3D_Pathfinding_Demo2.gif)
