using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation;
using UnityEngine;
using UnityEngine.UIElements;
using Position = FootballLife.Domain.Position;

namespace FootballLife.Unity.UI.Creation
{
    /// <summary>
    /// UI Toolkit controller managing the Player Creation view interactions, live attribute preview, and validation.
    /// </summary>
    public class PlayerCreationController : MonoBehaviour
    {
        [SerializeField] private UIDocument? _uiDocument;

        public event Action<PlayerCreationData>? OnPlayerCreated;

        private TextField? _firstNameField;
        private TextField? _lastNameField;
        private Button? _randomizeBtn;
        private DropdownField? _nationalityDropdown;
        private IntegerField? _kitNumberField;

        private Button? _footLeftBtn;
        private Button? _footRightBtn;
        private Button? _footBothBtn;

        private readonly Dictionary<Position, Button> _positionButtons = new();
        private Position _selectedPosition = Position.ST;
        private Foot _selectedFoot = Foot.Right;

        // Preview Elements
        private Label? _previewOvrLabel;
        private Label? _positionDescLabel;
        private Label? _valShooting;
        private Label? _valPace;
        private Label? _valDribbling;
        private Label? _valPassing;
        private Label? _valTackling;
        private VisualElement? _fillShooting;
        private VisualElement? _fillPace;
        private VisualElement? _fillDribbling;
        private VisualElement? _fillPassing;
        private VisualElement? _fillTackling;

        private Label? _errorLabel;
        private Button? _proceedBtn;

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

        public void BindVisualElements(VisualElement root)
        {
            if (root == null) return;

            _firstNameField = root.Q<TextField>("input-firstname");
            _lastNameField = root.Q<TextField>("input-lastname");
            _randomizeBtn = root.Q<Button>("btn-randomize-name");
            _nationalityDropdown = root.Q<DropdownField>("dropdown-nationality");
            _kitNumberField = root.Q<IntegerField>("input-kit-number");

            _footLeftBtn = root.Q<Button>("btn-foot-left");
            _footRightBtn = root.Q<Button>("btn-foot-right");
            _footBothBtn = root.Q<Button>("btn-foot-both");

            _previewOvrLabel = root.Q<Label>("label-preview-ovr");
            _positionDescLabel = root.Q<Label>("label-position-desc");
            _valShooting = root.Q<Label>("val-shooting");
            _valPace = root.Q<Label>("val-pace");
            _valDribbling = root.Q<Label>("val-dribbling");
            _valPassing = root.Q<Label>("val-passing");
            _valTackling = root.Q<Label>("val-tackling");

            _fillShooting = root.Q<VisualElement>("fill-shooting");
            _fillPace = root.Q<VisualElement>("fill-pace");
            _fillDribbling = root.Q<VisualElement>("fill-dribbling");
            _fillPassing = root.Q<VisualElement>("fill-passing");
            _fillTackling = root.Q<VisualElement>("fill-tackling");

            _errorLabel = root.Q<Label>("label-error");
            _proceedBtn = root.Q<Button>("btn-proceed");

            // Register Foot Callbacks
            _footLeftBtn?.RegisterCallback<ClickEvent>(_ => SetPreferredFoot(Foot.Left));
            _footRightBtn?.RegisterCallback<ClickEvent>(_ => SetPreferredFoot(Foot.Right));
            _footBothBtn?.RegisterCallback<ClickEvent>(_ => SetPreferredFoot(Foot.Both));

            // Register Position Callbacks
            _positionButtons.Clear();
            foreach (Position pos in Enum.GetValues(typeof(Position)))
            {
                var btn = root.Q<Button>($"btn-pos-{pos}");
                if (btn != null)
                {
                    _positionButtons[pos] = btn;
                    btn.RegisterCallback<ClickEvent>(_ => SetPosition(pos));
                }
            }

            _randomizeBtn?.RegisterCallback<ClickEvent>(_ => RandomizeName());
            _proceedBtn?.RegisterCallback<ClickEvent>(_ => HandleProceed());

            // Initial view state setup
            UpdatePositionHighlights();
            UpdateLiveAttributePreview();
        }

        private void SetPreferredFoot(Foot foot)
        {
            _selectedFoot = foot;
            _footLeftBtn?.EnableInClassList("btn-primary", foot == Foot.Left);
            _footLeftBtn?.EnableInClassList("btn-secondary", foot != Foot.Left);

            _footRightBtn?.EnableInClassList("btn-primary", foot == Foot.Right);
            _footRightBtn?.EnableInClassList("btn-secondary", foot != Foot.Right);

            _footBothBtn?.EnableInClassList("btn-primary", foot == Foot.Both);
            _footBothBtn?.EnableInClassList("btn-secondary", foot != Foot.Both);
        }

        private void SetPosition(Position position)
        {
            _selectedPosition = position;
            UpdatePositionHighlights();
            UpdateLiveAttributePreview();
        }

        private void UpdatePositionHighlights()
        {
            foreach (var kvp in _positionButtons)
            {
                bool isSelected = kvp.Key == _selectedPosition;
                kvp.Value.EnableInClassList("btn-primary", isSelected);
                kvp.Value.EnableInClassList("btn-secondary", !isSelected);
            }
        }

        private void UpdateLiveAttributePreview()
        {
            var abilities = StartingAttributeCalculator.CalculateStartingAbilities(_selectedPosition, varianceSeed: 42);
            int ovr = StartingAttributeCalculator.CalculateOverall(_selectedPosition, abilities);

            if (_previewOvrLabel != null)
            {
                _previewOvrLabel.text = $"{ovr} OVR";
            }

            if (_positionDescLabel != null)
            {
                _positionDescLabel.text = GetPositionDescription(_selectedPosition);
            }

            int shooting = abilities.Shooting;
            int pace = (abilities.Pace + abilities.Acceleration) / 2;
            int dribbling = abilities.Dribbling;
            int passing = abilities.Passing;
            int tackling = abilities.Tackling;

            SetBar(_valShooting, _fillShooting, shooting);
            SetBar(_valPace, _fillPace, pace);
            SetBar(_valDribbling, _fillDribbling, dribbling);
            SetBar(_valPassing, _fillPassing, passing);
            SetBar(_valTackling, _fillTackling, tackling);
        }

        private static void SetBar(Label? label, VisualElement? fill, int value)
        {
            if (label != null) label.text = value.ToString();
            if (fill != null) fill.style.width = new StyleLength(Length.Percent(value));
        }

        private static string GetPositionDescription(Position pos) => pos switch
        {
            Position.ST => "Striker • Key Focus: Shooting, Pace & Composure",
            Position.LW => "Left Wing • Key Focus: Pace, Dribbling & Crossing",
            Position.RW => "Right Wing • Key Focus: Pace, Dribbling & Crossing",
            Position.AM => "Attacking Midfielder • Key Focus: Vision, Dribbling & Passing",
            Position.CM => "Central Midfielder • Key Focus: Passing, Stamina & Decision Making",
            Position.DM => "Defensive Midfielder • Key Focus: Tackling, Positioning & Stamina",
            Position.FB => "Fullback • Key Focus: Pace, Tackling & Stamina",
            Position.CB => "Center Back • Key Focus: Tackling, Strength & Positioning",
            Position.GK => "Goalkeeper • Key Focus: Positioning, Agility & Composure",
            _ => "Footballer"
        };

        private void RandomizeName()
        {
            string nationality = _nationalityDropdown?.value ?? "England";
            var (firstName, lastName) = RandomNameGenerator.GenerateFullName(nationality);

            if (_firstNameField != null) _firstNameField.value = firstName;
            if (_lastNameField != null) _lastNameField.value = lastName;
        }

        private void HandleProceed()
        {
            string first = _firstNameField?.value?.Trim() ?? "";
            string last = _lastNameField?.value?.Trim() ?? "";
            string nationality = _nationalityDropdown?.value ?? "England";
            int kit = _kitNumberField?.value ?? 9;

            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
            {
                if (_errorLabel != null)
                {
                    _errorLabel.text = "Please enter both a first and last name.";
                    _errorLabel.style.display = DisplayStyle.Flex;
                }
                return;
            }

            if (_errorLabel != null)
            {
                _errorLabel.style.display = DisplayStyle.None;
            }

            var creationData = new PlayerCreationData
            {
                FirstName = first,
                LastName = last,
                Nationality = nationality,
                PrimaryPosition = _selectedPosition,
                PreferredFoot = _selectedFoot,
                KitNumber = kit
            };

            OnPlayerCreated?.Invoke(creationData);
        }
    }
}
