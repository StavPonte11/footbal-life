using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Creation
{
    /// <summary>
    /// UI Toolkit controller managing the Starting Club Selection cards and contract signing.
    /// </summary>
    public class ClubSelectionController : MonoBehaviour
    {
        [SerializeField] private UIDocument? _uiDocument;

        public event Action<ClubOfferData>? OnContractSigned;
        public event Action? OnBackRequested;

        private IReadOnlyList<ClubOfferData> _offers = ClubOfferData.GetDefaultStarterOffers();
        private int _selectedIndex = 0;

        private readonly List<VisualElement> _cards = new();
        private readonly List<Label> _badges = new();
        private Button? _backBtn;
        private Button? _signBtn;

        private void OnEnable()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            if (_uiDocument != null)
            {
                BindVisualElements(_uiDocument.rootVisualElement);
            }
        }

        public void SetOffers(IReadOnlyList<ClubOfferData> offers)
        {
            if (offers != null && offers.Count > 0)
            {
                _offers = offers;
                if (_uiDocument != null)
                {
                    BindVisualElements(_uiDocument.rootVisualElement);
                }
            }
        }

        public void BindVisualElements(VisualElement root)
        {
            if (root == null) return;

            _cards.Clear();
            _badges.Clear();

            for (int i = 0; i < 3; i++)
            {
                var card = root.Q<VisualElement>($"card-offer-{i}");
                var badge = root.Q<Label>($"badge-offer-{i}");
                var title = root.Q<Label>($"title-offer-{i}");
                var league = root.Q<Label>($"league-offer-{i}");
                var wage = root.Q<Label>($"wage-offer-{i}");
                var role = root.Q<Label>($"role-offer-{i}");
                var length = root.Q<Label>($"length-offer-{i}");
                var trust = root.Q<Label>($"trust-offer-{i}");

                if (card != null && i < _offers.Count)
                {
                    var offer = _offers[i];
                    if (title != null) title.text = offer.ClubName;
                    if (league != null) league.text = $"{offer.Division} • Stadium: {offer.Stadium}";
                    if (wage != null) wage.text = $"£{offer.WeeklyWage:N0} / week";
                    if (role != null) role.text = offer.SquadRole;
                    if (length != null) length.text = $"{offer.ContractYears} Years";
                    if (trust != null) trust.text = $"{offer.InitialManagerTrust}%";

                    int index = i;
                    card.RegisterCallback<ClickEvent>(_ => SelectCard(index));
                    _cards.Add(card);
                }

                if (badge != null)
                {
                    _badges.Add(badge);
                }
            }

            _backBtn = root.Q<Button>("btn-back");
            _signBtn = root.Q<Button>("btn-sign-contract");

            _backBtn?.RegisterCallback<ClickEvent>(_ => OnBackRequested?.Invoke());
            _signBtn?.RegisterCallback<ClickEvent>(_ => HandleSignContract());

            UpdateCardHighlights();
        }

        private void SelectCard(int index)
        {
            if (index < 0 || index >= _offers.Count) return;
            _selectedIndex = index;
            UpdateCardHighlights();
        }

        private void UpdateCardHighlights()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                bool isSelected = (i == _selectedIndex);
                var card = _cards[i];

                if (isSelected)
                {
                    SetBorderColor(card, new Color(0.06f, 0.73f, 0.51f)); // Pitch Green #10B981
                    card.EnableInClassList("card-elevated", true);
                    card.EnableInClassList("card", false);
                }
                else
                {
                    SetBorderColor(card, new Color(0.20f, 0.25f, 0.33f)); // Border #334155
                    card.EnableInClassList("card-elevated", false);
                    card.EnableInClassList("card", true);
                }

                if (i < _badges.Count)
                {
                    var badge = _badges[i];
                    badge.text = isSelected ? "SELECTED" : "OFFER";
                    badge.EnableInClassList("badge-green", isSelected);
                    badge.EnableInClassList("badge-slate", !isSelected);
                }
            }
        }

        private static void SetBorderColor(VisualElement el, Color color)
        {
            var styleColor = new StyleColor(color);
            el.style.borderTopColor = styleColor;
            el.style.borderRightColor = styleColor;
            el.style.borderBottomColor = styleColor;
            el.style.borderLeftColor = styleColor;
        }

        private void HandleSignContract()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _offers.Count)
            {
                OnContractSigned?.Invoke(_offers[_selectedIndex]);
            }
        }
    }
}
