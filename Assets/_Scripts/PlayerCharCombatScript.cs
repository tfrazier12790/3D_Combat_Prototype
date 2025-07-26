using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles everything the player can do during combat including movement and 
/// attack behaviors, and stats like health and the amout of actions they can perform
/// each turn.
/// </summary>

public class PlayerCharCombatScript : MonoBehaviour
{
    // Movement and grid parameters
    List<Vector3> gridLocations;       // Valid tiles the player can move to
    Vector3 destination;               // Target position for current movement
    Vector3 playerPosForTargeting;     // Position used for targeting
    float speed = 1f;                  // Movement speed
    bool canMove = true;               // Is the player allowed to move now
    bool isMoving = false;             // Is the player currently moving

    // Relevant stats for combat
    int maxHealth = 16;
    int currentHealth = 16;
    int maxActions = 4;
    int actions = 4;

    // Combat prefabs and attacks
    [SerializeField] TMP_Text actionsText; 
    [SerializeField] List<PlayerAttackSO> attacks;      // Attacks available to the player
    [SerializeField] GameObject playerHitMarker;        // Prefab used for previewing attacks
    List<GameObject> targetPreviews = new List<GameObject>();                    // Instantiated previews
    bool choosingAttack = false;
    [SerializeField] GameObject confirmationText;

    void Start()
    {
        // Initialize list of valid grid locations for movement and targeting
        gridLocations = BattleControllerScript.Instance.GetGridLocations();

        // Store initial player position for targeting
        playerPosForTargeting = transform.position;
    }

    void Update()
    {
        // Display Remaining Player Actions
        actionsText.gameObject.SetActive(true);
        actionsText.text = $"Actions: {actions}";

        // Handle character movement and input
        HandleMovement();   

        // Handle attack input if the player can act and is not already previewing an attack
        if (actions > 0 && canMove && !choosingAttack)
        {
            HandleAttackInput();
        }

        // End turn if spacebar is pressed
        if (Input.GetKeyDown(KeyCode.Space) && BattleControllerScript.Instance.currentTurn == Turn.PlayerTurn)
        {
            BattleControllerScript.Instance.EndTurn();
        }
    }

    private void HandleAttackInput()
    {
        // Handle hotkey inputs to initiate non-targeted attacks
        if (BattleControllerScript.Instance.currentTurn != Turn.PlayerTurn) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) TryAttack(attacks[0]);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryAttack(attacks[1]);
    }

    private void TryAttack(PlayerAttackSO attack)
    {
        // Only allow non-targeted attacks
        if (attack.GetTargetingType() == PlayerTargetingType.TargetedAttack) return;

        canMove = false;
        choosingAttack = true;

        // Show preview tiles and begin confirmation routine
        PreviewAttack(playerHitMarker, attack);
        StartCoroutine(WaitForConfirmation(attack));
    }

    private IEnumerator HandleTargetedAttack(PlayerAttackSO attack)
    {
        // Generate a list of valid targets
        List<Vector3> validTargets = BattleControllerScript.Instance.GetEnemyLocations();

        int selectedIndex = 0;
        bool targetSelected = false;

        // Highlight the current tartget
        GameObject targetHighlight = Instantiate(playerHitMarker, validTargets[selectedIndex], Quaternion.identity);

        while (!targetSelected)
        {
            // Allow the player to cycle through valid targets using the A and D keys
            if (Input.GetKeyDown(KeyCode.A))
            {
                selectedIndex = (selectedIndex - 1 + validTargets.Count) % validTargets.Count;
                targetHighlight.transform.position = validTargets[selectedIndex];
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                selectedIndex = (selectedIndex + 1) % validTargets.Count;
                targetHighlight.transform.position = validTargets[selectedIndex];
            }
            // Executee attack on pressing the enter key
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                targetSelected = true;
            }

            yield return null;
        }

        // Execute the chosen attack on the selected target
        Vector3 chosenTarget = validTargets[selectedIndex];
        attack.ExecuteAttack();
        Destroy(targetHighlight);
        yield return new WaitForSeconds(0.5f);
    }

    private void HandleMovement()
    {
        if (isMoving)
        {
            // Continue moving toward destination
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            // Snaap to the grid when reaching target tile
            if (transform.position == destination)
            {
                transform.position = RoundVector3(transform.position);
                playerPosForTargeting = transform.position;
                isMoving = false;
                canMove = true;
            }
        }

        // check if player can issue a movement command
        if (!canMove || actions <= 0) return;

        // Grid-based movement input
        if (Input.GetKeyDown(KeyCode.W)) TryToMove(Vector3.forward, PlayerDirection.UP);
        if (Input.GetKeyDown(KeyCode.S)) TryToMove(Vector3.back, PlayerDirection.DOWN);
        if (Input.GetKeyDown(KeyCode.D)) TryToMove(Vector3.right, PlayerDirection.RIGHT);
        if (Input.GetKeyDown(KeyCode.A)) TryToMove(Vector3.left, PlayerDirection.LEFT);
    }

        
    private void TryToMove(Vector3 movementDirection, PlayerDirection facingDirection)
    {
        // Snap position to grid and calculate target tile
        transform.position = RoundVector3(transform.position);
        destination = transform.position + movementDirection;

        // Face player in the given direction
        switch (facingDirection)
        {
            case PlayerDirection.UP: transform.rotation = Quaternion.Euler(0f, 0f, 0f); break;
            case PlayerDirection.RIGHT: transform.rotation = Quaternion.Euler(0f, 90f, 0f); break;
            case PlayerDirection.DOWN: transform.rotation = Quaternion.Euler(0f, 180f, 0f); break;
            case PlayerDirection.LEFT: transform.rotation = Quaternion.Euler(0f, -90f, 0f); break;
            default: break;
        }

        // Only allow move if tile is walkable/valid
        if (gridLocations.Contains(destination))
        {
            actions--;
            canMove = false;
            isMoving = true;
        }
    }

    private Vector3 RoundVector3(Vector3 vector3) => new Vector3(Mathf.Round(vector3.x), Mathf.Round(vector3.y), Mathf.Round(vector3.z));

    public void PreviewAttack(GameObject markerPrfab, PlayerAttackSO attack)
    {
        // Show target preview indicators for all positions affected by this attack
        foreach (Vector3 offset in attack.GetAttackLocations())
        {
            // Convert the local offset to world space on the player's rotation
            Vector3 relatedOffset = transform.TransformDirection(offset);

            // Apply it to the player's current position
            Vector3 targetPos = transform.position + relatedOffset;

            // Instantiate the marker related to player rotation
            GameObject targetPreview = Instantiate(markerPrfab, targetPos, Quaternion.identity, transform);
            targetPreviews.Add(targetPreview);
        }
    }

    public void CancelPreview()
    {
        // Remove all active preview indicators
        foreach (GameObject preview in targetPreviews)
        {
            Destroy(preview);
        }
        targetPreviews.Clear();
    }

    public IEnumerator WaitForConfirmation(PlayerAttackSO chosenAttack)
    {
        // Display confirmation UI
        bool decision = false;
        bool executeAttack = false;
        confirmationText.SetActive(true);

        // Wait for player to confirm or cancel attack
        while (!decision)
        {

            if(Input.GetKeyDown(KeyCode.D))
            {
                transform.Rotate(Vector3.up, 90f);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                transform.Rotate(Vector3.up, -90f);
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                decision = true;
                executeAttack = true;
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                decision = true;
            }
            yield return null;
        }

        // If confirmed, perform attack and apply damage
        if (executeAttack)
        {
            actions--;
            foreach (GameObject targetPreview in targetPreviews)
            {
                targetPreview.GetComponent<PlayerHitMarkerScript>().SetDamage(GetDamage());
            }
            CancelPreview();
            yield return StartCoroutine(chosenAttack.ExecuteAttack());
        }
        else
        {
            CancelPreview();
        }
        canMove = true;
        choosingAttack = false;
        confirmationText.SetActive(false);
    }

    // Deal damage to player
    public void Damage(int damageTaken)
    {
        currentHealth -= damageTaken;
    }

    // Current stat getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetRemainingActions() => actions;

    // Reset actions. used at beginning of turn
    public void ResetActions() => actions = maxActions;

    public Vector3 PlayerPositionForTargeting() => playerPosForTargeting;

    internal int GetDamage()
    {
        // PLACEHOLDER VALUE. SCALE WITH PLAYER STATS LATER
        return 3;
    }
    
    // Simple animation that returns the battle stage into the dungeon.
    public IEnumerator BattleEnd()
    {
        actionsText.gameObject.SetActive(false);
        Vector3 targetPos = new Vector3(transform.position.x, -0.125f, transform.position.z);
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10);
            yield return null;
        }
    }
}