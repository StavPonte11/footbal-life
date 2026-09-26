#nullable enable
using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Unity.Core.Bridge;
using FootballLife.Unity.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Sponsorship
{
    /// <summary>
    /// UI Toolkit controller managing commercial endorsements, sponsorship signings, and brand perks.
    /// </summary>
    public sealed class SponsorshipViewController
    {
        private readonly VisualElement _root;
        private readonly Action _onBack;

        private Button? _btnBack;
        private Label? _labelSlotsBadge;
        private Label? _labelTotalIncome;
        private Label? _labelPerksSummary;

        private Button? _tabActive;
        private Button? _tabAvailable;

        private VisualElement? _containerActive;
        private VisualElement? _containerAvailable;

        public SponsorshipViewController(VisualElement root, Action onBack)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _onBack = onBack ?? throw new ArgumentNullException(nameof(onBack));

            QueryElements();
            RegisterCallbacks();
        }

        private void QueryElements()
        {
            _btnBack = _root.Q<Button>("btn-sponsorship-back");
            _labelSlotsBadge = _root.Q<Label>("label-slots-badge");
            _labelTotalIncome = _root.Q<Label>("label-total-commercial-income");
            _labelPerksSummary = _root.Q<Label>("label-perks-summary");

            _tabActive = _root.Q<Button>("tab-active-deals");
            _tabAvailable = _root.Q<Button>("tab-available-offers");

            _containerActive = _root.Q<VisualElement>("container-active-deals");
            _containerAvailable = _root.Q<VisualElement>("container-available-offers");
        }

        private void RegisterCallbacks()
        {
            if (_btnBack != null) _btnBack.clicked += _onBack.Invoke;

            if (_tabActive != null) _tabActive.clicked += () => SwitchTab(true);
            if (_tabAvailable != null) _tabAvailable.clicked += () => SwitchTab(false);
        }

        public void Refresh()
        {
            var bridge = SimulationBridge.Instance;
            if (bridge == null) return;

            var activeDeals = bridge.GetActiveSponsorships();
            var availableOffers = bridge.GetAvailableSponsorshipOffers();

            int activeCount = activeDeals?.Count ?? 0;
            if (_labelSlotsBadge != null)
            {
                _labelSlotsBadge.text = $"{activeCount} / {SponsorshipSystem.MaxConcurrentSponsorships} Slots";
            }

            int weeklyIncome = 0;
            int energyBonus = 0;
            int fameBonus = 0;

            if (activeDeals != null)
            {
                foreach (var active in activeDeals)
                {
                    weeklyIncome += active.Deal.WeeklyPayout;
                    energyBonus += active.Deal.EnergyRecoveryBonus;
                    fameBonus += active.Deal.WeeklyFameBonus;
                }
            }

            if (_labelTotalIncome != null)
            {
                _labelTotalIncome.text = $"+£{weeklyIncome:N0} / wk";
            }

            if (_labelPerksSummary != null)
            {
                _labelPerksSummary.text = activeCount > 0
                    ? $"Active Perks: +{energyBonus} Energy recovery/wk, +{fameBonus} Fame multiplier/wk."
                    : "No active endorsements. Browse Available Offers to sign lucrative commercial contracts.";
            }

            PopulateActiveDeals(activeDeals);
            PopulateAvailableOffers(availableOffers, activeCount);
        }

        private void SwitchTab(bool showActive)
        {
            if (_containerActive != null) _containerActive.style.display = showActive ? DisplayStyle.Flex : DisplayStyle.None;
            if (_containerAvailable != null) _containerAvailable.style.display = showActive ? DisplayStyle.None : DisplayStyle.Flex;

            if (_tabActive != null)
            {
                if (showActive) _tabActive.AddToClassList("tab-button-active");
                else _tabActive.RemoveFromClassList("tab-button-active");
            }

            if (_tabAvailable != null)
            {
                if (!showActive) _tabAvailable.AddToClassList("tab-button-active");
                else _tabAvailable.RemoveFromClassList("tab-button-active");
            }
        }

        private void PopulateActiveDeals(IReadOnlyList<ActiveSponsorship>? activeDeals)
        {
            if (_containerActive == null) return;
            _containerActive.Clear();

            if (activeDeals == null || activeDeals.Count == 0)
            {
                var emptyElement = EmptyStateController.CreateEmptyStateElement(
                    "🤝",
                    "No Active Sponsors",
                    "You have no commercial endorsements currently active. Check available brand deals to boost your income.",
                    "View Offers",
                    () => SwitchTab(false));
                _containerActive.Add(emptyElement);
                return;
            }

            foreach (var item in activeDeals)
            {
                var card = new VisualElement();
                card.AddToClassList("card-elevated");
                card.style.marginBottom = 10;
                card.style.paddingTop = 12;
                card.style.paddingBottom = 12;
                card.style.paddingLeft = 14;
                card.style.paddingRight = 14;

                var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center } };
                var nameLabel = new Label(item.Deal.BrandName) { style = { fontSize = 14, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };
                var tierBadge = new Label(item.Deal.Tier.ToString().ToUpperInvariant());
                tierBadge.AddToClassList("badge");
                tierBadge.AddToClassList("badge-gold");
                tierBadge.style.fontSize = 10;

                headerRow.Add(nameLabel);
                headerRow.Add(tierBadge);
                card.Add(headerRow);

                var descLabel = new Label(item.Deal.Description) { style = { fontSize = 11, color = new Color(0.7f, 0.7f, 0.7f), marginTop = 4, marginBottom = 6 } };
                card.Add(descLabel);

                var metricsRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center } };
                var payoutLabel = new Label($"£{item.Deal.WeeklyPayout:N0}/wk") { style = { color = new Color(0.2f, 0.8f, 0.4f), unityFontStyleAndWeight = FontStyle.Bold, fontSize = 13 } };
                var durationLabel = new Label($"{item.WeeksRemaining} wks remaining") { style = { color = new Color(0.6f, 0.6f, 0.6f), fontSize = 11 } };

                metricsRow.Add(payoutLabel);
                metricsRow.Add(durationLabel);
                card.Add(metricsRow);

                _containerActive.Add(card);
            }
        }

        private void PopulateAvailableOffers(IReadOnlyList<SponsorshipDeal>? offers, int activeCount)
        {
            if (_containerAvailable == null) return;
            _containerAvailable.Clear();

            if (offers == null || offers.Count == 0)
            {
                var emptyElement = EmptyStateController.CreateEmptyStateElement(
                    "✨",
                    "No Available Endorsements",
                    "No sponsorship offers on the table. Increase your reputation on the pitch to attract premier brands.",
                    null);
                _containerAvailable.Add(emptyElement);
                return;
            }

            foreach (var deal in offers)
            {
                var card = new VisualElement();
                card.AddToClassList("card-elevated");
                card.style.marginBottom = 10;
                card.style.paddingTop = 12;
                card.style.paddingBottom = 12;
                card.style.paddingLeft = 14;
                card.style.paddingRight = 14;

                var headerRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center } };
                var nameLabel = new Label(deal.BrandName) { style = { fontSize = 14, color = Color.white, unityFontStyleAndWeight = FontStyle.Bold } };
                var typeBadge = new Label($"{deal.Tier} · {deal.Type}");
                typeBadge.AddToClassList("badge");
                typeBadge.AddToClassList("badge-amber");
                typeBadge.style.fontSize = 10;

                headerRow.Add(nameLabel);
                headerRow.Add(typeBadge);
                card.Add(headerRow);

                var descLabel = new Label(deal.Description) { style = { fontSize = 11, color = new Color(0.7f, 0.7f, 0.7f), marginTop = 4, marginBottom = 8 } };
                card.Add(descLabel);

                var financeRow = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center, marginBottom = 8 } };
                var termsLabel = new Label($"Payout: £{deal.WeeklyPayout:N0}/wk  ·  Bonus: £{deal.SigningBonus:N0}")
                {
                    style = { color = new Color(0.2f, 0.8f, 0.4f), fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold }
                };
                var durLabel = new Label($"{deal.DurationWeeks} weeks") { style = { color = new Color(0.6f, 0.6f, 0.6f), fontSize = 11 } };
                financeRow.Add(termsLabel);
                financeRow.Add(durLabel);
                card.Add(financeRow);

                var signBtn = new Button { text = $"Sign Contract (+£{deal.SigningBonus:N0} Bonus)" };
                signBtn.AddToClassList("btn");
                signBtn.AddToClassList("btn-primary");
                signBtn.style.height = 36;
                signBtn.style.fontSize = 12;

                if (activeCount >= SponsorshipSystem.MaxConcurrentSponsorships)
                {
                    signBtn.SetEnabled(false);
                    signBtn.text = "Max 3 Sponsorship Slots Full";
                }
                else
                {
                    signBtn.clicked += () =>
                    {
                        var (success, _) = SimulationBridge.Instance!.SignSponsorshipDeal(deal);
                        if (success)
                        {
                            Refresh();
                            SwitchTab(true);
                        }
                    };
                }

                card.Add(signBtn);
                _containerAvailable.Add(card);
            }
        }
    }
}
