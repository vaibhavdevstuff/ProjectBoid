using System.Collections.Generic;
using ProjectBoid.Data;
using UnityEngine;

namespace ProjectBoid.BoidCore
{
    public class BoidManager : MonoBehaviour
    {
        [SerializeField] private BoidDataSO _boidData;
        [SerializeField] private Transform _target;

        private List<int> _neighbourIndexBuffer = new List<int>(32);
        private List<BoidUnit> _boidUnits;
        private BoidJobData _jobData;
        
        public List<BoidUnit> BoidUnits => _boidUnits;

        private void Start()
        {
            _jobData = new BoidJobData();
            _jobData.Init(_boidData);
        }

        public void AddBoidUnit(BoidUnit unit)
        {
            _boidUnits ??= new List<BoidUnit>(_boidData.UnitSpawnCount);
            
            unit.SetBoidManager(this);
            unit.SetTarget(_target);
            _boidUnits.Add(unit);
        }

        private void Update()
        {
            if (_boidUnits == null || _boidUnits.Count == 0) return;

            UpdateBoidBehaviourData();

            UpdateJob();
        }

        private void UpdateJob()
        {
            var activeCount = _boidUnits.Count;

            for (var i = 0; i < activeCount; i++)
                _jobData.SetUnitPosition(i, _boidUnits[i].Position);

            _jobData.Update(_boidData, activeCount);
        }

        private void OnDestroy()
        {
            _jobData?.Dispose();
        }

        private void UpdateBoidBehaviourData()
        {
            var unitCount = _boidUnits.Count;

            var viewRadiusSqr = _boidData.PerceptionRadius * _boidData.PerceptionRadius;
            var avoidRadiusSqr = _boidData.AvoidanceRadius * _boidData.AvoidanceRadius;

            foreach (var boidUnit in _boidUnits)
            {
                boidUnit.ResetBoidBehaviourData();
            }

            for (var i = 0; i < unitCount; i++)
            {
                _neighbourIndexBuffer.Clear();
                
                _jobData.GetNeighbourIndices(i, _neighbourIndexBuffer);

                foreach (var _neighbourIndex in _neighbourIndexBuffer)
                {
                    if(i == _neighbourIndex) continue;
                    
                    if (_neighbourIndex >= _boidUnits.Count) continue;
                    
                    //var unit = _boidUnits[_neighbourIndex];
                    
                    var offset = _boidUnits[_neighbourIndex].Position - _boidUnits[i].Position;
                    var sqrDistance = offset.sqrMagnitude;

                    if (sqrDistance < viewRadiusSqr)
                    {
                        _boidUnits[i].PerceivedFlockmatesCount += 1;
                        _boidUnits[i].AverageFlockHeadingDirection += _boidUnits[_neighbourIndex].Forward;
                        _boidUnits[i].CenterOfFlock += _boidUnits[_neighbourIndex].Position;

                        if (sqrDistance < avoidRadiusSqr)
                        {
                            _boidUnits[i].AverageAvoidanceHeading -= offset / sqrDistance;
                        }
                    }
                }
            }

            foreach (var boidUnit in _boidUnits)
            {
                boidUnit.UpdateUnit();
            }
        }

    }
}