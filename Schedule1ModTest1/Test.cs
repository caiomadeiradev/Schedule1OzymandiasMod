using MelonLoader;
using Il2CppScheduleOne.Audio;
using Il2CppScheduleOne.UI.Phone;
using System;
using System.Collections;

using UnityEngine;
using Il2CppScheduleOne.Persistence;
using UnityEngine.Events;
using Il2CppScheduleOne.Property;
using UnityEngine.Windows;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.Vehicles;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.NPCs;
using UnityEngine.AI;
using Il2CppFishNet.Object;
using UnityEngine.TextCore.Text;
using Il2CppScheduleOne.NPCs.CharacterClasses;
using Il2CppSystem.Numerics;
using Il2CppScheduleOne.Building.Doors;
using static Il2CppRootMotion.FinalIK.IKSolverVR;
using static UnityEngine.UI.Image;
using System.Runtime.InteropServices;
using Il2CppIO.Swagger.Model;
using Il2CppScheduleOne.NPCs.Schedules;
using Il2CppScheduleOne.NPCs.Actions;
using Il2CppGameKit.Utilities;

[assembly: MelonInfo(typeof(Schedule1ModTest1.Test), "Schedule1ModTest1", "1.0.0", "cmadev", null)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace Schedule1ModTest1
{

    public static class BuildInfo
    {
        public const string Name = "Schedule Mod test1";
        public const string Description = "Aaaaa";
        public const string Author = "cmaddev";
        public const string Company = null;
        public const string Version = "1.0";
        public const string DownloadLink = null;
    }
    public class Test : MelonMod
    {
        private bool showText = false;
        private GameObject enemyGO;
        private NPC enemyNPC;
        private Dealer targetDealer;

        public List<string> npcIds = new()
        {
            "ming", "jessi_waters"
        };

        private List<Dealer> activeDealers = new List<Dealer>();

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Iniciou!!!");
        }

        public override void OnGUI()
        {
            if (showText)
            {
               GUI.Label(new Rect(50, 50, 300, 30), $"Player position: {Player.Local.transform.position}");
            }
        }

        public override void OnUpdate()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.I))
            {
                showText = !showText;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.K))
            {
                Vector3 dockWareHouseFront = new Vector3(x: -83.82198f, y: -2.1f, z: -35.59542f);
                // Player position: (-57.53, 0.98, 90.73) 

                Vector3 frontOfMotelRoom = new Vector3(x: -57.53f, y: 0.98f, z: 90.73f);
                targetDealer = GetMovingDealer();

                if (targetDealer != null)
                {
                    enemyGO = CreateEnemyGO("albert_hoover");
                    if (enemyGO != null)
                    {
                        ConfigureNPC(enemyGO, "cuzildo_da_silva", "Cuzildo", "Da Silva", frontOfMotelRoom);
                        if (enemyNPC != null)
                            MelonCoroutines.Start(MakeNPCAggressive(enemyNPC, targetDealer));
                        else { LoggerInstance.Msg("enemyNPC is null. Cant start Coroutines."); }

                    } else { LoggerInstance.Msg("enemyGO is null."); }

                } else { LoggerInstance.Msg("targetDealer is null."); }
            }
        }

        private GameObject CreateEnemyGO(string baseNPCName)
        {
            // albert_hoover
            var baseNPC = NPCManager.GetNPC(baseNPCName);
            if (baseNPC == null)
            {
                LoggerInstance.Msg($"BaseNPC with name: {baseNPCName} not found.");
                return null;
            }

            // Duplicate the NPC GameObject
            enemyGO = UnityEngine.Object.Instantiate(baseNPC.gameObject);
            if (enemyGO == null)
            {
                LoggerInstance.Msg($"CreateEnemyGO: enemyGO is null.");
                return null;
            }
            return enemyGO;
        }

        private void SetSpawnLocation(GameObject enemyGO, Vector3 location)
        {
            if (enemyGO != null)
            {
                // Set position
                enemyGO.transform.position = location;
                enemyGO.transform.parent = NPCManager.Instance.NPCContainer;
            } else {
                LoggerInstance.Msg($"SetSpawnLocation: enemyGO is null.");
                return;
            }
        }

        private void ConfigureNPC(GameObject enemyGO, string newId, string newFirstName, string newLastName, Vector3 spawnLocation)
        {
            if (enemyGO != null) 
            {
                enemyNPC = enemyGO.GetComponent<NPC>();
                if (enemyNPC == null)
                {
                    LoggerInstance.Msg($"ConfigureNPC: enemyNPC is null");
                    return;
                }
                // New ID
                if (NPCManager.GetNPC(newId) != null)
                {
                    LoggerInstance.Msg($"Enemy with {newId} already exists.");
                    return;
                }
                // Assing a new ID and name
                enemyNPC.ID = newId;
                enemyNPC.name = newFirstName + " " + newLastName;
                enemyNPC.FirstName = newFirstName;
                enemyNPC.LastName = newLastName;

                // Set spawn position
                SetSpawnLocation(enemyGO, spawnLocation);

                // Make sure navmeshagent and network obj are on
                var nav = enemyGO.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (nav == null)
                {
                    LoggerInstance.Msg($"nav is null.");
                    return;
                }
                if (nav != null) nav.enabled = true;

                var netObj = enemyGO.GetComponent<Il2CppFishNet.Object.NetworkObject>();
                if (netObj == null)
                {
                    LoggerInstance.Msg($"netObj is null.");
                    return;
                }
                if (netObj != null) netObj.enabled = false;

                // activate the game object
                enemyGO.SetActive(true);

                // Configure NPC - visual
                enemyNPC.SetVisible(true);
                enemyNPC.Movement.enabled = true;
                enemyNPC.CanOpenDoors = true;
                enemyNPC.Avatar.EmotionManager.AddEmotionOverride("Zombie", "deal_rejected", 30f, 0);
                enemyNPC.Movement.RunSpeed = 10f;
                enemyNPC.Movement.WalkSpeed = 10.0f;

                // register with ncpmanager
                NPCManager.NPCRegistry.Add(enemyNPC);

                MelonLogger.Msg($"{newFirstName} spawned and activated.");
                
            } else {
                LoggerInstance.Msg($"EnemyGO is null.");
                return;
            }
            
        }

        private Dealer GetMovingDealer()
        {
            activeDealers = GetActiveDealers();
            List<Dealer> movingDealers = new List<Dealer>();
            if (activeDealers.Count > 0)
            {
                foreach (var dealer in activeDealers)
                {
                    if (dealer.Movement.IsMoving)
                    {
                        movingDealers.Add(dealer);
                    }
                }
            }
            else
            {
                LoggerInstance.Msg("GetActiveDealers: No dealer in activeDealers.");
                return null;
            }

            if (movingDealers.Count > 1)
            {
                int rndIndex = UnityEngine.Random.Range(0, movingDealers.Count);
                return movingDealers[rndIndex];
            }
            else { return movingDealers[0]; }
        }

        private System.Collections.IEnumerator MakeNPCAggressive(NPC npc, Dealer targetDealer)
        {
            //Dealer targetDealer = GetMovingDealer();

                LoggerInstance.Msg($"Random Dealer Choiced: {targetDealer.FirstName}");
                targetDealer.defaultAggression = 10f;

                while (npc != null && npc.NetworkObject != null && npc.Health.Health > 0.0f && Player.Local != null &&
                    targetDealer != null)
                {
                    Vector3 npcPos = npc.transform.position;
                    Vector3 dealerPos = targetDealer.transform.position;
                    Vector3 playerPos = Player.Local.transform.position;

                    float distanceToPlayer = Vector3.Distance(npcPos, dealerPos);
                    float distanceToDealer = Vector3.Distance(npcPos, playerPos);
                    // LoggerInstance.Msg($"Dealer Pos: {targetDealer.FirstName} - {dealerPos}");

                    if (distanceToDealer > 5f)
                    {
                        LoggerInstance.Msg($"Indo ate o Dealer: {targetDealer.FirstName} que esta se movendo {targetDealer.Movement.IsMoving}...");
                        npc.Movement.GetClosestReachablePoint(targetDealer.transform.position, out Vector3 pos);
                        npc.Movement.SetDestination(dealerPos);
                    } else {
                        LoggerInstance.Msg("O inimigo chegou até o dealer.");
                        npc.Movement.Stop();
                        BeginAgressive(npc, targetDealer.NetworkObject);
                    }
                    yield return new WaitForSeconds(2f); // verifica a cada x segundos
                }
                LoggerInstance.Msg("The NPC is Dead");
        }

        private void BeginAgressive(NPC npc, NetworkObject netObj)
        {
            LoggerInstance.Msg("Comecando a agressividade...");
            npc.behaviour.CombatBehaviour.SetTarget(null, netObj);
            npc.OverrideAggression(10.0f);
            npc.behaviour.CombatBehaviour.VirtualPunchWeapon.Damage = UnityEngine.Random.Range(0.01f, 0.08f);
            npc.behaviour.CombatBehaviour.Enable_Networked(null);
            LoggerInstance.Msg("Encerrando a agressividade...");
        }

        private List<Dealer> GetActiveDealers()
        {
            var dealers = GameObject.FindObjectsOfType<Dealer>();
            foreach (var dealer in dealers)
            {
                if (dealer.IsRecruited && dealer.isActiveAndEnabled && dealer.GetTotalInventoryItemCount() > 0)
                {
                    activeDealers.Add(dealer);
                }
            }
            return activeDealers;
        }
    }
}