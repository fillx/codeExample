using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Core;
using _Game.Scripts.Enums;
using _Game.Scripts.Systems;
using _Game.Scripts.Systems.Base;
using _Game.Scripts.Systems.Tutorial;
using _Game.Scripts.Tools;
using _Game.Scripts.Ui.Base.Animations;
using _Game.Scripts.Ui.Buttons;
using _Game.Scripts.Ui.WindowTabs;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Game.Scripts.Ui.Base
{
    public class BaseWindow : BaseUIView
    {
        private enum WindowState
        {
            Opened,
            Closed,
            PlayingAnim
        }
        
        private enum GuiAnimType
        {
            Open,
            Close
        }
        
        public static event Action<int> SoundEvent = delegate { };
        
        public Action<BaseWindow> Opened;
        public Action<BaseWindow> Closed;
     
        [SerializeField] private bool _addToOpenedStack = true;
        [SerializeField] private bool _toOpenedOverStack;
        [SerializeField] private bool _ignoreAnimations;
        [SerializeField] private bool _ignoreInitClose;
        [SerializeField] private bool _isVerticalClosing;
        [SerializeField] private bool _renderUiOnly;
        [SerializeField] private GameObject[] _openObjectsQueue;
        
        [Inject] protected SceneData SceneData;
        [Inject] protected GameFlags Flags;
        
        private const string INNER_WINDOW = "Window";

        private object[] _lastParametersList;
        
        private WindowState _state;

        private CloseWindowButton _closeButton;
        private WindowBack _windowBack;
        private List<BaseUIAnimation> _windowAnimations = new();
        protected WindowTabView[] Tabs;

        protected ScrollRect scrollRect;
        protected Vector2 StartScrollPos;
        protected bool IsDrag;
        protected Vector2 LastTouchPos;
        protected Vector2 StartDragTouchPos;
        protected Vector2 OpenedPosition;
        protected RectTransform WindowRect { get; private set; }

        public object[] LastParametersList => _lastParametersList;
        public bool AddToOpenedStack => _addToOpenedStack;
        public bool ToOpenOverStack => _toOpenedOverStack;
        public bool IsOpened => _state == WindowState.Opened;
        public bool IsClosed => _state == WindowState.Closed;
        public BaseButton CloseButton => _closeButton;
        public bool RenderUiOnly => _renderUiOnly;

        public virtual void Init()
        {
            OpenedPosition = RectTransform.anchoredPosition;
            _state = WindowState.Closed;
            
            _closeButton = GetComponentInChildren<CloseWindowButton>(true);
            _windowBack = GetComponentInChildren<WindowBack>(true);

            if (!_ignoreAnimations)
            {
                _windowAnimations = GetComponentsInChildren<BaseUIAnimation>().ToList();
                if (_windowAnimations.Count == 0)
                {
                    var windowsAnimation = SceneData.GetComponentInChildren<ScaleGuiAnim>();
                    var defaultAnimation = gameObject.AddComponent<ScaleGuiAnim>().CopyFrom(windowsAnimation);
                    _windowAnimations.Add(defaultAnimation);
                }

                foreach (var windowAnimation in _windowAnimations)
                {
                    windowAnimation.Init();
                }
            }

            scrollRect = GetComponentInChildren<ScrollRect>(true);

            foreach (Transform child in transform)
            {
                if (child.name != INNER_WINDOW) continue;
                WindowRect = child as RectTransform;
                OpenedPosition = WindowRect ? WindowRect.anchoredPosition : RectTransform.anchoredPosition;
                break;
            }
            
            if (_closeButton != null) _closeButton.SetCallback(Close);
            if (_windowBack != null && _closeButton != null) _windowBack.Init(_closeButton);
            if (!_ignoreInitClose) this.Deactivate();

            Tabs = GetComponentsInChildren<WindowTabView>(true);
            foreach (var tab in Tabs)
            {
                tab.Init();
            }
            if(Tabs.Length > 0) SelectTab(Tabs[0]);
            foreach (var item in _openObjectsQueue)
            {
                item.Deactivate();
            }
        }

        public WindowTabType SelectedTab()
        {
            if(Tabs == null || Tabs.Length == 0) return WindowTabType.None;
            var tab = Tabs.FirstOrDefault(i => i.IsSelected);
            return tab ? tab.Type : WindowTabType.None;
        }

        public void SetParameters(params object[] list)
        {
            _lastParametersList = list;
        }


        public virtual void Open(params object[] list)
        {
            _lastParametersList = null;

            if (WindowRect) WindowRect.anchoredPosition = OpenedPosition;
            else RectTransform.anchoredPosition = OpenedPosition;
            SetState(WindowState.Opened);
            if(Tabs.Length > 0) SelectTab(Tabs[0]);
            if (_windowAnimations.Count == 0)
            {
                this.Activate();
                OnOpened();
            }
            else
            {
                this.Activate();
                SetState(WindowState.PlayingAnim);
                foreach (var windowAnim in _windowAnimations)
                {
                    windowAnim.PlayOpenAnimation(WindowRect, OnOpened);   
                }
            }
            if(this is not MainWindow && 
               this is not ReturnToGameWindow && 
               this is not CurrencyWindow) PlaySound(GameSoundType.Window);

            for (int i = 0; i < _openObjectsQueue.Length; i++)
            {
                var i1 = i;
                InvokeSystem.NextFrame(() => _openObjectsQueue[i1].Activate(), i + 1);
            }
        }

        public virtual void SelectTab(WindowTabView selected)
        {
            if(Tabs.Length == 0) return;
            if(!Tabs.Contains(selected)) return;

            foreach (var tab in Tabs)
            {
                tab.Select(tab == selected);
            }
        }

        private void OnOpened()
        {
            Opened?.Invoke(this);
        }

        public virtual void Close()
        {
            if(this is not MainWindow && this is not CurrencyWindow && !IsClosed) PlaySound(GameSoundType.Window);
            if (_windowAnimations.Count == 0)
            {
                OnClosed();
            }
            else
            {
                SetState(WindowState.PlayingAnim);
                foreach (var windowAnim in _windowAnimations)
                {
                    windowAnim.PlayCloseAnimation(WindowRect, OnClosed);   
                }
            }
        }

        private void OnClosed()
        {
            this.Deactivate();
            
            foreach (var item in _openObjectsQueue)
            {
                item.Deactivate();
            }
            SetState(WindowState.Closed);
            Closed?.Invoke(this);
        }

        private void SetState(WindowState state)
        {
            _state = state;
        }

        public virtual void UpdateLocalization()
        {
        }

        public virtual void Tick(float deltaTime)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Close();
                return;
            }
            
            if(_isVerticalClosing) CheckVerticalDrag();
        }

        private void CheckVerticalDrag()
        {
            if(Flags.Has(GameFlag.TutorialRunning)) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                IsDrag = true;
                StartDragTouchPos = Input.mousePosition;
                StartScrollPos = scrollRect ? scrollRect.content.anchoredPosition : StartDragTouchPos;
            }

            if (!IsDrag)
            {
                WindowRect.anchoredPosition = Vector2.Lerp(WindowRect.anchoredPosition, OpenedPosition, 0.2f);
                return;
            }

            var currentPosition = OpenedPosition;
            var scrollPosition = scrollRect ? scrollRect.content.anchoredPosition : Vector2.zero;
            var scrollDelta = scrollRect ?  StartScrollPos.y - scrollPosition.y : 0f;
            var allowToDrag = scrollDelta < 1f || scrollPosition.y < 0f;
            
            if (Input.GetMouseButton(0))
            {
                LastTouchPos = Input.mousePosition;
                if (scrollPosition.y > 1f) StartDragTouchPos = LastTouchPos;
                if (allowToDrag && LastTouchPos.y - StartDragTouchPos.y < 1f)
                {
                    currentPosition.y -= StartDragTouchPos.y - LastTouchPos.y;
                    if(StartDragTouchPos.y - LastTouchPos.y > 100f && scrollRect) scrollRect.vertical = false;
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                if(scrollRect) scrollRect.vertical = true;
                IsDrag = false;
                LastTouchPos = Input.mousePosition;
                if (StartDragTouchPos.y - LastTouchPos.y > 100f && allowToDrag)
                {
                    Close();
                    return;
                }
            }

            WindowRect.anchoredPosition = currentPosition;
        }

        public void PlaySound(GameSoundType type)
        {
            SoundEvent?.Invoke((int)type);
        }

        public virtual BaseButton GetTutorialButton(int value = 0)
        {
            return null;
        }
    }
}