#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Unity.Core.Bridge;

namespace FootballLife.Unity.UI.Transfers
{
    /// <summary>
    /// UI Toolkit presenter managing the Global Transfer Market, multi-club bidding wars,
    /// player-initiated transfer requests, and league hierarchy overview (#P5-001, #P5-002, #P5-003).
    /// </summary>
    public class TransferMarketController
    {
        private readonly VisualElement _root;
        private readonly Action? _onBack;
        private readonly Action? _onTransferCompleted;

        private Label? _labelMarketVal;
        private Label? _labelPlayerStatus;
        private Label? _labelPlayerWages;
        private Button? _btnRequestTransfer;
        private Button? _btnClose;
        private Button? _tabBids;
        private Button? _tabLeagues;
        private VisualElement? _bidsContainer;
        private VisualElement? _leaguesContainer;
        private VisualElement? _toast;
        private Label? _labelToast;

        private TransferBiddingWar? _currentBiddingWar;
        private bool _isTransferListed;

        public TransferMarketController(VisualElement root, Action? onBack = null, Action? onTransferCompleted = null)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _onBack = onBack;
            _onTransferCompleted = onTransferCompleted;

            QueryElements();
            BindEvents();
            Refresh();
        }

        private void QueryElements()
        {
            _labelMarketVal = _root.Q<Label>("label-market-val");
            _labelPlayerStatus = _root.Q<Label>("label-player-status");
            _labelPlayerWages = _root.Q<Label>("label-player-wages");
            _btnRequestTransfer = _root.Q<Button>("btn-request-transfer");
            _btnClose = _root.Q<Button>("btn-close-market");
            _tabBids = _root.Q<Button>("tab-bids");
            _tabLeagues = _root.Q<Button>("tab-leagues");
            _bidsContainer = _root.Q<VisualElement>("bids-container");
            _leaguesContainer = _root.Q<VisualElement>("leagues-container");
            _toast = _root.Q<VisualElement>("transfer-toast");
            _labelToast = _root.Q<Label>("label-transfer-toast");
        }

        private void BindEvents()
        {
            if (_btnClose != null) _btnClose.clicked += () => _onBack?.Invoke();
            if (_btnRequestTransfer != null) _btnRequestTransfer.clicked += OnRequestTransferClicked;
            if (_tabBids != null) _tabBids.clicked += ShowBidsTab;
            if (_tabLeagues != null) _tabLeagues.clicked += ShowLeaguesTab;
        }

        public void Refresh()
        {
            var bridge = SimulationBridge.Instance;
            var save = bridge?.CurrentSave;

            if (_labelPlayerStatus != null)
            {
                string statusText = _isTransferListed ? "TRANSFER LISTED" : "Under Contract";
                _labelPlayerStatus.text = $"Status: {statusText} at {save?.ClubName ?? "Northfield Town"}";
            }

            if (_labelPlayerWages != null)
            {
                _labelPlayerWages.text = $"Current Wage: £{save?.WeeklyWage ?? 500:N0}/wk · Manager Trust: {save?.ManagerTrust ?? 50}%";
            }

            if (bridge != null)
            {
                _currentBiddingWar = bridge.GetTransferBiddingWar(_isTransferListed);
                if (_labelMarketVal != null && _currentBiddingWar != null)
                {
                    _labelMarketVal.text = $"VALUATION: £{_currentBiddingWar.EstimatedMarketValue:N0}";
                }
            }

            PopulateBids();
            PopulateLeagues();
        }

        private void ShowBidsTab()
        {
            if (_tabBids != null)
            {
                _tabBids.style.backgroundColor = new Color(0.15f, 0.39f, 0.92f);
                _tabBids.style.color = Color.white;
            }
            if (_tabLeagues != null)
            {
                _tabLeagues.style.backgroundColor = new Color(1f, 1f, 1f, 0.08f);
                _tabLeagues.style.color = new Color(0.58f, 0.64f, 0.72f);
            }
            if (_bidsContainer != null) _bidsContainer.style.display = DisplayStyle.Flex;
            if (_leaguesContainer != null) _leaguesContainer.style.display = DisplayStyle.None;
        }

        private void ShowLeaguesTab()
        {
            if (_tabLeagues != null)
            {
                _tabLeagues.style.backgroundColor = new Color(0.15f, 0.39f, 0.92f);
                _tabLeagues.style.color = Color.white;
            }
            if (_tabBids != null)
            {
                _tabBids.style.backgroundColor = new Color(1f, 1f, 1f, 0.08f);
                _tabBids.style.color = new Color(0.58f, 0.64f, 0.72f);
            }
            if (_bidsContainer != null) _bidsContainer.style.display = DisplayStyle.None;
            if (_leaguesContainer != null) _leaguesContainer.style.display = DisplayStyle.Flex;
        }

        private void PopulateBids()
        {
            if (_bidsContainer == null) return;
            _bidsContainer.Clear();

            if (_currentBiddingWar == null || _currentBiddingWar.Bids.Count == 0)
            {
                var emptyLabel = new Label("No active club bids currently on the table. Inquire again during the transfer window or submit a transfer request.")
                {
                    style =
                    {
                        color = new Color(0.58f, 0.64f, 0.72f),
                        fontSize = 13,
                        marginTop = 24,
                        unityTextAlign = TextAnchor.MiddleCenter
                    }
                };
                _bidsContainer.Add(emptyLabel);
                return;
            }

            foreach (var bid in _currentBiddingWar.Bids)
            {
                var card = CreateBidCard(bid);
                _bidsContainer.Add(card);
            }
        }

        private VisualElement CreateBidCard(ClubBid bid)
        {
            var card = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(0.12f, 0.16f, 0.24f),
                    borderTopLeftRadius = 14,
                    borderTopRightRadius = 14,
                    borderBottomLeftRadius = 14,
                    borderBottomRightRadius = 14,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftColor = new Color(1f, 1f, 1f, 0.08f),
                    borderRightColor = new Color(1f, 1f, 1f, 0.08f),
                    borderTopColor = new Color(1f, 1f, 1f, 0.08f),
                    borderBottomColor = new Color(1f, 1f, 1f, 0.08f),
                    paddingTop = 16,
                    paddingBottom = 16,
                    paddingLeft = 18,
                    paddingRight = 18,
                    marginBottom = 12,
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    alignItems = Align.Center
                }
            };

            // Left side: Club info
            var left = new VisualElement { style = { flexGrow = 1 } };
            
            var titleRow = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };
            var clubName = new Label(bid.BiddingClubName)
            {
                style = { fontSize = 16, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white, marginRight = 10 }
            };
            var tierChip = new Label($"Tier {bid.LeagueTier}: {bid.LeagueName}")
            {
                style =
                {
                    fontSize = 10,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = bid.LeagueTier == 1 ? new Color(1f, 0.84f, 0f) : new Color(0.38f, 0.72f, 1f),
                    backgroundColor = new Color(0f, 0f, 0f, 0.35f),
                    paddingTop = 3,
                    paddingBottom = 3,
                    paddingLeft = 8,
                    paddingRight = 8,
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6
                }
            };
            titleRow.Add(clubName);
            titleRow.Add(tierChip);
            left.Add(titleRow);

            var detailsRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 6 } };
            var wageLabel = new Label($"💰 £{bid.OfferedWeeklyWage:N0}/wk")
            {
                style = { fontSize = 13, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.06f, 0.71f, 0.51f), marginRight = 14 }
            };
            var bonusLabel = new Label($"🎁 Bonus: £{bid.SigningBonus:N0}")
            {
                style = { fontSize = 12, color = new Color(0.95f, 0.75f, 0.2f), marginRight = 14 }
            };
            var roleLabel = new Label($"📋 {bid.PromisedSquadRole} ({bid.ContractLengthYears} Yrs)")
            {
                style = { fontSize = 12, color = new Color(0.58f, 0.64f, 0.72f) }
            };
            detailsRow.Add(wageLabel);
            detailsRow.Add(bonusLabel);
            detailsRow.Add(roleLabel);
            left.Add(detailsRow);

            card.Add(left);

            // Right side: Action buttons
            var right = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

            var btnAccept = new Button(() => OnAcceptBidClicked(bid))
            {
                text = "ACCEPT OFFER",
                style =
                {
                    backgroundColor = new Color(0.06f, 0.71f, 0.51f),
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 11,
                    paddingTop = 8,
                    paddingBottom = 8,
                    paddingLeft = 14,
                    paddingRight = 14,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    borderTopWidth = 0,
                    borderRightWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    marginRight = 8
                }
            };

            var btnReject = new Button(() => OnRejectBidClicked(bid, card))
            {
                text = "DECLINE",
                style =
                {
                    backgroundColor = new Color(1f, 1f, 1f, 0.08f),
                    color = new Color(0.58f, 0.64f, 0.72f),
                    unityFontStyleAndWeight = FontStyle.Bold,
                    fontSize = 11,
                    paddingTop = 8,
                    paddingBottom = 8,
                    paddingLeft = 12,
                    paddingRight = 12,
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    borderTopWidth = 0,
                    borderRightWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0
                }
            };

            right.Add(btnAccept);
            right.Add(btnReject);
            card.Add(right);

            return card;
        }

        private void PopulateLeagues()
        {
            if (_leaguesContainer == null) return;
            _leaguesContainer.Clear();

            var profiles = LeagueTierConfig.AllProfiles;
            foreach (var profile in profiles)
            {
                var card = new VisualElement
                {
                    style =
                    {
                        backgroundColor = new Color(0.12f, 0.16f, 0.24f),
                        borderTopLeftRadius = 14,
                        borderTopRightRadius = 14,
                        borderBottomLeftRadius = 14,
                        borderBottomRightRadius = 14,
                        borderLeftWidth = 1,
                        borderRightWidth = 1,
                        borderTopWidth = 1,
                        borderBottomWidth = 1,
                        borderLeftColor = new Color(1f, 1f, 1f, 0.08f),
                        borderRightColor = new Color(1f, 1f, 1f, 0.08f),
                        borderTopColor = new Color(1f, 1f, 1f, 0.08f),
                        borderBottomColor = new Color(1f, 1f, 1f, 0.08f),
                        paddingTop = 14,
                        paddingBottom = 14,
                        paddingLeft = 18,
                        paddingRight = 18,
                        marginBottom = 10,
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween,
                        alignItems = Align.Center
                    }
                };

                var left = new VisualElement();
                var title = new Label($"🏆 {profile.TierName} (Tier {(int)profile.Tier})")
                {
                    style = { fontSize = 15, unityFontStyleAndWeight = FontStyle.Bold, color = Color.white }
                };
                var sub = new Label($"Trophy: {profile.TrophyName} · Prestige Rating: {profile.PrestigeRating}/100")
                {
                    style = { fontSize = 11, color = new Color(0.58f, 0.64f, 0.72f), marginTop = 2 }
                };
                left.Add(title);
                left.Add(sub);

                var right = new VisualElement { style = { alignItems = Align.FlexEnd } };
                var wage = new Label($"Avg Wage: £{profile.AverageWeeklyWage:N0}/wk")
                {
                    style = { fontSize = 12, unityFontStyleAndWeight = FontStyle.Bold, color = new Color(0.06f, 0.71f, 0.51f) }
                };
                var spots = new Label($"Promotion: {profile.PromotionSpots} · Relegation: {profile.RelegationSpots}")
                {
                    style = { fontSize = 11, color = new Color(0.95f, 0.75f, 0.2f), marginTop = 2 }
                };
                right.Add(wage);
                right.Add(spots);

                card.Add(left);
                card.Add(right);
                _leaguesContainer.Add(card);
            }
        }

        private void OnRequestTransferClicked()
        {
            var bridge = SimulationBridge.Instance;
            if (bridge == null) return;

            var res = bridge.RequestTransferListing("Player submitted formal transfer request");
            if (res.Approved)
            {
                _isTransferListed = true;
                ShowToast($"📋 Transfer Request Accepted: {res.ManagerResponse}");
            }
            else
            {
                ShowToast($"❌ Transfer Request Rejected: {res.ManagerResponse}");
            }

            Refresh();
        }

        private void OnAcceptBidClicked(ClubBid bid)
        {
            var bridge = SimulationBridge.Instance;
            if (bridge == null) return;

            bool success = bridge.AcceptTransferBid(bid);
            if (success)
            {
                ShowToast($"🤝 Congratulations! Signed for {bid.BiddingClubName}!");
                _onTransferCompleted?.Invoke();
            }
        }

        private void OnRejectBidClicked(ClubBid bid, VisualElement card)
        {
            card.style.display = DisplayStyle.None;
            ShowToast($"Declined offer from {bid.BiddingClubName}.");
        }

        private void ShowToast(string message)
        {
            if (_toast == null || _labelToast == null) return;
            _labelToast.text = message;
            _toast.style.display = DisplayStyle.Flex;
        }
    }
}
