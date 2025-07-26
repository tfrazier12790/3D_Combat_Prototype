using System.Collections.Generic;
using UnityEngine;

// Enum for defining the cardinal directions
public enum CardinalDirection { UP, DOWN, LEFT, RIGHT };

/// <summary>
/// Procedurally generates a dungeon layout using a random walk algorithm for rooms and corridors.
/// Instantiates floor and wall objects based on the positions and communicates the layout to the
/// GameController.
/// </summary>

public class DungeonGeneratorScript : MonoBehaviour
{
    // Floor and wall prefabs
    [SerializeField] GameObject floorTile;
    [SerializeField] GameObject wallObject;

    // Parent objects for organization
    GameObject dungeonRoot;
    GameObject floorParts;
    GameObject wallParts;

    // Scriptable Object that stores dungeon generation parameters
    [SerializeField] DungeonGeneratorSO dungeonParameters;

    // Floor and wall position tracking
    private Vector3 startingPos;
    private List<Vector3> floorPositions = new List<Vector3>();
    private List<GameObject> dungeonFloorObjects = new List<GameObject>();
    private List<GameObject> dungeonWallObjects = new List<GameObject>();
    private List<Vector3> roomPositions = new List<Vector3>();
    private List<Vector3> wallPositions = new List<Vector3>();

    // Main generation logic, runs on start
    public void Start()
    {
        // Setup parent objects for organization
        dungeonRoot = new GameObject("DungeonRoot");
        floorParts = new GameObject("Floors");
        floorParts.transform.SetParent(dungeonRoot.transform);
        wallParts = new GameObject("Walls");
        wallParts.transform.SetParent(dungeonRoot.transform);

        // Setup and prepare the starting position of the dungeon generation
        startingPos = transform.position;
        Vector3 lastPosition = startingPos;
        Vector3 roomPosition = startingPos;
        floorPositions.Add(startingPos);
        roomPositions.Add(roomPosition);
        dungeonFloorObjects.Add(Instantiate(floorTile, startingPos, Quaternion.identity, floorParts.transform));

        // Main room generation loop
        for (int i = 0; i < dungeonParameters.iterations; i++)
        {
            CardinalDirection direction = GetRandomDirection();

            for (int s = 0; s < dungeonParameters.steps; s++)
            {
                Vector3 newPosition = GetPositionInDirection(lastPosition, direction);


                // Place tile only if not already placed
                if (!floorPositions.Contains(newPosition))
                {
                    dungeonFloorObjects.Add(Instantiate(floorTile, newPosition, Quaternion.identity, floorParts.transform));
                    floorPositions.Add(newPosition);
                }

                lastPosition = newPosition;

                // Pick a new direction, avoiding an immediate reversal
                do
                {
                    direction = GetRandomDirection();
                } while (IsOpposite(direction, lastPosition, newPosition));
            }

            // Corridor generation logic
            if (i != dungeonParameters.iterations - 1)
            {
                CardinalDirection corridorDirection = GetRandomDirection();
                Vector3 currentCorridorPos = lastPosition;

                for (int c = 0; c < dungeonParameters.corridorIterations; c++)
                {
                    for (int d = 0; d < dungeonParameters.corridorSteps; d++)
                    {
                        Vector3 newCorridorPos = GetPositionInDirection(currentCorridorPos, corridorDirection);

                        if (!floorPositions.Contains(newCorridorPos))
                        {
                            dungeonFloorObjects.Add(Instantiate(floorTile, newCorridorPos, Quaternion.identity, floorParts.transform));
                            floorPositions.Add(newCorridorPos);
                        }

                        currentCorridorPos = newCorridorPos;
                        roomPositions.Add(currentCorridorPos);
                        lastPosition = currentCorridorPos;
                    }
                }

                corridorDirection = GetNewCorridorDirection(corridorDirection);
            }
        }

        // Generate walls around floor positions, and communicate floor positions to the GameController
        GenerateWalls();
        GameController.Instance.SetFloorPositionList(GetDungeonFloorPositions(), dungeonFloorObjects, wallPositions, dungeonWallObjects);
    }

    // Instantiates walls around the edge of every floor tile if no floor is adjacent
    private void GenerateWalls()
    {
        foreach (Vector3 position in floorPositions)
        {
            TryPlaceWall(position + Vector3.forward);
            TryPlaceWall(position + Vector3.back);
            TryPlaceWall(position + Vector3.right);
            TryPlaceWall(position + Vector3.left);
            TryPlaceWall(position + new Vector3(1, 0, 1));
            TryPlaceWall(position + new Vector3(1, 0, -1));
            TryPlaceWall(position + new Vector3(-1, 0, 1));
            TryPlaceWall(position + new Vector3(-1, 0, -1));
        }
    }

    // Places a wall at the specified position if there is no floor there
    private void TryPlaceWall(Vector3 position)
    {
        Vector3 wallPosition = new Vector3(position.x, -0.5f, position.z);
        if (!floorPositions.Contains(position) && !wallPositions.Contains(wallPosition))
        {
            dungeonWallObjects.Add(Instantiate(wallObject, wallPosition, Quaternion.identity, wallParts.transform));
            wallPositions.Add(wallPosition);
        }
    }

    // Picks a random cardinal direction
    public CardinalDirection GetRandomDirection() => (CardinalDirection)Random.Range(0, 4);

    // Converts a directions to a position offset
    private Vector3 GetPositionInDirection(Vector3 currentPosition, CardinalDirection direction)
    {
        return direction switch
        {
            CardinalDirection.UP => currentPosition + Vector3.forward,
            CardinalDirection.DOWN => currentPosition + Vector3.back,
            CardinalDirection.RIGHT => currentPosition + Vector3.right,
            CardinalDirection.LEFT => currentPosition + Vector3.left,
            _ => currentPosition
        };
    }

    // Prevents corridor direction from turning back on itself
    private CardinalDirection GetNewCorridorDirection(CardinalDirection corridorDirection)
    {
        CardinalDirection newDirection;
        do
        {
            newDirection = GetRandomDirection();
        } while (
            (corridorDirection == CardinalDirection.UP && newDirection == CardinalDirection.DOWN) ||
            (corridorDirection == CardinalDirection.DOWN && newDirection == CardinalDirection.UP) ||
            (corridorDirection == CardinalDirection.RIGHT && newDirection == CardinalDirection.LEFT) ||
            (corridorDirection == CardinalDirection.LEFT && newDirection == CardinalDirection.RIGHT)
        );
        return newDirection;
    }

    // Determines if a direction is reversing the last move
    private bool IsOpposite(CardinalDirection direction, Vector3 lastPosition, Vector3 currentPosition)
    {
        Vector3 offset = currentPosition - lastPosition;
        return (direction == CardinalDirection.UP && offset == Vector3.back) ||
            (direction == CardinalDirection.DOWN && offset == Vector3.forward) ||
            (direction == CardinalDirection.LEFT && offset == Vector3.right) ||
            (direction == CardinalDirection.RIGHT && offset == Vector3.left);
    }

    // Returns adjusted floor positions with corrected Y-height
    public List<Vector3> GetDungeonFloorPositions()
    {
        List<Vector3> newFloorPositions = new List<Vector3>();
        foreach (Vector3 position in floorPositions)
        {
            newFloorPositions.Add(new Vector3(position.x, -0.125f, position.z));
        }
        return newFloorPositions;
    }
}
