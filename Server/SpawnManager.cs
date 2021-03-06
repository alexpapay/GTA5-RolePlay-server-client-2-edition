﻿using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared.Math;
using MpRpServer.Data;
using MpRpServer.Server.Characters;
using MpRpServer.Server.DBManager;
using MpRpServer.Server.Weapon;
using System.Linq;

namespace MpRpServer.Server
{
    public class SpawnManager : Script
    {
        private static readonly Vector3 NewPlayerPosition = new Vector3(-1034.794, -2727.422, 13.75663); //
        private static readonly Vector3 NewPlayerRotation = new Vector3(0.0, 0.0, -34.4588);
        private static readonly Vector3 FirstPlayerPosition = new Vector3(-1042.2, -2772.6, 4.639); //
        private static readonly Vector3 FirstPlayerRotation = new Vector3(0.0, 0.0, 58.7041);
        private static readonly int NewPlayerDimension = 0;

        public static void SpawnCharacter(Client player, CharacterController characterController)
        {
            API.shared.triggerClientEvent(player, "destroyCamera"); 
            API.shared.resetPlayerNametagColor(player);

            if (characterController.Character.RegistrationStep == 0)
            {
                var face = ContextFactory.Instance.Faces.FirstOrDefault(x => x.CharacterId == characterController.Character.Id);
                if (face == null)
                {
                    CharacterController.InitializePedFace(player.handle);
                    API.shared.triggerClientEvent(player, "face_custom");
                    return;
                }
                SetCharacterFace (player, characterController.Character);
                ClothesManager.SetPlayerSkinClothes(player, 0, characterController.Character, true);
                ClothesManager.WardrobeInit(characterController.Character);
                WeaponManager.SetPlayerWeapon(player, characterController.Character, 0);
                API.shared.setEntityPosition(player, FirstPlayerPosition);
                API.shared.setEntityRotation(player, FirstPlayerRotation);
                characterController.Character.RegistrationStep = -1; // 'Tutorial Done'                               
            }
            else
            {
                SetCharacterFace(player, characterController.Character);
                ClothesManager.SetPlayerSkinClothes(player, 0, characterController.Character, false);
                WeaponManager.SetPlayerWeapon(player, characterController.Character, 1);
                API.shared.setEntityPosition(player, NewPlayerPosition);
                API.shared.setEntityRotation(player, NewPlayerRotation);                
            }
            
            if (CharacterController.IsCharacterArmySoldier(characterController))
            {
                var placeArmy = characterController.Character.ActiveGroupID < 2003 ? 
                    ContextFactory.Instance.Property.FirstOrDefault(x => x.GroupId == 2000) 
                    : ContextFactory.Instance.Property.FirstOrDefault(x => x.GroupId == 2100);

                if (placeArmy != null)
                {
                    API.shared.setEntityPosition(player, new Vector3(placeArmy.ExtPosX, placeArmy.ExtPosY, placeArmy.ExtPosZ));
                    API.shared.setEntityRotation(player, NewPlayerRotation);
                }
            }
            if (CharacterController.IsCharacterInGang(characterController))
            {
                var placeGangs = new Data.Property();

                if (characterController.Character.ActiveGroupID > 1300 &&
                    characterController.Character.ActiveGroupID <= 1310)
                    placeGangs = ContextFactory.Instance.Property.FirstOrDefault(x => x.GroupId == 1300);
                if (characterController.Character.ActiveGroupID > 1400 &&
                    characterController.Character.ActiveGroupID <= 1410)
                    placeGangs = ContextFactory.Instance.Property.FirstOrDefault(x => x.GroupId == 1400);
                if (characterController.Character.ActiveGroupID > 1500 &&
                    characterController.Character.ActiveGroupID <= 1510)
                    placeGangs = ContextFactory.Instance.Property.FirstOrDefault(x => x.GroupId == 1500);
                if (characterController.Character.ActiveGroupID > 1600 &&
                    characterController.Character.ActiveGroupID <= 1610)
                    placeGangs = ContextFactory.Instance.Property.FirstOrDefault(x => x.GroupId == 1600);
                if (characterController.Character.ActiveGroupID > 1700 &&
                    characterController.Character.ActiveGroupID <= 1710)
                    placeGangs = ContextFactory.Instance.Property.FirstOrDefault(x => x.GroupId == 1700);

                if (placeGangs != null)
                {
                    API.shared.setEntityPosition(player, new Vector3(placeGangs.ExtPosX, placeGangs.ExtPosY, placeGangs.ExtPosZ));
                    API.shared.setEntityRotation(player, NewPlayerRotation);
                }
            }
            var userHouse = ContextFactory.Instance.Property.FirstOrDefault(x => x.CharacterId == characterController.Character.Id);
            if (userHouse != null)
            {
                API.shared.setEntityPosition(player, new Vector3(userHouse.IntPosX, userHouse.IntPosY, userHouse.IntPosZ));
                API.shared.setEntityRotation(player, NewPlayerRotation);
            }
            if (player.getData("ISDYED") == true && !CharacterController.IsCharacterInGang(characterController))
            {
                API.shared.setEntityPosition(player, new Vector3(260.26f, -1357.64f, 24.54f));
                player.setData("ISDYED", false);
                player.health = 20;
            }
            ContextFactory.Instance.SaveChanges();
        }

        public static void SetCharacterFace (Client player, Character character)
        {
            var face = ContextFactory.Instance.Faces.FirstOrDefault(x => x.CharacterId == character.Id);
            if (face == null) return;

            var pedHash = face.SEX == 1885233650 ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01;

            API.shared.setPlayerSkin(player, pedHash);

            CharacterController.InitializePedFace(player.handle);

            API.shared.setEntitySyncedData(player, "GTAO_SHAPE_FIRST_ID", face.GTAO_SHAPE_FIRST_ID);
            API.shared.setEntitySyncedData(player, "GTAO_SHAPE_SECOND_ID", face.GTAO_SHAPE_SECOND_ID);
            API.shared.setEntitySyncedData(player, "GTAO_SKIN_FIRST_ID", face.GTAO_SKIN_FIRST_ID);
            API.shared.setPlayerClothes(player, 2, face.GTAO_HAIR, 0);
            API.shared.setEntitySyncedData(player, "GTAO_HAIR_COLOR", face.GTAO_HAIR_COLOR);
            API.shared.setEntitySyncedData(player, "GTAO_EYE_COLOR", face.GTAO_SHAPE_FIRST_ID);
            API.shared.setEntitySyncedData(player, "GTAO_EYEBROWS", face.GTAO_EYEBROWS);
            API.shared.setEntitySyncedData(player, "GTAO_EYEBROWS_COLOR", face.GTAO_EYEBROWS_COLOR);

            CharacterController.UpdatePlayerFace(player.handle);
        }

        public static Vector3 GetSpawnPosition() { return NewPlayerPosition; }
        public static int GetSpawnDimension() { return NewPlayerDimension; }
    }
}