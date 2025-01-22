using System.Collections.Generic;
using UnityEngine;
using static Cats.LevelGenerator.Enums;

namespace Cats.LevelGenerator
{
    public static class LevelGenerator
    {
        private static int m_minBaseRoomCount = 4;
        private static int m_maxBaseRoomCount = 6;
        private static float m_levelMultiplier = 3.5f;
        private static bool m_isLevelGenerated;
        private static int m_roomCount;
        private static int m_totalRoomsForFloor;
        private static int m_roomsLeft;
        private static int m_floorXSize = 11;
        private static int m_floorYSize = 11;
        private static int m_currentRoomIndex;
        private static List<RoomData> m_rooms = new List<RoomData>();
        private static List<RoomData> m_deadEndRooms = new List<RoomData>();
        private static Rooms m_roomPrefabData;
        private static int m_distanceToBoss;
        private static float m_roomSplitValue = 1.5f;
        private static int[,] m_roomGrid;

        public static void GenerateLevel(Difficulty _difficulty, int _currentFloor, Rooms _roomPrefabData)
        {
            IsLevelAlreadyGenerated();
            m_roomGrid = new int[m_floorYSize, m_floorXSize];
            m_roomPrefabData = _roomPrefabData;
            m_totalRoomsForFloor = CalculateRoomCount(_difficulty, _currentFloor);
            m_roomsLeft = m_totalRoomsForFloor;
            m_distanceToBoss = (int)(m_totalRoomsForFloor / m_roomSplitValue);
            m_roomCount = m_distanceToBoss;
            Debug.Log("m_totalRoomsForFloor: " + m_totalRoomsForFloor);
            GeneratePathToBoss();
            GenerateBranches();

            DebugVisualiseLayout();

            m_isLevelGenerated = true;

            // Debug
            Debug.Log($"Total rooms count {m_totalRoomsForFloor}");
            for (int i = 0; i < m_rooms.Count; i++)
                Debug.Log($"Room [{i}] has {m_rooms[i].AdjacentRooms} Adjacent Rooms and it is deadend: {m_rooms[i].IsDeadEnd}");
        }

        private static void IsLevelAlreadyGenerated()
        {
            if (m_isLevelGenerated)
            {
                Debug.Log("Level is already generated. Removing old level");
                RemoveLevel();
            }
        }

        private static int CalculateRoomCount(Difficulty _difficulty, int _currentFloor)
        {
            int rooms = (int)(_currentFloor * m_levelMultiplier);
            int additionalRooms = Random.Range(m_minBaseRoomCount, (m_maxBaseRoomCount + 1));
            rooms += additionalRooms;
            rooms += _difficulty == Difficulty.Easy ? 0 : 5;
            return rooms;
        }

        private static void RemoveLevel()
        {
            m_totalRoomsForFloor = 0;
            m_roomCount = 0;
            m_roomsLeft = 0;
            m_isLevelGenerated = false;
            m_currentRoomIndex = 0;
            m_rooms.Clear();
            m_roomGrid = null;

            // Debug. Remove the placeholder room sprites
            for (int i = 0; i < cubes.transform.childCount; i++)
            {
                Object.Destroy(cubes.transform.GetChild(i).gameObject);
            }
        }

        public static void GeneratePathToBoss()
        {
            int randomWalkLoop = 0;
            do
            {
                RandomWalkFromEntranceToBoss();
                randomWalkLoop++;
                if (randomWalkLoop >= 10)
                {
                    Debug.LogError("Breaking the RandomWalkFromEntranceToBoss loop. Too many tries.");
                    break;
                }
            } while (m_currentRoomIndex < m_roomCount);
        }

        private static void GenerateBranches()
        {
            //RandomiseDirection();
            //SelectValidRoom();
            //bool isValidPosition = false;
            //isValidPosition = IsNewRoomPositionEmpty();
            // HasValidAdjacentRooms();
            //roomType = GetRoomType();
            //PlaceRoomLayout(roomType, position, isDeadEnd);
        }

        private static GameObject cubes = new GameObject("Floor");
        private static Texture texture;
        private static void DebugVisualiseLayout()
        {
            // Instantiate prefab rooms
            foreach (var room in m_rooms)
            {
                GameObject roomPrefab = Object.Instantiate(m_roomPrefabData.GetRoomPrefab(room));
                roomPrefab.transform.SetParent(cubes.transform);
                roomPrefab.transform.localPosition = new Vector2(room.GetPosition.x, room.GetPosition.y);
            }

            foreach (var room in m_rooms)
                UpdateAdjacentRooms(room);

            // Print level grid to Console
            string levelAsString = "";
            for (int y = m_roomGrid.GetLength(0) - 1; y >= 0; y--)
            {
                for (int x = 0; x < m_roomGrid.GetLength(1); x++)
                {
                    levelAsString += $"{m_roomGrid[y, x]} ";
                }
                levelAsString += "\n";
            }
            Debug.Log(levelAsString);
        }

        private static void RandomWalkFromEntranceToBoss()
        {
            Vector2 position = Vector2.zero;
            if (m_currentRoomIndex == 0) // Place Entrance Room to the middle
            {
                position.x = m_floorXSize / 2;
                position.y = m_floorYSize / 2;
            }
            else // Place path to the Boss Room
            {
                int randomDirection = 0;
                //Debug.Log("Randomising direction");
                randomDirection = Random.Range(0, 4);
                bool isValidPosition = false;
                int attempts = 0;
                do
                {
                    int lastIndex = m_rooms.Count - 1;
                    position = m_rooms[lastIndex].GetPosition;
                    int[,] directions = Directions();

                    int tries = 1;
                    bool isNextPositionValid = false;
                    do
                    {
                        //Debug.Log($"Direction: {randomDirection}");
                        float newXPosition = position.x + directions[randomDirection, 0];
                        float newYPosition = position.y + directions[randomDirection, 1];
                        Vector2 newPosition = new Vector2(newXPosition, newYPosition);

                        isNextPositionValid = IsValidPosition(newPosition);

                        if (!isNextPositionValid)
                        {
                            //Debug.Log("Is not valid position for next room. Setting next direction.");
                            randomDirection++;
                            if (randomDirection > 3)
                                randomDirection = 0;
                            //Debug.Log($"New direction is: {randomDirection}");
                        }

                        tries++;
                        if (tries > 4)
                        {
                            Debug.LogError("The new room position is illegal. Probably a deadend.");
                            break;
                        }
                    } while (!isNextPositionValid);

                    position.x += directions[randomDirection, 0];
                    position.y += directions[randomDirection, 1];

                    // Check are the adjacent positions valid
                    isValidPosition = HasValidAdjacentRooms(position, directions, randomDirection);

                    if (!isValidPosition)
                    {
                        //Debug.Log("Is not valid adjacent room. Setting next direction.");
                        randomDirection++;
                        if (randomDirection > 3)
                            randomDirection = 0;
                        //Debug.Log($"New direction is: {randomDirection}");
                    }

                    attempts++;
                    if (attempts >= 10)
                    {
                        Debug.LogError("Breaking the loop");
                        break;
                    }
                } while (!isValidPosition);
            }

            RoomType roomType = RoomType.None;
            roomType = GetRoomType();

            PlaceRoomLayout(roomType, position);
            m_roomGrid[(int)position.y, (int)position.x] = (int)roomType;
        }

        private static RoomType GetRoomType()
        {
            RoomType roomType = RoomType.None;

            if (m_currentRoomIndex == 0)
                roomType = RoomType.Entrance;
            else if (m_currentRoomIndex == m_distanceToBoss - 1)
                roomType = RoomType.Boss;
            else
                roomType = RoomType.Normal;

            return roomType;
        }

        private static void PlaceRoomLayout(RoomType _roomType, Vector2 _position)
        {
            if (_roomType == RoomType.None)
                Debug.LogError("RoomType can't be None!");

            int lastInIndex = 0;
            int distanceToEntrance = 0;
            if (m_rooms.Count >= 1)
            {
                lastInIndex = m_rooms.Count - 1;
                distanceToEntrance = m_rooms[lastInIndex].DistanceToEntrance + 1;
            }
            RoomData room = new RoomData(_position, _roomType, distanceToEntrance);
            m_rooms.Add(room);
            lastInIndex = m_rooms.Count - 1;

            m_currentRoomIndex++;
            m_roomsLeft--;

            //UpdateAdjacentRooms(room);
        }

        private static void UpdateAdjacentRooms(RoomData _room)
        {
            _room.UpdateAdjacentRooms(Directions(), m_roomGrid, m_floorXSize, m_floorYSize);
        }

        private static int[,] Directions()
        {
            int[,] directions = new int[,] { { 0, -1 },   // 0 = up
                                             { 1,  0 },   // 1 = right
                                             { 0,  1 },   // 2 = down
                                             { -1, 0 } }; // 3 = left
            return directions;
        }

        private static bool IsValidPosition(Vector2 position)
        {
            if (position.x < 0 || position.x >= m_floorXSize) // Position X is out of bounds
                return false;

            if (position.y < 0 || position.y >= m_floorYSize) // Position Y is out of bounds
                return false;

            return IsNewRoomPositionEmpty(position);
        }

        private static bool IsNewRoomPositionEmpty(Vector2 position)
        {
            foreach (var room in m_rooms) // Check is there already a room on that position
                if (position.x == room.GetPosition.x && position.y == room.GetPosition.y)
                    return false;
            return true;
        }

        private static bool HasValidAdjacentRooms(Vector2 _position, int[,] _directions, int _direction)
        {
            int negativeDirection = 0;
            if (_direction == 0)
                negativeDirection = 2;
            else if (_direction == 1)
                negativeDirection = 3;
            else if (_direction == 2)
                negativeDirection = 0;
            else if (_direction == 3)
                negativeDirection = 1;

            for (int i = 0; i < _directions.GetLength(0); i++)
            {
                if (negativeDirection != i)
                {
                    float newXPos = _position.x + _directions[i, 0];
                    float newYPos = _position.y + _directions[i, 1];
                    Vector2 newPosition = new Vector2(newXPos, newYPos);

                    if (newPosition.x < 0 || newPosition.x >= m_floorXSize) // Adjacent position is out of bounds and it is ok
                        continue;

                    if (newPosition.y < 0 || newPosition.y >= m_floorYSize) // Adjacent position is out of bounds and it is ok
                        continue;

                    if (!IsNewRoomPositionEmpty(newPosition))
                        return false;
                }
            }
            return true;
        }
    }
}
