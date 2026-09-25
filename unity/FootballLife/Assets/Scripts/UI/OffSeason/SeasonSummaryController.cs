using System;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.OffSeason
{
    /// <summary>
    /// Controller for SeasonSummaryView.uxml (#P2-018).
    /// Presents annual campaign achievements, individual statistics, and financial recap.
    /// </summary>
    public class SeasonSummaryController
    {
        private readonly VisualElement _root;
        private readonly Action _onViewGrowth;
        private readonly Action _onReturnToHub;

        // UI Labels
        private readonly Label _labelBadge;
        private readonly Label _labelFinish;
        private readonly Label _labelClubDiv;
        private readonly Label _labelTrophy;
        private readonly Label _labelRating;
        private readonly Label _valApps;
        private readonly Label _valGoals;
        private readonly Label _valAssists;
        private readonly Label _labelHonor;
        private readonly Label _valWages;
        private readonly Label _valExpenses;
        private readonly Label _valNet;

        // Buttons
        private readonly Button _btnGrowth;
        private readonly Button _btnHub;

        public SeasonSummaryController(
            VisualElement root,
            Action onViewGrowth,
            Action onReturnToHub)
        {
            _root = root;
            _onViewGrowth = onViewGrowth;
            _onReturnToHub = onReturnToHub;

            _labelBadge    = root.Q<Label>("label-season-badge");
            _labelFinish   = root.Q<Label>("label-season-finish");
            _labelClubDiv  = root.Q<Label>("label-season-club-div");
            _labelTrophy   = root.Q<Label>("label-season-trophy");
            _labelRating   = root.Q<Label>("label-season-rating");
            _valApps       = root.Q<Label>("val-season-apps");
            _valGoals      = root.Q<Label>("val-season-goals");
            _valAssists    = root.Q<Label>("val-season-assists");
            _labelHonor    = root.Q<Label>("label-season-honor");
            _valWages      = root.Q<Label>("val-season-wages");
            _valExpenses   = root.Q<Label>("val-season-expenses");
            _valNet        = root.Q<Label>("val-season-net");

            _btnGrowth = root.Q<Button>("btn-summary-growth");
            _btnHub    = root.Q<Button>("btn-summary-hub");

            if (_btnGrowth != null)
                _btnGrowth.clicked += () => _onViewGrowth?.Invoke();

            if (_btnHub != null)
                _btnHub.clicked += () => _onReturnToHub?.Invoke();
        }

        public void Bind(SeasonSummarySnapshot summary, CareerSaveData? save)
        {
            if (_labelBadge != null)
                _labelBadge.text = $"SEASON {summary.Season} RECAP 🏆";

            if (_labelFinish != null)
                _labelFinish.text = summary.LeaguePosition;

            if (_labelClubDiv != null)
                _labelClubDiv.text = $"{summary.ClubName} · {summary.Division}";

            if (_labelTrophy != null)
                _labelTrophy.text = summary.TrophyAchievement;

            if (_labelRating != null)
                _labelRating.text = $"{summary.AverageRating:F2} ★";

            if (_valApps != null)
                _valApps.text = summary.TotalAppearances.ToString();

            if (_valGoals != null)
                _valGoals.text = summary.TotalGoals.ToString();

            if (_valAssists != null)
                _valAssists.text = summary.TotalAssists.ToString();

            if (_labelHonor != null)
            {
                if (summary.TotalGoals >= 15)
                    _labelHonor.text = "🏅 Award: Golden Boot Winner & Fans' Player of the Year";
                else if (summary.TotalGoals >= 8)
                    _labelHonor.text = "🏅 Award: Club Top Goalscorer Award";
                else
                    _labelHonor.text = "🏅 Recognition: Breakthrough Prospect of the Season";
            }

            if (_valWages != null)
                _valWages.text = $"£{summary.TotalWagesEarned:N0}";

            if (_valExpenses != null)
                _valExpenses.text = $"-£{summary.TotalExpenses:N0}";

            if (_valNet != null)
                _valNet.text = $"+£{summary.NetSavings:N0}";
        }
    }
}
