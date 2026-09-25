#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using FootballLife.Domain;
using FootballLife.Simulation;
using FootballLife.Simulation.Persistence;
using FootballLife.Unity.Core.Bridge;

namespace FootballLife.Unity.UI.Phone
{
    public enum PhoneAppTab
    {
        Messages,
        Social,
        Contacts,
        News
    }

    /// <summary>
    /// UI Toolkit presentation controller for the in-game Smartphone OS (#P4-003)
    /// and people-centric Relationship Hub (#P4-004).
    /// </summary>
    public class PhoneOSController : MonoBehaviour
    {
        [Header("State")]
        [SerializeField] private PhoneAppTab _currentTab = PhoneAppTab.Messages;

        private VisualElement? _backdrop;
        private Button? _btnClose;
        private Button? _btnHomeIndicator;

        // Dock buttons
        private Button? _btnDockMessages;
        private Button? _btnDockSocial;
        private Button? _btnDockContacts;
        private Button? _btnDockNews;

        // App Screen Containers
        private VisualElement? _screenMessages;
        private VisualElement? _screenSocial;
        private VisualElement? _screenContacts;
        private VisualElement? _screenNews;

        // Messages App Elements
        private VisualElement? _containerThreads;
        private VisualElement? _cardActiveChat;
        private Label? _chatSender;
        private Label? _chatRole;
        private Label? _chatIncomingBubble;
        private VisualElement? _containerChoices;
        private Label? _labelReplyFeedback;

        // Social App Elements
        private VisualElement? _containerSocialPosts;

        // Contacts App Elements
        private VisualElement? _containerContacts;

        // News App Elements
        private VisualElement? _containerNews;

        // Data caches
        private List<Relationship> _contacts = new List<Relationship>();
        private List<PhoneMessage> _messages = new List<PhoneMessage>();
        private List<SocialPost> _socialPosts = new List<SocialPost>();
        private List<SocialPost> _newsArticles = new List<SocialPost>();
        private PhoneMessage? _selectedMessage;

        public event Action? OnPhoneClosed;

        public void BindRoot(VisualElement root)
        {
            if (root == null) return;

            _backdrop = root.Q<VisualElement>("phone-os-backdrop");
            _btnClose = root.Q<Button>("btn-phone-close");
            _btnHomeIndicator = root.Q<Button>("btn-phone-home-indicator");

            _btnDockMessages = root.Q<Button>("btn-dock-messages");
            _btnDockSocial = root.Q<Button>("btn-dock-social");
            _btnDockContacts = root.Q<Button>("btn-dock-contacts");
            _btnDockNews = root.Q<Button>("btn-dock-news");

            _screenMessages = root.Q<VisualElement>("screen-app-messages");
            _screenSocial = root.Q<VisualElement>("screen-app-social");
            _screenContacts = root.Q<VisualElement>("screen-app-contacts");
            _screenNews = root.Q<VisualElement>("screen-app-news");

            _containerThreads = root.Q<VisualElement>("container-messages-threads");
            _cardActiveChat = root.Q<VisualElement>("card-active-chat");
            _chatSender = root.Q<Label>("chat-active-sender");
            _chatRole = root.Q<Label>("chat-active-role");
            _chatIncomingBubble = root.Q<Label>("chat-incoming-bubble");
            _containerChoices = root.Q<VisualElement>("container-reply-choices");
            _labelReplyFeedback = root.Q<Label>("label-reply-feedback");

            _containerSocialPosts = root.Q<VisualElement>("container-social-posts");
            _containerContacts = root.Q<VisualElement>("container-contacts-cards");
            _containerNews = root.Q<VisualElement>("container-news-cards");

            // Wire events
            if (_btnClose != null) _btnClose.clicked += ClosePhone;
            if (_btnHomeIndicator != null) _btnHomeIndicator.clicked += ClosePhone;

            if (_btnDockMessages != null) _btnDockMessages.clicked += () => SwitchTab(PhoneAppTab.Messages);
            if (_btnDockSocial != null) _btnDockSocial.clicked += () => SwitchTab(PhoneAppTab.Social);
            if (_btnDockContacts != null) _btnDockContacts.clicked += () => SwitchTab(PhoneAppTab.Contacts);
            if (_btnDockNews != null) _btnDockNews.clicked += () => SwitchTab(PhoneAppTab.News);

            InitData();
            SwitchTab(PhoneAppTab.Messages);
        }

        public void OpenPhone()
        {
            if (_backdrop != null)
            {
                _backdrop.style.display = DisplayStyle.Flex;
                InitData();
                SwitchTab(_currentTab);
            }
        }

        public void ClosePhone()
        {
            if (_backdrop != null)
            {
                _backdrop.style.display = DisplayStyle.None;
            }
            OnPhoneClosed?.Invoke();
        }

        private void InitData()
        {
            var bridge = SimulationBridge.Instance;
            var save = bridge?.CurrentSave;
            var playerId = save?.PlayerId ?? Guid.NewGuid();

            if (_contacts.Count == 0)
            {
                _contacts = PhoneSystem.CreateDefaultContacts(playerId);
            }

            if (save != null && _messages.Count == 0)
            {
                _messages = PhoneSystem.GenerateMessages(save, _contacts);
            }

            if (save != null && _socialPosts.Count == 0)
            {
                _socialPosts = PhoneSystem.GenerateSocialPosts(save);
            }

            if (save != null && _newsArticles.Count == 0)
            {
                _newsArticles = PhoneSystem.GenerateNewsArticles(save);
            }
        }

        public void SwitchTab(PhoneAppTab tab)
        {
            _currentTab = tab;

            // Hide all screens
            if (_screenMessages != null) _screenMessages.style.display = DisplayStyle.None;
            if (_screenSocial != null) _screenSocial.style.display = DisplayStyle.None;
            if (_screenContacts != null) _screenContacts.style.display = DisplayStyle.None;
            if (_screenNews != null) _screenNews.style.display = DisplayStyle.None;

            // Reset dock colors
            ResetDockButton(_btnDockMessages);
            ResetDockButton(_btnDockSocial);
            ResetDockButton(_btnDockContacts);
            ResetDockButton(_btnDockNews);

            switch (tab)
            {
                case PhoneAppTab.Messages:
                    if (_screenMessages != null) _screenMessages.style.display = DisplayStyle.Flex;
                    HighlightDockButton(_btnDockMessages, new Color(0.06f, 0.73f, 0.51f)); // #10B981
                    RefreshMessagesUI();
                    break;

                case PhoneAppTab.Social:
                    if (_screenSocial != null) _screenSocial.style.display = DisplayStyle.Flex;
                    HighlightDockButton(_btnDockSocial, new Color(0.96f, 0.25f, 0.37f)); // #F43F5E
                    RefreshSocialUI();
                    break;

                case PhoneAppTab.Contacts:
                    if (_screenContacts != null) _screenContacts.style.display = DisplayStyle.Flex;
                    HighlightDockButton(_btnDockContacts, new Color(0.22f, 0.74f, 0.97f)); // #38BDF8
                    RefreshContactsUI();
                    break;

                case PhoneAppTab.News:
                    if (_screenNews != null) _screenNews.style.display = DisplayStyle.Flex;
                    HighlightDockButton(_btnDockNews, new Color(0.96f, 0.62f, 0.04f)); // #F59E0B
                    RefreshNewsUI();
                    break;
            }
        }

        private static void ResetDockButton(Button? btn)
        {
            if (btn == null) return;
            btn.style.backgroundColor = new Color(0.12f, 0.16f, 0.23f); // #1E293B
        }

        private static void HighlightDockButton(Button? btn, Color color)
        {
            if (btn == null) return;
            btn.style.backgroundColor = color;
        }

        // ── 1. Messages App (WhatsApp) ────────────────────────────────────────
        private void RefreshMessagesUI()
        {
            if (_containerThreads == null) return;
            _containerThreads.Clear();

            foreach (var msg in _messages)
            {
                var threadCard = new Button();
                threadCard.style.flexDirection = FlexDirection.Row;
                threadCard.style.alignItems = Align.Center;
                threadCard.style.justifyContent = Justify.SpaceBetween;
                threadCard.style.backgroundColor = new Color(0.07f, 0.11f, 0.18f); // #131C2E
                threadCard.style.borderTopLeftRadius = 10;
                threadCard.style.borderTopRightRadius = 10;
                threadCard.style.borderBottomLeftRadius = 10;
                threadCard.style.borderBottomRightRadius = 10;
                threadCard.style.paddingTop = 8;
                threadCard.style.paddingBottom = 8;
                threadCard.style.paddingLeft = 10;
                threadCard.style.paddingRight = 10;
                threadCard.style.marginBottom = 6;
                SetBorder(threadCard.style, 1f, msg.IsRead ? new Color(0.12f, 0.16f, 0.23f) : new Color(0.06f, 0.73f, 0.51f));

                var infoBox = new VisualElement();
                infoBox.style.flexDirection = FlexDirection.Column;

                var senderLabel = new Label(msg.SenderName);
                senderLabel.style.color = new Color(0.97f, 0.98f, 0.99f);
                senderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                senderLabel.style.fontSize = 12;

                var previewLabel = new Label(msg.MessageText.Length > 36 ? msg.MessageText.Substring(0, 36) + "..." : msg.MessageText);
                previewLabel.style.color = new Color(0.58f, 0.64f, 0.72f);
                previewLabel.style.fontSize = 10;

                infoBox.Add(senderLabel);
                infoBox.Add(previewLabel);

                var timeBox = new VisualElement();
                timeBox.style.alignItems = Align.FlexEnd;

                var timeLabel = new Label(msg.Timestamp);
                timeLabel.style.color = new Color(0.39f, 0.45f, 0.55f);
                timeLabel.style.fontSize = 9;

                timeBox.Add(timeLabel);

                if (!msg.IsRead)
                {
                    var unreadBadge = new Label("NEW");
                    unreadBadge.style.color = new Color(0.06f, 0.73f, 0.51f);
                    unreadBadge.style.fontSize = 9;
                    unreadBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
                    timeBox.Add(unreadBadge);
                }

                threadCard.Add(infoBox);
                threadCard.Add(timeBox);

                var targetMsg = msg;
                threadCard.clicked += () => SelectMessageThread(targetMsg);

                _containerThreads.Add(threadCard);
            }

            if (_selectedMessage == null && _messages.Count > 0)
            {
                SelectMessageThread(_messages[0]);
            }
        }

        private void SelectMessageThread(PhoneMessage message)
        {
            _selectedMessage = message;

            if (_chatSender != null) _chatSender.text = message.SenderName;
            if (_chatRole != null) _chatRole.text = message.SenderType.ToString();
            if (_chatIncomingBubble != null) _chatIncomingBubble.text = message.MessageText;

            if (_labelReplyFeedback != null)
            {
                _labelReplyFeedback.style.display = DisplayStyle.None;
                _labelReplyFeedback.text = "";
            }

            if (_containerChoices == null) return;
            _containerChoices.Clear();

            if (message.SelectedChoiceIndex.HasValue)
            {
                int idx = message.SelectedChoiceIndex.Value;
                if (idx >= 0 && idx < message.Choices.Count)
                {
                    var chosen = message.Choices[idx];
                    var sentBubble = new Label($"You: {chosen.ChoiceText}");
                    sentBubble.style.color = new Color(0.97f, 0.98f, 0.99f);
                    sentBubble.style.fontSize = 11;
                    sentBubble.style.backgroundColor = new Color(0.06f, 0.73f, 0.51f, 0.25f);
                    sentBubble.style.paddingTop = 6;
                    sentBubble.style.paddingBottom = 6;
                    sentBubble.style.paddingLeft = 8;
                    sentBubble.style.paddingRight = 8;
                    sentBubble.style.borderTopLeftRadius = 8;
                    sentBubble.style.borderTopRightRadius = 8;
                    sentBubble.style.borderBottomLeftRadius = 8;
                    sentBubble.style.borderBottomRightRadius = 8;
                    sentBubble.style.marginBottom = 4;
                    _containerChoices.Add(sentBubble);

                    if (_labelReplyFeedback != null)
                    {
                        _labelReplyFeedback.style.display = DisplayStyle.Flex;
                        _labelReplyFeedback.text = $"{message.SenderName}: {chosen.ReplyText}";
                    }
                }
                return;
            }

            for (int i = 0; i < message.Choices.Count; i++)
            {
                var choice = message.Choices[i];
                var btnChoice = new Button();
                btnChoice.text = $"💬 {choice.ChoiceText}";
                btnChoice.style.fontSize = 11;
                btnChoice.style.color = new Color(0.97f, 0.98f, 0.99f);
                btnChoice.style.backgroundColor = new Color(0.12f, 0.16f, 0.23f);
                btnChoice.style.paddingTop = 6;
                btnChoice.style.paddingBottom = 6;
                btnChoice.style.marginBottom = 4;
                btnChoice.style.borderTopLeftRadius = 6;
                btnChoice.style.borderTopRightRadius = 6;
                btnChoice.style.borderBottomLeftRadius = 6;
                btnChoice.style.borderBottomRightRadius = 6;

                int capturedIndex = i;
                btnChoice.clicked += () =>
                {
                    var save = SimulationBridge.Instance?.CurrentSave;
                    if (save != null)
                    {
                        var rel = _contacts.Find(c => c.Id == message.SenderId);
                        string reply = PhoneSystem.ProcessMessageReply(message, capturedIndex, save, rel, out var updatedRel);
                        if (updatedRel != null)
                        {
                            int relIdx = _contacts.FindIndex(c => c.Id == updatedRel.Id);
                            if (relIdx >= 0) _contacts[relIdx] = updatedRel;
                        }

                        SelectMessageThread(message);
                        RefreshMessagesUI();
                    }
                };

                _containerChoices.Add(btnChoice);
            }
        }

        // ── 2. Social App (FootyGram) ─────────────────────────────────────────
        private void RefreshSocialUI()
        {
            if (_containerSocialPosts == null) return;
            _containerSocialPosts.Clear();

            foreach (var post in _socialPosts)
            {
                var postCard = new VisualElement();
                postCard.style.backgroundColor = new Color(0.07f, 0.11f, 0.18f); // #131C2E
                postCard.style.borderTopLeftRadius = 12;
                postCard.style.borderTopRightRadius = 12;
                postCard.style.borderBottomLeftRadius = 12;
                postCard.style.borderBottomRightRadius = 12;
                postCard.style.paddingTop = 10;
                postCard.style.paddingBottom = 10;
                postCard.style.paddingLeft = 12;
                postCard.style.paddingRight = 12;
                postCard.style.marginBottom = 8;
                SetBorder(postCard.style, 1f, new Color(0.12f, 0.16f, 0.23f));

                // Author Header
                var authorRow = new VisualElement();
                authorRow.style.flexDirection = FlexDirection.Row;
                authorRow.style.justifyContent = Justify.SpaceBetween;
                authorRow.style.alignItems = Align.Center;
                authorRow.style.marginBottom = 6;

                var nameRow = new VisualElement();
                nameRow.style.flexDirection = FlexDirection.Row;
                nameRow.style.alignItems = Align.Center;

                var authorLabel = new Label(post.AuthorName);
                authorLabel.style.color = new Color(0.97f, 0.98f, 0.99f);
                authorLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                authorLabel.style.fontSize = 12;

                var handleLabel = new Label($" {post.Handle}");
                handleLabel.style.color = new Color(0.58f, 0.64f, 0.72f);
                handleLabel.style.fontSize = 10;

                nameRow.Add(authorLabel);
                nameRow.Add(handleLabel);

                var timeLabel = new Label(post.TimeAgo);
                timeLabel.style.color = new Color(0.39f, 0.45f, 0.55f);
                timeLabel.style.fontSize = 9;

                authorRow.Add(nameRow);
                authorRow.Add(timeLabel);

                // Post Content
                var contentLabel = new Label(post.Content);
                contentLabel.style.color = new Color(0.80f, 0.84f, 0.88f);
                contentLabel.style.fontSize = 11;
                contentLabel.style.whiteSpace = WhiteSpace.Normal;
                contentLabel.style.marginBottom = 8;

                // Action Row
                var actionRow = new VisualElement();
                actionRow.style.flexDirection = FlexDirection.Row;
                actionRow.style.justifyContent = Justify.SpaceBetween;
                actionRow.style.alignItems = Align.Center;

                var btnLike = new Button();
                btnLike.text = post.IsLikedByPlayer ? $"❤️ {post.LikesCount:N0}" : $"🤍 {post.LikesCount:N0}";
                btnLike.style.fontSize = 10;
                btnLike.style.color = post.IsLikedByPlayer ? new Color(0.96f, 0.25f, 0.37f) : new Color(0.58f, 0.64f, 0.72f);
                btnLike.style.backgroundColor = new Color(0.12f, 0.16f, 0.23f);
                btnLike.style.borderTopLeftRadius = 6;
                btnLike.style.borderTopRightRadius = 6;
                btnLike.style.borderBottomLeftRadius = 6;
                btnLike.style.borderBottomRightRadius = 6;

                var targetPost = post;
                btnLike.clicked += () =>
                {
                    targetPost.ToggleLike();
                    btnLike.text = targetPost.IsLikedByPlayer ? $"❤️ {targetPost.LikesCount:N0}" : $"🤍 {targetPost.LikesCount:N0}";
                    btnLike.style.color = targetPost.IsLikedByPlayer ? new Color(0.96f, 0.25f, 0.37f) : new Color(0.58f, 0.64f, 0.72f);
                };

                var commentsLabel = new Label($"💬 {post.CommentsCount} comments");
                commentsLabel.style.color = new Color(0.39f, 0.45f, 0.55f);
                commentsLabel.style.fontSize = 10;

                actionRow.Add(btnLike);
                actionRow.Add(commentsLabel);

                postCard.Add(authorRow);
                postCard.Add(contentLabel);
                postCard.Add(actionRow);

                _containerSocialPosts.Add(postCard);
            }
        }

        // ── 3. Contacts / Relationship Hub (#P4-004) ──────────────────────────
        private void RefreshContactsUI()
        {
            if (_containerContacts == null) return;
            _containerContacts.Clear();

            foreach (var contact in _contacts)
            {
                var card = new VisualElement();
                card.style.backgroundColor = new Color(0.07f, 0.11f, 0.18f); // #131C2E
                card.style.borderTopLeftRadius = 12;
                card.style.borderTopRightRadius = 12;
                card.style.borderBottomLeftRadius = 12;
                card.style.borderBottomRightRadius = 12;
                card.style.paddingTop = 10;
                card.style.paddingBottom = 10;
                card.style.paddingLeft = 12;
                card.style.paddingRight = 12;
                card.style.marginBottom = 10;
                SetBorder(card.style, 1f, new Color(0.12f, 0.16f, 0.23f));

                // Header
                var topRow = new VisualElement();
                topRow.style.flexDirection = FlexDirection.Row;
                topRow.style.justifyContent = Justify.SpaceBetween;
                topRow.style.alignItems = Align.Center;
                topRow.style.marginBottom = 6;

                var nameLabel = new Label(contact.Name);
                nameLabel.style.color = new Color(0.97f, 0.98f, 0.99f);
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                nameLabel.style.fontSize = 13;

                var roleBadge = new Label(contact.Type.ToString());
                roleBadge.style.color = new Color(0.22f, 0.74f, 0.97f);
                roleBadge.style.fontSize = 10;
                roleBadge.style.backgroundColor = new Color(0.22f, 0.74f, 0.97f, 0.15f);
                roleBadge.style.paddingTop = 2;
                roleBadge.style.paddingBottom = 2;
                roleBadge.style.paddingLeft = 6;
                roleBadge.style.paddingRight = 6;
                roleBadge.style.borderTopLeftRadius = 6;
                roleBadge.style.borderTopRightRadius = 6;
                roleBadge.style.borderBottomLeftRadius = 6;
                roleBadge.style.borderBottomRightRadius = 6;

                topRow.Add(nameLabel);
                topRow.Add(roleBadge);

                // Affinity & Trust Meters
                var metersRow = new VisualElement();
                metersRow.style.flexDirection = FlexDirection.Row;
                metersRow.style.justifyContent = Justify.SpaceBetween;
                metersRow.style.marginBottom = 6;

                var affinityText = new Label($"❤️ Affinity: {contact.Affinity:F0}%");
                affinityText.style.color = new Color(0.96f, 0.25f, 0.37f);
                affinityText.style.fontSize = 11;

                var trustText = new Label($"🤝 Trust: {contact.Trust:F0}%");
                trustText.style.color = new Color(0.22f, 0.74f, 0.97f);
                trustText.style.fontSize = 11;

                metersRow.Add(affinityText);
                metersRow.Add(trustText);

                // History snippet
                string snippet = contact.SharedHistory.Count > 0 ? contact.SharedHistory[contact.SharedHistory.Count - 1] : "Bond established.";
                var snippetLabel = new Label(snippet);
                snippetLabel.style.color = new Color(0.58f, 0.64f, 0.72f);
                snippetLabel.style.fontSize = 10;
                snippetLabel.style.whiteSpace = WhiteSpace.Normal;
                snippetLabel.style.marginBottom = 8;

                // Action Buttons Row
                var actionRow = new VisualElement();
                actionRow.style.flexDirection = FlexDirection.Row;
                actionRow.style.justifyContent = Justify.SpaceBetween;

                var targetRel = contact;

                // 1. Call Button
                var btnCall = CreateSocialActionButton("📞 Call", () => HandleSocialAction(targetRel, SocialActionType.CallCatchUp));
                actionRow.Add(btnCall);

                // 2. Gift Button
                var btnGift = CreateSocialActionButton("🎁 Gift", () => HandleSocialAction(targetRel, SocialActionType.SendGift));
                actionRow.Add(btnGift);

                // 3. Dinner Button
                var btnDinner = CreateSocialActionButton("🍽️ Dinner", () => HandleSocialAction(targetRel, SocialActionType.DinnerHangOut));
                actionRow.Add(btnDinner);

                // 4. Tactics Button (Manager & Teammates only)
                if (contact.Type == RelationshipType.Manager || contact.Type == RelationshipType.Teammate)
                {
                    var btnTactics = CreateSocialActionButton("📋 Tactics", () => HandleSocialAction(targetRel, SocialActionType.TalkTactics));
                    actionRow.Add(btnTactics);
                }

                card.Add(topRow);
                card.Add(metersRow);
                card.Add(snippetLabel);
                card.Add(actionRow);

                _containerContacts.Add(card);
            }
        }

        private static Button CreateSocialActionButton(string text, Action onClick)
        {
            var btn = new Button();
            btn.text = text;
            btn.style.fontSize = 10;
            btn.style.color = new Color(0.97f, 0.98f, 0.99f);
            btn.style.backgroundColor = new Color(0.12f, 0.16f, 0.23f);
            btn.style.paddingTop = 4;
            btn.style.paddingBottom = 4;
            btn.style.paddingLeft = 8;
            btn.style.paddingRight = 8;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
            btn.clicked += onClick;
            return btn;
        }

        private void HandleSocialAction(Relationship contact, SocialActionType action)
        {
            var bridge = SimulationBridge.Instance;
            var save = bridge?.CurrentSave;
            if (save == null) return;

            var result = RelationshipSystem.ExecuteSocialAction(contact, action, save);
            if (result.Success)
            {
                int index = _contacts.FindIndex(c => c.Id == contact.Id);
                if (index >= 0)
                {
                    _contacts[index] = result.UpdatedRelationship;
                }

                RefreshContactsUI();
                Debug.Log($"[PhoneOS] {result.Message}");
            }
            else
            {
                Debug.LogWarning($"[PhoneOS] Action failed: {result.Message}");
            }
        }

        // ── 4. News App (Football Daily) ──────────────────────────────────────
        private void RefreshNewsUI()
        {
            if (_containerNews == null) return;
            _containerNews.Clear();

            foreach (var news in _newsArticles)
            {
                var card = new VisualElement();
                card.style.backgroundColor = new Color(0.07f, 0.11f, 0.18f); // #131C2E
                card.style.borderTopLeftRadius = 12;
                card.style.borderTopRightRadius = 12;
                card.style.borderBottomLeftRadius = 12;
                card.style.borderBottomRightRadius = 12;
                card.style.paddingTop = 10;
                card.style.paddingBottom = 10;
                card.style.paddingLeft = 12;
                card.style.paddingRight = 12;
                card.style.marginBottom = 8;
                SetBorder(card.style, 1f, new Color(0.12f, 0.16f, 0.23f));

                var tagRow = new VisualElement();
                tagRow.style.flexDirection = FlexDirection.Row;
                tagRow.style.justifyContent = Justify.SpaceBetween;
                tagRow.style.alignItems = Align.Center;
                tagRow.style.marginBottom = 4;

                var tagBadge = new Label(news.Tag);
                tagBadge.style.color = new Color(0.96f, 0.62f, 0.04f);
                tagBadge.style.fontSize = 9;
                tagBadge.style.unityFontStyleAndWeight = FontStyle.Bold;

                var timeLabel = new Label(news.TimeAgo);
                timeLabel.style.color = new Color(0.39f, 0.45f, 0.55f);
                timeLabel.style.fontSize = 9;

                tagRow.Add(tagBadge);
                tagRow.Add(timeLabel);

                var sourceLabel = new Label(news.AuthorName);
                sourceLabel.style.color = new Color(0.22f, 0.74f, 0.97f);
                sourceLabel.style.fontSize = 10;
                sourceLabel.style.marginBottom = 2;

                var contentLabel = new Label(news.Content);
                contentLabel.style.color = new Color(0.97f, 0.98f, 0.99f);
                contentLabel.style.fontSize = 11;
                contentLabel.style.whiteSpace = WhiteSpace.Normal;

                card.Add(tagRow);
                card.Add(sourceLabel);
                card.Add(contentLabel);

                _containerNews.Add(card);
            }
        }

        private static void SetBorder(IStyle style, float width, Color color)
        {
            style.borderTopWidth = width;
            style.borderBottomWidth = width;
            style.borderLeftWidth = width;
            style.borderRightWidth = width;
            style.borderTopColor = color;
            style.borderBottomColor = color;
            style.borderLeftColor = color;
            style.borderRightColor = color;
        }
    }
}
