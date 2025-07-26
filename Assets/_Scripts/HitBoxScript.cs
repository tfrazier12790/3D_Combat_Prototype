using UnityEngine;

/// <summary>
/// Handles detection for the enemy's hit telegraph area.
/// Informs the parent HitTelegraphScript when the player enters or exits the hit zone.
/// </summary>

public class HitBoxScript : MonoBehaviour
{
    private HitTelegraphScript hitTelegraphScript;
    private void Awake()
    {
        // Cache reference to parent script
        hitTelegraphScript = GetComponentInParent<HitTelegraphScript>();
    }

    // Set flag to true when the player enters hit box
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            hitTelegraphScript.PlayerInTrigger(true);
        }
    }

    // Set flag to false when player leaves hit box
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            hitTelegraphScript.PlayerInTrigger(false);
        }
    }
}
