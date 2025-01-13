using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static LevelGenerator.Enums;

namespace LevelGenerator
{
    public class LevelManager : MonoBehaviour
    {
        private int m_currentFloor;
        private Difficulty m_difficulty;

        public void GenerateLevelButton()
        {
            if (m_currentFloor == 0)
                m_currentFloor = 1;

            Debug.Log("GenerateLevelButton");
            LevelGenerator.GenerateLevel(m_difficulty, m_currentFloor);
        }

        public void GenerateNextRoom()
        {
            //LevelGenerator.RandomWalkFromEntranceToBoss();
        }
    }
}
