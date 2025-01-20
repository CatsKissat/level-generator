using UnityEngine;
using static Cats.LevelGenerator.Enums;

namespace Cats.LevelGenerator
{
    public class RoomData : MonoBehaviour
    {
        public int m_xPosition;
        public int m_yPosition;
        public int m_roomValue;

        public RoomData(int _xPosition, int _yPosition, RoomType _roomValue)
        {
            m_xPosition = _xPosition;
            m_yPosition = _yPosition;
            m_roomValue = (int)_roomValue;
        }
    }
}
