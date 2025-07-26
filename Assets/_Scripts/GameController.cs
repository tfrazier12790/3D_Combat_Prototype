using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines possible game states for the game controller to use
public enum GameState { OVERWORLD, BATTLE, MENU, TRANSITIONING };

/// <summary>
/// Controls high-level game flow including transitions between overworld and battle.
/// Manages singleton instance, dungeon data, and 
/// </summary>

public class GameController : MonoBehaviour
{
    // Singleton instance
    public static GameController Instance { get; private set; }

    [Header("Current Game State")]
    public GameState currentState;

    // Object references
    GameObject player;
    GameObject mainCam;
    DungeonGeneratorScript dungeonGenerator;
    [SerializeField] GameObject tutorialWindow;

    // Dungeon data
    List<Vector3> dungeonFloorPositions;
    List<Vector3> dungeonWallPositions;
    List<GameObject> dungeonFloorObjects;
    List<GameObject> dungeonWallObjects;

    // Objects currently active in battle staging
    List<GameObject> battleStageObjects;

    // Time between each iteration of floor movement in floor drop animation
    float waitTime;

    // Counters for determining when a random battle occurs
    [SerializeField] int randomBattleCounter;
    [SerializeField] int randomBattleDecrement = 12;

    private void Awake()
    {
        // Classic singleton script
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Find and cache player and camera object
        player = GameObject.FindGameObjectWithTag("Player");
        mainCam = GameObject.FindGameObjectWithTag("MainCamera");

        // Initialize battle stage objects list
        battleStageObjects = new List<GameObject>();

        // Get Dungeon generator and floor positions
        dungeonGenerator = GameObject.FindAnyObjectByType<DungeonGeneratorScript>();
        dungeonFloorPositions = dungeonGenerator.GetDungeonFloorPositions();

        // Set current state
        currentState = GameState.OVERWORLD;

        // Initialize starting random battle counter
        SetRandomBattleCounter();

        // Display Tutorial Window
        tutorialWindow.SetActive(true);
        Time.timeScale = 0f;
        currentState = GameState.MENU;
    }

    void Update()
    {
        // FOR DEBUGGING: End current battle immediately
        if (Input.GetKeyDown(KeyCode.F) && currentState == GameState.BATTLE)
        {
            Debug.Log("F Key Down");
            StartCoroutine(EndBattle());
        }
    }

    private void LateUpdate()
    {
        // Trigger random battle when counter reaches zero
        if (currentState == GameState.OVERWORLD && randomBattleCounter <= 0)
        {
            SetRandomBattleCounter();
            currentState = GameState.TRANSITIONING;
            StartCoroutine(StartBattle());
        }
    }

    // Update the dungeon floor and wall positions and objects for battle setup
    public void SetFloorPositionList(List<Vector3> floorPositions, List<GameObject> dungeonFloorObjectsList, List<Vector3> wallPositions, List<GameObject> dungeonWallObjectsList)
    {
        dungeonFloorPositions = floorPositions;
        dungeonFloorObjects = dungeonFloorObjectsList;
        dungeonWallPositions = wallPositions;
        dungeonWallObjects = dungeonWallObjectsList;
    }

    // Returns walkable floor positions
    public List<Vector3> FloorPositionsList() => dungeonFloorPositions;

    // Starts the battle sequence by settins up battle stage objects, enemy positions, and triggers the animation
    public IEnumerator StartBattle()
    {
        List<Vector3> potentialEnemyPositions = new List<Vector3>();

        // Define a 5x5 area around the player in the dungeon to set up the battle stage objects
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                Vector3 floorCheckPos = new Vector3(player.transform.position.x + i, -0.125f, player.transform.position.z + j);
                Vector3 wallCheckPos = new Vector3(player.transform.position.x + i, -0.5f, player.transform.position.z + j);

                if (dungeonFloorPositions.Contains(floorCheckPos))
                {
                    int index = dungeonFloorPositions.IndexOf(floorCheckPos);
                    GameObject floorObject = dungeonFloorObjects[index];
                    battleStageObjects.Add(floorObject);

                    //Add enemy spawn positions adjacent to edges of the 5x5 grid
                    if (i == -2) potentialEnemyPositions.Add(new Vector3(player.transform.position.x + i - 1, -10, player.transform.position.z + j));
                    if (i == 2) potentialEnemyPositions.Add(new Vector3(player.transform.position.x + i + 1, -10, player.transform.position.z + j));
                    if (j == -2) potentialEnemyPositions.Add(new Vector3(player.transform.position.x + i, -10, player.transform.position.z + j - 1));
                    if (j == 2) potentialEnemyPositions.Add(new Vector3(player.transform.position.x + i, -10, player.transform.position.z + j + 1));

                }
                else if (dungeonWallPositions.Contains(wallCheckPos))
                {
                    int index = dungeonWallPositions.IndexOf(wallCheckPos);
                    battleStageObjects.Add(dungeonWallObjects[index]);
                }
            }
        }

        List<Vector3> battleFloorLocations = new List<Vector3>();
        List<Vector3> battleWallLocations = new List<Vector3>();

        // Drop battle stage objects into place with animation
        foreach (GameObject dungeonObject in battleStageObjects)
        {
            if (dungeonFloorObjects.Contains(dungeonObject))
            {
                battleFloorLocations.Add(new Vector3(dungeonObject.transform.position.x, dungeonObject.transform.position.y, dungeonObject.transform.position.z));
            }
            else if (dungeonWallObjects.Contains(dungeonObject))
            {
                battleWallLocations.Add(new Vector3(dungeonObject.transform.position.x, dungeonObject.transform.position.y, dungeonObject.transform.position.z));
            }

            // Play drop animation for each object
            StartCoroutine(dungeonObject.GetComponent<BattleDropScript>().DropScript());

            // If this object is under the player, trigger battle start animations
            if (dungeonObject.transform.position.x == player.transform.position.x && dungeonObject.transform.position.z == player.transform.position.z)
            {
                StartCoroutine(player.GetComponent<PlayerOverworldScript>().BattleStart());
                StartCoroutine(mainCam.GetComponent<MainOverworldCameraScript>().BattleStartAnimation());
            }
            yield return new WaitForSeconds(waitTime);
        }

        // Setup the battle grid with floor, enemy spawn, and wall positions
        BattleControllerScript.Instance.SetGridLocations(battleFloorLocations, potentialEnemyPositions, battleWallLocations);

        // Disable overworld controls and enable battle controls on player
        player.GetComponent<PlayerOverworldScript>().enabled = false;
        player.GetComponent<PlayerCharCombatScript>().enabled = true;

        // Update current game state and start battle controller setup
        currentState = GameState.BATTLE;
        StartCoroutine(BattleControllerScript.Instance.BattleSetup());
    }

    // Ends the battle sequence, returning player and dungeons objects to original positions
    public IEnumerator EndBattle()
    {
        currentState = GameState.TRANSITIONING;

        // Play return animation for each battle stage object
        for (int i = 0; i < battleStageObjects.Count; i++)
        {
            GameObject dungeonObject = battleStageObjects[i];
            
            // Trigger battle end animations on player and camera at players position
            if (dungeonObject.transform.position.x == player.transform.position.x && 
                dungeonObject.transform.position.z ==  player.transform.position.z)
            {
                StartCoroutine(player.GetComponent<PlayerCharCombatScript>().BattleEnd());
                StartCoroutine(mainCam.GetComponent<MainOverworldCameraScript>().BattleEndAnimation());
            }

            // Skip waiting on last object; It will be handled after the loop
            if (i != battleStageObjects.Count - 1)
            {
                StartCoroutine(dungeonObject.GetComponent<BattleDropScript>().ReturnScript());
                yield return new WaitForSeconds(waitTime);
            }
        }

        //Wait for last object return animation to finish
        GameObject lastObject = battleStageObjects[battleStageObjects.Count - 1];
        StartCoroutine(lastObject.GetComponent<BattleDropScript>().ReturnScript());
        yield return new WaitUntil(() => lastObject.GetComponent<BattleDropScript>().GetIsFinished());

        // Restore overworld controls
        player.GetComponent<PlayerOverworldScript>().enabled = true;
        player.GetComponent<PlayerCharCombatScript>().enabled = false;

        BattleControllerScript.Instance.EndBattle();

        battleStageObjects.Clear();
        currentState = GameState.OVERWORLD;
    }

    // Sets the battle counter to a random number in specified range
    public void SetRandomBattleCounter()
    {
        randomBattleCounter = Random.Range(64, 513);
    }

    // Decrements the random battle counter by the predefined decrement value
    public void DecrementBattleCounter()
    {
        randomBattleCounter -= randomBattleDecrement;
    }

    public void CloseTutorialWindow()
    {
        tutorialWindow.SetActive(false);
        currentState = GameState.OVERWORLD;
        Time.timeScale = 1.0f;
    }
}
