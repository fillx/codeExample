namespace _Game.Scripts.Interfaces
{
    public interface ITickableSystem
    {
        void Tick(float deltaTime);
    }
    public interface IFixedTickableSystem
    {
        void Tick(float deltaTime);
    }
}