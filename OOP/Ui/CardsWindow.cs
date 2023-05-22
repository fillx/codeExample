using System;
using System.Linq;
using _Game.Scripts.Enums;
using _Game.Scripts.Factories;
using _Game.Scripts.ScriptableObjects;
using _Game.Scripts.Systems.Base;
using _Game.Scripts.Tools;
using _Game.Scripts.Ui.Base;
using _Game.Scripts.Ui.Offers;
using TMPro;
using UnityEngine;
using Zenject;

namespace _Game.Scripts.Ui.Cards
{
    public class CardsWindow : BaseWindow
    {
        public class RewardConfig
        {
            public GameParamType Type;
            public double Amount;
            public int RewardId;
            public bool NewCard;
        }
        
        [SerializeField] private TextMeshProUGUI _title;
        [SerializeField] private BaseButton _hireMoreButton;
        [SerializeField] private RectTransform[] _containers;
        [SerializeField] private TextMeshProUGUI[] _containerTitles;
        
        [Inject] private WindowsSystem _windows;
        [Inject] private CardsFactory _cards;

        public override void Init()
        {
            _hireMoreButton.SetCallback(OnPressedHireMore);
            for (int i = 0; i < Enum.GetNames(typeof(CardType)).Length; i++)
            {
                var id = i;
                var cards = _cards.All.Where(c => c.Config.CardType == (CardType)id);
                foreach (var cardUI in cards)
                {
                    if (_containers.Length <= id)
                    {
                        cardUI.Deactivate();
                        break;
                    }
                    
                    cardUI.Activate();
                    cardUI.transform.SetParent(_containers[id]);
                }
            }
            base.Init();
        }

        private void OnPressedHireMore()
        {
            Close();
            _windows.OpenWindow<StoreWindow>();
        }

        public override void UpdateLocalization()
        {
            _title.text = "CARDS_WINDOW_TITLE".ToLocalized();

            var names = Enum.GetNames(typeof(CardType));
            for (int i = 0; i < names.Length; i++)
            {
                if (_containerTitles.Length <= i) break;
                _containerTitles[i].text = names[i].ToUpper().ToLocalized();
            }
            
            base.UpdateLocalization();
        }

        public override void Open(params object[] list)
        {
            foreach (var card in _cards.All)
            {
               card.Redraw();
            }

            base.Open(list);
        }

        public override void Close()
        {
            base.Close();
        }
    }
}