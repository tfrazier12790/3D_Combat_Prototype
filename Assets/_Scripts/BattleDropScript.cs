using System.Collections;
using UnityEngine;

/// <summary>
/// Handles a visual "drop in" and "return" animation for the battle stage components.
/// </summary>

public class BattleDropScript : MonoBehaviour
{
    // Speed at which the object moves during the drop/return animation
    [SerializeField] float speed = 10;

    // Flag indicating whenether the return animation has completed
    private bool isFinished = false;

    // Starting and target positions for movement
    Vector3 startPos;
    Vector3 targetPos;

    // Coroutine that moves the object downward to simulate a drop-in effect
    public IEnumerator DropScript()
    {
        isFinished = false;
        
        // Store the current starting position
        startPos = transform.position;

        // Set target position as 10 units below the start positiuon
        targetPos = new Vector3(transform.position.x, transform.position.y - 10, transform.position.z);

        // Smoothly move towards the target position using Lerp until it is close to target position
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        // Snap to exact position at end
        transform.position = targetPos;
    }

    // Coroutine that moves the object back to its original position
    public IEnumerator ReturnScript()
    {
        // Smoothly moves back to the start position
        while (Vector3.Distance(transform.position, startPos) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, startPos, speed * Time.deltaTime);
            yield return null;
        }

        // Snaps object back to original position at the end
        transform.position = startPos;

        // Marks the animation as complete
        isFinished = true;
    }

    // Returns whether the animation is finished
    public bool GetIsFinished ()
    {
        return isFinished;
    }
}
