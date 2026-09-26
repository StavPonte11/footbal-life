#nullable enable
using System;
using UnityEngine.UIElements;

namespace FootballLife.Unity.UI.Common
{
    /// <summary>
    /// Reusable controller managing the three universal UI states across all game screens:
    ///   1. Empty state   — no data yet (no transfer offers, empty sponsor list, etc.)
    ///   2. Loading state — async operation in progress
    ///   3. Error state   — failure, offline, cloud-save conflict
    ///
    /// Milestone 7.6 (#P7-602): Cohesive empty / loading / error state system.
    ///
    /// Usage: Construct with a root VisualElement that contains
    ///   - An element with name "container-empty"   (houses .empty-state children)
    ///   - An element with name "container-content" (the normal populated view)
    ///   - An element with name "error-banner"      (top-of-screen error strip)
    ///
    /// Then call ShowEmpty/ShowLoading/ShowError/ShowContent to switch modes.
    /// All states are mutually exclusive.
    /// </summary>
    public sealed class EmptyStateController
    {
        public event Action? OnEmptyActionClicked;
        public event Action? OnErrorRetryClicked;

        // Containers
        private readonly VisualElement? _containerEmpty;
        private readonly VisualElement? _containerContent;
        private readonly VisualElement? _errorBanner;

        // Empty state elements
        private readonly Label? _emptyIcon;
        private readonly Label? _emptyTitle;
        private readonly Label? _emptyBody;
        private readonly Button? _emptyAction;

        // Error banner elements
        private readonly Label? _errorBannerIcon;
        private readonly Label? _errorBannerText;
        private readonly Button? _errorBannerRetry;

        public EmptyStateController(VisualElement root)
        {
            _containerEmpty = root.Q<VisualElement>("container-empty");
            _containerContent = root.Q<VisualElement>("container-content");
            _errorBanner = root.Q<VisualElement>("error-banner");

            if (_containerEmpty != null)
            {
                _emptyIcon = _containerEmpty.Q<Label>("empty-state-icon");
                _emptyTitle = _containerEmpty.Q<Label>("empty-state-title");
                _emptyBody = _containerEmpty.Q<Label>("empty-state-body");
                _emptyAction = _containerEmpty.Q<Button>("btn-empty-action");

                if (_emptyAction != null)
                {
                    _emptyAction.clicked -= HandleEmptyActionClicked;
                    _emptyAction.clicked += HandleEmptyActionClicked;
                }
            }

            if (_errorBanner != null)
            {
                _errorBannerIcon = _errorBanner.Q<Label>("error-banner-icon");
                _errorBannerText = _errorBanner.Q<Label>("error-banner-text");
                _errorBannerRetry = _errorBanner.Q<Button>("btn-error-retry");

                if (_errorBannerRetry != null)
                {
                    _errorBannerRetry.clicked -= HandleErrorRetryClicked;
                    _errorBannerRetry.clicked += HandleErrorRetryClicked;
                }

                _errorBanner.style.display = DisplayStyle.None;
            }

            // Default: hide empty state until explicitly requested
            SetContainerEmptyVisible(false);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Shows the normal populated content view.
        /// Hides empty state and dismisses any error banner.
        /// </summary>
        public void ShowContent()
        {
            SetContainerEmptyVisible(false);
            SetContainerContentVisible(true);
            SetErrorBannerVisible(false);
        }

        /// <summary>
        /// Shows the empty-state panel with contextual icon, heading, body, and optional CTA.
        /// </summary>
        /// <param name="icon">Emoji or text glyph (e.g. "📭", "⚽")</param>
        /// <param name="title">Bold heading (e.g. "No Transfer Offers Yet")</param>
        /// <param name="body">Explanatory sentence (e.g. "Improve your reputation to attract clubs.")</param>
        /// <param name="actionLabel">Optional CTA button text. Pass null to hide the button.</param>
        public void ShowEmpty(string icon, string title, string body, string? actionLabel = null)
        {
            SetContainerContentVisible(false);
            SetContainerEmptyVisible(true);
            SetErrorBannerVisible(false);

            if (_emptyIcon != null) _emptyIcon.text = icon;
            if (_emptyTitle != null) _emptyTitle.text = title;
            if (_emptyBody != null) _emptyBody.text = body;

            if (_emptyAction != null)
            {
                bool hasAction = !string.IsNullOrEmpty(actionLabel);
                _emptyAction.style.display = hasAction ? DisplayStyle.Flex : DisplayStyle.None;
                if (hasAction) _emptyAction.text = actionLabel!;
            }
        }

        /// <summary>
        /// Shows a loading spinner/placeholder state.
        /// Hides both content and empty-state panels.
        /// </summary>
        public void ShowLoading()
        {
            SetContainerContentVisible(false);
            ShowEmpty("⏳", "Loading...", "Please wait a moment.", null);
        }

        /// <summary>
        /// Displays a non-blocking error banner at the top of the screen
        /// while keeping the underlying content visible.
        /// </summary>
        /// <param name="icon">Emoji prefix (e.g. "⚠️", "📡")</param>
        /// <param name="message">Short error description.</param>
        /// <param name="retryLabel">Retry button text. Pass null to hide the retry button.</param>
        public void ShowError(string icon, string message, string? retryLabel = "Retry")
        {
            SetErrorBannerVisible(true);

            if (_errorBannerIcon != null) _errorBannerIcon.text = icon;
            if (_errorBannerText != null) _errorBannerText.text = message;

            if (_errorBannerRetry != null)
            {
                bool hasRetry = !string.IsNullOrEmpty(retryLabel);
                _errorBannerRetry.style.display = hasRetry ? DisplayStyle.Flex : DisplayStyle.None;
                if (hasRetry) _errorBannerRetry.text = retryLabel!;
            }
        }

        /// <summary>Hides the error banner without changing content/empty state.</summary>
        public void DismissError()
        {
            SetErrorBannerVisible(false);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Convenience factory methods for common empty-state contexts (#P7-602)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>No transfer offers available yet.</summary>
        public void ShowNoTransferOffers() =>
            ShowEmpty("📭", "No Transfer Offers", "Build your reputation and keep performing to attract clubs.", "Improve Training");

        /// <summary>No active sponsorship deals.</summary>
        public void ShowNoSponsors() =>
            ShowEmpty("🤝", "No Active Sponsors", "Raise your profile to unlock sponsorship opportunities.", null);

        /// <summary>No trophies earned yet.</summary>
        public void ShowNoTrophies() =>
            ShowEmpty("🏆", "No Trophies Yet", "Win your first title and it will appear here.", null);

        /// <summary>No media headlines available.</summary>
        public void ShowNoMediaHeadlines() =>
            ShowEmpty("📰", "Quiet in the Press", "Get on the pitch and make your mark — the media will follow.", null);

        /// <summary>Cloud save offline / unavailable.</summary>
        public void ShowCloudSaveError() =>
            ShowError("📡", "Cloud save unavailable. Your progress is saved locally.", "Retry");

        /// <summary>Cloud save conflict detected.</summary>
        public void ShowCloudSaveConflict() =>
            ShowError("⚠️", "Save conflict detected. Choose which save to keep.", "Resolve");

        /// <summary>
        /// Creates a standalone VisualElement matching the universal empty-state pattern
        /// that can be dynamically inserted into any list, container, or card (#P7-602).
        /// </summary>
        public static VisualElement CreateEmptyStateElement(
            string icon,
            string title,
            string body,
            string? actionLabel = null,
            Action? onAction = null)
        {
            var container = new VisualElement();
            container.AddToClassList("empty-state");

            var iconLabel = new Label(icon);
            iconLabel.name = "empty-state-icon";
            iconLabel.AddToClassList("empty-state__icon");
            container.Add(iconLabel);

            var titleLabel = new Label(title);
            titleLabel.name = "empty-state-title";
            titleLabel.AddToClassList("empty-state__title");
            container.Add(titleLabel);

            var bodyLabel = new Label(body);
            bodyLabel.name = "empty-state-body";
            bodyLabel.AddToClassList("empty-state__body");
            container.Add(bodyLabel);

            if (!string.IsNullOrEmpty(actionLabel))
            {
                var actionBtn = new Button(() => onAction?.Invoke())
                {
                    text = actionLabel,
                    name = "btn-empty-action"
                };
                actionBtn.AddToClassList("btn");
                actionBtn.AddToClassList("btn-secondary");
                actionBtn.AddToClassList("empty-state__action");
                container.Add(actionBtn);
            }

            return container;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        private void SetContainerEmptyVisible(bool visible)
        {
            if (_containerEmpty != null)
                _containerEmpty.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetContainerContentVisible(bool visible)
        {
            if (_containerContent != null)
                _containerContent.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetErrorBannerVisible(bool visible)
        {
            if (_errorBanner != null)
                _errorBanner.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void HandleEmptyActionClicked()
        {
            OnEmptyActionClicked?.Invoke();
        }

        private void HandleErrorRetryClicked()
        {
            DismissError();
            OnErrorRetryClicked?.Invoke();
        }
    }
}
