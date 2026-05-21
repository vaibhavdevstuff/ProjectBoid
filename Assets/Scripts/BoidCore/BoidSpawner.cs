using System.Collections;
using ProjectBoid.Data;
using UnityEngine;

namespace ProjectBoid.BoidCore
{
    public class BoidSpawner : MonoBehaviour
    {
        [SerializeField] private BoidDataSO _boidData;
        [SerializeField] private bool _spawnAtOnce = false;

        BoidManager _boidManager;
        
        private void Awake()
        {
            _boidManager = FindAnyObjectByType<BoidManager>();

            if (!_boidManager)
            {
                Debug.LogError($"BoidManager not found on {gameObject.name}");
                return;
            }
            
            _boidManager.SetBoidCount(_boidData.UnitSpawnCount);
            
            StartCoroutine(SpawnBoids());
        }

        IEnumerator SpawnBoids()
        {
            var unitParent =  new GameObject("UnitParent");
            unitParent.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            
            for (var i = 0; i < _boidData.UnitSpawnCount; i++) 
            {
                var spawnPosition = transform.position + Random.insideUnitSphere * _boidData.SpawnRadius;
                
                var unit = Instantiate(_boidData.BoidUnit);
                unit.transform.position = spawnPosition;
                unit.transform.forward = Random.insideUnitSphere;
                
                unit.name = $"BoidUnit ({i + 1})";
                
                unit.transform.parent = unitParent.transform;
                
                _boidManager.AddBoidUnit(unit);
                
                if (!_spawnAtOnce)
                    yield return new WaitForEndOfFrame();
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
