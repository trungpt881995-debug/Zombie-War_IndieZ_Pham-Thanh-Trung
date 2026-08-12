using UnityEngine;
using ZombieWar.Features.Zombie.Domain;
using ZombieWar.Features.Zombie.Ports;

namespace ZombieWar.Features.Zombie.Unity.Movement
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class CharacterControllerZombieMotor : MonoBehaviour, IZombieMotor
    {
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedStickVelocity = -2f;
        private CharacterController _controller;
        private bool _enabled = true;
        private float _verticalVelocity;
        private float _normalizedSpeed;

        public ZombiePoint Position
        {
            get
            {
                Vector3 p = transform.position;
                return new ZombiePoint(p.x, p.y, p.z);
            }
        }
        public float NormalizedSpeed => _normalizedSpeed;

        private void Awake() => _controller = GetComponent<CharacterController>();

        public void Warp(in ZombiePoint position)
        {
            bool wasEnabled = _controller.enabled;
            _controller.enabled = false;
            transform.position = new Vector3(position.X, position.Y, position.Z);
            _controller.enabled = wasEnabled;
            _verticalVelocity = 0f;
            _normalizedSpeed = 0f;
        }
        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!enabled) Stop();
        }
        public void MoveTowards(in ZombiePoint target, float speed, float deltaTime)
        {
            if (!_enabled || deltaTime <= 0f) { Stop(); return; }
            Vector3 current = transform.position;
            Vector3 delta = new Vector3(target.X - current.x, 0f, target.Z - current.z);
            float sqr = delta.sqrMagnitude;
            Vector3 horizontal = Vector3.zero;
            if (sqr > 0.000001f && speed > 0f)
            {
                horizontal = delta.normalized * speed;
                _normalizedSpeed = 1f;
            }
            else _normalizedSpeed = 0f;

            if (_controller.isGrounded && _verticalVelocity < 0f) _verticalVelocity = groundedStickVelocity;
            else _verticalVelocity += gravity * deltaTime;
            Vector3 velocity = horizontal;
            velocity.y = _verticalVelocity;
            _controller.Move(velocity * deltaTime);
        }
        public void Stop() { _normalizedSpeed = 0f; }
    }
}
