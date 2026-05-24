using UnityEngine;

namespace ProjectBoid.Helper
{
    public class Rotator : MonoBehaviour
    {
        [Header("Rotation Axis")] [SerializeField] private bool _rotateX;
        [SerializeField] private bool _rotateY = true;
        [SerializeField] private bool _rotateZ;

        [Header("Settings")] [SerializeField] private float _rotationSpeed = 50f;

        private void Update()
        {
            Vector3 rotationAxis = Vector3.zero;

            if (_rotateX)
            {
                rotationAxis.x = 1f;
            }

            if (_rotateY)
            {
                rotationAxis.y = 1f;
            }

            if (_rotateZ)
            {
                rotationAxis.z = 1f;
            }

            transform.Rotate(rotationAxis * (_rotationSpeed * Time.deltaTime));
        }
    }
}