using System;
using UnityEngine;

/// <summary>
/// Handles telegraphed attack animation and applies damage if the player is within the area
/// </summary>

public class HitTelegraphScript : MonoBehaviour
{
    float t = 0;                        // Progress timer for duration
    float duration = 1.0f;              // Duration telegraph is visible until damage is dealt
    public int damage;                  // Amount of damage attack deals
    bool playerInTrigger = false;       // Flag to tell if player is inside the hit

    void Update()
    {
        if (t < 1.0f)
        {
            // Increase the time based on duration
            t += Time.deltaTime / duration;
            // Smoothly scale the telegraph object vertically
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1, 5, 1), t);
        } else
        {
            // Destroy object when duration is up
            Destroy(gameObject);
        }
    }

    // Called by the HitBoxScript when the player enters or exits the hit box
    // True if player is in hit box; false is player is not in hit box
    public void PlayerInTrigger(bool playerInTriggerSet)
    {
        playerInTrigger = playerInTriggerSet;
        // Possibly change later to cache player object here if necessary instead of using Find...() later
    }

    // Applies automatically when object is destroyed
    // Applies damage to player if still inside
    private void OnDestroy()
    {
        if (playerInTrigger)
        { 
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharCombatScript>().Damage(damage);
            playerInTrigger = false;
        }
    }
}
