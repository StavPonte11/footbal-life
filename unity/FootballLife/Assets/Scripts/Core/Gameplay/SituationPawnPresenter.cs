using System.Collections.Generic;
using UnityEngine;
using FootballLife.Unity.Core.Camera;

namespace FootballLife.Unity.Core.Gameplay
{
    /// <summary>
    /// Spawns and configures situation-driven pawns on the pitch:
    /// User Striker, Supporting Teammate, Opponent Center-backs, and Opponent Goalkeeper.
    /// Integrates cleanly under [ENTITIES]/Pawns and binds targets to MatchCameraRig.
    /// </summary>
    public sealed class SituationPawnPresenter : MonoBehaviour
    {
        [Header("Spawned Pawns")]
        [SerializeField] private MatchPawn? _userPlayer;
        [SerializeField] private MatchPawn? _teammate;
        [SerializeField] private MatchPawn? _defender1;
        [SerializeField] private MatchPawn? _defender2;
        [SerializeField] private MatchPawn? _goalkeeper;

        public MatchPawn? UserPlayer => _userPlayer;
        public MatchPawn? Teammate => _teammate;
        public MatchPawn? Goalkeeper => _goalkeeper;

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
            var userGo = HumanoidPawnBuilder.CreatePawn(
                pawnsRoot.transform,
                userPos,
                userRot,
                "Pawn_User_Striker",
                PawnKitScheme.HomeOutfield,
                hasSelectionRing: true
            );
            var userPawn = userGo.GetComponent<MatchPawn>();
            userPawn.Initialize("Marcus Vance", 9, PawnRole.UserStriker, PawnTeam.Home);
            userPawn.SetSelected(true);
            presenter._userPlayer = userPawn;

            // 2. Supporting Teammate (Right Winger / Midfielder making run)
            var tmPos = new Vector3(11.5f, 0f, 18.5f);
            var tmRot = Quaternion.Euler(0f, -20f, 0f);
            var tmGo = HumanoidPawnBuilder.CreatePawn(
                pawnsRoot.transform,
                tmPos,
                tmRot,
                "Pawn_Teammate_Support",
                PawnKitScheme.HomeOutfield,
                hasSelectionRing: false
            );
            var tmPawn = tmGo.GetComponent<MatchPawn>();
            tmPawn.Initialize("Liam Sterling", 11, PawnRole.Teammate, PawnTeam.Home);
            var tmController = tmGo.GetComponent<PlayerPawnController>();
            tmController.SetState(PawnAnimState.Jog); // Making dynamic supporting run
            presenter._teammate = tmPawn;

            // 3. Opponent Center Back 1 (Left CB jockeying)
            var cb1Pos = new Vector3(-3.5f, 0f, 22.5f);
            var cb1Rot = Quaternion.Euler(0f, 180f, 0f); // Facing attacker
            var cb1Go = HumanoidPawnBuilder.CreatePawn(
                pawnsRoot.transform,
                cb1Pos,
                cb1Rot,
                "Pawn_Opponent_CB1",
                PawnKitScheme.AwayOutfield,
                hasSelectionRing: false
            );
            var cb1Pawn = cb1Go.GetComponent<MatchPawn>();
            cb1Pawn.Initialize("Diego Silva", 4, PawnRole.Defender, PawnTeam.Away);
            var cb1Controller = cb1Go.GetComponent<PlayerPawnController>();
            cb1Controller.SetState(PawnAnimState.Idle);
            presenter._defender1 = cb1Pawn;

            // 4. Opponent Center Back 2 (Right CB covering)
            var cb2Pos = new Vector3(4.0f, 0f, 23.5f);
            var cb2Rot = Quaternion.Euler(0f, 180f, 0f);
            var cb2Go = HumanoidPawnBuilder.CreatePawn(
                pawnsRoot.transform,
                cb2Pos,
                cb2Rot,
                "Pawn_Opponent_CB2",
                PawnKitScheme.AwayOutfield,
                hasSelectionRing: false
            );
            var cb2Pawn = cb2Go.GetComponent<MatchPawn>();
            cb2Pawn.Initialize("Klaus Weber", 5, PawnRole.Defender, PawnTeam.Away);
            var cb2Controller = cb2Go.GetComponent<PlayerPawnController>();
            cb2Controller.SetState(PawnAnimState.Idle);
            presenter._defender2 = cb2Pawn;

            // 5. Opponent Goalkeeper (Positioned on goal line at Z = 34.8m)
            var gkPos = new Vector3(0f, 0f, 34.8f);
            var gkRot = Quaternion.Euler(0f, 180f, 0f); // Facing incoming play
            var gkGo = HumanoidPawnBuilder.CreatePawn(
                pawnsRoot.transform,
                gkPos,
                gkRot,
                "Pawn_Opponent_GK",
                PawnKitScheme.Goalkeeper,
                hasSelectionRing: false
            );
            var gkPawn = gkGo.GetComponent<MatchPawn>();
            gkPawn.Initialize("Oliver Kahn", 1, PawnRole.Goalkeeper, PawnTeam.Away);

            // Replace standard player controller with specialized GoalkeeperController
            var defaultController = gkGo.GetComponent<PlayerPawnController>();
            if (defaultController != null)
            {
                Object.DestroyImmediate(defaultController);
            }

            var hips = gkGo.transform.Find("Hips");
            var torso = hips?.Find("Torso");
            var head = torso?.Find("Head");
            var leftArm = torso?.Find("Arm_L");
            var rightArm = torso?.Find("Arm_R");
            var leftLeg = hips?.Find("Leg_L");
            var rightLeg = hips?.Find("Leg_R");
            var leftFoot = leftLeg?.Find("Foot_L");
            var rightFoot = rightLeg?.Find("Foot_R");

            if (hips != null && torso != null && head != null && leftArm != null && rightArm != null &&
                leftLeg != null && rightLeg != null && leftFoot != null && rightFoot != null)
            {
                var gkRig = new PawnRigTransforms(
                    hips, torso, head, leftArm, rightArm, leftLeg, rightLeg, leftFoot, rightFoot
                );
                var gkController = gkGo.AddComponent<GoalkeeperController>();
                gkController.Initialize(gkRig, ball);
            }
            presenter._goalkeeper = gkPawn;

            // Connect camera rig targets
            if (cameraRig != null && ball != null)
            {
                cameraRig.SetTargets(userGo.transform, ball.transform, goalTarget);
            }

            return presenter;
        }
    }
}
