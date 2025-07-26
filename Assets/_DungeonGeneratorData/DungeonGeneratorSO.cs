using UnityEngine;

/// <summary>
/// A ScriptableObject that defines the generation parameters for the dungeon layout. 
/// These values control how many steps and iterations are used when generating rooms
/// and corridors, allowing for flexible tuning of dungeon layouts.
/// 
/// Used by the Dungeon Generator system to create different size dungeons.
/// </summary>

[CreateAssetMenu(fileName = "New Dungeon Type", menuName = "Dungeon/New Type")]

public class DungeonGeneratorSO : ScriptableObject
{
    [Header("Dungeon Parameters")]
    [Tooltip("Size of each room in steps")]
    public int steps;

    [Tooltip("How many rooms to create")]
    public int iterations;

    [Tooltip("Length of each corridor in steps")]
    public int corridorSteps;

    [Tooltip("How many corridor segments before another room is generated")]
    public int corridorIterations;

}
