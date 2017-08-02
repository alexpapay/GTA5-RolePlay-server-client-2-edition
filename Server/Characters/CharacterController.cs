using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using MpRpServer.Data;
using MpRpServer.Server.DBManager;
using MpRpServer.Server.Jobs;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace MpRpServer.Server.Characters
{
    public class CharacterController
    {
        public Character Character = new Character();

        public string FormatName { get; }
        public JobController Job;
        public GroupMember ActiveGroup = new GroupMember();

        public CharacterController(Client player, Character characterData)
        {
            if (characterData != null)
            {
                Character = characterData;
                FormatName = Character.Name.Replace("_", " ");
                
                player.setData("CHARACTER", this);
                player.setData("PRISONKEYS", false);
                player.setData("MOBILEPHONE", characterData.MobilePhone);
                player.setData("MODEL", characterData.Model);

                // MESSAGE TO PLAYER:
                switch (Character.RegistrationStep)
                {
                    case 0:
                        API.shared.sendChatMessageToPlayer(player, string.Format("~w~Добро пожаловать {0} \nна ~g~{1}~w~. \nЭто Ваш первый визит. Наслаждайтесь игрой!", FormatName, Global.GlobalVars.ServerName));
                        break;
                    default:
                        API.shared.sendChatMessageToPlayer(player, string.Format("~w~Добро пожаловать {0}! \nВаше последнее подключение было: {1}", FormatName, Character.LastLoginDate));
                        break;
                }

                Character.OID = 0;

                // DYNAMIC ID
                var characters = ContextFactory.Instance.Character.Where(x => x.Online).OrderBy(x => x.OID).ToList();
                var dynamicId = 1;

                if (!characters.Any()) Character.OID = 1; // Alone on the server
                else
                {
                    if (characters.Count >= 1)
                    {
                        foreach (var character in characters)
                        {
                            if (character.OID - 1 > 0 && dynamicId != character.OID)
                            {
                                dynamicId = character.OID - 1;
                                goto OIDok;
                            }
                            if (dynamicId == character.OID)
                            {
                                dynamicId++;
                            }
                        }
                        dynamicId = characters.Max().OID + 1;
                    }
                }
                OIDok:
                Character.OID = dynamicId;
                Character.LastLoginDate = DateTime.Now;
                Character.Online = true;
                ContextFactory.Instance.SaveChangesAsync();

                API.shared.setPlayerName(player, characterData.Name.Replace("_", " ") + " (" + characterData.OID + ")");
                API.shared.setPlayerNametagColor(player, 255, 255, 255);
            }
            else
            {
                API.shared.sendChatMessageToPlayer(player, "~r~[ОШИБКА]: characterData is null");
            }
        }
        public CharacterController(Client player, string name, string pwd, int language)
        {
            Character.AccountId = pwd;
            Character.GroupType = 0;
            Character.ActiveGroupID = 1;
            Character.Admin = 0;
            Character.Bank = 0;
            Character.Cash = 300;
            Character.JobId = 0;
            Character.Level = 0;
            Character.Model = PedHash.FreemodeMale01.GetHashCode();
            Character.Name = name;
            Character.Online = false;
            Character.PosX = -1034.794f;
            Character.PosY = -2727.422f;
            Character.PosZ = 13.75663f;
            Character.RegisterDate = DateTime.Now;
            Character.DebtDate = DateTime.Now;
            Character.PlayMinutes = 0;
            Character.DriverLicense = 0;
            Character.TempVar = 0;
            Character.Material = 0;
            Character.SocialClub = player.socialClubName;
            Character.ClothesTypes = 0;
            Character.ActiveClothes = 999;
            Character.Debt = 0;
            Character.DebtMafia = 0;
            Character.DebtLimit = 0;
            Character.MafiaRoof = 0;
            Character.Narco = 0;
            Character.NarcoDep = 0;
            Character.NarcoHealDate = DateTime.Now;
            Character.NarcoHealQty = 0;
            Character.PrisonTime = 0;
            Character.IsPrisoned = false;
            Character.Language = language;

            ContextFactory.Instance.Character.Add(Character);
            ContextFactory.Instance.SaveChangesAsync();
            API.shared.setPlayerSkin(player, PedHash.FreemodeMale01);
            
            InitializePedFace(player.handle);
            API.shared.triggerClientEvent(player, "face_custom");
        }

        // LOGIN OR REGISTER METHODS:
        public static void CreateCharacter(Client player)
        {
            API.shared.triggerClientEvent(player, "create_char_menu", 0);
        }
        public static void SelectCharacter(Client player, Character character)
        {
            var characterController = new CharacterController(player, character);
            SpawnManager.SpawnCharacter(player, characterController);

            API.shared.triggerClientEvent(player, "stopAudio");
            player.freeze(false);
            player.transparency = 255;
            API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
            //API.shared.triggerClientEvent(player, "update_bank_money_display", character.Bank);
            API.shared.triggerClientEvent(player, "CEF_DESTROY");
            SetPlayerColor(player, character.GroupType);
            player.setData("ISLOGINSUCCES", true);
            CreateVoiceHash(player, character);
        }
        private static void CreateVoiceHash(Client player, Character character)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[16];
            var random = new Random();
            for (var i = 0; i < stringChars.Length; i++) stringChars[i] = chars[random.Next(chars.Length)];
            var finalString = new string(stringChars);
            API.shared.triggerClientEvent(player, "EnableVoice", "https://voice.mp-rp.ru/voice.php?s=voice&code=" + finalString);
            character.player_voice_hash = finalString;
            character.button_voice = 0;
            ContextFactory.Instance.SaveChangesAsync();
            //mysql_query("UPDATE `character` SET `player_voice_hash` = '" + finalString + "' WHERE `SocialClub` = '" + player.socialClubName + "'");
        }
        public GroupMember GetGroupInfo(int groupId)
        {
            return Character.GroupMember.FirstOrDefault(x => x.Group.Id == groupId);
        }
        public static bool NameValidityCheck(Client player, string name)
        {
            if (!name.Contains("_") || name.Count(x => x == '_') > 1)
            {
                API.shared.sendChatMessageToPlayer(player, "~r~ОШИБКА: ~w~Вы должны ввести Имя и Фамилию.\nПожалуйста разделите ваше Имя и Фамилию\nсимволом '_'.");
                return false;
            }
            if (ContextFactory.Instance.Character.FirstOrDefault(x => x.Name.ToLower() == name.ToLower()) != null)
            {
                API.shared.sendChatMessageToPlayer(player, "~r~ОШИБКА: ~w~Такое имя уже существует!");
                return false;
            }
            if (Regex.IsMatch(name, @"^[a-zA-Z_]+$")) return true;
            API.shared.sendChatMessageToPlayer(player, "~r~ОШИБКА: ~w~Вы ввели неверное имя");
            return false;
        }

        // ANIMATIONS & SCENARIO METHODS:
        public static void PlayScenario(Client player, string scenario)
        {
            player.playScenario(scenario);
            API.shared.triggerClientEvent(player, "animation_text");
        }
        public static void PlayAnimation(Client player, string animDict, string animName, int flag)
        {
            player.playAnimation(animDict, animName, flag);
            API.shared.triggerClientEvent(player, "animation_text");
        }
        public static void StopAnimation(Client player)
        {
            player.stopAnimation();
            API.shared.triggerClientEvent(player, "animation_text");
        }

        // WHICH FRACTION METHODS:
        public static bool IsCharacterInMafia(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID >= 3000 &&
                characterController.Character.ActiveGroupID <= 3010) return true;
            if (characterController.Character.ActiveGroupID >= 3100 &&
                characterController.Character.ActiveGroupID <= 3110) return true;
            if (characterController.Character.ActiveGroupID >= 3200 &&
                characterController.Character.ActiveGroupID <= 3210) return true;
            return false;
        }
        public static bool IsCharacterInMafia(Character character)
        {
            if (character.ActiveGroupID >= 3000 &&
                character.ActiveGroupID <= 3010) return true;
            if (character.ActiveGroupID >= 3100 &&
                character.ActiveGroupID <= 3110) return true;
            if (character.ActiveGroupID >= 3200 &&
                character.ActiveGroupID <= 3210) return true;
            return false;
        }
        public static bool IsCharacterInGang(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID >= 1300 &&
                characterController.Character.ActiveGroupID <= 1310) return true;
            if (characterController.Character.ActiveGroupID >= 1400 &&
                characterController.Character.ActiveGroupID <= 1410) return true;
            if (characterController.Character.ActiveGroupID >= 1500 &&
                characterController.Character.ActiveGroupID <= 1510) return true;
            if (characterController.Character.ActiveGroupID >= 1600 &&
                characterController.Character.ActiveGroupID <= 1610) return true;
            if (characterController.Character.ActiveGroupID >= 1700 &&
                characterController.Character.ActiveGroupID <= 1710) return true;
            return false;
        }
        public static bool IsCharacterInGang(Character character)
        {
            if (character.ActiveGroupID >= 1300 &&
                character.ActiveGroupID <= 1310) return true;
            if (character.ActiveGroupID >= 1400 &&
                character.ActiveGroupID <= 1410) return true;
            if (character.ActiveGroupID >= 1500 &&
                character.ActiveGroupID <= 1510) return true;
            if (character.ActiveGroupID >= 1600 &&
                character.ActiveGroupID <= 1610) return true;
            if (character.ActiveGroupID >= 1700 &&
                character.ActiveGroupID <= 1710) return true;
            return false;
        }
        public static bool IsCharacterHighRankInGang(Character character)
        {
            if (character.ActiveGroupID >= 1307 &&
                character.ActiveGroupID <= 1310) return true;
            if (character.ActiveGroupID >= 1407 &&
                character.ActiveGroupID <= 1410) return true;
            if (character.ActiveGroupID >= 1507 &&
                character.ActiveGroupID <= 1510) return true;
            if (character.ActiveGroupID >= 1607 &&
                character.ActiveGroupID <= 1610) return true;
            if (character.ActiveGroupID >= 1707 &&
                character.ActiveGroupID <= 1710) return true;
            return false;
        }
        public static bool IsCharacterGangBoss(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID == 1310) return true;
            if (characterController.Character.ActiveGroupID == 1410) return true;
            if (characterController.Character.ActiveGroupID == 1510) return true;
            if (characterController.Character.ActiveGroupID == 1610) return true;
            if (characterController.Character.ActiveGroupID == 1710) return true;
            return false;
        }
        public static bool IsCharacterGangBoss(Character character)
        {
            if (character.ActiveGroupID == 1310) return true;
            if (character.ActiveGroupID == 1410) return true;
            if (character.ActiveGroupID == 1510) return true;
            if (character.ActiveGroupID == 1610) return true;
            if (character.ActiveGroupID == 1710) return true;
            return false;
        }
        public static bool IsCharacterInActiveArmyCloth(Character character)
        {
            switch (character.ActiveClothes)
            {
                case 201: return true;
                case 202: return true;
                case 203: return true;
                case 2010: return true;
                case 2020: return true;
                case 2030: return true;
                default: return false;
            }
        }
        public static bool IsCharacterInArmy(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID >= 2000 &&
                characterController.Character.ActiveGroupID <= 2015) return true;
            if (characterController.Character.ActiveGroupID >= 2100 &&
                characterController.Character.ActiveGroupID <= 2115) return true;
            return false;
        }
        public static bool IsCharacterInArmy(Character character)
        {
            if (character.ActiveGroupID >= 2000 &&
                character.ActiveGroupID <= 2015) return true;
            if (character.ActiveGroupID >= 2100 &&
                character.ActiveGroupID <= 2115) return true;
            return false;
        }
        public static bool IsCharacterArmySoldier(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID == 2001 || characterController.Character.ActiveGroupID == 2002) return true;
            if (characterController.Character.ActiveGroupID == 2101 || characterController.Character.ActiveGroupID == 2102) return true;
            return false;
        }
        public static bool IsCharacterArmyHighOfficer(Character character)
        {
            if (character.ActiveGroupID >= 2012 && character.ActiveGroupID <= 2014) return true;
            if (character.ActiveGroupID >= 2112 && character.ActiveGroupID <= 2114) return true;
            return false;
        }
        public static bool IsCharacterArmyInAllOfficers(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID >= 2003 && characterController.Character.ActiveGroupID <= 2014) return true;
            if (characterController.Character.ActiveGroupID >= 2103 && characterController.Character.ActiveGroupID <= 2114) return true;
            return false;
        }
        public static bool IsCharacterArmyInAllOfficers(Character character)
        {
            if (character.ActiveGroupID >= 2003 && character.ActiveGroupID <= 2014) return true;
            if (character.ActiveGroupID >= 2103 && character.ActiveGroupID <= 2114) return true;
            return false;
        }
        public static bool IsCharacterArmyGeneral(CharacterController characterController)
        {
            switch (characterController.Character.ActiveGroupID)
            {
                case 2015:return true;
                case 2115:return true;
                default: return false;
            }
        }
        public static bool IsCharacterArmyGeneral(Character character)
        {
            if (character.ActiveGroupID == 2015) return true;
            if (character.ActiveGroupID == 2115) return true;
            return false;
        }
        public static bool IsCharacterInPolice(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID >= 100 &&
                characterController.Character.ActiveGroupID <= 114) return true;
            return false;
        }
        public static bool IsCharacterInPolice(Character character)
        {
            if (character.ActiveGroupID >= 100 &&
                character.ActiveGroupID <= 114) return true;
            return false;
        }
        public static bool IsCharacterInMeria(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID >= 1100 &&
                characterController.Character.ActiveGroupID <= 1106) return true;
            return false;
        }
        public static bool IsCharacterInMeria(Character character)
        {
            if (character.ActiveGroupID >= 1100 &&
                character.ActiveGroupID <= 1106) return true;
            return false;
        }
        public static bool IsCharacterInEmergency(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID >= 200 &&
                characterController.Character.ActiveGroupID <= 210) return true;
            return false;
        }
        public static bool IsCharacterInEmergency(Character character)
        {
            if (character.ActiveGroupID >= 200 &&
                character.ActiveGroupID <= 210) return true;
            return false;
        }
        public static bool IsCharacterInFbi(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID >= 300 &&
                characterController.Character.ActiveGroupID <= 310) return true;
            return false;
        }
        public static bool IsCharacterInFbi(Character character)
        {
            if (character.ActiveGroupID >= 300 &&
                character.ActiveGroupID <= 310) return true;
            return false;
        }
        public static bool IsCharacterInNews(CharacterController characterController)
        {
            if (characterController.Character.ActiveGroupID >= 700 &&
                characterController.Character.ActiveGroupID <= 710) return true;
            return false;
        }
        public static bool IsCharacterInNews(Character character)
        {
            if (character.ActiveGroupID >= 700 &&
                character.ActiveGroupID <= 710) return true;
            return false;
        }

        // GANG SECTORS:
        public static bool IsCharacterInGhetto(Client player)
        {
            var x1 = -468.0; var y1 = -2262.0;
            var x3 = 1222.0; var y3 = -572.0;
            
            if (x1 < player.position.X && player.position.X < x3)
                if (y1 < player.position.Y && player.position.Y < y3)
                    return true;
            return false;
        }
        public static bool IsPlayerInYourGangSector(Client player)
        {
            var currentSector = InWhichSectorOfGhetto(player).Split(';');
            var currentSectorData = ContextFactory.Instance.GangSectors.First(
                x => x.IdRow == Convert.ToInt32(currentSector[0]));

            var sectorData = 0;
            switch(currentSector[1])
            {
                case "1": sectorData = currentSectorData.Col1; break;
                case "2": sectorData = currentSectorData.Col2; break;
                case "3": sectorData = currentSectorData.Col3; break;
                case "4": sectorData = currentSectorData.Col4; break;
                case "5": sectorData = currentSectorData.Col5; break;
                case "6": sectorData = currentSectorData.Col6; break;
                case "7": sectorData = currentSectorData.Col7; break;
                case "8": sectorData = currentSectorData.Col8; break;
                case "9": sectorData = currentSectorData.Col9; break;
                case "10": sectorData = currentSectorData.Col10; break;
                case "11": sectorData = currentSectorData.Col11; break;
                case "12": sectorData = currentSectorData.Col12; break;
                case "13": sectorData = currentSectorData.Col13; break;
            }

            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return false;
            var character = characterController.Character;

            if (sectorData == character.GroupType || 
                sectorData == character.GroupType * 10) return true;

            return false;
        }
        public static string InWhichSectorOfGhetto(Client player)
        {
            // START points:
            var y1 = -2262.0;
            var y3 = -2132.0;

            for (var i = 1; i < 14; i++)
            {
                var x1 = -468.0;
                var x3 = -338.0;
                for (var j = 1; j < 14; j++)
                {
                    if (x1 < player.position.X && player.position.X < x3)
                        if (y1 < player.position.Y && player.position.Y < y3)
                            return i + ";" + j;
                    x1 += 130;
                    x3 += 130;
                }
                y1 += 130;
                y3 += 130;
            }
            return "0;0";
        }

        // COLOR METHODS:
        public static void SetPlayerColor(Client player, int groupType)
        {
            switch (groupType)
            {
                case 0: API.shared.setPlayerNametagColor(player, 255, 255, 255); break;
                case 1: API.shared.setPlayerNametagColor(player, 0, 0, 250); break;
                case 2: API.shared.setPlayerNametagColor(player, 255, 0, 0); break;
                case 3: API.shared.setPlayerNametagColor(player, 0, 0, 0); break;
                case 11: API.shared.setPlayerNametagColor(player, 255, 180, 60); break;
                case 12: API.shared.setPlayerNametagColor(player, 170, 255, 60); break;
                case 13: API.shared.setPlayerNametagColor(player, 100, 0, 100); break;
                case 14: API.shared.setPlayerNametagColor(player, 9, 15, 70); break;
                case 15: API.shared.setPlayerNametagColor(player, 100, 100, 0); break;
                case 16: API.shared.setPlayerNametagColor(player, 0, 100, 0); break;
                case 17: API.shared.setPlayerNametagColor(player, 0, 100, 100); break;
                case 20: API.shared.setPlayerNametagColor(player, 0, 255, 50); break;
                case 21: API.shared.setPlayerNametagColor(player, 0, 255, 0); break;
                case 30: API.shared.setPlayerNametagColor(player, 255, 200, 0); break;
                case 31: API.shared.setPlayerNametagColor(player, 255, 200, 0); break;
                case 32: API.shared.setPlayerNametagColor(player, 255, 200, 0); break;
                default: API.shared.setPlayerNametagColor(player, 255, 255, 255); break;
            }
            
        }

        // FACE METHODS:
        public static void InitializePedFace(NetHandle ent)
        {
            API.shared.setEntitySyncedData(ent, "GTAO_HAS_CHARACTER_DATA", true);

            API.shared.setEntitySyncedData(ent, "GTAO_SHAPE_FIRST_ID", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_SHAPE_SECOND_ID", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_SKIN_FIRST_ID", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_SKIN_SECOND_ID", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_SHAPE_MIX", 0f);
            API.shared.setEntitySyncedData(ent, "GTAO_SKIN_MIX", 0f);
            API.shared.setEntitySyncedData(ent, "GTAO_HAIR", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_HAIR_COLOR", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_HAIR_HIGHLIGHT_COLOR", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_EYE_COLOR", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_EYEBROWS", 0);

            //API.setEntitySyncedData(ent, "GTAO_MAKEUP", 0); // No lipstick by default. 
            //API.setEntitySyncedData(ent, "GTAO_LIPSTICK", 0); // No makeup by default.

            API.shared.setEntitySyncedData(ent, "GTAO_EYEBROWS_COLOR", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_MAKEUP_COLOR", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_LIPSTICK_COLOR", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_EYEBROWS_COLOR2", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_MAKEUP_COLOR2", 0);
            API.shared.setEntitySyncedData(ent, "GTAO_LIPSTICK_COLOR2", 0);

            var list = new float[21];

            for (var i = 0; i < 21; i++)
            {
                list[i] = 0f;
            }

            API.shared.setEntitySyncedData(ent, "GTAO_FACE_FEATURES_LIST", list);
        }
        private void RemovePedFace(NetHandle ent)
        {
            API.shared.setEntitySyncedData(ent, "GTAO_HAS_CHARACTER_DATA", false);

            API.shared.resetEntitySyncedData(ent, "GTAO_SHAPE_FIRST_ID");
            API.shared.resetEntitySyncedData(ent, "GTAO_SHAPE_SECOND_ID");
            API.shared.resetEntitySyncedData(ent, "GTAO_SKIN_FIRST_ID");
            API.shared.resetEntitySyncedData(ent, "GTAO_SKIN_SECOND_ID");
            API.shared.resetEntitySyncedData(ent, "GTAO_SHAPE_MIX");
            API.shared.resetEntitySyncedData(ent, "GTAO_SKIN_MIX");
            API.shared.resetEntitySyncedData(ent, "GTAO_HAIR");
            API.shared.resetEntitySyncedData(ent, "GTAO_HAIR_COLOR");
            API.shared.resetEntitySyncedData(ent, "GTAO_HAIR_HIGHLIGHT_COLOR");
            API.shared.resetEntitySyncedData(ent, "GTAO_EYE_COLOR");
            API.shared.resetEntitySyncedData(ent, "GTAO_EYEBROWS");
            API.shared.resetEntitySyncedData(ent, "GTAO_MAKEUP");
            API.shared.resetEntitySyncedData(ent, "GTAO_LIPSTICK");
            API.shared.resetEntitySyncedData(ent, "GTAO_EYEBROWS_COLOR");
            API.shared.resetEntitySyncedData(ent, "GTAO_MAKEUP_COLOR");
            API.shared.resetEntitySyncedData(ent, "GTAO_LIPSTICK_COLOR");
            API.shared.resetEntitySyncedData(ent, "GTAO_EYEBROWS_COLOR2");
            API.shared.resetEntitySyncedData(ent, "GTAO_MAKEUP_COLOR2");
            API.shared.resetEntitySyncedData(ent, "GTAO_LIPSTICK_COLOR2");
            API.shared.resetEntitySyncedData(ent, "GTAO_FACE_FEATURES_LIST");
        }
        private bool IsPlayerFaceValid(NetHandle ent)
        {
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_SHAPE_FIRST_ID")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_SHAPE_SECOND_ID")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_SKIN_FIRST_ID")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_SKIN_SECOND_ID")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_SHAPE_MIX")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_SKIN_MIX")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_HAIR")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_HAIR_COLOR")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_HAIR_HIGHLIGHT_COLOR")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_EYE_COLOR")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_EYEBROWS")) return false;
            //if (!API.hasEntitySyncedData(ent, "GTAO_MAKEUP")) return false; // Player may have no makeup
            //if (!API.hasEntitySyncedData(ent, "GTAO_LIPSTICK")) return false; // Player may have no lipstick
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_EYEBROWS_COLOR")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_MAKEUP_COLOR")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_LIPSTICK_COLOR")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_EYEBROWS_COLOR2")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_MAKEUP_COLOR2")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_LIPSTICK_COLOR2")) return false;
            if (!API.shared.hasEntitySyncedData(ent, "GTAO_FACE_FEATURES_LIST")) return false;

            return true;
        }
        public static void UpdatePlayerFace(NetHandle player)
        {
            API.shared.triggerClientEventForAll("UPDATE_CHARACTER", player);
        }
    }
}