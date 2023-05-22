using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Game.Components.CarComponents
{
    [Game, Event(EventTarget.Self)]
    public class ProcessComponent : IComponent
    {
        public float value;
    }
}