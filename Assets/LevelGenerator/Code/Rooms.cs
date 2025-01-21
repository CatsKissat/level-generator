using UnityEngine;
using static Cats.LevelGenerator.Enums;

namespace Cats.LevelGenerator
{
    [CreateAssetMenu(fileName = "RoomPrefabs", menuName = "Scriptable Objects/Room Prefabs")]
    public class Rooms : ScriptableObject
    {
        [SerializeField] private GameObject m_entranceRoomPrefab;
        [SerializeField] private GameObject m_normalRoomPrefab;
        [SerializeField] private GameObject m_bossRoomPrefab;

        public GameObject EntranceRoomPrefab => m_entranceRoomPrefab;
        public GameObject NormalRoomPrefab => m_normalRoomPrefab;
        public GameObject BossRoomPrefab => m_bossRoomPrefab;

        public GameObject GetRoomPrefab(RoomData _roomData)
        {
            switch (_roomData.RoomType)
            {
                case RoomType.None:
                    return null;
                case RoomType.Entrance:
                    return m_entranceRoomPrefab;
                case RoomType.Normal:
                    return m_normalRoomPrefab;
                case RoomType.Boss:
                    return m_bossRoomPrefab;
                case RoomType.Secret:
                    Debug.LogError("The Secret room prefab not implemented yet");
                    break;
                case RoomType.Exit:
                    Debug.LogError("The Exit room prefab not implemented yet");
                    break;
                case RoomType.Shop:
                    Debug.LogError("The Shop room prefab not implemented yet");
                    break;
                case RoomType.Special:
                    Debug.LogError("The Special room prefab not implemented yet");
                    break;
            }
            return null;
        }
    }
}
