namespace Core.Interfaces
{
    public interface IHittable
    {
        void TakeDamage(int amount);
        void Die();
    }
}
