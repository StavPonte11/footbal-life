import json

# Load existing 20 events
with open("content/data/events.json", "r") as f:
    existing_data = json.load(f)

events = existing_data.get("events", [])
print(f"Loaded {len(events)} existing events.")

existing_ids = {e["id"] for e in events}

# List of 85 additional rich life events
new_events = [
    # ─── LOCKER ROOM & TEAM DYNAMICS ───────────────────────────
    {
        "id": "locker_captaincy_debate",
        "title": "Vice-Captaincy Discussion",
        "description": "With the club vice-captain sidelined, the manager and senior players consider who should wear the armband for upcoming matches.",
        "category": "LockerRoom",
        "weight": 50,
        "cooldownWeeks": 12,
        "conditions": { "minAge": 21, "minManagerTrust": 60 },
        "choices": [
            {
                "id": "step_up_leader",
                "text": "Express readiness to lead and take on vocal leadership responsibilities.",
                "effects": [
                    { "target": "ManagerTrust", "delta": 7, "description": "Manager admires your emerging leadership" },
                    { "target": "Confidence", "delta": 5, "description": "Embraced leadership responsibility" }
                ]
            },
            {
                "id": "support_veteran",
                "text": "Advocate for an experienced veteran defender to take the armband.",
                "effects": [
                    { "target": "Morale", "delta": 6, "description": "Squad respects your humility and team-first ethos" },
                    { "target": "Happiness", "delta": 3, "description": "Maintained great locker room harmony" }
                ]
            }
        ]
    },
    {
        "id": "locker_academy_mentorship",
        "title": "Academy Prospect Mentorship",
        "description": "An anxious 17-year-old academy prospect is promoted to first-team training and looks overwhelmed during high-intensity drills.",
        "category": "LockerRoom",
        "weight": 65,
        "cooldownWeeks": 8,
        "conditions": { "minAge": 22 },
        "choices": [
            {
                "id": "mentor_prospect",
                "text": "Take the rookie under your wing, stay after training to coach his positioning.",
                "effects": [
                    { "target": "ManagerTrust", "delta": 6, "description": "Coaching staff value senior mentorship" },
                    { "target": "Fatigue", "delta": 4, "description": "Spent extra 45 minutes on the training pitch" },
                    { "target": "Morale", "delta": 4, "description": "Bonded with the incoming generation" }
                ]
            },
            {
                "id": "tough_love",
                "text": "Let him adapt through the natural physical intensity of professional football.",
                "effects": [
                    { "target": "Confidence", "delta": 3, "description": "Kept training ruthlessly competitive" }
                ]
            }
        ]
    },
    {
        "id": "locker_tactical_disagreement",
        "title": "Tactical Video Room Debate",
        "description": "During a heated video analysis session, the squad debates whether to press high or maintain a compact mid-block against the league leaders.",
        "category": "LockerRoom",
        "weight": 60,
        "cooldownWeeks": 6,
        "conditions": {},
        "choices": [
            {
                "id": "back_manager_system",
                "text": "Vocalize full commitment to the manager's tactical instructions.",
                "effects": [
                    { "target": "ManagerTrust", "delta": 8, "description": "Manager notes steadfast tactical obedience" },
                    { "target": "Morale", "delta": -3, "description": "A few teammates feel you dismissed their concerns" }
                ]
            },
            {
                "id": "voice_squad_opinion",
                "text": "Respectfully suggest tactical adjustments based on on-pitch player feelings.",
                "effects": [
                    { "target": "Morale", "delta": 6, "description": "Teammates rally around your honest voice" },
                    { "target": "ManagerTrust", "delta": -4, "description": "Manager slightly bristles at questioning tactics" }
                ]
            }
        ]
    },
    {
        "id": "locker_gaming_tournament",
        "title": "Team Hotel Gaming Tournament",
        "description": "On the eve of an away fixture, several teammates set up a multiplayer football gaming console tournament in the hotel lounge.",
        "category": "LockerRoom",
        "weight": 70,
        "cooldownWeeks": 5,
        "conditions": {},
        "choices": [
            {
                "id": "join_tournament",
                "text": "Join the tournament and battle for locker room gaming bragging rights.",
                "effects": [
                    { "target": "Happiness", "delta": 8, "description": "Great fun and banter with teammates" },
                    { "target": "Morale", "delta": 5, "description": "Squad cohesion noticeably reinforced" },
                    { "target": "Fatigue", "delta": 3, "description": "Stayed up an hour later than usual" }
                ]
            },
            {
                "id": "early_sleep",
                "text": "Politely decline and adhere strictly to your 10:00 PM pre-match sleep schedule.",
                "effects": [
                    { "target": "Fatigue", "delta": -5, "description": "Deep restorative pre-match rest" },
                    { "target": "Confidence", "delta": 2, "description": "Completely focused on tomorrow's kickoff" }
                ]
            }
        ]
    },
    {
        "id": "locker_penalty_taker_dispute",
        "title": "Penalty Duty Discussion",
        "description": "Following a missed spot-kick in the last match, the manager asks the designated penalty takers who should take the next one.",
        "category": "LockerRoom",
        "weight": 45,
        "cooldownWeeks": 10,
        "conditions": { "minManagerTrust": 50 },
        "choices": [
            {
                "id": "claim_penalties",
                "text": "Step up boldly and declare you will bury the next penalty under pressure.",
                "effects": [
                    { "target": "Confidence", "delta": 8, "description": "Unwavering self-belief in high-pressure moments" },
                    { "target": "ManagerTrust", "delta": 4, "description": "Manager likes your cold-blooded nerve" }
                ]
            },
            {
                "id": "defer_to_specialist",
                "text": "Suggest the club's veteran set-piece specialist handle penalty duties.",
                "effects": [
                    { "target": "Morale", "delta": 4, "description": "Defused any individualist drama" },
                    { "target": "ManagerTrust", "delta": 2, "description": "Manager appreciates pragmatic honesty" }
                ]
            }
        ]
    },
    {
        "id": "locker_prank_war",
        "title": "Locker Room Prank",
        "description": "You arrive at your locker after training to find your training boots suspended from the ceiling by the squad jokesters.",
        "category": "LockerRoom",
        "weight": 65,
        "cooldownWeeks": 6,
        "conditions": {},
        "choices": [
            {
                "id": "laugh_and_retaliate",
                "text": "Laugh it off and plot a harmless, clever prank for tomorrow's recovery session.",
                "effects": [
                    { "target": "Happiness", "delta": 6, "description": "Laughter brightens up a grueling training week" },
                    { "target": "Morale", "delta": 5, "description": "Beloved in the dressing room for good humor" }
                ]
            },
            {
                "id": "stay_professional",
                "text": "Ignore the antics calmly and focus on your physical recovery routine.",
                "effects": [
                    { "target": "Confidence", "delta": 2, "description": "Unfazed by locker room distractions" }
                ]
            }
        ]
    },
    {
        "id": "locker_teammate_slump",
        "title": "Supporting a Slumping Teammate",
        "description": "Your striking partner has gone eight matches without a goal and looks visibly downcast during training.",
        "category": "LockerRoom",
        "weight": 60,
        "cooldownWeeks": 7,
        "conditions": {},
        "choices": [
            {
                "id": "dinner_and_encouragement",
                "text": "Invite him out for dinner, review match clips, and offer genuine encouragement.",
                "effects": [
                    { "target": "Morale", "delta": 7, "description": "Locker room unity at its finest" },
                    { "target": "Money", "delta": -250, "description": "Covered the dinner bill" },
                    { "target": "Happiness", "delta": 4, "description": "Felt great supporting your teammate" }
                ]
            },
            {
                "id": "give_space",
                "text": "Give him personal space to figure out his mental game independently.",
                "effects": [
                    { "target": "Confidence", "delta": 1, "description": "Kept attention on your own preparation" }
                ]
            }
        ]
    },
    {
        "id": "locker_music_playlist",
        "title": "Pre-Match DJ Responsibility",
        "description": "The team DJ was transferred, and the squad is looking for someone to curate the motivational pre-match soundtrack.",
        "category": "LockerRoom",
        "weight": 55,
        "cooldownWeeks": 8,
        "conditions": {},
        "choices": [
            {
                "id": "take_dj_aux",
                "text": "Take over the dressing room speakers with high-tempo, hype motivational beats.",
                "effects": [
                    { "target": "Morale", "delta": 6, "description": "Squad is buzzing with energy before kickoff" },
                    { "target": "Confidence", "delta": 4, "description": "Pumping tunes elevate matchday focus" }
                ]
            },
            {
                "id": "pass_aux",
                "text": "Pass the auxiliary cable to another teammate and wear your own noise-canceling headphones.",
                "effects": [
                    { "target": "Happiness", "delta": 3, "description": "Relaxing in your personal zone" }
                ]
            }
        ]
    },
    {
        "id": "locker_sub_frustration",
        "title": "Substituted in the 60th Minute",
        "description": "The manager hooks you off in the 60th minute while you were on a roll and hungry for another goal.",
        "category": "LockerRoom",
        "weight": 50,
        "cooldownWeeks": 6,
        "conditions": {},
        "choices": [
            {
                "id": "clap_fans_bench",
                "text": "Applaud the fans, high-five your replacement, and encourage the squad from the bench.",
                "effects": [
                    { "target": "ManagerTrust", "delta": 6, "description": "Manager appreciates your mature, professional reaction" },
                    { "target": "Morale", "delta": 4, "description": "Team spirit prioritized over ego" }
                ]
            },
            {
                "id": "show_disappointment",
                "text": "Walk straight to the bench with obvious frustration at being denied a full 90.",
                "effects": [
                    { "target": "ManagerTrust", "delta": -5, "description": "Manager annoyed by public petulance" },
                    { "target": "Confidence", "delta": 3, "description": "Hunger to dominate remains burning hot" }
                ]
            }
        ]
    },
    {
        "id": "locker_derby_week_huddle",
        "title": "Derby Week Dressing Room Speech",
        "description": "It's local derby week, and local fans have gathered outside demanding nothing short of victory.",
        "category": "LockerRoom",
        "weight": 40,
        "cooldownWeeks": 16,
        "conditions": { "minManagerTrust": 55 },
        "choices": [
            {
                "id": "give_fiery_speech",
                "text": "Deliver an emotional rallying cry reminding everyone what this derby means to the city.",
                "effects": [
                    { "target": "Morale", "delta": 10, "description": "Dressing room is electrified with passion" },
                    { "target": "Confidence", "delta": 6, "description": "Embraced the immense pressure of derby day" },
                    { "target": "ManagerTrust", "delta": 4, "description": "Manager impressed by your fierce leadership" }
                ]
            },
            {
                "id": "calm_composure",
                "text": "Remind teammates to stay ice-cold, avoid reckless cards, and execute the gameplan.",
                "effects": [
                    { "target": "Confidence", "delta": 5, "description": "Controlled emotional temperament" },
                    { "target": "ManagerTrust", "delta": 6, "description": "Manager values your tactical composure" }
                ]
            }
        ]
    },

    # ─── MEDIA & PUBLIC RELATIONS ───────────────────────────────
    {
        "id": "media_pundit_feud",
        "title": "Television Pundit Criticism",
        "description": "A prominent national TV pundit harshly questions your work ethic and tactical discipline during a prime-time broadcast.",
        "category": "Media",
        "weight": 60,
        "cooldownWeeks": 8,
        "conditions": { "minAge": 18 },
        "choices": [
            {
                "id": "answer_on_pitch",
                "text": "Ignore the noise completely and vow to let your performance do the talking.",
                "effects": [
                    { "target": "Confidence", "delta": 5, "description": "Focused purely on football excellence" },
                    { "target": "ManagerTrust", "delta": 4, "description": "Manager praises your quiet professional dignity" }
                ]
            },
            {
                "id": "clap_back_social",
                "text": "Post a sharp, witty rebuttal on social media highlighting your recent statistics.",
                "effects": [
                    { "target": "Happiness", "delta": 4, "description": "Felt good to defend your name publicly" },
                    { "target": "ManagerTrust", "delta": -4, "description": "Manager dislikes social media controversies" }
                ]
            }
        ]
    },
    {
        "id": "media_transfer_speculation",
        "title": "Front Page Transfer Rumor",
        "description": "Tabloids link you with a blockbuster €75M summer move to a continental giant, creating widespread media buzz.",
        "category": "Media",
        "weight": 50,
        "cooldownWeeks": 10,
        "conditions": { "minSalary": 2000 },
        "choices": [
            {
                "id": "commit_to_club",
                "text": "Hold a press conference reaffirming your 100% commitment to your current club.",
                "effects": [
                    { "target": "ManagerTrust", "delta": 8, "description": "Club hierarchy and manager deeply respect your loyalty" },
                    { "target": "Morale", "delta": 5, "description": "Fans celebrate your unwavering commitment" }
                ]
            },
            {
                "id": "keep_options_open",
                "text": "Issue a diplomatic statement: 'In football, you never know what the future holds.'",
                "effects": [
                    { "target": "Confidence", "delta": 4, "description": "Recognized your growing global market status" },
                    { "target": "ManagerTrust", "delta": -3, "description": "Manager slightly unsettled by ambiguity" }
                ]
            }
        ]
    },
    {
        "id": "media_viral_skill_video",
        "title": "Viral Nutmeg Clip",
        "description": "A 10-second clip of your audacious flick and nutmeg in the last match racks up 25 million views across TikTok and Instagram.",
        "category": "Media",
        "weight": 55,
        "cooldownWeeks": 8,
        "conditions": {},
        "choices": [
            {
                "id": "share_and_engage",
                "text": "Repost the clip, tag the defender with playful banter, and engage with fans.",
                "effects": [
                    { "target": "Happiness", "delta": 7, "description": "Basking in global fan adoration" },
                    { "target": "Confidence", "delta": 6, "description": "Skyrocketing commercial appeal" }
                ]
            },
            {
                "id": "stay_humble",
                "text": "Post a team photo instead, captioning it: 'The 3 points matter most.'",
                "effects": [
                    { "target": "ManagerTrust", "delta": 6, "description": "Manager loves humble team-first mindset" },
                    { "target": "Morale", "delta": 4, "description": "Dressing room respects your modesty" }
                ]
            }
        ]
    },
    {
        "id": "media_podcast_interview",
        "title": "In-Depth Football Podcast",
        "description": "The country's most respected football tactics podcast invites you for a 60-minute deep dive on your career journey.",
        "category": "Media",
        "weight": 50,
        "cooldownWeeks": 12,
        "conditions": { "minAge": 19 },
        "choices": [
            {
                "id": "accept_podcast",
                "text": "Accept the invitation and give thoughtful insights into your training and mindset.",
                "effects": [
                    { "target": "Confidence", "delta": 5, "description": "Praised nationwide for your football intelligence" },
                    { "target": "Fatigue", "delta": 3, "description": "Evening studio recording session" },
                    { "target": "Happiness", "delta": 4, "description": "Enjoyed telling your authentic story" }
                ]
            },
            {
                "id": "decline_podcast",
                "text": "Politely decline to maintain complete privacy and low public profile.",
                "effects": [
                    { "target": "Fatigue", "delta": -2, "description": "Protected valuable evening downtime" }
                ]
            }
        ]
    },
    {
        "id": "media_controversial_referee_call",
        "title": "Post-Match VAR Controversy",
        "description": "After a contentious late penalty decision denies your team victory, journalists corner you in the tunnel for a reaction.",
        "category": "Media",
        "weight": 60,
        "cooldownWeeks": 6,
        "conditions": {},
        "choices": [
            {
                "id": "diplomatic_response",
                "text": "'Referees have a tough job; we should have put the game away earlier.'",
                "effects": [
                    { "target": "ManagerTrust", "delta": 6, "description": "Avoided an FA disciplinary fine" },
                    { "target": "Confidence", "delta": 2, "description": "Maintained emotional maturity" }
                ]
            },
            {
                "id": "blast_decision",
                "text": "Vent your outrage: 'Everyone in the stadium saw it was never a penalty!'",
                "effects": [
                    { "target": "Morale", "delta": 6, "description": "Supporters love that you stood up for the club" },
                    { "target": "Money", "delta": -3000, "description": "Fined by the football association" },
                    { "target": "ManagerTrust", "delta": -2, "description": "Club warned by league officials" }
                ]
            }
        ]
    },
    {
        "id": "media_magazine_cover",
        "title": "GQ Style Magazine Cover",
        "description": "A premier international lifestyle and fashion magazine invites you for a cover shoot and style interview.",
        "category": "Media",
        "weight": 40,
        "cooldownWeeks": 14,
        "conditions": { "minSalary": 3000 },
        "choices": [
            {
                "id": "shoot_cover",
                "text": "Agree to the high-fashion photoshoot in designer tailored suits.",
                "effects": [
                    { "target": "Money", "delta": 15000, "description": "Lucrative media appearance fee" },
                    { "target": "Happiness", "delta": 8, "description": "Celebrity status validated worldwide" },
                    { "target": "Fatigue", "delta": 5, "description": "Long day under studio flashlights" }
                ]
            },
            {
                "id": "focus_on_pitch",
                "text": "Decline the shoot: 'I want my face associated only with football kits.'",
                "effects": [
                    { "target": "ManagerTrust", "delta": 5, "description": "Manager approves of 100% football purity" },
                    { "target": "Confidence", "delta": 3, "description": "Unwavering commitment to the craft" }
                ]
            }
        ]
    },
    {
        "id": "media_documentary_crew",
        "title": "Club All-Access Documentary",
        "description": "A streaming platform is filming a behind-the-scenes documentary series and requests a dedicated episode about your home life.",
        "category": "Media",
        "weight": 45,
        "cooldownWeeks": 16,
        "conditions": { "minSalary": 2500 },
        "choices": [
            {
                "id": "open_doors",
                "text": "Open your home to the film crew and share your daily routine with millions.",
                "effects": [
                    { "target": "Money", "delta": 25000, "description": "Streaming royalties credited" },
                    { "target": "Happiness", "delta": 6, "description": "Fans connect deeply with your backstory" },
                    { "target": "Fatigue", "delta": 4, "description": "Camera crews tracking your home life" }
                ]
            },
            {
                "id": "protect_privacy",
                "text": "Keep your home sacred and decline private residence filming.",
                "effects": [
                    { "target": "Happiness", "delta": 3, "description": "Preserved domestic peace and privacy" }
                ]
            }
        ]
    },

    # ─── TRAINING & ATHLETIC DEVELOPMENT ───────────────────────
    {
        "id": "training_free_kick_dedication",
        "title": "After-Hours Free Kick Practice",
        "description": "As the sun sets over the training complex, you consider staying behind with a bag of balls to hone dead-ball curling technique.",
        "category": "Training",
        "weight": 70,
        "cooldownWeeks": 4,
        "conditions": {},
        "choices": [
            {
                "id": "practice_50_kicks",
                "text": "Stay out for an extra hour drilling 50 whipped free kicks into the top corners.",
                "effects": [
                    { "target": "Confidence", "delta": 8, "description": "Sharpened ball-striking mastery" },
                    { "target": "Fatigue", "delta": 8, "description": "Hamstrings and hip flexors feeling the burn" },
                    { "target": "ManagerTrust", "delta": 4, "description": "Manager notices your relentless dedication" }
                ]
            },
            {
                "id": "hit_ice_baths",
                "text": "Head straight to the cryotherapy chamber to ensure peak freshness for Saturday.",
                "effects": [
                    { "target": "Fatigue", "delta": -6, "description": "Accelerated physical muscle recovery" },
                    { "target": "Happiness", "delta": 3, "description": "Feeling refreshed and rejuvenated" }
                ]
            }
        ]
    },
    {
        "id": "training_fitness_coach_dispute",
        "title": "High-Intensity GPS Sprint Target",
        "description": "The sport science staff insists you haven't hit your target high-speed running meters this week and prescribes extra interval sprints.",
        "category": "Training",
        "weight": 65,
        "cooldownWeeks": 5,
        "conditions": {},
        "choices": [
            {
                "id": "crush_sprints",
                "text": "Put your head down and smash through every sprint interval with maximum intensity.",
                "effects": [
                    { "target": "ManagerTrust", "delta": 6, "description": "Coaching staff love your tireless work ethic" },
                    { "target": "Fatigue", "delta": 10, "description": "Lactic acid screaming through legs" },
                    { "target": "Confidence", "delta": 4, "description": "Unmatched aerobic conditioning" }
                ]
            },
            {
                "id": "manage_load",
                "text": "Explain to the physio that your hamstrings feel tight and request a modified load.",
                "effects": [
                    { "target": "Fatigue", "delta": -2, "description": "Smart injury prevention management" },
                    { "target": "ManagerTrust", "delta": -2, "description": "Fitness coach slightly skeptical of work rate" }
                ]
            }
        ]
    },
    {
        "id": "training_ice_bath_challenge",
        "title": "Sub-Zero Ice Bath Challenge",
        "description": "After a grueling two-hour tactical session, teammates challenge each other to a 10-minute ice bath recovery endurance test.",
        "category": "Training",
        "weight": 60,
        "cooldownWeeks": 6,
        "conditions": {},
        "choices": [
            {
                "id": "embrace_freeze",
                "text": "Submerge up to your neck, maintain steady breathing, and win the mental challenge.",
                "effects": [
                    { "target": "Confidence", "delta": 5, "description": "Mental toughness tested and proven" },
                    { "target": "Fatigue", "delta": -8, "description": "Systemic inflammation wiped clean" },
                    { "target": "Morale", "delta": 4, "description": "Teammates respect the freezing stoicism" }
                ]
            },
            {
                "id": "warm_jacuzzi",
                "text": "Opt for the heated hydrotherapy jacuzzi instead.",
                "effects": [
                    { "target": "Happiness", "delta": 5, "description": "Warm, luxurious muscle relaxation" }
                ]
            }
        ]
    },
    {
        "id": "training_private_chef_hire",
        "title": "Private Sports Nutritionist",
        "description": "A high-performance sports dietitian offers to customize your daily meal plans with organic, anti-inflammatory superfoods.",
        "category": "Training",
        "weight": 55,
        "cooldownWeeks": 12,
        "conditions": { "minSalary": 1000 },
        "choices": [
            {
                "id": "hire_dietitian",
                "text": "Hire the nutritionist and commit to a strict, scientific performance diet.",
                "effects": [
                    { "target": "Money", "delta": -4000, "description": "Monthly chef and organic nutrition retainer" },
                    { "target": "Fatigue", "delta": -6, "description": "Nutrient-dense meals enhance daily recovery" },
                    { "target": "Confidence", "delta": 4, "description": "Body operating at peak physiological efficiency" }
                ]
            },
            {
                "id": "cook_own_meals",
                "text": "Keep cooking balanced homemade pasta and grilled chicken yourself.",
                "effects": [
                    { "target": "Happiness", "delta": 2, "description": "Simple, home-cooked routine" }
                ]
            }
        ]
    },
    {
        "id": "training_hyperbaric_chamber",
        "title": "Hyperbaric Oxygen Chamber",
        "description": "An elite sports technology manufacturer offers to install a pressurized hyperbaric oxygen recovery capsule in your apartment.",
        "category": "Training",
        "weight": 40,
        "cooldownWeeks": 16,
        "conditions": { "minSalary": 3500 },
        "choices": [
            {
                "id": "install_chamber",
                "text": "Invest in the cutting-edge hyperbaric chamber for rapid cellular regeneration.",
                "effects": [
                    { "target": "Money", "delta": -18000, "description": "Hyperbaric oxygen chamber installation" },
                    { "target": "Fatigue", "delta": -10, "description": "Hyper-oxygenated sleep provides elite recovery" },
                    { "target": "Confidence", "delta": 6, "description": "State-of-the-art recovery setup" }
                ]
            },
            {
                "id": "stick_to_basics",
                "text": "Stick to good old-fashioned 8 hours of sleep and regular stretching.",
                "effects": [
                    { "target": "Happiness", "delta": 2, "description": "Saved substantial financial capital" }
                ]
            }
        ]
    },
    {
        "id": "training_tactical_position_shift",
        "title": "Positional Adaptation Request",
        "description": "Due to a squad injury crisis, the manager asks you to train in a secondary position for the next three weeks.",
        "category": "Training",
        "weight": 50,
        "cooldownWeeks": 10,
        "conditions": { "minManagerTrust": 45 },
        "choices": [
            {
                "id": "accept_utility_role",
                "text": "Embrace the tactical challenge enthusiastically to help the team succeed.",
                "effects": [
                    { "target": "ManagerTrust", "delta": 9, "description": "Manager thrilled by selfless tactical versatility" },
                    { "target": "Confidence", "delta": 4, "description": "Expanded your football understanding" }
                ]
            },
            {
                "id": "insist_primary_role",
                "text": "Explain politely that you can only deliver world-class output in your natural position.",
                "effects": [
                    { "target": "Confidence", "delta": 3, "description": "Protected your primary positional identity" },
                    { "target": "ManagerTrust", "delta": -5, "description": "Manager frustrated by tactical inflexibility" }
                ]
            }
        ]
    },

    # ─── COMMERCIAL & FINANCIAL ─────────────────────────────────
    {
        "id": "commercial_boot_custom_colorway",
        "title": "Signature Boot Colorway",
        "description": "Your boot sponsor wants to design a signature personalized colorway featuring your initials and national flag.",
        "category": "Commercial",
        "weight": 45,
        "cooldownWeeks": 14,
        "conditions": { "minSalary": 2000 },
        "choices": [
            {
                "id": "design_signature_boot",
                "text": "Collaborate on a bold, stylish neon design and launch it globally.",
                "effects": [
                    { "target": "Money", "delta": 35000, "description": "Boot release royalties credited" },
                    { "target": "Confidence", "delta": 8, "description": "Wearing signature boots on the pitch" },
                    { "target": "Happiness", "delta": 6, "description": "Childhood dream fulfilled" }
                ]
            },
            {
                "id": "classic_blackout_boot",
                "text": "Request a minimalist, understated all-black leather design.",
                "effects": [
                    { "target": "Money", "delta": 20000, "description": "Endorsement fee credited" },
                    { "target": "ManagerTrust", "delta": 4, "description": "Manager appreciates no-nonsense aesthetic" }
                ]
            }
        ]
    },
    {
        "id": "commercial_watch_ambassador",
        "title": "Swiss Luxury Watch Partnership",
        "description": "A prestigious Swiss watchmaker offers a lucrative multi-year ambassador contract requiring a Geneva launch event.",
        "category": "Commercial",
        "weight": 35,
        "cooldownWeeks": 18,
        "conditions": { "minSalary": 5000 },
        "choices": [
            {
                "id": "sign_watch_deal",
                "text": "Fly to Geneva, sign the ambassador deal, and receive a bespoke platinum chronograph.",
                "effects": [
                    { "target": "Money", "delta": 60000, "description": "Luxury endorsement signing fee" },
                    { "target": "Happiness", "delta": 9, "description": "Entering elite global luxury circles" },
                    { "target": "Fatigue", "delta": 5, "description": "International travel during off day" }
                ]
            },
            {
                "id": "turn_down_watch",
                "text": "Decline the contract to avoid mid-season commercial travel obligations.",
                "effects": [
                    { "target": "ManagerTrust", "delta": 4, "description": "Manager values your athletic priorities" }
                ]
            }
        ]
    },
    {
        "id": "commercial_crypto_sponsor_pitch",
        "title": "Speculative Fintech Proposal",
        "description": "An aggressive cryptocurrency platform offers a massive upfront payment to promote an upcoming token launch on your social channels.",
        "category": "Commercial",
        "weight": 55,
        "cooldownWeeks": 10,
        "conditions": { "minSalary": 1000 },
        "choices": [
            {
                "id": "decline_crypto_pitch",
                "text": "Turn down the deal to protect your supporters from volatile financial risks.",
                "effects": [
                    { "target": "Morale", "delta": 7, "description": "Fans praise your ethical integrity" },
                    { "target": "Happiness", "delta": 4, "description": "Clear conscience" }
                ]
            },
            {
                "id": "take_fast_money",
                "text": "Sign the promotional deal and pocket the substantial upfront check.",
                "effects": [
                    { "target": "Money", "delta": 45000, "description": "Crypto sponsorship payment received" },
                    { "target": "Morale", "delta": -6, "description": "Criticized by consumer advocacy groups" }
                ]
            }
        ]
    },
    {
        "id": "commercial_perfume_line",
        "title": "Signature Fragrance Launch",
        "description": "A French cosmetics house pitches a signature perfume line under your brand name, marketed across Europe.",
        "category": "Commercial",
        "weight": 40,
        "cooldownWeeks": 16,
        "conditions": { "minSalary": 3000 },
        "choices": [
            {
                "id": "launch_fragrance",
                "text": "Launch your signature scent with a high-profile fragrance ad campaign.",
                "effects": [
                    { "target": "Money", "delta": 30000, "description": "Advance royalty payment credited" },
                    { "target": "Happiness", "delta": 7, "description": "Commercial empire expanding" }
                ]
            },
            {
                "id": "shelve_cosmetics",
                "text": "Politely pass; prefer to keep endorsements strictly athletic.",
                "effects": [
                    { "target": "Confidence", "delta": 2, "description": "Keeping personal brand disciplined" }
                ]
            }
        ]
    },
    {
        "id": "commercial_real_estate_opportunity",
        "title": "Commercial Real Estate Investment",
        "description": "Your financial advisor presents an opportunity to acquire a prime commercial property unit in the city centre.",
        "category": "Commercial",
        "weight": 45,
        "cooldownWeeks": 14,
        "conditions": { "minSalary": 4000 },
        "choices": [
            {
                "id": "buy_property",
                "text": "Invest capital in prime real estate for secure, long-term rental cashflow.",
                "effects": [
                    { "target": "Money", "delta": -50000, "description": "Commercial property down payment" },
                    { "target": "Happiness", "delta": 6, "description": "Building sustainable generational wealth" }
                ]
            },
            {
                "id": "keep_liquid_cash",
                "text": "Keep funds liquid in high-yield bank accounts and treasury bonds.",
                "effects": [
                    { "target": "Confidence", "delta": 2, "description": "Maximum financial flexibility retained" }
                ]
            }
        ]
    },

    # ─── PERSONAL, FAMILY & RELATIONSHIPS ───────────────────────
    {
        "id": "family_parents_house",
        "title": "Buying Your Parents a Home",
        "description": "After establishing yourself as a first-team star, you look to buy your parents the dream family home they always deserved.",
        "category": "Family",
        "weight": 35,
        "cooldownWeeks": 24,
        "conditions": { "minSalary": 3000, "minAge": 20 },
        "choices": [
            {
                "id": "buy_dream_home",
                "text": "Purchase a beautiful home for your parents and celebrate with a family dinner.",
                "effects": [
                    { "target": "Money", "delta": -120000, "description": "Purchased parents' dream family house" },
                    { "target": "Happiness", "delta": 15, "description": "Deepest emotional pride and family gratitude" },
                    { "target": "Confidence", "delta": 8, "description": "Realized the ultimate childhood promise" }
                ]
            },
            {
                "id": "delay_purchase",
                "text": "Postpone the purchase until your next major contract extension.",
                "effects": [
                    { "target": "Happiness", "delta": -3, "description": "Felt a slight pang of guilt" }
                ]
            }
        ]
    },
    {
        "id": "family_wedding_attendance",
        "title": "Sibling's Weekend Wedding",
        "description": "Your older sibling is getting married on an international break weekend; attending requires a red-eye flight back before training.",
        "category": "Family",
        "weight": 40,
        "cooldownWeeks": 20,
        "conditions": {},
        "choices": [
            {
                "id": "attend_wedding_family",
                "text": "Fly out for the wedding, give an emotional toast, and return on the red-eye.",
                "effects": [
                    { "target": "Happiness", "delta": 12, "description": "Unforgettable family celebration" },
                    { "target": "Fatigue", "delta": 7, "description": "Red-eye travel fatigue" },
                    { "target": "Money", "delta": -3500, "description": "Wedding gift and travel expenses" }
                ]
            },
            {
                "id": "send_heartfelt_video",
                "text": "Send an emotional video message and expensive gift while remaining in camp.",
                "effects": [
                    { "target": "Fatigue", "delta": -3, "description": "Remained fully rested for upcoming fixtures" },
                    { "target": "Happiness", "delta": -2, "description": "Sad to miss the live ceremony" }
                ]
            }
        ]
    },
    {
        "id": "family_relative_loan_request",
        "title": "Relative's Business Venture",
        "description": "A distant cousin asks for a £30,000 loan to open a franchise cafe, with vague financial forecasts.",
        "category": "Family",
        "weight": 55,
        "cooldownWeeks": 12,
        "conditions": { "minSalary": 1500 },
        "choices": [
            {
                "id": "help_family_loan",
                "text": "Provide the money as a gift with no strings attached.",
                "effects": [
                    { "target": "Money", "delta": -30000, "description": "Family financial assistance" },
                    { "target": "Happiness", "delta": 4, "description": "Supported extended family in need" }
                ]
            },
            {
                "id": "decline_tactfully",
                "text": "Explain that your financial management team prohibits unvetted loans.",
                "effects": [
                    { "target": "Confidence", "delta": 3, "description": "Set healthy financial boundaries" },
                    { "target": "Happiness", "delta": -3, "description": "Temporary tension in family group chat" }
                ]
            }
        ]
    },
    {
        "id": "family_hometown_youth_tournament",
        "title": "Grassroots Youth Cup Sponsorship",
        "description": "The local grassroots club where you kicked your first football asks if you will sponsor their annual summer youth tournament.",
        "category": "Family",
        "weight": 50,
        "cooldownWeeks": 14,
        "conditions": {},
        "choices": [
            {
                "id": "sponsor_grassroots",
                "text": "Fund tournament trophies, new match balls, and kit for all 20 junior teams.",
                "effects": [
                    { "target": "Money", "delta": -5000, "description": "Grassroots tournament sponsorship" },
                    { "target": "Happiness", "delta": 10, "description": "Inspiring hundreds of young local players" },
                    { "target": "Morale", "delta": 6, "description": "Community hero status back home" }
                ]
            },
            {
                "id": "donate_autographed_shirts",
                "text": "Donate signed match-worn jerseys for their charity raffle.",
                "effects": [
                    { "target": "Happiness", "delta": 4, "description": "Helped the club raise valuable funds" }
                ]
            }
        ]
    },
    {
        "id": "family_hospital_charity_visit",
        "title": "Children's Hospital Holiday Visit",
        "description": "You are invited to join a hospital visit to hand out gifts to young patients recovering in the pediatric ward.",
        "category": "Family",
        "weight": 60,
        "cooldownWeeks": 12,
        "conditions": {},
        "choices": [
            {
                "id": "visit_and_spend_time",
                "text": "Spend the afternoon sitting with children, signing boots, and hearing their stories.",
                "effects": [
                    { "target": "Happiness", "delta": 11, "description": "Deeply humbled and inspired by the brave kids" },
                    { "target": "Morale", "delta": 8, "description": "Puts professional football pressure into perspective" },
                    { "target": "Money", "delta": -1500, "description": "Bought toys and holiday gifts" }
                ]
            },
            {
                "id": "send_presents_only",
                "text": "Send generous gift packages directly to the ward.",
                "effects": [
                    { "target": "Money", "delta": -1500, "description": "Holiday hospital donation" },
                    { "target": "Happiness", "delta": 4, "description": "Brought smiles to young fans" }
                ]
            }
        ]
    },

    # ─── LIFESTYLE & PERSONAL DISCIPLINE ────────────────────────
    {
        "id": "lifestyle_supercar_temptation",
        "title": "Exotic Sports Car Dealership",
        "description": "A luxury motor dealer offers you an allocation on a limited-edition Italian V12 supercar with immediate delivery.",
        "category": "Lifestyle",
        "weight": 45,
        "cooldownWeeks": 16,
        "conditions": { "minSalary": 4000 },
        "choices": [
            {
                "id": "buy_supercar",
                "text": "Purchase the roaring Italian supercar in metallic racing green.",
                "effects": [
                    { "target": "Money", "delta": -180000, "description": "Purchased luxury supercar" },
                    { "target": "Happiness", "delta": 10, "description": "Incredible rush turning heads at the training ground" },
                    { "target": "Confidence", "delta": 5, "description": "Living the elite football lifestyle" }
                ]
            },
            {
                "id": "invest_funds",
                "text": "Drive your sensible hybrid SUV and funnel the cash into index funds.",
                "effects": [
                    { "target": "Confidence", "delta": 3, "description": "Financial wisdom over flashiness" }
                ]
            }
        ]
    },
    {
        "id": "lifestyle_social_media_detox",
        "title": "Social Media Detox",
        "description": "After noticing yourself endlessly scrolling through football fan comment sections, you consider taking a complete digital detox.",
        "category": "Lifestyle",
        "weight": 65,
        "cooldownWeeks": 8,
        "conditions": {},
        "choices": [
            {
                "id": "delete_apps",
                "text": "Hand your accounts over to your agency and delete social media apps for a month.",
                "effects": [
                    { "target": "Happiness", "delta": 8, "description": "Tremendous mental peace and clarity" },
                    { "target": "Confidence", "delta": 6, "description": "Zero distraction from internet criticism" }
                ]
            },
            {
                "id": "remain_connected",
                "text": "Keep checking your feeds to stay connected with trending football culture.",
                "effects": [
                    { "target": "Happiness", "delta": -2, "description": "Occasional annoyance from toxic online trolls" }
                ]
            }
        ]
    },
    {
        "id": "lifestyle_late_night_party",
        "title": "Celebrity Birthday Bash",
        "description": "A famous music artist invites you to an exclusive VIP birthday party 72 hours before a mid-week league fixture.",
        "category": "Lifestyle",
        "weight": 55,
        "cooldownWeeks": 10,
        "conditions": { "minSalary": 1500 },
        "choices": [
            {
                "id": "attend_vip_bash",
                "text": "Attend the party, mingle with celebrities, but leave by 1:00 AM.",
                "effects": [
                    { "target": "Happiness", "delta": 9, "description": "Thrilling night in celebrity circles" },
                    { "target": "Fatigue", "delta": 6, "description": "Interrupted circadian rhythm" }
                ]
            },
            {
                "id": "stay_home_hydrate",
                "text": "Send congratulations and stay home with herbal tea and game tape.",
                "effects": [
                    { "target": "ManagerTrust", "delta": 4, "description": "Committed to elite athletic habits" },
                    { "target": "Fatigue", "delta": -3, "description": "Flawless physical readiness" }
                ]
            }
        ]
    },
    {
        "id": "lifestyle_hometown_vacation",
        "title": "Summer Vacation Choice",
        "description": "During the summer off-season break, you choose between a high-profile party island or a quiet mountain wellness retreat.",
        "category": "Lifestyle",
        "weight": 40,
        "cooldownWeeks": 20,
        "conditions": {},
        "choices": [
            {
                "id": "wellness_retreat",
                "text": "Book a tranquil Alpine wellness resort with altitude hiking and thermal spas.",
                "effects": [
                    { "target": "Fatigue", "delta": -12, "description": "Total physiological reset and lung regeneration" },
                    { "target": "Happiness", "delta": 8, "description": "Deep peace surrounded by nature" },
                    { "target": "Money", "delta": -6000, "description": "Luxury Alpine retreat booking" }
                ]
            },
            {
                "id": "yacht_party_island",
                "text": "Charter a yacht in Ibiza with friends and celebrate a great season.",
                "effects": [
                    { "target": "Happiness", "delta": 12, "description": "Epic memories partying under the Mediterranean sun" },
                    { "target": "Money", "delta": -25000, "description": "Luxury yacht charter expenses" },
                    { "target": "Fatigue", "delta": 5, "description": "Partying took a toll on fitness baseline" }
                ]
            }
        ]
    },
    {
        "id": "lifestyle_pet_dog_adoption",
        "title": "Adopting a Rescue Dog",
        "description": "While visiting a local animal shelter, you connect with an energetic young rescue Golden Retriever.",
        "category": "Lifestyle",
        "weight": 50,
        "cooldownWeeks": 16,
        "conditions": {},
        "choices": [
            {
                "id": "adopt_dog",
                "text": "Adopt the rescue dog and welcome a loyal furry companion into your home.",
                "effects": [
                    { "target": "Happiness", "delta": 12, "description": "Unconditional love waiting after every match" },
                    { "target": "Money", "delta": -1200, "description": "Adoption fees, pet supplies, and vet care" },
                    { "target": "Fatigue", "delta": 2, "description": "Daily morning walks around the park" }
                ]
            },
            {
                "id": "wait_until_later",
                "text": "Decide that constant away travel makes pet ownership impractical right now.",
                "effects": [
                    { "target": "Confidence", "delta": 2, "description": "Pragmatic decision based on lifestyle" }
                ]
            }
        ]
    }
]

# Add more procedural variations to comfortably cross 100+ events
extra_templates = [
    ("fan_autograph_overtime", "Fan Autograph Session", "Supporters line up for autographs as the team bus prepares to depart.", "Lifestyle", 60, [("stay_sign_all", "Stay and sign every shirt", [("Happiness", 7), ("Fatigue", 4)]), ("board_team_bus", "Politely board bus with team", [("ManagerTrust", 3), ("Fatigue", -2)])]),
    ("fan_graffiti_mural", "City Street Art Mural", "Local street artists paint a stunning two-story portrait of you in the city center.", "Media", 45, [("visit_and_photo", "Visit mural and thank the artists", [("Happiness", 10), ("Morale", 8)]), ("share_story_quietly", "Share a quiet appreciation post", [("Happiness", 5)])]),
    ("fan_young_supporter_letter", "Young Fan's Inspiring Letter", "A 9-year-old hospital patient writes you a handwritten letter thanking you for your dedication.", "Family", 65, [("reply_and_send_kit", "Write personal letter and gift signed boots", [("Happiness", 10), ("Morale", 8), ("Money", -500)]), ("reply_with_photo", "Send signed autographed card", [("Happiness", 4)])]),
    ("locker_training_duel", "Heated Training Pitch Tackle", "A fiery tackle between you and a starting midfielder leads to momentary pushing and shoving.", "LockerRoom", 70, [("shake_hands_fierce", "Shake hands firmly: 'Good battle, let's win Saturday'", [("Morale", 6), ("Confidence", 5)]), ("walk_away_scowling", "Walk away with an icy stare", [("Confidence", 3), ("Morale", -3)])]),
    ("locker_foreign_language_learning", "Language Barrier at New Club", "The coaching staff speaks mainly the local language, creating communication hurdles.", "Training", 55, [("hire_language_tutor", "Hire private language tutor for daily lessons", [("ManagerTrust", 8), ("Confidence", 6), ("Money", -2000)]), ("learn_on_pitch", "Pick up phrases organically during training", [("Confidence", 2)])]),
    ("locker_ice_cold_stewards", "Snowy Winter Training Session", "A sub-zero blizzard blankets the training pitches in frost.", "Training", 60, [("train_in_shorts", "Train in shorts and sleeves to prove mental toughness", [("Confidence", 6), ("Fatigue", 5)]), ("wear_thermal_gear", "Wear full thermal snood, gloves, and leggings", [("Fatigue", -2)])]),
    ("locker_preseason_world_tour", "Grueling Far East Tour", "Commercial marketing demands 3 matches in 7 days across Tokyo, Seoul, and Singapore.", "Commercial", 50, [("embrace_fan_events", "Engage enthusiastically at every commercial signing", [("Money", 15000), ("Fatigue", 8), ("Confidence", 5)]), ("rest_in_hotel_room", "Prioritize sleep between promotional appearances", [("Fatigue", -4), ("ManagerTrust", 4)])]),
    ("locker_halftime_hairdryer", "Manager Halftime Hairdryer", "Losing 2-0 at halftime, the manager unleashes a thunderous tactical tirade.", "LockerRoom", 60, [("respond_with_passion", "Rally the troops and promise a second-half comeback", [("ManagerTrust", 7), ("Confidence", 6), ("Morale", 6)]), ("listen_in_silence", "Silently absorb tactical corrections", [("ManagerTrust", 4)])]),
    ("locker_rest_for_cup", "Rested for League Fixture", "The manager rests you for an early cup round to preserve legs for the title race.", "LockerRoom", 50, [("support_youngsters", "Cheer on the reserve team from the directors' box", [("ManagerTrust", 6), ("Fatigue", -8), ("Morale", 4)]), ("ask_to_play", "Lobby manager to at least feature on the bench", [("Confidence", 3), ("ManagerTrust", -3)])]),
    ("media_pfa_award_nomination", "Player of the Month Nomination", "League officials nominate you for the official Player of the Month award.", "Media", 45, [("thank_squad_publicly", "Thank teammates: 'Individual awards reflect team effort'", [("ManagerTrust", 6), ("Morale", 6), ("Confidence", 6)]), ("celebrate_form", "Celebrate your blistering run of form", [("Confidence", 8)])]),
    ("media_headline_pressure", "Back Page Headline: 'Key Man'", "National newspapers brand you the definitive difference-maker ahead of this weekend's clash.", "Media", 50, [("thrive_under_spotlight", "Embrace the pressure: 'This is what I live for'", [("Confidence", 8), ("Happiness", 5)]), ("downplay_hype", "Deflect hype and keep expectations measured", [("Confidence", 3), ("ManagerTrust", 4)])]),
    ("media_var_postmatch_interview", "Tunnel Interview on Controversial Goal", "Television cameras press you on whether the ball touched your arm before the goal.", "Media", 55, [("defend_goal_honesty", "Maintain ball struck chest: 'Referee made the right call'", [("Confidence", 4), ("ManagerTrust", 3)]), ("admit_lucky_bounce", "Admit with a grin: 'Got a lucky break there'", [("Morale", 4), ("Happiness", 3)])]),
    ("commercial_billboard_campaign", "City Center Billboard", "Your massive 50-foot commercial billboard is unveiled overlooking the main railway station.", "Commercial", 40, [("visit_billboard", "Take a selfie in front of the billboard with friends", [("Happiness", 8), ("Confidence", 6)]), ("let_work_speak", "Stay focused on Saturday's fixture", [("ManagerTrust", 3)])]),
    ("commercial_investment_adviser", "Wealth Management Portfolio Review", "Your financial team presents options to diversify your savings into renewable energy assets.", "Commercial", 50, [("invest_green_energy", "Commit capital to sustainable infrastructure fund", [("Money", -20000), ("Happiness", 7)]), ("keep_cash_in_bank", "Maintain standard low-risk bank deposits", [("Confidence", 2)])]),
    ("training_gps_record_speed", "Top Sprint Speed Record", "Telemetry tracking records you clocking 36.2 km/h in training, the fastest at the club.", "Training", 50, [("celebrate_athleticism", "Challenge squad wingers to a 40m sprint tournament", [("Confidence", 7), ("Morale", 5)]), ("keep_working", "Focus on agility and deceleration control", [("ManagerTrust", 4)])]),
    ("training_yoga_mobility_class", "Yoga & Mobility Program", "A renowned sports mobility coach introduces an optional 7:00 AM yoga session.", "Training", 60, [("attend_early_yoga", "Wake up early for breathing and deep hip mobility", [("Fatigue", -6), ("Confidence", 4)]), ("sleep_extra_hour", "Prioritize sleep in bed instead", [("Fatigue", -3)])]),
    ("family_charity_foundation", "Launching Personal Foundation", "You formally establish a charitable foundation providing sports access to underprivileged youth.", "Family", 35, [("fund_foundation_launch", "Endow the charity foundation with a significant grant", [("Money", -50000), ("Happiness", 14), ("Morale", 10)]), ("host_charity_gala", "Host an annual gala dinner to fundraise from sponsors", [("Money", -10000), ("Happiness", 8), ("Confidence", 6)])]),
    ("lifestyle_coffee_ritual", "Artisan Coffee Machine", "You install a state-of-the-art Italian espresso bar in your home kitchen.", "Lifestyle", 55, [("master_espresso", "Learn brewing temperatures and enjoy rich pre-training espresso", [("Happiness", 6), ("Fatigue", -2), ("Money", -2500)]), ("drink_club_coffee", "Stick to the basic canteen brew", [("Happiness", 1)])]),
    ("lifestyle_meditation_mindfulness", "Sports Psychology Mindfulness", "The club psychologist recommends a daily 15-minute visualization mindfulness routine.", "Training", 65, [("practice_mindfulness", "Incorporate daily visualization before kickoff", [("Confidence", 7), ("Happiness", 5)]), ("trust_instincts", "Rely purely on instinctive gameplay", [("Confidence", 2)])]),
    ("locker_derby_victory_celebration", "Derby Triumph Dressing Room Scene", "After winning a chaotic derby 3-2, the dressing room erupts into jubilant song.", "LockerRoom", 45, [("lead_victory_chants", "Jump onto the central table and lead the club anthem", [("Happiness", 12), ("Morale", 10), ("Confidence", 8)]), ("hug_teammates_quietly", "Savor the hard-earned victory with quiet emotion", [("Happiness", 8), ("Morale", 6)])]),
    ("media_transfer_window_deadline", "Transfer Deadline Day Drama", "Television screens in the gym show reporters camped outside the stadium on deadline day.", "Media", 50, [("reassure_supporters", "Post reassuring video that you are staying to fight", [("ManagerTrust", 8), ("Morale", 7)]), ("watch_moves_curiously", "Watch other blockbuster transfers unfold with fascination", [("Happiness", 3)])]),
    ("training_mental_fatigue_break", "Mid-Season Mental Recharge", "After 15 consecutive starts, you feel mental fatigue setting in.", "Training", 60, [("request_rest_day", "Discuss with manager and take a quiet 48h rest", [("Fatigue", -10), ("Confidence", 4)]), ("push_through_burnout", "Tough it out and train at 100% intensity", [("Fatigue", 8), ("Confidence", 5)])]),
    ("commercial_gaming_stream", "Twitch Charity Livestream", "Teammates invite you to join a live video game stream to raise money for clean water charities.", "Commercial", 50, [("stream_for_charity", "Stream for 3 hours and match viewer donations", [("Money", -5000), ("Happiness", 8), ("Morale", 6)]), ("decline_stream", "Relax quietly off-camera", [("Fatigue", -2)])]),
    ("lifestyle_interior_design", "Apartment Interior Renovation", "An architect offers to redesign your living room with custom acoustic paneling and smart lighting.", "Lifestyle", 45, [("renovate_home", "Commission modern luxury Scandinavian redesign", [("Money", -25000), ("Happiness", 9)]), ("keep_current_furniture", "Keep your comfortable current setup", [("Happiness", 2)])]),
    ("family_childhood_coach_visit", "Visit from Childhood Youth Coach", "Your first grassroots coach who discovered you at age 8 comes to watch you play.", "Family", 40, [("give_vip_tickets", "Give him directors' box tickets and present your shirt after full-time", [("Happiness", 12), ("Morale", 8), ("Confidence", 6)]), ("quick_wave_tunnel", "Catch up for 5 minutes by the tunnel", [("Happiness", 5)])]),
    ("locker_new_signing_welcome", "Welcoming Star Foreign Signing", "The club shatters its transfer record on a talented striker from South America.", "LockerRoom", 50, [("welcome_to_city", "Help him find housing and show him around the best restaurants", [("Morale", 8), ("ManagerTrust", 6), ("Happiness", 5)]), ("compete_for_starting_spot", "Focus on maintaining your own starting status", [("Confidence", 4)])]),
    ("training_injury_scare_relief", "Post-Scan Injury Relief", "An MRI scan on your twisted knee comes back completely clear of ligament damage.", "Training", 55, [("celebrate_good_health", "Praise the physio team and return to full training", [("Happiness", 12), ("Confidence", 8), ("Fatigue", -4)]), ("take_cautious_session", "Do light gym cycling before full contact", [("Fatigue", -2)])]),
    ("media_tactical_breakdown_praise", "Pundits Analyze Your Off-Ball Movement", "Tactical analysts highlight your decoy runs as a masterclass in space creation.", "Media", 50, [("appreciate_expert_analysis", "Pleased that your unselfish runs are recognized", [("Confidence", 7), ("ManagerTrust", 5)]), ("keep_improving", "Review areas where passing could be crisper", [("Confidence", 3)])]),
    ("commercial_suit_fitting", "Official Club Cup Final Suit", "Tailors arrive at the stadium to measure the squad for bespoke Italian wool cup final suits.", "Commercial", 40, [("enjoy_luxury_tailoring", "Soak in the anticipation of an upcoming final", [("Happiness", 7), ("Confidence", 5)]), ("quick_fitting", "Quick 5-minute fitting and back to gym", [("ManagerTrust", 3)])]),
    ("lifestyle_cooking_mastery", "Culinary Cooking Class", "You take weekend Italian culinary lessons to master homemade pasta and sauces.", "Lifestyle", 50, [("cook_for_friends", "Host a delicious homemade pasta night for friends", [("Happiness", 8), ("Money", -800)]), ("simple_routine", "Stick to pre-prepared meal delivery", [("Happiness", 2)])]),
    ("family_high_school_reunion", "Hometown School Assembly", "Your former secondary school invites you to speak to students on resilience and discipline.", "Family", 45, [("inspire_next_generation", "Deliver an inspiring speech and answer student questions", [("Happiness", 11), ("Morale", 8)]), ("send_signed_pennant", "Send an official club framed pennant", [("Happiness", 3)])]),
    ("locker_penalty_shootout_prep", "Cup Shootout Preparation", "Ahead of a cup knockout tie, the manager has players practice penalties under sudden-death stakes.", "LockerRoom", 45, [("bury_decisive_penalty", "Step up to the spot and calmly strike into the top corner", [("Confidence", 8), ("ManagerTrust", 6)]), ("practice_goalkeeper_reads", "Study the opposing keeper's diving tendencies", [("Confidence", 5)])]),
    ("training_recovery_sauna", "Finnish Birch Sauna Session", "You spend 30 minutes in the wood-fired cedar sauna relaxing tired leg muscles.", "Training", 60, [("sauna_and_cold_plunge", "Complete 3 cycles of sauna heat and freezing plunge", [("Fatigue", -8), ("Happiness", 5)]), ("quick_shower", "Head home early to rest", [("Fatigue", -3)])]),
    ("media_legendary_player_praise", "Praise from Club Legend", "A legendary hall-of-fame striker calls you 'the most natural finisher this club has seen in 20 years.'", "Media", 40, [("humbled_by_legend", "Express profound respect: 'I have so much left to achieve'", [("Confidence", 10), ("ManagerTrust", 6), ("Happiness", 8)]), ("frame_newspaper", "Keep the quote on your phone lock screen for motivation", [("Confidence", 8)])]),
    ("commercial_sports_beverage", "Sports Hydration Brand Campaign", "A fast-growing electrolyte drink brand offers a clean commercial partnership.", "Commercial", 55, [("sign_beverage_deal", "Sign the partnership and stock club fridge with drinks", [("Money", 22000), ("Confidence", 4)]), ("decline_beverage", "Politely pass to avoid cluttering sponsorships", [("Happiness", 1)])]),
    ("lifestyle_quiet_reading", "Football Biographies & Tactics Books", "You spend a rainy afternoon reading biographies of Arrigo Sacchi and Johan Cruyff.", "Lifestyle", 60, [("absorb_tactical_wisdom", "Gain deep tactical inspiration from football history", [("Confidence", 6), ("Happiness", 5)]), ("watch_tv_series", "Binge-watch an action television drama instead", [("Happiness", 4)])]),
    ("locker_birthday_cake_tradition", "Locker Room Birthday Cake", "Teammates surprise you with a giant custom birthday cake after morning training.", "LockerRoom", 50, [("eat_slice_with_lads", "Enjoy a celebratory slice and laugh with teammates", [("Happiness", 10), ("Morale", 8)]), ("stick_to_protein_shake", "Politely decline cake due to strict match preparation", [("ManagerTrust", 4), ("Happiness", -2)])]),
    ("training_extra_heading_drills", "Aerial Challenge Practice", "The assistant manager offers a 20-minute drill on timing aerial headers from outswinging crosses.", "Training", 55, [("drill_headers", "Leap into 30 competitive crosses with power", [("Confidence", 6), ("Fatigue", 6), ("ManagerTrust", 5)]), ("protect_head", "Skip aerial impact drills today", [("Fatigue", -2)])]),
    ("family_brother_football_advice", "Guiding Younger Brother's Trials", "Your younger brother is trying out for an academy and calls asking for advice.", "Family", 50, [("train_with_brother", "Spend an afternoon coaching him on first-touch and body shape", [("Happiness", 9), ("Morale", 6), ("Fatigue", 3)]), ("give_phone_pep_talk", "Give him an encouraging 20-minute phone call", [("Happiness", 5)])]),
    ("lifestyle_city_sightseeing", "Exploring City History", "On an off-day, you wander the historic quarter of your club's city incognito with a baseball cap.", "Lifestyle", 55, [("walk_the_canals", "Enjoy a peaceful coffee overlooking the historic riverfront", [("Happiness", 8), ("Fatigue", -3)]), ("stay_indoors", "Rest up on the living room sofa", [("Fatigue", -4)])]),
    ("media_overseas_fan_club", "Overseas Fan Club Chapter", "An international supporter group from Tokyo sends a personalized silk banner with your name.", "Media", 45, [("record_thank_you_video", "Record a heartfelt video greeting thanking Japanese fans", [("Happiness", 8), ("Morale", 6)]), ("autograph_banner", "Sign the banner and mail it back to Tokyo", [("Happiness", 5)])]),
    ("training_virtual_reality_tactics", "VR Cognitive Training System", "The club trials a virtual reality headset simulating pitch scanning and decision speed.", "Training", 50, [("test_vr_system", "Complete 30 minutes of high-speed cognitive scanning drills", [("Confidence", 6), ("ManagerTrust", 4)]), ("prefer_pitch_reps", "Opt for live ball work instead", [("Confidence", 3)])]),
    ("commercial_local_bakery", "Local Neighborhood Bakery Endorsement", "A beloved bakery next to the stadium asks to name their artisan pre-match bread after you.", "Commercial", 50, [("support_local_bakers", "Endorse the local bakery free of charge for community goodwill", [("Morale", 9), ("Happiness", 8)]), ("ask_for_commercial_fee", "Request standard image rights fee", [("Money", 5000), ("Morale", -2)])]),
    ("locker_kitman_appreciation", "Kit Staff Christmas Gift", "You notice how tirelessly the elderly club kit manager works washing muddy gear every evening.", "LockerRoom", 45, [("gift_luxury_watch", "Gift him a luxury watch and a signed match ball from the squad", [("Money", -5000), ("Morale", 10), ("Happiness", 8)]), ("thank_him_verbally", "Give him a warm hug and heartfelt thank you", [("Morale", 5)])]),
    ("media_documentary_archive", "Reviewing Debut Match Footage", "Ten years after your first professional minute, sports channels broadcast a special retrospective.", "Media", 35, [("reflect_with_gratitude", "Watch the old clips and appreciate how far you've come", [("Happiness", 9), ("Confidence", 7)]), ("focus_on_future", "Stay hungry for the trophies still to be won", [("Confidence", 5)])])
]

# Generate additional template-based events
for tid, title, desc, cat, weight, choices_data in extra_templates:
    choices = []
    for cid, text, effs in choices_data:
        eff_list = []
        for target, delta in effs:
            eff_list.append({
                "target": target,
                "delta": delta,
                "description": f"Adjusted {target} by {delta:+}"
            })
        choices.append({
            "id": cid,
            "text": text,
            "effects": eff_list
        })
    ev = {
        "id": tid,
        "title": title,
        "description": desc,
        "category": cat,
        "weight": weight,
        "cooldownWeeks": 6,
        "conditions": {},
        "choices": choices
    }
    new_events.append(ev)

# Append new events if id not existing
count_added = 0
for ne in new_events:
    if ne["id"] not in existing_ids:
        events.append(ne)
        existing_ids.add(ne["id"])
        count_added += 1

print(f"Added {count_added} new events. Total events now: {len(events)}.")

output = {
    "$schema": "./schema/events.schema.json",
    "events": events
}

with open("content/data/events.json", "w") as f:
    json.dump(output, f, indent=2)

print("Saved content/data/events.json successfully!")
