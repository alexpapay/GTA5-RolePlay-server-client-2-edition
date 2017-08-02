using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using MpRpServer.Data;
using MpRpServer.Data.Enums;
using MpRpServer.Data.Models;
using MpRpServer.Server.Characters;
using MpRpServer.Server.DBManager;
using MpRpServer.Server.Groups;
using MpRpServer.Server.Property;
using MpRpServer.Server.Vehicles;
using System;
using System.Linq;

namespace MpRpServer.Server.Global
{
    class KeyManager : Script
    {    
        public KeyManager()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
        }        
        private void OnClientEventTrigger(Client player, string eventName, object[] args)
        {
            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            var character = characterController.Character;
            var formatName = character.Name.Replace("_", " ");

            if (eventName == "onKeyUp")
            {
                var key = (int) args[0];

                if (key == 2)
                {
                    PropertyController propertyController;
                    if ((propertyController = player.getData("AT_PROPERTY")) != null)
                    {
                        propertyController.PropertyDoor(player);
                    }
                }
                else if(key == 3)
                {
                    if(player.isInVehicle)
                    {
                        player.vehicle.specialLight = true;
                    }
                }
                else if (key == 4)
                {
                    if (player.isInVehicle)
                    {
                        player.vehicle.specialLight = true;
                    }
                }
                else if (key == 5)
                {
                    if (player.isInVehicle) VehicleController.TriggerDoor(player.vehicle, 4);
                }
                else if (key == 6)
                {
                    if (player.isInVehicle) VehicleController.TriggerDoor(player.vehicle, 5);                    
                }
                else if (key == 8)
                {
                    CharacterController.StopAnimation(player);
                }
                // Get info about Player:
                else if (key == 9)
                {                    
                    if (character == null) return;
                    var job = "";
                    switch (character.JobId)
                    {
                        case JobsIdNonDataBase.Homeless: job = "Бомж"; break;
                        case JobsIdNonDataBase.Loader1: job = "Грузчик: С1"; break;
                        case JobsIdNonDataBase.Loader2: job = "Грузчик: С2"; break;
                        case JobsIdNonDataBase.TaxiDriver: job = "Таксист"; break;
                        case JobsIdNonDataBase.BusDriver1: job = "Водитель автобуса: М1"; break;
                        case JobsIdNonDataBase.BusDriver2: job = "Водитель автобуса: М2"; break;
                        case JobsIdNonDataBase.BusDriver3: job = "Водитель автобуса: М3"; break;
                        case JobsIdNonDataBase.BusDriver4: job = "Водитель автобуса: М4"; break;
                        case JobsIdNonDataBase.Mechanic: job = "Автомеханик"; break;
                    }

                    var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == character.ActiveGroupID);
                    if (getGroup == null) return;
                    var groupType = (GroupType)Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
                    var groupExtraType = (GroupExtraType)Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());
                    
                    var driverLicense = character.DriverLicense == 1 ? "Да" : "Нет";                   

                    API.shared.triggerClientEvent(player, "character_menu",
                        2,                                                  // 0
                        "Ваша статистика",                                  // 1
                        "Ваше имя: " + formatName,                          // 2
                        character.Age,                                      // 3
                        character.Level.ToString(),                         // 4
                        job,                                                // 5
                        character.Bank.ToString(),                          // 6
                        driverLicense,                                      // 7
                        character.Debt,                                     // 8
                        EntityManager.GetDisplayName(groupType),            // 9
                        EntityManager.GetDisplayName(groupExtraType),       // 10
                        character.Material,                                 // 11
                        CharacterController.IsCharacterInMafia(character),  // 12
                        CharacterController.IsCharacterInGang(character),   // 13
                        character.Narco,                                    // 14
                        character.MobilePhone);                             // 15
                }
                // GET info about car
                else if (key == 10)
                {
                    try
                    {
                        VehicleController vehicleController;

                        bool inVehicleCheck;
                        if (player.isInVehicle)
                        {
                            vehicleController = EntityManager.GetVehicle(player.vehicle);
                            inVehicleCheck = true;
                        }
                        else
                        {
                            vehicleController = EntityManager.GetVehicleControllers().Find(x => x.Vehicle.position.DistanceTo(player.position) < 3.0f);
                            inVehicleCheck = false;
                        }
                        if (vehicleController == null)
                        {
                            API.sendNotificationToPlayer(player, "Вы находитесь далеко от транспорта.");
                            return;
                        }

                        int engineStatus = 0;
                        if (API.getVehicleEngineStatus(vehicleController.Vehicle)) engineStatus = 1;

                        int driverDoorStatus = 1;
                        if (API.getVehicleLocked(vehicleController.Vehicle)) driverDoorStatus = 0;
                        var fuel = vehicleController.VehicleData.FuelTank;

                        var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == character.ActiveGroupID);
                        if (getGroup == null) return;

                        var groupType = (GroupType)Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
                        string owner;
                        if (vehicleController.VehicleData.GroupId == null) owner = "Владелец: " + formatName;
                        else owner = "Владелец: " + EntityManager.GetDisplayName(groupType);

                        API.shared.triggerClientEvent(player, "vehicle_menu",
                            2,                                          // 0
                            "Меню транспорта",                          // 1
                            owner,                                      // 2
                            engineStatus,                               // 3
                            fuel.ToString(),                            // 4
                            inVehicleCheck,                             // 5
                            driverDoorStatus,                           // 6
                            vehicleController.VehicleData.Material);    // 7   
                    }
                    catch (Exception)
                    {
                        
                    }
                }
                // Get info about work possibilities:
                else if (key == 12)
                {
                    try
                    {
                        if (character == null) return;
                        var getGroup = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == character.ActiveGroupID);
                        if (getGroup == null) return;
                        var groupType = (GroupType)Enum.Parse(typeof(GroupType), getGroup.Type.ToString());
                        var groupExtraType = (GroupExtraType)Enum.Parse(typeof(GroupExtraType), getGroup.ExtraType.ToString());

                        var playerWeapons = API.getPlayerWeapons(player);
                        var weaponList = "";
                        for (var i = 0; i < playerWeapons.Length; i++)
                            weaponList += playerWeapons[i] + "-";

                        var materialProperty = new Data.Property();

                        var moneyStockGroup = new Group();
                        var groupStockName = GroupController.GetGroupStockName(character);
                        var moneyBankGroup = character.GroupType * 100;

                        var gangRank = (int)groupExtraType - (int)groupType * 100;

                        try { materialProperty = ContextFactory.Instance.Property.First(x => x.Name == groupStockName); }
                        catch (Exception)
                        {
                            // ignored
                        }
                        try { moneyStockGroup = ContextFactory.Instance.Group.First(x => x.Id == moneyBankGroup); }
                        catch (Exception)
                        {
                            // ignored
                        }

                        var gangsSectors = GroupController.GetGangsSectors();
                        var gangCurrentSector = CharacterController.InWhichSectorOfGhetto(player).Split(';');
                        var gangCurrentSectorData = GroupController.GetGangSectorData(Convert.ToInt32(gangCurrentSector[0]), Convert.ToInt32(gangCurrentSector[1]));
                        if (gangCurrentSectorData > 100) gangCurrentSectorData /= 10;
                        var isSectorInYourGang = gangCurrentSectorData == (int)groupType;
                        if (gangCurrentSectorData > 100 && isSectorInYourGang == false)
                            isSectorInYourGang = true;

                        var emergencyCost = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 200);

                        API.shared.triggerClientEvent(player, "workposs_menu",
                            1,                                                                                  // 0
                            character.ActiveGroupID,                                                            // 1
                            character.JobId,                                                                    // 2
                            character.TempVar,                                                                  // 3
                            character.Admin,                                                                    // 4
                            EntityManager.GetDisplayName(groupType),                                            // 5
                            EntityManager.GetDisplayName(groupExtraType),                                       // 6
                            character.Material,                                                                 // 7
                            CharacterController.IsCharacterInGang(characterController),                         // 8
                            CharacterController.IsCharacterGangBoss(characterController),                       // 9
                            CharacterController.IsCharacterArmyHighOfficer(characterController.Character),      // 10
                            CharacterController.IsCharacterInGhetto(player),                                    // 11
                            CharacterController.IsCharacterArmyGeneral(characterController),                    // 12
                            weaponList,                                                                         // 13
                            character.OID,                                                                      // 14
                            materialProperty.Stock,                                                             // 15
                            moneyStockGroup.MoneyBank,                                                          // 16
                            CharacterController.IsCharacterHighRankInGang(character),                           // 17
                            gangRank,                                                                           // 18
                            (int)groupType,                                                                     // 19
                            gangsSectors,                                                                       // 20
                            isSectorInYourGang,                                                                 // 21
                            groupStockName,                                                                     // 22
                            CharacterController.IsCharacterInMafia(characterController),                        // 23
                            emergencyCost.MoneyBank);                                                           // 24
                    }
                    catch (Exception)
                    {}
                }
                // MOBILE PHONE:
                else if (key == 13)
                {
                    if (player.getData("MOBILEPHONE") == 0)
                    {
                        API.sendNotificationToPlayer(player, "~r~У вас нет мобильного телефона. Купите его в 24/7!");
                        return;
                    }
                    var allMessages = ContextFactory.Instance.Mobile.Where(x => x.To == character.MobilePhone).ToList();
                    foreach (var message in allMessages)
                    {
                        var fromPlayers = message.From;
                    }
                }
                else if (key == 14)
                {
                }
                else if (key == 15)
                {
                    player.setData("BUTTON_VOICE", player.getData("BUTTON_VOICE") == 0 ? 1 : 0);
                    API.shared.sendNotificationToPlayer(player, "~y~Микрофон: " + (player.getData("BUTTON_VOICE") == 0 ? "~r~Выкл" : "~g~Вкл"));
                }
            }
        }
    }
}
