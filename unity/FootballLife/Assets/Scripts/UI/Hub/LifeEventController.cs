using System;
using System.Collections.Generic;
using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Hub
{
    /// <summary>
    /// Binds LifeEventView.uxml — populates category, character badge, title, description,
    /// and dynamic choice cards with consequence preview chips.
    /// Executes choice consequences directly via SimulationBridge.
    /// </summary>
    public class LifeEventController
    {
        private readonly Label _labelEventCategory;
        private readonly Label _labelCharacterBadge;
        private readonly Label _labelEventTitle;
        private readonly Label _labelEventDescription;
        private readonly Label _labelEventIcon;
        private readonly VisualElement _containerChoices;
        private readonly VisualElement _panelFeedback;
        private readonly Label _labelFeedbackText;

        private Action? _onDismiss;
        private LifeEventSnapshot? _currentSnapshot;

        private static readonly Dictionary<string, string> _categoryIcons = new()
        {
            { "Lifestyle",    "🌟" },
            { "Media",        "📰" },
            { "Relationship", "💬" },
            { "Finance",      "💰" },
            { "Commercial",   "💼" },
            { "Social",       "🎉" },
            { "Club",         "⚽" },
            { "Injury",       "🏥" },
        };

        public LifeEventController(VisualElement root, Action? onDismiss)
        {
            _onDismiss = onDismiss;
            _labelEventCategory   = root.Q<Label>("label-event-category");
            _labelCharacterBadge  = root.Q<Label>("label-character-badge");
            _labelEventTitle      = root.Q<Label>("label-event-title");
            _labelEventDescription= root.Q<Label>("label-event-description");
            _labelEventIcon       = root.Q<Label>("label-event-icon");
            _containerChoices     = root.Q<VisualElement>("container-choices");
            _panelFeedback        = root.Q<VisualElement>("panel-event-feedback");
            _labelFeedbackText    = root.Q<Label>("label-event-feedback-text");
        }

        /// <summary>
        /// Populates the panel from a LifeEventSnapshot.
        /// </summary>
        public void Show(LifeEventSnapshot snap)
        {
            _currentSnapshot = snap;

            if (_labelEventCategory != null)
                _labelEventCategory.text = snap.Category.ToUpperInvariant();

            if (_labelCharacterBadge != null)
                _labelCharacterBadge.text = !string.IsNullOrWhiteSpace(snap.CharacterBadge) ? snap.CharacterBadge : "📋 Club Liaison";

            if (_labelEventTitle != null)
                _labelEventTitle.text = snap.Title;

            if (_labelEventDescription != null)
                _labelEventDescription.text = snap.Description;

            if (_labelEventIcon != null)
                _labelEventIcon.text = _categoryIcons.TryGetValue(snap.Category, out var icon) ? icon : "📋";

            if (_panelFeedback != null)
                _panelFeedback.style.display = DisplayStyle.None;

            if (_containerChoices == null) return;

            _containerChoices.Clear();

            // Dynamically generate an interactive card per choice
            for (int i = 0; i < snap.Choices.Count; i++)
            {
                int choiceIndex = i;
                string choiceText = snap.Choices[i];
                string impactEffect = (snap.ChoiceEffects != null && i < snap.ChoiceEffects.Count)
                    ? snap.ChoiceEffects[i]
                    : string.Empty;

                var cardBtn = new Button(() => OnChoiceSelected(snap, choiceIndex, choiceText, impactEffect));
                cardBtn.AddToClassList("card-interactive");
                cardBtn.style.paddingTop = 10;
                cardBtn.style.paddingBottom = 10;
                cardBtn.style.paddingLeft = 14;
                cardBtn.style.paddingRight = 14;
                cardBtn.style.borderTopLeftRadius = 10;
                cardBtn.style.borderTopRightRadius = 10;
                cardBtn.style.borderBottomLeftRadius = 10;
                cardBtn.style.borderBottomRightRadius = 10;
                cardBtn.style.backgroundColor = new Color(0.12f, 0.16f, 0.24f, 0.9f);
                cardBtn.style.borderTopWidth = 1;
                cardBtn.style.borderBottomWidth = 1;
                cardBtn.style.borderLeftWidth = 1;
                cardBtn.style.borderRightWidth = 1;
                cardBtn.style.borderTopColor = new Color(0.2f, 0.27f, 0.38f, 0.6f);
                cardBtn.style.borderBottomColor = new Color(0.2f, 0.27f, 0.38f, 0.6f);
                cardBtn.style.borderLeftColor = new Color(0.2f, 0.27f, 0.38f, 0.6f);
                cardBtn.style.borderRightColor = new Color(0.2f, 0.27f, 0.38f, 0.6f);
                cardBtn.style.alignItems = Align.FlexStart;

                var labelChoice = new Label(choiceText);
                labelChoice.style.fontSize = 13;
                labelChoice.style.unityFontStyleAndWeight = FontStyle.Bold;
                labelChoice.style.color = new Color(0.97f, 0.98f, 0.99f);
                labelChoice.style.marginBottom = 4;
                cardBtn.Add(labelChoice);

                if (!string.IsNullOrWhiteSpace(impactEffect))
                {
                    var labelImpact = new Label(impactEffect);
                    labelImpact.style.fontSize = 11;
                    labelImpact.style.color = new Color(0.0f, 0.9f, 1.0f);
                    cardBtn.Add(labelImpact);
                }

                _containerChoices.Add(cardBtn);
            }
        }

        private void OnChoiceSelected(LifeEventSnapshot snap, int choiceIndex, string choiceText, string impactText)
        {
            // Parse deltas from impact text heuristic
            int moneyDelta = 0;
            int moraleDelta = 0;
            int trustDelta = 0;
            int energyDelta = 0;

            if (impactText.Contains("£3,500")) moneyDelta = 3500;
            else if (impactText.Contains("£1,500")) moneyDelta = 1500;
            else if (impactText.Contains("-£15,000")) moneyDelta = -15000;

            if (impactText.Contains("+15 Morale")) moraleDelta = 15;
            else if (impactText.Contains("+12 Morale")) moraleDelta = 12;
            else if (impactText.Contains("+8 Morale")) moraleDelta = 8;
            else if (impactText.Contains("+6 Morale")) moraleDelta = 6;
            else if (impactText.Contains("+4 Morale")) moraleDelta = 4;
            else if (impactText.Contains("-5 Morale")) moraleDelta = -5;

            if (impactText.Contains("+15 Manager Trust")) trustDelta = 15;
            else if (impactText.Contains("+12 Manager Trust")) trustDelta = 12;
            else if (impactText.Contains("+10 Manager Trust")) trustDelta = 10;
            else if (impactText.Contains("+8 Trust") || impactText.Contains("+8 Manager Trust")) trustDelta = 8;
            else if (impactText.Contains("+6 Manager Trust")) trustDelta = 6;
            else if (impactText.Contains("-8 Trust") || impactText.Contains("-8 Manager Trust")) trustDelta = -8;
            else if (impactText.Contains("-12 Manager Trust")) trustDelta = -12;
            else if (impactText.Contains("-15 Manager Trust")) trustDelta = -15;

            if (impactText.Contains("+12 Energy")) energyDelta = 12;
            else if (impactText.Contains("+5 Energy")) energyDelta = 5;
            else if (impactText.Contains("-20 Energy")) energyDelta = -20;
            else if (impactText.Contains("-6 Energy")) energyDelta = -6;
            else if (impactText.Contains("-4 Energy")) energyDelta = -4;

            SimulationBridge.Instance?.ResolveLifeEventChoice(
                snap,
                choiceIndex,
                choiceText,
                moneyDelta,
                moraleDelta,
                trustDelta,
                energyDelta);

            if (_panelFeedback != null && _labelFeedbackText != null)
            {
                _panelFeedback.style.display = DisplayStyle.Flex;
                _labelFeedbackText.text = $"✓ Decision Recorded: {choiceText}";
            }

            // Dismiss after feedback
            _containerChoices.SetEnabled(false);
            _onDismiss?.Invoke();
        }
    }
}
