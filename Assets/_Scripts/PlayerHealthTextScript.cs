using TMPro;
using UnityEngine;

/// <summary>
/// Displays player's current health and remaining actions as on-screen UI text.
/// </summary>

public class PlayerHealthTextScript : MonoBehaviour
{
    // Reference to TMP Text element in UI
    [SerializeField] TMP_Text text;

    // Reference to player combat script
    PlayerCharCombatScript playerChar;

    private void Start()
    {
        // Cache player combat script for health and action variables
        playerChar = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharCombatScript>();
    }

    private void Update()
    {
        // Update UI text with player's current health and actions  
        text.text = string.Format("HP: {0}/{1} Actions: {2}", playerChar.GetCurrentHealth(), playerChar.GetMaxHealth(), playerChar.GetRemainingActions());
    }
}
