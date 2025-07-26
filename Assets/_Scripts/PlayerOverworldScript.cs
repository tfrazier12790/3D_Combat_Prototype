using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

// Enum representing the four cardinal directions the player can face
public enum PlayerDirection { UP, DOWN, LEFT, RIGHT }

/// <summary>
/// Controls player movement and battle initiation in the overworld state.
/// Movement is grid-based similar to older RPG games.
/// </summary>

public class PlayerOverworldScript : MonoBehaviour
{
    float speed = 5f;                               // Movement speed of the player
    bool isMoving = false;                          // Flag to determine whether the player is currently moving
    PlayerDirection direction;                      // Direction the player is facing
    Vector3 destination;                            // Target position for movement
    float failsafeTimer = 0;                        // Timer to make sure player doesn't get stuck
    float failsafeTimeOut = 3f;                     // How long failsafe waits to snap player to destination

    private const float Y_SNAP_HEIGHT = -0.125f;    // Y-axis snap value to keep player grounded

    void Update()
    {
        // Only allow movement if game is in overworld state
        if (GameController.Instance.currentState != GameState.OVERWORLD) return;

        if (!isMoving)
        {
            // Snap to grid to prevent drift
            transform.position = new Vector3(Mathf.Round(transform.position.x), Y_SNAP_HEIGHT, Mathf.Round(transform.position.z));

            // Check input and attempt to move in given direction
            if (Input.GetKey(KeyCode.W)) 
                TryMove(Vector3.forward, PlayerDirection.UP);
            else if (Input.GetKey(KeyCode.S)) 
                TryMove(Vector3.back, PlayerDirection.DOWN);
            else if (Input.GetKey(KeyCode.D)) 
                TryMove(Vector3.right, PlayerDirection.RIGHT);
            else if (Input.GetKey(KeyCode.A)) 
                TryMove(Vector3.left, PlayerDirection.LEFT);
        }

        // If currently moving, continue to move toward destination
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            failsafeTimer += Time.deltaTime;

            // Stop movement when destination is reached
            if (transform.position == destination || failsafeTimer > failsafeTimeOut)
            {
                isMoving = false;
                failsafeTimer = 0;

                // Ensure final position is snapped to grid
                transform.position = new Vector3(Mathf.Round(destination.x), destination.y, Mathf.Round(destination.z));

                // Decrement random battle counter
                GameController.Instance.DecrementBattleCounter();
            }
        }
    }

    // Try to move player in the specified direction of target tile is walkable
    private void TryMove(Vector3 offset, PlayerDirection facingDirection)
    {
        // Rotate player to face attempted direction
        switch (facingDirection)
        {
            case PlayerDirection.UP: transform.rotation = Quaternion.Euler(0f, 0f, 0f); break;
            case PlayerDirection.RIGHT: transform.rotation = Quaternion.Euler(0f, 90f, 0f); break;
            case PlayerDirection.DOWN: transform.rotation = Quaternion.Euler(0f, 180f, 0f); break;
            case PlayerDirection.LEFT: transform.rotation = Quaternion.Euler(0f, -90f, 0f); break;
            default: break;
        }
        // Determine target position to try
        Vector3 targetPos = transform.position + offset;

        // If position is walkable, start walk cycle in direction
        if (GameController.Instance.FloorPositionsList().Contains(targetPos))
        {
            destination = targetPos;
            direction = facingDirection;
            isMoving = true;
        }
    }

    // Coroutine that move player to battle position at battle start
    public IEnumerator BattleStart()
    {
        // Target position for battle start
        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y - 10, transform.position.z);

        // Smoothly move player to battle start position
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10);
            yield return null;
        }
    }
}
