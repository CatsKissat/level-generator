This level generator takes inspiration from The Binding of Isaac and is currently work in progress in Unity.

The idea is that the generator creates 2D int array and randomises room types into it. 0 is empty, 1 is the Entrance Room, 2 is Normal Room and 3 is the Boss Room. Example layout looks like this:
[0] [0] [0] [0] [0]
[0] [0] [0] [0] [0]
[0] [0] [1] [2] [0]
[0] [0] [0] [2] [0]
[0] [0] [3] [2] [0]

When the layout is ready the generator will create a floor by using it. Currently it uses 1x1 2D textures for the the rooms, but later I will replace them to be randomised rooms from a arrays of hand crafted room.

Todo:
- Make branching corridors
- Create arrays of hand crafted rooms
- Add tiles for the visualisation
- Add different kinds of room types
