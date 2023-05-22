using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Game.Components.CarComponents
{
    [Game]
    public class DetailIndexComponent : IComponent
    {
        [EntityIndex]
        public int value;
    }
}