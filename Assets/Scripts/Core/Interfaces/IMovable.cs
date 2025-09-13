using UnityEngine;

namespace Core.Interfaces
{
    public interface IMovable
    {
        void Move(Vector2 direction, float deltaTime);
        void Jump();
        void Dash(Vector2 direction);
        void UpdateGrounded(bool isGrounded);
        bool IsGrounded();
        Vector2 GetVelocity();
        void SetVelocity(Vector2 velocity);
        void SetExtraJumps(int extra);
    }
}
