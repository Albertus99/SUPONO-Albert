using System;
using System.IO;
using UnityEngine;
using VContainer;

namespace Supono.App.Saving
{
    /// <summary>Raw persistence backend for the save file. Swap for cloud saves, tests, etc.</summary>
    public interface ISaveStorage
    {
        /// <summary>The current save, or empty if there is none.</summary>
        string Read();

        /// <summary>The previous good save, used when <see cref="Read"/> is unreadable.</summary>
        string ReadBackup();

        void Write(string contents);
        void Delete();
    }

    /// <summary>
    /// Save file in <see cref="Application.persistentDataPath"/>. Writes are atomic (temp file, then swap) and keep
    /// the previous save as a backup, so a crash or power loss mid-write can't destroy progress.
    /// </summary>
    public sealed class SaveFileStorage : ISaveStorage
    {
        readonly string path;
        readonly string backupPath;
        readonly string tempPath;

        [Inject] // the container uses persistentDataPath; the path overload is for tools and tests
        public SaveFileStorage() : this(Path.Combine(Application.persistentDataPath, "save.json")) { }

        public SaveFileStorage(string path)
        {
            this.path = path;
            backupPath = path + ".bak";
            tempPath = path + ".tmp";
        }

        public string FilePath => path;

        public string Read() => ReadFile(path);
        public string ReadBackup() => ReadFile(backupPath);

        public void Write(string contents)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(tempPath, contents);
                if (File.Exists(path)) File.Replace(tempPath, path, backupPath);
                else File.Move(tempPath, path);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Save] Failed to write {path}: {exception}");
            }
        }

        public void Delete()
        {
            foreach (string file in new[] { path, backupPath, tempPath })
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }

        static string ReadFile(string file)
        {
            if (!File.Exists(file)) return string.Empty;
            try
            {
                return File.ReadAllText(file);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Save] Failed to read {file}: {exception.Message}");
                return string.Empty;
            }
        }
    }
}
