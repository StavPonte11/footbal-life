#nullable enable
using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Media
{
    /// <summary>
    /// UI Toolkit presenter for PressConferenceView.uxml (#P4-009).
    /// Orchestrates question presentation, interactive response selection, and consequence execution.
    /// </summary>
    public class PressConferenceController
    {
        private readonly VisualElement _root;
        private readonly Action _onFinish;

        // UI Labels
        private readonly Label? _labelTitle;
        private readonly Label? _labelAvatar;
        private readonly Label? _labelName;
        private readonly Label? _labelOutlet;
        private readonly Label? _labelContext;
        private readonly Label? _labelQuestion;

        // Containers
        private readonly VisualElement? _containerChoices;
        private readonly VisualElement? _boxConclusion;
        private readonly Label? _labelConclusionSummary;
        private readonly Button? _btnFinish;

        private PressConference? _currentConference;
        private int _currentQuestionIndex = 0;
        private readonly List<string> _responseHistory = new();

        public PressConferenceController(VisualElement root, Action onFinish)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _onFinish = onFinish;

            _labelTitle = _root.Q<Label>("lbl-press-title");
            _labelAvatar = _root.Q<Label>("lbl-journalist-avatar");
            _labelName = _root.Q<Label>("lbl-journalist-name");
            _labelOutlet = _root.Q<Label>("lbl-journalist-outlet");
            _labelContext = _root.Q<Label>("lbl-press-context");
            _labelQuestion = _root.Q<Label>("lbl-press-question");

            _containerChoices = _root.Q<VisualElement>("container-press-choices");
            _boxConclusion = _root.Q<VisualElement>("box-press-conclusion");
            _labelConclusionSummary = _root.Q<Label>("lbl-conclusion-summary");
            _btnFinish = _root.Q<Button>("btn-press-finish");

            if (_btnFinish != null)
                _btnFinish.clicked += OnFinishClicked;
        }

        private static void SetBorderRadius(VisualElement el, float radius)
        {
            el.style.borderTopLeftRadius = radius;
            el.style.borderTopRightRadius = radius;
            el.style.borderBottomLeftRadius = radius;
            el.style.borderBottomRightRadius = radius;
        }

        public void StartConference(PressConference conference)
        {
            _currentConference = conference ?? throw new ArgumentNullException(nameof(conference));
            _currentQuestionIndex = 0;
            _responseHistory.Clear();

            if (_labelTitle != null)
                _labelTitle.text = _currentConference.Title;

            if (_boxConclusion != null)
                _boxConclusion.style.display = DisplayStyle.None;

            if (_containerChoices != null)
                _containerChoices.style.display = DisplayStyle.Flex;

            ShowCurrentQuestion();
        }

        private void ShowCurrentQuestion()
        {
            if (_currentConference == null || _currentConference.Questions == null || _currentConference.Questions.Count == 0)
            {
                FinishConference();
                return;
            }

            if (_currentQuestionIndex >= _currentConference.Questions.Count)
            {
                FinishConference();
                return;
            }

            var q = _currentConference.Questions[_currentQuestionIndex];

            if (_labelAvatar != null) _labelAvatar.text = q.Journalist?.AvatarEmoji ?? "🎙️";
            if (_labelName != null) _labelName.text = q.Journalist?.Name ?? "Journalist";
            if (_labelOutlet != null) _labelOutlet.text = q.Journalist?.Outlet ?? "Press";
            if (_labelContext != null) _labelContext.text = $"Question {_currentQuestionIndex + 1} of {_currentConference.Questions.Count} — {q.Category}";
            if (_labelQuestion != null) _labelQuestion.text = q.QuestionText;

            if (_containerChoices == null) return;
            _containerChoices.Clear();

            foreach (var resp in q.Responses)
            {
                var card = CreateChoiceCard(q, resp);
                _containerChoices.Add(card);
            }
        }

        private VisualElement CreateChoiceCard(PressQuestion question, PressResponseChoice choice)
        {
            var btn = new Button();
            btn.AddToClassList("card-interactive");
            btn.style.width = new Length(100, LengthUnit.Percent);
            btn.style.paddingTop = 10;
            btn.style.paddingBottom = 10;
            btn.style.paddingLeft = 14;
            btn.style.paddingRight = 14;
            btn.style.marginBottom = 8;
            SetBorderRadius(btn, 10);
            btn.style.backgroundColor = new Color(0.12f, 0.16f, 0.24f, 0.95f);
            btn.style.borderTopWidth = 1;
            btn.style.borderBottomWidth = 1;
            btn.style.borderLeftWidth = 1;
            btn.style.borderRightWidth = 1;
            btn.style.borderTopColor = new Color(0.2f, 0.27f, 0.38f, 0.6f);
            btn.style.borderBottomColor = btn.style.borderTopColor;
            btn.style.borderLeftColor = btn.style.borderTopColor;
            btn.style.borderRightColor = btn.style.borderTopColor;
            btn.style.alignItems = Align.FlexStart;

            // Top row: Tone Badge + Consequence summary
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 4;

            var toneBadge = new Label(choice.Tone.ToString().ToUpperInvariant());
            toneBadge.AddToClassList("badge");
            toneBadge.style.fontSize = 9;
            toneBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
            toneBadge.style.paddingTop = 1;
            toneBadge.style.paddingBottom = 1;
            toneBadge.style.paddingLeft = 6;
            toneBadge.style.paddingRight = 6;
            toneBadge.style.marginRight = 8;

            switch (choice.Tone)
            {
                case PressTone.Humble:
                    toneBadge.style.backgroundColor = new Color(0.06f, 0.73f, 0.5f, 0.3f);
                    toneBadge.style.color = new Color(0.2f, 0.83f, 0.6f);
                    break;
                case PressTone.Confident:
                    toneBadge.style.backgroundColor = new Color(0.0f, 0.6f, 0.9f, 0.3f);
                    toneBadge.style.color = new Color(0.0f, 0.9f, 1.0f);
                    break;
                case PressTone.Defiant:
                    toneBadge.style.backgroundColor = new Color(0.94f, 0.27f, 0.27f, 0.3f);
                    toneBadge.style.color = new Color(0.97f, 0.44f, 0.44f);
                    break;
                case PressTone.Diplomatic:
                    toneBadge.style.backgroundColor = new Color(0.55f, 0.36f, 0.96f, 0.3f);
                    toneBadge.style.color = new Color(0.65f, 0.5f, 1.0f);
                    break;
            }

            headerRow.Add(toneBadge);

            // Add impact chips
            if (choice.ManagerTrustDelta != 0)
            {
                var chip = new Label(choice.ManagerTrustDelta > 0 ? $"+{choice.ManagerTrustDelta:0} Trust" : $"{choice.ManagerTrustDelta:0} Trust");
                chip.style.fontSize = 9;
                chip.style.unityFontStyleAndWeight = FontStyle.Bold;
                chip.style.color = choice.ManagerTrustDelta > 0 ? new Color(0.2f, 0.83f, 0.6f) : new Color(0.97f, 0.44f, 0.44f);
                chip.style.marginRight = 6;
                headerRow.Add(chip);
            }

            if (choice.FanPopularityDelta != 0)
            {
                var chip = new Label(choice.FanPopularityDelta > 0 ? $"+{choice.FanPopularityDelta:0} Fame" : $"{choice.FanPopularityDelta:0} Fame");
                chip.style.fontSize = 9;
                chip.style.unityFontStyleAndWeight = FontStyle.Bold;
                chip.style.color = choice.FanPopularityDelta > 0 ? new Color(0.96f, 0.75f, 0.14f) : new Color(0.97f, 0.44f, 0.44f);
                chip.style.marginRight = 6;
                headerRow.Add(chip);
            }

            btn.Add(headerRow);

            // Choice Text
            var labelText = new Label($"\"{choice.Text}\"");
            labelText.style.fontSize = 12;
            labelText.style.color = new Color(0.95f, 0.96f, 0.98f);
            labelText.style.whiteSpace = WhiteSpace.Normal;
            labelText.style.unityFontStyleAndWeight = FontStyle.Normal;
            btn.Add(labelText);

            btn.clicked += () => OnChoiceSelected(question, choice);
            return btn;
        }

        private void OnChoiceSelected(PressQuestion question, PressResponseChoice choice)
        {
            var bridge = SimulationBridge.Instance;
            if (bridge != null)
            {
                bridge.AnswerPressQuestion(question, choice);
            }

            _responseHistory.Add($"[{choice.Tone}] {choice.ConsequenceSummary}");
            _currentQuestionIndex++;

            if (_currentQuestionIndex < (_currentConference?.Questions.Count ?? 0))
            {
                ShowCurrentQuestion();
            }
            else
            {
                FinishConference();
            }
        }

        private void FinishConference()
        {
            if (_containerChoices != null)
                _containerChoices.style.display = DisplayStyle.None;

            if (_boxConclusion != null)
                _boxConclusion.style.display = DisplayStyle.Flex;

            if (_labelConclusionSummary != null)
            {
                string summary = string.Join("\n• ", _responseHistory);
                _labelConclusionSummary.text = string.IsNullOrWhiteSpace(summary)
                    ? "Your statements were broadcast live to supporters and pundits worldwide."
                    : $"Media Verdict:\n• {summary}";
            }
        }

        private void OnFinishClicked()
        {
            _onFinish?.Invoke();
        }
    }
}
