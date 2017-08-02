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
            API.onPlayerRespawn += OnPlayerRespawnHandler;
        }

        private void OnPlayerRespawnHandler(Client player)
        {
            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            SpawnManager.SpawnCharacter(player, characterController);
        }

        private void OnPlayerDeath(Client player, NetHandle entityKiller, int weapon)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var diedCharacter = characterController?.Character;
            if (diedCharacter == null) return;
            var killer = API.getPlayerFromHandle(entityKiller);
            var isMale = diedCharacter.Model == 1885233650;

            if (killer != null)
            {
                API.sendNotificationToAll(killer.name + Localize.Lang(2, "killed") + player.name);
                var killerCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.SocialClub == killer.socialClubName);
                if (killerCharacter == null) return;

                // CAPTION GANGS:
                var caption = ContextFactory.Instance.Caption.First(x => x.Id == 1);
                if (caption.Sector != "0;0")
                {
                    if (CharacterController.InWhichSectorOfGhetto(player) == caption.Sector &&
                        CharacterController.InWhichSectorOfGhetto(killer) == caption.Sector)
                    {
                        var getAttackGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == caption.GangAttack * 100);
                        if (getAttackGroup == null) return;
                        var groupAttackType = (GroupType)Enum.Parse(typeof(GroupType), getAttackGroup.Type.ToString());
                        var getDefendGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == caption.GangDefend * 100);
                        if (getDefendGroup == null) return;
                        var groupDefendType = (GroupType)Enum.Parse(typeof(GroupType), getDefendGroup.Type.ToString());

                        if (killerCharacter.GroupType == caption.GangAttack && diedCharacter.GroupType == caption.GangDefend) caption.FragsAttack += 1;
                        if (killerCharacter.GroupType == caption.GangDefend && diedCharacter.GroupType == caption.GangAttack) caption.FragsDefend += 1;

                        var fragsMessage = "~s~Фрагов у банды ~r~" + EntityManager.GetDisplayName(groupAttackType) + " ~s~: ~r~" + caption.FragsAttack +
                                           "\n~s~Фрагов у банды ~g~" + EntityManager.GetDisplayName(groupDefendType) + " ~s~: ~g~" + caption.FragsDefend;
                        ChatController.SendMessageInGroup(caption.GangAttack, fragsMessage);
                        ChatController.SendMessageInGroup(caption.GangDefend, fragsMessage);
                        ContextFactory.Instance.SaveChanges();
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(killer, "Вы вне сектора капта. ~r~Фраг незасчитан!");
                    }
                }

                // Army change cloth after death algorithm
                if (CharacterController.IsCharacterInArmy(diedCharacter) &&
                    CharacterController.IsCharacterInGhetto(killer))
                {
                    switch (diedCharacter.ActiveClothes)
                    {
                        case 201:
                            if (killerCharacter.ClothesTypes == 201)
                            {
                                API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_soldier_have"));
                                break;
                            }
                            killerCharacter.ClothesTypes = 201;
                            API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_soldier")); break;
                        case 2010:
                            if (killerCharacter.ClothesTypes == 2010)
                            {
                                API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_soldier_have"));
                                break;
                            }
                            killerCharacter.ClothesTypes = 2010;
                            API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_soldier")); break;
                        case 202:
                            if (killerCharacter.ClothesTypes == 202)
                            {
                                API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_officer_have"));
                                break;
                            }
                            killerCharacter.ClothesTypes = 202;
                            API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_officer")); break;
                        case 2020:
                            if (killerCharacter.ClothesTypes == 2020)
                            {
                                API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_officer_have"));
                                break;
                            }
                            killerCharacter.ClothesTypes = 2020;
                            API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_officer")); break;
                        case 203:
                            if (killerCharacter.ClothesTypes == 203)
                            {
                                API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_general_have"));
                                break;
                            }
                            killerCharacter.ClothesTypes = 203;
                            API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_general")); break;
                        case 2030:
                            if (killerCharacter.ClothesTypes == 2030)
                            {
                                API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_general_have"));
                                break;
                            }
                            killerCharacter.ClothesTypes = 2030;
                            API.sendNotificationToPlayer(killer, Localize.Lang(killerCharacter.Language, "kill_cloth_general")); break;
                    }
                    ClothesManager.SetPlayerSkinClothesToDb(player, isMale ? 999 : 9990, diedCharacter, false);
                }

                // TO PRISON IF STARS:
                if (CharacterController.IsCharacterInPolice(killerCharacter) || 
                    CharacterController.IsCharacterInFbi(killerCharacter))
                {
                    if (API.getPlayerWantedLevel(player) > 0)
                    {
                        diedCharacter.PrisonTime = 10;
                        diedCharacter.IsPrisoned = true;
                        API.setEntityPosition(player, new Vector3(458.81, -1001.43, 24.91));
                        API.shared.freezePlayer(player, true);
                        API.shared.setPlayerWantedLevel(player, 0);
                        API.sendNotificationToPlayer(player, "~r~Вас посадили в тюрьму!");
                    }
                }

                // STARS FOR KILL

            }
            else
            {
                API.sendNotificationToAll(player.name + Localize.Lang(2, "death"));
            }
            WeaponManager.SetPlayerWeapon(player, characterController.Character, 2);

            // Gang change clothes after death

            if (CharacterController.IsCharacterInGang(diedCharacter) &&
                CharacterController.IsCharacterInActiveArmyCloth(diedCharacter))
                diedCharacter.ClothesTypes = 0;
            if (CharacterController.IsCharacterArmyHighOfficer(diedCharacter) ||
                CharacterController.IsCharacterArmyGeneral(diedCharacter))
                ClothesManager.SetPlayerSkinClothesToDb(player, isMale ? 999 : 9990, diedCharacter, false);
            ContextFactory.Instance.SaveChanges();

            if (!CharacterController.IsCharacterInGang(diedCharacter))
            {
                player.setData("ISDYED", true);
                player.setData("NEEDHEAL", true);
            }
        }

        public void OnPlayerConnectedHandler(Client player)
        {
            var character = ContextFactory.Instance.Character.FirstOrDefault(x => x.SocialClub == player.socialClubName);
            if (character == null) return;
            API.triggerClientEventForAll("playerlist_join", player.socialClubName, player.name, ColorForPlayer(player), character.Id, character.Name);

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
            player.setData("BUTTON_VOICE", 0);
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

            if (type != 0) LoginMenu(player);

            WeaponManager.SetPlayerWeapon(player, character, 3);
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

            var character = ContextFactory.Instance.Character.FirstOrDefault(x => x.SocialClub == player.socialClubName);
            if (character == null) CharacterController.CreateCharacter(player);
            else
            {
                var face = ContextFactory.Instance.Faces.FirstOrDefault(x => x.CharacterId == character.Id);
                if (face != null)
                {
                    SpawnManager.SetCharacterFace(player, character);
                    ClothesManager.SetPlayerSkinClothes(player, 0, character, false);
                    player.transparency = 255;
                    API.shared.triggerClientEvent(player, "login_char_menu", character.Language);
                }
                else
                {
                    CharacterController.InitializePedFace(player.handle);
                    API.shared.triggerClientEvent(player, "face_custom");
                }
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