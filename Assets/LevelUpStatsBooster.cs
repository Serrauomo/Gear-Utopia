using System;
using System.Collections.Generic;
using UnityEngine;
using TopDownCharacter2D.Stats;

[Serializable]
public class StatBoost
{
    [Header("Stat Boost Configuration")]
    public string boostName = "Level Up Boost";
    
    [Header("Character Stats")]
    public int maxHealthBoost = 0;
    public float speedBoost = 0f;
    
    [Header("Attack Stats")]
    public float attackDelayReduction = 0.02f;
    public float attackPowerBoost = 0.1f;
    public float attackSizeBoost = 0f;
    public float attackSpeedBoost = 1f;
    
    [Header("Ranged Attack Stats")]
    public float spreadChange = 0f;
    public float durationBoost = 0f;
    public int projectilesBoost = 0;
    public float projectileAngleChange = 0f;
    
    [Header("Boost Type")]
    public StatsChangeType changeType = StatsChangeType.Add;
}

[Serializable]
public class BreakthroughConfig
{
    [Header("Breakthrough Configuration")]
    public string breakthroughName = "Breakthrough";
    public int levelInterval = 5;
    public bool enabled = true;
    
    [Header("Breakthrough Stats")]
    public StatBoost statBoost = new StatBoost()
    {
        boostName = "Breakthrough Boost",
        attackDelayReduction = 0.05f,
        attackPowerBoost = 0.5f,
        maxHealthBoost = 5,
        changeType = StatsChangeType.Add
    };
}

public class LevelUpStatsBooster : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterStatsHandler statsHandler;
    [SerializeField] private ExpManager expManager;
    
    [Header("Level Up Boosts")]
    [SerializeField] private StatBoost normalLevelBoost = new StatBoost();
    
    [Header("Breakthrough System")]
    [SerializeField] private List<BreakthroughConfig> breakthroughs = new List<BreakthroughConfig>()
    {
        new BreakthroughConfig()
        {
            breakthroughName = "Rapid Fire Mastery",
            levelInterval = 5,
            enabled = true,
            statBoost = new StatBoost()
            {
                boostName = "Rapid Fire Boost",
                attackDelayReduction = 0.05f,
                attackPowerBoost = 0.3f,
                changeType = StatsChangeType.Add
            }
        },
        new BreakthroughConfig()
        {
            breakthroughName = "Marksman Elite",
            levelInterval = 10,
            enabled = true,
            statBoost = new StatBoost()
            {
                boostName = "Marksman Boost",
                attackPowerBoost = 1f,
                attackSizeBoost = 0.2f,
                projectilesBoost = 1,
                maxHealthBoost = 10,
                changeType = StatsChangeType.Add
            }
        },
        new BreakthroughConfig()
        {
            breakthroughName = "Combat Veteran",
            levelInterval = 25,
            enabled = true,
            statBoost = new StatBoost()
            {
                boostName = "Veteran Boost",
                attackDelayReduction = 0.1f,
                attackPowerBoost = 2f,
                speedBoost = 3f,
                maxHealthBoost = 25,
                projectilesBoost = 2,
                changeType = StatsChangeType.Add
            }
        }
    };
    
    // Stato interno
    private int lastProcessedLevel = 1;
    private CharacterStats currentLevelBoostStats;
    private Dictionary<string, CharacterStats> activeBreakthroughStats = new Dictionary<string, CharacterStats>();
    
    private void Start()
    {
        InitializeComponents();
        SetupInitialLevel();
    }
    
    // Ensure references are set, disable if missing
    private void InitializeComponents()
    {
        if (statsHandler == null)
            statsHandler = GetComponent<CharacterStatsHandler>();
        if (expManager == null)
            expManager = FindObjectOfType<ExpManager>();
        if (statsHandler == null || expManager == null)
            enabled = false;
    }
    
    private void Update()
    {
        CheckForLevelUp();
    }
    
    // Apply all boosts if starting at a level > 1
    private void SetupInitialLevel()
    {
        lastProcessedLevel = expManager.level;
        if (expManager.level > 1)
            ApplyAllBoostsForLevel(expManager.level);
    }
    
    // Check if the player has leveled up since last frame
    private void CheckForLevelUp()
    {
        if (expManager.level > lastProcessedLevel)
        {
            int levelsGained = expManager.level - lastProcessedLevel;
            for (int i = 1; i <= levelsGained; i++)
            {
                int levelToProcess = lastProcessedLevel + i;
                ProcessLevelUp(levelToProcess);
            }
            lastProcessedLevel = expManager.level;
        }
    }
    
    // Apply normal and breakthrough boosts for the given level
    private void ProcessLevelUp(int newLevel)
    {
        List<BreakthroughConfig> triggeredBreakthroughs = new List<BreakthroughConfig>();
        foreach (var breakthrough in breakthroughs)
        {
            // Check if this breakthrough should trigger at this level
            if (breakthrough.enabled && newLevel % breakthrough.levelInterval == 0)
                triggeredBreakthroughs.Add(breakthrough);
        }
        ApplyLevelBoost();
        foreach (var breakthrough in triggeredBreakthroughs)
            ApplyBreakthroughBoost(breakthrough);
    }
    
    private void ApplyLevelBoost()
    {
        if (currentLevelBoostStats != null)
            statsHandler.statsModifiers.Remove(currentLevelBoostStats);
        int totalLevels = expManager.level - 1;
        currentLevelBoostStats = CreateBoostStats(normalLevelBoost, totalLevels);
        statsHandler.statsModifiers.Add(currentLevelBoostStats);
    }
    
    private void ApplyBreakthroughBoost(BreakthroughConfig breakthrough)
    {
        string breakthroughKey = breakthrough.breakthroughName;
        if (activeBreakthroughStats.ContainsKey(breakthroughKey))
        {
            statsHandler.statsModifiers.Remove(activeBreakthroughStats[breakthroughKey]);
            activeBreakthroughStats.Remove(breakthroughKey);
        }
        int breakthroughCount = expManager.level / breakthrough.levelInterval;
        var breakthroughStats = CreateBoostStats(breakthrough.statBoost, breakthroughCount);
        statsHandler.statsModifiers.Add(breakthroughStats);
        activeBreakthroughStats[breakthroughKey] = breakthroughStats;
    }
    
    private void ApplyAllBoostsForLevel(int level)
    {
        int totalLevels = level - 1;
        if (totalLevels > 0)
        {
            currentLevelBoostStats = CreateBoostStats(normalLevelBoost, totalLevels);
            statsHandler.statsModifiers.Add(currentLevelBoostStats);
        }
        foreach (var breakthrough in breakthroughs)
        {
            if (breakthrough.enabled)
            {
                int breakthroughCount = level / breakthrough.levelInterval;
                if (breakthroughCount > 0)
                {
                    var breakthroughStats = CreateBoostStats(breakthrough.statBoost, breakthroughCount);
                    statsHandler.statsModifiers.Add(breakthroughStats);
                    activeBreakthroughStats[breakthrough.breakthroughName] = breakthroughStats;
                }
            }
        }
    }
    
    private CharacterStats CreateBoostStats(StatBoost boost, int multiplier)
    {
        var stats = new CharacterStats();
        stats.maxHealth = boost.maxHealthBoost * multiplier;
        stats.speed = boost.speedBoost * multiplier;
        stats.statsChangeType = boost.changeType;
        if (HasAttackBoosts(boost) && statsHandler.CurrentStats?.attackConfig != null)
        {
            var baseAttackConfig = statsHandler.CurrentStats.attackConfig;
            if (baseAttackConfig is TopDownCharacter2D.Attacks.Range.RangedAttackConfig)
            {
                var rangedConfig = ScriptableObject.CreateInstance<TopDownCharacter2D.Attacks.Range.RangedAttackConfig>();
                ApplyAttackBoosts(rangedConfig, boost, multiplier);
                ApplyRangedBoosts(rangedConfig, boost, multiplier);
                stats.attackConfig = rangedConfig;
            }
        }
        return stats;
    }
    
    private bool HasAttackBoosts(StatBoost boost)
    {
        return boost.attackDelayReduction != 0 || boost.attackPowerBoost != 0 || 
               boost.attackSizeBoost != 0 || boost.attackSpeedBoost != 0 ||
               boost.spreadChange != 0 || boost.durationBoost != 0 || 
               boost.projectilesBoost != 0 || boost.projectileAngleChange != 0;
    }
    
    private void ApplyAttackBoosts(TopDownCharacter2D.Attacks.AttackConfig config, StatBoost boost, int multiplier)
    {
        config.delay = -boost.attackDelayReduction * multiplier;
        config.power = boost.attackPowerBoost * multiplier;
        config.size = boost.attackSizeBoost * multiplier;
        config.speed = boost.attackSpeedBoost * multiplier;
    }
    
    private void ApplyRangedBoosts(TopDownCharacter2D.Attacks.Range.RangedAttackConfig config, StatBoost boost, int multiplier)
    {
        config.spread = boost.spreadChange * multiplier;
        config.duration = boost.durationBoost * multiplier;
        config.numberOfProjectilesPerShot = boost.projectilesBoost * multiplier;
        config.multipleProjectilesAngle = boost.projectileAngleChange * multiplier;
    }
    
    public void ForceRecalculateBoosts()
    {
        if (currentLevelBoostStats != null)
        {
            statsHandler.statsModifiers.Remove(currentLevelBoostStats);
            currentLevelBoostStats = null;
        }
        foreach (var breakthroughStat in activeBreakthroughStats.Values)
            statsHandler.statsModifiers.Remove(breakthroughStat);
        activeBreakthroughStats.Clear();
        ApplyAllBoostsForLevel(expManager.level);
    }
    
    public StatBoost GetNormalBoost() => normalLevelBoost;
    public List<BreakthroughConfig> GetBreakthroughs() => breakthroughs;
    public int GetCurrentLevel() => expManager.level;
    public Dictionary<string, int> GetBreakthroughCounts()
    {
        var counts = new Dictionary<string, int>();
        foreach (var breakthrough in breakthroughs)
            counts[breakthrough.breakthroughName] = expManager.level / breakthrough.levelInterval;
        return counts;
    }
    
    private void OnValidate()
    {
        foreach (var breakthrough in breakthroughs)
        {
            if (breakthrough.levelInterval <= 0) 
                breakthrough.levelInterval = 1;
            breakthrough.statBoost.attackDelayReduction = Mathf.Max(0, breakthrough.statBoost.attackDelayReduction);
        }
        normalLevelBoost.attackDelayReduction = Mathf.Max(0, normalLevelBoost.attackDelayReduction);
    }
}