#nullable enable
using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Shop
{
    /// <summary>
    /// UI Toolkit presenter for LifestyleShopView.uxml.
    /// Manages category filtering, catalog presentation, item purchasing,
    /// and dynamic wallet & perk updates.
    /// </summary>
    public class LifestyleShopController
    {
        private readonly VisualElement _root;
        private readonly Action _onBack;

        private readonly Label _labelShopBalance;
        private readonly Label _labelShopPrestige;

        private readonly Button _tabCatAll;
        private readonly Button _tabCatVehicles;
        private readonly Button _tabCatFashion;
        private readonly Button _tabCatTech;
        private readonly Button _tabCatWellness;

        private readonly VisualElement _containerShopItems;
        private readonly VisualElement _panelToast;
        private readonly Label _labelToastText;

        private LifestyleCategory? _activeCategoryFilter = null; // null = all

        public LifestyleShopController(VisualElement root, Action onBack)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _onBack = onBack ?? throw new ArgumentNullException(nameof(onBack));

            var btnBack = _root.Q<Button>("btn-shop-back");
            if (btnBack != null)
                btnBack.clicked += _onBack;

            _labelShopBalance    = _root.Q<Label>("label-shop-balance");
            _labelShopPrestige   = _root.Q<Label>("label-shop-prestige");

            _tabCatAll           = _root.Q<Button>("tab-cat-all");
            _tabCatVehicles      = _root.Q<Button>("tab-cat-vehicles");
            _tabCatFashion       = _root.Q<Button>("tab-cat-fashion");
            _tabCatTech          = _root.Q<Button>("tab-cat-tech");
            _tabCatWellness      = _root.Q<Button>("tab-cat-wellness");

            _containerShopItems  = _root.Q<VisualElement>("container-shop-items");
            _panelToast          = _root.Q<VisualElement>("panel-shop-toast");
            _labelToastText      = _root.Q<Label>("label-shop-toast-text");

            HookCategoryTabs();
        }

        private void HookCategoryTabs()
        {
            if (_tabCatAll != null)
                _tabCatAll.clicked += () => SetCategoryFilter(null);

            if (_tabCatVehicles != null)
                _tabCatVehicles.clicked += () => SetCategoryFilter(LifestyleCategory.Vehicles);

            if (_tabCatFashion != null)
                _tabCatFashion.clicked += () => SetCategoryFilter(LifestyleCategory.Fashion);

            if (_tabCatTech != null)
                _tabCatTech.clicked += () => SetCategoryFilter(LifestyleCategory.Tech);

            if (_tabCatWellness != null)
                _tabCatWellness.clicked += () => SetCategoryFilter(LifestyleCategory.Wellness);
        }

        public void SetCategoryFilter(LifestyleCategory? category)
        {
            _activeCategoryFilter = category;
            HighlightActiveTab();
            Refresh();
        }

        private void HighlightActiveTab()
        {
            SetTabStyle(_tabCatAll, _activeCategoryFilter == null);
            SetTabStyle(_tabCatVehicles, _activeCategoryFilter == LifestyleCategory.Vehicles);
            SetTabStyle(_tabCatFashion, _activeCategoryFilter == LifestyleCategory.Fashion);
            SetTabStyle(_tabCatTech, _activeCategoryFilter == LifestyleCategory.Tech);
            SetTabStyle(_tabCatWellness, _activeCategoryFilter == LifestyleCategory.Wellness);
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

            // Ensure owned list initialized
            save.OwnedLifestyleItemIds ??= new List<string>();

            // Calculate total prestige and perks
            var (_, _, totalPrestige) = LifestyleShopSystem.CalculateTotalPerks(save.OwnedLifestyleItemIds);
            var prop = HomePropertyCatalog.GetProperty((LifestyleTier)save.LifestyleTier);
            int grandPrestige = (prop?.PrestigeScore ?? 10) + totalPrestige;

            if (_labelShopBalance != null)
                _labelShopBalance.text = $"£{save.BankBalance:N0}";

            if (_labelShopPrestige != null)
                _labelShopPrestige.text = $"{grandPrestige} Pts";

            if (_containerShopItems == null) return;
            _containerShopItems.Clear();

            // Populate cards
            var items = _activeCategoryFilter.HasValue
                ? LifestyleCatalog.GetByCategory(_activeCategoryFilter.Value)
                : LifestyleCatalog.AllItems;

            foreach (var item in items)
            {
                var card = CreateItemCard(item, save);
                _containerShopItems.Add(card);
            }
        }

        private VisualElement CreateItemCard(LifestyleItem item, CareerSaveData save)
        {
            bool isOwned = save.OwnedLifestyleItemIds.Contains(item.Id);
            bool canAfford = save.BankBalance >= (int)item.Price;

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
            card.style.borderTopColor = isOwned
                ? new Color(0.06f, 0.73f, 0.5f, 0.7f)
                : new Color(0.2f, 0.27f, 0.38f, 0.5f);
            card.style.borderBottomColor = card.style.borderTopColor;
            card.style.borderLeftColor = card.style.borderTopColor;
            card.style.borderRightColor = card.style.borderTopColor;

            // Top Header: Emoji + Name + Category Tag
            var topRow = new VisualElement();
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.justifyContent = Justify.SpaceBetween;
            topRow.style.alignItems = Align.Center;
            topRow.style.marginBottom = 6;

            var leftBox = new VisualElement();
            leftBox.style.flexDirection = FlexDirection.Row;
            leftBox.style.alignItems = Align.Center;

            var iconLabel = new Label(item.IconEmoji);
            iconLabel.style.fontSize = 20;
            iconLabel.style.marginRight = 8;

            var nameLabel = new Label(item.Name);
            nameLabel.style.fontSize = 13;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameLabel.style.color = new Color(0.97f, 0.98f, 0.99f);

            leftBox.Add(iconLabel);
            leftBox.Add(nameLabel);

            var catLabel = new Label(item.Category.ToString().ToUpperInvariant());
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
            var descLabel = new Label(item.Description);
            descLabel.style.fontSize = 11;
            descLabel.style.color = new Color(0.58f, 0.64f, 0.72f);
            descLabel.style.whiteSpace = WhiteSpace.Normal;
            descLabel.style.marginBottom = 10;
            card.Add(descLabel);

            // Perks Row
            var perksRow = new VisualElement();
            perksRow.style.flexDirection = FlexDirection.Row;
            perksRow.style.flexWrap = Wrap.Wrap;
            perksRow.style.marginBottom = 12;

            if (item.MoralePerk > 0)
            {
                var perkMorale = new Label($"✨ +{item.MoralePerk:0.0} Morale");
                perkMorale.style.fontSize = 10;
                perkMorale.style.color = new Color(0.0f, 0.9f, 1.0f);
                perkMorale.style.backgroundColor = new Color(0.0f, 0.3f, 0.4f, 0.4f);
                perkMorale.style.paddingLeft = 6;
                perkMorale.style.paddingRight = 6;
                perkMorale.style.marginRight = 6;
                perkMorale.style.marginBottom = 4;
                SetBorderRadius(perkMorale, 6);
                perksRow.Add(perkMorale);
            }

            if (item.EnergyRecoveryPerk > 0)
            {
                var perkEnergy = new Label($"⚡ +{item.EnergyRecoveryPerk * 100:0}% Rest");
                perkEnergy.style.fontSize = 10;
                perkEnergy.style.color = new Color(0.2f, 0.83f, 0.6f);
                perkEnergy.style.backgroundColor = new Color(0.05f, 0.35f, 0.25f, 0.4f);
                perkEnergy.style.paddingLeft = 6;
                perkEnergy.style.paddingRight = 6;
                perkEnergy.style.marginRight = 6;
                perkEnergy.style.marginBottom = 4;
                SetBorderRadius(perkEnergy, 6);
                perksRow.Add(perkEnergy);
            }

            var perkPrestige = new Label($"⭐ +{item.PrestigeScore} Prestige");
            perkPrestige.style.fontSize = 10;
            perkPrestige.style.color = new Color(0.96f, 0.75f, 0.14f);
            perkPrestige.style.backgroundColor = new Color(0.4f, 0.3f, 0.05f, 0.4f);
            perkPrestige.style.paddingLeft = 6;
            perkPrestige.style.paddingRight = 6;
            perkPrestige.style.marginRight = 6;
            perkPrestige.style.marginBottom = 4;
            SetBorderRadius(perkPrestige, 6);
            perksRow.Add(perkPrestige);

            card.Add(perksRow);

            // Bottom Action Row: Price & Upkeep + Button
            var bottomRow = new VisualElement();
            bottomRow.style.flexDirection = FlexDirection.Row;
            bottomRow.style.justifyContent = Justify.SpaceBetween;
            bottomRow.style.alignItems = Align.Center;

            var priceBox = new VisualElement();
            var labelPrice = new Label($"£{item.Price:N0}");
            labelPrice.style.fontSize = 14;
            labelPrice.style.unityFontStyleAndWeight = FontStyle.Bold;
            labelPrice.style.color = new Color(0.97f, 0.98f, 0.99f);

            var labelUpkeep = new Label($"£{item.WeeklyUpkeep:N0} / wk upkeep");
            labelUpkeep.style.fontSize = 9;
            labelUpkeep.style.color = new Color(0.58f, 0.64f, 0.72f);

            priceBox.Add(labelPrice);
            priceBox.Add(labelUpkeep);
            bottomRow.Add(priceBox);

            var btnAction = new Button();
            btnAction.style.height = 34;
            btnAction.style.paddingLeft = 14;
            btnAction.style.paddingRight = 14;
            SetBorderRadius(btnAction, 8);
            btnAction.style.fontSize = 11;
            btnAction.style.unityFontStyleAndWeight = FontStyle.Bold;

            if (isOwned)
            {
                btnAction.text = "✓ OWNED";
                btnAction.SetEnabled(false);
                btnAction.style.backgroundColor = new Color(0.06f, 0.73f, 0.5f, 0.3f);
                btnAction.style.color = new Color(0.2f, 0.83f, 0.6f);
            }
            else if (!canAfford)
            {
                btnAction.text = "CANNOT AFFORD";
                btnAction.SetEnabled(false);
                btnAction.style.backgroundColor = new Color(0.2f, 0.27f, 0.38f, 0.3f);
                btnAction.style.color = new Color(0.58f, 0.64f, 0.72f);
            }
            else
            {
                btnAction.text = "BUY ITEM";
                btnAction.AddToClassList("btn-primary");
                btnAction.style.backgroundColor = new Color(0.0f, 0.9f, 1.0f);
                btnAction.style.color = new Color(0.04f, 0.07f, 0.12f);
                btnAction.clicked += () => ExecutePurchase(item);
            }

            bottomRow.Add(btnAction);
            card.Add(bottomRow);

            return card;
        }

        private void ExecutePurchase(LifestyleItem item)
        {
            var save = SimulationBridge.Instance?.CurrentSave;
            if (save == null) return;

            if (save.BankBalance < (int)item.Price) return;

            // Process purchase
            save.BankBalance -= (int)item.Price;
            save.OwnedLifestyleItemIds.Add(item.Id);
            save.Morale = Math.Clamp(save.Morale + (int)item.MoralePerk, 0, 100);

            // Toast feedback
            ShowToast($"🎉 Purchased {item.Name} for £{item.Price:N0}!");

            // Refresh UI
            Refresh();
        }

        private void ShowToast(string message)
        {
            if (_panelToast == null || _labelToastText == null) return;
            _labelToastText.text = message;
            _panelToast.style.display = DisplayStyle.Flex;
        }
    }
}
