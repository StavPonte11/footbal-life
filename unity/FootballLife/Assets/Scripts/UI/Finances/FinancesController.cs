#nullable enable
using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Finances
{
    /// <summary>
    /// UI Toolkit presenter for FinancesView.uxml.
    /// Computes full cash flow breakdown, manages lifestyle living tier selection,
    /// and displays recent financial transactions.
    /// </summary>
    public class FinancesController
    {
        private readonly VisualElement _root;
        private readonly Action _onBack;

        // Metric Labels
        private readonly Label _labelHeaderBalance;
        private readonly Label _metricBankBalance;
        private readonly Label _metricCashFlow;
        private readonly Label _metricCashFlowSub;
        private readonly Label _metricNetWorth;
        private readonly Label _metricPrestige;

        // Income Labels
        private readonly Label _labelTotalIncome;
        private readonly Label _valIncomeSalary;
        private readonly Label _valIncomeBonuses;
        private readonly Label _valIncomeSponsorship;

        // Expenses Labels
        private readonly Label _labelTotalExpenses;
        private readonly Label _valExpenseLiving;
        private readonly Label _valExpenseApartment;
        private readonly Label _valExpenseItems;
        private readonly Label _valExpenseTax;
        private readonly Label _valExpenseAgent;

        // Tier Buttons
        private readonly Button _btnTierModest;
        private readonly Button _btnTierComfortable;
        private readonly Button _btnTierLuxurious;
        private readonly Button _btnTierExtravagant;
        private readonly Button _btnTierSuperstar;

        // Transactions Container
        private readonly ScrollView _scrollTransactions;

        public FinancesController(VisualElement root, Action onBack)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _onBack = onBack ?? throw new ArgumentNullException(nameof(onBack));

            var btnBack = _root.Q<Button>("btn-finances-back");
            if (btnBack != null)
                btnBack.clicked += _onBack;

            _labelHeaderBalance   = _root.Q<Label>("label-header-balance");
            _metricBankBalance    = _root.Q<Label>("metric-bank-balance");
            _metricCashFlow       = _root.Q<Label>("metric-cash-flow");
            _metricCashFlowSub    = _root.Q<Label>("metric-cash-flow-sub");
            _metricNetWorth       = _root.Q<Label>("metric-net-worth");
            _metricPrestige       = _root.Q<Label>("metric-prestige");

            _labelTotalIncome     = _root.Q<Label>("label-total-income");
            _valIncomeSalary      = _root.Q<Label>("val-income-salary");
            _valIncomeBonuses     = _root.Q<Label>("val-income-bonuses");
            _valIncomeSponsorship = _root.Q<Label>("val-income-sponsorship");

            _labelTotalExpenses   = _root.Q<Label>("label-total-expenses");
            _valExpenseLiving     = _root.Q<Label>("val-expense-living");
            _valExpenseApartment  = _root.Q<Label>("val-expense-apartment");
            _valExpenseItems      = _root.Q<Label>("val-expense-items");
            _valExpenseTax        = _root.Q<Label>("val-expense-tax");
            _valExpenseAgent      = _root.Q<Label>("val-expense-agent");

            _btnTierModest        = _root.Q<Button>("btn-tier-modest");
            _btnTierComfortable   = _root.Q<Button>("btn-tier-comfortable");
            _btnTierLuxurious     = _root.Q<Button>("btn-tier-luxurious");
            _btnTierExtravagant   = _root.Q<Button>("btn-tier-extravagant");
            _btnTierSuperstar     = _root.Q<Button>("btn-tier-superstar");

            _scrollTransactions   = _root.Q<ScrollView>("scroll-transactions");

            HookTierButtons();
        }

        private void HookTierButtons()
        {
            if (_btnTierModest != null)
                _btnTierModest.clicked += () => SelectTier(LifestyleTier.Modest);

            if (_btnTierComfortable != null)
                _btnTierComfortable.clicked += () => SelectTier(LifestyleTier.Comfortable);

            if (_btnTierLuxurious != null)
                _btnTierLuxurious.clicked += () => SelectTier(LifestyleTier.Luxurious);

            if (_btnTierExtravagant != null)
                _btnTierExtravagant.clicked += () => SelectTier(LifestyleTier.Extravagant);

            if (_btnTierSuperstar != null)
                _btnTierSuperstar.clicked += () => SelectTier(LifestyleTier.Superstar);
        }

        public void Refresh()
        {
            var save = SimulationBridge.Instance?.CurrentSave;
            if (save == null) return;

            var tier = (LifestyleTier)save.LifestyleTier;
            var prop = HomePropertyCatalog.GetProperty(tier);
            var ownedItems = LifestyleShopSystem.ResolveOwnedItems(save.OwnedLifestyleItemIds);

            // Compute complete finance breakdown
            var breakdown = FinanceBreakdown.Calculate(
                weeklyWage: save.WeeklyWage,
                bankBalance: save.BankBalance,
                tier: tier,
                property: prop,
                ownedItems: ownedItems,
                matchBonusesRecent: 850m,
                sponsorshipWeekly: save.OverallRating >= 70 ? 1200m : 350m);

            // Header & Primary Metrics
            if (_labelHeaderBalance != null)
                _labelHeaderBalance.text = $"£{breakdown.CurrentBankBalance:N0}";

            if (_metricBankBalance != null)
                _metricBankBalance.text = $"£{breakdown.CurrentBankBalance:N0}";

            if (_metricCashFlow != null)
            {
                string sign = breakdown.NetWeeklyCashFlow >= 0m ? "+" : "-";
                decimal absCashFlow = Math.Abs(breakdown.NetWeeklyCashFlow);
                _metricCashFlow.text = $"{sign}£{absCashFlow:N0} / wk";
                _metricCashFlow.style.color = breakdown.NetWeeklyCashFlow >= 0m
                    ? new Color(0.2f, 0.83f, 0.6f)
                    : new Color(0.97f, 0.44f, 0.44f);
            }

            if (_metricCashFlowSub != null)
            {
                _metricCashFlowSub.text = breakdown.NetWeeklyCashFlow >= 0m
                    ? "Healthy weekly surplus"
                    : "Deficit! Reduce lifestyle costs";
                _metricCashFlowSub.style.color = breakdown.NetWeeklyCashFlow >= 0m
                    ? new Color(0.06f, 0.73f, 0.5f)
                    : new Color(0.97f, 0.44f, 0.44f);
            }

            if (_metricNetWorth != null)
                _metricNetWorth.text = $"£{breakdown.EstimatedNetWorth:N0}";

            if (_metricPrestige != null)
                _metricPrestige.text = $"⭐ {breakdown.TotalPrestige} / 100";

            // Income Breakdown
            if (_labelTotalIncome != null)
                _labelTotalIncome.text = $"+£{breakdown.TotalWeeklyIncome:N0}/wk";

            if (_valIncomeSalary != null)
                _valIncomeSalary.text = $"£{breakdown.WeeklyWage:N0}";

            if (_valIncomeBonuses != null)
                _valIncomeBonuses.text = $"£{breakdown.EstimatedMatchBonuses:N0}";

            if (_valIncomeSponsorship != null)
                _valIncomeSponsorship.text = $"£{breakdown.SponsorshipIncome:N0}";

            // Expenses Breakdown
            if (_labelTotalExpenses != null)
                _labelTotalExpenses.text = $"-£{breakdown.TotalWeeklyExpenses:N0}/wk";

            if (_valExpenseLiving != null)
                _valExpenseLiving.text = $"£{breakdown.LifestyleTierCost:N0}";

            if (_valExpenseApartment != null)
                _valExpenseApartment.text = $"£{breakdown.ApartmentUpkeep:N0}";

            if (_valExpenseItems != null)
                _valExpenseItems.text = $"£{breakdown.LifestyleItemsUpkeep:N0}";

            if (_valExpenseTax != null)
                _valExpenseTax.text = $"£{breakdown.TaxEstimate:N0}";

            if (_valExpenseAgent != null)
                _valExpenseAgent.text = $"£{breakdown.AgentCommission:N0}";

            // Highlight Active Living Tier
            UpdateTierButtons(tier);

            // Populate Ledger
            PopulateTransactions(save, breakdown);
        }

        private void UpdateTierButtons(LifestyleTier currentTier)
        {
            SetTierButtonActive(_btnTierModest, currentTier == LifestyleTier.Modest, "Modest — £150 / wk");
            SetTierButtonActive(_btnTierComfortable, currentTier == LifestyleTier.Comfortable, "Comfortable — £500 / wk");
            SetTierButtonActive(_btnTierLuxurious, currentTier == LifestyleTier.Luxurious, "Luxurious — £2,000 / wk");
            SetTierButtonActive(_btnTierExtravagant, currentTier == LifestyleTier.Extravagant, "Extravagant — £8,000 / wk");
            SetTierButtonActive(_btnTierSuperstar, currentTier == LifestyleTier.Superstar, "Superstar — £25,000 / wk");
        }

        private void SetTierButtonActive(Button? btn, bool active, string baseText)
        {
            if (btn == null) return;
            btn.text = active ? $"✓ {baseText} (ACTIVE)" : baseText;
            btn.style.borderTopWidth = active ? 2 : 1;
            btn.style.borderBottomWidth = active ? 2 : 1;
            btn.style.borderLeftWidth = active ? 2 : 1;
            btn.style.borderRightWidth = active ? 2 : 1;
            btn.style.borderTopColor = active ? new Color(0.0f, 0.9f, 1.0f) : new Color(0.2f, 0.27f, 0.38f);
            btn.style.borderBottomColor = active ? new Color(0.0f, 0.9f, 1.0f) : new Color(0.2f, 0.27f, 0.38f);
            btn.style.borderLeftColor = active ? new Color(0.0f, 0.9f, 1.0f) : new Color(0.2f, 0.27f, 0.38f);
            btn.style.borderRightColor = active ? new Color(0.0f, 0.9f, 1.0f) : new Color(0.2f, 0.27f, 0.38f);
            btn.style.backgroundColor = active ? new Color(0.0f, 0.23f, 0.35f, 0.9f) : new Color(0.12f, 0.16f, 0.24f, 0.7f);
        }

        private void SelectTier(LifestyleTier tier)
        {
            var save = SimulationBridge.Instance?.CurrentSave;
            if (save == null) return;

            save.LifestyleTier = (int)tier;
            Refresh();
        }

        private void PopulateTransactions(CareerSaveData save, FinanceBreakdown breakdown)
        {
            if (_scrollTransactions == null) return;
            _scrollTransactions.Clear();

            // Curated ledger transactions based on recent career history
            var transactions = new (string date, string desc, decimal amount, bool isCredit)[]
            {
                ("Week " + save.CurrentWeek + " Day 1", "Weekly Wage Deposit", save.WeeklyWage, true),
                ("Week " + save.CurrentWeek + " Day 1", "Match Appearance & Win Bonus", 850m, true),
                ("Week " + save.CurrentWeek + " Day 2", "Living Standard Upkeep (" + breakdown.CurrentTier + ")", -breakdown.LifestyleTierCost, false),
                ("Week " + save.CurrentWeek + " Day 2", "Apartment Residence Upkeep", -breakdown.ApartmentUpkeep, false),
                ("Week " + save.CurrentWeek + " Day 3", "Income Tax Withholding (PAYE)", -breakdown.TaxEstimate, false),
                ("Week " + save.CurrentWeek + " Day 3", "Sports Agent Management Fee (5%)", -breakdown.AgentCommission, false),
                ("Week " + Math.Max(1, save.CurrentWeek - 1) + " Day 7", "Weekly Wage Deposit", save.WeeklyWage, true),
                ("Week " + Math.Max(1, save.CurrentWeek - 1) + " Day 7", "Commercial Sponsorship Payment", 500m, true)
            };

            foreach (var tx in transactions)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.alignItems = Align.Center;
                row.style.paddingTop = 6;
                row.style.paddingBottom = 6;
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(0.2f, 0.27f, 0.38f, 0.3f);

                var leftBox = new VisualElement();
                var labelDesc = new Label(tx.desc);
                labelDesc.style.fontSize = 11;
                labelDesc.style.color = new Color(0.97f, 0.98f, 0.99f);
                var labelDate = new Label(tx.date);
                labelDate.style.fontSize = 9;
                labelDate.style.color = new Color(0.58f, 0.64f, 0.72f);
                leftBox.Add(labelDesc);
                leftBox.Add(labelDate);

                string amountStr = (tx.isCredit ? "+" : "-") + $"£{Math.Abs(tx.amount):N0}";
                var labelAmount = new Label(amountStr);
                labelAmount.style.fontSize = 12;
                labelAmount.style.unityFontStyleAndWeight = FontStyle.Bold;
                labelAmount.style.color = tx.isCredit
                    ? new Color(0.2f, 0.83f, 0.6f)
                    : new Color(0.97f, 0.44f, 0.44f);

                row.Add(leftBox);
                row.Add(labelAmount);
                _scrollTransactions.Add(row);
            }
        }
    }
}
