using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Game.Components.CarComponents
{
    [Game]
    public class ParentCarHashcodeComponent : IComponent
    {
        [EntityIndex] public int value;
    }
}