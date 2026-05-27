using Unity.Cinemachine;
using UnityEngine;

namespace ProjectBoid.Helper
{
    /// <summary>
    /// Drag a target. Drag the Cinemachine camera. Done.
    /// </summary>
    public class TargetFollower : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        [Space]
        [SerializeField] private float _followSpeed = 8f;
        [SerializeField] private float _rotationSpeed = 5f;

        private void Start()
        {
            if (_cinemachineCamera != null)
            {
                _cinemachineCamera.Follow = transform;
                _cinemachineCamera.LookAt = transform;
            }
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            _cinemachineCamera.gameObject.SetActive(true);
        }

        public void ResetTarget()
        {
            _target = null;
            _cinemachineCamera.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_target == null) return;

            transform.position = Vector3.Lerp(transform.position, _target.position, _followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, _target.rotation, _rotationSpeed * Time.deltaTime);
        }
    }
}
