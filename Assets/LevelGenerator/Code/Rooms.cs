using UnityEngine;

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
    }
}
