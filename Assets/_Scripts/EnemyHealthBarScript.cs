using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the behavior of the enemy health bar UI including
/// updating the slider values and rotating it to face the camera
/// </summary>

public class EnemyHealthBarScript : MonoBehaviour
{
    [Header("Health bar prefab")]
    [SerializeField] Slider healthSlider;

    void Awake()
    {
        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
        }
    }

    private void LateUpdate()
    {
        // Turn object to face camera
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        transform.forward = camForward;
    }

    // Updates health bar values to show relative health level
    public void UpdateHealthBarSlider(int currentHealth, int maxHealth)
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
    }
}
