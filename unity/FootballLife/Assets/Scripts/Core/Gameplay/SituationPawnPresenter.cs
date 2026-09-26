using System.Collections.Generic;
using UnityEngine;
using FootballLife.Domain;
using FootballLife.Unity.Core.Camera;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Spawns, arranges, and manages 3D match situations driven by simulation events.
    /// Sets up User Striker, Supporting Teammate, Opponent Defenders, and Goalkeeper,
    /// wires touch gestures, aim trajectory guide, shooting, and passing interactions.
    /// </summary>
    public sealed class SituationPawnPresenter : MonoBehaviour
    {
        [Header("Spawned Pawns")]
        [SerializeField] private MatchPawn? _userPlayer;
        [SerializeField] private MatchPawn? _teammate;
        [SerializeField] private MatchPawn? _defender1;
        [SerializeField] private MatchPawn? _defender2;
        [SerializeField] private MatchPawn? _goalkeeper;

        [Header("Interactions")]
        [SerializeField] private ShootingInteraction? _shooting;
        [SerializeField] private PassingInteraction? _passing;
        [SerializeField] private AimTrajectoryRenderer? _trajectoryGuide;
        [SerializeField] private TouchGestureController? _touchGesture;

        public MatchPawn? UserPlayer => _userPlayer;
        public MatchPawn? Teammate => _teammate;
        public MatchPawn? Goalkeeper => _goalkeeper;
        public MatchPawn[] Defenders => new[] { _defender1!, _defender2! };

        /// <summary>
        /// Builds and spawns the full attacking situation pawn layout.
        /// </summary>
        public static SituationPawnPresenter SetupMatchSituationPawns(
            Transform entitiesRoot,
            BallController? ball,
            MatchCameraRig? cameraRig,
            Transform? goalTarget)
        {
            var pawnsParent = entitiesRoot.Find("Pawns");
            if (pawnsParent != null)
            {
                Object.DestroyImmediate(pawnsParent.gameObject);
            }

            var pawnsRoot = new GameObject("Pawns");
            pawnsRoot.transform.SetParent(entitiesRoot, false);

            var presenter = pawnsRoot.AddComponent<SituationPawnPresenter>();

            // 1. User Player (Striker)
            var userPos = new Vector3(0f, 0f, 13.5f);
            var userRot = Quaternion.Euler(0f, 0f, 0f); // Facing opponent goal
            var userProfile = PlayerVisualProfile.CreateDeterministic("Marcus Vance", 9, Position.ST);
            var userGo = HumanoidPawnBuilder.CreatePawn(
                pawnsRoot.transform,
                userPos,
                userRot,
                "Pawn_User_Striker",
                PawnKitScheme.HomeOutfield,
                hasSelectionRing: true,
                visualProfile: userProfile,
                squadNumber: 9,
                isGoalkeeper: false
            );
            var userPawn = userGo.GetComponent<MatchPawn>();
            userPawn.Initialize("Marcus Vance", 9, PawnRole.UserStriker, PawnTeam.Home);
            userPawn.SetSelected(true);
            presenter._userPlayer = userPawn;

            // 2. Supporting Teammate (Right Winger / Midfielder making run)
            var tmPos = new Vector3(11.5f, 0f, 18.5f);
            var tmRot = Quaternion.Euler(0f, -20f, 0f);
            var tmProfile = PlayerVisualProfile.CreateDeterministic("Liam Sterling", 11, Position.RW);
            var tmGo = HumanoidPawnBuilder.CreatePawn(
                pawnsRoot.transform,
                tmPos,
                tmRot,
                "Pawn_Teammate_Support",
                PawnKitScheme.HomeOutfield,
                hasSelectionRing: false,
                visualProfile: tmProfile,
                squadNumber: 11,
                isGoalkeeper: false
            );
            var tmPawn = tmGo.GetComponent<MatchPawn>();
            tmPawn.Initialize("Liam Sterling", 11, PawnRole.Teammate, PawnTeam.Home);
            var tmController = tmGo.GetComponent<PlayerPawnController>();
            tmController.SetState(PawnAnimState.Jog);
            presenter._teammate = tmPawn;

            // 3. Opponent Center Back 1 (Left CB jockeying)
            var cb1Pos = new Vector3(-3.5f, 0f, 22.5f);
            var cb1Rot = Quaternion.Euler(0f, 180f, 0f);
            var cb1Profile = PlayerVisualProfile.CreateDeterministic("Diego Silva", 4, Position.CB);
            var cb1Go = HumanoidPawnBuilder.CreatePawn(
                pawnsRoot.transform,
                cb1Pos,
                cb1Rot,
                "Pawn_Opponent_CB1",
                PawnKitScheme.AwayOutfield,
                hasSelectionRing: false,
                visualProfile: cb1Profile,
                squadNumber: 4,
                isGoalkeeper: false
            );
            var cb1Pawn = cb1Go.GetComponent<MatchPawn>();
            cb1Pawn.Initialize("Diego Silva", 4, PawnRole.Defender, PawnTeam.Away);
            var cb1Controller = cb1Go.GetComponent<PlayerPawnController>();
            cb1Controller.SetState(PawnAnimState.Idle);
            presenter._defender1 = cb1Pawn;

            // 4. Opponent Center Back 2 (Right CB covering)
            var cb2Pos = new Vector3(4.0f, 0f, 23.5f);
            var cb2Rot = Quaternion.Euler(0f, 180f, 0f);
            var cb2Profile = PlayerVisualProfile.CreateDeterministic("Klaus Weber", 5, Position.CB);
            var cb2Go = HumanoidPawnBuilder.CreatePawn(
                pawnsRoot.transform,
                cb2Pos,
                cb2Rot,
                "Pawn_Opponent_CB2",
                PawnKitScheme.AwayOutfield,
                hasSelectionRing: false,
                visualProfile: cb2Profile,
                squadNumber: 5,
                isGoalkeeper: false
            );
            var cb2Pawn = cb2Go.GetComponent<MatchPawn>();
            cb2Pawn.Initialize("Klaus Weber", 5, PawnRole.Defender, PawnTeam.Away);
            var cb2Controller = cb2Go.GetComponent<PlayerPawnController>();
            cb2Controller.SetState(PawnAnimState.Idle);
            presenter._defender2 = cb2Pawn;

            // 5. Opponent Goalkeeper (Positioned on goal line at Z = 34.8m)
            var gkPos = new Vector3(0f, 0f, 34.8f);
            var gkRot = Quaternion.Euler(0f, 180f, 0f);
            var gkProfile = PlayerVisualProfile.CreateDeterministic("Oliver Kahn", 1, Position.GK);
            var gkGo = HumanoidPawnBuilder.CreatePawn(
                pawnsRoot.transform,
                gkPos,
                gkRot,
                "Pawn_Opponent_GK",
                PawnKitScheme.Goalkeeper,
                hasSelectionRing: false,
                visualProfile: gkProfile,
                squadNumber: 1,
                isGoalkeeper: true
            );
            var gkPawn = gkGo.GetComponent<MatchPawn>();
            gkPawn.Initialize("Oliver Kahn", 1, PawnRole.Goalkeeper, PawnTeam.Away);

            var defaultController = gkGo.GetComponent<PlayerPawnController>();
            if (defaultController != null)
            {
                Object.DestroyImmediate(defaultController);
            }
            var gkController = gkGo.AddComponent<GoalkeeperController>();
            presenter._goalkeeper = gkPawn;

            // ── Trajectory Guide & Reticle ──────────────────────────────────────
            var guideGo = new GameObject("Aim_Trajectory_Guide");
            guideGo.transform.SetParent(pawnsRoot.transform, false);
            var trajGuide = guideGo.AddComponent<AimTrajectoryRenderer>();
            presenter._trajectoryGuide = trajGuide;

            // ── Touch Gesture Controller ────────────────────────────────────────
            var inputGo = GameObject.Find("TouchGestureController");
            if (inputGo == null)
            {
                inputGo = new GameObject("TouchGestureController");
                var managers = GameObject.Find("[MANAGERS]");
                if (managers != null) inputGo.transform.SetParent(managers.transform, false);
            }
            var gestureCtrl = inputGo.GetComponent<TouchGestureController>() ?? inputGo.AddComponent<TouchGestureController>();
            presenter._touchGesture = gestureCtrl;

            // ── Shooting Interaction ────────────────────────────────────────────
            var shootGo = new GameObject("ShootingInteraction");
            shootGo.transform.SetParent(pawnsRoot.transform, false);
            var shooting = shootGo.AddComponent<ShootingInteraction>();
            var strikerCtrl = userGo.GetComponent<PlayerPawnController>();
            shooting.Initialize(ball, strikerCtrl, trajGuide, cameraRig);
            presenter._shooting = shooting;

            // ── Passing Interaction ─────────────────────────────────────────────
            var passGo = new GameObject("PassingInteraction");
            passGo.transform.SetParent(pawnsRoot.transform, false);
            var passing = passGo.AddComponent<PassingInteraction>();
            passing.Initialize(ball, strikerCtrl, tmPawn, new[] { cb1Pawn, cb2Pawn });
            presenter._passing = passing;

            // Connect camera rig targets
            if (cameraRig != null && ball != null)
            {
                cameraRig.SetTargets(userGo.transform, ball.transform, goalTarget);
            }

            return presenter;
        }

        /// <summary>
        /// Applies situation positions corresponding to simulation domain SituationType.
        /// </summary>
        public void ApplySituationPreset(SituationType type, BallController? ball, MatchCameraRig? cameraRig)
        {
            if (_userPlayer == null || ball == null) return;

            switch (type)
            {
                case SituationType.ReceivingInBox:
                case SituationType.OpenPlay:
                    // Central Box Shot
                    _userPlayer.transform.position = new Vector3(0f, 0f, 13.5f);
                    _userPlayer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    ball.ResetBall(new Vector3(0f, 0.11f, 15.0f));

                    if (_teammate != null) _teammate.transform.position = new Vector3(11.5f, 0f, 18.5f);
                    if (_defender1 != null) _defender1.transform.position = new Vector3(-3.5f, 0f, 22.5f);
                    if (_defender2 != null) _defender2.transform.position = new Vector3(4.0f, 0f, 23.5f);
                    if (_goalkeeper != null) _goalkeeper.transform.position = new Vector3(0f, 0f, 34.8f);
                    break;

                case SituationType.OneOnOne:
                    // Clean Breakaway
                    _userPlayer.transform.position = new Vector3(0f, 0f, 18.0f);
                    _userPlayer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    ball.ResetBall(new Vector3(0f, 0.11f, 19.5f));

                    // Defenders trailing behind
                    if (_defender1 != null) _defender1.transform.position = new Vector3(-4.0f, 0f, 12.0f);
                    if (_defender2 != null) _defender2.transform.position = new Vector3(4.0f, 0f, 12.0f);
                    if (_teammate != null) _teammate.transform.position = new Vector3(-10.0f, 0f, 16.0f);
                    if (_goalkeeper != null) _goalkeeper.transform.position = new Vector3(0f, 0f, 33.5f); // Rushing off line
                    break;

                case SituationType.Cross:
                    // Wing Cross setup
                    if (_teammate != null)
                    {
                        _teammate.transform.position = new Vector3(16.0f, 0f, 24.0f);
                        _teammate.transform.rotation = Quaternion.Euler(0f, -60f, 0f);
                    }
                    _userPlayer.transform.position = new Vector3(-1.5f, 0f, 25.0f); // Striker attacking cross
                    _userPlayer.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
                    ball.ResetBall(new Vector3(15.0f, 0.11f, 24.0f));

                    if (_defender1 != null) _defender1.transform.position = new Vector3(0f, 0f, 26.0f);
                    if (_defender2 != null) _defender2.transform.position = new Vector3(2.5f, 0f, 27.0f);
                    if (_goalkeeper != null) _goalkeeper.transform.position = new Vector3(0.5f, 0f, 34.8f);
                    break;

                case SituationType.ThroughBall:
                    // Ball rolling into space
                    _userPlayer.transform.position = new Vector3(0f, 0f, 11.0f);
                    _userPlayer.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    ball.ResetBall(new Vector3(0f, 0.11f, 17.5f));

                    if (_teammate != null) _teammate.transform.position = new Vector3(-8.0f, 0f, 10.0f);
                    if (_defender1 != null) _defender1.transform.position = new Vector3(2.5f, 0f, 16.0f);
                    if (_defender2 != null) _defender2.transform.position = new Vector3(-4.0f, 0f, 21.0f);
                    if (_goalkeeper != null) _goalkeeper.transform.position = new Vector3(0f, 0f, 34.8f);
                    break;
            }

            if (cameraRig != null)
            {
                cameraRig.SetMode(MatchCameraRig.CameraMode.ActionAim);
                cameraRig.SnapToTarget();
            }
        }
    }
}
