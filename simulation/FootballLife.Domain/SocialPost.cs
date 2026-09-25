using System;

namespace FootballLife.Domain
{
    /// <summary>
    /// Represents a social media post on FootyGram or a news bulletin in Football Daily (#P4-003).
    /// </summary>
    public sealed record SocialPost
    {
        public Guid Id { get; init; }
        public string AuthorName { get; init; }
        public string Handle { get; init; }
        public string AvatarIcon { get; init; }
        public string Content { get; init; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; init; }
        public string TimeAgo { get; init; }
        public bool IsLikedByPlayer { get; set; }
        public string Tag { get; init; }
        public bool IsNewsArticle { get; init; }

        public SocialPost(
            Guid id,
            string authorName,
            string handle,
            string avatarIcon,
            string content,
            int likesCount,
            int commentsCount,
            string timeAgo,
            bool isLikedByPlayer = false,
            string tag = "#FootballLife",
            bool isNewsArticle = false)
        {
            Id = id;
            AuthorName = authorName;
            Handle = handle;
            AvatarIcon = avatarIcon;
            Content = content;
            LikesCount = likesCount;
            CommentsCount = commentsCount;
            TimeAgo = timeAgo;
            IsLikedByPlayer = isLikedByPlayer;
            Tag = tag;
            IsNewsArticle = isNewsArticle;
        }

        public void ToggleLike()
        {
            if (IsLikedByPlayer)
            {
                IsLikedByPlayer = false;
                LikesCount = Math.Max(0, LikesCount - 1);
            }
            else
            {
                IsLikedByPlayer = true;
                LikesCount++;
            }
        }
    }
}
