using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using MpRpServer.Data;
using MpRpServer.Data.Enums;
using MpRpServer.Data.Models;
using MpRpServer.Server.Characters;
using MpRpServer.Server.DBManager;
using MpRpServer.Server.Groups;
using MpRpServer.Server.Jobs;
using MpRpServer.Server.Vehicles;
using MpRpServer.Server.Weapon;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MpRpServer.Server.Menu
{
    public class MenuManager : Script
    {
        [Flags]
        public enum AnimationFlags
        {
            Loop = 1 << 0,
            StopOnLastFrame = 1 << 1,
            OnlyAnimateUpperBody = 1 << 4,
            AllowPlayerControl = 1 << 5,
            Cancellable = 1 << 7
        }

        public MenuManager()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
            API.onClientEventTrigger += OnCreateEventTrigger;
        }

        // Login & Registration
        private void OnCreateEventTrigger(Client player, string eventName, object[] args)
        {  
            if (eventName == "create_char")
            {
                if (CharacterController.NameValidityCheck(player, args[0].ToString()))
                {
                    // Password correct
                    MD5 md5 = new MD5CryptoServiceProvider();
                    var pass = args[1].ToString();
                    var checkSum = md5.ComputeHash(Encoding.UTF8.GetBytes(pass));
                    var result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                    var newCharacterController = new CharacterController(player, args[0].ToString(), result, (int)args[2]);
                }
                else
                {
                    // Password is wrong
                    API.shared.triggerClientEvent(player, "create_char_menu", 0);
                    return;
                }
            }
            if (eventName == "login_char")
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                var pass = args[0].ToString();
                var checkSum = md5.ComputeHash(Encoding.UTF8.GetBytes(pass));
                var result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                var character = ContextFactory.Instance.Character.FirstOrDefault(x => x.SocialClub == player.socialClubName);
                if (character == null) return;

                if (character.AccountId == result)
                {                    
                    CharacterController.SelectCharacter(player, character);
                    API.shared.triggerClientEvent(player, "reset_menu");
                }                    
                else
                {
                    API.shared.sendChatMessageToPlayer(player, "~r~Вы ввели неверный пароль!");
                    API.shared.triggerClientEvent(player, "login_char_menu", character.Language);
                    return;
                }
                ContextFactory.Instance.SaveChanges();
            }
            if (eventName == "change_face")
            {
                var value = (int)args[1];                

                switch (args[0])
                {
                    case "GTAO_SHAPE_FIRST_ID":
                        API.setEntitySyncedData(player, "GTAO_SHAPE_FIRST_ID", value);
                        CharacterController.UpdatePlayerFace(player.handle);
                        break;
                    case "GTAO_SHAPE_SECOND_ID":
                        API.setEntitySyncedData(player, "GTAO_SHAPE_SECOND_ID", value);
                        CharacterController.UpdatePlayerFace(player.handle);
                        break;
                    case "GTAO_SKIN_FIRST_ID":
                        API.setEntitySyncedData(player, "GTAO_SKIN_FIRST_ID", value);
                        CharacterController.UpdatePlayerFace(player.handle);
                        break;
                    case "GTAO_HAIR":
                        API.setPlayerClothes(player, 2, value, 0); break;
                    case "GTAO_HAIR_COLOR":
                        API.setEntitySyncedData(player, "GTAO_HAIR_COLOR", value);
                        CharacterController.UpdatePlayerFace(player.handle);
                        break;
                    case "GTAO_EYE_COLOR":
                        API.setEntitySyncedData(player, "GTAO_EYE_COLOR", value);
                        CharacterController.UpdatePlayerFace(player.handle); 
                        break;
                    case "GTAO_EYEBROWS":
                        API.setEntitySyncedData(player, "GTAO_EYEBROWS", value);
                        CharacterController.UpdatePlayerFace(player.handle);
                        break;
                    case "GTAO_EYEBROWS_COLOR":
                        API.setEntitySyncedData(player, "GTAO_EYEBROWS_COLOR", value);
                        CharacterController.UpdatePlayerFace(player.handle);
                        break;                    
                }
            }
            if (eventName == "custom_char")
            {
                var faceJson = JsonConvert.DeserializeObject<Faces>((string)args[0]);
                var character = ContextFactory.Instance.Character.First(x => x.SocialClub == player.socialClubName);

                var characterFace = new Faces
                {
                    CharacterId = character.Id,
                    SEX = faceJson.SEX,
                    GTAO_SHAPE_FIRST_ID = faceJson.GTAO_SHAPE_FIRST_ID,
                    GTAO_SHAPE_SECOND_ID = faceJson.GTAO_SHAPE_SECOND_ID,
                    GTAO_SKIN_FIRST_ID = faceJson.GTAO_SKIN_FIRST_ID,
                    GTAO_HAIR = faceJson.GTAO_HAIR,
                    GTAO_HAIR_COLOR = faceJson.GTAO_HAIR_COLOR,
                    GTAO_EYE_COLOR = faceJson.GTAO_EYE_COLOR,
                    GTAO_EYEBROWS = faceJson.GTAO_EYEBROWS,
                    GTAO_EYEBROWS_COLOR = faceJson.GTAO_EYEBROWS_COLOR
                };

                character.Model = faceJson.SEX;

                ContextFactory.Instance.Faces.Add(characterFace);
                ContextFactory.Instance.SaveChanges();

                CharacterController.SelectCharacter(player, character);
                API.shared.triggerClientEvent(player, "reset_menu");
            }
        }

        private void OnClientEventTrigger(Client player, string eventName, object[] args)
        {
            var vehicleController = EntityManager.GetVehicle(player.vehicle);
            
            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            var character = characterController.Character;            
            var formatName = character.Name.Replace("_", " ");

            if (eventName == "playerlist_pings")
            {
                try
                {
                    var players = API.getAllPlayers();
                    var characters = ContextFactory.Instance.Character.Where(x => x.Online).ToList();
                    var list = new List<string>();
                    foreach (var ply in players)
                    {
                        var characterToList = characters.FirstOrDefault(x => x.SocialClub == ply.socialClubName);
                        var dic = new Dictionary<string, object>
                        {
                            ["userName"] = characterToList.Name,
                            ["userID"] = characterToList.OID,
                            ["socialClubName"] = ply.socialClubName,
                            ["ping"] = ply.ping
                        };
                        list.Add(API.toJson(dic));
                    }
                    API.triggerClientEvent(player, "playerlist_pings", list);
                }
                catch (Exception) { }
            }
            // CLOTHES:
            if (eventName == "change_clothes")
            {
                var slot = (int)args[0];
                var drawable = (int)args[1];
                var texture = (int)args[2];
                API.setPlayerClothes(player, slot, drawable, texture);
            }
            if (eventName == "buy_clothes")
            {
                var slot = (int) args[0];
                var itemSelect = (int) args[1];
                var drawSelect = (int) args[2];
                var cost = (int) args[3];
                var jobId = (int)args[4];
                var currentJob = ContextFactory.Instance.Job.First(x => x.Id == jobId);

                if (character.Cash - cost < 0)
                {
                    API.sendNotificationToPlayer(player, "~r~У вас недостаточно средств для покупки!");
                    return;
                }
                character.Cash -= cost;
                currentJob.Money += cost;
                ContextFactory.Instance.SaveChanges();
                API.triggerClientEvent(player, "update_money_display", character.Cash);

                if (slot == 51) drawSelect = 0;
                ClothesManager.SetPlayerClothType(character, slot, itemSelect);
                ClothesManager.SetPlayerClothDraw(character, slot, drawSelect);
                JobController.AddBuyedClothes(player,character, slot, itemSelect, drawSelect);
            }
            if (eventName == "reset_clothes")
            {
                ClothesManager.SetPlayerSkinClothes(player, 0, character, false);
            }
            if (eventName == "change_accessory")
            {
                var slot = (int)args[0];
                var drawable = (int)args[1];
                var texture = (int)args[2];
                API.setPlayerAccessory(player, slot, drawable, texture);
            }

            // HOUSE MENU:
            if (eventName == "house_menu_buysell")
            {
                var propertyId = (int)args[0];
                var cost = (int)args[1];
                var trigger = (int)args[2];

                var buyedProperty = ContextFactory.Instance.Property.First(x => x.PropertyID == propertyId);
                if (buyedProperty == null) return;

                // Buy house
                if (trigger == 1)
                {
                    if (character.Cash - cost < 0)
                    {
                        API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~У вас недостаточно средств для покупки!");
                        return;
                    }
                    var characterHouses = ContextFactory.Instance.Property.FirstOrDefault(x => x.CharacterId == character.Id);
                    if (characterHouses != null)
                    {
                        API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~У вас уже есть купленное жилье!");
                        return;
                    }

                    character.Cash -= cost;
                    buyedProperty.CharacterId = character.Id;

                    var blips = API.getAllBlips();
                    foreach (var blip in blips)
                    {
                        var blipPos = API.getBlipPosition(blip);
                        if (blipPos.X == buyedProperty.ExtPosX &&
                            blipPos.Y == buyedProperty.ExtPosY &&
                            blipPos.Z == buyedProperty.ExtPosZ)
                            API.setBlipColor(blip, 1);
                    }
                    var labels = API.getAllLabels();
                    foreach (var label in labels)
                    {
                        var labelText = API.getTextLabelText(label);
                        if (labelText.Contains("~g~Купить дом №" + buyedProperty.PropertyID))
                            API.setTextLabelText(label, "~g~Вход в дом №" + buyedProperty.PropertyID + ".\nВладелец: " + character.Name);
                    }
                    API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
                    ContextFactory.Instance.SaveChanges();
                }
                if (trigger == 0)
                {
                    if (character.Id != buyedProperty.CharacterId)
                    {
                        API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~Вы не можете продать данный дом!");
                        return;
                    }
                    character.Cash += cost/2;
                    buyedProperty.CharacterId = null;

                    var list = API.getAllBlips();
                    foreach (var blip in list)
                    {
                        var blipPos = API.getBlipPosition(blip);
                        if (blipPos.X == buyedProperty.ExtPosX &&
                            blipPos.Y == buyedProperty.ExtPosY &&
                            blipPos.Z == buyedProperty.ExtPosZ)
                            API.setBlipColor(blip, 2);
                    }
                    var labels = API.getAllLabels();
                    foreach (var label in labels)
                    {
                        var labelText = API.getTextLabelText(label);
                        if (labelText.Contains("~g~Вход в дом №" + buyedProperty.PropertyID))
                            API.setTextLabelText(label, "~g~Купить дом №" + buyedProperty.PropertyID +".\nСтоимость: " + cost + "$");
                    }
                    API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
                    ContextFactory.Instance.SaveChanges();
                }
            }

            // CAR MENU
            if (eventName == "driver_door")
            {
                var curVehicleController = player.isInVehicle ? EntityManager.GetVehicle(player.vehicle) 
                    : EntityManager.GetVehicleControllers().Find(x => x.Vehicle.position.DistanceTo(player.position) < 2.5f);

                if (curVehicleController == null)
                {
                    API.sendNotificationToPlayer(player, "Вы находитесь далеко от транспорта.");
                    return;
                }

                if (curVehicleController.CheckAccess(characterController))
                {
                    if ((int)args[0] == 1)
                    {
                        API.setVehicleLocked(curVehicleController.Vehicle, false);
                        ChatController.SendProxMessage(player, 15.0f, "~#C2A2DA~", formatName + " открыл водительскую дверь.");
                    }
                    else
                    {
                        API.setVehicleLocked(curVehicleController.Vehicle, true);
                        ChatController.SendProxMessage(player, 15.0f, "~#C2A2DA~", formatName + " закрыл водительскую дверь.");
                    }
                }
                else API.sendNotificationToPlayer(player, "~r~ERROR: ~w~Вы не можете открыть данный транспорт!");
            }
            if (eventName == "engine_on")
            {
                try
                {
                    if (character.DriverLicense == 0 && vehicleController.VehicleData.Model != RentModels.ScooterModel)
                    {
                        API.sendNotificationToPlayer(player, "У вас нет прав на управление данным транспортом.");
                        return;
                    }
                    if (!vehicleController.CheckAccess(characterController) ||
                        vehicleController.VehicleData.RentTime == 0)
                    {
                        API.sendNotificationToPlayer(player, "Вы не можете использовать данный транспорт.");
                        return;
                    }
                    if (vehicleController.VehicleData.Fuel > 0)
                    {
                        vehicleController.Vehicle.engineStatus = true;
                        ChatController.SendProxMessage(player, 15.0f, "~#C2A2DA~",
                            formatName + " вставил ключ в зажигание и запустил мотор.");
                    }
                    else API.sendNotificationToPlayer(player, "~r~В данном транспорте закончился бензин!");
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Машина не завелась. Попробуйте еще раз!");
                }
                
            }
            if (eventName == "engine_off")
            {
                try
                {
                    if (!vehicleController.CheckAccess(characterController))
                    {
                        API.sendNotificationToPlayer(player, "Вы не можете использовать данный транспорт.");
                        return;
                    }
                    vehicleController.Vehicle.engineStatus = false;
                    ChatController.SendProxMessage(player, 15.0f, "~#C2A2DA~",
                        formatName + " повернул ключ зажигания и заглушил мотор.");
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Мотор не заглох. Попробуйте еще раз!");
                }
            }
            if (eventName == "park_vehicle")
            {
                if (vehicleController.CheckAccess(characterController))
                {
                    vehicleController.ParkVehicle(player);
                }
                else API.sendNotificationToPlayer(player, "~r~ОШИБКА: ~w~Вы не можете парковать данный транспорт");
            }
            if (eventName == "hood_trunk")
            {
                var curVehicleController = player.isInVehicle ? EntityManager.GetVehicle(player.vehicle) 
                    : EntityManager.GetVehicleControllers().Find(x => x.Vehicle.position.DistanceTo(player.position) < 2.0f);

                if (curVehicleController == null)
                {
                    API.sendNotificationToPlayer(player, "Вы находитесь далеко от транспорта.");
                    return;
                }

                if (curVehicleController.CheckAccess(characterController))
                {
                    if ((int)args[0] == 1)
                    {
                        VehicleController.TriggerDoor(curVehicleController.Vehicle, 4);
                        ChatController.SendProxMessage(player, 15.0f, "~#C2A2DA~", formatName + " открыл/закрыл капот.");
                    }
                    else
                    {
                        VehicleController.TriggerDoor(curVehicleController.Vehicle, 5);
                        ChatController.SendProxMessage(player, 15.0f, "~#C2A2DA~", formatName + " открыл/закрыл багажник.");
                    }
                }
                else API.sendNotificationToPlayer(player, "~r~ERROR: ~w~Вы не можете открыть капот или багажник данного транспорта.");
            }

            // RENT MENU
            if (eventName == "rent_scooter")
            {
                // Delete 30$ from cash
                if (character == null) return;
                if (character.Cash - Prices.ScooterRentPrice < 0)
                {
                    API.shared.sendNotificationToPlayer(player, "У вас недостаточно средств для аренды!");
                    return;
                }

                character.Cash -= Prices.ScooterRentPrice;
                API.shared.triggerClientEvent(player, "update_money_display", character.Cash);

                var vehicleData = new Data.Vehicle
                {
                    Character = character,
                    Type = 1,
                    Model = RentModels.ScooterModel,
                    PosX = player.position.X + 3.0f,
                    PosY = player.position.Y + 3.0f,
                    PosZ = player.position.Z,
                    Rot = player.rotation.Z,
                    Color1 = 0,
                    Color2 = 0,
                    RentTime = Time.ScooterRentTime,
                    Fuel = 10,
                    Respawnable = true,
                    GroupId = character.ActiveGroupID
                };

                var VehicleController = new VehicleController(vehicleData, API.createVehicle(VehicleHash.Faggio, player.position, player.rotation, 0, 0));
                
                ContextFactory.Instance.Vehicle.Add(vehicleData);
                ContextFactory.Instance.SaveChanges();
            }
            if (eventName == "rent_prolong")
            {
                if (character == null) return;
                var callback = (int)args[0];
                var vehicleModel = (int)args[1];

                var vehicle = ContextFactory.Instance.Vehicle
                        .Where(x => x.Model == vehicleModel)
                        .FirstOrDefault(x => x.CharacterId == character.Id);

                if (callback == 1)
                {
                    // Checking money for prolongate
                    if (vehicleModel ==RentModels.ScooterModel)
                    {
                        if (character.Cash - Prices.ScooterRentPrice < 0)
                        {
                            API.shared.sendNotificationToPlayer(player, "У вас недостаточно средств для продления аренды!");                            
                        }
                    }   // Scooter
                    if (vehicleModel == RentModels.TaxiModel)
                    {
                        if (character.Cash - Prices.TaxiRentPrice < 0)
                        {
                                API.shared.sendNotificationToPlayer(player, "У вас недостаточно средств для продления аренды!");                      
                        }
                    }    // Taxi 
                    else
                    {
                        // Adding additional time for using and take money
                        if (vehicleModel == RentModels.ScooterModel)
                        {
                            vehicle.RentTime = Time.ScooterRentTime;
                            character.Cash -= Prices.ScooterRentPrice;
                        }   // Scooter
                        if (vehicleModel == RentModels.TaxiModel)
                        {
                            vehicle.RentTime = Time.TaxiRentTime;
                            character.Cash -= Prices.TaxiRentPrice;
                        }    // Taxi 
                        ContextFactory.Instance.SaveChanges();
                    }                    
                }
                if (callback == 0)
                {
                    if (vehicleModel == RentModels.ScooterModel)
                    {
                        var curVehicleController = EntityManager.GetVehicle(vehicle);
                        curVehicleController.UnloadVehicle(character);
                        ContextFactory.Instance.Vehicle.Remove(vehicle);
                    }  // Scooter deleting
                    if (vehicleModel == RentModels.TaxiModel) VehicleController.RespawnWorkVehicle(vehicle, vehicleModel, 126, 126);

                    ContextFactory.Instance.SaveChanges();
                }
            }

            // WORK MENU    (NO TRY):
            if (eventName == "work_unemployers")
            {
                character.JobId = JobsIdNonDataBase.Unemployer;
                ContextFactory.Instance.SaveChanges();
            }
            if (eventName == "work_mechanic")
            {
                if (character.Level < 2)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не можете устроиться. Нужен 2 уровень!");
                    return;
                }

                var trigger = (int) args[0];
                switch (trigger)
                {
                    case 1:
                        if (character.JobId == 333)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы уже работаете автомехаником!");
                            break;
                        }
                        character.JobId = JobsIdNonDataBase.Mechanic;
                        ContextFactory.Instance.SaveChanges();
                        API.sendNotificationToPlayer(player, "~g~Поздравляем! ~s~Вы устроились автомехаником.");

                        // TODO: ADD CLOTHES!!!
                        //ClothesManager.SetPlayerSkinClothes(player, 0, characterController.Character, false);
                        break;

                    case 0:
                        character.JobId = JobsIdNonDataBase.Unemployer;
                        ContextFactory.Instance.SaveChanges();
                        ClothesManager.SetPlayerSkinClothes(player, 0, characterController.Character, false);
                        break;
                }
            }
            if (eventName == "work_loader")
            {
                if (character == null) return;
                var callback = (int)args[0];
                var jobId = (int)args[1];
                var posX = (float)args[2];
                var posY = (float)args[3];
                var posZ = (float)args[4];

                if (callback == 1)
                {
                    character.JobId = jobId;
                    ContextFactory.Instance.SaveChanges();
                    API.shared.triggerClientEvent(player, "loader_one", posX, posY, posZ);
                    player.setData("SECOND_OK", null);
                    var isMale = character.Model == 1885233650;
                    ClothesManager.SetPlayerSkinClothes(player, isMale ? 700 : 7000, characterController.Character, false);
                }
                if (callback == 0)
                {
                    character.JobId = 0;
                    ContextFactory.Instance.SaveChanges();
                    API.shared.triggerClientEvent(player, "loader_end");
                    API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
                    ClothesManager.SetPlayerSkinClothes(player, 0, characterController.Character, false);
                    player.resetData("FIRST_OK");
                    player.resetData("SECOND_OK");
                }
            }
            if (eventName == "work_busdriver")
            {
                if (character == null) return;
                if (character.Level < 2)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не можете устроиться. Нужен 2 уровень!");
                    return;
                }

                var callback = (int)args[0];

                if (callback != 0)
                    API.sendNotificationToPlayer(player, "~g~Поздравляем! ~s~Вы устроились водителем автобуса.");

                // BusOne trace
                switch (callback)
                {
                    case 1: JobController.BusDriverStart(player, character, "BUSONE_1", callback, 
                        BusOne.Marker1.X, BusOne.Marker1.Y, BusOne.Marker1.Z); break;
                    case 2: JobController.BusDriverStart(player, character, "BUSTWO_1", callback,
                        BusTwo.Marker1.X, BusTwo.Marker1.Y, BusTwo.Marker1.Z); break;
                    case 3: JobController.BusDriverStart(player, character, "BUSTHREE_1", callback,
                        BusThree.Marker1.X, BusThree.Marker1.Y, BusThree.Marker1.Z); break;
                    case 4: JobController.BusDriverStart(player, character, "BUSFOUR_1", callback,
                        BusFour.Marker1.X, BusFour.Marker1.Y, BusFour.Marker1.Z); break;
                }

                // BusDriver finish work
                if (callback == 0)
                {
                    character.JobId = JobsIdNonDataBase.Unemployer;
                    ContextFactory.Instance.SaveChanges();
                    API.shared.triggerClientEvent(player, "bus_end");
                    ClothesManager.SetPlayerSkinClothes(player, 0, characterController.Character, false);
                    for (var i = 1; i < 6; i++) player.resetData(string.Format("BUSONE_" + i));
                    for (var i = 1; i < 6; i++) player.resetData(string.Format("BUSTWO_" + i));
                    for (var i = 1; i < 6; i++) player.resetData(string.Format("BUSTHREE_" + i));
                    for (var i = 1; i < 6; i++) player.resetData(string.Format("BUSFOUR_" + i));
                }
            }
            if (eventName == "work_gasstation")
            {
                var trigger = args[0];
                var jobId = (int)args[1];

                var currentJob = ContextFactory.Instance.Job.First(x => x.Id == jobId);

                switch (trigger)
                {
                    // Buy GasStation:
                    case 0:
                        var cost = (int)args[2];
                        if (character.Cash - cost < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас недостаточно средств для покупки!");
                            break;
                        }
                        else
                        {
                            currentJob.OwnerName = character.Name;

                            var listBlips = API.getAllBlips();
                            foreach (var blip in listBlips)
                            {
                                var blipPos = API.getBlipPosition(blip);
                                if (blipPos.X == currentJob.PosX && blipPos.Y == currentJob.PosY && blipPos.Z == currentJob.PosZ)
                                    API.setBlipColor(blip, 1);
                            }
                            var labels = API.getAllLabels();
                            foreach (var label in labels)
                            {
                                var labelText = API.getTextLabelText(label);
                                if (labelText.Contains("~w~Бизнес: заправка\n(свободен): " + currentJob.Id))
                                    API.setTextLabelText(label, "~w~Бизнес: заправка\nВладелец: " + currentJob.OwnerName);
                            }
                            currentJob.CharacterId = character.Id;
                            
                            character.Cash -= cost;
                            ContextFactory.Instance.SaveChanges();
                            API.sendNotificationToPlayer(player, "Вы купили заправку за " + cost + "$");
                            API.triggerClientEvent(player, "update_money_display", character.Cash);
                            break;
                        }                        
                    
                    // 24/7 menu:
                    case 1:
                        var command = (string)args[2];

                        switch (command)
                        {
                            case "canister": break;
                            case "phone":
                                if (character.MobilePhone != 0)
                                {
                                    API.sendNotificationToPlayer(player, "~r~У вас уже есть телефон!");
                                }
                                else
                                {
                                    if (character.Cash - Prices.MobilePhone < 0)
                                    {
                                        API.sendNotificationToPlayer(player, "~r~У вас недостаточно денег для покупки!");
                                        break;
                                    }
                                    character.Cash -= Prices.MobilePhone;
                                    var lastMobileNumber = ContextFactory.Instance.Character.Max(x => x.MobilePhone);
                                    character.MobilePhone = lastMobileNumber + 1;
                                    ContextFactory.Instance.SaveChanges();
                                    API.sendNotificationToPlayer(player, "~g~Вы купили телефон. Ваш номер: " + (lastMobileNumber + 1));
                                    API.triggerClientEvent(player, "update_money_display", character.Cash);
                                }
                                break;
                            case "food": break;
                            case "getmoney":
                                character.Cash += (int)args[3];
                                currentJob.Money = 0;
                                ContextFactory.Instance.SaveChanges();
                                API.sendNotificationToPlayer(player, "Вы сняли с кассы ~g~" + (int)args[3] + "$");
                                API.triggerClientEvent(player, "update_money_display", character.Cash); break;
                            case "sell":
                                if (currentJob.CharacterId == character.Id)
                                {
                                    var listBlips = API.getAllBlips();
                                    foreach (var blip in listBlips)
                                    {
                                        var blipPos = API.getBlipPosition(blip);
                                        if (blipPos.X == currentJob.PosX && blipPos.Y == currentJob.PosY && blipPos.Z == currentJob.PosZ)
                                            API.setBlipColor(blip, 2);
                                    }
                                    var labels = API.getAllLabels();
                                    foreach (var label in labels)
                                    {
                                        var labelText = API.getTextLabelText(label);
                                        if (labelText.Contains("~w~Бизнес: заправка\nВладелец: " + currentJob.OwnerName))
                                            API.setTextLabelText(label, "~w~Бизнес: заправка\n(свободен): " + currentJob.Id);
                                    }
                                    character.Cash += currentJob.Cost;
                                    currentJob.CharacterId = 0;
                                    currentJob.OwnerName = "";
                                    ContextFactory.Instance.SaveChanges();
                                    API.sendNotificationToPlayer(player, "Вы продали заправку за ~g~" + currentJob.Cost + "$");
                                    API.triggerClientEvent(player, "update_money_display", character.Cash); break;
                                }
                                else
                                {
                                    API.sendNotificationToPlayer(player, "~r~Вы не можете продать данную заправку!");
                                }
                                break;
                        }
                        break;
                }
            }
            if (eventName == "work_clothstore")
            {
                var trigger = args[0];
                var jobId = (int)args[1];

                var currentJob = ContextFactory.Instance.Job.First(x => x.Id == jobId);

                switch (trigger)
                {
                    case 0:
                        var cost = (int)args[2];
                        if (character.Cash - cost < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас недостаточно средств для покупки!");
                            break;
                        }
                        else
                        {
                            currentJob.OwnerName = character.Name;

                            var listBlips = API.getAllBlips();
                            foreach (var blip in listBlips)
                            {
                                var blipPos = API.getBlipPosition(blip);
                                if (blipPos.X == currentJob.PosX && blipPos.Y == currentJob.PosY && blipPos.Z == currentJob.PosZ)
                                    API.setBlipColor(blip, 1);
                            }
                            var labels = API.getAllLabels();
                            foreach (var label in labels)
                            {
                                var labelText = API.getTextLabelText(label);
                                if (labelText.Contains("~w~Бизнес: магазин одежды\n(свободен): " + currentJob.Id))
                                    API.setTextLabelText(label, "~w~Бизнес: магазин одежды\nВладелец: " + currentJob.OwnerName);
                            }
                            currentJob.CharacterId = character.Id;

                            character.Cash -= cost;
                            ContextFactory.Instance.SaveChanges();
                            API.sendNotificationToPlayer(player, "Вы купили магазин одежды за " + cost + "$");
                            API.triggerClientEvent(player, "update_money_display", character.Cash);
                            break;
                        }
                        
                    case 1:
                        var command = (string)args[2];

                        switch (command)
                        {
                            case "getmoney":
                                character.Cash += (int)args[3];
                                currentJob.Money = 0;
                                ContextFactory.Instance.SaveChanges();
                                API.sendNotificationToPlayer(player, "Вы сняли с кассы ~g~" + (int)args[3] + "$");
                                API.triggerClientEvent(player, "update_money_display", character.Cash); break;
                            case "sell":
                                if (currentJob.CharacterId == character.Id)
                                {
                                    var listBlips = API.getAllBlips();
                                    foreach (var blip in listBlips)
                                    {
                                        var blipPos = API.getBlipPosition(blip);
                                        if (blipPos.X == currentJob.PosX && blipPos.Y == currentJob.PosY && blipPos.Z == currentJob.PosZ)
                                            API.setBlipColor(blip, 2);
                                    }
                                    var labels = API.getAllLabels();
                                    foreach (var label in labels)
                                    {
                                        var labelText = API.getTextLabelText(label);
                                        if (labelText.Contains("~w~Бизнес: магазин одежды\nВладелец: " + currentJob.OwnerName))
                                            API.setTextLabelText(label, "~w~Бизнес: магазин одежды\n(свободен): " + currentJob.Id);
                                    }
                                    character.Cash += currentJob.Cost;
                                    currentJob.CharacterId = 0;
                                    currentJob.OwnerName = "";
                                    ContextFactory.Instance.SaveChanges();
                                    API.sendNotificationToPlayer(player, "Вы продали магазин одежды за ~g~" + currentJob.Cost + "$");
                                    API.triggerClientEvent(player, "update_money_display", character.Cash);
                                }
                                else
                                {
                                    API.sendNotificationToPlayer(player, "~r~Вы не можете продать данный магазин одежды!");
                                }
                                break;
                        }
                        break;
                }
            }
            if (eventName == "work_ammunation")
            {
                var trigger = args[0];
                var jobId = (int)args[1];

                var currentJob = ContextFactory.Instance.Job.First(x => x.Id == jobId);

                switch (trigger)
                {
                    // Buy GasStation:
                    case 0:
                        var cost = (int)args[2];
                        if (character.Cash - cost < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас недостаточно средств для покупки!");
                            break;
                        }
                        else
                        {
                            currentJob.OwnerName = character.Name;

                            var listBlips = API.getAllBlips();
                            foreach (var blip in listBlips)
                            {
                                var blipPos = API.getBlipPosition(blip);
                                if (blipPos.X == currentJob.PosX && blipPos.Y == currentJob.PosY && blipPos.Z == currentJob.PosZ)
                                    API.setBlipColor(blip, 1);
                            }
                            var labels = API.getAllLabels();
                            foreach (var label in labels)
                            {
                                var labelText = API.getTextLabelText(label);
                                if (labelText.Contains("~w~Бизнес: магазин оружия\n(свободен): " + currentJob.Id))
                                    API.setTextLabelText(label, "~w~Бизнес: магазин оружия\nВладелец: " + currentJob.OwnerName);
                            }
                            currentJob.CharacterId = character.Id;

                            character.Cash -= cost;
                            ContextFactory.Instance.SaveChanges();
                            API.sendNotificationToPlayer(player, "Вы купили магазин оружия за " + cost + "$");
                            API.triggerClientEvent(player, "update_money_display", character.Cash);
                            break;
                        }
                        
                    case 1:
                        var command = (string)args[2];

                        switch (command)
                        {
                            case "getmoney":
                                character.Cash += (int)args[3];
                                currentJob.Money = 0;
                                ContextFactory.Instance.SaveChanges();
                                API.sendNotificationToPlayer(player, "Вы сняли с кассы ~g~" + (int)args[3] + "$");
                                API.triggerClientEvent(player, "update_money_display", character.Cash); break;
                            case "sell":
                                if (currentJob.CharacterId == character.Id)
                                {
                                    var listBlips = API.getAllBlips();
                                    foreach (var blip in listBlips)
                                    {
                                        var blipPos = API.getBlipPosition(blip);
                                        if (blipPos.X == currentJob.PosX && blipPos.Y == currentJob.PosY && blipPos.Z == currentJob.PosZ)
                                            API.setBlipColor(blip, 2);
                                    }
                                    var labels = API.getAllLabels();
                                    foreach (var label in labels)
                                    {
                                        var labelText = API.getTextLabelText(label);
                                        if (labelText.Contains("~w~Бизнес: магазин оружия\nВладелец: " + currentJob.OwnerName))
                                            API.setTextLabelText(label, "~w~Бизнес: магазин оружия\n(свободен): " + currentJob.Id);
                                    }
                                    character.Cash += currentJob.Cost;
                                    currentJob.CharacterId = 0;
                                    currentJob.OwnerName = "";
                                    ContextFactory.Instance.SaveChanges();
                                    API.sendNotificationToPlayer(player, "Вы продали магазин оружия за ~g~" + currentJob.Cost + "$");
                                    API.triggerClientEvent(player, "update_money_display", character.Cash);
                                }
                                else
                                {
                                    API.sendNotificationToPlayer(player, "~r~Вы не можете продать данный магазин оружия!");
                                }
                                break;
                        }
                        break;
                }
            }
            if (eventName == "get_petrolium")
            {
                var jobId = (int)args[0];
                var fuel = (int)args[1];
                var trigger = (int) args[2];
                var currentJob = ContextFactory.Instance.Job.First(x => x.Id == jobId);

                if (character.Cash - fuel < 0)
                {
                    API.sendNotificationToPlayer(player,
                        character.JobId == JobsIdNonDataBase.Mechanic
                            ? "~r~У вас недостаточно средств для покупки!"
                            : "~r~У вас недостаточно средств для заправки!");
                    return;
                }
                VehicleController currentVehicleController = null;
                if (player.isInVehicle) currentVehicleController = EntityManager.GetVehicle(player.vehicle);

                if (trigger == 0)
                {
                    if (currentVehicleController.VehicleData.Fuel + fuel > currentVehicleController.VehicleData.FuelTank)
                    {
                        API.sendNotificationToPlayer(player, "~r~В ваш бак больше не лезет топливо!");
                        return;
                    }
                    currentVehicleController.VehicleData.Fuel += fuel;
                    character.Cash -= fuel;
                }
                if (trigger == 1)
                {
                    if (character.JobId != JobsIdNonDataBase.Mechanic)
                    {
                        API.sendNotificationToPlayer(player, "~r~Покупать топливо могут только автомеханики!");
                        return;
                    }
                    if (currentVehicleController.VehicleData.Material + fuel > currentVehicleController.VehicleData.FuelTank * 4)
                    {
                        API.sendNotificationToPlayer(player, "~r~В ваш багажник больше не лезет топливо!");
                        return;
                    }
                    currentVehicleController.VehicleData.Material += fuel;
                    character.Cash -= fuel;
                }
                currentJob.Money += fuel;
                ContextFactory.Instance.SaveChanges();
                API.sendNotificationToPlayer(player, "Вы купили " + fuel + " литров бензина");
                API.triggerClientEvent(player, "update_money_display", character.Cash);
            }
            if (eventName == "work_taxi")
            {
                var callback = (int)args[0];

                if (callback == 1) // Начало работы 
                {
                    var playerVehicles = ContextFactory.Instance.Vehicle.Where(x => x.JobId == JobsIdNonDataBase.TaxiDriver).ToList();

                    var isPlayerInTaxi = false;

                    foreach (var playerVehicle in playerVehicles)
                    {
                        if (playerVehicle.JobId == character.JobId) isPlayerInTaxi = true;                        
                    }

                    if (isPlayerInTaxi)
                    {
                        if (character.Cash - Prices.TaxiRentPrice < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы не можете работать таксистом!\nУ вас нет " + Prices.TaxiRentPrice + "$ на аренду авто!");
                        }
                        else
                        {
                            var taxiVehicles = ContextFactory.Instance.Vehicle.Where(x => x.JobId == JobsIdNonDataBase.TaxiDriver).ToList();
                            var hasPlayerTaxi = false;

                            foreach (var taxi in taxiVehicles)
                                if (taxi.Character == character) hasPlayerTaxi = true;                            

                            if (hasPlayerTaxi)
                                API.sendNotificationToPlayer(player, "~r~У вас уже есть арендованное такси для работы!");
                            else
                            {
                                vehicleController.VehicleData.Character = characterController.Character;
                                vehicleController.VehicleData.RentTime += Time.TaxiRentTime;
                                character.Cash -= Prices.TaxiRentPrice;
                                character.TempVar = character.JobId;
                                ContextFactory.Instance.SaveChanges();

                                API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
                                API.setEntityData(player, "TAXI", true);
                                API.sendNotificationToPlayer(player, "~g~Вы за рулем такси,~s~ждите вызовов клиентов\nиспользуйте кнопку 3 для принятия заявки.");
                            }
                        }
                    }
                    else API.sendChatMessageToPlayer(player, "~r~Вы не за рулем такси,~s~ сядьте в машину\nи после этого вы сможете принимать заявки");
                }
                if (callback == 2) // [Command("accept")] 
                {
                    if (API.getEntityData(player.handle, "TAXI"))
                    {
                        var id = API.random();
                        API.call("JobController", "Accepted", player, id);
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "~r~You are not a job");
                    }
                }
                if (callback == 3) // [Command("done")]
                {
                    API.setEntityData(player, "TASK", 1.623482);
                    API.sendChatMessageToPlayer(player, "~g~Вы свободны для клиентов.");
                }
                if (callback == 4)
                {
                    if (character.Cash - Prices.TaxiRentPrice < 0)
                    {
                        if (character.DriverLicense == 0)
                        {
                            API.sendChatMessageToPlayer(player, "~r~Вы не можете работать таксистом! У вас нет водительских прав!");
                            return;
                        }
                        API.sendChatMessageToPlayer(player, "~r~Вы не можете работать таксистом!\nУ вас нет " + Data.Models.Prices.TaxiRentPrice + "$ на аренду авто!");
                    }
                    else
                    {
                        if (character.JobId != JobsIdNonDataBase.TaxiDriver && character.Level >= 2)
                        {
                            API.sendChatMessageToPlayer(player, "~g~Поздравляем! Вы устроились на работу таксистом!\nПроследуйте в ближайщий таксопарк для аренды такси.");
                            character.JobId = JobsIdNonDataBase.TaxiDriver;
                            ContextFactory.Instance.SaveChanges();
                            API.shared.triggerClientEvent(player, "markonmap", -1024, -2728);
                        }
                        else if (character.JobId == JobsIdNonDataBase.TaxiDriver && character.Level >= 2)
                            API.sendChatMessageToPlayer(player, "~r~Вы уже работаете таксистом.");
                        else API.sendChatMessageToPlayer(player, "~r~Вы не можете работать таксистом!");
                    }
                }
                if (callback == 0) // Уволиться с работы
                {
                    if (player.isInVehicle)
                    {
                        API.sendNotificationToPlayer(player, "Пожалуйста выйдите из машины перед увольнением.");
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "~r~Вы уволились с работы таксиста.\n~s~Для получения пособия пройдите в мэрию");
                        character.JobId = 0;
                        character.TempVar = 0;

                        var playerTaxiVehicle = ContextFactory.Instance.Vehicle.FirstOrDefault(x => x.CharacterId.ToString() == character.Id.ToString());
                        playerTaxiVehicle.Character = null;
                        playerTaxiVehicle.RentTime = 0;
                        ContextFactory.Instance.SaveChanges();

                        VehicleController.RespawnWorkVehicle(playerTaxiVehicle, RentModels.TaxiModel, 126, 126);
                    }                    
                }
            }
            if (eventName == "get_taxi")
            {
                API.call("JobController", "UseTaxis", player);
                API.sendNotificationToPlayer(player, "~b~Вызов такси для вас!");
            }

            // CHARACTER    (TRY OK):
            if (eventName == "narco_menu")
            {
                try
                {
                    var drugs = (int) args[0];
                    if (character.Narco - drugs < 0)
                    {
                        API.sendNotificationToPlayer(player, "~r~У вас нет столько наркотиков!");
                        return;
                    }
                    character.Narco -= drugs;
                    var playerHealth = API.getPlayerHealth(player);
                    API.setPlayerHealth(player, playerHealth + drugs * 10);
                    character.NarcoDep += drugs * 4;
                    ContextFactory.Instance.SaveChanges();
                    API.sendNotificationToPlayer(player,
                        "~g~Вы приняли" + drugs + " грамм наркотиков. Ваша зависимость: " + character.NarcoDep);

                    API.shared.triggerClientEvent(player, "player_effect", "DrugsDrivingIn", 30000, false);
                    Global.Util.delay(30000, () =>
                    {
                        API.shared.triggerClientEvent(player, "player_effect_stop", "DrugsDrivingIn");
                        API.shared.triggerClientEvent(player, "player_effect", "DrugsDrivingOut", 5000, false);
                    });
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            // GANG MENU    (TRY OK):
            if (eventName == "gang_menu")
            {
                try
                {
                    if (character == null) return;
                    var propertyName = (string) args[0];
                    var trigger = (int) args[1];

                    var stockName = propertyName;
                    if (propertyName == "Army2_stock") stockName = "Army2_stock";
                    if (propertyName == "Army1_stock") stockName = "Army1_stock";
                    if (propertyName == "Army2_gang") stockName = "Army2_stock";
                    if (propertyName == "Army1_gang") stockName = "Army1_stock";
                    var propertyData = ContextFactory.Instance.Property.FirstOrDefault(x => x.Name == stockName);
                    var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == propertyData.GroupId);
                    var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());

                    if (trigger == 1)
                    {
                        var drugs = (int) args[2];
                        if (character.Narco + drugs > 150)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы не можете нести больше 150 грамм наркотиков!");
                            return;
                        }
                        character.Narco += drugs;
                        if (character.Cash - (drugs * 10) < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас недостаточно средств на покупку!");
                            return;
                        }
                        character.Cash -= drugs * 10;
                        var playerGang =
                            ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == character.GroupType * 100);
                        playerGang.MoneyBank += drugs * 10;
                        ContextFactory.Instance.SaveChanges();
                        API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
                        API.sendNotificationToPlayer(player, "~g~Вы взяли " + drugs + " грамм наркотиков!");
                    }
                    // Steal by gang and vagoon
                    if (trigger == 2)
                    {
                        if (propertyData.Stock - 1000 < 0)
                            API.sendNotificationToPlayer(player,
                                "~r~Вы не можете украсть с данного склада!\nНа складе нет материалов!");
                        else
                        {
                            if (character.Material == 1000)
                                API.sendNotificationToPlayer(player,
                                    "~r~Вы не можете украсть с данного склада!\nВы перегружены у вас уже: " +
                                    character.Material + " материалов");
                            else
                            {
                                propertyData.Stock -= 1000;
                                character.Material = 1000;
                                ContextFactory.Instance.SaveChanges();
                                API.sendNotificationToPlayer(player,
                                    "~g~Вы украли 1000 материалов со склада: " +
                                    EntityManager.GetDisplayName(groupType) +
                                    "\nЗагрузите в свой транспорт и берите новую порцию со склада.");
                            }
                        }
                    }
                    // Unload materials by gang from vagoon
                    if (trigger == 3)
                    {
                        VehicleController vehicleControllerLoad = null;

                        if (player.isInVehicle)
                            vehicleControllerLoad = EntityManager.GetVehicle(player.vehicle);
                        else API.sendNotificationToPlayer(player, "Вы не в транспорте!");
                        if (vehicleControllerLoad == null) return;

                        if (vehicleControllerLoad.VehicleData.Material != 0)
                        {
                            propertyData.Stock += vehicleControllerLoad.VehicleData.Material;

                            API.sendNotificationToPlayer(player,
                                "~g~Вы разгрузили " + vehicleControllerLoad.VehicleData.Material +
                                " материалов с машины.\nНа свой склад.");
                            vehicleControllerLoad.VehicleData.Material = 0;
                            ContextFactory.Instance.SaveChanges();
                        }
                        else API.sendNotificationToPlayer(player, "~r~В вашей машине нет материалов!");
                    }
                    // FORM:
                    // DRESS ARMY CLOTHES:
                    if (trigger == 4)
                    {
                        if (character == null) return;

                        if (character.ClothesTypes != 0)
                        {
                            ClothesManager.SetPlayerSkinClothes(player, character.ClothesTypes, character, false);
                            character.ActiveClothes = character.ClothesTypes;
                            ContextFactory.Instance.SaveChanges();
                        }
                        else API.sendNotificationToPlayer(player, "~s~У вас ~r~нет ~s~доступной формы!");
                    }
                    // DRESS GANG CLOTHES:
                    if (trigger == 5)
                    {
                        if (character == null) return;
                        var isMale = character.Model == 1885233650;
                        int cloth;
                        switch (propertyName)
                        {
                            case "Ballas_main":
                                cloth = isMale ? 131 : 1310;
                                break;
                            case "Vagos_main":
                                cloth = isMale ? 141 : 1410;
                                break;
                            case "LaCostaNotsa_main":
                                cloth = isMale ? 151 : 1510;
                                break;
                            case "GroveStreet_main":
                                cloth = isMale ? 161 : 1610;
                                break;
                            case "TheRifa_main":
                                cloth = isMale ? 171 : 1710;
                                break;
                            default:
                                cloth = isMale ? 999 : 9990;
                                break;
                        }
                        if (character.ActiveClothes == cloth)
                        {
                            API.sendNotificationToPlayer(player, "~s~Вы ~y~уже одеты ~s~в форму своей банды");
                        }
                        else
                        {
                            ClothesManager.SetPlayerSkinClothesToDb(player, cloth, character, false);
                            character.ActiveClothes = cloth;
                            ContextFactory.Instance.SaveChanges();
                            API.sendNotificationToPlayer(player, "~s~Вы ~g~успешно ~s~одели форму своей банды");
                        }
                    }
                    // TAKE THE METALL:
                    if (trigger == 6)
                    {
                        if (character == null) return;
                        character.TempVar = 0;
                        character.Cash += 500;
                        ContextFactory.Instance.SaveChanges();
                        API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
                        API.sendNotificationToPlayer(player, "~g~Вы успешно сдали металл и получили 500$");
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }            
            if (eventName == "gang_weapon")
            {
                if (character == null) return;
                var callback = (int)args[0];
                var cost = (int)args[1];

                if (character.Material - cost < 0)
                    API.sendNotificationToPlayer(player, "~r~Вы не можете создать оружие!\nУ вас недостаточно материалов!");
                else
                {
                    switch (callback)
                    {
                        case 1: character.Material -= cost;
                            API.givePlayerWeapon(player, WeaponHash.APPistol, 150, true, true); break;
                        case 2: character.Material -= cost;
                            API.givePlayerWeapon(player, WeaponHash.SMG, 250, true, true); break;
                        case 3: character.Material -= cost;
                            API.givePlayerWeapon(player, WeaponHash.AdvancedRifle, 350, true, true); break;
                        case 4: character.Material -= cost;
                            API.givePlayerWeapon(player, WeaponHash.HeavySniper, 150, true, true); break;
                        case 5: character.Material -= cost;
                            API.givePlayerWeapon(player, WeaponHash.GrenadeLauncher, 150, true, true); break;
                        case 6: character.Material -= cost;
                            API.givePlayerWeapon(player, WeaponHash.Grenade, 150, true, true); break;
                    }
                }
                ContextFactory.Instance.SaveChanges();                
            }
            if (eventName == "gang_add_money")
            {
                try
                {
                    var money = (int) args[0];
                    var gangGroupBank = character.GroupType * 100;
                    var gangBank = ContextFactory.Instance.Group.First(x => x.Id == gangGroupBank);
                    if (character.Cash - money < 0)
                    {
                        API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~У вас недостаточно средств для вклада!");
                        return;
                    }
                    character.Cash -= money;
                    gangBank.MoneyBank += money;
                    API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
                    ContextFactory.Instance.SaveChanges();
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "gang_get_money")
            {
                try
                {
                    var money = (int) args[0];
                    var gangGroupBank = character.GroupType * 100;
                    var gangBank = ContextFactory.Instance.Group.First(x => x.Id == gangGroupBank);
                    if (gangBank.MoneyBank - money < 0)
                    {
                        API.sendNotificationToPlayer(player,
                            "~r~[ОШИБКА]: ~w~В банке фракции недостаточно средств для снятия!");
                        return;
                    }
                    character.Cash += money;
                    gangBank.MoneyBank -= money;
                    API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
                    ContextFactory.Instance.SaveChanges();
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "gang_rob_house")
            {
                try
                {
                    var targetCharacterId = (int) args[0];
                    var targetCharacter =
                        ContextFactory.Instance.Character.FirstOrDefault(x => x.Id == targetCharacterId);
                    if (targetCharacter == null || targetCharacter.Online == false) return;
                    character.TempVar = 111;
                    ContextFactory.Instance.SaveChanges();
                    API.sendNotificationToPlayer(player, "~g~Вы успешно украли металл и теперь можете сдать его.");
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "gang_get_material")
            {
                try
                {
                    var material = (int) args[0];
                    var gangStockName = GroupController.GetGroupStockName(character);
                    var gangStockProperty = ContextFactory.Instance.Property
                        .First(x => x.Name == gangStockName);
                    if (gangStockProperty.Stock - material < 0)
                    {
                        API.sendNotificationToPlayer(player,
                            "~r~[ОШИБКА]: ~w~В банке банды недостаточно материалов для снятия!");
                        return;
                    }
                    character.Material += material;
                    gangStockProperty.Stock -= material;
                    ContextFactory.Instance.SaveChanges();
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "gang_capting_sector")
            {
                var startCapting = ContextFactory.Instance.Caption.First(x => x.Id == 1);
                if (startCapting.Sector != "0;0")
                {
                    API.sendNotificationToPlayer(player, "~r~Недоступно. В данный момент идет чужой капт сектора: " + startCapting.Sector);
                    return;
                }
                /*
                if (startCapting.Sector == "0;0" && DateTime.Now.Hour-startCapting.Time.Hour < 2)
                {
                    API.sendNotificationToPlayer(player, "~r~Недоступно. С момента последнего капта еще не прошло 2 часа! Последнее время капта: " + startCapting.Time);
                    return;
                }*/
                startCapting.Time = DateTime.Now;
                startCapting.Sector = CharacterController.InWhichSectorOfGhetto(player);
                startCapting.GangAttack = character.GroupType;
                var gangSector = CharacterController.InWhichSectorOfGhetto(player).Split(';');                
                startCapting.GangDefend = GroupController.GetGangSectorData(Convert.ToInt32(gangSector[0]), Convert.ToInt32(gangSector[1]));
            }
            if (eventName == "load_unload_material")
            {
                var trigger = (int)args[0];

                // Load material into vagoon
                if (trigger == 1)
                {
                    var vehicleControllerLoad = player.isInVehicle ? EntityManager.GetVehicle(player.vehicle) 
                        : EntityManager.GetVehicleControllers().Find(x => x.Vehicle.position.DistanceTo(player.position) < 3.0f);
                    if (vehicleControllerLoad == null)
                    {
                        API.sendNotificationToPlayer(player, "Вы находитесь далеко от транспорта!");
                        return;
                    }
                    if (vehicleControllerLoad.VehicleData.Material + character.Material <= 10000)
                    {
                        vehicleControllerLoad.VehicleData.Material += character.Material;
                        API.sendChatMessageToPlayer(player, "~g~Вы загрузили " + character.Material + " материалов со склада.\nВ свой транспорт. Берите очередную новую порцию со склада.");
                        character.Material = 0;
                        ContextFactory.Instance.SaveChanges();
                    }
                    else API.sendNotificationToPlayer(player, "~r~Вы не можете загрузить в эту машину больше!\nОна перегружена и в ней" + vehicleControllerLoad.VehicleData.Material + " материалов.");
                }                
            }
            if (eventName == "gang_add_to_group")
            {
                try
                {
                    var callBack = (int) args[2];
                    if (character == null) return;

                    // Принятие в банду
                    if (callBack == 1)
                    {
                        try
                        {
                            var userId = (int) args[0];
                            var gangId = (int) args[1];
                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;

                            var target = API.shared.getAllPlayers()
                                .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                            if (target == null) return;
                            if (player.position.DistanceTo(target.position) > 3.0F)
                            {
                                API.sendNotificationToPlayer(player,
                                    "~y~[ПРЕДУПРЕЖДЕНИЕ]: ~w~Вы находитесь далеко от пользователя!");
                                return;
                            }

                            var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == gangId);
                            var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
                            var groupExtraType =
                                (GroupExtraType) Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());

                            targetCharacter.ActiveGroupID = gangId;
                            targetCharacter.GroupType = (int) groupType;
                            targetCharacter.JobId = 0;
                            targetCharacter.ActiveClothes =
                                ClothesManager.SetFractionClothes(target, gangId, targetCharacter);
                            ContextFactory.Instance.SaveChanges();

                            target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " принял вас в банду: " +
                                                   EntityManager.GetDisplayName(groupType) + "\nНа должность: " +
                                                   EntityManager.GetDisplayName(groupExtraType));
                            API.sendChatMessageToPlayer(player,
                                "~g~[УСПЕШНО]: ~w~Вы перевели пользователя: " + targetCharacter.Name +
                                "\nВо фракцию: " + EntityManager.GetDisplayName(groupType) + "\nНа должность: " +
                                EntityManager.GetDisplayName(groupExtraType));
                        }
                        catch (Exception)
                        {
                            API.sendChatMessageToPlayer(player,
                                "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                            return;
                        }
                    }
                    // Выгнать из банды
                    if (callBack == 2)
                    {
                        try
                        {
                            var userId = (int) args[0];
                            var gangId = character.GroupType * 100;
                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;

                            if (targetCharacter.GroupType == character.GroupType &&
                                CharacterController.IsCharacterGangBoss(character))
                            {
                                targetCharacter.ActiveGroupID = 1;
                                targetCharacter.GroupType = 100;

                                var target = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                if (target == null) return;
                                targetCharacter.ActiveClothes =
                                    ClothesManager.SetFractionClothes(target, 0, targetCharacter);
                                ContextFactory.Instance.SaveChanges();

                                var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == gangId);
                                var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());

                                target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                                       " выгнал вас из банды: " +
                                                       EntityManager.GetDisplayName(groupType) +
                                                       "\nДля пособия по безработице - проследуйте в мэрию.");
                                API.sendChatMessageToPlayer(player,
                                    "~g~[УСПЕШНО]: ~w~Вы выгнали пользователя: " + targetCharacter.Name +
                                    "\nИз фракции: " + EntityManager.GetDisplayName(groupType));
                            }
                            else
                            {
                                API.sendNotificationToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции\nИли вы пытаетесь выгнать сами себя!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player,
                                "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                            return;
                        }
                    }
                    // Поменять ранг
                    if (callBack == 3)
                    {
                        try
                        {
                            var userId = (int) args[0];
                            var rangId = (int) args[1];
                            var groupId = (int) args[3];
                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;

                            var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == groupId);
                            var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
                            var groupExtraType =
                                (GroupExtraType) Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());
                            var gangGroupId = (int) groupType * 100 + rangId;

                            if (gangGroupId >= groupId)
                            {
                                API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~Присваемый ранг выше вашего!");
                                return;
                            }
                            targetCharacter.ActiveGroupID = gangGroupId;
                            ContextFactory.Instance.SaveChanges();

                            var target = API.shared.getAllPlayers()
                                .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                            if (target == null) return;
                            target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " присвоил вам ранг: " +
                                                   EntityManager.GetDisplayName(groupExtraType));
                            API.sendChatMessageToPlayer(player,
                                "~g~[УСПЕШНО]: ~w~Вы присвоили пользователю: " + targetCharacter.Name +
                                "\nРанг в банде: " + EntityManager.GetDisplayName(groupExtraType));
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player,
                                "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            // MAFIA MENU   (TRY OK):
            if (eventName == "mafia_get_info")
            {
                try
                {
                    var targetId = (int) args[0];
                    var targetCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == targetId);
                    if (targetCharacter == null)
                    {
                        API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                        return;
                    }
                    string roof;
                    var mafiaGroupProperty = targetCharacter.MafiaRoof * 100;

                    if (mafiaGroupProperty == 0) roof = "нет";
                    else
                    {
                        var mafiaBank = ContextFactory.Instance.Group
                            .First(x => x.Id == mafiaGroupProperty);
                        var groupType = (GroupType) Enum.Parse(typeof(GroupType), mafiaBank.Type.ToString());
                        roof = EntityManager.GetDisplayName(groupType);
                    }
                    string isDebtOff = "нет";
                    if (targetCharacter.Debt != 0)
                    {
                        isDebtOff = DateTime.Now.Subtract(targetCharacter.DebtDate).Hours > 24 ? "да" : "нет";
                    }

                    API.sendNotificationToPlayer(player,
                        "~g~Имя: ~w~" + targetCharacter.Name +
                        "\n~g~Крыша: ~w~" + roof +
                        "\n~g~Долг: ~w~" + targetCharacter.Debt +
                        "\n~g~Просрочен: ~w~" + isDebtOff);
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            // POLICE       (TRY OK):
            if (eventName == "police_add_to_group")
            {
                try
                {
                    var callBack = (int) args[0];
                    if (character == null) return;

                    // Принятие в police
                    if (callBack == 1)
                    {
                        try
                        {
                            var userId = (int) args[1];
                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;

                            var target = API.shared.getAllPlayers()
                                .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                            if (target == null) return;
                            if (player.position.DistanceTo(target.position) < 3.0F)
                            {
                                API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от пользователя.");
                                return;
                            }

                            var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 100);
                            var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
                            var groupExtraType =
                                (GroupExtraType) Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());

                            targetCharacter.ActiveGroupID = 101;
                            targetCharacter.GroupType = (int) groupType;
                            targetCharacter.JobId = 0;
                            targetCharacter.ActiveClothes =
                                ClothesManager.SetFractionClothes(target, 101, targetCharacter);
                            ContextFactory.Instance.SaveChanges();

                            target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " принял вас в: " +
                                                   EntityManager.GetDisplayName(groupType) + "\nНа звание: " +
                                                   EntityManager.GetDisplayName(groupExtraType));
                            API.sendChatMessageToPlayer(player,
                                "~g~[УСПЕШНО]: ~w~Вы перевели пользователя: " + targetCharacter.Name +
                                "\nВо фракцию: " + EntityManager.GetDisplayName(groupType) + "\nНа должность: " +
                                EntityManager.GetDisplayName(groupExtraType));
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                    // Выгнать из фракции
                    if (callBack == 2)
                    {
                        try
                        {
                            var userId = (int) args[1];
                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;

                            if (targetCharacter.GroupType == character.GroupType)
                            {
                                var target = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                if (target == null) return;
                                targetCharacter.ActiveClothes =
                                    ClothesManager.SetFractionClothes(target, 0, targetCharacter);
                                ContextFactory.Instance.SaveChanges();

                                var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 100);
                                var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());

                                target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                                       " выгнал вас из фракции: " +
                                                       EntityManager.GetDisplayName(groupType) +
                                                       "\nДля пособия по безработице - проследуйте в мэрию.");
                                API.sendChatMessageToPlayer(player,
                                    "~g~[УСПЕШНО]: ~w~Вы выгнали пользователя: " + targetCharacter.Name +
                                    "\nИз фракции: " + EntityManager.GetDisplayName(groupType));

                                var isMale = targetCharacter.Model == 1885233650;
                                var baseClothes = isMale ? 999 : 9990;
                                ClothesManager.SetPlayerSkinClothesToDb(target, baseClothes, targetCharacter, false);
                                targetCharacter.ActiveClothes = baseClothes;

                                targetCharacter.ActiveGroupID = 1;
                                targetCharacter.GroupType = 100;
                                ContextFactory.Instance.SaveChanges();
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции.\nЛибо вы пытаетесь выгнать сами себя!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                    // Поменять ранг
                    if (callBack == 3)
                    {
                        try
                        {
                            var userId = (int) args[1];
                            var rangId = (int) args[2];
                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;

                            if (character.ActiveGroupID == 111)
                            {
                                if (rangId >= 1 && rangId <= 10)
                                {
                                    var target = API.shared.getAllPlayers()
                                        .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                    if (target == null) return;

                                    if (player.position.DistanceTo(target.position) < 3.0F)
                                    {
                                        API.sendNotificationToPlayer(player,
                                            "~y~Вы находитесь далеко от пользователя.");
                                        return;
                                    }
                                    targetCharacter.ActiveGroupID = character.GroupType * 100 + rangId;
                                    ContextFactory.Instance.SaveChanges();

                                    var getGroup =
                                        ContextFactory.Instance.Group.FirstOrDefault(
                                            x => x.Id == targetCharacter.ActiveGroupID);
                                    var groupExtraType = (GroupExtraType) Enum.Parse(typeof(GroupExtraType),
                                        getGroup.ExtraType.ToString());
                                    target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                                           " присвоил вам ранг: " +
                                                           EntityManager.GetDisplayName(groupExtraType));
                                    API.sendChatMessageToPlayer(player,
                                        "~g~[УСПЕШНО]: ~w~Вы присвоили пользователю: " + targetCharacter.Name +
                                        "\nРанг: " + EntityManager.GetDisplayName(groupExtraType));
                                }
                                else
                                {
                                    API.sendChatMessageToPlayer(player,
                                        "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции\nИли вам недопустимо присвоение данного звания!");
                                    return;
                                }
                            }
                            if (character.ActiveGroupID == 114)
                            {
                                if (rangId >= 1 && rangId <= 13)
                                {
                                    var target = API.shared.getAllPlayers()
                                        .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                    if (target == null) return;

                                    if (player.position.DistanceTo(target.position) < 3.0F)
                                    {
                                        API.sendNotificationToPlayer(player,
                                            "~y~Вы находитесь далеко от пользователя.");
                                        return;
                                    }
                                    targetCharacter.ActiveGroupID = character.GroupType * 100 + rangId;
                                    ContextFactory.Instance.SaveChanges();

                                    var getGroup =
                                        ContextFactory.Instance.Group.FirstOrDefault(
                                            x => x.Id == targetCharacter.ActiveGroupID);
                                    var groupExtraType = (GroupExtraType) Enum.Parse(typeof(GroupExtraType),
                                        getGroup.ExtraType.ToString());
                                    target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                                           " присвоил вам ранг: " +
                                                           EntityManager.GetDisplayName(groupExtraType));
                                    API.sendChatMessageToPlayer(player,
                                        "~g~[УСПЕШНО]: ~w~Вы присвоили пользователю: " + targetCharacter.Name +
                                        "\nРанг: " + EntityManager.GetDisplayName(groupExtraType));
                                }
                                else
                                {
                                    API.sendChatMessageToPlayer(player,
                                        "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции\nИли вам недопустимо присвоение данного звания!");
                                    return;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "polices_menu")
            {
                try
                {
                    if (character == null) return;
                    var trigger = (int)args[0];

                    // LOCK PLAYER:
                    if (trigger == 1)
                    {
                        var targetUserId = (int)args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        var openCloseVar = (int)args[2];

                        if (openCloseVar == 1)
                        {
                            API.playPlayerAnimation(target, (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "mp_arresting", "idle");

                            API.sendNotificationToPlayer(player, "~g~Вы надели наручники на игрока: " + targetCharacter.Name);
                            API.sendNotificationToPlayer(target, "~r~На вас одели наручники! Следуйте за полицейским!");
                        }
                        else
                        {
                            API.stopPlayerAnimation(target);

                            API.sendNotificationToPlayer(player, "~g~Вы сняли наручники с игрока: " + targetCharacter.Name);
                            API.sendNotificationToPlayer(target, "~g~С вас сняли наручники. Вы свободны.");
                        }
                    }
                    // SET PLAYER TO CAR:
                    if (trigger == 2)
                    {
                        var targetUserId = (int)args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        var openCloseVar = (int)args[2];
                        VehicleController currentVehicleController;
                        if (player.isInVehicle) currentVehicleController = EntityManager.GetVehicle(player.vehicle);
                        else
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы не машине!");
                            return;
                        }

                        if (openCloseVar == 1)
                        {
                            API.setPlayerIntoVehicle(target, currentVehicleController.Vehicle, 1);

                            API.sendNotificationToPlayer(player, "~g~Вы посадили в машину игрока: " + targetCharacter.Name);
                            API.sendNotificationToPlayer(target, "~r~Вас посадили в машину к полицейскому!");
                        }
                        else
                        {
                            API.warpPlayerOutOfVehicle(target);

                            API.sendNotificationToPlayer(player, "~g~Вы высадили из машины игрока: " + targetCharacter.Name);
                            API.sendNotificationToPlayer(target, "~r~Вас высадили из машины. Проследуйте за полицеским!");
                        }
                    }
                    // TO PRISON
                    if (trigger == 3)
                    {
                        var targetUserId = (int)args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;


                        bool isHaveKeys = player.getData("PRISONKEYS");
                        if (!isHaveKeys)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас нет ключей от тюрьмы!");
                            return;
                        }

                        var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        var openCloseVar = (int)args[2];

                        if (openCloseVar == 1)
                        {
                            targetCharacter.PrisonTime = 10;
                            targetCharacter.IsPrisoned = true;
                            API.setEntityPosition(target, new Vector3(458.81, -1001.43, 24.91));
                            API.shared.freezePlayer(target, true);
                            API.shared.setPlayerWantedLevel(target, 0);

                            API.sendNotificationToPlayer(player, "~g~Вы посадили в тюрьму игрока: " + targetCharacter.Name);
                            API.sendNotificationToPlayer(target, "~r~Вас посадили в тюрьму!");
                        }
                        else
                        {
                            targetCharacter.PrisonTime = 0;
                            targetCharacter.IsPrisoned = false;
                            API.setEntityPosition(target, new Vector3(432.43f, -981.47f, 30.71f));
                            API.shared.setPlayerWantedLevel(target, 0);

                            API.sendNotificationToPlayer(player, "~g~Вы выпустили из тюрьмы игрока: " + targetCharacter.Name);
                            API.sendNotificationToPlayer(target, "~g~Вас выпустили из тюрьмы. Вы свободны.");
                        }
                        ContextFactory.Instance.SaveChanges();
                    }
                    // CHANGE CLOTHES:
                    if (trigger == 4)
                    {
                        var clothDo = (string)args[2];
                        var isMale = character.Model == 1885233650;
                        var type = 0;

                        if (clothDo == "Cloth_up" &&
                            CharacterController.IsCharacterInPolice(character))
                        {
                            switch ((int) args[3])
                            {
                                case 1: type = isMale ? 10 : 10011; break;
                                case 2: type = isMale ? 11 : 11011; break;
                                case 3: type = isMale ? 12 : 12011; break;
                                case 4: type = isMale ? 13 : 13011; break;
                            }
                        }
                        if (clothDo == "Cloth_down") type = 0;

                        if (CharacterController.IsCharacterInPolice(character))
                        {
                            ClothesManager.SetPlayerSkinClothes(player, type, characterController.Character, false);
                            characterController.Character.ActiveClothes = type;
                        }
                    }
                    // STARS
                    if (trigger == 5)
                    {
                        var targetUserId = (int)args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        if (targetCharacter == null)
                        {
                            API.shared.sendNotificationToPlayer(player, "~r~Вами был введен неверный ID!");
                            return;
                        }
                        var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        var wantedLevel = (int)args[2];
                        var what = Convert.ToString(args[3]);

                        if (wantedLevel >= 1 && wantedLevel <= 5)
                        {
                            API.setPlayerWantedLevel(target, wantedLevel);

                            API.sendNotificationToPlayer(player, "~g~Вы установили уровень розыска " + wantedLevel + " игроку: " + targetCharacter.Name);
                            API.sendNotificationToPlayer(player, "~y~Причина: " + what);
                            API.sendNotificationToPlayer(target, "~r~Вам установлен уровень розыска: " + wantedLevel);
                            API.sendNotificationToPlayer(target, "~y~Причина: " + what);
                            API.triggerClientEvent(player, "markonmap", target.position.X, target.position.Y);
                        }
                        if (wantedLevel == 0)
                        {
                            API.setPlayerWantedLevel(target, wantedLevel);

                            API.sendNotificationToPlayer(player, "~g~Вы сняли розыск с игрока: " + targetCharacter.Name);
                            API.sendNotificationToPlayer(player, "~y~Причина: " + what);
                            API.sendNotificationToPlayer(target, "~g~Вас больше не разыскивает полиция!");
                            API.sendNotificationToPlayer(target, "~y~Причина: " + what);
                        }
                    }
                    // TICKET:
                    if (trigger == 6)
                    {
                        var targetUserId = (int)args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        var cost = (int)args[2];
                        var what = Convert.ToString(args[3]);

                        targetCharacter.Cash -= cost;
                        character.Cash += cost;
                        ContextFactory.Instance.SaveChanges();

                        API.sendNotificationToPlayer(player, "~g~Вы выписали штраф " + cost + "$ игроку: " + targetCharacter.Name);
                        API.sendNotificationToPlayer(player, "~y~Причина: " + what);
                        API.sendNotificationToPlayer(target, "~r~Вы оплатили штраф: " + cost + "$");
                        API.sendNotificationToPlayer(target, "~y~Причина: " + what);
                    }
                    // SEARCH PLAYER:
                    if (trigger == 7)
                    {
                        var targetUserId = (int)args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 3.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от пользователя.");
                            return;
                        }

                        var isPlayerHaveWeapon = API.getPlayerWeapons(target).Count() == 0 ? "~r~Да" : "~g~Нет";
                        var isPlayerHaveNarco = targetCharacter.Narco != 0 ? string.Format("~r~Да. " + targetCharacter.Narco + " г.") : "~g~Нет";
                        var isPlayerHaveDriverLicense = targetCharacter.DriverLicense == 1 ? "~g~Да" : "~r~Нет";

                        API.sendChatMessageToPlayer(player,
                            "~y~Наличие оружия: " + isPlayerHaveWeapon +
                            "\n~y~Наличие наркотиков: " + isPlayerHaveNarco +
                            "\n~y~Наличие прав: " + isPlayerHaveDriverLicense);
                    }
                    // GET COORD:
                    if (trigger == 8)
                    {
                        var targetUserId = (int)args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (API.getPlayerWantedLevel(target) == 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~Данный игрок не в розыске!");
                            return;
                        }

                        API.sendNotificationToPlayer(player, "~g~Координаты установлены!");
                        API.triggerClientEvent(player, "markonmap", target.position.X, target.position.Y);
                    }
                    // GET KEYS
                    if (trigger == 9)
                    {
                        player.setData("PRISONKEYS", true);
                    }
                    // GET SOMETHING:
                    if (trigger == 10)
                    {
                        var targetUserId = (int)args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var what = Convert.ToString(args[2]);
                        if (what != "weapon" && what != "narco")
                        {
                            API.sendNotificationToPlayer(player, "~r~Вами был выбран неверный объект изъятия!");
                            return;
                        }

                        var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (what == "weapon")
                        {
                            var targetWeapons = API.getPlayerWeapons(target);

                            foreach (var weapon in targetWeapons)
                            {
                                var targetAmmo = API.getPlayerWeaponAmmo(target, weapon);
                                API.givePlayerWeapon(player, weapon, targetAmmo, false, true);
                            }
                            API.removeAllPlayerWeapons(target);
                            API.sendNotificationToPlayer(player, "~g~Вы изъяли " + targetWeapons.Length + " ед. оружия!");
                        }
                        if (what == "narco")
                        {
                            character.Narco += targetCharacter.Narco;
                            API.sendNotificationToPlayer(player, "~g~Вы изъяли " + targetCharacter.Narco + " г. наркотиков!");
                            targetCharacter.Narco = 0;
                            ContextFactory.Instance.SaveChanges();
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            // EMERGENCY    (TRY OK):
            if (eventName == "emergency_add_to_group")
            {
                try
                {
                    var callBack = (int) args[0];
                    if (character == null) return;

                    // Принятие в медики
                    if (callBack == 1)
                    {
                        try
                        {
                            var userId = (int) args[1];

                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                                return;
                            }

                            var target = API.shared.getAllPlayers()
                                .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                            if (target == null) return;
                            if (player.position.DistanceTo(target.position) > 3.0F)
                            {
                                API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от пользователя.");
                                return;
                            }

                            var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 200);
                            var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
                            var groupExtraType =
                                (GroupExtraType) Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());

                            targetCharacter.ActiveGroupID = 201;
                            targetCharacter.GroupType = (int) groupType;
                            targetCharacter.JobId = 0;
                            targetCharacter.ActiveClothes =
                                ClothesManager.SetFractionClothes(target, 201, targetCharacter);
                            ContextFactory.Instance.SaveChanges();

                            target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " принял вас в: " +
                                                   EntityManager.GetDisplayName(groupType) + "\nНа звание: " +
                                                   EntityManager.GetDisplayName(groupExtraType));
                            API.sendChatMessageToPlayer(player,
                                "~g~[УСПЕШНО]: ~w~Вы перевели пользователя: " + targetCharacter.Name +
                                "\nВо фракцию: " + EntityManager.GetDisplayName(groupType) + "\nНа должность: " +
                                EntityManager.GetDisplayName(groupExtraType));
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                    // Выгнать из фракции
                    if (callBack == 2)
                    {
                        try
                        {
                            var userId = (int) args[1];

                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                                return;
                            }

                            if (targetCharacter.GroupType == character.GroupType)
                            {
                                var target = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                if (target == null) return;
                                targetCharacter.ActiveClothes =
                                    ClothesManager.SetFractionClothes(target, 0, targetCharacter);
                                ContextFactory.Instance.SaveChanges();

                                var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 200);
                                var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());

                                target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                                       " выгнал вас из фракции: " +
                                                       EntityManager.GetDisplayName(groupType) +
                                                       "\nДля пособия по безработице - проследуйте в мэрию.");
                                API.sendChatMessageToPlayer(player,
                                    "~g~[УСПЕШНО]: ~w~Вы выгнали пользователя: " + targetCharacter.Name +
                                    "\nИз фракции: " + EntityManager.GetDisplayName(groupType));

                                var isMale = targetCharacter.Model == 1885233650;
                                var baseClothes = isMale ? 999 : 9990;
                                ClothesManager.SetPlayerSkinClothesToDb(target, baseClothes, targetCharacter, false);
                                targetCharacter.ActiveClothes = baseClothes;

                                targetCharacter.ActiveGroupID = 1;
                                targetCharacter.GroupType = 100;
                                ContextFactory.Instance.SaveChanges();
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции.\nЛибо вы пытаетесь выгнать сами себя!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                    // Поменять ранг
                    if (callBack == 3)
                    {
                        try
                        {
                            var userId = (int) args[1];
                            var rangId = (int) args[2];

                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                                return;
                            }
                            if (rangId >= 7 && rangId <= 10 && character.ActiveGroupID == 211)
                            {
                                var target = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                if (target == null) return;

                                if (player.position.DistanceTo(target.position) > 3.0F)
                                {
                                    API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от пользователя.");
                                    return;
                                }
                                targetCharacter.ActiveGroupID = character.GroupType * 100 + rangId;
                                ContextFactory.Instance.SaveChanges();

                                var getGroup =
                                    ContextFactory.Instance.Group.FirstOrDefault(
                                        x => x.Id == targetCharacter.ActiveGroupID);
                                var groupExtraType =
                                    (GroupExtraType) Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());
                                target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " присвоил вам ранг: " +
                                                       EntityManager.GetDisplayName(groupExtraType));
                                API.sendChatMessageToPlayer(player,
                                    "~g~[УСПЕШНО]: ~w~Вы присвоили пользователю: " + targetCharacter.Name + "\nРанг: " +
                                    EntityManager.GetDisplayName(groupExtraType));
                            }
                            if (rangId >= 1 && rangId <= 6)
                            {
                                var target = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                if (target == null) return;

                                if (player.position.DistanceTo(target.position) > 3.0F)
                                {
                                    API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от пользователя.");
                                    return;
                                }
                                targetCharacter.ActiveGroupID = character.GroupType * 100 + rangId;
                                ContextFactory.Instance.SaveChanges();

                                var getGroup =
                                    ContextFactory.Instance.Group.FirstOrDefault(
                                        x => x.Id == targetCharacter.ActiveGroupID);
                                var groupExtraType =
                                    (GroupExtraType) Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());
                                target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " присвоил вам ранг: " +
                                                       EntityManager.GetDisplayName(groupExtraType));
                                API.sendChatMessageToPlayer(player,
                                    "~g~[УСПЕШНО]: ~w~Вы присвоили пользователю: " + targetCharacter.Name + "\nРанг: " +
                                    EntityManager.GetDisplayName(groupExtraType));
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции\nИли вам недопустимо присвоение данного звания!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "emergencys_menu")
            {
                try
                {
                    if (character == null) return;
                    var trigger = (int) args[0];

                    // HEAL:
                    if (trigger == 1)
                    {
                        VehicleController currentVehicleController;
                        if (player.isInVehicle) currentVehicleController = EntityManager.GetVehicle(player.vehicle);
                        else
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы не машине!");
                            return;
                        }
                        if (currentVehicleController.VehicleData.Model == AutoModels.EmergencyCar)
                        {
                            var targetUserId = (int) args[1];
                            var initCharacter = character;
                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == targetUserId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player, "~r~Вами был введен неверный ID!");
                                return;
                            }

                            var target = API.shared.getAllPlayers()
                                .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                            if (target == null) return;
                            VehicleController targetVehicleController;
                            targetVehicleController = EntityManager.GetVehicle(target.vehicle);

                            if (targetVehicleController != null && targetVehicleController.VehicleData.Id ==
                                currentVehicleController.VehicleData.Id)
                            {
                                var healPoints = (int) args[2];
                                var emergencyCost = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 200);

                                if (targetCharacter.Cash - emergencyCost.MoneyBank * healPoints < 0)
                                {
                                    API.sendNotificationToPlayer(target, "~r~У вас недостаточно средств для лечения!");
                                    return;
                                }

                                targetCharacter.Cash -= emergencyCost.MoneyBank * healPoints;
                                initCharacter.Cash += emergencyCost.MoneyBank * healPoints;
                                API.setPlayerHealth(target, API.getPlayerHealth(target) + healPoints);
                                ContextFactory.Instance.SaveChanges();
                                API.shared.triggerClientEvent(player, "update_money_display", initCharacter.Cash);
                                API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                                API.sendNotificationToPlayer(player,
                                    "~g~Вы вылечили " + healPoints + " здоровья за " + emergencyCost.MoneyBank + "$");
                                API.sendNotificationToPlayer(target,
                                    "~g~Вам вылечили " + healPoints + " здоровья за " + emergencyCost.MoneyBank + "$");
                            }
                            else
                            {
                                API.sendNotificationToPlayer(player, "~r~Больной не в машине!");
                                API.sendNotificationToPlayer(target, "~r~Вы не в машине скорой помощи!");
                                return;
                            }
                        }
                        else
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы не в рабочей машине!");
                            return;
                        }
                    }
                    // SET COST FOR HEAL 1 XP:
                    if (trigger == 2)
                    {
                        var emergencyCost = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 200);
                        var cost = (int) args[1];

                        emergencyCost.MoneyBank = cost;
                        ContextFactory.Instance.SaveChanges();
                    }
                    // FREE!
                    if (trigger == 3)
                    {
                        var targetUserId = (int) args[1];
                        var initCharacter = character;
                        var targetCharacter =
                            ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == targetUserId);
                        if (targetCharacter == null)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вами был введен неверный ID!");
                            return;
                        }
                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        API.stopPlayerAnimation(target); // Ломка

                        if (DateTime.Now.Subtract(targetCharacter.NarcoHealDate).Hours > 1)
                        {
                            var emergencyCost = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 200);

                            if (targetCharacter.Cash - emergencyCost.MoneyBank < 0)
                            {
                                API.sendNotificationToPlayer(target, "~r~У вас недостаточно средств для лечения!");
                                return;
                            }

                            targetCharacter.Cash -= emergencyCost.MoneyBank;
                            initCharacter.Cash += emergencyCost.MoneyBank;
                            targetCharacter.NarcoHealDate = DateTime.Now;
                            targetCharacter.NarcoHealQty++;
                            ContextFactory.Instance.SaveChanges();

                            if (targetCharacter.NarcoHealQty >= 50)
                            {
                                targetCharacter.NarcoHealQty = 0;
                                targetCharacter.NarcoDep = 2000;
                                API.sendNotificationToPlayer(player, "~g~Вы провели все сеансы лечения.");
                                API.sendNotificationToPlayer(target,
                                    "~g~Вам провели все сеансы лечения. Ваша зависимость 2000.");
                                ContextFactory.Instance.SaveChanges();
                                return;
                            }

                            API.shared.triggerClientEvent(player, "update_money_display", initCharacter.Cash);
                            API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                            API.sendNotificationToPlayer(player,
                                "~g~Вы провели " + targetCharacter.NarcoHealQty + " сеанс за " +
                                emergencyCost.MoneyBank + "$. Осталось: " + (50 - targetCharacter.NarcoHealQty) +
                                " сеансов.");
                            API.sendNotificationToPlayer(target,
                                "~g~Вам провели " + targetCharacter.NarcoHealQty + " сеанс лечения за " +
                                emergencyCost.MoneyBank + "$. Осталось: " + (50 - targetCharacter.NarcoHealQty) +
                                " сеансов.");
                        }
                        else
                        {
                            API.sendNotificationToPlayer(player,
                                "~r~Время с прошлого сеанса еще не прошло! Осталось: " +
                                DateTime.Now.Subtract(targetCharacter.NarcoHealDate).Minutes + " мин.");
                            API.sendNotificationToPlayer(target,
                                "~r~Время с прошлого сеанса еще не прошло! Осталось: " +
                                DateTime.Now.Subtract(targetCharacter.NarcoHealDate).Minutes + " мин.");
                            return;
                        }
                    }
                    // CHANGE CLOTHES:
                    if (trigger == 4)
                    {
                        var clothDo = (string) args[2];
                        var isMale = character.Model == 1885233650;
                        var type = 0;

                        if (clothDo == "Cloth_up" &&
                            CharacterController.IsCharacterInEmergency(character))
                        {
                            switch ((int)args[3])
                            {
                                case 1: type = isMale ? 20 : 200; break;
                                case 2: type = isMale ? 21 : 210; break;
                            }
                        }
                        if (clothDo == "Cloth_down") type = 0;

                        if (CharacterController.IsCharacterInEmergency(character))
                        {
                            ClothesManager.SetPlayerSkinClothes(player, type, characterController.Character, false);
                            characterController.Character.ActiveClothes = type;
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            // MERIA MENU   (TRY OK):
            if (eventName == "meria_add_to_group")
            {
                try
                {
                    var callBack = (int) args[0];
                    if (character == null) return;

                    // Принятие в Meria
                    if (callBack == 1)
                    {
                        try
                        {
                            var userId = (int) args[1];

                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                                return;
                            }

                            var target = API.shared.getAllPlayers()
                                .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                            if (target == null) return;
                            if (player.position.DistanceTo(target.position) < 3.0F)
                            {
                                API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от пользователя.");
                                return;
                            }

                            var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 1100);
                            var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
                            var groupExtraType =
                                (GroupExtraType) Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());

                            targetCharacter.ActiveGroupID = 1101;
                            targetCharacter.GroupType = (int) groupType;
                            targetCharacter.JobId = 0;
                            targetCharacter.ActiveClothes =
                                ClothesManager.SetFractionClothes(target, 1101, targetCharacter);
                            ContextFactory.Instance.SaveChanges();

                            target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " принял вас в: " +
                                                   EntityManager.GetDisplayName(groupType) + "\nНа звание: " +
                                                   EntityManager.GetDisplayName(groupExtraType));
                            API.sendChatMessageToPlayer(player,
                                "~g~[УСПЕШНО]: ~w~Вы перевели пользователя: " + targetCharacter.Name +
                                "\nВо фракцию: " + EntityManager.GetDisplayName(groupType) + "\nНа должность: " +
                                EntityManager.GetDisplayName(groupExtraType));
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                    // Выгнать из фракции
                    if (callBack == 2)
                    {
                        try
                        {
                            var userId = (int) args[1];

                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                                return;
                            }

                            if (targetCharacter.GroupType == character.GroupType)
                            {
                                var target = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                if (target == null) return;
                                targetCharacter.ActiveClothes =
                                    ClothesManager.SetFractionClothes(target, 0, targetCharacter);
                                ContextFactory.Instance.SaveChanges();

                                var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 1100);
                                var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());

                                target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                                       " выгнал вас из фракции: " +
                                                       EntityManager.GetDisplayName(groupType) +
                                                       "\nДля пособия по безработице - проследуйте в мэрию.");
                                API.sendChatMessageToPlayer(player,
                                    "~g~[УСПЕШНО]: ~w~Вы выгнали пользователя: " + targetCharacter.Name +
                                    "\nИз фракции: " + EntityManager.GetDisplayName(groupType));

                                var isMale = targetCharacter.Model == 1885233650;
                                var baseClothes = isMale ? 999 : 9990;
                                ClothesManager.SetPlayerSkinClothesToDb(target, baseClothes, targetCharacter, false);
                                targetCharacter.ActiveClothes = baseClothes;

                                targetCharacter.ActiveGroupID = 1;
                                targetCharacter.GroupType = 100;
                                ContextFactory.Instance.SaveChanges();
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции.\nЛибо вы пытаетесь выгнать сами себя!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                    // Поменять ранг
                    if (callBack == 3)
                    {
                        try
                        {
                            var userId = (int) args[1];
                            var rangId = (int) args[2];

                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                                return;
                            }
                            if (character.ActiveGroupID == 1106 || character.ActiveGroupID == 1105)
                            {
                                if (rangId >= 1 && rangId <= 6)
                                {
                                    var target = API.shared.getAllPlayers()
                                        .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                    if (target == null) return;

                                    if (player.position.DistanceTo(target.position) < 3.0F)
                                    {
                                        API.sendNotificationToPlayer(player,
                                            "~y~Вы находитесь далеко от пользователя.");
                                        return;
                                    }
                                    targetCharacter.ActiveGroupID = character.GroupType * 100 + rangId;
                                    ContextFactory.Instance.SaveChanges();

                                    var getGroup =
                                        ContextFactory.Instance.Group.FirstOrDefault(
                                            x => x.Id == targetCharacter.ActiveGroupID);
                                    var groupExtraType = (GroupExtraType) Enum.Parse(typeof(GroupExtraType),
                                        getGroup.ExtraType.ToString());
                                    target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                                           " присвоил вам ранг: " +
                                                           EntityManager.GetDisplayName(groupExtraType));
                                    API.sendChatMessageToPlayer(player,
                                        "~g~[УСПЕШНО]: ~w~Вы присвоили пользователю: " + targetCharacter.Name +
                                        "\nРанг: " + EntityManager.GetDisplayName(groupExtraType));
                                }
                                else
                                {
                                    API.sendChatMessageToPlayer(player,
                                        "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции\nИли вам недопустимо присвоение данного звания!");
                                    return;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "meria_menu")
            {
                try
                {
                    if (character == null) return;
                    var trigger = (int) args[0];

                    // SET PIT TAX:
                    if (trigger == 1)
                    {
                        var taxesTable = ContextFactory.Instance.Taxes.FirstOrDefault(x => x.Id == 1);
                        if (taxesTable == null) return;

                        var taxPercent = (int) args[2];

                        var typeOfTax = (string) args[1];
                        switch (typeOfTax)
                        {
                            case "pit":
                                taxesTable.PersonalIncomeTax = taxPercent;
                                API.sendChatMessageToAll("~r~Мэр города установил НДФЛ: " + taxPercent + "%");
                                break;
                            case "houseA":
                                taxesTable.HouseA = taxPercent;
                                API.sendChatMessageToAll("~r~Мэр города установил налог на жилье класса А: " +
                                                         taxPercent + "%");
                                break;
                            case "houseB":
                                taxesTable.HouseB = taxPercent;
                                API.sendChatMessageToAll("~r~Мэр города установил налог на жилье класса B: " +
                                                         taxPercent + "%");
                                break;
                            case "houseC":
                                taxesTable.HouseC = taxPercent;
                                API.sendChatMessageToAll("~r~Мэр города установил налог на жилье класса C: " +
                                                         taxPercent + "%");
                                break;
                            case "houseD":
                                taxesTable.HouseD = taxPercent;
                                API.sendChatMessageToAll("~r~Мэр города установил налог на жилье класса D: " +
                                                         taxPercent + "%");
                                break;
                        }
                    }
                    // GOS_RADIO:
                    if (trigger == 2)
                    {
                        var text = Convert.ToString(args[1]);
                        API.shared.sendChatMessageToAll("~b~[МЭРИЯ]:~s~ " + text);
                    }
                    // CHANGE CLOTHES:
                    if (trigger == 4)
                    {
                        var clothDo = (string)args[2];
                        var isMale = character.Model == 1885233650;
                        var type = 0;

                        if (clothDo == "Cloth_up" &&
                            CharacterController.IsCharacterInMeria(character))
                        {
                            switch ((int)args[3])
                            {
                                case 1: type = isMale ? 110 : 1100; break;
                                case 2: type = isMale ? 111 : 111011; break;
                                case 3: type = isMale ? 112 : 1120; break;
                                case 4: type = isMale ? 113 : 1130; break;
                                case 5: type = isMale ? 114 : 1140; break;
                            }
                        }
                        if (clothDo == "Cloth_down")
                        {
                            type = 0;
                            if (type == 111 || type == 111011 || type == 112 || type == 1120)
                            {
                                API.shared.removePlayerWeapon(player, WeaponHash.Nightstick);
                                API.shared.removePlayerWeapon(player, WeaponHash.StunGun);
                            }
                        }

                        if (CharacterController.IsCharacterInMeria(character))
                        {
                            ClothesManager.SetPlayerSkinClothes(player, type, characterController.Character, false);
                            characterController.Character.ActiveClothes = type;
                            if (type == 111 || type == 111011 || type == 112 || type == 1120)
                            {
                                API.shared.givePlayerWeapon(player, WeaponHash.Nightstick, 1, true, true);
                                API.shared.givePlayerWeapon(player, WeaponHash.StunGun, 1, true, true);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }

            // ARMY MENU    (TRY OK):
            if (eventName == "armys_menu")
            {
                try
                {
                    if (character == null) return;
                    var trigger = (int) args[0];

                    // ARMY 2, loading in his stock
                    if (trigger == 1)
                    {
                        var propertyName = (string) args[1];
                        var propertyData = ContextFactory.Instance.Property.FirstOrDefault(x => x.Name == propertyName);
                        if (propertyData == null) return;
                        VehicleController vehicleControllerLoad = null;
                        var propertyDestName = (string) args[2];
                        var propertyDestData =
                            ContextFactory.Instance.Property.FirstOrDefault(x => x.Name == propertyDestName);
                        if (propertyDestData == null) return;

                        if (player.isInVehicle)
                            vehicleControllerLoad = EntityManager.GetVehicle(player.vehicle);
                        else API.sendNotificationToPlayer(player, "Вы не в транспорте!");
                        if (vehicleControllerLoad == null) return;

                        if (propertyData.Stock - 20000 > 0)
                        {
                            if (vehicleController.VehicleData.Material == 0)
                            {
                                propertyData.Stock -= 20000;
                                vehicleControllerLoad.VehicleData.Material += 20000;
                                API.sendNotificationToPlayer(player,
                                    "~g~Вы загрузили " + vehicleControllerLoad.VehicleData.Material +
                                    " материалов в машину.");
                                API.triggerClientEvent(player, "markonmap", propertyDestData.ExtPosX,
                                    propertyDestData.ExtPosY);
                            }
                            else API.sendNotificationToPlayer(player, "~r~Ваша машина заполнена!");
                        }
                        else API.sendNotificationToPlayer(player, "~r~На вашем складе недостаточно материалов!");
                        ContextFactory.Instance.SaveChanges();
                    }
                    // Unload to Police/FBI/ARMY 1/ARMY 2
                    if (trigger == 2)
                    {
                        var propertyName = (string) args[1];
                        var propertyData = ContextFactory.Instance.Property.FirstOrDefault(x => x.Name == propertyName);
                        if (propertyData == null) return;
                        VehicleController vehicleControllerLoad = null;

                        if (player.isInVehicle)
                            vehicleControllerLoad = EntityManager.GetVehicle(player.vehicle);
                        else API.sendNotificationToPlayer(player, "Вы не в транспорте!");
                        if (vehicleControllerLoad == null) return;

                        if (vehicleControllerLoad.VehicleData.Material != 0)
                        {
                            if (propertyData.Stock + vehicleControllerLoad.VehicleData.Material >
                                Stocks.GetStockCapacity(propertyName))
                            {
                                var difStocks = Stocks.GetStockCapacity(propertyName) - propertyData.Stock;
                                propertyData.Stock += difStocks;
                                API.sendNotificationToPlayer(player,
                                    "~g~Вы разгрузили " + difStocks + " материалов. На склад: " + propertyData.Name);
                            }
                            else
                            {
                                propertyData.Stock += vehicleControllerLoad.VehicleData.Material;
                                API.sendNotificationToPlayer(player,
                                    "~g~Вы разгрузили " + vehicleControllerLoad.VehicleData.Material.ToString() +
                                    " материалов. На склад: " + propertyData.Name);
                            }
                            vehicleControllerLoad.VehicleData.Material = 0;
                        }
                        else API.sendNotificationToPlayer(player, "~r~В вашей машине нет материалов!");
                        ContextFactory.Instance.SaveChanges();
                    }
                    // ARMY 1, loading from carrier:
                    if (trigger == 3)
                    {
                        VehicleController vehicleControllerLoad = null;
                        var propertyDestData = ContextFactory.Instance.Property.First(x => x.Name == "Army2_stock");

                        if (player.isInVehicle)
                            vehicleControllerLoad = EntityManager.GetVehicle(player.vehicle);
                        else API.sendNotificationToPlayer(player, "Вы не в транспорте!");
                        if (vehicleControllerLoad == null) return;

                        if (vehicleController.VehicleData.Material < 100000)
                        {
                            var factMaterial = 100000 - vehicleControllerLoad.VehicleData.Material;
                            API.sendNotificationToPlayer(player,
                                "~g~Вы загрузили " + factMaterial + " материалов в машину.");
                            vehicleControllerLoad.VehicleData.Material = 100000;
                            API.triggerClientEvent(player, "markonmap", propertyDestData.ExtPosX,
                                propertyDestData.ExtPosY);
                        }
                        else API.sendNotificationToPlayer(player, "~r~Ваша машина заполнена!");
                        ContextFactory.Instance.SaveChanges();
                    }
                    // Change clothes for officers
                    if (trigger == 4)
                    {
                        var clothDo = (string) args[2];
                        var isMale = character.Model == 1885233650;
                        var type = 0;

                        if (clothDo == "Cloth_up" &&
                            CharacterController.IsCharacterArmyInAllOfficers(character)) type = isMale ? 202 : 2020;
                        if (clothDo == "Cloth_up" &&
                            CharacterController.IsCharacterArmyGeneral(character))
                        {
                            type = isMale ? 203 : 2030;
                            API.setPlayerArmor(player, 120);
                        }
                        if (clothDo == "Cloth_down") type = 0;

                        if (CharacterController.IsCharacterArmyInAllOfficers(character) ||
                            CharacterController.IsCharacterArmyGeneral(character))
                        {
                            ClothesManager.SetPlayerSkinClothes(player, type, characterController.Character, false);
                            characterController.Character.ActiveClothes = type;
                            API.setPlayerArmor(player, 0);
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "army_add_to_group")
            {
                try
                {
                    var callBack = (int) args[2];
                    if (character == null) return;

                    // Принятие в армию
                    if (callBack == 1)
                    {
                        try
                        {
                            var userId = (int) args[0];
                            var armyId = (int) args[1];
                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;

                            var target = API.shared.getAllPlayers()
                                .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                            if (target == null) return;
                            if (player.position.DistanceTo(target.position) > 3.0F)
                            {
                                API.sendNotificationToPlayer(player,
                                    "~y~[ПРЕД]: ~w~Вы находитесь далеко от пользователя!");
                                return;
                            }

                            var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == armyId);
                            var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
                            var groupExtraType =
                                (GroupExtraType) Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());

                            targetCharacter.ActiveGroupID = armyId;
                            targetCharacter.GroupType = (int) groupType;
                            targetCharacter.JobId = 0;
                            targetCharacter.ActiveClothes =
                                ClothesManager.SetFractionClothes(target, armyId, targetCharacter);
                            ContextFactory.Instance.SaveChanges();

                            target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " принял вас в армию: " +
                                                   EntityManager.GetDisplayName(groupType) + "\nНа звание: " +
                                                   EntityManager.GetDisplayName(groupExtraType));
                            API.sendChatMessageToPlayer(player,
                                "~g~[УСПЕШНО]: ~w~Вы перевели пользователя: " + targetCharacter.Name +
                                "\nВо фракцию: " + EntityManager.GetDisplayName(groupType) + "\nНа должность: " +
                                EntityManager.GetDisplayName(groupExtraType));
                        }
                        catch (Exception)
                        {
                            API.sendChatMessageToPlayer(player,
                                "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                            return;
                        }
                    }
                    // Выгнать из армии
                    if (callBack == 2)
                    {
                        try
                        {
                            var userId = (int) args[0];
                            var groupId = (int) args[1];

                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                                return;
                            }

                            if (targetCharacter.GroupType == character.GroupType &&
                                CharacterController.IsCharacterArmyHighOfficer(character) ||
                                CharacterController.IsCharacterArmyGeneral(character))
                            {
                                var target = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                if (target == null) return;
                                targetCharacter.ActiveClothes =
                                    ClothesManager.SetFractionClothes(target, 0, targetCharacter);
                                ContextFactory.Instance.SaveChanges();

                                var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == groupId);
                                var groupType = (GroupType) Enum.Parse(typeof(GroupType), getGroup.Type.ToString());

                                target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                                       " выгнал вас из фракции: " +
                                                       EntityManager.GetDisplayName(groupType) +
                                                       "\nДля пособия по безработице - проследуйте в мэрию.");
                                API.sendChatMessageToPlayer(player,
                                    "~g~[УСПЕШНО]: ~w~Вы выгнали пользователя: " + targetCharacter.Name +
                                    "\nИз фракции: " + EntityManager.GetDisplayName(groupType));

                                var isMale = targetCharacter.Model == 1885233650;
                                var baseClothes = isMale ? 999 : 9990;
                                ClothesManager.SetPlayerSkinClothesToDb(target, baseClothes, targetCharacter, false);
                                targetCharacter.ActiveClothes = baseClothes;

                                targetCharacter.ActiveGroupID = 1;
                                targetCharacter.GroupType = 100;
                                ContextFactory.Instance.SaveChanges();
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции.\nЛибо вы пытаетесь выгнать сами себя!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendChatMessageToPlayer(player,
                                "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                            return;
                        }
                    }
                    // Поменять звание
                    if (callBack == 3)
                    {
                        try
                        {
                            var userId = (int) args[0];
                            var rangId = (int) args[1];
                            var correctGroupId = 0;

                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                                return;
                            }
                            var isMale = targetCharacter.Model == 1885233650;

                            if (rangId >= 1 && rangId <= 11 &&
                                CharacterController.IsCharacterArmyHighOfficer(character))
                            {
                                targetCharacter.ActiveGroupID = character.GroupType * 100 + rangId;
                                ContextFactory.Instance.SaveChanges();

                                var target = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                if (target == null) return;

                                if (CharacterController.IsCharacterArmyInAllOfficers(targetCharacter))
                                {
                                    var baseClothes = isMale ? 999 : 9990;
                                    var clothesId = isMale ? 202 : 2020;
                                    ClothesManager.SetPlayerSkinClothesToDb(target, baseClothes, targetCharacter,
                                        false);
                                    ClothesManager.SetPlayerSkinClothes(target, clothesId, targetCharacter, false);
                                    targetCharacter.ActiveClothes = clothesId;
                                    ContextFactory.Instance.SaveChanges();
                                }

                                var getGroup =
                                    ContextFactory.Instance.Group.FirstOrDefault(
                                        x => x.Id == targetCharacter.ActiveGroupID);
                                var groupExtraType =
                                    (GroupExtraType) Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());

                                target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                                       " присвоил вам звание: " +
                                                       EntityManager.GetDisplayName(groupExtraType));
                                API.sendChatMessageToPlayer(player,
                                    "~g~[УСПЕШНО]: ~w~Вы присвоили пользователю: " + targetCharacter.Name +
                                    "\nВоинское звание: " + EntityManager.GetDisplayName(groupExtraType));
                            }
                            else if (rangId >= 1 && rangId <= 14 &&
                                     CharacterController.IsCharacterArmyGeneral(character))
                            {
                                targetCharacter.ActiveGroupID = correctGroupId + rangId;
                                ContextFactory.Instance.SaveChanges();

                                var target = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                                if (target == null) return;

                                if (CharacterController.IsCharacterArmyInAllOfficers(targetCharacter))
                                {
                                    var baseClothes = isMale ? 999 : 9990;
                                    var clothesId = isMale ? 202 : 2020;
                                    ClothesManager.SetPlayerSkinClothesToDb(target, baseClothes, targetCharacter,
                                        false);
                                    ClothesManager.SetPlayerSkinClothes(target, clothesId, targetCharacter, false);
                                    targetCharacter.ActiveClothes = clothesId;
                                    ContextFactory.Instance.SaveChanges();
                                }

                                var getGroup =
                                    ContextFactory.Instance.Group.FirstOrDefault(
                                        x => x.Id == targetCharacter.ActiveGroupID);
                                var groupExtraType =
                                    (GroupExtraType) Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());

                                target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                                       " присвоил вам звание: " +
                                                       EntityManager.GetDisplayName(groupExtraType));
                                API.sendChatMessageToPlayer(player,
                                    "~g~[УСПЕШНО]: ~w~Вы присвоили пользователю: " + targetCharacter.Name +
                                    "\nВоинское звание: " + EntityManager.GetDisplayName(groupExtraType));
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции\nИли вам недопустимо присвоение данного звания!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player,
                                "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }

            // FBI          (TRY OK):
            if (eventName == "fbi_add_to_group")
            {
                try
                {
                    var callBack = (int) args[0];
                    if (character == null) return;

                    // Принятие в FBI:
                    if (callBack == 1)
                    {
                        try
                        {
                            var userId = (int) args[1];

                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;
                            AddUserToFraction(player, targetCharacter, character, 300);
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                    // Выгнать из фракции
                    if (callBack == 2)
                    {
                        try
                        {
                            var userId = (int) args[1];

                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;

                            if (targetCharacter.GroupType == character.GroupType)
                            {
                                DeleteUserFromFraction(player, targetCharacter,character, 300);
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции.\nЛибо вы пытаетесь выгнать сами себя!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                    // Поменять ранг
                    if (callBack == 3)
                    {
                        try
                        {
                            var userId = (int) args[1];
                            var rangId = (int) args[2];

                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                                return;
                            }
                            if (character.ActiveGroupID == 310 && rangId >= 1 && rangId <= 9)
                                ChangeUserRang(player, targetCharacter, character, rangId);
                            else if (character.ActiveGroupID == 308 && rangId >= 1 && rangId <= 7)
                                ChangeUserRang(player, targetCharacter, character, rangId);
                            else
                            {
                                API.sendChatMessageToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции\nИли вам недопустимо присвоение данного звания!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "fbis_menu")
            {
                try
                {
                    if (character == null) return;
                    var trigger = (int) args[0];

                    // KEYS TO PLAYER:
                    if (trigger == 1)
                    {
                        var targetUserId = (int) args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;
                        if (player.getData("PRISONKEYS") == false)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас нет ключей от камер!");
                            return;
                        }

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        player.setData("PRISONKEYS", false);
                        target.setData("PRISONKEYS", true);

                        API.sendNotificationToPlayer(player, "~g~Вы отдали ключи от камер: " + targetCharacter.Name);
                        API.sendNotificationToPlayer(target, "~g~Вам передали ключи от камер.");
                    }
                    // SEARCH PLAYER:
                    if (trigger == 2)
                    {
                        var targetUserId = (int) args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        API.sendNotificationToPlayer(player, "~g~Координаты установлены!");
                        API.triggerClientEvent(player, "markonmap", target.position.X, target.position.Y);
                    }
                    // OPEN/CLOSE BUSINESS:
                    if (trigger == 3)
                    {
                        var businessId = (int) args[1];
                        var selectedBusiness = ContextFactory.Instance.Job.FirstOrDefault(x => x.Id == businessId);
                        if (selectedBusiness == null)
                        {
                            API.shared.sendNotificationToPlayer(player, "~r~Вами был введен неверный ID!");
                            return;
                        }

                        var openClose = (int) args[2];
                        if (openClose == 1)
                        {
                            // TODO: MECHANICS!!!
                        }
                        else
                        {
                            // TODO: MECHANICS!!!
                        }
                    }
                    // CHANGE CLOTHES:
                    if (trigger == 4)
                    {
                        var clothDo = (string) args[2];
                        var isMale = character.Model == 1885233650;
                        var type = 0;

                        if (clothDo == "Cloth_up" &&
                            CharacterController.IsCharacterInFbi(character))
                        {
                            if (character.ActiveGroupID == 310) type = isMale ? 31 : 310;
                            else type = isMale ? 30 : 300;
                        }
                        if (clothDo == "Cloth_down") type = 0;

                        if (CharacterController.IsCharacterInFbi(character))
                        {
                            ClothesManager.SetPlayerSkinClothes(player, type, characterController.Character, false);
                            characterController.Character.ActiveClothes = type;
                        }
                    }
                    // TAKE STEAL:
                    if (trigger == 5)
                    {
                        var targetUserId = (int) args[1];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        target.invincible = !target.invincible;
                        API.sendNotificationToPlayer(target,
                            target.invincible ? "Вы замаскированы!" : "Вас незамаскированы!");
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }

            // NEWS         (TRY OK):
            if (eventName == "news_add_to_group")
            {
                try
                {
                    var callBack = (int)args[0];
                    if (character == null) return;

                    // Принятие в NEWS:
                    if (callBack == 1)
                    {
                        try
                        {
                            var userId = (int)args[1];

                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;

                            AddUserToFraction(player, targetCharacter, character, 700);
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                    // Выгнать из фракции
                    if (callBack == 2)
                    {
                        try
                        {
                            var userId = (int)args[1];

                            Character targetCharacter;
                            if (!MenuMethods.CheckTargetId(player, out targetCharacter, userId)) return;

                            if (targetCharacter.GroupType == character.GroupType)
                            {
                                DeleteUserFromFraction(player, targetCharacter, character, 700);
                            }
                            else
                            {
                                API.sendChatMessageToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции.\nЛибо вы пытаетесь выгнать сами себя!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                    // Поменять ранг
                    if (callBack == 3)
                    {
                        try
                        {
                            var userId = (int)args[1];
                            var rangId = (int)args[2];

                            var targetCharacter =
                                ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                            if (targetCharacter == null)
                            {
                                API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                                return;
                            }
                            if (character.ActiveGroupID == 710 && rangId >= 1 && rangId <= 9)
                                ChangeUserRang(player, targetCharacter, character, rangId);
                            else if (character.ActiveGroupID == 709 && rangId >= 1 && rangId <= 8)
                                ChangeUserRang(player, targetCharacter, character, rangId);
                            else if (character.ActiveGroupID == 708 && rangId >= 1 && rangId <= 7)
                                ChangeUserRang(player, targetCharacter, character, rangId);
                            else if (character.ActiveGroupID == 707 && rangId >= 1 && rangId <= 6)
                                ChangeUserRang(player, targetCharacter, character, rangId);
                            else
                            {
                                API.sendChatMessageToPlayer(player,
                                    "~r~[ОШИБКА]: ~w~Пользователь не состоит в вашей фракции\nИли вам недопустимо присвоение данного звания!");
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный пользовательский ID!");
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }

            // YES_NO SELL  (TRYOK):
            if (eventName == "yes_no_menu")
            {
                try
                {
                    var type = (string) args[0];
                    var targetUserId = (int) args[1]; // OID of buyer
                    var initUserId = (int) args[4]; // OID of seller

                    if (type == "cloth")
                    {
                        var cost = (int) args[3];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 3.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вам предлагают купить военную форму за " + cost + "$", // 0
                            type, // 1
                            0, // 2
                            cost, // 3 
                            initUserId, // 4  
                            targetUserId); // 5 
                    }
                    if (type == "drugs")
                    {
                        var gramms = (int) args[2];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 3.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вам предлагают " + gramms + " г. наркотиков за" + Prices.DrugSellPrice * gramms + "$", // 0
                            type, // 1
                            gramms, // 2
                            Prices.DrugSellPrice * gramms, // 3 
                            initUserId, // 4  
                            targetUserId); // 5 
                    }
                    if (type == "weapon")
                    {
                        var weapon = (string) args[2];
                        var cost = (int) args[3];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 3.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вам предлагают " + weapon + " за " + cost + "$", // 0
                            type, // 1
                            weapon, // 2
                            cost, // 3 
                            initUserId, // 4  
                            targetUserId); // 5 
                    }
                    if (type == "gang_sector")
                    {

                        var sector = (string) args[2];
                        var gangNum = (int) args[3]; // Gang Number for selling
                        var cost = (int) args[5];
                        var sellerGangType = (int) args[6]; // Seller Gang ID

                        var selectedSector = sector.Split('_');
                        var selectedSectorData = GroupController.GetGangSectorData(
                            Convert.ToInt32(selectedSector[0]), Convert.ToInt32(selectedSector[1]));

                        if (selectedSectorData == sellerGangType * 10 || selectedSectorData != sellerGangType)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы не можете продать данный сектор!");
                            return;
                        }

                        var targetGangBoss = gangNum * 100 + 10;
                        var targetCharacter =
                            ContextFactory.Instance.Character.FirstOrDefault(x => x.ActiveGroupID == targetGangBoss);
                        if (targetCharacter == null)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный номер банды!");
                            return;
                        }
                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вам предлагают сектор " + selectedSector[0] + " " + selectedSector[1] + " за " + cost +
                            "$", // 0
                            type, // 1
                            selectedSector, // 2
                            cost, // 3 
                            initUserId, // 4  
                            targetGangBoss); // 5 
                    }
                    if (type == "gang_material_mafia")
                    {
                        var material = (int) args[2];
                        var mafiaId = (int) args[3]; // Mafia Id for selling
                        var cost = (int) args[5];

                        var gangStockName = GroupController.GetGroupStockName(character);
                        var gangStockProperty = ContextFactory.Instance.Property
                            .First(x => x.Name == gangStockName);
                        if (gangStockProperty.Stock - material < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~В банке банды недостаточно материалов!");
                            return;
                        }

                        var targetMafiaBoss = mafiaId * 100 + 10;
                        var targetCharacter =
                            ContextFactory.Instance.Character.FirstOrDefault(x => x.ActiveGroupID == targetMafiaBoss);
                        if (targetCharacter == null)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы ввели неверный номер мафии!");
                            return;
                        }
                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вам предлагают " + material + " материалов за " + cost + "$", // 0
                            type, // 1
                            material, // 2
                            cost, // 3 
                            initUserId, // 4  
                            targetMafiaBoss); // 5 
                    }
                    if (type == "mafia_debt")
                    {
                        var cost = (int) args[3];

                        var mafiaGroupProperty = character.GroupType * 100;
                        var mafiaBank = ContextFactory.Instance.Group
                            .First(x => x.Id == mafiaGroupProperty);

                        if (mafiaBank.MoneyBank - cost < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~В банке мафии нет средств для дачи в долг!");
                            return;
                        }

                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 3.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }

                        var characterRank = character.ActiveGroupID - character.GroupType * 100;
                        switch (characterRank)
                        {
                            case 1:
                                if (cost > 10000 && character.DebtLimit + cost > 10000)
                                {
                                    API.sendNotificationToPlayer(player,
                                        "~r~Вы превысили лимит! Ваш лимит долга 10.000$!");
                                    return;
                                }
                                else break;
                            case 2:
                                if (cost > 20000 && character.DebtLimit + cost > 20000)
                                {
                                    API.sendNotificationToPlayer(player,
                                        "~r~Вы превысили лимит! Ваш лимит долга 20.000$!");
                                    return;
                                }
                                else break;
                            case 3:
                                if (cost > 30000 && character.DebtLimit + cost > 30000)
                                {
                                    API.sendNotificationToPlayer(player,
                                        "~r~Вы превысили лимит! Ваш лимит долга 30.000$!");
                                    return;
                                }
                                else break;
                            case 4:
                                if (cost > 40000 && character.DebtLimit + cost > 40000)
                                {
                                    API.sendNotificationToPlayer(player,
                                        "~r~Вы превысили лимит! Ваш лимит долга 40.000$!");
                                    return;
                                }
                                else break;
                            case 5:
                                if (cost > 50000 && character.DebtLimit + cost > 50000)
                                {
                                    API.sendNotificationToPlayer(player,
                                        "~r~Вы превысили лимит! Ваш лимит долга 50.000$!");
                                    return;
                                }
                                else break;
                            case 6:
                                if (cost > 60000 && character.DebtLimit + cost > 60000)
                                {
                                    API.sendNotificationToPlayer(player,
                                        "~r~Вы превысили лимит! аш лимит долга 60.000$!");
                                    return;
                                }
                                else break;
                            case 7:
                                if (cost > 200000 && character.DebtLimit + cost > 200000)
                                {
                                    API.sendNotificationToPlayer(player,
                                        "~r~Вы превысили лимит! Ваш лимит долга 200.000$!");
                                    return;
                                }
                                else break;
                            case 8:
                                if (cost > 200000 && character.DebtLimit + cost > 200000)
                                {
                                    API.sendNotificationToPlayer(player,
                                        "~r~Вы превысили лимит! Ваш лимит долга 200.000$!");
                                    return;
                                }
                                else break;
                            case 9:
                                if (cost > 200000 && character.DebtLimit + cost > 200000)
                                {
                                    API.sendNotificationToPlayer(player,
                                        "~r~Вы превысили лимит! Ваш лимит долга 200.000$!");
                                    return;
                                }
                                else break;
                            case 10: break;
                        }

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вам предлагают в долг: " + cost + "$", // 0
                            type, // 1
                            0, // 2
                            cost, // 3 
                            initUserId, // 4  
                            targetUserId); // 5 
                    }
                    if (type == "mafia_roof")
                    {
                        var cost = (int) args[3];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 3.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вам предлагают крышу за: " + cost + "$", // 0
                            type, // 1
                            0, // 2
                            cost, // 3 
                            initUserId, // 4  
                            targetUserId); // 5 
                    }
                    if (type == "mafia_bus_roof")
                    {
                        var cost = (int) args[3];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 3.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вашему бизнесу предлагают крышу за: " + cost + "$", // 0
                            type, // 1
                            0, // 2
                            cost, // 3 
                            initUserId, // 4  
                            targetUserId); // 5 
                    }
                    if (type == "sell_fuel")
                    {
                        var qty = (string) args[2];
                        var cost = (int) args[3];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 6.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вам предлагают " + qty + " Литров за " + cost + "$", // 0
                            type, // 1
                            qty, // 2
                            cost, // 3 
                            initUserId, // 4  
                            targetUserId); // 5 
                    }
                    if (type == "repair_car")
                    {
                        var cost = (int) args[3];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 6.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вам предлагают ремонт авто за " + cost + "$", // 0
                            type, // 1
                            0, // 2 NULL
                            cost, // 3 
                            initUserId, // 4  
                            targetUserId); // 5 
                    }
                    if (type == "lawyer_prison")
                    {
                        //var gramms = (int)args[2];
                        var cost = args[3];
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        if (!targetCharacter.IsPrisoned)
                        {
                            API.sendNotificationToPlayer(player, "~r~Игрок не осужден!");
                            return;
                        }

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 5.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }

                        API.shared.triggerClientEvent(target, "yes_no_menu_client",
                            "Вам предлагают выйти из тюрьмы за" + cost + "$", // 0
                            type, // 1
                            0, // 2
                            cost, // 3 
                            initUserId, // 4  
                            targetUserId); // 5 
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "sell")
            {
                try
                {
                    var targetUserId = (int) args[1];
                    var cost = (int) args[3];
                    var initUserId = (int) args[4];

                    if (args[0].ToString() == "gang_sector")
                    {
                        if (character == null) return;

                        var sectorRow = ((List<object>) args[2]).ElementAt(0);
                        var sectorCol = ((List<object>) args[2]).ElementAt(1);

                        var initCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == initUserId);
                        var targetCharacter =
                            ContextFactory.Instance.Character.FirstOrDefault(x => x.ActiveGroupID == targetUserId);
                        if (targetCharacter == null)
                        {
                            API.sendChatMessageToPlayer(player, "~r~Вами был введен неверный ID!");
                            return;
                        }
                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (targetCharacter.Cash - cost < 0)
                        {
                            API.sendChatMessageToPlayer(player, "~r~У вас недостаточно средств для покупки!");
                            return;
                        }
                        targetCharacter.Cash -= cost;
                        initCharacter.Cash += cost;
                        GroupController.SetGangSectorData(Convert.ToInt32(sectorRow), Convert.ToInt32(sectorCol),
                            targetCharacter.GroupType);
                        API.shared.triggerClientEvent(player, "update_money_display", initCharacter.Cash);
                        API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                        ContextFactory.Instance.SaveChanges();
                        API.sendChatMessageToPlayer(player,
                            "~g~Вы успешно продали сектор" + sectorRow + " " + sectorCol + " за " + cost + "$");
                        API.sendChatMessageToPlayer(target,
                            "~g~Вы успешно купили " + sectorRow + " " + sectorCol + " за " + cost + "$");
                    }
                    if (args[0].ToString() == "drugs")
                    {
                        if (character == null) return;
                        var gramms = (int) args[2];

                        var initCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == initUserId);
                        if (initCharacter.Narco - gramms < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас недостаточно наркотиков для продажи!");
                            return;
                        }
                        var targetCharacter =
                            ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == targetUserId);
                        if (targetCharacter == null)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вами был введен неверный ID!");
                            return;
                        }

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 3.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }
                        if (targetCharacter.Cash - cost < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~У покупателя недостаточно денег для покупки!");
                            API.sendNotificationToPlayer(target, "~r~У вас недостаточно денег для покупки!");
                            return;
                        }
                        targetCharacter.Cash -= cost;
                        initCharacter.Cash += cost;
                        initCharacter.Narco -= gramms;
                        targetCharacter.Narco += gramms;
                        ContextFactory.Instance.SaveChanges();
                        API.shared.triggerClientEvent(player, "update_money_display", initCharacter.Cash);
                        API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                        API.sendNotificationToPlayer(player,
                            "~g~Вы продали ~w~" + gramms + " г. наркотиков за " + cost + "$");
                        API.sendNotificationToPlayer(target,
                            "~g~Вы купили ~w~" + gramms + " г. наркотиков за " + cost + "$");
                    }
                    if (args[0].ToString() == "weapon")
                    {
                        if (character == null) return;
                        var weaponName = (string) args[2];

                        if (weaponName == "Unarmed")
                        {
                            API.sendChatMessageToPlayer(player, "~r~Вы не можете продать данное оружие!");
                            return;
                        }

                        var initCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == initUserId);
                        var sellPlayerWeapons =
                            ContextFactory.Instance.Weapon.FirstOrDefault(x => x.CharacterId == initCharacter.Id);
                        if (sellPlayerWeapons == null)
                        {
                            API.sendChatMessageToPlayer(player, "~r~У вас нет оружия!");
                            return;
                        }
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var buyPlayerWeapons =
                            ContextFactory.Instance.Weapon.FirstOrDefault(x => x.CharacterId == targetCharacter.Id);
                        if (buyPlayerWeapons == null)
                        {
                            API.sendChatMessageToPlayer(player, "~r~Вами был введен неверный ID!");
                            return;
                        }

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 3.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }
                        if (targetCharacter.Cash - cost < 0)
                        {
                            API.sendChatMessageToPlayer(target, "~r~У вас недостаточно средств для покупки!");
                            return;
                        }
                        targetCharacter.Cash -= cost;
                        initCharacter.Cash += cost;
                        var ammoAmount = API.getPlayerWeaponAmmo(player, WeaponManager.GetWeaponHash(weaponName));

                        WeaponManager.BuySellWeapon(target, player,
                            sellPlayerWeapons, buyPlayerWeapons,
                            weaponName, ammoAmount,
                            initCharacter, targetCharacter);

                        API.shared.triggerClientEvent(player, "update_money_display", initCharacter.Cash);
                        API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                        ContextFactory.Instance.SaveChanges();
                        API.sendNotificationToPlayer(player,
                            "~g~Вы успешно продали " + weaponName + " за " + cost + "$");
                        API.sendNotificationToPlayer(target,
                            "~g~Вы успешно купили " + weaponName + " за " + cost + "$");
                    }
                    if (args[0].ToString() == "cloth")
                    {
                        if (character == null) return;

                        var initCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == initUserId);

                        if (initCharacter.ClothesTypes <= 200 && initCharacter.ClothesTypes >= 204)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас нет формы!");
                            return;
                        }
                        if (initCharacter.ClothesTypes <= 2000 && initCharacter.ClothesTypes >= 2040)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас нет формы!");
                            return;
                        }
                        var targetCharacter =
                            ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == targetUserId);
                        if (targetCharacter == null)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вами был введен неверный ID!");
                            return;
                        }

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 3.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }
                        if (targetCharacter.Cash - cost < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас недостаточно средств для покупки!");
                            return;
                        }
                        targetCharacter.Cash -= cost;
                        initCharacter.Cash += cost;
                        targetCharacter.ClothesTypes = initCharacter.ClothesTypes;
                        initCharacter.ClothesTypes = 0;
                        var isMale = initCharacter.Model == 1885233650;
                        ClothesManager.SetPlayerSkinClothesToDb(player, isMale ? 888 : 8880, initCharacter, false);
                        API.shared.triggerClientEvent(player, "update_money_display", initCharacter.Cash);
                        API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                        ContextFactory.Instance.SaveChanges();
                        API.sendChatMessageToPlayer(player, "~g~Вы успешно продали свою военную форму!");
                        API.sendChatMessageToPlayer(target, "~g~Вы успешно купили себе военную форму!");
                    }
                    if (args[0].ToString() == "gang_material_mafia")
                    {
                        if (character == null) return;

                        var material = (int) args[2];
                        var mafiaGroupProperty = character.GroupType * 100;
                        var mafiaStockName = GroupController.GetGroupStockName(character);
                        var mafiaStockProperty = ContextFactory.Instance.Property.First(x => x.Name == mafiaStockName);
                        var mafiaBank = ContextFactory.Instance.Group.First(x => x.Id == mafiaGroupProperty);

                        if (mafiaBank.MoneyBank - cost < 0)
                        {
                            API.sendChatMessageToPlayer(player, "~r~У вас недостаточно средств для покупки!");
                            return;
                        }
                        var gangGroupProperty = character.GroupType * 100;
                        var gangStockName = GroupController.GetGroupStockName(character);
                        var gangStockProperty = ContextFactory.Instance.Property.First(x => x.Name == gangStockName);
                        var gangBank = ContextFactory.Instance.Group.First(x => x.Id == gangGroupProperty);

                        mafiaBank.MoneyBank -= cost;
                        gangBank.MoneyBank += cost;
                        gangStockProperty.Stock -= material;
                        mafiaStockProperty.Stock += material;
                        ContextFactory.Instance.SaveChanges();

                        var targetCharacter =
                            ContextFactory.Instance.Character.FirstOrDefault(x => x.ActiveGroupID == targetUserId);
                        if (targetCharacter == null)
                        {
                            API.sendChatMessageToPlayer(player, "~r~Вами был введен неверный ID!");
                            return;
                        }
                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;
                        API.sendChatMessageToPlayer(player,
                            "~g~Вы успешно продали " + material + " материалов за " + cost + "$");
                        API.sendChatMessageToPlayer(target,
                            "~g~Вы успешно купили " + material + " материалов за " + cost + "$");
                    }
                    if (args[0].ToString() == "mafia_debt")
                    {
                        if (character == null) return;

                        var mafiaGroupProperty = character.GroupType * 100;
                        var mafiaBank = ContextFactory.Instance.Group
                            .First(x => x.Id == mafiaGroupProperty);

                        if (mafiaBank.MoneyBank - cost < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~В банке мафии нет средств для долга!");
                            return;
                        }
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        targetCharacter.Debt = cost * 2;
                        targetCharacter.DebtMafia = mafiaGroupProperty;
                        targetCharacter.DebtDate = DateTime.Now;
                        targetCharacter.Cash += cost;
                        character.DebtLimit += cost;
                        mafiaBank.MoneyBank -= cost;
                        ContextFactory.Instance.SaveChanges();

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;
                        API.sendChatMessageToPlayer(player, "~g~Вы успешно дали в долг " + cost + "$");
                        API.sendChatMessageToPlayer(target, "~g~Вы успешно взяли в долг " + cost + "$");
                    }
                    if (args[0].ToString() == "mafia_roof")
                    {
                        if (character == null) return;
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var mafiaGroupProperty = character.GroupType * 100;
                        var mafiaBank = ContextFactory.Instance.Group.First(x => x.Id == mafiaGroupProperty);
                        var groupType = (GroupType) Enum.Parse(typeof(GroupType), mafiaBank.Type.ToString());

                        targetCharacter.MafiaRoof = mafiaGroupProperty;
                        targetCharacter.Cash -= cost;
                        mafiaBank.MoneyBank += cost;
                        ContextFactory.Instance.SaveChanges();

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;
                        API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                        API.sendChatMessageToPlayer(player,
                            "~g~Вы успешно взяли под крышу" + targetCharacter.Name + " за " + cost + "$");
                        API.sendChatMessageToPlayer(target,
                            "~g~Вы успешно стали под крышей мафии " + EntityManager.GetDisplayName(groupType));
                    }
                    if (args[0].ToString() == "mafia_bus_roof")
                    {
                        if (character == null) return;
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var mafiaGroupProperty = character.GroupType * 100;
                        var mafiaBank = ContextFactory.Instance.Group.First(x => x.Id == mafiaGroupProperty);
                        var groupType = (GroupType) Enum.Parse(typeof(GroupType), mafiaBank.Type.ToString());

                        var currentBusiness =
                            ContextFactory.Instance.Job.FirstOrDefault(x => x.CharacterId == targetUserId);

                        currentBusiness.MafiaRoofId = mafiaGroupProperty;
                        currentBusiness.MafiaRoofMoney = cost;
                        ContextFactory.Instance.SaveChanges();

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;
                        API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                        API.sendChatMessageToPlayer(player,
                            "~g~Вы успешно взяли под крышу бизнес " + currentBusiness.Id + " за " + cost + "$");
                        API.sendChatMessageToPlayer(target,
                            "~g~Ваш бизнес " + currentBusiness.Id + " успешно стали под крышей мафии " +
                            EntityManager.GetDisplayName(groupType));
                    }
                    if (args[0].ToString() == "sell_fuel")
                    {
                        if (character == null) return;
                        var qty = (int) args[2];

                        VehicleController currentVehicleController;
                        if (player.isInVehicle) currentVehicleController = EntityManager.GetVehicle(player.vehicle);
                        else
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы не в рабочей машине!");
                            return;
                        }
                        if (currentVehicleController.VehicleData.Material - qty < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~У вас недостаточно топлива на продажу!");
                            return;
                        }

                        var initCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == initUserId);
                        var targetCharacter =
                            ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == targetUserId);
                        if (targetCharacter == null)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вами был введен неверный ID.");
                            return;
                        }

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;
                        VehicleController targetVehicleController;
                        if (target.isInVehicle) targetVehicleController = EntityManager.GetVehicle(target.vehicle);
                        else
                        {
                            API.sendNotificationToPlayer(player, "~r~Покупатель не в машине!");
                            API.sendNotificationToPlayer(target, "~r~Вы не в машине!");
                            return;
                        }

                        if (player.position.DistanceTo(target.position) > 6.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }
                        if (targetCharacter.Cash - cost < 0)
                        {
                            API.sendNotificationToPlayer(target, "~r~У вас недостаточно средств для покупки!");
                            return;
                        }
                        if (targetVehicleController.VehicleData.Fuel + qty >
                            targetVehicleController.VehicleData.FuelTank)
                        {
                            API.sendNotificationToPlayer(player, "~r~Бак покупателя полный!");
                            API.sendNotificationToPlayer(target, "~r~У вас полный бак!");
                            return;
                        }
                        targetCharacter.Cash -= cost;
                        initCharacter.Cash += cost;
                        currentVehicleController.VehicleData.Material -= qty;
                        targetVehicleController.VehicleData.Fuel += qty;
                        ContextFactory.Instance.SaveChanges();
                        API.shared.triggerClientEvent(player, "update_money_display", initCharacter.Cash);
                        API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                        API.sendNotificationToPlayer(player, "~g~Вы продали " + qty + " литров за " + cost + "$");
                        API.sendNotificationToPlayer(target, "~g~Вы купили " + qty + " литров за " + cost + "$");
                    }
                    if (args[0].ToString() == "repair_car")
                    {
                        if (character == null) return;

                        if (!player.isInVehicle)
                        {
                            API.sendNotificationToPlayer(player, "~r~Вы не в рабочей машине!");
                            return;
                        }
                        var initCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == initUserId);
                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;
                        VehicleController targetVehicleController;
                        if (target.isInVehicle) targetVehicleController = EntityManager.GetVehicle(target.vehicle);
                        else
                        {
                            API.sendNotificationToPlayer(player, "~r~Покупатель не в машине!");
                            API.sendNotificationToPlayer(target, "~r~Вы не в машине!");
                            return;
                        }

                        if (player.position.DistanceTo(target.position) > 6.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }
                        if (targetCharacter.Cash - cost < 0)
                        {
                            API.sendNotificationToPlayer(target, "~r~У вас недостаточно средств для покупки!");
                            return;
                        }
                        API.repairVehicle(targetVehicleController.Vehicle);
                        targetCharacter.Cash -= cost;
                        initCharacter.Cash += cost;
                        ContextFactory.Instance.SaveChanges();
                        API.shared.triggerClientEvent(player, "update_money_display", initCharacter.Cash);
                        API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                        API.sendNotificationToPlayer(player, "~g~Вы починили авто за " + cost + "$");
                        API.sendNotificationToPlayer(target, "~g~Вам починили авто за " + cost + "$");

                    }
                    if (args[0].ToString() == "lawyer_prison")
                    {
                        if (character == null) return;

                        var initCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == initUserId);

                        Character targetCharacter;
                        if (!MenuMethods.CheckTargetId(player, out targetCharacter, targetUserId)) return;

                        if (!targetCharacter.IsPrisoned)
                        {
                            API.sendNotificationToPlayer(player, "~r~Игрок не осужден!");
                            return;
                        }

                        var target = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                        if (target == null) return;

                        if (player.position.DistanceTo(target.position) > 5.0F)
                        {
                            API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от покупателя.");
                            API.sendNotificationToPlayer(target, "~y~Вы находитесь далеко от продавца.");
                            return;
                        }
                        if (targetCharacter.Cash - cost < 0)
                        {
                            API.sendNotificationToPlayer(player, "~r~У осужденного недостаточно денег!");
                            API.sendNotificationToPlayer(target, "~r~У вас недостаточно средств для освобождения!");
                            return;
                        }
                        targetCharacter.Cash -= cost;
                        initCharacter.Cash += cost;
                        targetCharacter.IsPrisoned = false;
                        targetCharacter.PrisonTime = 0;
                        API.freezePlayer(target, false);
                        API.setEntityPosition(target, new Vector3(432.43, -981.47, 30.71));
                        API.shared.triggerClientEvent(player, "update_money_display", initCharacter.Cash);
                        API.shared.triggerClientEvent(target, "update_money_display", targetCharacter.Cash);
                        ContextFactory.Instance.SaveChanges();
                        API.sendNotificationToPlayer(player,
                            "~g~Вы выпустили игрока " + targetCharacter.Name + " за " + cost + "$");
                        API.sendNotificationToPlayer(target, "~g~Вас выпустили из тюрьмы за " + cost + "$");
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }

            if (eventName == "get_weapon")
            {
                if (character == null) return;
                var callback = (int)args[0];
                var cost = (int)args[1];
                var propertyName = (string)args[2];

                var weaponTint = new WeaponTint();
                if (CharacterController.IsCharacterInArmy(character)) weaponTint = WeaponTint.Army;
                if (CharacterController.IsCharacterInFbi(character) ||
                    CharacterController.IsCharacterInPolice(character)) weaponTint = WeaponTint.LSPD;
                if (CharacterController.IsCharacterInGang(character)) weaponTint = WeaponTint.Gold;

                var weaponData = ContextFactory.Instance.Weapon.FirstOrDefault(x => x.CharacterId == character.Id);
                var propertyData = ContextFactory.Instance.Property.FirstOrDefault(x => x.Name == propertyName);
                // GANG weapons:
                var inGang = false;
                if (CharacterController.IsCharacterInGang(character))
                {
                    if (character.Material - cost >= 0) inGang = true;
                    else
                    {
                        API.sendNotificationToPlayer(player, "~r~Вы не можете это сделать!Недостаточно материалов!");
                        return;
                    }
                }
                // ARMY weapons:
                if (propertyData.Stock - cost < 0)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не можете это сделать!Недостаточно материалов!");
                    return;
                }
                switch (callback)
                {
                    case 1:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.Revolver, 50, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.Revolver, weaponTint);
                        weaponData.Revolver = 1; weaponData.RevolverPt = 50; break;
                    case 2:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.CarbineRifle, 250, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.CarbineRifle, weaponTint);
                        weaponData.CarbineRifle = 1; weaponData.CarbineRiflePt = 250; break;
                    case 3:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.SniperRifle, 50, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.SniperRifle, weaponTint);
                        weaponData.SniperRifle = 1; weaponData.SniperRiflePt = 50; break;
                    case 4:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.SmokeGrenade, 10, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.SmokeGrenade, weaponTint);
                        weaponData.SmokeGrenade = 1; weaponData.SmokeGrenadePt = 10; break;
                    case 5:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.FlareGun, 50, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.FlareGun, weaponTint);
                        weaponData.FlareGun = 1; weaponData.FlareGunPt = 50; break;
                    case 6:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.CompactRifle, 250, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.CompactRifle, weaponTint);
                        weaponData.CompactRifle = 1; weaponData.CompactRiflePt = 250; break;
                    case 7:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.PumpShotgun, 100, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.PumpShotgun, weaponTint);
                        weaponData.PumpShotgun = 1; weaponData.PumpShotgunPt = 100; break;
                    case 8:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.BZGas, 100, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.BZGas, weaponTint);
                        weaponData.BZGas = 1; weaponData.BZGasPt = 100; break;
                    case 9:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.Nightstick, 1, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.Nightstick, weaponTint);
                        weaponData.Nightstick = 1; break;
                    case 10:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.StunGun, 120, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.StunGun, weaponTint);
                        weaponData.StunGun = 1; weaponData.StunGunPt = 120; break;
                    case 11:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.HeavyPistol, 100, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.HeavyPistol, weaponTint);
                        weaponData.HeavyPistol = 1; weaponData.HeavyPistolPt = 100; break;
                    case 12:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.BullpupRifle, 200, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.BullpupRifle, weaponTint);
                        weaponData.BullpupRifle = 1; weaponData.BullpupRiflePt = 200; break;
                    case 13:
                        if (inGang) character.Material -= cost; else propertyData.Stock -= cost;
                        API.givePlayerWeapon(player, WeaponHash.HeavyShotgun, 100, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.HeavyShotgun, weaponTint);
                        weaponData.HeavyShotgun = 1; weaponData.HeavyShotgunPt = 100; break;
                }
                ContextFactory.Instance.SaveChanges();
            }
            if (eventName == "get_weapon_ammunation")
            {
                if (character == null) return;
                var callback = (int)args[0];
                var cost = (int)args[1];
                var jobId = (int)args[2];

                var weaponTint = new WeaponTint();
                if (CharacterController.IsCharacterInArmy(character)) weaponTint = WeaponTint.Army;
                if (CharacterController.IsCharacterInFbi(character) ||
                    CharacterController.IsCharacterInPolice(character)) weaponTint = WeaponTint.LSPD;
                if (CharacterController.IsCharacterInGang(character)) weaponTint = WeaponTint.Gold;

                var weaponData = ContextFactory.Instance.Weapon.FirstOrDefault(x => x.CharacterId == character.Id);
                var currentJob = ContextFactory.Instance.Job.FirstOrDefault(x => x.Id == jobId);

                if (character.Cash - cost < 0)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не можете это купить! Недостаточно денег!");
                    return;
                }
                var weaponName = "";

                switch (callback)
                {
                    case 1:
                        API.givePlayerWeapon(player, WeaponHash.Revolver, 50, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.Revolver, weaponTint);
                        weaponData.Revolver = 1; weaponData.RevolverPt = 50;
                        weaponName = "Revolver"; break;
                    case 2:
                        API.givePlayerWeapon(player, WeaponHash.CarbineRifle, 250, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.CarbineRifle, weaponTint);
                        weaponData.CarbineRifle = 1; weaponData.CarbineRiflePt = 250;
                        weaponName = "Carbine Rifle"; break;
                    case 3:
                        API.givePlayerWeapon(player, WeaponHash.SniperRifle, 50, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.SniperRifle, weaponTint);
                        weaponData.SniperRifle = 1; weaponData.SniperRiflePt = 50;
                        weaponName = "Sniper Rifle"; break;
                    case 4:
                        API.givePlayerWeapon(player, WeaponHash.SmokeGrenade, 10, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.SmokeGrenade, weaponTint);
                        weaponData.SmokeGrenade = 1; weaponData.SmokeGrenadePt = 10;
                        weaponName = "Smoke Grenade"; break;
                    case 5:
                        API.givePlayerWeapon(player, WeaponHash.FlareGun, 50, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.FlareGun, weaponTint);
                        weaponData.FlareGun = 1; weaponData.FlareGunPt = 50;
                        weaponName = "CarbineRifle"; break;
                    case 6:
                        API.givePlayerWeapon(player, WeaponHash.CompactRifle, 250, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.CompactRifle, weaponTint);
                        weaponData.CompactRifle = 1; weaponData.CompactRiflePt = 250;
                        weaponName = "Compact Rifle"; break;
                    case 7:
                        API.givePlayerWeapon(player, WeaponHash.PumpShotgun, 100, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.PumpShotgun, weaponTint);
                        weaponData.PumpShotgun = 1; weaponData.PumpShotgunPt = 100;
                        weaponName = "Pump Shotgun"; break;
                    case 8:
                        API.givePlayerWeapon(player, WeaponHash.BZGas, 100, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.BZGas, weaponTint);
                        weaponData.BZGas = 1; weaponData.BZGasPt = 100;
                        weaponName = "BZGas"; break;
                    case 9:
                        API.givePlayerWeapon(player, WeaponHash.Nightstick, 1, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.Nightstick, weaponTint);
                        weaponData.Nightstick = 1;
                        weaponName = "Nightstick"; break;
                    case 10:
                        API.givePlayerWeapon(player, WeaponHash.StunGun, 120, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.StunGun, weaponTint);
                        weaponData.StunGun = 1; weaponData.StunGunPt = 120;
                        weaponName = "Stun Gun"; break;
                    case 11:
                        API.givePlayerWeapon(player, WeaponHash.HeavyPistol, 100, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.HeavyPistol, weaponTint);
                        weaponData.HeavyPistol = 1; weaponData.HeavyPistolPt = 100;
                        weaponName = "Heavy Pistol"; break;
                    case 12:
                        API.givePlayerWeapon(player, WeaponHash.BullpupRifle, 200, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.BullpupRifle, weaponTint);
                        weaponData.BullpupRifle = 1; weaponData.BullpupRiflePt = 200;
                        weaponName = "Bullpup Rifle"; break;
                    case 13:
                        API.givePlayerWeapon(player, WeaponHash.HeavyShotgun, 100, true, true);
                        API.setPlayerWeaponTint(player, WeaponHash.HeavyShotgun, weaponTint);
                        weaponData.HeavyShotgun = 1; weaponData.HeavyShotgunPt = 100;
                        weaponName = "Heavy Shotgun"; break;
                }
                character.Cash -= cost;
                currentJob.Money += cost;
                ContextFactory.Instance.SaveChanges();
                API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
                API.sendNotificationToPlayer(player, "~g~Вы купили оружие: "+ weaponName + " за " + cost + "$");
            }
            if (eventName == "take_debt")
            {
                if (character == null) return;
                var debtSize = (int)args[0];

                if (debtSize > character.Cash)
                {
                    API.sendChatMessageToPlayer(player, "~r~[ОШИБКА]: ~w~У вас недостаточно средств!");
                    return;
                }
                var debtMafiaGroup = character.DebtMafia * 100;
                var mafiaGroup = ContextFactory.Instance.Group.First(x => x.Id == debtMafiaGroup);

                mafiaGroup.MoneyBank += debtSize;
                character.Cash -= debtSize;
                ContextFactory.Instance.SaveChanges();
                API.sendChatMessageToPlayer(player, "~g~[УСПЕШНО]: ~w~Вы вернули " + debtSize + "$");
                ChatController.SendMessageInMyGroup(player, "Возвращено " + debtSize + "$ от игрока " + character.Name);
            }
            
            if (eventName == "buy_driver_license")
            {
                if (character == null) return;
                if (character.Cash - Prices.DriverLicensePrice < 0)
                {
                    API.shared.sendNotificationToPlayer(player, "У вас нет " + Prices.DriverLicensePrice + "$ для покупки прав!");
                    return;
                }
                if (character.DriverLicense == 1)
                    API.shared.sendNotificationToPlayer(player, "У вас уже есть права!");
                else
                {
                    character.Cash -= Prices.DriverLicensePrice;
                    character.DriverLicense = 1;
                    API.shared.triggerClientEvent(player, "update_money_display", character.Cash);
                    ContextFactory.Instance.SaveChanges();
                    API.shared.sendNotificationToPlayer(player, "Вы успешно приобрели права!");
                }
            }
            if (eventName == "got_driver_license")
            {
                if (character == null) return;
                try
                {
                    var userId = (int)args[0];
                    var targetCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                    if (targetCharacter == null)
                    {
                        API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                        return;
                    }

                    var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                    if (target == null) return;
                    if (player.position.DistanceTo(target.position) < 3.0F)
                    {
                        API.sendNotificationToPlayer(player, "~y~[ПРЕДУПРЕЖДЕНИЕ]: ~w~Вы находитесь далеко от покупателя!");
                        API.sendNotificationToPlayer(target, "~y~[ПРЕДУПРЕЖДЕНИЕ]: ~w~Вы находитесь далеко от продавца!");
                        return;
                    }
                    targetCharacter.DriverLicense = 1;
                    ContextFactory.Instance.SaveChanges();

                    target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " выдал вам водительские права \nкатегории " + targetCharacter.DriverLicense);
                    API.sendNotificationToPlayer(player, "~g~[УСПЕШНО]: ~w~Вы выдали права категории" + targetCharacter.DriverLicense + "\nпользователю с ID: " + userId);
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                    return;
                }
            }

            // UTILS:
            if (eventName == "change_color")
            {
                var r = Convert.ToByte(args[0]);
                var g = Convert.ToByte(args[1]);
                var b = Convert.ToByte(args[2]);

                API.shared.setPlayerNametagColor(player, r, g, b);
            }
            if (eventName == "send_chat_message")
            {
                try
                {
                    var message = (string) args[0];
                    ChatController.SendMessageInMyGroup(player, message);
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }

            // ADMIN        (TRY OK):
            if (eventName == "admin_add_to_group")
            {
                try
                {
                    var userId = (int)args[0];
                    var groupId = (int)args[1];
                    var targetCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                    if (targetCharacter == null)
                    {
                        API.sendChatMessageToPlayer(player, "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                        return;
                    }
                    
                    var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                    if (target == null) return;

                    var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == groupId);
                    var groupType = (GroupType)Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
                    var groupExtraType = (GroupExtraType)Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());

                    targetCharacter.ActiveGroupID = groupId;
                    targetCharacter.GroupType = (int)groupType;
                    ContextFactory.Instance.SaveChanges();

                    targetCharacter.ActiveClothes = ClothesManager.SetFractionClothes(target, groupId, targetCharacter);

                    target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " перевел вас во фракцию: " + EntityManager.GetDisplayName(groupType) + "\nНа должность: " + EntityManager.GetDisplayName(groupExtraType));
                    API.sendChatMessageToPlayer(player, "~g~[УСПЕШНО]: ~w~Вы перевели пользователя: " + targetCharacter.Name + "\nВо фракцию: " + EntityManager.GetDisplayName(groupType) + "\nНа должность: " + EntityManager.GetDisplayName(groupExtraType));
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "admin_add_to_admin")
            {
                try
                {
                    var userId = (int)args[0];
                    var adminId = (int)args[1];
                    var targetCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                    if (targetCharacter == null)
                    {
                        API.sendChatMessageToPlayer(player, "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                        return;
                    }
                    targetCharacter.Admin = adminId;
                    ContextFactory.Instance.SaveChanges();

                    var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                    if (target == null) return;
                    
                    target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " сделал Вас администратором уровня: " + adminId);
                    API.sendChatMessageToPlayer(player, "~g~[УСПЕШНО]: ~w~Вы сделали пользователя: " + targetCharacter.Name + "\nАдминистратором уровня: " + adminId);
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "admin_change user_level")
            {
                try
                {
                    var userId = (int)args[0];
                    var level = (int)args[1];
                    var targetCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                    if (targetCharacter == null)
                    {
                        API.sendChatMessageToPlayer(player, "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                        return;
                    }
                    targetCharacter.Level = level;
                    ContextFactory.Instance.SaveChanges();

                    var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                    if (target == null) return;

                    target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " сделал вам уровень: " + level);
                    API.sendChatMessageToPlayer(player, "~g~[УСПЕШНО]: ~w~Вы сделали пользователю: " + targetCharacter.Name + " уровень: " + level);
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "spectate_player")
            {
                try
                {
                    var userId = (int) args[0];

                    var targetCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                    if (targetCharacter == null)
                    {
                        API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                        return;
                    }
                    if (userId == 0) API.unspectatePlayer(player);

                    var target = API.shared.getAllPlayers()
                        .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                    if (target == null) return;
                    API.setPlayerToSpectatePlayer(player, target);
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "teleport_to_player")
            {
                try
                {
                    var userId = (int) args[0];

                    var targetCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == userId);
                    if (targetCharacter == null)
                    {
                        API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                        return;
                    }

                    var target = API.shared.getAllPlayers()
                        .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
                    if (target == null) return;
                    player.position = target.position;
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели значение!");
                }
            }
            if (eventName == "report_to_admin")
            {
                try
                {
                    var message = Convert.ToString(args[0]);

                    var admins = ContextFactory.Instance.Character.Where(x => x.Admin != 0 && x.Online).ToList();
                    if (admins == null)
                    {
                        API.sendNotificationToPlayer(player, "~r~На данный момент на сервере нет администрации!");
                        return;
                    }
                    
                    foreach (var admin in admins)
                    {
                        var currentAdmin = API.shared.getAllPlayers()
                            .FirstOrDefault(x => x.socialClubName == admin.SocialClub);
                        API.sendChatMessageToPlayer(currentAdmin, "~r~[REPORT]: ~s~ от игрока: " + character.Name + "\n" + message);
                    }
                }
                catch (Exception)
                {
                    API.sendNotificationToPlayer(player, "~r~Вы не ввели сообщение!");
                }
            }

            // UTILITY:
            if (eventName == "ask_user_posXY")
            {
                API.shared.triggerClientEvent(player, "send_user_posXY", (int)player.position.X, (int)player.position.Y);
            }
            if (eventName == "ask_user_sector")
            {
                API.shared.triggerClientEvent(player, "send_user_sector", CharacterController.InWhichSectorOfGhetto(player));
            }

        }

        private void AddUserToFraction(Client player, Character targetCharacter, Character character, int groupMain)
        {
            var formatName = character.Name.Replace("_", " ");
            var target = API.shared.getAllPlayers()
                .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
            if (target == null) return;
            if (player.position.DistanceTo(target.position) < 3.0F)
            {
                API.sendNotificationToPlayer(player, "~y~Вы находитесь далеко от пользователя.");
                return;
            }

            var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == groupMain);
            var groupType = (GroupType)Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
            var groupExtraType =
                (GroupExtraType)Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());

            targetCharacter.ActiveGroupID = groupMain + 1;
            targetCharacter.GroupType = (int)groupType;
            targetCharacter.JobId = 0;
            targetCharacter.ActiveClothes =
                ClothesManager.SetFractionClothes(target, groupMain + 1, targetCharacter);
            ContextFactory.Instance.SaveChangesAsync();

            target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName + " принял вас в: " +
                                   EntityManager.GetDisplayName(groupType) + "\nНа звание: " +
                                   EntityManager.GetDisplayName(groupExtraType));
            API.sendChatMessageToPlayer(player,
                "~g~[УСПЕШНО]: ~w~Вы перевели пользователя: " + targetCharacter.Name +
                "\nВо фракцию: " + EntityManager.GetDisplayName(groupType) + "\nНа должность: " +
                EntityManager.GetDisplayName(groupExtraType));
        }
        private void ChangeUserRang(Client player, Character targetCharacter, Character character, int rangId )
        {
            var formatName = character.Name.Replace("_", " ");
            var target = API.shared.getAllPlayers()
                .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
            if (target == null) return;

            if (player.position.DistanceTo(target.position) < 3.0F)
            {
                API.sendNotificationToPlayer(player,
                    "~y~Вы находитесь далеко от пользователя.");
                return;
            }
            targetCharacter.ActiveGroupID = character.GroupType * 100 + rangId;
            ContextFactory.Instance.SaveChangesAsync();

            var getGroup =
                ContextFactory.Instance.Group.FirstOrDefault(
                    x => x.Id == targetCharacter.ActiveGroupID);
            var groupExtraType = (GroupExtraType)Enum.Parse(typeof(GroupExtraType),
                getGroup.ExtraType.ToString());
            target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                   " присвоил вам ранг: " +
                                   EntityManager.GetDisplayName(groupExtraType));
            API.sendChatMessageToPlayer(player,
                "~g~[УСПЕШНО]: ~w~Вы присвоили пользователю: " + targetCharacter.Name +
                "\nРанг: " + EntityManager.GetDisplayName(groupExtraType));
        }
        private void DeleteUserFromFraction(Client player, Character targetCharacter, Character character, int groupBase)
        {
            var formatName = character.Name.Replace("_", " ");
            var target = API.shared.getAllPlayers()
                .FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
            if (target == null) return;
            targetCharacter.ActiveClothes =
                ClothesManager.SetFractionClothes(target, 0, targetCharacter);
            ContextFactory.Instance.SaveChanges();

            var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == groupBase);
            var groupType = (GroupType)Enum.Parse(typeof(GroupType), getGroup.Type.ToString());

            target.sendChatMessage("~g~[СЕРВЕР]: ~w~Игрок " + formatName +
                                   " выгнал вас из фракции: " +
                                   EntityManager.GetDisplayName(groupType) +
                                   "\nДля пособия по безработице - проследуйте в мэрию.");
            API.sendChatMessageToPlayer(player,
                "~g~[УСПЕШНО]: ~w~Вы выгнали пользователя: " + targetCharacter.Name +
                "\nИз фракции: " + EntityManager.GetDisplayName(groupType));

            var isMale = targetCharacter.Model == 1885233650;
            var baseClothes = isMale ? 999 : 9990;
            ClothesManager.SetPlayerSkinClothesToDb(target, baseClothes, targetCharacter, false);
            targetCharacter.ActiveClothes = baseClothes;

            targetCharacter.ActiveGroupID = 1;
            targetCharacter.GroupType = 100;
            ContextFactory.Instance.SaveChangesAsync();
        }
    }
}
