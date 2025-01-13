using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using static LevelGenerator.Enums;

namespace LevelGenerator
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
        //private static Dictionary m_generatedRooms;
        private static int m_roomCount;
        public static int[,] m_floorLayout;
        private static int m_maxXSize = 11;
        private static int m_maxYSize = 9;
        private static int m_currentRoomIndex;

        public static void GenerateLevel(Difficulty _difficulty, int _currentFloor)
        {
            IsLevelAlreadyGenerated();
            m_roomCount = CalculateRoomCount(_difficulty, _currentFloor);
            Debug.Log("m_currentFloorSize: " + m_roomCount);
            m_floorLayout = new int[m_maxXSize, m_maxYSize];
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
            m_roomCount = 0;
            m_isLevelGenerated = false;
            m_currentRoomIndex = 0;
        }

        public static void RandomWalkFromEntranceToBoss()
        //public static void RandomWalkFromEntranceToBoss()
        {
            if (m_currentRoomIndex < m_roomCount)
            {
                int debugExitLoop = 0;
                do
                {
                    //Debug.Log("_currentRoomIndex: " + _currentRoomIndex);
                    PlaceRoomLayout();
                    Debug.Log("_currentRoomIndex: " + m_currentRoomIndex);
                    //PlaceRoomLayout();
                    debugExitLoop++;
                    if (debugExitLoop >= 10)
                    {
                        Debug.LogError("Breaking the debugExitLoop loop. Too many tries.");
                        break;
                    }
                } while (m_currentRoomIndex < m_roomCount);

                // Debug
                string printableString = "";
                for (int y = 0; y < m_floorLayout.GetLength(1); y++)
                {
                    for (int x = 0; x < m_floorLayout.GetLength(0); x++)
                    {
                        printableString += m_floorLayout[x, y].ToString() + " ";
                    }
                    printableString += "\n";
                }
                Debug.Log(printableString);
                m_isLevelGenerated = true;
            }
            else
            {
                Debug.LogWarning("Can't generate more. The floor layout is completed.");
            }
        }

        private static int[] m_lastPosition = new int[2];
        private static void PlaceRoomLayout()
        //private static void PlaceRoomLayout()
        {
            int xPosition = 0;
            int yPosition = 0;
            //if (_currentRoomIndex == 0) // Place Entrance
            if (m_currentRoomIndex == 0) // Place Entrance
            {
                xPosition = m_floorLayout.GetLength(0) / 3;
                yPosition = m_floorLayout.GetLength(1) / 2;
                m_floorLayout[xPosition, yPosition] = (int)Enums.RoomType.Entrance;
            }
            else // Place path to the boss
            {
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

                    int randomDirection = 0;
                    randomDirection = Random.Range(0, 4);
                    // TODO: Check is direction empty
                    int tries = 0;
                    bool isValid = false;
                    do
                    {
                        int newXPosition = xPosition + directions[randomDirection, 0];
                        int newYPosition = yPosition + directions[randomDirection, 1];
                        // TODO: Check that the new position is not out of bounds
                        if (m_floorLayout[newXPosition, newYPosition] != 0)
                        {
                            randomDirection++;
                            if (randomDirection > 3)
                                randomDirection = 0;
                        }
                        else
                        {
                            isValid = true;
                        }

                        tries++;
                        if (tries > 10)
                        {
                            Debug.LogError("Breaking do while. Too much tries");
                            break;
                        }
                    } while (!isValid);


                    xPosition += directions[randomDirection, 0];
                    yPosition += directions[randomDirection, 1];

                    // Check is new position valid
                    isValidPosition = IsValid(directions, randomDirection, xPosition, yPosition);
                    attempts++;
                    if (attempts >= 10)
                    {
                        Debug.LogError("Breaking the loop");
                        break;
                    }
                } while (!isValidPosition);

                m_floorLayout[xPosition, yPosition] = 2;
            }

            m_lastPosition[0] = xPosition;
            m_lastPosition[1] = yPosition;
            Debug.Log($"LastPosition: {m_lastPosition[0] + 1}, {m_lastPosition[1] + 1}");
            //_currentRoomIndex++;
            m_currentRoomIndex++;
        }

        private static bool IsValid(int[,] _directions, int _direction, int _xPosition, int _yPosition)
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

                    Debug.Log("m_floorLayout.GetLength(0): " + m_floorLayout.GetLength(0));
                    if (newXPos < 0 || newXPos >= m_floorLayout.GetLength(0))
                    {
                        Debug.LogWarning($"newXPos ({newXPos}) is out of bounds and it is valid adjacent room. Continue loop");
                        continue;
                    }

                    Debug.Log("m_floorLayout.GetLength(1): " + m_floorLayout.GetLength(1));
                    if (newYPos < 0 || newYPos >= m_floorLayout.GetLength(1))
                    {
                        Debug.LogWarning($"newYPos ({newYPos}) is out of bounds and it is valid adjacent room. Continue loop");
                        continue;
                    }

                    Debug.LogWarning($"Checking is {newXPos + 1}, {newYPos + 1} valid from {_xPosition + 1}, {_yPosition + 1}");
                    if (m_floorLayout[newXPos, newYPos] != 0)
                    {
                        Debug.Log("Is not valid");
                        return false;
                    }
                }
            }
            Debug.Log("Is valid");
            return true;
        }
    }
}
