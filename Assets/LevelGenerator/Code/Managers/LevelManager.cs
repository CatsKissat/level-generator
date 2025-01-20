using UnityEngine;
using static Cats.LevelGenerator.Enums;

namespace Cats.LevelGenerator
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private GameObject m_roomPrefab;
        private int m_currentFloor;
        private Difficulty m_difficulty;

        public void GenerateLevelButton()
        {
            if (m_currentFloor == 0)
                m_currentFloor = 1;

            LevelGenerator.GenerateLevel(m_difficulty, m_currentFloor, m_roomPrefab);
        }

        public void GenerateLevelMultipleTimesButton()
        {
            float time = Time.realtimeSinceStartup;
            int generationTimes = 100;
            for (int i = 0; i < generationTimes; i++)
            {
                GenerateLevelButton();
            }
            Debug.Log($"Generated level {generationTimes} times and it took {Time.realtimeSinceStartup - time} seconds.");
        }
    }
}
