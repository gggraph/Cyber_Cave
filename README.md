
<p align="center">
  <img src=git-content/highligh3.gif />
</p>

# CyberCave

CyberCave is a **multiplayer experience in VR** where users can walk, chat, paint together in a **persistent world**. 

The project was launched by Esad Orléans, this repository only contains script files of the Unity project. Cyber Cave development could continue in near future, but for the moment, the software appears **as a demo version**.

## Networking and Avatar

<p align="center">
  <img src=git-content/avatarsyncs.gif />
</p>

User automatically connect to the cybercave world where he can choose an avatar (skins) and interact with the world and other users. All movement (body parts, fingers...) and most actions are synchronised using Photon servers. Users can talk with the headset microphone, sound is spatialized.

## Tools

In this demo version, there is two main activities : painting and sculpting. 

###  Paint with others

<p align="center">
  <img src=git-content/painting.gif />
</p>

User can paint on any mesh who have uv coordinates using **painter** component. There is actually 3 types of painter : brushes, pens and sprays. All are customizable in the unity editor. Add **Paintable** component to any surface you would like to draw .

Painting actions are synchronized through the network, it allows multiple users to experiment drawing on the same canvas, share theirs paintings...

Paintable object's textures can be exported as .svg or .gcode files.

###  Sculpt mesh 

<p align="center">
  <img src=git-content/sculptingmc.gif />
</p>

Users can sculpt shapes by adding or substracting volumes with different tools (**Mooduler** component). Mesh generation works with marching cubes algorithm. Physics work with CCL algorithm. Generate default shapes by using **MoodulerCreator** component. Sculpts can only be painted with **Mooduler** gameobjects : shapes have no uvs, it works with vertex colors and triplanar mapping. 

Sculpting is synchronised. Shapes can be exported as .obj, .stl, .gcode files. 

## Hand interaction

**gif ici manquant**
Hand pose **recognition** is available. Users can interact the world with **sign langage**. 

## Server system, updates and persistence

<p align="center">
  <img src=git-content/servs.gif />
</p>

Most of users interaction with the world is saved server-side. At connection, users download latest world data to see what happened while they was off-line.
Any headset can act as a "server" and get authority on what is saved and what is the official state of the world with the use of a **single file** .

## Contributors

* Théo Bonnet, 3d modeling
* Lionel Broye, project director.
* Armandine Chasle, narrative designer
* Léon Denise, artist, vfx/shader programmer
* Gael Goutard, developper
