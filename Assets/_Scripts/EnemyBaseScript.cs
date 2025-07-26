using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the basic behavior and data for an enemy in the battle system.
/// Manages health, attack selection and execution, and health bar UI
/// </summary>

public class EnemyBaseScript : MonoBehaviour
{
    [Header("Enemy data")]
    [SerializeField] EnemyDataSO enemyData;           
    
    // List of available attacks
    List<EnemyAttackSO> attackData;  
    
    [Header("Hit telegraph object prefab")]
    [SerializeField] GameObject hitTelegraphObject; 
    
    // Reference to player object
    GameObject player;   
    
    // Health parameters
    int maxHealth;                                         
    int currentHealth;

    // Reference to battle grid
    List<Vector3> gridLocations = new List<Vector3>();

    // Flag for current enemy state
    bool isAttacking = false;

    [Header("Health bar prefab")]
    [SerializeField] GameObject healthBarPrefab;

    // References to healthbar object
    GameObject healthBar;
    EnemyHealthBarScript healthBarSlider;

    // Amount of damage this enemy deals
    public int damage;

    void Start()
    {
        // Find references at runtime
        player = GameObject.FindGameObjectWithTag("Player");

        // Initialize attack list from enemy data
        attackData = new List<EnemyAttackSO>(enemyData.attacks);

        // Initialize health values
        maxHealth = enemyData.maxHealth;
        currentHealth = maxHealth;

        // Instantiate health bar and position above enemy
        healthBar = Instantiate(healthBarPrefab, transform);
        healthBar.transform.localPosition = new Vector3(0, 1.5f, 0);

        // Get reference to slider and set its initial value
        healthBarSlider = healthBar.GetComponent<EnemyHealthBarScript>();
        healthBarSlider.UpdateHealthBarSlider(currentHealth, maxHealth);

        // Get grid locations from Battle Controller
        gridLocations = BattleControllerScript.Instance.GetGridLocations();
    }

    // Performs a random attack from available attacks.
    // Plays telegraph and waits for attack animation to finish
    public IEnumerator PerformAttack()
    {
        if (isAttacking) yield break;

        isAttacking = true;

        // Choose a random attack from the list
        // Can be updated later to pick attack using logic
        EnemyAttackSO enemyAttack = attackData[Random.Range(0, attackData.Count)];

        // Execute attack coroutine
        yield return StartCoroutine(enemyAttack.EnemyAttack(player, this.gameObject, hitTelegraphObject, gridLocations));

        isAttacking = false;
    }

    // Returns whether the enemy is still attacking
    public bool IsAttacking() => isAttacking;
    // Reduces current health by given damage
    public void DealDamage(int damage)
    {
        currentHealth -= damage;
        healthBarSlider.UpdateHealthBarSlider(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        BattleControllerScript.Instance.RemoveEnemy(this.gameObject);
        Destroy(this.gameObject);
    }
}
