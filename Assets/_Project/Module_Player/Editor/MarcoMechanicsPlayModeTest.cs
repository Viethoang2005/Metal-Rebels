#if UNITY_EDITOR
using System.Reflection;
using MetalRebels.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalRebels.Player.EditorTools
{
    public static class MarcoMechanicsPlayModeTest
    {
        private static PlayerController player;
        private static PlayerCombat combat;
        private static double nextCheckTime;
        private static int phase;
        private static int passed;
        private static int failed;
        private static double testStartedAt;

        [MenuItem("MetalRebels/Module Player/Run Marco Mechanics Test %&m")]
        public static void Run()
        {
            if (!EditorApplication.isPlaying)
            {
                EnsureSandboxScene();
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
                EditorApplication.isPlaying = true;
                return;
            }

            BeginTest();
        }

        private static void EnsureSandboxScene()
        {
            const string sandboxPath = "Assets/_Project/Module_Player/Sandbox_VietHoang.unity";

            PlayerSetupTool.SetupAll();

            Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (activeScene.path != sandboxPath)
            {
                if (!System.IO.File.Exists(sandboxPath))
                {
                    PlayerSetupTool.CreateSandboxScene();
                }
                else
                {
                    EditorSceneManager.OpenScene(sandboxPath, OpenSceneMode.Single);
                }
            }

            Debug.Log("[Marco Mechanics Test] Opening Sandbox_VietHoang before Play Mode.");
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                BeginTest();
            }
        }

        private static void BeginTest()
        {
            EditorApplication.update -= UpdateTest;
            player = null;
            combat = null;
            nextCheckTime = 0d;
            phase = 0;
            passed = 0;
            failed = 0;
            testStartedAt = EditorApplication.timeSinceStartup;
            Debug.Log("<color=cyan>[Marco Mechanics Test]</color> Test started.");
            EditorApplication.update += UpdateTest;
        }

        private static void UpdateTest()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update -= UpdateTest;
                return;
            }

            player ??= Object.FindAnyObjectByType<PlayerController>();
            combat ??= Object.FindAnyObjectByType<PlayerCombat>();
            if (player == null || combat == null)
            {
                if (EditorApplication.timeSinceStartup - testStartedAt > 5d)
                {
                    Debug.LogError("[Marco Mechanics Test] Marco was not found in the Play Mode scene.");
                    EditorApplication.update -= UpdateTest;
                }
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            if (now < nextCheckTime)
            {
                return;
            }

            switch (phase)
            {
                case 0:
                    Check(player.CurrentHP == player.MaxHP, "Marco starts at max HP");
                    Check(player.CurrentLives == 3, "Marco starts with 3 lives");
                    Check(combat.CurrentGrenades == 5, "Marco starts with 5 grenades");
                    Check(combat.CurrentWeapon != null && combat.CurrentWeapon.weaponType == Core.Data.WeaponType.Pistol,
                        "Marco starts with Pistol");
                    phase++;
                    player.TakeDamage(20);
                    nextCheckTime = now + 0.1d;
                    break;

                case 1:
                    Check(player.CurrentHP == 80, "Damage reduces Marco HP by 20");
                    Check(player.IsInvincible, "Damage starts temporary invincibility");
                    Check(!player.CanShoot, "Marco cannot shoot during invincibility");
                    phase++;
                    nextCheckTime = now + 1.1d;
                    break;

                case 2:
                    Check(!player.IsInvincible, "Damage invincibility expires");
                    Check(player.CanShoot, "Marco can shoot after invincibility");
                    Check(player.TryHeal(30), "Health pickup heals Marco");
                    Check(player.CurrentHP == 100, "Health pickup restores Marco to max HP");
                    player.SetCheckpoint(new Vector3(4f, 1f, 0f));
                    phase++;
                    nextCheckTime = now + 0.1d;
                    break;

                case 3:
                    Check(combat.TryAddAmmo(Core.Data.WeaponType.Shotgun, 20),
                        "Shotgun ammo pickup can unlock ammo");
                    Check(combat.TryAddAmmo(Core.Data.WeaponType.AssaultRifle, 30),
                        "Assault rifle ammo pickup can unlock ammo");
                    Check(combat.TryAddAmmo(Core.Data.WeaponType.RocketLauncher, 1),
                        "Rocket pickup can unlock ammo");
                    Check(combat.TryAddGrenades(1) == false,
                        "Grenade pickup respects max grenade count");
                    phase++;
                    nextCheckTime = now + 0.1d;
                    break;

                case 4:
                    Check(combat.TryAddGrenades(-1) == false,
                        "Invalid grenade amount is rejected");
                    Check(RocketIdleSpritesAssigned(),
                        "Rocket idle sprite frames are assigned to Marco");
                    Finish();
                    break;
            }
        }

        private static bool RocketIdleSpritesAssigned()
        {
            PlayerAnimation animation = Object.FindAnyObjectByType<PlayerAnimation>();
            if (animation == null)
            {
                return false;
            }

            FieldInfo field = typeof(PlayerAnimation).GetField(
                "rocketIdleSprites",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return field?.GetValue(animation) is Sprite[] sprites && sprites.Length >= 3;
        }

        private static void Check(bool condition, string description)
        {
            if (condition)
            {
                passed++;
                Debug.Log($"<color=green>[PASS]</color> Marco mechanics: {description}");
            }
            else
            {
                failed++;
                Debug.LogError($"[FAIL] Marco mechanics: {description}");
            }
        }

        private static void Finish()
        {
            EditorApplication.update -= UpdateTest;
            if (failed == 0)
            {
                Debug.Log($"<color=green>[Marco Mechanics Test]</color> All {passed} checks passed.");
            }
            else
            {
                Debug.LogError($"[Marco Mechanics Test] {failed} checks failed; {passed} passed.");
            }
        }
    }
}
#endif
