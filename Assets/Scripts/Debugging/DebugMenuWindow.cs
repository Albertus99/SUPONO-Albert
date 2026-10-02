using System;
using System.Collections.Generic;
using Supono.UI;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Supono.Debugging
{
    /// <summary>Scrollable list of debug toggles. Passive view: <see cref="DebugMenuExtension"/> applies changes and re-binds.</summary>
    public sealed class DebugMenuWindow : ChoiceWindow<bool>
    {
        [SerializeField] DebugToggleRow rowTemplate;
        [SerializeField] RectTransform rows;
        [SerializeField] Button resetButton;
        [SerializeField] Button backButton;

        readonly List<DebugToggleRow> spawned = new();
        IReadOnlyList<DebugToggle> toggles;
        IObjectResolver resolver;

        public event Action<DebugToggle, bool> ToggleChanged;
        public event Action ResetClicked;

        [Inject]
        public void Construct(IObjectResolver resolver) => this.resolver = resolver;

        protected override void Awake()
        {
            base.Awake();
            rowTemplate.gameObject.SetActive(false);
            resetButton.onClick.AddListener(() => ResetClicked?.Invoke());
            backButton.onClick.AddListener(() => Choose(true));
        }

        public void Setup(IReadOnlyList<DebugToggle> debugToggles)
        {
            toggles = debugToggles;
            while (spawned.Count < toggles.Count)
            {
                // Through the resolver so the row's components (e.g. ToggleSound) get injected.
                DebugToggleRow row = resolver.Instantiate(rowTemplate, rows);
                row.gameObject.SetActive(true);
                int index = spawned.Count;
                row.Changed += on => ToggleChanged?.Invoke(toggles[index], on);
                spawned.Add(row);
            }
            Refresh();
        }

        public void Refresh()
        {
            for (int i = 0; i < spawned.Count; i++) spawned[i].Bind(toggles[i]);
        }

        public override bool HandleBack()
        {
            Choose(true);
            return true;
        }
    }
}
