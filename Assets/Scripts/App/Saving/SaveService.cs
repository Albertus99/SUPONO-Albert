using System;
using System.Collections.Generic;
using UnityEngine;

namespace Supono.App.Saving
{
    /// <summary>
    /// The single point for everything persisted. Systems keep their own data class and store it as a named
    /// section; every <see cref="Store{T}"/> is written to disk immediately.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>The stored section, or a fresh instance if it doesn't exist or can't be read.</summary>
        T Load<T>(string section) where T : class, new();

        void Store<T>(string section, T data) where T : class;

        /// <summary>Wipes every section (a brand new save).</summary>
        void DeleteAll();
    }

    /// <summary>One JSON save file made of sections. Reads fall back to the backup when the main file is corrupt.</summary>
    public sealed class SaveService : ISaveService
    {
        [Serializable]
        sealed class SaveFile
        {
            public List<Section> sections = new();
        }

        [Serializable]
        sealed class Section
        {
            public string key;
            public string json;
        }

        readonly ISaveStorage storage;
        readonly Dictionary<string, string> sections = new();

        public SaveService(ISaveStorage storage)
        {
            this.storage = storage;
            ReadFile();
        }

        public T Load<T>(string section) where T : class, new()
        {
            if (!sections.TryGetValue(section, out string json) || string.IsNullOrWhiteSpace(json)) return new T();
            try
            {
                return JsonUtility.FromJson<T>(json) ?? new T();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Save] Section '{section}' is unreadable, starting it fresh: {exception.Message}");
                return new T();
            }
        }

        public void Store<T>(string section, T data) where T : class
        {
            sections[section] = JsonUtility.ToJson(data);
            WriteFile();
        }

        public void DeleteAll()
        {
            sections.Clear();
            storage.Delete();
        }

        void ReadFile()
        {
            SaveFile file = Parse(storage.Read(), "save");
            if (file == null)
            {
                file = Parse(storage.ReadBackup(), "backup");
                if (file != null) Debug.LogWarning("[Save] Main save was unreadable; restored from backup.");
            }
            if (file == null) return;
            foreach (Section section in file.sections) sections[section.key] = section.json;
        }

        void WriteFile()
        {
            var file = new SaveFile();
            foreach (KeyValuePair<string, string> pair in sections) file.sections.Add(new Section { key = pair.Key, json = pair.Value });
            storage.Write(JsonUtility.ToJson(file, prettyPrint: true));
        }

        static SaveFile Parse(string json, string source)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                return JsonUtility.FromJson<SaveFile>(json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Save] Could not read {source}: {exception.Message}");
                return null;
            }
        }
    }
}
