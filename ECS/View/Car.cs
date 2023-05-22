using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Views
{
    public class Car : View
    {
        [field: SerializeField] public DiagnosticPoint DiagnosticPoint { get; private set; }
        [field: SerializeField] public Detail[] Details {get; private set;}
        [FoldoutGroup("Navigation")]
        [SerializeField] private NavMeshPathfinderAgent navMeshPathfinderAgent;
        [FoldoutGroup("Navigation")]
        [SerializeField] private float speed = 2f;
        [FoldoutGroup("Navigation")]
        [SerializeField] private float rotationSpeed = 10f;

        public override void Link(GameEntity entity)
        {
            base.Link(entity);
            GameEntity.isCar = true;
            GameEntity.AddStateObjectType(StateObjectType.Car);
            GameEntity.AddSpeed(speed);
            GameEntity.AddRotateSpeed(rotationSpeed);
            navMeshPathfinderAgent.Link(entity);
            var diagnosticPointEntity = Contexts.sharedInstance.game.CreateEntity();
            diagnosticPointEntity.AddParentCarHashcode(GameEntity.hashCode.value);
            DiagnosticPoint.Link(diagnosticPointEntity);
            var detailIndex = 0;
            foreach (var detail in Details)
            {
                var gameEntity = Contexts.sharedInstance.game.CreateEntity();
                gameEntity.AddParentCarHashcode(this.GameEntity.hashCode.value);
                gameEntity.AddDetailIndex(detailIndex++);
                detail.Link(gameEntity);
            }
        }
        

        private void OnDestroy()
        {
            // for (int i = 0; i < Details.Length; i++)
            // {
            //     Details[i].GameEntity.Destroy();
            // }
        }
    }
}