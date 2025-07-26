using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the main camera behavior in both the overworld and in battle.
/// Smoothly follows player in overworld mode, and handles cinematic transitions 
/// into and out of battle with animated position and rotation changes.
/// </summary>

public class MainOverworldCameraScript : MonoBehaviour
{
    // Reference to player object
    GameObject player;

    // Camera movement settings for overworld mode
    float speed = 10f;
    float yOffset = 4.5f;
    float zOffset = 4.35f;
    Vector3 overworldRotation = new Vector3(45, 0, 0);

    // Duration for transitioning into and out of battle
    float battleStartLength = 0.5f;

    // Camera position and rotation for battle mode
    Vector3 battlePosition;
    Vector3 battleRotation;
    
    void Start()
    {
        // Cache reference to player object
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (GameController.Instance.currentState == GameState.OVERWORLD)
        {
            // Follow player in overworld mode
            Vector3 targetPosition = new Vector3(player.transform.position.x, player.transform.position.y + yOffset, player.transform.position.z - zOffset);

            // Maintain fixed overworld rotation and interpolate toward player position
            transform.eulerAngles = overworldRotation;
            transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
        }
        else if (GameController.Instance.currentState == GameState.BATTLE)
        {
            // Lock position/rotation in battle mode
            transform.position = battlePosition;
            transform.eulerAngles = battleRotation;
        }
    }

    // Animates the camera moving into battle position while facing the player
    public IEnumerator BattleStartAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(player.transform.position.x + 5, player.transform.position.y - 4, player.transform.position.z - 5);

        float elapsedTime = 0f;
        while (elapsedTime < battleStartLength)
        {
            float steps = Mathf.SmoothStep(0f, 1f, elapsedTime / battleStartLength);

            transform.position = Vector3.Lerp(startPos, endPos, steps);
            transform.LookAt(player.transform);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Final camera position and rotation stored and set
        transform.position = endPos;
        transform.LookAt(player.transform);
        battlePosition = transform.position;
        battleRotation = transform.eulerAngles;
    }

    // Coroutine that animates the camera returning to overworld mode after battle.
    public IEnumerator BattleEndAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(player.transform.position.x, player.transform.position.y + yOffset + 10, player.transform.position.z - zOffset);

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(overworldRotation);

        float elapsedTime = 0f;
        while (elapsedTime < battleStartLength)
        {
            float steps = Mathf.SmoothStep(0f, 1f, elapsedTime / battleStartLength);

            // Smoothly move camera back to overworld position and rotation
            transform.position = Vector3.Lerp(startPos, targetPos, steps);
            transform.rotation = Quaternion.Lerp(startRot, endRot, steps);
            transform.LookAt(player.transform);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Snap to final overworld position and rotation
        transform.position = targetPos;
        transform.rotation = endRot;
    }
}
