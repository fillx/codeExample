using System.Collections.Generic;
using _Game.Scripts.Ui.ResourceBubbles;
using _Game.Scripts.View.Other;
using UnityEngine;

namespace _Game.Scripts.Core
{
    public class SceneData : MonoBehaviour
    {
        [SerializeField] private GameCamera _camera;
        
        [Header("UI")]
        [SerializeField] private Transform _ui;
        [SerializeField] private Transform _windowsOverlay;
        [SerializeField] private WorldSpaceCanvas _worldSpaceCanvas;
        [SerializeField] private List<ResourceBubbleSetup> _resourceBubbleSetups;

        [Header("Settings")]
        [SerializeField] private Vector3 _itemFocusOffset;

        public GameCamera Camera => _camera;
        public Transform UI => _ui;
        public Transform WindowsOverlay => _windowsOverlay;
        public Vector3 ItemFocusOffset => _itemFocusOffset;
        public List<ResourceBubbleSetup> ResourceBubbleSetups => _resourceBubbleSetups;

        public Transform WorldSpaceCanvas => _worldSpaceCanvas.transform;
    }
}