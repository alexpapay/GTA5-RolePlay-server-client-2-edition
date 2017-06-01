using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using MpRpServer.Data;
using MpRpServer.Data.Enums;
using MpRpServer.Data.Localize;
using MpRpServer.Server.Characters;
using MpRpServer.Server.DBManager;
using MpRpServer.Server.Weapon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MpRpServer.Server
{
    public class ConnectionController : Script
    {
        //public static readonly Vector3 _startPos = new Vector3(3433.339f, 5177.579f, 39.79541f);
        //public static readonly Vector3 _startCamPos = new Vector3(3476.85f, 5228.022f, 9.453369f);
        //public static readonly Vector3 _startPos = new Vector3(-1042.2, -2772.6, 4.639);
        public static readonly Vector3 StartPos = new Vector3(-1043.045, -2772.4, 4.639);
        private static readonly Vector3 StartRot = new Vector3(0.0, 0.0, 58.7041);
        public static readonly Vector3 StartCamPos = new Vector3(-1042.0, -2776.0, 4.639);
        public static readonly Vector3 StopCamPos = new Vector3(-1044.0, -2772.0, 5.3);

        public ConnectionController()
        {
            API.onPlayerConnected += OnPlayerConnectedHandler;
            API.onPlayerFinishedDownload += OnPlayerFinishedDownloadHandler;
            API.onPlayerDisconnected += OnPlayerDisconnectedHandler;
            API.onPlayerDeath += OnPlayerDeath;
        }

        private void OnPlayerDeath(Client player, NetHandle entityKiller, int weapon)
        {
            Character killerCharacter = null;
            Client killer = API.getPlayerFromHandle(entityKiller);
            if (killer != null)
            {                                           // TODO: Lokalize
                API.sendNotificationToAll(killer.name + Localize.Lang(2, "killed") + player.name);
                killerCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.SocialClub == killer.socialClubName);
                if (killerCharacter == null) return;

                var caption = ContextFactory.Instance.Caption.First(x => x.Id == 1);
                if (caption.Sector != "0;0")
                {
                    var getAttackGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == caption.GangAttack * 100);
                    if (getAttackGroup == null) return;
                    var groupAttackType = (GroupType)Enum.Parse(typeof(GroupType), getAttackGroup.Type.ToString());
                    var getDefendGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == caption.GangDefend * 100);
                    if (getDefendGroup == null) return;
                    var groupDefendType = (GroupType)Enum.Parse(typeof(GroupType), getDefendGroup.Type.ToString());

                    if (killerCharacter.GroupType == caption.GangAttack) caption.FragsAttack += 1;
                    if (killerCharacter.GroupType == caption.GangDefend) caption.FragsDefend += 1;
                    API.shared.sendChatMessageToAll(
                        "Фрагов у банды " + EntityManager.GetDisplayName(groupAttackType) + ": " + caption.FragsAttack + 
                        "\nФрагов у банды " + EntityManager.GetDisplayName(groupDefendType) + ": " + caption.FragsDefend);
                    ContextFactory.Instance.SaveChanges();
                }                
            }
            else
            {                                           // TODO: Lokalize
                API.sendNotificationToAll(player.name + Localize.Lang(2, "death"));
            }

            CharacterController characterController = player.getData("CHARACTER");            
            if (characterController == null) return;

            WeaponManager.SetPlayerWeapon(player, characterController.Character, 2);

            // Army change cloth after death
            if (CharacterController.IsCharacterInArmy(characterController))
            {                
                if (killerCharacter != null && CharacterController.IsCharacterInGhetto(killer))
                {     
                    switch (characterController.Character.ActiveClothes)
                    {
                        case 2: killerCharacter.ClothesTypes = 2;
                            API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_soldier")); break;
                        case 3: killerCharacter.ClothesTypes = 3;
                            API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_officer")); break;
                        case 4: killerCharacter.ClothesTypes = 4;
                            API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_general")); break;
                    }
                    ClothesManager.SetPlayerSkinClothesToDb(player, 101, characterController.Character, 1);

                    ContextFactory.Instance.SaveChanges();
                }
                else API.sendNotificationToAll("[DEBUG]: Ошибка передачи формы!");
            }            
            // Gang change clothes after death
            if (CharacterController.IsCharacterInGang(characterController) && 
                CharacterController.IsCharacterInActiveArmyCloth(characterController.Character))
            {
                characterController.Character.ClothesTypes = 0;
                ContextFactory.Instance.SaveChanges();
            }            
        }

        public void OnPlayerConnectedHandler(Client player)
        {
            var character = ContextFactory.Instance.Character.FirstOrDefault(x => x.SocialClub == player.socialClubName);
            if (character == null) return;
            API.triggerClientEventForAll("playerlist_join", player.socialClubName, player.name, ColorForPlayer(player), character.Id , character.Name);
 
            if (IsAccountBanned(player))
            {
                player.kick(Localize.Lang(character.Language, "kick"));
            }
        }
        public void OnPlayerFinishedDownloadHandler(Client player)
        {
            var players = API.getAllPlayers();
            var list = new List<string>();
            foreach (var ply in players)
            {
                var dic = new Dictionary<string, object>
                {
                    ["socialClubName"] = ply.socialClubName,
                    ["name"] = ply.name,
                    ["ping"] = ply.ping,
                    ["color"] = ColorForPlayer(ply)
                };
                list.Add(API.toJson(dic));
            }
            API.triggerClientEvent(player, "playerlist", list);

            API.setEntityData(player, "DOWNLOAD_FINISHED", true);
            LoginMenu(player);
        }
        public void OnPlayerDisconnectedHandler(Client player, string reason)
        {
            API.triggerClientEventForAll("playerlist_leave", player.socialClubName);
            var character = ContextFactory.Instance.Character.FirstOrDefault(x => x.SocialClub == player.socialClubName);
            if (character == null) return;
            // Gang change clothes after disconnect
            if (CharacterController.IsCharacterInGang(character) && 
                CharacterController.IsCharacterInActiveArmyCloth(character))
                character.ClothesTypes = 0;
                
            character.LastLogoutDate = DateTime.Now;
            character.Online = false;
            character.OID = 0;
            WeaponManager.SetPlayerWeapon(player, character, 3);
            ContextFactory.Instance.SaveChanges();
        }

        public static void LogOut(Client player, Character character, int type = 0)
        {            
            character.Online = false;
            character.OID = 0;
            ContextFactory.Instance.SaveChanges();

            if (type != 0)
            {
                LoginMenu(player);
            }
            
            Global.CEFController.Close(player);
            player.resetData("CHARACTER");
            API.shared.resetEntityData(player, "DOWNLOAD_FINISHED");
        }
        public static void LoginMenu(Client player)
        {
            API.shared.triggerClientEvent(player, "interpolateCamera", 2000, StartCamPos, StopCamPos, new Vector3(0.0, 0.0, -80.0), new Vector3(0.0, 0.0, -115.0));
            player.position = StartPos;
            player.rotation = StartRot;
            player.freeze(true);
            player.transparency = 0;
            PromptLoginScreen(player);
        }
        public static void PromptLoginScreen(Client player)
        {
            var character = ContextFactory.Instance.Character.FirstOrDefault(x => x.SocialClub == player.socialClubName);
            if (character == null) CharacterController.CreateCharacter(player);
            else
            {
                SpawnManager.SetCharacterFace(player, character);
                ClothesManager.SetPlayerSkinClothes(player, 0, character, 1);
                player.transparency = 255;
                API.shared.triggerClientEvent(player, "login_char_menu", character.Language);
            }
        }

        public static bool IsAccountBanned(Client player)
        {
            var ipBanEntity = ContextFactory.Instance.Ban.FirstOrDefault(x => x.Active && x.Ip == player.address);
            if (ipBanEntity != null) return true;
            var socialClubBanEntity = ContextFactory.Instance.Ban.FirstOrDefault(x => x.Active && x.IsSocialClubBanned && x.SocialClub == player.socialClubName);
            if (socialClubBanEntity != null) return true;
            return false;
        }

        private string ColorForPlayer(Client player)
        {
            if (!API.isResourceRunning("colorednames"))
            {
                return "FFFFFF";
            }
            string ret = player.getData("PROFILE_color");
            if (ret == null)
            {
                return "FFFFFF";
            }
            return ret;
        }
    }
}