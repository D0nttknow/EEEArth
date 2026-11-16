using Solution;
using System;
using UnityEngine;

public static class UtilitySortEnemies 
{
    public static OOPEnemy[] SortEnemiesByRemainningEnergy1(OOPMapGenerator mapGenerator)
    {
        var enemies = mapGenerator.GetEnemies();

        for (int i = 0; i < enemies.Length - 1; i++)
        {
            int minIndex = i;
            for (int j = i + 1; j < enemies.Length; j++)
            {
                if (enemies[j].energy < enemies[minIndex].energy)
                {
                    minIndex = j;
                }
            }
            var temp = enemies[i];
            enemies[i] = enemies[minIndex];
            enemies[minIndex] = temp;
        }

        return enemies;
    }

    public static OOPEnemy[] SortEnemiesByRemainningEnergy2(OOPMapGenerator mapGenerator)
    {
        var enemies = mapGenerator.GetEnemies();
        Array.Sort(enemies, (a, b) => a.energy.CompareTo(b.energy));
        return enemies;
    }
}
