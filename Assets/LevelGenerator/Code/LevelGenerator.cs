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
        private static RoomPrefabs m_roomPrefabData;
        private static int m_distanceToBoss;
        private static float m_roomSplitValue = 1.5f;
        private static int[,] m_roomGrid;

        public static void GenerateLevel(Difficulty _difficulty, int _currentFloor, RoomPrefabs _roomPrefabData)
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
            int randomWalkLoop = m_roomCount;
            do
            {
                RandomWalkFromEntranceToBoss();
                if (InfiniteLoop(ref randomWalkLoop))
                    break;
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

            // Instantiate prefab rooms
            foreach (var room in m_rooms)
            {
                GameObject roomPrefab = Object.Instantiate(m_roomPrefabData.GetRoomPrefab(room));
                roomPrefab.transform.SetParent(cubes.transform);
                roomPrefab.transform.localPosition = new Vector2(room.GetPosition.x, room.GetPosition.y);
            }
        }

        private static void RandomWalkFromEntranceToBoss()
        {
            Vector2 position = Vector2.zero;
            if (m_currentRoomIndex == 0) // Place the Entrance Room to the middle of the Room Grid
            {
                position.x = m_floorXSize / 2;
                position.y = m_floorYSize / 2;
            }
            else // Place path to the Boss Room
            {
                position = GetNextValidPosition();
            }
            PlaceRoomLayout(position);
        }

        private static Vector2 GetNextValidPosition()
        {
            Vector2 nextRoomPosition = Vector2.zero;
            bool hasValidAdjacentSpaces = false;
            int adjacentRoomLoop = 4;
            int randomDirectionValue = RandomiseDirectionValue();
            do
            {
                nextRoomPosition = GetValidRoomPosition(ref randomDirectionValue);
                //Debug.Log($"nextRoomPosition: {nextRoomPosition}");
                hasValidAdjacentSpaces = HasValidAdjacentRooms(nextRoomPosition, randomDirectionValue);
                //Debug.Log($"nextRoomPosition: {nextRoomPosition}, {m_currentRoomIndex}] adjacentRoomLoop: {adjacentRoomLoop}, isValidPosition: {hasValidAdjacentSpaces}");

                if (!hasValidAdjacentSpaces)
                    IncreaseDirectionValue(ref randomDirectionValue);

                if (InfiniteLoop(ref adjacentRoomLoop))
                    break;
            } while (!hasValidAdjacentSpaces);
            //Debug.LogWarning($"{m_currentRoomIndex}] found valid position.");
            return nextRoomPosition;
        }

        private static Vector2 GetValidRoomPosition(ref int directionValue)
        {
            int lastIndex = m_rooms.Count - 1;
            Vector2 previousRoomPosition = m_rooms[lastIndex].GetPosition;
            bool isNextPositionValid = false;
            int newRoomLoop = 4;
            Vector2 nextRoomPosition = Vector2.zero;
            do
            {
                Vector2 directionToAdjacentRoom = Direction(directionValue);
                nextRoomPosition = previousRoomPosition + directionToAdjacentRoom;
                isNextPositionValid = IsValidPosition(nextRoomPosition);
                //Debug.Log($"[{m_currentRoomIndex}] GetNextRoomPosition {nextRoomPosition}, newRoomLoop: {newRoomLoop}, direction {directionValue}, isNextPositionValid {isNextPositionValid}");

                if (!isNextPositionValid)
                    IncreaseDirectionValue(ref directionValue);

                if (InfiniteLoop(ref newRoomLoop))
                    break;
            } while (!isNextPositionValid);
            return nextRoomPosition;
        }

        private static void IncreaseDirectionValue(ref int _randomisedDirectionValue)
        {
            _randomisedDirectionValue++;
            if (_randomisedDirectionValue > 3)
                _randomisedDirectionValue = 0;
        }

        private static int RandomiseDirectionValue()
        {
            return Random.Range(0, 4);
        }

        private static bool InfiniteLoop(ref int _loop)
        {
            if (_loop < 0)
            {
                Debug.LogError("Ending infinite do-while loop.");
                return true;
            }
            _loop--;
            return false;
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

        private static void PlaceRoomLayout(Vector2 _position)
        {
            RoomType roomType = RoomType.None;
            roomType = GetRoomType();

            if (roomType == RoomType.None)
                Debug.LogError("RoomType can't be None!");

            int lastInIndex = 0;
            int distanceToEntrance = 0;
            if (m_rooms.Count >= 1)
            {
                lastInIndex = m_rooms.Count - 1;
                distanceToEntrance = m_rooms[lastInIndex].DistanceToEntrance + 1;
            }
            RoomData room = new RoomData(_position, roomType, distanceToEntrance);
            m_rooms.Add(room);
            lastInIndex = m_rooms.Count - 1;

            m_roomGrid[(int)_position.y, (int)_position.x] = (int)roomType;
            m_currentRoomIndex++;
            m_roomsLeft--;

            //UpdateAdjacentRooms(room);
        }

        private static void UpdateAdjacentRooms(RoomData _room)
        {
            _room.UpdateAdjacentRooms(Directions(), m_roomGrid, m_floorXSize, m_floorYSize);
        }

        private static Vector2 Direction(int _directionValue)
        {
            switch (_directionValue)
            {
                case 0:
                    return Vector2.up;
                case 1:
                    return Vector2.right;
                case 2:
                    return Vector2.down;
                case 3:
                    return Vector2.left;
            }
            return Vector2.zero;
        }

        private static Vector2[] Directions()
        {
            Vector2[] directions = new Vector2[4] { Vector2.up,
                                                    Vector2.right,
                                                    Vector2.down,
                                                    Vector2.left };
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

        private static bool HasValidAdjacentRooms(Vector2 _position, int _directionValue)
        {
            int negativeDirectionValue = 0;
            if (_directionValue == 0)
                negativeDirectionValue = 2;
            else if (_directionValue == 1)
                negativeDirectionValue = 3;
            else if (_directionValue == 2)
                negativeDirectionValue = 0;
            else if (_directionValue == 3)
                negativeDirectionValue = 1;

            int directions = 4;
            for (int i = 0; i < directions; i++)
            {
                if (i != negativeDirectionValue)
                {
                    Vector2 newPosition = _position + Direction(i);

                    if (newPosition.x < 0 || newPosition.x >= m_floorXSize) // Adjacent position is out of bounds and it is ok
                        continue;

                    if (newPosition.y < 0 || newPosition.y >= m_floorYSize) // Adjacent position is out of bounds and it is ok
                        continue;

                    //Debug.Log($"Checking {newPosition}, direction index: {i}");
                    if (!IsNewRoomPositionEmpty(newPosition))
                    {
                        //Debug.Log("IsNewRoomPositionEmpty is FALSE");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
