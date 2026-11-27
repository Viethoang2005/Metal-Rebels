#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MetalRebels.Core.Data;
using MetalRebels.Player;
using UnityEditor;
using UnityEngine;

namespace MetalRebels.Player.EditorTools
{
    public static class WeaponDamagePlayModeTest
    {
        private readonly struct WeaponTestCase
        {
            public readonly WeaponType Type;
            public readonly int ExpectedDamage;

            public WeaponTestCase(WeaponType type, int expectedDamage)
            {
                Type = type;
                ExpectedDamage = expectedDamage;
            }
        }

        private static readonly WeaponTestCase[] TestCases =
        {
            new WeaponTestCase(WeaponType.Pistol, 20),
            new WeaponTestCase(WeaponType.Shotgun, 50),
            new WeaponTestCase(WeaponType.AssaultRifle, 15),
            new WeaponTestCase(WeaponType.RocketLauncher, 100)
        };

        private static PlayerCombat playerCombat;
        private static TargetDummy target;
        private static int testIndex;
        private static int startingHP;
        private static int passedTests;
        private static int failedTests;
        private static double nextTestTime;
        private static double timeoutAt;
        private static bool waitingForDamage;
        private static double startTime;
        private static readonly HashSet<UnityEngine.Object> existingProjectiles = new HashSet<UnityEngine.Object>();

        [MenuItem("MetalRebels/Module Player/Run Weapon Damage Play Mode Test %&w")]
        public static void Run()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
                EditorApplication.isPlaying = true;
                return;
            }

            BeginTest();
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
            playerCombat = null;
            target = null;
            testIndex = 0;
            passedTests = 0;
            failedTests = 0;
            waitingForDamage = false;
            startTime = EditorApplication.timeSinceStartup;
            nextTestTime = startTime;
            EditorApplication.update += UpdateTest;
        }

        private static void UpdateTest()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update -= UpdateTest;
                return;
            }

            playerCombat ??= UnityEngine.Object.FindAnyObjectByType<PlayerCombat>();
            target ??= GameObject.Find("Enemy_WeaponDamage_Test")?.GetComponent<TargetDummy>();

            if (playerCombat == null || target == null || target.CurrentHP <= 0)
            {
                if (EditorApplication.timeSinceStartup - startTime > 5d)
                {
                    Debug.LogError("[Weapon Damage Test] Marco or Enemy_WeaponDamage_Test is unavailable or not initialized.");
                    EditorApplication.update -= UpdateTest;
                }
                return;
            }

            if (testIndex >= TestCases.Length)
            {
                if (failedTests == 0)
                {
                    Debug.Log($"<color=green>[Weapon Damage Test]</color> All {passedTests} weapon damage checks passed in Play Mode.");
                }
                else
                {
                    Debug.LogError($"[Weapon Damage Test] {failedTests} of {TestCases.Length} checks failed; {passedTests} passed.");
                }
                EditorApplication.update -= UpdateTest;
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            if (!waitingForDamage)
            {
                if (now < nextTestTime) return;

                WeaponTestCase testCase = TestCases[testIndex];
                Debug.Log($"[Weapon Damage Test] Firing {testCase.Type} at {target.name}.");
                if (testCase.Type != WeaponType.Pistol)
                {
                    playerCombat.TryAddAmmo(testCase.Type, 30);
                }

                int slotIndex = FindWeaponSlot(playerCombat, testCase.Type);
                if (slotIndex < 0)
                {
                    Debug.LogError($"[Weapon Damage Test] {testCase.Type} is not configured on Marco.");
                    failedTests++;
                    testIndex++;
                    nextTestTime = now + 0.25d;
                    return;
                }

                playerCombat.EquipWeaponByIndex(slotIndex);
                if (playerCombat.CurrentWeapon?.weaponType != testCase.Type)
                {
                    Debug.LogError($"[Weapon Damage Test] Could not equip {testCase.Type}.");
                    failedTests++;
                    testIndex++;
                    nextTestTime = now + 0.25d;
                    return;
                }

                target.ResetDummy();
                Vector2 aimDirection = playerCombat.GetAimDirection();
                bool usesShotgunForwardFirePoint =
                    testCase.Type == WeaponType.Shotgun
                    || testCase.Type == WeaponType.AssaultRifle;
                MethodInfo spawnPointMethod = typeof(PlayerCombat).GetMethod(
                    "GetSpawnPoint",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Transform spawnPoint = spawnPointMethod?.Invoke(
                    playerCombat,
                    new object[] { aimDirection, usesShotgunForwardFirePoint }) as Transform;
                Vector3 muzzlePosition = spawnPoint != null ? spawnPoint.position : playerCombat.transform.position;
                target.transform.position = muzzlePosition + (Vector3)(aimDirection * 0.5f);
                Collider2D targetCollider = target.GetComponent<Collider2D>();
                if (targetCollider != null)
                {
                    targetCollider.isTrigger = true;
                }
                if (targetCollider is BoxCollider2D boxCollider)
                {
                    boxCollider.offset = Vector2.zero;
                    boxCollider.size = Vector2.one * 1.5f;
                }
                startingHP = target.CurrentHP;
                CaptureExistingProjectiles();
                MethodInfo fireMethod = typeof(PlayerCombat).GetMethod(
                    "FireCurrentWeapon",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (fireMethod == null)
                {
                    Debug.LogError("[Weapon Damage Test] PlayerCombat.FireCurrentWeapon was not found.");
                    EditorApplication.update -= UpdateTest;
                    return;
                }

                try
                {
                    fireMethod.Invoke(playerCombat, null);
                }
                catch (TargetInvocationException exception)
                {
                    Debug.LogException(exception.InnerException ?? exception);
                    EditorApplication.update -= UpdateTest;
                    return;
                }

                timeoutAt = now + 3d;
                waitingForDamage = true;
                return;
            }

            int actualDamage = startingHP - target.CurrentHP;
            if (actualDamage > 0)
            {
                WeaponTestCase testCase = TestCases[testIndex];
                string result = actualDamage == testCase.ExpectedDamage ? "PASS" : "FAIL";
                string log = $"[{result}] {testCase.Type}: expected {testCase.ExpectedDamage}, got {actualDamage} damage.";
                if (actualDamage == testCase.ExpectedDamage)
                {
                    Debug.Log($"<color=green>[Weapon Damage Test]</color> {log}");
                    passedTests++;
                }
                else
                {
                    Debug.LogError($"[Weapon Damage Test] {log}");
                    failedTests++;
                }

                CleanupTestProjectiles();
                waitingForDamage = false;
                testIndex++;
                nextTestTime = now + 0.5d;
            }
            else if (now >= timeoutAt)
            {
                Debug.LogError($"[Weapon Damage Test] {TestCases[testIndex].Type} did not damage the target within 3 seconds.");
                failedTests++;
                CleanupTestProjectiles();
                waitingForDamage = false;
                testIndex++;
                nextTestTime = now + 0.5d;
            }
        }

        private static void CaptureExistingProjectiles()
        {
            existingProjectiles.Clear();
            foreach (PlayerBullet bullet in UnityEngine.Object.FindObjectsByType<PlayerBullet>())
            {
                existingProjectiles.Add(bullet);
            }
            foreach (PlayerRocket rocket in UnityEngine.Object.FindObjectsByType<PlayerRocket>())
            {
                existingProjectiles.Add(rocket);
            }
        }

        private static void CleanupTestProjectiles()
        {
            foreach (PlayerBullet bullet in UnityEngine.Object.FindObjectsByType<PlayerBullet>())
            {
                if (!existingProjectiles.Contains(bullet))
                {
                    UnityEngine.Object.Destroy(bullet.gameObject);
                }
            }
            foreach (PlayerRocket rocket in UnityEngine.Object.FindObjectsByType<PlayerRocket>())
            {
                if (!existingProjectiles.Contains(rocket))
                {
                    UnityEngine.Object.Destroy(rocket.gameObject);
                }
            }
            existingProjectiles.Clear();
        }

        private static int FindWeaponSlot(PlayerCombat combat, WeaponType type)
        {
            FieldInfo slotsField = typeof(PlayerCombat).GetField(
                "weaponSlots",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (slotsField?.GetValue(combat) is not IList slots)
            {
                return -1;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                object slot = slots[i];
                FieldInfo dataField = slot.GetType().GetField("weaponData");
                if (dataField?.GetValue(slot) is WeaponData data && data.weaponType == type)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
#endif
