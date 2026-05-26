using ProjectBoid.BoidCore;
using UnityEngine;

namespace ProjectBoid.Data
{
    [CreateAssetMenu(menuName = "Game Data/Boid Data", fileName = "New Boid Data")]
    public class BoidDataSO : ScriptableObject
    {
        [field: Header("Unit Data")] 
        [field: SerializeField] public float MaxSpeed { get; private set; } = 8f;
        [field: SerializeField] public float MinSpeed { get; private set; } = 5f;
        [field: SerializeField] public float MaxSteerForce { get; private set; } = 50f;
        [field: SerializeField] public float PerceptionRadius { get; private set; } = 1f;
        [field: SerializeField] public float AvoidanceRadius { get; private set; } = 1f;
        [field: SerializeField] public float AlignWeight { get; private set; } = 1f;
        [field: SerializeField] public float CohesionWeight { get; private set; } = 1f;
        [field: SerializeField] public float SeperateWeight { get; private set; } = 1f;

        [field: Header("Obstacle Data")]
        [field: SerializeField] public int RayViewDirectionCount { get; private set; } = 200;
        [field: SerializeField] public float BoundsRadius { get; private set; } = 0.3f;
        [field: SerializeField] public float RayDistance { get; private set; } = 5f;
        [field: SerializeField] public float AvoidCollisionWeight { get; private set; } = 20f;
        [field: SerializeField] public LayerMask ObstacleLayer { get; private set; }

        [field: Header("Partitioning Data")] 
        [field: SerializeField] public float CellSize { get; private set; } = 2f;
        
        [field: Header("Spawn Data")] 
        [field: SerializeField] public BoidUnit BoidUnit { get; private set; }
        [field: SerializeField] public int UnitSpawnCount { get; private set; } = 200;
        [field: SerializeField] public float SpawnRadius { get; private set; } = 5f;
    }
}