#nullable enable
using System;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Match
{
    /// <summary>
    /// Presentation and logic controller for MatchPostView.uxml (#P2-017).
    /// Displays match outcome, player rating, stats, manager locker room reaction,
    /// and invokes SimulationBridge.RecordMatchResult to persist career progression.
    /// </summary>
    public class MatchPostController
    {
        private readonly Label _labelResultTag;
        private readonly Label _labelHomeTeam;
        private readonly Label _labelAwayTeam;
        private readonly Label _labelHomeScore;
        private readonly Label _labelAwayScore;
        private readonly Label _labelCompetition;

        private readonly Label _labelRating;
        private readonly Label _valGoals;
        private readonly Label _valAssists;
        private readonly Label _valKeyActions;
        private readonly Label _valErrors;

        private readonly Label _labelManagerQuote;
        private readonly Label _valTrustDelta;
        private readonly Label _valFormDelta;
        private readonly Label _valEnergyCost;

        private readonly Button _btnReturnHub;
        private readonly Action _onReturnToHub;

        public MatchPostController(
            VisualElement root,
            Action onReturnToHub)
        {
            _onReturnToHub = onReturnToHub;

            _labelResultTag  = root.Q<Label>("label-post-result-tag");
            _labelHomeTeam   = root.Q<Label>("label-post-home-team");
            _labelAwayTeam   = root.Q<Label>("label-post-away-team");
            _labelHomeScore  = root.Q<Label>("label-post-home-score");
            _labelAwayScore  = root.Q<Label>("label-post-away-score");
            _labelCompetition = root.Q<Label>("label-post-competition");

            _labelRating     = root.Q<Label>("label-post-rating");
            _valGoals        = root.Q<Label>("val-post-goals");
            _valAssists      = root.Q<Label>("val-post-assists");
            _valKeyActions   = root.Q<Label>("val-post-keyactions");
            _valErrors       = root.Q<Label>("val-post-errors");

            _labelManagerQuote = root.Q<Label>("label-post-manager-quote");
            _valTrustDelta     = root.Q<Label>("val-post-trust-delta");
            _valFormDelta      = root.Q<Label>("val-post-form-delta");
            _valEnergyCost     = root.Q<Label>("val-post-energy-cost");

            _btnReturnHub = root.Q<Button>("btn-return-hub");

            if (_btnReturnHub != null)
                _btnReturnHub.clicked += () => _onReturnToHub?.Invoke();
        }

        public void Bind(MatchSummaryData summary, CareerSaveData? save, SimulationBridge? bridge)
        {
            if (_labelHomeTeam != null) _labelHomeTeam.text = summary.HomeClub;
            if (_labelAwayTeam != null) _labelAwayTeam.text = summary.AwayClub;
            if (_labelHomeScore != null) _labelHomeScore.text = summary.HomeScore.ToString();
            if (_labelAwayScore != null) _labelAwayScore.text = summary.AwayScore.ToString();
            if (_labelCompetition != null) _labelCompetition.text = $"{summary.Competition} · League Fixture";

            bool won = summary.HomeScore > summary.AwayScore;
            bool draw = summary.HomeScore == summary.AwayScore;

            if (_labelResultTag != null)
            {
                _labelResultTag.RemoveFromClassList("badge-green");
                _labelResultTag.RemoveFromClassList("badge-gold");
                _labelResultTag.RemoveFromClassList("badge-slate");
                _labelResultTag.AddToClassList("badge");

                if (won)
                {
                    _labelResultTag.text = "VICTORY 🏆";
                    _labelResultTag.AddToClassList("badge-green");
                }
                else if (draw)
                {
                    _labelResultTag.text = "DRAW ⚖️";
                    _labelResultTag.AddToClassList("badge-gold");
                }
                else
                {
                    _labelResultTag.text = "DEFEAT";
                    _labelResultTag.AddToClassList("badge-slate");
                }
            }

            // Rating & Stats
            if (_labelRating != null)
                _labelRating.text = $"{summary.MatchRating:F2} ★";

            if (_valGoals != null) _valGoals.text = summary.PlayerGoals.ToString();
            if (_valAssists != null) _valAssists.text = summary.PlayerAssists.ToString();
            if (_valKeyActions != null) _valKeyActions.text = summary.KeyActions.ToString();
            if (_valErrors != null) _valErrors.text = summary.Errors.ToString();

            // Manager Reaction & Deltas
            int trustDelta;
            int formDelta;
            string managerQuote;

            if (won && summary.MatchRating >= 7.00)
            {
                trustDelta = 8;
                formDelta = 5;
                managerQuote = "\"Outstanding performance today! You created chances, finished clinically, and worked relentlessly for the team.\"";
            }
            else if (won)
            {
                trustDelta = 5;
                formDelta = 3;
                managerQuote = "\"Good team win. You played your role responsibly and helped secure the three points.\"";
            }
            else if (draw)
            {
                trustDelta = 2;
                formDelta = 1;
                managerQuote = "\"A tough, evenly matched contest. We take the point and move forward to the next fixture.\"";
            }
            else
            {
                trustDelta = -3;
                formDelta = -2;
                managerQuote = "\"Disappointing result. We need sharper decision-making and higher intensity in training this week.\"";
            }

            int currentTrust = save != null ? save.ManagerTrust : 50;
            int currentForm = save != null ? save.Form : 70;
            int currentEnergy = save != null ? save.Energy : 100;

            int newTrust = Math.Clamp(currentTrust + trustDelta, 0, 100);
            int newForm = Math.Clamp(currentForm + formDelta, 0, 100);
            int newEnergy = Math.Max(10, currentEnergy - 25);

            if (_labelManagerQuote != null)
                _labelManagerQuote.text = managerQuote;

            if (_valTrustDelta != null)
                _valTrustDelta.text = $"{(trustDelta >= 0 ? "+" : "")}{trustDelta} Trust (Now {newTrust} / 100)";

            if (_valFormDelta != null)
                _valFormDelta.text = $"{(formDelta >= 0 ? "+" : "")}{formDelta} Form (Now {newForm}%)";

            if (_valEnergyCost != null)
                _valEnergyCost.text = $"-25 Energy (Now {newEnergy}%)";

            // Persist to simulation
            bridge?.RecordMatchResult(
                playerGoals: summary.PlayerGoals,
                playerAssists: summary.PlayerAssists,
                matchRating: summary.MatchRating,
                homeScore: summary.HomeScore,
                awayScore: summary.AwayScore,
                managerTrustDelta: trustDelta,
                formDelta: formDelta,
                moraleDelta: won ? 5 : (draw ? 0 : -3),
                energyCost: 25
            );
        }
    }
}
