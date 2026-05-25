using System;
using ProjectBoid.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace ProjectBoid.BoidCore
{
    public class BoidUnit : MonoBehaviour
    {
        [SerializeField] private bool _debug = false;
        [Space] [SerializeField] private BoidDataSO _boidData;
        [SerializeField] private Transform _target;

        [Header("Visual")] [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _colorA;
        [SerializeField] private Color _colorB;

        //--------------------------------------------------------------------------------------
        
        private bool _canFollowTarget = false;
        
        private float _perceivedFlockmatesCount;
        
        private Vector3 _averageFlockHeadingDirection;
        private Vector3 _centerOfFlock;
        private Vector3 _averageAvoidanceHeading;
        
        private Vector3 _position;
        private Vector3 _velocity;
        private Vector3 _acceleration;
        private Vector3 _forward;
        private Vector3[] _rayDirections;
        
        private Transform _cachedTransform;
        
        private BoidManager _boidManager;
        
        //--------------------------------------------------------------------------------------

        public Vector3 Position => _position;
        public Vector3 Forward => _forward;
        public float PerceivedFlockmatesCount
        {
            get => _perceivedFlockmatesCount;
            set => _perceivedFlockmatesCount = value;
        }
        public Vector3 AverageFlockHeadingDirection
        {
            get => _averageFlockHeadingDirection;
            set => _averageFlockHeadingDirection = value;
        }
        public Vector3 CenterOfFlock
        {
            get => _centerOfFlock;
            set => _centerOfFlock = value;
        }
        public Vector3 AverageAvoidanceHeading
        {
            get => _averageAvoidanceHeading;
            set => _averageAvoidanceHeading = value;
        }
        
        private void Awake()
        {
            GetRandomGradientColor();
        }

        private void Start()
        {
            GetAllRayDirections();

            _cachedTransform = transform;

            _position = _cachedTransform.position;
            _forward = _cachedTransform.forward;
            _velocity = _forward * _boidData.MaxSpeed;
        }

        public void SetBoidManager(BoidManager boidManager)
        {
            _boidManager = boidManager;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void Update()
        {
            //UpdateUnit();

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                _canFollowTarget = !_canFollowTarget;
        }
        
        public void UpdateUnit()
        {
            _acceleration += GetTargetSteerForce();
            
            if (_perceivedFlockmatesCount != 0) {
                _centerOfFlock /= _perceivedFlockmatesCount;

                var offsetToFlockmatesCentre = (_centerOfFlock - _position);

                var alignmentForce = SteerTowards (_averageFlockHeadingDirection) * _boidData.AlignWeight;
                var cohesionForce = SteerTowards (offsetToFlockmatesCentre) * _boidData.CohesionWeight;
                var seperationForce = SteerTowards (_averageAvoidanceHeading) * _boidData.SeperateWeight;

                _acceleration += alignmentForce;
                _acceleration += cohesionForce;
                _acceleration += seperationForce;
            }

            // Check Obstacle Collision
            if (IsGoingToCollision())
            {
                var collisionAvoidDir = GetObstacleRays();
                var collisionAvoidForce = SteerTowards(collisionAvoidDir) * _boidData.AvoidCollisionWeight;
                _acceleration += collisionAvoidForce;
            }

            _velocity += _acceleration * Time.deltaTime;
            _velocity = Vector3.ClampMagnitude(_velocity, _boidData.MaxSpeed);

            var currentSpeed = _velocity.magnitude;
            var clampedSpeed = Mathf.Clamp(currentSpeed, _boidData.MinSpeed, _boidData.MaxSpeed);

            if (currentSpeed < 0.01f)
                _velocity = _forward * clampedSpeed;
            else
                _velocity = _velocity.normalized * clampedSpeed;

            _cachedTransform.position += _velocity * Time.deltaTime;

            if (_velocity != Vector3.zero)
                _cachedTransform.forward = _velocity.normalized;

            _position = _cachedTransform.position;
            _forward = _cachedTransform.forward;
            _acceleration = Vector3.zero;
        }

        private Vector3 GetTargetSteerForce()
        {
            if (!_target || !_canFollowTarget) return Vector3.zero;

            var dirToTarget = _target.position - _position;
            var targetSteerForce = SteerTowards(dirToTarget);
            return targetSteerForce;
        }

        private Vector3 SteerTowards(Vector3 targetDirection)
        {
            var desireVelocity = targetDirection.normalized * _boidData.MaxSpeed;

            var steerForce = desireVelocity - _velocity;

            steerForce = Vector3.ClampMagnitude(steerForce, _boidData.MaxSteerForce);

            return steerForce;
        }

        private bool IsGoingToCollision()
        {
            return Physics.SphereCast(_position, _boidData.BoundsRadius, _forward, out var hit, _boidData.RayDistance,
                _boidData.ObstacleLayer);
        }

        private Vector3 GetObstacleRays()
        {
            foreach (var direction in _rayDirections)
            {
                var dir = _cachedTransform.TransformDirection(direction);
                var ray = new Ray(_position, dir);

                var hitObstacle = Physics.SphereCast(
                    ray,
                    _boidData.BoundsRadius,
                    _boidData.RayDistance,
                    _boidData.ObstacleLayer
                );

                if (_debug)
                {
                    Debug.DrawRay(
                        _position,
                        dir * _boidData.RayDistance,
                        hitObstacle ? Color.red : Color.green
                    );
                }

                if (!hitObstacle)
                {
                    return dir;
                }
            }

            return _forward;
        }

        private void GetAllRayDirections()
        {
            var rayViewDirectionCount = _boidData.RayViewDirectionCount;

            _rayDirections = new Vector3[rayViewDirectionCount];

            var goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
            var angleIncrement = Mathf.PI * 2 * goldenRatio;

            for (int i = 0; i < rayViewDirectionCount; i++)
            {
                var t = (float)i / rayViewDirectionCount;
                var inclination = Mathf.Acos(1 - 2 * t);
                var azimuth = angleIncrement * i;

                var x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                var y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                var z = Mathf.Cos(inclination);
                _rayDirections[i] = new Vector3(x, y, z);
            }
        }

        private void GetRandomGradientColor()
        {
            var t = Random.Range(0f, 1f);
            
            var gradColor = Color.Lerp(_colorA, _colorB, t);

            if (_renderer)
                _renderer.material.color = gradColor;
        }

        public void ResetBoidBehaviourData()
        {
            _perceivedFlockmatesCount = 0;
            _centerOfFlock = Vector3.zero;
            _averageAvoidanceHeading = Vector3.zero;
            _averageFlockHeadingDirection = Vector3.zero;
        }
    }
}