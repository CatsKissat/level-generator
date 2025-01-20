using UnityEngine;
using static Cats.LevelGenerator.Enums;

namespace Cats.LevelGenerator
{
    public class RoomData : MonoBehaviour
    {
        public Vector2 m_position;
        public RoomType m_roomType;
        public bool m_isDeadEnd;

        public RoomData(Vector2 _position, RoomType _roomValue)
        {
            m_position = _position;
            m_roomType = _roomValue;
        }
    }
}
