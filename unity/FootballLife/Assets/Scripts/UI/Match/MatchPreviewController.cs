#nullable enable
using System;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Match
{
    /// <summary>
    /// Presentation controller for MatchPreviewView.uxml (#P2-015).
    /// Displays opponent details, competition context, player squad role, and tactical briefing.
    /// </summary>
    public class MatchPreviewController
    {
        private readonly Label _labelCompetition;
        private readonly Label _labelVenueBadge;
        private readonly Label _labelHomeInitial;
        private readonly Label _labelHomeClub;
        private readonly Label _labelAwayInitial;
        private readonly Label _labelAwayClub;

        private readonly Label _labelRoleBadge;
        private readonly Label _labelPlayerPosition;
        private readonly Label _labelPlayerEnergy;
        private readonly Label _labelPlayerForm;

        private readonly Label _labelTacticalObjective;
        private readonly Label _labelTacticalBriefing;

        private readonly Button _btnKickoff;
        private readonly Button _btnPreviewBack;
        private readonly Button _btnPreviewCancel;

        private readonly Action _onKickoff;
        private readonly Action _onCancel;

        public MatchPreviewController(
            VisualElement root,
            Action onKickoff,
            Action onCancel)
        {
            _onKickoff = onKickoff;
            _onCancel = onCancel;

            _labelCompetition     = root.Q<Label>("label-competition");
            _labelVenueBadge       = root.Q<Label>("label-venue-badge");
            _labelHomeInitial     = root.Q<Label>("label-home-initial");
            _labelHomeClub        = root.Q<Label>("label-home-club");
            _labelAwayInitial     = root.Q<Label>("label-away-initial");
            _labelAwayClub        = root.Q<Label>("label-away-club");

            _labelRoleBadge       = root.Q<Label>("label-role-badge");
            _labelPlayerPosition  = root.Q<Label>("label-player-position");
            _labelPlayerEnergy    = root.Q<Label>("label-player-energy");
            _labelPlayerForm      = root.Q<Label>("label-player-form");

            _labelTacticalObjective = root.Q<Label>("label-tactical-objective");
            _labelTacticalBriefing  = root.Q<Label>("label-tactical-briefing");

            _btnKickoff       = root.Q<Button>("btn-kickoff");
            _btnPreviewBack   = root.Q<Button>("btn-preview-back");
            _btnPreviewCancel = root.Q<Button>("btn-preview-cancel");

            if (_btnKickoff != null)
                _btnKickoff.clicked += () => _onKickoff?.Invoke();

            if (_btnPreviewBack != null)
                _btnPreviewBack.clicked += () => _onCancel?.Invoke();

            if (_btnPreviewCancel != null)
                _btnPreviewCancel.clicked += () => _onCancel?.Invoke();
        }

        public void Bind(MatchOpportunitySnapshot? opp, CareerSaveData? save)
        {
            string homeClub = save != null && !string.IsNullOrEmpty(save.ClubName) ? save.ClubName : "Northfield Town";
            string awayClub = opp.HasValue ? opp.Value.OpponentName : "Westford United";
            string comp = opp.HasValue ? opp.Value.Competition : "Division 4";
            bool isHome = !opp.HasValue || opp.Value.IsHome;

            if (_labelCompetition != null)
                _labelCompetition.text = $"{comp.ToUpperInvariant()} · MATCHDAY FIXTURE";

            if (_labelVenueBadge != null)
                _labelVenueBadge.text = isHome ? "HOME MATCH" : "AWAY FIXTURE";

            if (_labelHomeClub != null)
                _labelHomeClub.text = (isHome ? homeClub : awayClub).ToUpperInvariant();

            if (_labelHomeInitial != null)
                _labelHomeInitial.text = (isHome ? homeClub : awayClub).Substring(0, 1).ToUpperInvariant();

            if (_labelAwayClub != null)
                _labelAwayClub.text = (isHome ? awayClub : homeClub).ToUpperInvariant();

            if (_labelAwayInitial != null)
                _labelAwayInitial.text = (isHome ? awayClub : homeClub).Substring(0, 1).ToUpperInvariant();

            // Player role & condition
            if (save != null)
            {
                if (_labelRoleBadge != null)
                    _labelRoleBadge.text = string.IsNullOrEmpty(save.SquadRole) ? "FIRST TEAM STARTER" : save.SquadRole.ToUpperInvariant();

                if (_labelPlayerPosition != null)
                    _labelPlayerPosition.text = $"{save.PrimaryPosition} · {save.PreferredFoot} Foot";

                if (_labelPlayerEnergy != null)
                    _labelPlayerEnergy.text = $"{save.Energy}% Energy";

                if (_labelPlayerForm != null)
                    _labelPlayerForm.text = $"{save.Form}% (Match Sharp)";
            }

            // Tactical briefing
            int targetDiff = opp.HasValue ? opp.Value.TargetScoreDiff : 1;
            if (_labelTacticalObjective != null)
                _labelTacticalObjective.text = $"Target: Secure 3 points with a positive goal difference (+{targetDiff}).";

            if (_labelTacticalBriefing != null)
                _labelTacticalBriefing.text = $"{awayClub} play an expansive style with a high defensive line. Attack the half-spaces, make incisive runs in behind, and test their goalkeeper early.";
        }
    }
}
