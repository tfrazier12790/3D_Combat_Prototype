using UnityEngine;

/// <summary>
/// Handles visually determining which enemy the player is currently targeting and 
/// applying damage to that enemy when the hit marker is destroyed.
/// </summary>

public class PlayerHitMarkerScript : MonoBehaviour
{
    // Reference to player and the attached combat script
    [SerializeField] GameObject player;
    [SerializeField] PlayerCharCombatScript playerCharScript;

    // Amount of damage this attack will apply
    [SerializeField] int damage;

    // Determines whether damage will be applied
    [SerializeField] bool dealDamage = false;

    // Reference to enemy currently inside trigger
    [SerializeField] GameObject enemy;

    void Start()
    {
        // Cache references to player and player combat script
        player = GameObject.FindGameObjectWithTag("Player");
        playerCharScript = player.GetComponent<PlayerCharCombatScript>();
    }

    // Remotely set how much damage will be dealt
    public void SetDamage(int damageToDeal)
    {
        damage = damageToDeal;
        dealDamage = true;
    }

    // Sets enemy reference if an enemy enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            enemy = other.gameObject;
        }
    }

    // Clears enemy reference if enemy leaves the trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            enemy = null;
        }
    }

    // If this hit marker is destroyed while and enemy is in the trigger, and the deal damage flag is set:
    // Applies damage to the enemy
    private void OnDestroy()
    {
        if (enemy != null && dealDamage)
        {
            enemy.GetComponent<EnemyBaseScript>().DealDamage(playerCharScript.GetDamage());
        }
    }
}
