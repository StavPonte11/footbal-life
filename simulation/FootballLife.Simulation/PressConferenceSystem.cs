using System;
using System.Collections.Generic;
using FootballLife.Domain;
using FootballLife.Simulation.Persistence;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Consequence result after answering a press conference question (#P4-009).
    /// </summary>
    public sealed record PressAnswerResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public float ManagerTrustDelta { get; init; }
        public float FanPopularityDelta { get; init; }
        public float TeammateMoraleDelta { get; init; }
        public float MediaReputationDelta { get; init; }
    }

    /// <summary>
    /// Pure C# simulation system generating pre-match, post-match, and transfer press conferences
    /// and deterministically resolving media dialogue consequences (#P4-009).
    /// </summary>
    public static class PressConferenceSystem
    {
        private static readonly Journalist s_skySports = new()
        {
            Name = "James Thornton",
            Outlet = "Sky Sports News",
            Temperament = JournalistTemperament.Tactical,
            AvatarEmoji = "📺"
        };

        private static readonly Journalist s_dailyMirror = new()
        {
            Name = "Chloe Bennett",
            Outlet = "The Daily Mirror",
            Temperament = JournalistTemperament.Sensationalist,
            AvatarEmoji = "📰"
        };

        private static readonly Journalist s_athletic = new()
        {
            Name = "Marcus Vance",
            Outlet = "The Athletic",
            Temperament = JournalistTemperament.Supportive,
            AvatarEmoji = "🎙️"
        };

        /// <summary>
        /// Generates a contextual press conference tailored to recent match performance,
        /// current form, transfer status, or manager trust.
        /// </summary>
        public static PressConference GeneratePressConference(CareerSaveData save, MatchResult? recentMatch = null)
        {
            save ??= new CareerSaveData();
            var questions = new List<PressQuestion>();

            // 1. Post-Match or Form Question
            if (recentMatch != null)
            {
                bool won = recentMatch.HomeScore > recentMatch.AwayScore;
                if (won)
                {
                    questions.Add(new PressQuestion
                    {
                        Id = "q_match_win",
                        Journalist = s_skySports,
                        Category = "Matchday",
                        QuestionText = $"Sensational result today, {save.PlayerName}! The fans are calling this a defining performance. How do you assess your contribution?",
                        Responses = new List<PressResponseChoice>
                        {
                            new()
                            {
                                Tone = PressTone.Humble,
                                Text = "All credit goes to the squad and the manager's tactical preparation. I just played my part.",
                                ManagerTrustDelta = +5f,
                                FanPopularityDelta = +4f,
                                TeammateMoraleDelta = +6f,
                                MediaReputationDelta = +3f,
                                ConsequenceSummary = "Boosts team harmony and manager trust."
                            },
                            new()
                            {
                                Tone = PressTone.Confident,
                                Text = "I stepped up when the lights were brightest. That's why I wear this shirt.",
                                ManagerTrustDelta = +2f,
                                FanPopularityDelta = +8f,
                                TeammateMoraleDelta = +1f,
                                MediaReputationDelta = +6f,
                                ConsequenceSummary = "Significant boost to fan fame and media hype."
                            },
                            new()
                            {
                                Tone = PressTone.Defiant,
                                Text = "To everyone who doubted us all week in the papers: keep talking, we'll keep winning.",
                                ManagerTrustDelta = -2f,
                                FanPopularityDelta = +6f,
                                TeammateMoraleDelta = +4f,
                                MediaReputationDelta = -3f,
                                ConsequenceSummary = "Fires up the fanbase but ruffles media feathers."
                            },
                            new()
                            {
                                Tone = PressTone.Diplomatic,
                                Text = "Three points is all that matters. We rest tonight, then back on the training pitch tomorrow.",
                                ManagerTrustDelta = +4f,
                                FanPopularityDelta = +3f,
                                TeammateMoraleDelta = +3f,
                                MediaReputationDelta = +4f,
                                ConsequenceSummary = "Professional and composed stance."
                            }
                        }
                    });
                }
                else
                {
                    questions.Add(new PressQuestion
                    {
                        Id = "q_match_loss",
                        Journalist = s_dailyMirror,
                        Category = "Matchday",
                        QuestionText = $"Tough defeat out there today. The attack looked disjointed at times. What went wrong in the final third?",
                        Responses = new List<PressResponseChoice>
                        {
                            new()
                            {
                                Tone = PressTone.Humble,
                                Text = "I hold my hands up—I didn't take my chances. I will work overtime this week to put it right.",
                                ManagerTrustDelta = +6f,
                                FanPopularityDelta = +3f,
                                TeammateMoraleDelta = +2f,
                                MediaReputationDelta = +4f,
                                ConsequenceSummary = "Honest accountability appreciated by the manager."
                            },
                            new()
                            {
                                Tone = PressTone.Diplomatic,
                                Text = "We win together and lose together. We'll analyze the tape and bounce back next weekend.",
                                ManagerTrustDelta = +3f,
                                FanPopularityDelta = +2f,
                                TeammateMoraleDelta = +5f,
                                MediaReputationDelta = +2f,
                                ConsequenceSummary = "Shields teammates from media criticism."
                            },
                            new()
                            {
                                Tone = PressTone.Defiant,
                                Text = "The referee made questionable decisions that changed the momentum. We deserved much better.",
                                ManagerTrustDelta = -4f,
                                FanPopularityDelta = +5f,
                                TeammateMoraleDelta = -2f,
                                MediaReputationDelta = -5f,
                                ConsequenceSummary = "Blames officials; creates media controversy."
                            },
                            new()
                            {
                                Tone = PressTone.Confident,
                                Text = "Every great team faces adversity. Watch how we respond next match.",
                                ManagerTrustDelta = +2f,
                                FanPopularityDelta = +4f,
                                TeammateMoraleDelta = +3f,
                                MediaReputationDelta = +3f,
                                ConsequenceSummary = "Forward-looking confidence."
                            }
                        }
                    });
                }
            }
            else
            {
                // General League Outlook
                questions.Add(new PressQuestion
                {
                    Id = "q_general_form",
                    Journalist = s_athletic,
                    Category = "Tactics",
                    QuestionText = $"You've been featured heavily in training this week. How are you adapting to your tactical responsibilities?",
                    Responses = new List<PressResponseChoice>
                    {
                        new()
                        {
                            Tone = PressTone.Humble,
                            Text = "I'm learning every day from the coaching staff and veterans in our dressing room.",
                            ManagerTrustDelta = +5f,
                            FanPopularityDelta = +2f,
                            TeammateMoraleDelta = +4f,
                            MediaReputationDelta = +3f,
                            ConsequenceSummary = "Builds strong coachable reputation."
                        },
                        new()
                        {
                            Tone = PressTone.Confident,
                            Text = "I feel physically sharper than ever. I'm ready to dominate and carry the team's ambitions.",
                            ManagerTrustDelta = +2f,
                            FanPopularityDelta = +6f,
                            TeammateMoraleDelta = +1f,
                            MediaReputationDelta = +5f,
                            ConsequenceSummary = "High self-belief elevates fan excitement."
                        },
                        new()
                        {
                            Tone = PressTone.Diplomatic,
                            Text = "Wherever the manager asks me to play, I will execute the game plan to the letter.",
                            ManagerTrustDelta = +6f,
                            FanPopularityDelta = +2f,
                            TeammateMoraleDelta = +3f,
                            MediaReputationDelta = +4f,
                            ConsequenceSummary = "Strongly reinforces manager loyalty."
                        }
                    }
                });
            }

            // 2. Off-pitch / Ambition Question
            questions.Add(new PressQuestion
            {
                Id = "q_ambition_future",
                Journalist = s_dailyMirror,
                Category = "Transfer",
                QuestionText = $"Rumors are circulating online regarding your long-term contract and European interest. Are you fully committed here?",
                Responses = new List<PressResponseChoice>
                {
                    new()
                    {
                        Tone = PressTone.Humble,
                        Text = "I love this club and these supporters. My entire focus is giving everything for our badge.",
                        ManagerTrustDelta = +6f,
                        FanPopularityDelta = +8f,
                        TeammateMoraleDelta = +5f,
                        MediaReputationDelta = +2f,
                        ConsequenceSummary = "Reaffirms commitment; beloved by supporters."
                    },
                    new()
                    {
                        Tone = PressTone.Defiant,
                        Text = "I let my agent handle business matters. My job is to play football, not comment on headlines.",
                        ManagerTrustDelta = -3f,
                        FanPopularityDelta = -2f,
                        TeammateMoraleDelta = 0f,
                        MediaReputationDelta = +4f,
                        ConsequenceSummary = "Fuels transfer speculation in the press."
                    },
                    new()
                    {
                        Tone = PressTone.Confident,
                        Text = "I want to compete at the highest level and win major trophies. That's the ambition we all share.",
                        ManagerTrustDelta = +1f,
                        FanPopularityDelta = +5f,
                        TeammateMoraleDelta = +2f,
                        MediaReputationDelta = +6f,
                        ConsequenceSummary = "Shows world-class ambition and drive."
                    }
                }
            });

            return new PressConference
            {
                Id = $"pc_w{save.CurrentWeek}_{Guid.NewGuid().ToString("N")[..6]}",
                Title = recentMatch != null ? "Post-Match Press Briefing" : "Weekly Media Availability",
                ContextDescription = $"Live broadcast from the media room at {save.ClubName}.",
                Questions = questions
            };
        }

        /// <summary>
        /// Answers a press conference question and deterministically mutates manager trust,
        /// fan popularity, player morale, and media reputation.
        /// </summary>
        public static PressAnswerResult AnswerQuestion(
            CareerSaveData save,
            PressQuestion question,
            PressResponseChoice choice)
        {
            if (save == null || choice == null)
            {
                return new PressAnswerResult
                {
                    Success = false,
                    Message = "Invalid press answer parameters."
                };
            }

            // Apply stat deltas
            save.ManagerTrust = Math.Clamp((int)Math.Round(save.ManagerTrust + choice.ManagerTrustDelta), 0, 100);
            save.FanPopularity = Math.Clamp((int)Math.Round(save.FanPopularity + choice.FanPopularityDelta), 0, 100);
            save.Morale = Math.Clamp((int)Math.Round(save.Morale + choice.TeammateMoraleDelta), 0, 100);
            save.MediaReputation = Math.Clamp((int)Math.Round(save.MediaReputation + choice.MediaReputationDelta), 0, 100);

            string feedback = $"[{choice.Tone.ToString().ToUpperInvariant()}] {choice.ConsequenceSummary}";

            return new PressAnswerResult
            {
                Success = true,
                Message = feedback,
                ManagerTrustDelta = choice.ManagerTrustDelta,
                FanPopularityDelta = choice.FanPopularityDelta,
                TeammateMoraleDelta = choice.TeammateMoraleDelta,
                MediaReputationDelta = choice.MediaReputationDelta
            };
        }
    }
}
