
<p align="center">
  <img src=git-content/highligh3.gif />
</p>

# CyberCave

CyberCave is a multiplayer experience in VR where users can walk, chat, paint together in a persistant world. 

The project was launched by Esad Orléans, this repository contains all source files of the Unity project. Cyber Cave development could continue in near future, but for the moment, the software appears as a demo version.

## Networking and Avatar

<p align="center">
  <img src=git-content/avatarsyncs.gif />
</p>

User automatically connect to the cybercave world where he can choose an avatar (skins) and interact with the world and other users. All movement (body parts, fingers...) and most actions are synchronised using Photon servers. Users can talk with the headset microphone, sound is spatialized.

## Tools

In this demo version, there is two main activities you can share with others : painting and sculpting. 

###  Paint with others on any mesh 

<p align="center">
  <img src=git-content/painting.gif />
</p>

User can paint on any mesh who have uv coordinate by using painter objects. There is actually 3 type of painters : brushes, pens and sprays. All are customizable in the unity editor. You can add more painting tools by adding **Painter** component to any gameobject of the scene. You can add more surface for painting, adding **Paintable** component to any mesh of your choice.
Painting actions are synchronized through the network, it allows multiple users to experiment drawing on the same canvas, share theirs paintings...
Paintable object's textures can be exported as .svg file or .gcode files, ready to be print with 2d plotter.

###  Sculpt mesh with marching cubes

<p align="center">
  <img src=git-content/sculptingmc.gif />
</p>

Users can sculpt shapes by adding or substracting volumes with different tools. merge or split. Mesh generation works with marching cubes algorithm. Physics work with CCL algorithm. You can add more tools adding **Mooduler** component to any gamobject in the scene and generate default shapes using **MoodulerCreator** component. User can paint on vertex of marching cubes' meshes.

## Contributors

* Théo Bonnet, 3d modeling
* Lionel Broye, project director.
* Armandine Chasles, narrative designer
* Léon Denise, artist, vfx/shader programmer.
* Gael Goutard, developper
