using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Supono.Debugging
{
    public sealed class DebugToggleRow : MonoBehaviour
    {
        [SerializeField] TMP_Text label;
        [SerializeField] TMP_Text description;
        [SerializeField] Toggle toggle;

        public event Action<bool> Changed;

        void Awake() => toggle.onValueChanged.AddListener(on => Changed?.Invoke(on));

        public void Bind(DebugToggle debugToggle)
        {
            label.text = debugToggle.Label;
            description.text = debugToggle.Description;
            toggle.SetIsOnWithoutNotify(debugToggle.IsOn);
        }
    }
}
