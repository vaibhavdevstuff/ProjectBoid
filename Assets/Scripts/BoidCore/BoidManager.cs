using System;
using System.Collections.Generic;
using ProjectBoid.Data;
using UnityEngine;

namespace ProjectBoid.BoidCore
{
    public class BoidManager : MonoBehaviour
    {
        [SerializeField] public BoidDataSO _boidData;
        [SerializeField] private Transform _target;
        
        private List<BoidUnit> _boidUnits;

        public List<BoidUnit> BoidUnits => _boidUnits;
        
        public void SetBoidCount(int count)
        {
            _boidUnits = new List<BoidUnit>(count);
        }

        public void AddBoidUnit(BoidUnit unit)
        {
            unit.SetBoidManager(this);
            unit.SetTarget(_target);
            _boidUnits.Add(unit);
        }

        private void Update()
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
                for (var j = 0; j < unitCount; j++)
                {
                    if (i == j) continue;
                    
                    var offset = _boidUnits[j].Position - _boidUnits[i].Position;
                    var sqrDistance = offset.sqrMagnitude;

                    if (sqrDistance < viewRadiusSqr)
                    {
                        _boidUnits[i].PerceivedFlockmatesCount += 1;
                        _boidUnits[i].AverageFlockHeadingDirection += _boidUnits[j].Forward;
                        _boidUnits[i].CenterOfFlock += _boidUnits[j].Position;

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
