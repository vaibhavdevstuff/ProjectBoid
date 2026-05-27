using ProjectBoid.BoidCore;
using ProjectBoid.Helper;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectBoid.BoidDebug
{
    public class BoidLookup : MonoBehaviour
    {
        [SerializeField] private TargetFollower _targetFollower;

        private BoidManager _boidManager;
        private BoidUnit _target;

        private bool _isLookingUp = false;

        private void Start()
        {
            _boidManager = BoidManager.Instance;
        }

        private void Update()
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                _isLookingUp = !_isLookingUp;

                Lookup();
            }
        }

        private void Lookup()
        {
            if (!_isLookingUp)
            {
                if (_target)
                    _target.UnderLookup(false);
                
                _targetFollower.ResetTarget();
                return;
            }

            _target = _boidManager.GetRandomBoidUnit();
            _target.UnderLookup(true);

            _targetFollower.SetTarget(_target.transform);
        }
    }
}