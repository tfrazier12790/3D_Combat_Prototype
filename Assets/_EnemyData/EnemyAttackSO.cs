using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object that defines an enemy attack pattern composed of one or more phases.
/// Supports different targeting types (fixed or player position based) and handles telegraphing
/// hits over time.
/// </summary>

[CreateAssetMenu(fileName = "New Enemy Attack Type", menuName = "Enemy/New Attack")]
public class EnemyAttackSO : ScriptableObject
{
    [Tooltip("Relative location the attack is based around")]
    public Vector3 attackLocation;

    [Tooltip("List of attack phases with their own hit offsets")]
    public List<AttackPhase> attackPhases;

    [Tooltip("Delay between each attack phase")]
    public float timeBetweenAttacks;

    [Tooltip("Flag for determining how the target position is selected")]
    public EnemyTargetingType targetingType;

    // Defines the phase of an attack using a list of hit area offsets
    [System.Serializable]
    public class AttackPhase
    {
        public List<Vector3> offsetIndices;
    }

    // Determines how the enemt selects the target location for the attack
    public enum EnemyTargetingType { FixedPosition, TargetPlayer }

    /// <summary>
    /// Executes attack sequence by spawning hit telegraphs at positions based on the selected targeting type and attack phases
    /// </summary>
    /// <param name="target">The player object to potentially target</param>
    /// <param name="enemy">The enemy executing the attack</param>
    /// <param name="hitTelegraph">The prefab used to visually telegraph the incoming hit</param>
    /// <param name="gridLocations">The valid grid locations that can be targeted</param>
    /// <returns></returns>
    public IEnumerator EnemyAttack(GameObject target, GameObject enemy, GameObject hitTelegraph, List<Vector3> gridLocations)
    {
        Vector3 targetPosition = new Vector3();
        List<GameObject> spawnedTelegraphs = new List<GameObject>();

        // Determine the target position based on targeting type
        switch (targetingType)
        {
            case EnemyTargetingType.FixedPosition:
                targetPosition = enemy.transform.position;
                break;

            case EnemyTargetingType.TargetPlayer:
                targetPosition = target.GetComponent<PlayerCharCombatScript>().PlayerPositionForTargeting();
                break;
            default:
                break;
        }
                
        // Loop through all attack phases and spawn telegraphs for each hit
        foreach (AttackPhase attack in attackPhases)
        {
            foreach (Vector3 location in attack.offsetIndices)
            {
                Vector3 hitLocation = targetPosition - location;

                // Only spawn if the location is valid on the grid 
                if (gridLocations.Contains(hitLocation + new Vector3(0, 0.5f, 0)))
                {
                    GameObject newHitTelegraph = Instantiate(hitTelegraph, hitLocation, Quaternion.identity);
                    newHitTelegraph.GetComponent<HitTelegraphScript>().damage = enemy.GetComponent<EnemyBaseScript>().damage;
                    spawnedTelegraphs.Add(newHitTelegraph);
                }
            }
            // Wait before continuing on to the next phase
            yield return new WaitForSeconds(timeBetweenAttacks);
        }

        // Wait until all telegraphs are gone meaning the attack is resolved
        bool allTelegraphsGone = false;
        while(!allTelegraphsGone)
        {
            allTelegraphsGone = true;

            foreach (GameObject telegraph in spawnedTelegraphs) 
            { 
                if (telegraph != null)
                {
                    allTelegraphsGone = false; 
                    break;
                }
            }
            yield return null;
        }
    }
}
