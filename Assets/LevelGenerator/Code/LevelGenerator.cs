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
        private static int m_roomsLeft;
        private static int m_maxXSize = 11;
        private static int m_maxYSize = 11;
        private static int m_currentRoomIndex;
        private static List<RoomData> m_rooms = new List<RoomData>();
        private static List<RoomData> m_deadEndRooms = new List<RoomData>();
        private static Rooms m_roomPrefabData;
        private static int m_distanceToBoss;
        private static float m_roomSplitValue = 1.5f;

        public static void GenerateLevel(Difficulty _difficulty, int _currentFloor, Rooms _roomPrefabData)
        {
            m_roomPrefabData = _roomPrefabData;
            IsLevelAlreadyGenerated();
            m_roomCount = CalculateRoomCount(_difficulty, _currentFloor);
            m_roomsLeft = m_roomCount;
            m_roomCount = (int)(m_roomCount / m_roomSplitValue);
            Debug.Log("m_currentFloorRoomCount: " + m_roomCount);
            RandomWalkFromEntranceToBoss();
            GenerateOtherPaths();
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
            m_roomCount = 0;
            m_isLevelGenerated = false;
            m_currentRoomIndex = 0;
            m_rooms.Clear();

            // Debug. Remove the placeholder room sprites
            for (int i = 0; i < cubes.transform.childCount; i++)
            {
                Object.Destroy(cubes.transform.GetChild(i).gameObject);
            }
        }

        public static void RandomWalkFromEntranceToBoss()
        {
            if (m_currentRoomIndex < m_roomCount)
            {
                int debugExitLoop = 0;
                do
                {
                    Debug.Log("_currentRoomIndex: " + m_currentRoomIndex);
                    PlaceRoomLayout();
                    debugExitLoop++;
                    if (debugExitLoop >= 10)
                    {
                        Debug.LogError("Breaking the debugExitLoop loop. Too many tries.");
                        break;
                    }
                } while (m_currentRoomIndex < m_roomCount);

                DebugVisualiseLayout();

                Debug.Log($"Total rooms is {m_roomCount}");
                m_isLevelGenerated = true;

                UpdateAdjacentRooms();

                for (int i = 0; i < m_rooms.Count; i++)
                {
                    Debug.Log($"Room [{i}] has {m_rooms[i].GetAdjacentRooms.Count} and it is deadend: {m_rooms[i].IsDeadEnd}");
                }
            }
            else
            {
                Debug.LogWarning("Can't generate more. The floor layout is completed.");
            }
        }

        private static void GenerateOtherPaths()
        {
            m_currentRoomIndex = 0;

            //SelectValidRoom();
            //RandomiseDirection();
            //CheckIsPositionValid();
            //PlaceRoom();
        }

        private static GameObject cubes = new GameObject("Floor");
        private static Texture texture;
        private static void DebugVisualiseLayout()
        {
            foreach (var room in m_rooms)
            {
                GameObject roomPrefab = Object.Instantiate(m_roomPrefabData.GetRoomPrefab(room));
                roomPrefab.transform.SetParent(cubes.transform);
                roomPrefab.transform.localPosition = new Vector2(room.GetPosition.x, room.GetPosition.y);
            }
        }

        private static void PlaceRoomLayout()
        {
            RoomType roomType = RoomType.None;
            Vector2 position = Vector2.zero;
            bool isDeadEnd = false;
            if (m_currentRoomIndex == 0) // Place Entrance Room to the middle
            {
                position.x = m_maxXSize / 2;
                position.y = m_maxYSize / 2;
                roomType = RoomType.Entrance;
                isDeadEnd = true;
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

                if (m_currentRoomIndex == m_roomCount - 1)
                {
                    roomType = RoomType.Boss;
                    isDeadEnd = true;
                }
                else
                {
                    roomType = RoomType.Normal;
                }
            }

            int lastInIndex = 0;
            int distanceToEntrance = 0;
            Vector2 lastPosition = Vector2.zero;
            if (m_rooms.Count >= 1)
            {
                lastInIndex = m_rooms.Count - 1;
                lastPosition = m_rooms[lastInIndex].GetPosition;
                distanceToEntrance = m_rooms[lastInIndex].DistanceToEntrance + 1;
            }
            RoomData room = new RoomData(position, roomType, isDeadEnd, distanceToEntrance);
            m_rooms.Add(room);
            if (room.RoomType == RoomType.Boss)
                m_distanceToBoss = distanceToEntrance;
            lastInIndex = m_rooms.Count - 1;

            //// Add room to the previous room adjacent room list
            //if (m_rooms.Count > 1)
            //    m_rooms[lastInIndex].AddAdjacentRoom(room);

            m_currentRoomIndex++;
            m_roomsLeft--;

            // Debug
            lastPosition = m_rooms[lastInIndex].GetPosition;
            Debug.Log($"Last Position: {lastPosition.x}, {lastPosition.y}");
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
            if (position.x < 0 || position.x >= m_maxXSize) // Position X is out of bounds
                return false;

            if (position.y < 0 || position.y >= m_maxYSize) // Position Y is out of bounds
                return false;

            return IsRoomPositionEmpty(position);
        }

        private static bool IsRoomPositionEmpty(Vector2 position)
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

                    if (newPosition.x < 0 || newPosition.x >= m_maxXSize) // Adjacent position is out of bounds and it is ok
                        continue;

                    if (newPosition.y < 0 || newPosition.y >= m_maxYSize) // Adjacent position is out of bounds and it is ok
                        continue;

                    if (!IsRoomPositionEmpty(newPosition))
                        return false;
                }
            }
            return true;
        }

        private static void UpdateAdjacentRooms()
        {
            int adjacentRooms = 0;
            int[,] directions = Directions();
            foreach (var room in m_rooms)
            {
                for (int i = 0; i < directions.GetLength(0); i++)
                {
                    float adjacentXPosition = room.GetPosition.x + directions[i, 0];
                    float adjacentYPosition = room.GetPosition.y + directions[i, 1];

                    foreach (var comparedRoom in m_rooms)
                    {
                        if (adjacentXPosition == comparedRoom.GetPosition.x && adjacentYPosition == comparedRoom.GetPosition.y)
                        {
                            room.AddAdjacentRoom(comparedRoom);
                            adjacentRooms++;
                        }
                    }
                }
            }
        }
    }
}
