using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object that defines the base data for an enemy type, including stats,
/// experience rewards, and a list of attack patterns.
/// </summary>

[CreateAssetMenu(fileName = "New Enemy Type", menuName = "Enemy/New Type")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Basic info")]
    [Tooltip("Display name of the enemy")]
    public string enemyName;

    [Tooltip("Max health of the enemy")]
    public int maxHealth;

    [Tooltip("Current health (use during initialization and testing)")]
    public int currentHealth;

    [Tooltip("Experience points granted when defeated")]
    public int exp;

    [Header("Combat behavior")]
    [Tooltip("List of possible attack patterns this enemy can use")]
    public List<EnemyAttackSO> attacks;
}
