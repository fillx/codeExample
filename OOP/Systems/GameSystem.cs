using System;
using System.Globalization;
using _Game.Scripts.Balance;
using _Game.Scripts.Components.Loader;
using _Game.Scripts.Enums;
using _Game.Scripts.Factories;
using _Game.Scripts.Interfaces;
using _Game.Scripts.Systems.Boosts;
using _Game.Scripts.Systems.Save;
using _Game.Scripts.Tools;
using _Game.Scripts.Ui;
using _Game.Scripts.Ui.Cards;
using UnityEngine;
using Zenject;

namespace _Game.Scripts.Systems.Base
{
    public class GameSystem : IGameParam, IGameProgress, ITickableSystem
    {
        public static event Action<int> SoundEvent = delegate { };
        public Action<float> OnIncomeChanged;
        public Action OnResetLocalTimers;
        
        private int _lastVisitedDayOfYear;
        
        [Inject] private GameBalanceConfigs _balance;
        [Inject] private GameParamFactory _params;
        [Inject] private GameProgressFactory _progresses;
        [Inject] private RoomsFactory _rooms;
        [Inject] private WindowsSystem _windows;
        [Inject] private ConnectionSystem _connection;
        [Inject] private CardsFactory _cards;
        [Inject] private SaveSystem _save;
        [Inject] private CalculationComponent _calculation;
        [Inject] private AppEventProvider _eventProvider;

        private readonly LoadingSystem _loader;
        private readonly LevelSystem _levels;
        private readonly BoostSystem _boosts;

        private GameParam _mapId;
        private GameParam _soft;
        private GameParam _hard;
        private GameParam _tokens;
        private GameParam _tickets;
        private GameParam _workers;
        private GameParam _researchers;
        private GameProgress _starterPackTimer;
        
        private float _income;

        private const int DAILY_HOURS = 4;

        public float Income()
        {
            if (_income == 0)
            {
                _income = _rooms.GetIncome();
            }
            return _income;
        }

        public int MapId => (int) (_mapId?.Value ?? 1);
        public DateTime DailyRestartTime { get; private set; }
        public float ToNextDayTime => (float) (DailyRestartTime - _connection.ServerTime).TotalSeconds;
        
        private enum GameState 
        {
            Loading,
            Play,
            Pause
        }

        private GameState _state;

        public bool GameIsLoading => _state is GameState.Loading;

        public bool GamePaused => _state is GameState.Pause or GameState.Loading;

        public GameSystem(LoadingSystem loader, LevelSystem levels, BoostSystem boosts)
        {
            _loader = loader;
            _loader.OnLoadedGame += OnLoadedGame;
            _loader.OnResumeGame += ResumeGame;
            
            _levels = levels;
            _levels.OnLoadedLevel += OnLoadedLevel;

            _boosts = boosts;
            _boosts.BoostStateChangedEvent += OnBoostChanged;
            
            _state = GameState.Loading;
        }

        public void PauseGame()
        {
            if(_state == GameState.Loading) return;
            Time.timeScale = 0f;
            _state = GameState.Pause;
            _save.SaveAll();
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
            _state = GameState.Play;
        }

        private void OnBoostChanged(Boost boost)
        {
            if (!boost.Config.Global && boost.Config.ParamType != GameParamType.Income) return;
            OnUpdateIncome();
        }

        public void Init()
        {
            var income = _progresses.CreateProgress(this, 
                GameParamType.Income, 
                _balance.DefaultBalance.IncomeDelay);
            income.CompletedEvent += OnGetIncome;
            income.Play();

            _progresses.CreateProgress(this, GameParamType.StarterPackTimer,
                _balance.DefaultBalance.StarterPackDuration, false,true, true);
            _progresses.CreateProgress(this, GameParamType.BoxStoreTimer, 1f, false,true, true);
            _progresses.CreateProgress(this, GameParamType.CrystalsStoreTimer, 1f, false,true, true);
            _progresses.CreateProgress(this, GameParamType.ParkingClickableCash, 
                0, 
                false, 
                false,
                true);
            
            _params.CreateParam(this, GameParamType.VideoBoostCounter, 0, true);
            _params.CreateParam(this, GameParamType.DailyRewardCurrentDay, 1, true);
            _params.CreateParam(this, GameParamType.DailyRewardReceived, 1, true);
            _params.CreateParam(this, GameParamType.DailyQuestsRewardReceived, 0, true);
            _params.CreateParam(this, GameParamType.DailyQuestsStarsReceived, 0, true);
            _params.CreateParam(this, GameParamType.DailyQuestsRewardCounter, 0, true);
            _params.CreateParam(this, GameParamType.MaxOfflineTime, _balance.DefaultBalance.MaxOfflineTime, true);
            _params.CreateParam(this, GameParamType.WatchedAdCounter, 0, true);
            _mapId = _params.CreateParam(this, GameParamType.Level, 1, true);
            _soft = _params.CreateParam(this, GameParamType.Soft, _balance.DefaultBalance.StartSoft, true);
            _hard = _params.CreateParam(this, GameParamType.Hard, 0, true);
            _tokens = _params.CreateParam(this, GameParamType.Tokens, 0, true);
            _workers = _params.CreateParam(this, GameParamType.Workers, 0, true);
            _researchers = _params.CreateParam(this, GameParamType.Researchers, 1, true);
            _tickets = _params.CreateParam(this, GameParamType.AdTickets, 0, true);
            
            _params.CreateParam(this, GameParamType.MaxWorkers, 1, true);
            _params.CreateParam(this, GameParamType.MaxResearchers, 1, true);
            var corpseProgress = _params.CreateParam(this, GameParamType.Corpses, 0, true);
            corpseProgress.UpdatedEvent += OnCorpseBuried;
            _params.CreateParam(this, GameParamType.PurchaseCount, 0, true);
        }
        
        private void OnLoadedGame()
        {
            _loader.OnLoadedGame -= OnLoadedGame;
            _state = GameState.Play;
            Time.timeScale = 1f;
            
            OnUpdateIncome();
            _rooms.ItemUpgradeEvent += OnUpdateIncome;
            
            SetCurrentDay();
            
            var soft = _params.GetParamValue<GameSystem>(GameParamType.Soft);
            var hard = _params.GetParamValue<GameSystem>(GameParamType.Hard);
            _eventProvider.TriggerEvent(AppEventType.Analytics, GameEvents.Launch, (int)hard, (int)soft);
            _eventProvider.TriggerEvent(AppEventType.Tasks, GameEvents.Launch);

            _windows.OpenWindow<MainWindow>();
            _windows.OpenWindow<CurrencyWindow>();
            
            _workers.SetValue(_progresses.GetActiveBuilderCount());
        }

        private void OnLoadedLevel()
        {
            _mapId.SetValue(_levels.CurrentLevel.Id);
            _workers.SetValue(0);
            OnUpdateIncome();
        }

        private void OnCorpseBuried()
        {
            var progress = _params.GetParam<GameSystem>(GameParamType.Corpses);
            _eventProvider.TriggerEvent(AppEventType.Tasks, GameEvents.BuryCorpses);
            _eventProvider.TriggerEvent(AppEventType.Tasks, GameEvents.ReachBuriedCorpses, (int)progress.Value);

        }
        
        private void OnUpdateIncome()
        {
            _income = _rooms.GetIncome();
            _progresses.GetProgress(this, GameParamType.ParkingClickableCash)?.SetTarget(_calculation.GetIncomeByTime(_balance.DefaultBalance.ParkingMeterLimit));
            OnIncomeChanged?.Invoke(_income);
        }

        private void OnGetIncome()
        {
            if(_income == 0) return;
            var income = Mathf.Round(_income / 30);
            AddCurrency(GameParamType.Soft, income, GetCurrencyPlace.None);
        }
        
        public void AddCurrency(GameParamType type, float value, GetCurrencyPlace place)
        {
            switch (type)
            {
                case GameParamType.Soft:
                    _soft.Change(value);
                    switch (place)
                    {
                        case GetCurrencyPlace.Store:
                        case GetCurrencyPlace.AdOffer:
                        case GetCurrencyPlace.StarterPack:
                        case GetCurrencyPlace.CharacterCash:
                        case GetCurrencyPlace.CompleteCorpseOrder:
                        case GetCurrencyPlace.BoxStore:
                        case GetCurrencyPlace.TimeSkipStore:
                        case GetCurrencyPlace.Visitor:
                            SendAddCurrency("soft", type, place, value, (int)_soft.Value);
                            break;
                        case GetCurrencyPlace.None:
                            return;
                    }
                    break;
                
                case GameParamType.Hard:
                    _hard.Change(value);
                    if (place != GetCurrencyPlace.None) SendAddCurrency("hard", type, place, (int) value, (int)_hard.Value);
                    else ; // just breakpoint
                    break;
                
                case GameParamType.Tokens:
                    _tokens.Change(value);
                    _eventProvider.TriggerEvent(AppEventType.Tasks, GameEvents.GotTokens, value);
                    if (place != GetCurrencyPlace.None) SendAddCurrency("tokens", type, place, (int)value, (int)_tokens.Value);
                    break;
            }
            
            if(_state == GameState.Play) _save.SaveAll();
        }

        private void SendAddCurrency(string currencyType, GameParamType currencyName, GetCurrencyPlace place, float value,
            float total)
        {
            var formattedValue = value.ToString(CultureInfo.CurrentCulture);
            if (value > 1000f) formattedValue = value.ToFormattedString();
            _eventProvider.TriggerEvent(AppEventType.Analytics,
                GameEvents.CurrencyGet,
                currencyType,
                currencyName,
                place,
                value,
                (int)total);
        }

        public bool IsEnoughCurrency(GameParamType type, float needed)
        {
            switch (type)
            {
                case GameParamType.Soft:
                    return _soft.Value >= needed;

                case GameParamType.Hard:
                    return _hard.Value >= needed;
                
                case GameParamType.Tokens:
                    return _tokens.Value >= needed;
                
                case GameParamType.Workers:
                    return _params.GetParamValue<GameSystem>(GameParamType.Workers) <
                           _params.GetParamValue<GameSystem>(GameParamType.MaxWorkers);
            }

            return false;
        }

        public void SpendCurrency(GameParamType type, float value, SpendCurrencyPlace place, SpendCurrencyItem item, string placement = "")
        {
            switch (type)
            {
                case GameParamType.Soft:
                    _soft.Change(- Mathf.Min(value, _soft.Value));
                    if (place != SpendCurrencyPlace.None && item != SpendCurrencyItem.None && item != SpendCurrencyItem.RoomItem)
                    {
                        SendSpendCurrency("soft", type, place, item,(int) value, (int)_soft.Value, placement);
                    }
                    break;
                
                case GameParamType.Hard:
                    _hard.Change(-value);
                    if (place != SpendCurrencyPlace.None && item != SpendCurrencyItem.None)
                    {
                        SendSpendCurrency("hard", type, place, item, (int)value, (int)_hard.Value, placement);
                    }
                    break;
                
                case GameParamType.Tokens:
                    _tokens.Change(-value);
                    if (place != SpendCurrencyPlace.None && item != SpendCurrencyItem.None)
                    {
                        SendSpendCurrency("soft", type, place, item, (int)value, (int)_tokens.Value, placement);
                    }
                    break;
                
                case GameParamType.AdTickets:
                    _tickets.Change(-value);
                    break;
            }
            _eventProvider.TriggerEvent(AppEventType.Tasks, GameEvents.CurrencySpend, value);
            
            _save.SaveAll(true);
        }

        public void UpdateCurrency(GameParamType paramType)
        {
            switch (paramType)
            {
                case GameParamType.Workers:
                    _params.GetParam<GameSystem>(GameParamType.Workers).SetValue(_progresses.GetActiveBuilderCount());
                    break;
            }
            
            _save.SaveAll();
        }
        
        private void SendSpendCurrency(string currencyType, 
            GameParamType currencyName, 
            SpendCurrencyPlace place, 
            SpendCurrencyItem item, 
            int value, 
            int total,
            string placement)
        {
            var text = placement != "" ? placement : place.ToString(); 
            _eventProvider.TriggerEvent(AppEventType.Analytics, 
                GameEvents.CurrencySpend, 
                currencyType, 
                currencyName, 
                text,
                item,
                value,
                total);
        }

        public void Tick(float deltaTime)
        {
            CheckLocalTimers();
        }

        private void CheckLocalTimers()
        {
            var time = _connection.ServerTime;
            if (time.DayOfYear == _lastVisitedDayOfYear || time.Hour != DAILY_HOURS) return;
            UpdateCurrentDay();
            ResetLocalTimers();
            _lastVisitedDayOfYear = time.DayOfYear;
        }

        private void SetCurrentDay()
        {
            if (DailyRestartTime != DateTime.MinValue) return;
            var time = _connection.ServerTime;
            var newDay = _connection.ServerTime;
            if (time.Hour >= DAILY_HOURS && _lastVisitedDayOfYear != newDay.DayOfYear)
            {
                _lastVisitedDayOfYear = newDay.DayOfYear;
                ResetLocalTimers();
            }
            newDay = newDay.AddDays(1);
            DailyRestartTime = new DateTime(newDay.Year, newDay.Month, newDay.Day, DAILY_HOURS, 0, 0);
        }
        
        private void UpdateCurrentDay()
        {
            var newDay = _connection.ServerTime.AddDays(1);
            DailyRestartTime = new DateTime(newDay.Year, newDay.Month, newDay.Day, DAILY_HOURS, 0, 0);
        }
        
        public void ResetLocalTimers()
        {
            OnResetLocalTimers?.Invoke();
        }
        
        public void ClaimReward(GameParamType type, double amount, GetCurrencyPlace place, int id = 0)
        {
            switch (type)
            {
                case GameParamType.Soft:
                    AddCurrency(GameParamType.Soft, (float)amount, place);
                    break;
				
                case GameParamType.Hard:
                    AddCurrency(GameParamType.Hard, (float)amount, place);
                    break;
				
                case GameParamType.Tokens:
                    AddCurrency(GameParamType.Tokens, (float)amount, place);
                    break;
				
                case GameParamType.Card:
                    _cards.AddCard(id, (int)amount);
                    break;
                
                case GameParamType.Box:
                    var items = _cards.GetBoxItems(id, 1, 1);
                    if (items.Length > 0)
                    {
                        _windows.OpenWindow<CaseOpeningWindow>(items);
                        foreach (var item in items)
                        {
                            ClaimReward(item.Type, item.Amount, GetCurrencyPlace.Store, item.RewardId);
                        }
                    }
                    break;
                
                case GameParamType.AdTickets:
                    _tickets.Change((int)amount);
                    break;
            }
            
            _save.SaveAll();
        }

        public void SetGameParams(DateTime date)
        {
            if (date.Hour < DAILY_HOURS || date.DayOfYear > _connection.ServerTime.DayOfYear)
            {
                DailyRestartTime = date;
            }
            _loader.ResumeGame();
        }
    }
}