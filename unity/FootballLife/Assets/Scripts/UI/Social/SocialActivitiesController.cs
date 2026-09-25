#nullable enable
using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Social
{
    /// <summary>
    /// UI Toolkit presenter for SocialActivitiesView.uxml (#P4-008).
    /// Manages social outings selection, category filtering, energy & financial cost verification,
    /// and execution feedback.
    /// </summary>
    public class SocialActivitiesController
    {
        private readonly VisualElement _root;
        private readonly Action _onBack;

        // Header controls
        private readonly Label? _labelEnergy;
        private readonly Label? _labelBalance;
        private readonly Button? _btnBack;
        private readonly Label? _labelToast;

        // Category tabs
        private readonly Button? _tabAll;
        private readonly Button? _tabCasual;
        private readonly Button? _tabTeamBonding;
        private readonly Button? _tabNightlife;
        private readonly Button? _tabGlamour;
        private readonly Button? _tabCharity;

        // Content
        private readonly VisualElement? _containerGrid;
        private SocialActivityCategory? _activeCategoryFilter = null;

        public SocialActivitiesController(VisualElement root, Action onBack)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _onBack = onBack;

            _labelEnergy = _root.Q<Label>("lbl-social-energy");
            _labelBalance = _root.Q<Label>("lbl-social-balance");
            _btnBack = _root.Q<Button>("btn-social-back");
            _labelToast = _root.Q<Label>("lbl-social-toast");

            _tabAll = _root.Q<Button>("tab-cat-all");
            _tabCasual = _root.Q<Button>("tab-cat-casual");
            _tabTeamBonding = _root.Q<Button>("tab-cat-teambonding");
            _tabNightlife = _root.Q<Button>("tab-cat-nightlife");
            _tabGlamour = _root.Q<Button>("tab-cat-glamour");
            _tabCharity = _root.Q<Button>("tab-cat-charity");

            _containerGrid = _root.Q<VisualElement>("container-activities-grid");

            RegisterEvents();
            Refresh();
        }

        private void RegisterEvents()
        {
            if (_btnBack != null)
                _btnBack.clicked += () => _onBack?.Invoke();

            if (_tabAll != null) _tabAll.clicked += () => SetFilter(null);
            if (_tabCasual != null) _tabCasual.clicked += () => SetFilter(SocialActivityCategory.Casual);
            if (_tabTeamBonding != null) _tabTeamBonding.clicked += () => SetFilter(SocialActivityCategory.TeamBonding);
            if (_tabNightlife != null) _tabNightlife.clicked += () => SetFilter(SocialActivityCategory.Nightlife);
            if (_tabGlamour != null) _tabGlamour.clicked += () => SetFilter(SocialActivityCategory.Glamour);
            if (_tabCharity != null) _tabCharity.clicked += () => SetFilter(SocialActivityCategory.Philanthropy);
        }

        private void SetFilter(SocialActivityCategory? category)
        {
            _activeCategoryFilter = category;
            UpdateTabStyles();
            Refresh();
        }

        private void UpdateTabStyles()
        {
            SetTabStyle(_tabAll, _activeCategoryFilter == null);
            SetTabStyle(_tabCasual, _activeCategoryFilter == SocialActivityCategory.Casual);
            SetTabStyle(_tabTeamBonding, _activeCategoryFilter == SocialActivityCategory.TeamBonding);
            SetTabStyle(_tabNightlife, _activeCategoryFilter == SocialActivityCategory.Nightlife);
            SetTabStyle(_tabGlamour, _activeCategoryFilter == SocialActivityCategory.Glamour);
            SetTabStyle(_tabCharity, _activeCategoryFilter == SocialActivityCategory.Philanthropy);
        }

        private static void SetBorderRadius(VisualElement el, float radius)
        {
            el.style.borderTopLeftRadius = radius;
            el.style.borderTopRightRadius = radius;
            el.style.borderBottomLeftRadius = radius;
            el.style.borderBottomRightRadius = radius;
        }

        private void SetTabStyle(Button? btn, bool active)
        {
            if (btn == null) return;
            btn.style.backgroundColor = active ? new Color(0.0f, 0.9f, 1.0f, 0.9f) : new Color(0.12f, 0.16f, 0.24f, 0.8f);
            btn.style.color = active ? new Color(0.04f, 0.07f, 0.12f) : new Color(0.85f, 0.9f, 0.95f);
            btn.style.unityFontStyleAndWeight = active ? FontStyle.Bold : FontStyle.Normal;
        }

        public void Refresh()
        {
            var save = SimulationBridge.Instance?.CurrentSave;
            if (save == null) return;

            if (_labelEnergy != null)
                _labelEnergy.text = $"⚡ {save.Energy}% Energy";

            if (_labelBalance != null)
                _labelBalance.text = $"£{save.BankBalance:N0}";

            if (_containerGrid == null) return;
            _containerGrid.Clear();

            var activities = _activeCategoryFilter.HasValue
                ? SocialActivityCatalog.GetByCategory(_activeCategoryFilter.Value)
                : SocialActivityCatalog.AllActivities;

            foreach (var act in activities)
            {
                var card = CreateActivityCard(act, save);
                _containerGrid.Add(card);
            }
        }

        private VisualElement CreateActivityCard(SocialActivity act, CareerSaveData save)
        {
            bool canAfford = SocialActivitySystem.CanAffordActivity(save, act, out _);

            var card = new VisualElement();
            card.AddToClassList("card-elevated");
            card.style.width = new Length(48, LengthUnit.Percent);
            card.style.minWidth = 240;
            card.style.paddingTop = 14;
            card.style.paddingBottom = 14;
            card.style.paddingLeft = 14;
            card.style.paddingRight = 14;
            SetBorderRadius(card, 12);
            card.style.backgroundColor = new Color(0.08f, 0.12f, 0.19f, 0.95f);
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = new Color(0.2f, 0.27f, 0.38f, 0.5f);
            card.style.borderBottomColor = card.style.borderTopColor;
            card.style.borderLeftColor = card.style.borderTopColor;
            card.style.borderRightColor = card.style.borderTopColor;
            card.style.marginBottom = 12;

            // Top Header: Emoji + Title + Category Badge
            var topRow = new VisualElement();
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.justifyContent = Justify.SpaceBetween;
            topRow.style.alignItems = Align.Center;
            topRow.style.marginBottom = 6;

            var leftBox = new VisualElement();
            leftBox.style.flexDirection = FlexDirection.Row;
            leftBox.style.alignItems = Align.Center;

            var iconLabel = new Label(act.IconEmoji);
            iconLabel.style.fontSize = 20;
            iconLabel.style.marginRight = 8;

            var nameLabel = new Label(act.Name);
            nameLabel.style.fontSize = 13;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.color = new Color(0.97f, 0.98f, 0.99f);

            leftBox.Add(iconLabel);
            leftBox.Add(nameLabel);

            var catLabel = new Label(act.Category.ToString().ToUpperInvariant());
            catLabel.AddToClassList("badge");
            catLabel.style.fontSize = 9;
            catLabel.style.paddingTop = 2;
            catLabel.style.paddingBottom = 2;
            catLabel.style.paddingLeft = 6;
            catLabel.style.paddingRight = 6;

            topRow.Add(leftBox);
            topRow.Add(catLabel);
            card.Add(topRow);

            // Description
            var descLabel = new Label(act.Description);
            descLabel.style.fontSize = 11;
            descLabel.style.color = new Color(0.58f, 0.64f, 0.72f);
            descLabel.style.whiteSpace = WhiteSpace.Normal;
            descLabel.style.marginBottom = 10;
            card.Add(descLabel);

            // Perks & Benefits Row
            var perksRow = new VisualElement();
            perksRow.style.flexDirection = FlexDirection.Row;
            perksRow.style.flexWrap = Wrap.Wrap;
            perksRow.style.marginBottom = 10;

            var perkMorale = new Label($"✨ +{act.MoraleBoost:0} Morale");
            perkMorale.style.fontSize = 10;
            perkMorale.style.color = new Color(0.0f, 0.9f, 1.0f);
            perkMorale.style.backgroundColor = new Color(0.0f, 0.3f, 0.4f, 0.4f);
            perkMorale.style.paddingLeft = 6;
            perkMorale.style.paddingRight = 6;
            perkMorale.style.marginRight = 6;
            perkMorale.style.marginBottom = 4;
            SetBorderRadius(perkMorale, 6);
            perksRow.Add(perkMorale);

            if (act.TeamAffinityBoost > 0)
            {
                var perkTeam = new Label($"🤝 +{act.TeamAffinityBoost:0} Team");
                perkTeam.style.fontSize = 10;
                perkTeam.style.color = new Color(0.2f, 0.83f, 0.6f);
                perkTeam.style.backgroundColor = new Color(0.05f, 0.35f, 0.25f, 0.4f);
                perkTeam.style.paddingLeft = 6;
                perkTeam.style.paddingRight = 6;
                perkTeam.style.marginRight = 6;
                perkTeam.style.marginBottom = 4;
                SetBorderRadius(perkTeam, 6);
                perksRow.Add(perkTeam);
            }

            if (act.PrestigeBoost > 0)
            {
                var perkPrestige = new Label($"⭐ +{act.PrestigeBoost} Prestige");
                perkPrestige.style.fontSize = 10;
                perkPrestige.style.color = new Color(0.96f, 0.75f, 0.14f);
                perkPrestige.style.backgroundColor = new Color(0.4f, 0.3f, 0.05f, 0.4f);
                perkPrestige.style.paddingLeft = 6;
                perkPrestige.style.paddingRight = 6;
                perkPrestige.style.marginRight = 6;
                perkPrestige.style.marginBottom = 4;
                SetBorderRadius(perkPrestige, 6);
                perksRow.Add(perkPrestige);
            }

            card.Add(perksRow);

            // Risk Banner if nightlife or has manager risk
            if (act.ManagerTrustRisk > 0f)
            {
                var riskBox = new Label($"⚠️ {act.ManagerTrustRisk * 100:0}% risk of Manager penalty if late near matchday");
                riskBox.style.fontSize = 9;
                riskBox.style.color = new Color(0.97f, 0.44f, 0.44f);
                riskBox.style.marginBottom = 10;
                card.Add(riskBox);
            }

            // Bottom Action Row: Energy & Money Cost + Go Out Button
            var bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.justifyContent = Justify.SpaceBetween;
            bottomRow.style.alignItems = Align.Center;

            var costBox = new VisualElement();
            var labelEnergyCost = new Label($"⚡ -{act.EnergyCost}% Energy");
            labelEnergyCost.style.fontSize = 11;
            labelEnergyCost.style.unityFontStyleAndWeight = FontStyle.Bold;
            labelEnergyCost.style.color = save.Energy >= act.EnergyCost ? new Color(0.34f, 0.83f, 0.6f) : new Color(0.97f, 0.44f, 0.44f);

            var labelMoneyCost = new Label($"💰 £{act.FinancialCost:N0}");
            labelMoneyCost.style.fontSize = 12;
            labelMoneyCost.style.unityFontStyleAndWeight = FontStyle.Bold;
            labelMoneyCost.style.color = save.BankBalance >= (int)act.FinancialCost ? new Color(0.97f, 0.98f, 0.99f) : new Color(0.97f, 0.44f, 0.44f);

            costBox.Add(labelEnergyCost);
            costBox.Add(labelMoneyCost);
            bottomRow.Add(costBox);

            var btnGo = new Button();
            btnGo.style.height = 34;
            btnGo.style.paddingLeft = 16;
            btnGo.style.paddingRight = 16;
            SetBorderRadius(btnGo, 8);
            btnGo.style.fontSize = 11;
            btnGo.style.unityFontStyleAndWeight = FontStyle.Bold;

            if (save.Energy < act.EnergyCost)
            {
                btnGo.text = "EXHAUSTED";
                btnGo.SetEnabled(false);
                btnGo.style.backgroundColor = new Color(0.18f, 0.22f, 0.3f, 0.4f);
                btnGo.style.color = new Color(0.5f, 0.55f, 0.65f);
            }
            else if (save.BankBalance < (int)act.FinancialCost)
            {
                btnGo.text = "NO FUNDS";
                btnGo.SetEnabled(false);
                btnGo.style.backgroundColor = new Color(0.18f, 0.22f, 0.3f, 0.4f);
                btnGo.style.color = new Color(0.5f, 0.55f, 0.65f);
            }
            else
            {
                btnGo.text = "GO OUT";
                btnGo.style.backgroundColor = new Color(0.0f, 0.9f, 1.0f);
                btnGo.style.color = new Color(0.04f, 0.07f, 0.12f);
                btnGo.clicked += () => OnGoOutClicked(act);
            }

            bottomRow.Add(btnGo);
            card.Add(bottomRow);

            return card;
        }

        private void OnGoOutClicked(SocialActivity activity)
        {
            var bridge = SimulationBridge.Instance;
            if (bridge == null) return;

            var result = bridge.ExecuteSocialActivity(activity, nearMatchday: false);
            ShowToast(result.Message, result.Success ? (result.IncurredManagerDisapproval ? ToastType.Warning : ToastType.Success) : ToastType.Error);
            Refresh();
        }

        private enum ToastType { Success, Warning, Error }

        private void ShowToast(string message, ToastType type)
        {
            if (_labelToast == null) return;
            _labelToast.text = message;
            _labelToast.style.display = DisplayStyle.Flex;

            switch (type)
            {
                case ToastType.Success:
                    _labelToast.style.backgroundColor = new Color(0.06f, 0.73f, 0.5f, 0.25f);
                    _labelToast.style.color = new Color(0.2f, 0.83f, 0.6f);
                    break;
                case ToastType.Warning:
                    _labelToast.style.backgroundColor = new Color(0.96f, 0.75f, 0.14f, 0.25f);
                    _labelToast.style.color = new Color(0.96f, 0.75f, 0.14f);
                    break;
                case ToastType.Error:
                    _labelToast.style.backgroundColor = new Color(0.94f, 0.27f, 0.27f, 0.25f);
                    _labelToast.style.color = new Color(0.97f, 0.44f, 0.44f);
                    break;
            }
        }
    }
}
