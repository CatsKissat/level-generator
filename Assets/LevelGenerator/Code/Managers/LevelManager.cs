using System.Reflection;
using UnityEngine;
using static Cats.LevelGenerator.Enums;

namespace Cats.LevelGenerator
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private Rooms m_roomPrefabData;
        private int m_currentFloor;
        private Difficulty m_difficulty;

        public void GenerateLevelButton(bool _isClearingConsole = true)
        {
            if (m_currentFloor == 0)
                m_currentFloor = 1;

            if (_isClearingConsole)
                ClearConsole();

            LevelGenerator.GenerateLevel(m_difficulty, m_currentFloor, m_roomPrefabData);
        }

        public void GenerateLevelMultipleTimesButton()
        {
            float time = Time.realtimeSinceStartup;
            int generationTimes = 100;
            for (int i = 0; i < generationTimes; i++)
            {
                GenerateLevelButton(false);
            }
            Debug.Log($"Generated level {generationTimes} times and it took {Time.realtimeSinceStartup - time} seconds.");
        }

        private static void ClearConsole()
        {
            var assemly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assemly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }
}
