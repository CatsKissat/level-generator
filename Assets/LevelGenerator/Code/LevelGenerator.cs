using System.Collections.Generic;
using UnityEngine;
using static Cats.LevelGenerator.Enums;
using System.Reflection;

namespace Cats.LevelGenerator
{
    public static class LevelGenerator
    {
        private static int m_minRoomCount = 4;
        private static int m_maxRoomCount = 6;
        private static float m_levelMultiplier = 3.5f;
        //private static int m_extraRoomsPerLevel;
        //private static int m_maxFloors = 3;
        private static bool m_isLevelGenerated;
        //private static Rooms m_roomPrefabs;
        private static int m_currentFloorRoomCount;
        //public static int[,] m_floorLayout;
        private static int m_maxXSize = 11;
        private static int m_maxYSize = 11;
        private static int m_currentRoomIndex;
        private static List<RoomData> m_roomData = new List<RoomData>();
        private static GameObject m_roomPrefab;

        public static void GenerateLevel(Difficulty _difficulty, int _currentFloor, GameObject _roomPrefab)
        {
            m_roomPrefab = _roomPrefab;
            IsLevelAlreadyGenerated();
            m_currentFloorRoomCount = CalculateRoomCount(_difficulty, _currentFloor);
            Debug.Log("m_currentFloorRoomCount: " + m_currentFloorRoomCount);
            RandomWalkFromEntranceToBoss();
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
            int additionalRooms = Random.Range(m_minRoomCount, (m_maxRoomCount + 1));
            rooms += additionalRooms;
            rooms += _difficulty == Difficulty.Easy ? 0 : 5;
            return rooms;
        }

        private static void RemoveLevel()
        {
            m_currentFloorRoomCount = 0;
            m_isLevelGenerated = false;
            m_currentRoomIndex = 0;
            m_roomData.Clear();

            // Debug
            for (int i = 0; i < cubes.transform.childCount; i++)
            {
                Object.Destroy(cubes.transform.GetChild(i).gameObject);
            }
            ClearConsole();
        }

        private static void ClearConsole()
        {
            var assemly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assemly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        public static void RandomWalkFromEntranceToBoss()
        {
            if (m_currentRoomIndex < m_currentFloorRoomCount)
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
                } while (m_currentRoomIndex < m_currentFloorRoomCount);

                DebugVisualiseLayout();

                Debug.Log($"Total rooms is {m_currentFloorRoomCount}");
                m_isLevelGenerated = true;
            }
            else
            {
                Debug.LogWarning("Can't generate more. The floor layout is completed.");
            }
        }

        private static GameObject cubes = new GameObject("Floor");
        private static Texture texture;
        private static void DebugVisualiseLayout()
        {
            foreach (var room in m_roomData)
            {
                GameObject roomPrefab = Object.Instantiate(m_roomPrefab);
                roomPrefab.transform.SetParent(cubes.transform);
                roomPrefab.transform.localPosition = new Vector2(room.m_xPosition, room.m_yPosition);
            }
        }

        private static int[] m_lastPosition = new int[2];
        private static void PlaceRoomLayout()
        {
            RoomType roomType = RoomType.None;
            int xPosition = 0;
            int yPosition = 0;
            if (m_currentRoomIndex == 0) // Place Entrance
            {
                xPosition = m_maxXSize / 2;
                yPosition = m_maxYSize / 2;
                roomType = RoomType.Entrance;
            }
            else // Place path to the boss
            {
                int randomDirection = 0;
                //Debug.Log("Randomising direction");
                randomDirection = Random.Range(0, 4);
                bool isValidPosition = false;
                int attempts = 0;
                do
                {
                    xPosition = m_lastPosition[0];
                    yPosition = m_lastPosition[1];
                    int[,] directions = new int[,] { { 0, -1 }, // 0 = up
                                                     { 1,  0 }, // 1 = right
                                                     { 0,  1 }, // 2 = down
                                                     { -1, 0 }  // 3 = left
                                                   };

                    int tries = 1;
                    bool isNextPositionValid = false;
                    do
                    {
                        //Debug.Log($"Direction: {randomDirection}");
                        int newXPosition = xPosition + directions[randomDirection, 0];
                        int newYPosition = yPosition + directions[randomDirection, 1];

                        isNextPositionValid = IsValidPosition(newXPosition, newYPosition);

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

                    xPosition += directions[randomDirection, 0];
                    yPosition += directions[randomDirection, 1];

                    // Check is new position valid
                    isValidPosition = HasValidNeighbours(directions, randomDirection, xPosition, yPosition);

                    if (!isValidPosition)
                    {
                        //Debug.Log("Is not valid neighbour. Setting next direction.");
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

                if (m_currentRoomIndex == m_currentFloorRoomCount - 1)
                    roomType = RoomType.Boss;
                else
                    roomType = RoomType.Normal;
            }

            m_lastPosition[0] = xPosition;
            m_lastPosition[1] = yPosition;
            Debug.Log($"LastPosition: {m_lastPosition[0]}, {m_lastPosition[1]}");
            m_currentRoomIndex++;
            m_roomData.Add(new RoomData(xPosition, yPosition, roomType));
        }

        private static bool IsValidPosition(int xPosition, int yPosition)
        {
            //Debug.LogWarning($"Checking new position {xPosition}, {yPosition} from {m_lastPosition[0]}, {m_lastPosition[1]}");

            if (xPosition < 0 || xPosition >= m_maxXSize) // Position X is out of bounds
                return false;

            if (yPosition < 0 || yPosition >= m_maxYSize) // Position Y is out of bounds
                return false;

            foreach (var room in m_roomData)
                if (xPosition == room.m_xPosition && yPosition == room.m_yPosition)
                    return false;

            return true;
        }

        private static bool HasValidNeighbours(int[,] _directions, int _direction, int _xPosition, int _yPosition)
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

            for (int x = 0; x < _directions.GetLength(0); x++)
            {
                if (negativeDirection != x)
                {
                    int newXPos = _xPosition + _directions[x, 0];
                    int newYPos = _yPosition + _directions[x, 1];

                    if (newXPos < 0 || newXPos >= m_maxXSize)
                    {
                        //Debug.LogWarning($"newXPos ({newXPos}) is out of bounds and it is valid adjacent room. Continue loop");
                        continue;
                    }

                    if (newYPos < 0 || newYPos >= m_maxYSize)
                    {
                        //Debug.LogWarning($"newYPos ({newYPos}) is out of bounds and it is valid adjacent room. Continue loop");
                        continue;
                    }

                    foreach (var room in m_roomData)
                    {
                        if (newXPos == room.m_xPosition && newYPos == room.m_yPosition)
                        {
                            Debug.Log($"A room already exists in {newXPos}, {newYPos} and thus is not valid location.");
                            return false;
                        }
                    }
                }
            }
            //Debug.Log("Is valid");
            return true;
        }
    }
}
