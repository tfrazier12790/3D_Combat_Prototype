using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

/// <summary>
/// Manages the core combat loop including turn management, enemy spawning,
/// movement grid setup, and enemy interaction.
/// </summary>

// Defines the possible turn states of the flow of battle
public enum Turn
{
    PlayerTurn,
    EnemyTurn,
    WaitTurn
}

public class BattleControllerScript : MonoBehaviour
{
    public static BattleControllerScript Instance;

    // Grid related data
    [SerializeField] List<Vector3> gridLocations;           // Valid grid locations for movement
    [SerializeField] List<Vector3> wallLocations;           // Wall positions that block movement
    [SerializeField] List<Vector3> enemyLocations;          // Valid spawn locations for enemies

    // Enemy Data
    [SerializeField] List<GameObject> enemies;              // List of enemy prefabs
    [SerializeField] List<GameObject> spawnedEnemyList;     // List of active enemies in current battle

    // Player reference
    [SerializeField] GameObject player;

    // Turn and battle state
    [SerializeField] public Turn currentTurn;
    [SerializeField] bool continueBattle = true;

    // Utility function to shuffle any list
    public static void Shuffle<T>(List<T> list)
    {

        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Classic singleton script for creating an instance
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Prepares the battle by spawning enemies and starting the main battle loop
    public IEnumerator BattleSetup()
    {
        currentTurn = Turn.PlayerTurn;

        // Try spawning up to three enemies in valid positions
        for (int i = 0; i < 3; i++)
        {
            // Creates a 50% chance to spawn an enemy at the valid location
            if (Random.Range(0, 2) % 2 == 0)
            {
                spawnedEnemyList.Add(Instantiate(enemies[0], enemyLocations[i], Quaternion.identity));
            }
        }

        // Ensure at least one enemy spawns
        if (spawnedEnemyList.Count == 0)
        {
            Debug.Log("No Enemies Spawned; Spawning Enemy");
            spawnedEnemyList.Add(Instantiate(enemies[0], enemyLocations[0], Quaternion.identity));
        }

        yield return StartCoroutine(BattleLoop());
    }

    // Sets up walkable tiles, walls positions, and enemy spawn points
    public void SetGridLocations(List<Vector3> gridLocationsList, List<Vector3> potentialEnemyLocations, List<Vector3> wallPositionsList)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Flatten Y value to match the floor layer
        foreach (Vector3 location in gridLocationsList)
        {
            gridLocations.Add(new Vector3(location.x, /*player.transform.position.y*/ -10, location.z));        
        }

        foreach (Vector3 position in wallPositionsList)
        {
            wallLocations.Add(new Vector3(position.x, -10, position.z));
        }

        // Only accept enemy locations that can be reached from the player's location
        // Dont to prevent unreachable enemies behind walls
        foreach (Vector3 location in potentialEnemyLocations)
        {
            if (isReachable(new Vector3(player.transform.position.x, -10, player.transform.position.z), location, gridLocations, wallLocations))
            {
                enemyLocations.Add(location);
            }
        }
    }

    // Returns a copy of valid grid locations
    public List<Vector3> GetGridLocations() => gridLocations; 
    // Retrns the list of spawned enemy locations
    public List<Vector3> GetEnemyLocations() => enemyLocations;

    // Handles the player's turn. Ends turn when space key is pressed
    public IEnumerator PlayerTurn()
    {
        // Reset player actions count
        player.GetComponent<PlayerCharCombatScript>().ResetActions();

        // Yield until player pressed space, ending turn
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }
        currentTurn = Turn.EnemyTurn;
    }

    // Handles enemy turn, shuffling and executing each enemy's attack routine
    // CAN BE CHANGED LATER TO DETERMINE ATTACK ORDER BASED ON ENEMY STATS, ETC.
    public IEnumerator EnemyTurn()
    {
        // Randomize enemy list, and iterate through list performing attacks with 1 second apart
        Shuffle(spawnedEnemyList);
        foreach (GameObject enemy in spawnedEnemyList)
        {
            StartCoroutine(enemy.GetComponent<EnemyBaseScript>().PerformAttack());
            yield return new WaitForSeconds(1);
        }

        // Wait until all enemies finish their attacks
        bool isAttackingCheck;
        do
        {
            isAttackingCheck = false;
            foreach (GameObject enemy in spawnedEnemyList)
            {
                if (enemy.GetComponent<EnemyBaseScript>().IsAttacking()) 
                {
                    isAttackingCheck = true;
                    break;
                }
            }
            yield return null;
        } while (isAttackingCheck);

        // When attacks are finished, begin player turn
        currentTurn = Turn.PlayerTurn;
    }

    // Ends the current player's turn
    internal void EndTurn()
    {
        currentTurn = Turn.EnemyTurn;
        Debug.Log("End Turn");
    }

    // Removes an enemy from the active list when it is defeated
    public void RemoveEnemy(GameObject enemy)
    {
        if (spawnedEnemyList.Contains(enemy))
        {
            spawnedEnemyList.Remove(enemy);
        }

        if (spawnedEnemyList.Count == 0)
        {
            EndBattle();
            StartCoroutine(GameController.Instance.EndBattle());
        }
    }

    // Core turn loop that alternates between player and enemy turns
    public IEnumerator BattleLoop()
    {
        continueBattle = true;
        while (continueBattle)
        {
            if (currentTurn == Turn.PlayerTurn)
            {
                yield return StartCoroutine(PlayerTurn());
                currentTurn = Turn.EnemyTurn;
            }

            if (currentTurn == Turn.EnemyTurn)
            {
                yield return StartCoroutine(EnemyTurn());
            }
        }

        yield return null;
    }
    
    /// <summary>
    /// Breadth First Search algorithm to determine if the player can reach a given tile,
    /// indicating where an enemy can spawn and still be reached.
    /// </summary>
    /// <param name="startPos">Starting position of the player</param>
    /// <param name="targetPos">Target position to check, typically each position around the edge of the arena</param>
    /// <param name="floorPositions">All floor positions that can potentially be walked on</param>
    /// <param name="wallPositions">All wall positions that can block movement</param>
    /// <returns>If target position can be reached from player position</returns>
    bool isReachable (Vector3 startPos, Vector3 targetPos, List<Vector3> floorPositions, List<Vector3> wallPositions)
    {
        // Sets up a queue of positions to be checks and positions already visited
        Queue<Vector3> queue = new Queue<Vector3>();
        List<Vector3> visited = new List<Vector3>();

        // Initializes the starting position
        queue.Enqueue(startPos);
        visited.Add(startPos);

        // Determines each direction to check
        Vector3[] directions = new Vector3[] 
        {
            new Vector3 (1, 0, 0),
            new Vector3 (-1, 0, 0),
            new Vector3 (0, 0, 1),
            new Vector3 (0, 0, -1)
        };

        // Loops through the queue until it is complete
        while (queue.Count > 0)
        {
            // If the next position in the queue is the target position, the position can be reached
            Vector3 current = queue.Dequeue();
            if (current == targetPos)
            {
                return true;
            }

            foreach (Vector3 direction in directions)
            {
                // Determines the neightbors of the current position
                Vector3 neighbor = new Vector3(Mathf.Round(current.x), Mathf.Round(current.y), Mathf.Round(current.z)) + direction;

                // IF the neighbor is the target position, the position can be reached
                if (neighbor == targetPos)
                {
                    return true;
                }

                // Add neighbor to queue if it is a valid, walkable tile
                if (floorPositions.Contains(neighbor) && !wallPositions.Contains(neighbor) && !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }

        // If the queue is empty and the target position is never reached, the position is ureachable
        return false;
    }

    // Ends the battle and clears the grid
    public void EndBattle()
    {
        continueBattle = false;
        gridLocations.Clear();
        enemyLocations.Clear();
    }
}
