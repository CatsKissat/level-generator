using System.Collections.Generic;
using UnityEngine;
using static Cats.LevelGenerator.Enums;

namespace Cats.LevelGenerator
{
    public class RoomData
    {
        private Vector2 m_position;
        private RoomType m_roomType;
        private bool m_isDeadEnd;
        private List<Vector2> m_adjacentRooms = new List<Vector2>();
        private int m_distanceToEntrance;
        public Vector2 GetPosition => m_position;
        public RoomType RoomType => m_roomType;
        public bool IsDeadEnd => m_isDeadEnd;
        public int DistanceToEntrance
        {
            get { return m_distanceToEntrance; }
            set { m_distanceToEntrance = value; }
        }
        public int AdjacentRooms => m_adjacentRooms.Count;

        public RoomData(Vector2 _position, RoomType _roomValue, int _distanceFromEntrance)
        {
            m_position = _position;
            m_roomType = _roomValue;
            m_distanceToEntrance = _distanceFromEntrance;
        }

        public void UpdateAdjacentRooms(Vector2[] _directions, int[,] _roomGrid, int _floorXSize, int _floorYSize)
        {
            m_adjacentRooms.Clear();
            int directions = 4;
            for (int i = 0; i < directions; i++)
            {
                Vector2 direction = _directions[i];
                Vector2 adjacentPosition = m_position + direction;

                if (adjacentPosition.x >= _floorXSize || adjacentPosition.x < 0 || adjacentPosition.y >= _floorYSize || adjacentPosition.y < 0)
                    continue;

                if (_roomGrid[(int)adjacentPosition.y, (int)adjacentPosition.x] != 0)
                    m_adjacentRooms.Add(direction);
            }
            m_isDeadEnd = m_adjacentRooms.Count <= 1 ? true : false;
        }
    }
}
