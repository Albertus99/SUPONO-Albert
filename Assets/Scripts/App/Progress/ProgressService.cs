using System;
using System.Collections.Generic;
using Supono.App.Characters;
using Supono.App.Saving;
using UnityEngine;

namespace Supono.App.Progress
{
    /// <summary>
    /// Player progress: coins, unlocked/completed levels, owned/selected characters and the daily streak.
    /// Persisted as the "progress" section of the save; every change is stored immediately.
    /// </summary>
    public sealed class ProgressService
    {
        const string Section = "progress";

        [Serializable]
        sealed class Data
        {
            public int coins;
            public int unlockedLevels = 1;
            public List<int> completedLevels = new();
            public List<string> ownedCharacters = new();
            public string selectedCharacter;
            public string lastDailyClaim;
            public int dailyStreak;
        }

        readonly ISaveService saves;
        readonly CharacterCatalog characters;
        Data data;

        public event Action Changed;

        public ProgressService(ISaveService saves, CharacterCatalog characters)
        {
            this.saves = saves;
            this.characters = characters;
            Load();
        }

        public int Coins => data.coins;
        public int UnlockedLevels => data.unlockedLevels;
        public string SelectedCharacterId => data.selectedCharacter;
        public string LastDailyClaim => data.lastDailyClaim;
        public int DailyStreak => data.dailyStreak;

        public void ClaimDailyReward(string date, int streakDay, int coins)
        {
            data.lastDailyClaim = date;
            data.dailyStreak = streakDay;
            data.coins += coins;
            Save();
        }

        public bool IsLevelUnlocked(int index) => index < data.unlockedLevels;
        public bool IsLevelCompleted(int index) => data.completedLevels.Contains(index);

        public void CompleteLevel(int index)
        {
            if (!data.completedLevels.Contains(index)) data.completedLevels.Add(index);
            data.unlockedLevels = Mathf.Max(data.unlockedLevels, index + 2);
            Save();
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            data.coins += amount;
            Save();
        }

        public bool Owns(string characterId) => data.ownedCharacters.Contains(characterId);

        /// <summary>Unlocks (if needed and affordable at <paramref name="price"/>) and selects the character.</summary>
        public bool TryUnlock(PlayableCharacter character, int price)
        {
            if (Owns(character.id)) return true;
            if (data.coins < price) return false;
            data.coins -= price;
            data.ownedCharacters.Add(character.id);
            data.selectedCharacter = character.id;
            Save();
            return true;
        }

        public void Select(string characterId)
        {
            if (!Owns(characterId) || data.selectedCharacter == characterId) return;
            data.selectedCharacter = characterId;
            Save();
        }

        public void Reset()
        {
            saves.Store(Section, new Data());
            Load();
            Changed?.Invoke();
        }

        void Load()
        {
            data = saves.Load<Data>(Section);

            // Repair anything a partial save left empty.
            data.completedLevels ??= new List<int>();
            data.ownedCharacters ??= new List<string>();
            data.unlockedLevels = Mathf.Max(1, data.unlockedLevels);
            data.coins = Mathf.Max(0, data.coins);

            // Free characters are always owned; make sure something valid is selected.
            foreach (PlayableCharacter character in characters.characters)
            {
                if (character.price <= 0 && !data.ownedCharacters.Contains(character.id)) data.ownedCharacters.Add(character.id);
            }
            if (string.IsNullOrEmpty(data.selectedCharacter) || !data.ownedCharacters.Contains(data.selectedCharacter))
                data.selectedCharacter = characters.Default != null ? characters.Default.id : null;
        }

        /// <summary>Every change is written immediately, so quitting or crashing never loses progress.</summary>
        void Save()
        {
            saves.Store(Section, data);
            Changed?.Invoke();
        }
    }
}
