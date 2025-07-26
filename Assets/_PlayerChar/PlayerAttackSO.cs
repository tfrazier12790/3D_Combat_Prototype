using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Types of player targeting pattern
// Defines how the attack selects its target on the grid
public enum PlayerTargetingType
{
    MeleeAttack,        // Affects a tile adjacent to player
    RangedAttack,       // Affects tile in a line or range from player
    TargetedAttack      // Player selects a specific location
}


/// <summary>
/// ScriptableObject that defines a player attack pattern, including
/// its targeting type and relative tile offsets for attack patterns
/// </summary>

[CreateAssetMenu(fileName = "New Player Attack Type", menuName = "Player/New Attack")]
public class PlayerAttackSO : ScriptableObject
{
    [Tooltip("List of tile offsets where this attack will apply damage/effects")]
    [SerializeField] List<Vector3> attackLocations;

    [Tooltip("Determines how the player selects targets for this attack")]
    [SerializeField] PlayerTargetingType targetingType;

    // Placeholder coroutine for executing attack
    public IEnumerator ExecuteAttack()
    {
        yield return null;
    }

    // Returns the targeting type of the attack
    public PlayerTargetingType GetTargetingType() { return targetingType; }

    // Retuerns the list of relative positions this attack affects
    public List<Vector3> GetAttackLocations() { return attackLocations; }
}
