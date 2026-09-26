using System;
using FootballLife.Domain;
using FootballLife.Unity.Core.Bridge;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Hub
{
    /// <summary>
    /// Binds CareerHubView.uxml to SimulationBridge events.
    /// Pure presentation: no game logic, only reads from SimulationDaySnapshot and CurrentSave.
    /// </summary>
    public class CareerHubController
    {
        // ── Queried Elements ──────────────────────────────────────────────────
        private readonly Label _labelPlayerName;
        private readonly Label _labelClubPosition;
        private readonly Label _labelAvatarInitial;
        private readonly Label _labelOvr;
        private readonly Label _labelDate;
        private readonly Label _labelDay;

        private readonly VisualElement _fillEnergy;
        private readonly VisualElement _fillForm;
        private readonly VisualElement _fillMorale;
        private readonly VisualElement _fillTrust;
        private readonly Label _valEnergy;
        private readonly Label _valForm;
        private readonly Label _valMorale;
        private readonly Label _valTrust;

        private readonly Label _labelWage;
        private readonly Label _labelBalance;

        private readonly VisualElement _cardNextMatch;
        private readonly Label _labelMatchCountdown;
        private readonly Label _labelMatchHome;
        private readonly Label _labelMatchOpponent;
        private readonly Label _labelMatchCompetition;

        private readonly Label _labelStatus;

        private readonly Button _btnTrain;
        private readonly Button _btnRest;
        private readonly Button _btnMatch;
        private readonly Button _btnCareer;
        private readonly Button? _btnOffSeason;
        private readonly Button? _btnHome;
        private readonly Button? _btnQuickPhone;
        private readonly Button? _btnQuickFinances;
        private readonly Button? _btnQuickShop;
        private readonly Button? _btnQuickMarket;
        private readonly Button? _btnFinances;
        private readonly Button? _btnShop;
        private readonly Button? _btnTransferMarket;
        private readonly Button? _btnContinental;
        private readonly Button _btnAdvanceDay;

        // ── Overlays controlled externally ───────────────────────────────────
        private readonly Action _onOpenTraining;
        private readonly Action _onOpenRest;
        private readonly Action _onOpenCareer;
        private readonly Action _onOpenMatch;
        private readonly Action _onOpenOffSeason;
        private readonly Action? _onOpenHome;
        private readonly Action? _onOpenPhone;
        private readonly Action? _onOpenFinances;
        private readonly Action? _onOpenShop;
        private readonly Action? _onOpenSocial;
        private readonly Action? _onOpenPress;
        private readonly Action? _onOpenContinental;
        private readonly Action? _onOpenSponsorship;
        private readonly Action? _onOpenLegacy;
        private readonly Action? _onOpenTransferMarket;
        private readonly Action _onLifeEventPending;

        private SimulationBridge? _bridge;

        private static readonly string[] _dayNames =
            { "", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

        // ── Constructor ───────────────────────────────────────────────────────
        public CareerHubController(
            VisualElement root,
            Action onOpenTraining,
            Action onOpenRest,
            Action onOpenCareer,
            Action onOpenMatch,
            Action onLifeEventPending,
            Action? onOpenOffSeason = null,
            Action? onOpenHome = null,
            Action? onOpenPhone = null,
            Action? onOpenFinances = null,
            Action? onOpenShop = null,
            Action? onOpenSocial = null,
            Action? onOpenPress = null,
            Action? onOpenTransferMarket = null,
            Action? onOpenContinental = null,
            Action? onOpenSponsorship = null,
            Action? onOpenLegacy = null)
        {
            _onOpenTraining = onOpenTraining;
            _onOpenRest = onOpenRest;
            _onOpenCareer = onOpenCareer;
            _onOpenMatch = onOpenMatch;
            _onLifeEventPending = onLifeEventPending;
            _onOpenOffSeason = onOpenOffSeason;
            _onOpenHome = onOpenHome;
            _onOpenPhone = onOpenPhone;
            _onOpenFinances = onOpenFinances;
            _onOpenShop = onOpenShop;
            _onOpenSocial = onOpenSocial;
            _onOpenPress = onOpenPress;
            _onOpenTransferMarket = onOpenTransferMarket;
            _onOpenContinental = onOpenContinental;
            _onOpenSponsorship = onOpenSponsorship;
            _onOpenLegacy = onOpenLegacy;

            // ── Query ─────────────────────────────────────────────────────────
            _labelPlayerName   = root.Q<Label>("label-player-name");
            _labelClubPosition = root.Q<Label>("label-club-position");
            _labelAvatarInitial = root.Q<Label>("label-avatar-initial");
            _labelOvr          = root.Q<Label>("label-ovr");
            _labelDate         = root.Q<Label>("label-date");
            _labelDay          = root.Q<Label>("label-day");

            _fillEnergy  = root.Q<VisualElement>("fill-energy");
            _fillForm    = root.Q<VisualElement>("fill-form");
            _fillMorale  = root.Q<VisualElement>("fill-morale");
            _fillTrust   = root.Q<VisualElement>("fill-trust");
            _valEnergy   = root.Q<Label>("val-energy");
            _valForm     = root.Q<Label>("val-form");
            _valMorale   = root.Q<Label>("val-morale");
            _valTrust    = root.Q<Label>("val-trust");

            _labelWage    = root.Q<Label>("label-wage");
            _labelBalance = root.Q<Label>("label-balance");

            _cardNextMatch        = root.Q<VisualElement>("card-next-match");
            _labelMatchCountdown  = root.Q<Label>("label-match-countdown");
            _labelMatchHome       = root.Q<Label>("label-match-home");
            _labelMatchOpponent   = root.Q<Label>("label-match-opponent");
            _labelMatchCompetition= root.Q<Label>("label-match-competition");

            _labelStatus  = root.Q<Label>("label-status");

            _btnTrain       = root.Q<Button>("btn-train");
            _btnRest        = root.Q<Button>("btn-rest");
            _btnMatch       = root.Q<Button>("btn-match");
            _btnCareer      = root.Q<Button>("btn-career");
            _btnOffSeason   = root.Q<Button>("btn-offseason");
            _btnHome        = root.Q<Button>("btn-home");
            _btnQuickPhone  = root.Q<Button>("btn-quick-phone");
            _btnQuickFinances = root.Q<Button>("btn-quick-finances");
            _btnQuickShop   = root.Q<Button>("btn-quick-shop");
            _btnQuickMarket = root.Q<Button>("btn-quick-market");
            _btnFinances    = root.Q<Button>("btn-finances");
            _btnShop        = root.Q<Button>("btn-shop");
            _btnTransferMarket = root.Q<Button>("btn-transfer-market");
            _btnContinental    = root.Q<Button>("btn-continental");
            var btnSponsorship = root.Q<Button>("btn-sponsorship");
            var btnLegacy      = root.Q<Button>("btn-legacy");
            var btnQuickSponsorship = root.Q<Button>("btn-quick-sponsorship");
            var btnQuickLegacy = root.Q<Button>("btn-quick-legacy");
            var btnQuickOutings = root.Q<Button>("btn-quick-outings");
            var btnQuickPress   = root.Q<Button>("btn-quick-press");
            var btnSocialOutings= root.Q<Button>("btn-social-outings");
            var btnPressBriefing= root.Q<Button>("btn-press-briefing");
            _btnAdvanceDay  = root.Q<Button>("btn-advance-day");

            // ── Wire buttons ──────────────────────────────────────────────────
            _btnTrain.clicked      += () => _onOpenTraining?.Invoke();
            _btnRest.clicked       += () => _onOpenRest?.Invoke();
            _btnCareer.clicked     += () => _onOpenCareer?.Invoke();
            _btnMatch.clicked      += () => _onOpenMatch?.Invoke();
            if (_btnOffSeason != null)
                _btnOffSeason.clicked += () => _onOpenOffSeason?.Invoke();
            if (_btnHome != null)
                _btnHome.clicked += () => _onOpenHome?.Invoke();
            if (_btnQuickPhone != null)
                _btnQuickPhone.clicked += () => _onOpenPhone?.Invoke();
            if (_btnQuickFinances != null)
                _btnQuickFinances.clicked += () => _onOpenFinances?.Invoke();
            if (_btnQuickShop != null)
                _btnQuickShop.clicked += () => _onOpenShop?.Invoke();
            if (_btnQuickMarket != null)
                _btnQuickMarket.clicked += () => _onOpenTransferMarket?.Invoke();
            if (btnQuickSponsorship != null)
                btnQuickSponsorship.clicked += () => _onOpenSponsorship?.Invoke();
            if (btnQuickLegacy != null)
                btnQuickLegacy.clicked += () => _onOpenLegacy?.Invoke();
            if (btnQuickOutings != null)
                btnQuickOutings.clicked += () => _onOpenSocial?.Invoke();
            if (btnQuickPress != null)
                btnQuickPress.clicked += () => _onOpenPress?.Invoke();
            if (_btnFinances != null)
                _btnFinances.clicked += () => _onOpenFinances?.Invoke();
            if (_btnShop != null)
                _btnShop.clicked += () => _onOpenShop?.Invoke();
            if (_btnTransferMarket != null)
                _btnTransferMarket.clicked += () => _onOpenTransferMarket?.Invoke();
            if (_btnContinental != null)
                _btnContinental.clicked += () => _onOpenContinental?.Invoke();
            if (btnSponsorship != null)
                btnSponsorship.clicked += () => _onOpenSponsorship?.Invoke();
            if (btnLegacy != null)
                btnLegacy.clicked += () => _onOpenLegacy?.Invoke();
            if (btnSocialOutings != null)
                btnSocialOutings.clicked += () => _onOpenSocial?.Invoke();
            if (btnPressBriefing != null)
                btnPressBriefing.clicked += () => _onOpenPress?.Invoke();

            var btnQuickLanguage = root.Q<Button>("btn-quick-language");
            if (btnQuickLanguage != null)
            {
                void UpdateLanguageButtonLabel()
                {
                    var bridge = SimulationBridge.Instance;
                    if (bridge != null)
                    {
                        var info = LanguageInfo.FromLanguage(bridge.Localization.CurrentLanguage);
                        btnQuickLanguage.text = $"{info.FlagEmoji} {info.Code.ToUpperInvariant()}";
                    }
                }
                UpdateLanguageButtonLabel();

                btnQuickLanguage.clicked += () =>
                {
                    var bridge = SimulationBridge.Instance;
                    if (bridge != null)
                    {
                        var next = (GameLanguage)(((int)bridge.Localization.CurrentLanguage + 1) % 5);
                        bridge.SetLanguage(next);
                        UpdateLanguageButtonLabel();
                        RefreshLocalizedLabels();
                    }
                };
            }

            _btnAdvanceDay.clicked += OnAdvanceDayClicked;

            // Hide match card initially (no pending match)
            SetMatchCardVisible(false);
            RefreshLocalizedLabels();
        }

        public void RefreshLocalizedLabels()
        {
            var bridge = SimulationBridge.Instance;
            if (bridge == null) return;

            if (_btnTrain != null) _btnTrain.text = $"⚡ {bridge.T("nav.training")}";
            if (_btnRest != null) _btnRest.text = $"🛏️ {bridge.T("nav.rest")}";
            if (_btnMatch != null) _btnMatch.text = $"⚽ {bridge.T("nav.match")}";
            if (_btnCareer != null) _btnCareer.text = $"📋 {bridge.T("nav.overview")}";
            if (_btnShop != null) _btnShop.text = $"🛍️ {bridge.T("nav.shop")}";
            if (_btnTransferMarket != null) _btnTransferMarket.text = $"🤝 {bridge.T("nav.transfers")}";
        }

        // ── Public: Bind / Unbind bridge ─────────────────────────────────────
        public void Bind(SimulationBridge bridge)
        {
            Unbind();
            _bridge = bridge;
            _bridge.OnDayAdvanced      += OnDayAdvanced;
            _bridge.OnMatchOpportunity += OnMatchOpportunity;
            _bridge.OnLifeEventOccurred+= OnLifeEvent;
            _bridge.OnStatusLog        += OnStatusLog;
            _bridge.OnLanguageChanged  += _ => RefreshLocalizedLabels();
            RefreshIdentity();
            RefreshLocalizedLabels();
        }

        public void Unbind()
        {
            if (_bridge == null) return;
            _bridge.OnDayAdvanced       -= OnDayAdvanced;
            _bridge.OnMatchOpportunity  -= OnMatchOpportunity;
            _bridge.OnLifeEventOccurred -= OnLifeEvent;
            _bridge.OnStatusLog         -= OnStatusLog;
            _bridge = null;
        }

        // ── Private: Event Handlers ───────────────────────────────────────────
        private void OnDayAdvanced(SimulationDaySnapshot snap)
        {
            // Header
            _labelDate.text = $"Season {snap.Season} · Week {snap.Week}";
            string dayName = snap.DayOfWeek >= 1 && snap.DayOfWeek <= 7
                ? _dayNames[snap.DayOfWeek]
                : $"Day {snap.DayOfWeek}";
            _labelDay.text = dayName;
            _labelOvr.text = $"{snap.OverallRating} OVR";

            // Vital stat meters
            SetMeter(_fillEnergy, _valEnergy, snap.Energy);
            SetMeter(_fillForm,   _valForm,   snap.Form);
            SetMeter(_fillMorale, _valMorale, snap.Morale);
            SetMeter(_fillTrust,  _valTrust,  snap.ManagerTrust);

            // Finances from CurrentSave (bridge has the authoritative numbers)
            if (_bridge?.CurrentSave != null)
            {
                _labelWage.text    = $"£{_bridge.CurrentSave.WeeklyWage:N0}/wk";
                _labelBalance.text = $"£{snap.BankBalance:N0}";
            }

            // Status ticker
            if (!string.IsNullOrEmpty(snap.StatusMessage))
                _labelStatus.text = snap.StatusMessage;

            // Match button state (only enable on Saturday = day 6)
            bool isMatchDay = snap.DayOfWeek == 6;
            _btnMatch.SetEnabled(isMatchDay);
        }

        private void OnMatchOpportunity(MatchOpportunitySnapshot snap)
        {
            SetMatchCardVisible(true);
            _labelMatchHome.text        = (_bridge?.CurrentSave?.ClubName ?? "Your Club").ToUpperInvariant();
            _labelMatchOpponent.text    = snap.OpponentName.ToUpperInvariant();
            _labelMatchCompetition.text = snap.Competition;
            _labelMatchCountdown.text   = snap.IsHome ? "Today · Home" : "Today · Away";
        }

        private void OnLifeEvent(LifeEventSnapshot snap)
        {
            _labelStatus.text = $"📰 {snap.Title}: {snap.Description}";
            _onLifeEventPending?.Invoke();
        }

        private void OnStatusLog(string message)
        {
            // Only update the ticker if it's a meaningful non-day message
            if (!string.IsNullOrEmpty(message))
                _labelStatus.text = message;
        }

        private void OnAdvanceDayClicked()
        {
            _bridge?.AdvanceDay();
        }

        // ── Private: Identity ─────────────────────────────────────────────────
        public void RefreshIdentity()
        {
            var save = _bridge?.CurrentSave;
            if (save == null) return;

            _labelPlayerName.text   = save.PlayerName;
            _labelClubPosition.text = $"{save.ClubName} · {save.PrimaryPosition}";
            _labelAvatarInitial.text = save.PlayerName.Length > 0
                ? save.PlayerName[0].ToString().ToUpperInvariant()
                : "?";

            _labelWage.text    = $"£{save.WeeklyWage:N0}/wk";
            _labelBalance.text = $"£{save.BankBalance:N0}";
            _labelOvr.text     = $"{save.OverallRating} OVR";
        }

        // ── Private: Helpers ──────────────────────────────────────────────────
        private static void SetMeter(VisualElement fill, Label label, int value)
        {
            int clamped = Math.Clamp(value, 0, 100);
            fill.style.width = new StyleLength(new Length(clamped, LengthUnit.Percent));
            label.text = clamped.ToString();
        }

        private void SetMatchCardVisible(bool visible)
        {
            if (_cardNextMatch != null)
                _cardNextMatch.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
