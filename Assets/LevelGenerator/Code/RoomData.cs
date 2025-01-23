using System.Collections.Generic;
using UnityEngine;
using static Cats.LevelGenerator.Enums;

namespace Cats.LevelGenerator
{
    public class RoomData
    {
        // NOTE: Does this need to know its own position?
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

        public void UpdateAdjacentRooms(int[,] _directions, int[,] _roomGrid, int _floorXSize, int _floorYSize)
        {
            m_adjacentRooms.Clear();
            for (int i = 0; i < _directions.GetLength(0); i++)
            {
                int xDirection = _directions[i, 0];
                int yDirection = _directions[i, 1];
                int adjacentXPosition = (int)m_position.x + xDirection;
                int adjacentYPosition = (int)m_position.y + yDirection;

                if (adjacentXPosition >= _floorXSize || adjacentXPosition < 0 || adjacentYPosition >= _floorYSize || adjacentYPosition < 0)
                    continue;

                if (_roomGrid[adjacentYPosition, adjacentXPosition] != 0)
                    m_adjacentRooms.Add(new Vector2(xDirection, yDirection));
            }
            m_isDeadEnd = m_adjacentRooms.Count <= 1 ? true : false;
        }
    }
}
