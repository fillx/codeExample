using _Game.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace _Game.Scripts.View.Character
{
    public sealed class CharacterView : CharacterViewBase
    {
        [SerializeField] private bool _isPool;
        public bool IsPool => _isPool;
        
        public class Pool : MonoMemoryPool<CharacterView>
        {
            protected override void Reinitialize(CharacterView character)
            {
                character.Reset();
            }
        }

        protected override void Reset()
        {
            transform.position = Vector3.zero;
            GetComponent<NavMeshAgent>().agentTypeID = 0;
            ShowVisual();
            AnimateHandsDown();
            base.Reset();
        }

        public void SetConfig(CharacterConfig config)
        {
            Config = config;
        }

        public override void OnDestroyLevel()
        {
            GetComponent<NavMeshAgent>().agentTypeID = 0;
            base.OnDestroyLevel();
        }
    }
}
