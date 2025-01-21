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
        private List<RoomData> m_adjacentRooms = new List<RoomData>();
        private int m_distanceFromEntrance;
        public Vector2 GetPosition => m_position;
        public RoomType RoomType => m_roomType;
        public List<RoomData> GetAdjacentRooms => m_adjacentRooms;
        public bool IsDeadEnd => m_isDeadEnd;
        public int DistanceToEntrance
        {
            get { return m_distanceFromEntrance; }
            set { m_distanceFromEntrance = value; }
        }

        public RoomData(Vector2 _position, RoomType _roomValue, bool _isDeadEnd, int _distanceFromEntrance)
        {
            m_position = _position;
            m_roomType = _roomValue;
            m_isDeadEnd = _isDeadEnd;
            m_distanceFromEntrance = _distanceFromEntrance;
        }

        public void AddAdjacentRoom(RoomData _room)
        {
            m_adjacentRooms.Add(_room);
        }
    }
}
