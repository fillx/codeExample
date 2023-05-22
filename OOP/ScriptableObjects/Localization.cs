using System;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Balance.Attributes;
using _Game.Scripts.Tools;
using UnityEngine;
using Zenject;

namespace _Game.Scripts.ScriptableObjects
{
    /// <summary>
    /// Используется для локализации
    /// </summary>
    [CreateAssetMenu(fileName = "Localization", menuName = "_Game/Localization", order = 0)]
    public class Localization : ScriptableObjectInstaller, IParsable
    {
        [SerializeField] private List<LocalizationData> _localizationData;
        private List<LocalizationData> _expData, _tutorialData;
        private Language _language;

        private static Localization _instance;
        public static Localization Instance
        {
            get
            {
                if (_instance == null) _instance = Resources.Load<Localization>(nameof(Localization));
                return _instance;
            }
        }
        
        public void Initialize()
        {
            _language = Language.En;
            // _englishDictionary = new();
            // foreach (var item in _localizationData)
            // {
            //     if(_englishDictionary.ContainsKey(item.Token)) continue;
            //     _englishDictionary.Add(item.Token, item.Translations[1]);
            // }
        }
        
        public string Get(string token)
        {
            var data = _localizationData.Find(d => d.Token == token);
            return data != null ? data.Translations[(int)_language] : token;
        }

        public void OnParsed()
        {
            var allLanguages = Enum.GetValues(typeof(Language))
                .Cast<int>()
                .ToList();

            allLanguages.Remove((int)Language.None);
            var maxIndex = allLanguages.LastValue();

            var tokens = new List<string>();
            foreach (var localizationData in _localizationData)
            {
                var token = localizationData.Token = localizationData.Token.ToUpper();
                if (tokens.Contains(token))
                {
                    Debug.LogWarning($"\"{token}\" token already exists in localization");
                    continue;
                }

                tokens.Add(token);

                var translations = localizationData.Translations ??
                                   (localizationData.Translations = new List<string>(maxIndex));

                while (translations.Count != maxIndex + 1)
                {
                    translations.Add(string.Empty);
                }

                if (localizationData.Token == "empty") continue;

                foreach (var languageIndex in allLanguages)
                {
                    if (translations[languageIndex] == string.Empty)
                    {
                        translations[languageIndex] = localizationData.Token;
                    }
                }
            }
			
            _localizationData.AddRange(_expData);
            _localizationData.AddRange(_tutorialData);
        }
    }

    [Serializable]
    public class LocalizationData
    {
        public string Token;
        [RepetitiveTableValues(2)] public List<string> Translations;
    }
    
    public enum Language
    {
        None = -1,
        Ru = 0,
        En = 1
    }
}