using ProjectBoid.Data;
using UnityEngine;

namespace ProjectBoid.BoidCore
{
    public class BoidSpawner : MonoBehaviour
    {
        [SerializeField] private BoidDataSO _boidData;

        private void Awake()
        {
            GameObject unitParent =  new GameObject("UnitParent");
            unitParent.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            for (var i = 0; i < _boidData.UnitSpawnCount; i++) 
            {
                var spawnPosition = transform.position + Random.insideUnitSphere * _boidData.SpawnRadius;
                
                var unit = Instantiate (_boidData.UnitPrefab);
                unit.transform.position = spawnPosition;
                unit.transform.forward = Random.insideUnitSphere;
                
                unit.name = $"BoidUnit ({i + 1})";
                
                unit.transform.parent = unitParent.transform;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_boidData) return;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _boidData.SpawnRadius);
        }
    }
}
