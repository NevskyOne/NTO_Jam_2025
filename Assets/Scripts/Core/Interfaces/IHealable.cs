namespace Core.Interfaces
{
    public interface IHealable
    {
        void Heal(float amount);
        float GetCurrentHealth();
        float GetMaxHealth();
    }
}
