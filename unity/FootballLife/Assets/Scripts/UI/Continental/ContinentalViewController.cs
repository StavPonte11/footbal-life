#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using FootballLife.Domain;
using FootballLife.Unity.Core.Bridge;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Continental
{
    /// <summary>
    /// Presentation controller for continental club tournament (Champions Cup) view (#P5-005).
    /// Displays group stage standings, knockout tournament tree, and prize money.
    /// </summary>
    public class ContinentalViewController
    {
        private readonly VisualElement _root;
        private readonly Button? _btnBack;
        private readonly Button? _tabGroupsAD;
        private readonly Button? _tabGroupsEH;
        private readonly Button? _tabKnockout;

        private readonly Label? _labelPrizeBadge;
        private readonly Label? _labelTournamentTitle;
        private readonly Label? _labelTournamentStatus;
        private readonly Label? _labelWinnerDisplay;

        private readonly VisualElement? _containerGroups;
        private readonly VisualElement? _containerKnockout;
        private readonly Button? _btnSimulateRound;

        private readonly Action _onBack;
        private int _currentTab = 0; // 0 = A-D, 1 = E-H, 2 = Knockout

        public ContinentalViewController(VisualElement root, Action onBack)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _onBack = onBack;

            _btnBack = root.Q<Button>("btn-back");
            _tabGroupsAD = root.Q<Button>("tab-groups-ad");
            _tabGroupsEH = root.Q<Button>("tab-groups-eh");
            _tabKnockout = root.Q<Button>("tab-knockout");

            _labelPrizeBadge = root.Q<Label>("label-prize-badge");
            _labelTournamentTitle = root.Q<Label>("label-tournament-title");
            _labelTournamentStatus = root.Q<Label>("label-tournament-status");
            _labelWinnerDisplay = root.Q<Label>("label-winner-display");

            _containerGroups = root.Q<VisualElement>("container-groups");
            _containerKnockout = root.Q<VisualElement>("container-knockout");
            _btnSimulateRound = root.Q<Button>("btn-simulate-round");

            if (_btnBack != null) _btnBack.clicked += () => _onBack?.Invoke();
            if (_tabGroupsAD != null) _tabGroupsAD.clicked += () => SwitchTab(0);
            if (_tabGroupsEH != null) _tabGroupsEH.clicked += () => SwitchTab(1);
            if (_tabKnockout != null) _tabKnockout.clicked += () => SwitchTab(2);

            if (_btnSimulateRound != null)
            {
                _btnSimulateRound.clicked += OnSimulateRoundClicked;
            }
        }

        public void Refresh()
        {
            var bridge = SimulationBridge.Instance;
            if (bridge == null) return;

            var comp = bridge.GetActiveContinentalCompetition();
            if (comp != null)
            {
                Bind(comp);
            }
            else
            {
                DisplaySampleData();
            }
        }

        public void Bind(ContinentalCompetition competition)
        {
            if (competition == null) return;

            if (_labelTournamentTitle != null)
                _labelTournamentTitle.text = competition.Name;

            if (_labelPrizeBadge != null)
                _labelPrizeBadge.text = $"£{competition.PrizeMoney / 1_000_000:N0}M Prize Pool";

            if (_labelTournamentStatus != null)
                _labelTournamentStatus.text = competition.IsCompleted ? "Completed" : "In Progress";

            if (_labelWinnerDisplay != null)
            {
                if (competition.IsCompleted && competition.WinnerId.HasValue)
                {
                    _labelWinnerDisplay.text = $"🏆 Tournament Winner Crowned!";
                }
                else
                {
                    _labelWinnerDisplay.text = "Top 2 from each group advance to Round of 16.";
                }
            }

            RenderGroups(competition.GroupStandings);
            RenderKnockout(competition.KnockoutFixtures);
        }

        private void SwitchTab(int tabIndex)
        {
            _currentTab = tabIndex;

            if (_tabGroupsAD != null)
            {
                if (tabIndex == 0) _tabGroupsAD.AddToClassList("tab-button-active");
                else _tabGroupsAD.RemoveFromClassList("tab-button-active");
            }

            if (_tabGroupsEH != null)
            {
                if (tabIndex == 1) _tabGroupsEH.AddToClassList("tab-button-active");
                else _tabGroupsEH.RemoveFromClassList("tab-button-active");
            }

            if (_tabKnockout != null)
            {
                if (tabIndex == 2) _tabKnockout.AddToClassList("tab-button-active");
                else _tabKnockout.RemoveFromClassList("tab-button-active");
            }

            if (tabIndex == 2)
            {
                if (_containerGroups != null) _containerGroups.style.display = DisplayStyle.None;
                if (_containerKnockout != null) _containerKnockout.style.display = DisplayStyle.Flex;
            }
            else
            {
                if (_containerGroups != null) _containerGroups.style.display = DisplayStyle.Flex;
                if (_containerKnockout != null) _containerKnockout.style.display = DisplayStyle.None;

                Refresh();
            }
        }

        private void RenderGroups(IReadOnlyList<ContinentalGroupStanding> groups)
        {
            if (_containerGroups == null) return;
            _containerGroups.Clear();

            int startIndex = _currentTab == 1 ? 4 : 0;
            int count = Math.Min(4, groups.Count - startIndex);

            for (int i = 0; i < count; i++)
            {
                var group = groups[startIndex + i];
                var card = new VisualElement();
                card.AddToClassList("card");
                card.style.marginBottom = 10;

                var header = new VisualElement();
                header.AddToClassList("card-header");
                var title = new Label($"Group {group.GroupName}");
                title.AddToClassList("text-title");
                title.style.fontSize = 13;
                header.Add(title);
                card.Add(header);

                // Table Header
                var tableHeader = new VisualElement();
                tableHeader.style.flexDirection = FlexDirection.Row;
                tableHeader.style.justifyContent = Justify.SpaceBetween;
                tableHeader.style.paddingBottom = 4;
                tableHeader.style.borderBottomWidth = 1;
                tableHeader.style.borderBottomColor = new UnityEngine.Color(0.2f, 0.25f, 0.35f, 0.5f);

                var thClub = new Label("Club");
                thClub.style.width = 120;
                thClub.style.fontSize = 10;
                thClub.style.color = new UnityEngine.Color(0.6f, 0.65f, 0.75f);

                var thPts = new Label("PTS");
                thPts.style.width = 30;
                thPts.style.fontSize = 10;
                thPts.style.color = new UnityEngine.Color(0.6f, 0.65f, 0.75f);

                tableHeader.Add(thClub);
                tableHeader.Add(thPts);
                card.Add(tableHeader);

                var ranked = group.GetRankedEntries();
                for (int pos = 0; pos < ranked.Count; pos++)
                {
                    var entry = ranked[pos];
                    var row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.justifyContent = Justify.SpaceBetween;
                    row.style.paddingTop = 3;
                    row.style.paddingBottom = 3;

                    var nameLabel = new Label($"{pos + 1}. Club {entry.ClubId.ToString().Substring(0, 4)}");
                    nameLabel.style.fontSize = 11;
                    if (pos < 2) nameLabel.style.color = new UnityEngine.Color(0.2f, 0.8f, 0.4f); // Top 2 qualify

                    var ptsLabel = new Label(entry.Points.ToString());
                    ptsLabel.style.fontSize = 11;
                    ptsLabel.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;

                    row.Add(nameLabel);
                    row.Add(ptsLabel);
                    card.Add(row);
                }

                _containerGroups.Add(card);
            }
        }

        private void RenderKnockout(IReadOnlyList<ContinentalFixture> fixtures)
        {
            if (_containerKnockout == null) return;
            _containerKnockout.Clear();

            if (fixtures == null || fixtures.Count == 0)
            {
                var emptyLabel = new Label("Knockout fixtures will be drawn after group stage completion.");
                emptyLabel.AddToClassList("text-caption");
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                emptyLabel.style.marginTop = 20;
                _containerKnockout.Add(emptyLabel);
                return;
            }

            var stages = fixtures.GroupBy(f => f.Stage).OrderBy(g => (int)g.Key);
            foreach (var stageGroup in stages)
            {
                var card = new VisualElement();
                card.AddToClassList("card");
                card.style.marginBottom = 10;

                var header = new VisualElement();
                header.AddToClassList("card-header");
                var title = new Label(stageGroup.Key.ToString());
                title.AddToClassList("text-title");
                title.style.fontSize = 13;
                header.Add(title);
                card.Add(header);

                foreach (var fix in stageGroup)
                {
                    var row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Row;
                    row.style.justifyContent = Justify.SpaceBetween;
                    row.style.paddingTop = 4;
                    row.style.paddingBottom = 4;

                    string resultText = fix.IsCompleted ? $"{fix.HomeScore} - {fix.AwayScore}" : "vs";
                    var matchLabel = new Label($"Club {fix.HomeClubId.ToString().Substring(0, 4)}  {resultText}  Club {fix.AwayClubId.ToString().Substring(0, 4)}");
                    matchLabel.style.fontSize = 11;

                    row.Add(matchLabel);
                    card.Add(row);
                }

                _containerKnockout.Add(card);
            }
        }

        private void OnSimulateRoundClicked()
        {
            if (SimulationBridge.Instance != null)
            {
                SimulationBridge.Instance.AdvanceSeasonWithContinental();
                Refresh();
            }
        }

        private void DisplaySampleData()
        {
            if (_labelTournamentTitle != null) _labelTournamentTitle.text = "Champions Cup";
            if (_labelPrizeBadge != null) _labelPrizeBadge.text = "£50M Prize Pool";
            if (_labelTournamentStatus != null) _labelTournamentStatus.text = "Preview";
            if (_labelWinnerDisplay != null) _labelWinnerDisplay.text = "Top clubs qualify at season end.";
        }
    }
}
