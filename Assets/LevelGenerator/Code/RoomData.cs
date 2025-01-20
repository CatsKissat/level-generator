using UnityEngine;
using static Cats.LevelGenerator.Enums;

namespace Cats.LevelGenerator
{
    public class RoomData
    {
        private Vector2 m_position;
        private RoomType m_roomType;
        private bool m_isDeadEnd;
        public Vector2 GetPosition => m_position;
        public RoomType RoomType => m_roomType;

        public RoomData(Vector2 _position, RoomType _roomValue)
        {
            m_position = _position;
            m_roomType = _roomValue;
        }
    }
}
