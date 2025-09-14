using UnityEngine;

namespace Core.Interfaces
{
    public interface IMovable
    {
        void Move(Vector2 direction, float deltaTime);
        void Jump();
        bool TryJump();
        void Dash(Vector2 direction);
        void UpdateGrounded(bool isGrounded);
        bool IsGrounded();
        Vector2 GetVelocity();
        void SetVelocity(Vector2 velocity);
        float GetDashDuration();
    }
}
