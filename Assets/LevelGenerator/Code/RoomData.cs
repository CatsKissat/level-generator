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
        private Vector2[] m_adjacentRooms = new Vector2[4];
        private int m_distanceToEntrance;
        public Vector2 GetPosition => m_position;
        public RoomType RoomType => m_roomType;
        public bool IsDeadEnd => m_isDeadEnd;
        public int DistanceToEntrance
        {
            get { return m_distanceToEntrance; }
            set { m_distanceToEntrance = value; }
        }

        public RoomData(Vector2 _position, RoomType _roomValue, int _distanceFromEntrance)
        {
            m_position = _position;
            m_roomType = _roomValue;
            m_distanceToEntrance = _distanceFromEntrance;
        }

        private int adjacentRooms;
        public int AdjacentRooms => adjacentRooms;
        public void UpdateAdjacentRooms(int[,] _directions, int[,] _roomGrid, int _floorXSize, int _floorYSize)
        {
            adjacentRooms = 0;
            m_adjacentRooms = new Vector2[4];
            for (int i = 0; i < _directions.GetLength(0); i++)
            {
                int adjacentXPosition = (int)m_position.x + _directions[i, 0];
                int adjacentYPosition = (int)m_position.y + _directions[i, 1];

                if (adjacentXPosition >= _floorXSize || adjacentXPosition < 0 || adjacentYPosition >= _floorYSize || adjacentYPosition < 0)
                    continue;

                if (_roomGrid[adjacentYPosition, adjacentXPosition] != 0)
                {
                    m_adjacentRooms[i] = new Vector2(_directions[i, 0], _directions[i, 1]);
                    adjacentRooms++;
                }
            }
            m_isDeadEnd = adjacentRooms <= 1 ? true : false;
        }
    }
}
