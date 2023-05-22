using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Game.Components.CarComponents
{
    [Game, Event(EventTarget.Self)]
    public class DetailIconComponent : IComponent
    {
        public Sprite value;
    }
}