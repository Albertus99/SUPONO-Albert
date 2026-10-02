using UnityEngine;

namespace Supono.Debugging
{
    /// <summary>
    /// One switchable debug feature. Off is always the normal game. Subclasses that need a side effect
    /// (shader globals, audio...) apply it in <see cref="OnChanged"/>; plain flags are read by the debug
    /// rule overrides in this module.
    /// </summary>
    public class DebugToggle
    {
        public DebugToggle(string id, string label, string description)
        {
            Id = id;
            Label = label;
            Description = description;
        }

        public string Id { get; }
        public string Label { get; }
        public string Description { get; }
        public bool IsOn { get; private set; }

        internal void Set(bool on)
        {
            IsOn = on;
            OnChanged(on);
        }

        protected virtual void OnChanged(bool on) { }
    }

    /// <summary>Hides the ink outline via a global the outline shader reads.</summary>
    public sealed class OutlinesOffToggle : DebugToggle
    {
        static readonly int DisabledId = Shader.PropertyToID("_ToonOutlineDisabled");

        public OutlinesOffToggle() : base("outlines-off", "Outlines Off", "Hide the toon ink outline.") { }

        protected override void OnChanged(bool on) => Shader.SetGlobalFloat(DisabledId, on ? 1f : 0f);
    }

    /// <summary>Silences all audio, music included.</summary>
    public sealed class MuteToggle : DebugToggle
    {
        public MuteToggle() : base("mute", "Mute", "Silence all sounds and music.") { }

        protected override void OnChanged(bool on) => AudioListener.volume = on ? 0f : 1f;
    }
}
