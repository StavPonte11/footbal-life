using System;
using System.Collections.Generic;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.OffSeason
{
    /// <summary>
    /// Controller for TransferWindowView.uxml (#P2-020).
    /// Manages contract renewal options, incoming suitor bids, and transitions into the next season.
    /// </summary>
    public class TransferWindowController
    {
        private readonly VisualElement _root;
        private readonly Action<TransferOfferSnapshot> _onAcceptOffer;
        private readonly Action _onStartNextSeason;
        private readonly Action _onBackToGrowth;

        // Current contract labels
        private readonly Label _labelCurRole;
        private readonly Label _valCurClub;
        private readonly Label _valCurWage;
        private readonly Label _valCurExpiry;
        private readonly Label _valCurMarketVal;

        // Renewal labels & button
        private readonly Label _valRenewalWage;
        private readonly Label _valRenewalBonus;
        private readonly Button _btnAcceptRenewal;

        // Bid 1 labels & button
        private readonly Label _valBid1Club;
        private readonly Label _valBid1Wage;
        private readonly Label _valBid1Bonus;
        private readonly Button _btnAcceptBid1;

        // Bid 2 labels & button
        private readonly Label _valBid2Club;
        private readonly Label _valBid2Wage;
        private readonly Label _valBid2Bonus;
        private readonly Button _btnAcceptBid2;

        // Navigation
        private readonly Button _btnStartNextSeason;
        private readonly Button _btnTransfersBack;

        private IReadOnlyList<TransferOfferSnapshot> _currentOffers = Array.Empty<TransferOfferSnapshot>();

        public TransferWindowController(
            VisualElement root,
            Action<TransferOfferSnapshot> onAcceptOffer,
            Action onStartNextSeason,
            Action onBackToGrowth)
        {
            _root = root;
            _onAcceptOffer = onAcceptOffer;
            _onStartNextSeason = onStartNextSeason;
            _onBackToGrowth = onBackToGrowth;

            _labelCurRole     = root.Q<Label>("label-cur-role");
            _valCurClub       = root.Q<Label>("val-cur-club");
            _valCurWage       = root.Q<Label>("val-cur-wage");
            _valCurExpiry     = root.Q<Label>("val-cur-expiry");
            _valCurMarketVal  = root.Q<Label>("val-cur-market-val");

            _valRenewalWage   = root.Q<Label>("val-renewal-wage");
            _valRenewalBonus  = root.Q<Label>("val-renewal-bonus");
            _btnAcceptRenewal = root.Q<Button>("btn-accept-renewal");

            _valBid1Club      = root.Q<Label>("val-bid-1-club");
            _valBid1Wage      = root.Q<Label>("val-bid-1-wage");
            _valBid1Bonus     = root.Q<Label>("val-bid-1-bonus");
            _btnAcceptBid1    = root.Q<Button>("btn-accept-bid-1");

            _valBid2Club      = root.Q<Label>("val-bid-2-club");
            _valBid2Wage      = root.Q<Label>("val-bid-2-wage");
            _valBid2Bonus     = root.Q<Label>("val-bid-2-bonus");
            _btnAcceptBid2    = root.Q<Button>("btn-accept-bid-2");

            _btnStartNextSeason = root.Q<Button>("btn-start-next-season");
            _btnTransfersBack   = root.Q<Button>("btn-transfers-back");

            if (_btnAcceptRenewal != null)
            {
                _btnAcceptRenewal.clicked += () =>
                {
                    if (_currentOffers.Count > 0 && _currentOffers[0].IsRenewal)
                    {
                        _onAcceptOffer?.Invoke(_currentOffers[0]);
                        _btnAcceptRenewal.text = "✅ Contract Extension Signed!";
                        _btnAcceptRenewal.SetEnabled(false);
                    }
                };
            }

            if (_btnAcceptBid1 != null)
            {
                _btnAcceptBid1.clicked += () =>
                {
                    if (_currentOffers.Count > 1)
                    {
                        _onAcceptOffer?.Invoke(_currentOffers[1]);
                        _btnAcceptBid1.text = "✅ Transferred to " + _currentOffers[1].ClubName;
                        _btnAcceptBid1.SetEnabled(false);
                        if (_btnAcceptBid2 != null) _btnAcceptBid2.SetEnabled(false);
                        if (_btnAcceptRenewal != null) _btnAcceptRenewal.SetEnabled(false);
                    }
                };
            }

            if (_btnAcceptBid2 != null)
            {
                _btnAcceptBid2.clicked += () =>
                {
                    if (_currentOffers.Count > 2)
                    {
                        _onAcceptOffer?.Invoke(_currentOffers[2]);
                        _btnAcceptBid2.text = "✅ Transferred to " + _currentOffers[2].ClubName;
                        _btnAcceptBid2.SetEnabled(false);
                        if (_btnAcceptBid1 != null) _btnAcceptBid1.SetEnabled(false);
                        if (_btnAcceptRenewal != null) _btnAcceptRenewal.SetEnabled(false);
                    }
                };
            }

            if (_btnStartNextSeason != null)
                _btnStartNextSeason.clicked += () => _onStartNextSeason?.Invoke();

            if (_btnTransfersBack != null)
                _btnTransfersBack.clicked += () => _onBackToGrowth?.Invoke();
        }

        public void Bind(IReadOnlyList<TransferOfferSnapshot> offers, CareerSaveData? save)
        {
            _currentOffers = offers;

            if (save != null)
            {
                if (_valCurClub != null) _valCurClub.text = save.ClubName;
                if (_valCurWage != null) _valCurWage.text = $"£{save.WeeklyWage:N0} / week";
                if (_valCurExpiry != null) _valCurExpiry.text = $"June {save.ContractEndYear} (Active)";
                if (_valCurMarketVal != null) _valCurMarketVal.text = $"£{save.MarketValue:N0}";
                if (_labelCurRole != null) _labelCurRole.text = string.IsNullOrEmpty(save.SquadRole) ? "FIRST TEAM STARTER" : save.SquadRole.ToUpperInvariant();
            }

            if (offers.Count > 0)
            {
                var renewal = offers[0];
                if (_valRenewalWage != null) _valRenewalWage.text = $"£{renewal.OfferedWeeklyWage:N0} / wk (+50%)";
                if (_valRenewalBonus != null) _valRenewalBonus.text = $"£{renewal.SigningBonus:N0}";
            }

            if (offers.Count > 1)
            {
                var bid1 = offers[1];
                if (_valBid1Club != null) _valBid1Club.text = bid1.ClubName;
                if (_valBid1Wage != null) _valBid1Wage.text = $"£{bid1.OfferedWeeklyWage:N0} / week";
                if (_valBid1Bonus != null) _valBid1Bonus.text = $"£{bid1.SigningBonus:N0}";
            }

            if (offers.Count > 2)
            {
                var bid2 = offers[2];
                if (_valBid2Club != null) _valBid2Club.text = bid2.ClubName;
                if (_valBid2Wage != null) _valBid2Wage.text = $"£{bid2.OfferedWeeklyWage:N0} / week";
                if (_valBid2Bonus != null) _valBid2Bonus.text = $"£{bid2.SigningBonus:N0}";
            }
        }
    }
}
