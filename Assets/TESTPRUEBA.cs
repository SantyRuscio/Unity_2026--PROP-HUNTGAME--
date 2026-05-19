using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TESTPRUEBA : MonoBehaviour
{
    public List<Enemy> enemies;
    public List<Enemy> activeEnemies;
    public List<Enemy> reserveEnemies;
    public List<Squad> squads;
    public List<Patrol> patrols;
    public string[] missionNames;

    void Start()
    {
        ejecutarConsultas();
    }

    // 1 --- SANTIAGO RUSCIO //
    public IEnumerator GenerateItems<T>(int count, int batchSize, Func<T> factory, Action<List<T>> callback)
    {
        var list = new List<T>();
        for (int i = 0; i < count; i++)
        {
            list.Add(factory());
            if ((i + 1) % batchSize == 0)
                yield return null;
        }
        callback?.Invoke(list);
    }
    public void ejecutarConsultas()
    {
        // 2 --- SANTIAGO RUSCIO //
        var result2 = enemies
            .Where(e => e.Faction == "Undead" && e.HP > 0)
            .OrderBy(e => e.HP / e.MaxHP)
            .Take(5)
            .Select(e => e.Name);

        // 3 --- SANTIAGO RUSCIO //
        var result3 = activeEnemies
            .Concat(reserveEnemies)
            .OfType<Boss>()
            .Select(b => (Boss: b, NameUpper: b.Name.ToUpper()));

        // 4 --- SANTIAGO RUSCIO //
        var result4 = squads
            .SelectMany(s => s.Members)
            .Where(u => u.Level >= 10)
            .OrderByDescending(u => u.Level)
            .ThenBy(u => u.Name);

        // 5 --- SANTIAGO RUSCIO //
        var result5 = patrols
            .Zip(missionNames, (patrol, mission) => (Patrol: patrol, MissionName: mission))
            .SelectMany(x => x.Patrol.Members.Select(u => (x.MissionName, UnitName: u.Name, u.Power)));
    }

    // 6 --- SANTIAGO RUSCIO //
   public (bool hit, int damageDone) CalculateHit(Character target, int damage)
   {

        if (target == null || !target.IsAlive)
        {
            return (false, 0);
        }

        return (true, damage);
   }
}

public class Character
{
    public float HP;
    public float MaxHP;
    public bool IsAlive;
}

public class Enemy : Character
{
    public string Name;
    public string Faction;
}

public class Boss : Enemy { }

public class Unit
{
    public string Name;
    public int Level;
    public int Power;
}

public class Squad
{
    public List<Unit> Members;
}

public class Patrol
{
    public List<Unit> Members;
}