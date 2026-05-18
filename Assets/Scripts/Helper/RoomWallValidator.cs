using UnityEngine;

namespace ProjectBoid.Helper
{
    [ExecuteAlways]
    public class RoomWallValidator : MonoBehaviour
    {
        [Header("Assign 6 Cube Walls")] [SerializeField] private Transform[] _walls = new Transform[6];

        [Header("Inner Room Size")] [SerializeField] private Vector3 _roomSize = new Vector3(10f, 5f, 10f);

        [Header("Wall Thickness")] [SerializeField] private float _wallWidth = 0.25f;

        private void OnValidate()
        {
            if (_walls == null || _walls.Length < 6)
                return;

            float halfX = _roomSize.x * 0.5f;
            float halfY = _roomSize.y * 0.5f;
            float halfZ = _roomSize.z * 0.5f;

            float halfWall = _wallWidth * 0.5f;

            // FLOOR
            SetupWall(
                _walls[0],
                new Vector3(0f, -halfY - halfWall, 0f),
                new Vector3(_roomSize.x, _wallWidth, _roomSize.z)
            );

            // CEILING
            SetupWall(
                _walls[1],
                new Vector3(0f, halfY + halfWall, 0f),
                new Vector3(_roomSize.x, _wallWidth, _roomSize.z)
            );

            // LEFT
            SetupWall(
                _walls[2],
                new Vector3(-halfX - halfWall, 0f, 0f),
                new Vector3(_wallWidth, _roomSize.y + (_wallWidth * 2f), _roomSize.z)
            );

            // RIGHT
            SetupWall(
                _walls[3],
                new Vector3(halfX + halfWall, 0f, 0f),
                new Vector3(_wallWidth, _roomSize.y + (_wallWidth * 2f), _roomSize.z)
            );

            // FRONT
            SetupWall(
                _walls[4],
                new Vector3(0f, 0f, halfZ + halfWall),
                new Vector3(
                    _roomSize.x + (_wallWidth * 2f),
                    _roomSize.y + (_wallWidth * 2f),
                    _wallWidth
                )
            );

            // BACK
            SetupWall(
                _walls[5],
                new Vector3(0f, 0f, -halfZ - halfWall),
                new Vector3(
                    _roomSize.x + (_wallWidth * 2f),
                    _roomSize.y + (_wallWidth * 2f),
                    _wallWidth
                )
            );
        }

        private void SetupWall(Transform wall, Vector3 localPosition, Vector3 localScale)
        {
            if (wall == null)
                return;

            wall.localPosition = localPosition;
            wall.localRotation = Quaternion.identity;
            wall.localScale = localScale;
        }
    }
}