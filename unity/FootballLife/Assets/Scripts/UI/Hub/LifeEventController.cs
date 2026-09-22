using System.Collections.Generic;
using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Hub
{
    /// <summary>
    /// Binds LifeEventView.uxml — populates title, description, and dynamic choice buttons
    /// from a LifeEventSnapshot. Choices are logged and the panel dismissed.
    /// </summary>
    public class LifeEventController
    {
        private readonly Label _labelEventCategory;
        private readonly Label _labelEventTitle;
        private readonly Label _labelEventDescription;
        private readonly Label _labelEventIcon;
        private readonly VisualElement _containerChoices;

        private System.Action? _onDismiss;

        // Map category to display icon
        private static readonly Dictionary<string, string> _categoryIcons = new()
        {
            { "Lifestyle",    "🌟" },
            { "Media",        "📰" },
            { "Relationship", "💬" },
            { "Finance",      "💰" },
            { "Transfer",     "✈️" },
            { "Injury",       "🏥" },
        };

        public LifeEventController(VisualElement root, System.Action? onDismiss)
        {
            _onDismiss = onDismiss;
            _labelEventCategory   = root.Q<Label>("label-event-category");
            _labelEventTitle      = root.Q<Label>("label-event-title");
            _labelEventDescription= root.Q<Label>("label-event-description");
            _labelEventIcon       = root.Q<Label>("label-event-icon");
            _containerChoices     = root.Q<VisualElement>("container-choices");
        }

        /// <summary>Populates the panel from a LifeEventSnapshot.</summary>
        public void Show(LifeEventSnapshot snap)
        {
            _labelEventCategory.text    = snap.Category.ToUpperInvariant();
            _labelEventTitle.text       = snap.Title;
            _labelEventDescription.text = snap.Description;
            _labelEventIcon.text        = _categoryIcons.TryGetValue(snap.Category, out var icon) ? icon : "📋";

            // Clear old choice buttons
            _containerChoices.Clear();

            // Dynamically create a button per choice
            foreach (string choice in snap.Choices)
            {
                // Capture local for closure
                string localChoice = choice;
                var btn = new Button(() => OnChoiceSelected(snap, localChoice))
                {
                    text = localChoice
                };
                btn.AddToClassList("btn");
                btn.AddToClassList("btn-secondary");
                btn.style.marginBottom = 8;
                btn.style.height = 44;
                _containerChoices.Add(btn);
            }
        }

        private void OnChoiceSelected(LifeEventSnapshot snap, string choice)
        {
            Debug.Log($"[LifeEvent] '{snap.Title}' — Player chose: '{choice}'");
            _onDismiss?.Invoke();
        }
    }
}
