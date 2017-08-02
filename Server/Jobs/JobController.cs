using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using MpRpServer.Data;
using MpRpServer.Data.Attributes;
using MpRpServer.Data.Enums;
using MpRpServer.Data.Extensions;
using MpRpServer.Data.Models;
using MpRpServer.Server.Characters;
using MpRpServer.Server.DBManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MpRpServer.Server.Jobs
{
    public class JobController : Script
    {
        // Грузчик :: метки и маркеры
        private readonly Vector3 _job1Marker1 = new Vector3(895.07, -2968.57, 5.9);
        private readonly Vector3 _job1Marker2 = new Vector3(936.667, -2907.894, 5.9);
        private ColShape _job11MarCol;
        private ColShape _job12MarCol;

        private readonly Vector3 _job2Marker1 = new Vector3(-155.5, -959.14, 269.2);
        private readonly Vector3 _job2Marker2 = new Vector3(-179.88, -1008.7, 254.1316);
        private ColShape _job21MarCol;
        private ColShape _job22MarCol;

        // Таксист :: переменные
        private double _senderxcoords, _senderycoords;
        private Client _sen;
        private static int _i;

        public Job JobData;
        public Groups.GroupController Group;

        private Blip _blip;
        private ColShape _colShape;

        public int CharacterId { get; internal set; }

        public JobController() { }
        public JobController(Job jobData)
        {
            JobData = jobData;
            EntityManager.Add(this);
        }
        public static void LoadJobs()
        {
            foreach (var job in ContextFactory.Instance.Job)
            {
                var jobController = new JobController(job);
                jobController.CreateWorldEntity();
            }
            API.shared.consoleOutput("[GM] Загружено работ : " + ContextFactory.Instance.Job.Count());
            Main.LoadingFinished = true;
        }
        // Add new job below:
        public void CreateWorldEntity()
        {
            _blip = API.createBlip(new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ));
            _blip.shortRange = true;
            API.setBlipSprite(_blip, JobData.Type.GetAttributeOfType<BlipTypeAttribute>().BlipId);
            API.setBlipName(_blip, Group == null ? Type() : Group.Group.Name);

            switch (JobData.Type)
            {
                case JobType.Loader:
                    API.createTextLabel("~w~Работа грузчиком.\nЗаработок даже за один цикл", new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                    API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                                                 new Vector3(1f, 1f, 1f), 250, 25, 50, 200);
                    break;

                case JobType.BusDriver:
                    API.createTextLabel("~w~Работа водителем\nавтобуса", new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                    API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                                                 new Vector3(1.5f, 1.5f, 1.5f), 250, 25, 50, 200); break;
                case JobType.TaxiDriver:
                    API.createTextLabel("~w~Работа таксистом", new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                    API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                                                 new Vector3(1.5f, 1.5f, 1.5f), 250, 100, 100, 0); break;
                case JobType.GasStation:
                    if (JobData.CharacterId == 0)
                    {
                        API.createTextLabel("~w~Бизнес: заправка\n(свободен): " + JobData.Id, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                                                 new Vector3(1f, 1f, 1f), 250, 10, 250, 10);                        
                        API.setBlipColor(_blip, 2);
                    }
                    else
                    {                        
                        API.createTextLabel("~w~Бизнес: заправка\nВладелец: " + JobData.OwnerName, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                                                 new Vector3(1f, 1f, 1f), 250, 10, 250, 10);
                        API.setBlipColor(_blip, 1);
                    }
                    break;
                case JobType.Mechanic:
                    if (JobData.CharacterId == 0)
                    {
                        API.createTextLabel("~w~Бизнес: автомеханик\n(свободен): " + JobData.Id, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 250, 10, 250, 10);
                        API.setBlipColor(_blip, 2);
                    }
                    else
                    {
                        API.createTextLabel("~w~Бизнес: автомеханик\nВладелец: " + JobData.OwnerName, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 250, 10, 250, 10);
                        API.setBlipColor(_blip, 1);
                    }
                    break;
                case JobType.ClothStore:
                    if (JobData.CharacterId == 0)
                    {
                        API.createTextLabel("~w~Бизнес: магазин одежды\n(свободен): " + JobData.Id, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 250, 10, 250, 10);
                        API.setBlipColor(_blip, 2);
                    }
                    else
                    {
                        API.createTextLabel("~w~Бизнес: магазин одежды\nВладелец: " + JobData.OwnerName, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 250, 10, 250, 10);
                        API.setBlipColor(_blip, 1);
                    }
                    break;
                case JobType.AmmuNation:
                    if (JobData.CharacterId == 0)
                    {
                        API.createTextLabel("~w~Бизнес: магазин оружия\n(свободен): " + JobData.Id, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 250, 10, 250, 10);
                        API.setBlipColor(_blip, 2);
                    }
                    else
                    {
                        API.createTextLabel("~w~Бизнес: магазин оружия\nВладелец: " + JobData.OwnerName, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        API.createMarker(1, new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 250, 0, 0, 255);
                        API.setBlipColor(_blip, 1);
                    }
                    break;
            }

            CreateColShape();
            CreateMarkersColShape();
        }
        public void CreateColShape()
        {
            _colShape = API.createCylinderColShape(new Vector3(JobData.PosX, JobData.PosY, JobData.PosZ), 2f, 3f);
            _colShape.onEntityEnterColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    switch (JobData.Type)
                    {
                        case JobType.Mechanic:
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_mechanic_menu", 1,
                                "Работа автомехаником", "Заправляйте и чините за деньги!"); break;
                        case JobType.Loader:
                            Vector3 firstMarker = null;
                            if (JobData.Id == 1) firstMarker = _job1Marker1;
                            if (JobData.Id == 2) firstMarker = _job2Marker1;
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_loader_menu", 1,
                                "Работа грузчиком", "Заработайте свои первые деньги!", JobData.Id,
                                firstMarker.X, firstMarker.Y, firstMarker.Z); break;

                        case JobType.BusDriver:
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_busdriver_menu", 1,
                                "Водитель автобуса", "Перевозите пассажиров за деньги!"); break;

                        case JobType.TaxiDriver:
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_taxi_menu", 1,
                                "Таксист", "Перевозите пассажиров за деньги!"); break;

                        case JobType.GasStation:
                            if (JobData.CharacterId != 0)
                            {                                
                                var owner = ContextFactory.Instance.Character.First(x => x.Id == JobData.CharacterId);
                                CharacterController character = API.getPlayerFromHandle(entity).getData("CHARACTER");
                                var isOwner = owner.Id == character.Character.Id;

                                API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_gasstation_menu", 1,
                                    "24/7 заправка", "Владелец: " + owner.Name, JobData.Id, 1, JobData.Cost, isOwner, JobData.Money); break;
                            }
                            else
                            {
                                API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_gasstation_menu", 1,
                                    "24/7 заправка", "Доступная для покупки!",  JobData.Id, 2, JobData.Cost); break;
                            }

                        case JobType.ClothStore:
                            if (JobData.CharacterId != 0)
                            {
                                var owner = ContextFactory.Instance.Character.First(x => x.Id == JobData.CharacterId);
                                CharacterController character = API.getPlayerFromHandle(entity).getData("CHARACTER");
                                var isOwner = owner.Id == character.Character.Id;

                                API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_clothstore_menu", 1,
                                    "Магазин одежды", "Владелец: " + owner.Name, JobData.Id, 1, JobData.Cost, isOwner, JobData.Money); break;
                            }
                            else
                            {
                                API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_clothstore_menu", 1,
                                    "Магазин одежды", "Доступно для покупки!", JobData.Id, 2, JobData.Cost); break;
                            }
                        case JobType.AmmuNation:
                            if (JobData.CharacterId != 0)
                            {
                                var owner = ContextFactory.Instance.Character.First(x => x.Id == JobData.CharacterId);
                                CharacterController character = API.getPlayerFromHandle(entity).getData("CHARACTER");
                                var isOwner = owner.Id == character.Character.Id;

                                API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_ammunation_menu", 1,
                                    "Магазин оружия", "Владелец: " + owner.Name, JobData.Id, 1, JobData.Cost, isOwner, JobData.Money); break;
                            }
                            else
                            {
                                API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_ammunation_menu", 1,
                                    "Магазин оружия", "Доступно для покупки!", JobData.Id, 2, JobData.Cost); break;
                            }
                    }                                      
                }
            };
            _colShape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    switch (JobData.Type)
                    {
                        case JobType.Loader:
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_loader_menu", 0,
                                "Работа грузчиком", "Заработайте свои первые деньги!", JobData.Id, 0.0, 0.0, 0.0); break;
                        case JobType.BusDriver:
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_busdriver_menu", 0,
                                "Водитель автобуса", "Перевозите пассажиров за деньги!", JobData.Id, 0.0, 0.0, 0.0); break;
                        case JobType.TaxiDriver:
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_taxi_menu", 0,
                                "Таксист", "Перевозите пассажиров за деньги!"); break;

                        case JobType.GasStation:
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_gasstation_menu", 0,
                                    "", "", JobData.Id, 1, JobData.Cost, false, JobData.Money);
                            break;
                        case JobType.Mechanic:
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_mechanic_menu", 0,
                                "Работа автомехаником", "Заправляйте и чините за деньги!");
                            break;

                        case JobType.ClothStore:
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_clothstore_menu", 0,
                                "", "", JobData.Id, 1, JobData.Cost, false, JobData.Money);
                            break;
                        case JobType.AmmuNation:
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_ammunation_menu", 0,
                                "", "", JobData.Id, 1, JobData.Cost, false, JobData.Money);
                            break;
                    }                    
                }
            };
        }
        public void CreateMarkersColShape()
        {  
            // Bus Driver
            if (JobData.Id == 7 || JobData.Id == 17)
            {
                // BusOne trace:
                ColShape busOneColShape1 = API.shared.createCylinderColShape(BusOne.Marker1, 3f, 3f);
                ColShape busOneColShape2 = API.shared.createCylinderColShape(BusOne.Marker2, 3f, 3f);
                ColShape busOneColShape3 = API.shared.createCylinderColShape(BusOne.Marker3, 3f, 3f);
                ColShape busOneColShape4 = API.shared.createCylinderColShape(BusOne.Marker4, 3f, 3f);
                ColShape busOneColShapeFin = API.shared.createCylinderColShape(BusOne.MarkerFin, 3f, 3f);

                busOneColShape1.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSONE_1", "BUSONE_2", BusOne.Marker2.X, BusOne.Marker2.Y, BusOne.Marker2.Z,
                        "Первая остановка: Аэропорт. Двигайтесь дальше.");
                };
                busOneColShape2.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSONE_2", "BUSONE_3", BusOne.Marker3.X, BusOne.Marker3.Y, BusOne.Marker3.Z,
                        "Вторая остановка: Больница. Двигайтесь дальше.");                    
                };
                busOneColShape3.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSONE_3", "BUSONE_4", BusOne.Marker4.X, BusOne.Marker4.Y, BusOne.Marker4.Z,
                        "Третья остановка: Мэрия. Двигайтесь дальше.");                    
                };
                busOneColShape4.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSONE_4", "BUSONE_5", BusOne.MarkerFin.X, BusOne.MarkerFin.Y, BusOne.MarkerFin.Z,
                        "Конечная остановка: стройка. Двигайтесь на станцию.");
                };
                busOneColShapeFin.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSONE_5", "BUSONE_5", 0.0f, 0.0f, 0.0f, "");
                };

                // BusTwo trace:
                ColShape busTwoColShape1 = API.shared.createCylinderColShape(BusTwo.Marker1, 3f, 3f);
                ColShape busTwoColShape2 = API.shared.createCylinderColShape(BusTwo.Marker2, 3f, 3f);
                ColShape busTwoColShape3 = API.shared.createCylinderColShape(BusTwo.Marker3, 3f, 3f);
                ColShape busTwoColShape4 = API.shared.createCylinderColShape(BusTwo.Marker4, 3f, 3f);
                ColShape busTwoColShape5 = API.shared.createCylinderColShape(BusTwo.Marker5, 3f, 3f);
                ColShape busTwoColShape6 = API.shared.createCylinderColShape(BusTwo.Marker6, 3f, 3f);
                ColShape busTwoColShape7 = API.shared.createCylinderColShape(BusTwo.Marker7, 3f, 3f);
                ColShape busTwoColShape8 = API.shared.createCylinderColShape(BusTwo.Marker8, 3f, 3f);
                ColShape busTwoColShape9 = API.shared.createCylinderColShape(BusTwo.Marker9, 3f, 3f);
                ColShape busTwoColShape10 = API.shared.createCylinderColShape(BusTwo.Marker10, 3f, 3f);
                ColShape busTwoColShapeFin = API.shared.createCylinderColShape(BusTwo.MarkerFin, 3f, 3f);

                busTwoColShape1.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_1", "BUSTWO_2", BusTwo.Marker2.X, BusTwo.Marker2.Y, BusTwo.Marker2.Z,
                            "Первая остановка. Двигайтесь дальше.");
                };
                busTwoColShape2.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_2", "BUSTWO_3", BusTwo.Marker3.X, BusTwo.Marker3.Y, BusTwo.Marker3.Z,
                            "Вторая остановка. Двигайтесь дальше.");
                };
                busTwoColShape3.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_3", "BUSTWO_4", BusTwo.Marker4.X, BusTwo.Marker4.Y, BusTwo.Marker4.Z,
                            "Третья остановка. Двигайтесь дальше.");
                };
                busTwoColShape4.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_4", "BUSTWO_5", BusTwo.Marker5.X, BusTwo.Marker5.Y, BusTwo.Marker5.Z,
                            "Четвертая остановка. Двигайтесь дальше.");
                };
                busTwoColShape5.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_5", "BUSTWO_6", BusTwo.Marker6.X, BusTwo.Marker6.Y, BusTwo.Marker6.Z,
                            "Пятая остановка. Двигайтесь дальше.");
                };
                busTwoColShape6.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_6", "BUSTWO_7", BusTwo.Marker7.X, BusTwo.Marker7.Y, BusTwo.Marker7.Z,
                            "Шестая остановка. Двигайтесь дальше.");
                };
                busTwoColShape7.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_7", "BUSTWO_8", BusTwo.Marker8.X, BusTwo.Marker8.Y, BusTwo.Marker8.Z,
                            "Седьмая остановка. Двигайтесь дальше.");
                };
                busTwoColShape8.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_8", "BUSTWO_9", BusTwo.Marker9.X, BusTwo.Marker9.Y, BusTwo.Marker9.Z,
                            "Восьмая остановка. Двигайтесь дальше.");
                };
                busTwoColShape9.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_9", "BUSTWO_10", BusTwo.Marker10.X, BusTwo.Marker10.Y, BusTwo.Marker10.Z,
                            "Девятая остановка. Двигайтесь дальше.");
                };
                busTwoColShape10.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_10", "BUSTWO_11", BusTwo.MarkerFin.X, BusTwo.MarkerFin.Y, BusTwo.MarkerFin.Z,
                            "Десятая остановка. Двигайтесь на конечную!");
                };
                busTwoColShapeFin.onEntityEnterColShape += (shape, entity) =>
                {
                    if (IsPlayerInBus(entity))
                        BusDriver(entity, "BUSTWO_11", "BUSTWO_11", 0.0f, 0.0f, 0.0f, "");
                };
            }
            // Loader 1st place
            if (JobData.Id == 1)
            {
                _job11MarCol = API.shared.createCylinderColShape(_job1Marker1, 2f, 3f);
                _job12MarCol = API.shared.createCylinderColShape(_job1Marker2, 2f, 3f);                

                _job11MarCol.onEntityEnterColShape += (shape, entity) =>
                {                    
                    CharacterController characterController = API.getPlayerFromHandle(entity).getData("CHARACTER");
                    if (characterController == null) return;

                    if (characterController.Character.JobId == 1)
                    {
                        var player = API.getPlayerFromHandle(entity);

                        if (player.hasData("SECOND_OK"))
                        {
                            API.triggerClientEvent(player, "loader_end");
                            API.triggerClientEvent(player, "loader_two", _job1Marker2.X, _job1Marker2.Y, _job1Marker2.Z);
                            API.setPlayerClothes(player, 5, 44, 0);
                            player.resetData("SECOND_OK");
                            player.setData("FIRST_OK", null);
                            API.triggerClientEvent(player, "markonmap", _job1Marker2.X, _job1Marker2.Y);
                        }
                    }
                };
                _job12MarCol.onEntityEnterColShape += (shape, entity) =>
                {
                    CharacterController characterController = API.getPlayerFromHandle(entity).getData("CHARACTER");
                    if (characterController == null) return;

                    if (characterController.Character.JobId == 1)
                    {
                        Client player = API.getPlayerFromHandle(entity);

                        if (player.hasData("FIRST_OK"))
                        {
                            characterController.Character.Cash += WorkPay.Loader1Pay;
                            ContextFactory.Instance.SaveChanges();
                            API.sendNotificationToPlayer(player, "Вы заработали: " + WorkPay.Loader1Pay + "$");
                            API.triggerClientEvent(player, "update_money_display", characterController.Character.Cash);

                            API.triggerClientEvent(player, "loader_end");
                            API.triggerClientEvent(player, "loader_one", _job1Marker1.X, _job1Marker1.Y, _job1Marker1.Z);
                            API.setPlayerClothes(player, 5, 42, 0);
                            player.resetData("FIRST_OK");
                            player.setData("SECOND_OK", null);
                            API.triggerClientEvent(player, "markonmap", _job1Marker1.X, _job1Marker1.Y);
                        }
                    }                        
                };
            }
            // Loader 2nd place
            if (JobData.Id == 2)
            {                
                _job21MarCol = API.shared.createCylinderColShape(_job2Marker1, 2f, 3f);
                _job22MarCol = API.shared.createCylinderColShape(_job2Marker2, 2f, 3f);

                _job21MarCol.onEntityEnterColShape += (shape, entity) =>
                {                    
                    CharacterController characterController = API.getPlayerFromHandle(entity).getData("CHARACTER");
                    if (characterController == null) return;

                    if (characterController.Character.JobId == 2)
                    {
                        var player = API.getPlayerFromHandle(entity);

                        if (player.hasData("SECOND_OK"))
                        {                            
                            API.triggerClientEvent(player, "loader_end");
                            API.triggerClientEvent(player, "loader_two",
                                _job2Marker2.X, _job2Marker2.Y, _job2Marker2.Z);

                            //API.attachEntityToEntity(box, API.getPlayerFromHandle(entity), "IK_Head", API.getPlayerFromHandle(entity).position, API.getPlayerFromHandle(entity).rotation);

                            API.setPlayerClothes(player, 5, 44, 0);
                            player.resetData("SECOND_OK");
                            player.setData("FIRST_OK", null);
                            API.triggerClientEvent(player, "markonmap", _job2Marker2.X, _job2Marker2.Y);
                        }
                    }
                };
                _job22MarCol.onEntityEnterColShape += (shape, entity) =>
                {
                    CharacterController characterController = API.getPlayerFromHandle(entity).getData("CHARACTER");
                    if (characterController == null) return;

                    if (characterController.Character.JobId == 2)
                    {
                        Client player = API.getPlayerFromHandle(entity);

                        if (player.hasData("FIRST_OK"))
                        {                            
                            characterController.Character.Cash += WorkPay.Loader2Pay;
                            ContextFactory.Instance.SaveChanges();
                            API.sendNotificationToPlayer(player, "Вы заработали: " + WorkPay.Loader2Pay + "$");
                            API.triggerClientEvent(player, "update_money_display", characterController.Character.Cash);

                            API.triggerClientEvent(player, "loader_end");
                            API.triggerClientEvent(player, "loader_one", 
                                _job2Marker1.X, _job2Marker1.Y, _job2Marker1.Z);
                            API.setPlayerClothes(player, 5, 42, 0);
                            player.resetData("FIRST_OK");
                            player.setData("SECOND_OK", null);
                            API.triggerClientEvent(player, "markonmap", _job2Marker1.X, _job2Marker1.Y);
                        }
                    }
                };
            }
            // GasStation_Id3 petrolium
            if (JobData.Id == 3)
            { 
                ColShape gasOneColShape1 = API.shared.createCylinderColShape(GasStation_Id3.Marker1, 2f, 3f);
                API.createMarker(1, GasStation_Id3.Marker1, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape2 = API.shared.createCylinderColShape(GasStation_Id3.Marker2, 2f, 3f);
                API.createMarker(1, GasStation_Id3.Marker2, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape3 = API.shared.createCylinderColShape(GasStation_Id3.Marker3, 2f, 3f);
                API.createMarker(1, GasStation_Id3.Marker3, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape4 = API.shared.createCylinderColShape(GasStation_Id3.Marker4, 2f, 3f);
                API.createMarker(1, GasStation_Id3.Marker4, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape5 = API.shared.createCylinderColShape(GasStation_Id3.Marker5, 2f, 3f);
                API.createMarker(1, GasStation_Id3.Marker5, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape6 = API.shared.createCylinderColShape(GasStation_Id3.Marker6, 2f, 3f);
                API.createMarker(1, GasStation_Id3.Marker6, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);

                var gasOneColShapes = new List<ColShape>
                {gasOneColShape1, gasOneColShape2, gasOneColShape3, gasOneColShape4, gasOneColShape5, gasOneColShape6};

                foreach (var colshape in gasOneColShapes)
                {
                    colshape.onEntityEnterColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle && JobData.CharacterId != 0)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 1, JobData.Id);
                        else API.sendNotificationToPlayer(GetPlayerDriver(entity), "~r~Заправка не работает!");
                    };
                    colshape.onEntityExitColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 0, JobData.Id);
                    };
                }                
            }
            // GasStation_Id4 petrolium
            if (JobData.Id == 4)
            {
                 ColShape gasOneColShape1 = API.shared.createCylinderColShape(GasStation_Id4.Marker1, 2f, 3f);
                 API.createMarker(1, GasStation_Id4.Marker1, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                 ColShape gasOneColShape2 = API.shared.createCylinderColShape(GasStation_Id4.Marker2, 2f, 3f);
                 API.createMarker(1, GasStation_Id4.Marker2, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                 ColShape gasOneColShape3 = API.shared.createCylinderColShape(GasStation_Id4.Marker3, 2f, 3f);
                 API.createMarker(1, GasStation_Id4.Marker3, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                 ColShape gasOneColShape4 = API.shared.createCylinderColShape(GasStation_Id4.Marker4, 2f, 3f);
                 API.createMarker(1, GasStation_Id4.Marker4, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);

                 var gasOneColShapes = new List<ColShape>
                 {gasOneColShape1, gasOneColShape2, gasOneColShape3, gasOneColShape4};

                 foreach (var colshape in gasOneColShapes)
                 {
                      colshape.onEntityEnterColShape += (shape, entity) =>
                      {
                          if (GetPlayerDriver(entity).isInVehicle && JobData.CharacterId != 0)
                              API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 1, JobData.Id);
                          else API.sendNotificationToPlayer(GetPlayerDriver(entity), "~r~Заправка не работает!");
                      };
                      colshape.onEntityExitColShape += (shape, entity) =>
                      {
                          if (GetPlayerDriver(entity).isInVehicle)
                              API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 0, JobData.Id);
                      };
                 }                
            }
            // GasStation_Id5 petrolium
            if (JobData.Id == 5)
            {
                ColShape gasOneColShape1 = API.shared.createCylinderColShape(GasStation_Id5.Marker1, 2f, 3f);
                API.createMarker(1, GasStation_Id5.Marker1, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape2 = API.shared.createCylinderColShape(GasStation_Id5.Marker2, 2f, 3f);
                API.createMarker(1, GasStation_Id5.Marker2, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape3 = API.shared.createCylinderColShape(GasStation_Id5.Marker3, 2f, 3f);
                API.createMarker(1, GasStation_Id5.Marker3, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape4 = API.shared.createCylinderColShape(GasStation_Id5.Marker4, 2f, 3f);
                API.createMarker(1, GasStation_Id5.Marker4, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape5 = API.shared.createCylinderColShape(GasStation_Id5.Marker5, 2f, 3f);
                API.createMarker(1, GasStation_Id5.Marker5, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape6 = API.shared.createCylinderColShape(GasStation_Id5.Marker6, 2f, 3f);
                API.createMarker(1, GasStation_Id5.Marker6, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape7 = API.shared.createCylinderColShape(GasStation_Id5.Marker7, 2f, 3f);
                API.createMarker(1, GasStation_Id5.Marker7, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);

                var gasOneColShapes = new List<ColShape>
                    {gasOneColShape1, gasOneColShape2, gasOneColShape3, gasOneColShape4, gasOneColShape5, gasOneColShape6, gasOneColShape7};

                foreach (var colshape in gasOneColShapes)
                {
                    colshape.onEntityEnterColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle && JobData.CharacterId != 0)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 1, JobData.Id);
                        else API.sendNotificationToPlayer(GetPlayerDriver(entity), "~r~Заправка не работает!");
                    };
                    colshape.onEntityExitColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 0, JobData.Id);
                    };
                }
            }
            // GasStation_Id10 petrolium
            if (JobData.Id == 10)
            {
                ColShape gasOneColShape1 = API.shared.createCylinderColShape(GasStation_Id10.Marker1, 2f, 3f);
                API.createMarker(1, GasStation_Id10.Marker1, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape2 = API.shared.createCylinderColShape(GasStation_Id10.Marker2, 2f, 3f);
                API.createMarker(1, GasStation_Id10.Marker2, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape3 = API.shared.createCylinderColShape(GasStation_Id10.Marker3, 2f, 3f);
                API.createMarker(1, GasStation_Id10.Marker3, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape4 = API.shared.createCylinderColShape(GasStation_Id10.Marker4, 2f, 3f);
                API.createMarker(1, GasStation_Id10.Marker4, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape5 = API.shared.createCylinderColShape(GasStation_Id10.Marker5, 2f, 3f);
                API.createMarker(1, GasStation_Id10.Marker5, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape6 = API.shared.createCylinderColShape(GasStation_Id10.Marker6, 2f, 3f);
                API.createMarker(1, GasStation_Id10.Marker6, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape7 = API.shared.createCylinderColShape(GasStation_Id10.Marker7, 2f, 3f);
                API.createMarker(1, GasStation_Id10.Marker7, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);

                var gasOneColShapes = new List<ColShape>
                    {gasOneColShape1, gasOneColShape2, gasOneColShape3, gasOneColShape4, gasOneColShape5, gasOneColShape6, gasOneColShape7};

                foreach (var colshape in gasOneColShapes)
                {
                    colshape.onEntityEnterColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle && JobData.CharacterId != 0)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 1, JobData.Id);
                        else API.sendNotificationToPlayer(GetPlayerDriver(entity), "~r~Заправка не работает!");
                    };
                    colshape.onEntityExitColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 0, JobData.Id);
                    };
                }
            }
            // GasStation_Id11 petrolium
            if (JobData.Id == 11)
            {
                ColShape gasOneColShape1 = API.shared.createCylinderColShape(GasStation_Id11.Marker1, 2f, 3f);
                API.createMarker(1, GasStation_Id11.Marker1, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape2 = API.shared.createCylinderColShape(GasStation_Id11.Marker2, 2f, 3f);
                API.createMarker(1, GasStation_Id11.Marker2, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape3 = API.shared.createCylinderColShape(GasStation_Id11.Marker3, 2f, 3f);
                API.createMarker(1, GasStation_Id11.Marker3, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape4 = API.shared.createCylinderColShape(GasStation_Id11.Marker4, 2f, 3f);
                API.createMarker(1, GasStation_Id11.Marker4, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape5 = API.shared.createCylinderColShape(GasStation_Id11.Marker5, 2f, 3f);
                API.createMarker(1, GasStation_Id11.Marker5, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape6 = API.shared.createCylinderColShape(GasStation_Id11.Marker6, 2f, 3f);
                API.createMarker(1, GasStation_Id11.Marker6, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                
                var gasOneColShapes = new List<ColShape>
                    {gasOneColShape1, gasOneColShape2, gasOneColShape3, gasOneColShape4, gasOneColShape5, gasOneColShape6};

                foreach (var colshape in gasOneColShapes)
                {
                    colshape.onEntityEnterColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle && JobData.CharacterId != 0)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 1, JobData.Id);
                        else API.sendNotificationToPlayer(GetPlayerDriver(entity), "~r~Заправка не работает!");
                    };
                    colshape.onEntityExitColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 0, JobData.Id);
                    };
                }
            }
            // GasStation_Id12 petrolium
            if (JobData.Id == 12)
            {
                ColShape gasOneColShape1 = API.shared.createCylinderColShape(GasStation_Id12.Marker1, 2f, 3f);
                API.createMarker(1, GasStation_Id12.Marker1, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape2 = API.shared.createCylinderColShape(GasStation_Id12.Marker2, 2f, 3f);
                API.createMarker(1, GasStation_Id12.Marker2, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape3 = API.shared.createCylinderColShape(GasStation_Id12.Marker3, 2f, 3f);
                API.createMarker(1, GasStation_Id12.Marker3, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape4 = API.shared.createCylinderColShape(GasStation_Id12.Marker4, 2f, 3f);
                API.createMarker(1, GasStation_Id12.Marker4, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape5 = API.shared.createCylinderColShape(GasStation_Id12.Marker5, 2f, 3f);
                API.createMarker(1, GasStation_Id12.Marker5, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape6 = API.shared.createCylinderColShape(GasStation_Id12.Marker6, 2f, 3f);
                API.createMarker(1, GasStation_Id12.Marker6, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape7 = API.shared.createCylinderColShape(GasStation_Id12.Marker7, 2f, 3f);
                API.createMarker(1, GasStation_Id12.Marker7, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape8 = API.shared.createCylinderColShape(GasStation_Id12.Marker8, 2f, 3f);
                API.createMarker(1, GasStation_Id12.Marker8, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);

                var gasOneColShapes = new List<ColShape>
                    {gasOneColShape1, gasOneColShape2, gasOneColShape3, gasOneColShape4, gasOneColShape5, gasOneColShape6, gasOneColShape7, gasOneColShape8};

                foreach (var colshape in gasOneColShapes)
                {
                    colshape.onEntityEnterColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle && JobData.CharacterId != 0)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 1, JobData.Id);
                        else API.sendNotificationToPlayer(GetPlayerDriver(entity), "~r~Заправка не работает!");
                    };
                    colshape.onEntityExitColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 0, JobData.Id);
                    };
                }
            }
            // GasStation_Id13 petrolium
            if (JobData.Id == 13)
            {
                ColShape gasOneColShape1 = API.shared.createCylinderColShape(GasStation_Id13.Marker1, 2f, 3f);
                API.createMarker(1, GasStation_Id13.Marker1, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape2 = API.shared.createCylinderColShape(GasStation_Id13.Marker2, 2f, 3f);
                API.createMarker(1, GasStation_Id13.Marker2, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape3 = API.shared.createCylinderColShape(GasStation_Id13.Marker3, 2f, 3f);
                API.createMarker(1, GasStation_Id13.Marker3, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape4 = API.shared.createCylinderColShape(GasStation_Id13.Marker4, 2f, 3f);
                API.createMarker(1, GasStation_Id13.Marker4, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape5 = API.shared.createCylinderColShape(GasStation_Id13.Marker5, 2f, 3f);
                API.createMarker(1, GasStation_Id13.Marker5, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape6 = API.shared.createCylinderColShape(GasStation_Id13.Marker6, 2f, 3f);
                API.createMarker(1, GasStation_Id13.Marker6, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);

                var gasOneColShapes = new List<ColShape>
                    {gasOneColShape1, gasOneColShape2, gasOneColShape3, gasOneColShape4, gasOneColShape5, gasOneColShape6};

                foreach (var colshape in gasOneColShapes)
                {
                    colshape.onEntityEnterColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle && JobData.CharacterId != 0)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 1, JobData.Id);
                        else API.sendNotificationToPlayer(GetPlayerDriver(entity), "~r~Заправка не работает!");
                    };
                    colshape.onEntityExitColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 0, JobData.Id);
                    };
                }
            }
            // GasStation_Id14 petrolium
            if (JobData.Id == 14)
            {
                ColShape gasOneColShape1 = API.shared.createCylinderColShape(GasStation_Id14.Marker1, 2f, 3f);
                API.createMarker(1, GasStation_Id14.Marker1, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape2 = API.shared.createCylinderColShape(GasStation_Id14.Marker2, 2f, 3f);
                API.createMarker(1, GasStation_Id14.Marker2, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape3 = API.shared.createCylinderColShape(GasStation_Id14.Marker3, 2f, 3f);
                API.createMarker(1, GasStation_Id14.Marker3, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                
                var gasOneColShapes = new List<ColShape>
                    {gasOneColShape1, gasOneColShape2, gasOneColShape3};

                foreach (var colshape in gasOneColShapes)
                {
                    colshape.onEntityEnterColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle && JobData.CharacterId != 0)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 1, JobData.Id);
                        else API.sendNotificationToPlayer(GetPlayerDriver(entity), "~r~Заправка не работает!");
                    };
                    colshape.onEntityExitColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 0, JobData.Id);
                    };
                }
            }
            // GasStation_Id15 petrolium
            if (JobData.Id == 15)
            {
                ColShape gasOneColShape1 = API.shared.createCylinderColShape(GasStation_Id15.Marker1, 2f, 3f);
                API.createMarker(1, GasStation_Id15.Marker1, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape2 = API.shared.createCylinderColShape(GasStation_Id15.Marker2, 2f, 3f);
                API.createMarker(1, GasStation_Id15.Marker2, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape3 = API.shared.createCylinderColShape(GasStation_Id15.Marker3, 2f, 3f);
                API.createMarker(1, GasStation_Id15.Marker3, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape4 = API.shared.createCylinderColShape(GasStation_Id15.Marker4, 2f, 3f);
                API.createMarker(1, GasStation_Id15.Marker4, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);

                var gasOneColShapes = new List<ColShape>
                    {gasOneColShape1, gasOneColShape2, gasOneColShape3, gasOneColShape4};

                foreach (var colshape in gasOneColShapes)
                {
                    colshape.onEntityEnterColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle && JobData.CharacterId != 0)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 1, JobData.Id);
                        else API.sendNotificationToPlayer(GetPlayerDriver(entity), "~r~Заправка не работает!");
                    };
                    colshape.onEntityExitColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 0, JobData.Id);
                    };
                }
            }
            // GasStation_Id16 petrolium
            if (JobData.Id == 16)
            {
                ColShape gasOneColShape1 = API.shared.createCylinderColShape(GasStation_Id16.Marker1, 2f, 3f);
                API.createMarker(1, GasStation_Id16.Marker1, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                ColShape gasOneColShape2 = API.shared.createCylinderColShape(GasStation_Id16.Marker2, 2f, 3f);
                API.createMarker(1, GasStation_Id16.Marker2, new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 250, 0, 250, 0);
                
                var gasOneColShapes = new List<ColShape>
                    {gasOneColShape1, gasOneColShape2};

                foreach (var colshape in gasOneColShapes)
                {
                    colshape.onEntityEnterColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle && JobData.CharacterId != 0)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 1, JobData.Id);
                        else API.sendNotificationToPlayer(GetPlayerDriver(entity), "~r~Заправка не работает!");
                    };
                    colshape.onEntityExitColShape += (shape, entity) =>
                    {
                        if (GetPlayerDriver(entity).isInVehicle)
                            API.shared.triggerClientEvent(GetPlayerDriver(entity), "get_petrolium", 0, JobData.Id);
                    };
                }
            }
        }

        private Client GetPlayerDriver (NetHandle entity)
        {
            var playersOcupation = API.getVehicleOccupants(entity);
            var player = API.getPlayerFromHandle(entity);
            foreach (var playerInCar in playersOcupation)
                if (playerInCar.vehicleSeat == -1) player = playerInCar;
            return player;
        }
        
        // BusDriver methods:
        private void BusDriver (NetHandle entity, string startData, string endData,
            float nextX, float nextY, float nextZ, string textStop)
        {
            var playersOcupation = API.getVehicleOccupants(entity);
            var player = API.getPlayerFromHandle(entity);
            foreach (var playerInCar in playersOcupation)
                if (playerInCar.vehicleSeat == -1) player = playerInCar;

            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;

            if (characterController.Character.JobId == JobsIdNonDataBase.BusDriver1 ||
                characterController.Character.JobId == JobsIdNonDataBase.BusDriver2 ||
                characterController.Character.JobId == JobsIdNonDataBase.BusDriver3 ||
                characterController.Character.JobId == JobsIdNonDataBase.BusDriver4)
            {
                if (player.hasData(startData) && player.isInVehicle)
                {
                    if (startData == endData)
                    {
                        var money = 0;
                        switch (characterController.Character.JobId)
                        {
                            case JobsIdNonDataBase.BusDriver1: money = WorkPay.BusDriver1Pay; break;
                            case JobsIdNonDataBase.BusDriver2: money = WorkPay.BusDriver2Pay; break;
                            case JobsIdNonDataBase.BusDriver3: money = WorkPay.BusDriver3Pay; break;
                            case JobsIdNonDataBase.BusDriver4: money = WorkPay.BusDriver4Pay; break;
                        }
                        characterController.Character.Cash += money;
                        ContextFactory.Instance.SaveChanges();
                        API.triggerClientEvent(player, "update_money_display", characterController.Character.Cash);

                        API.triggerClientEvent(player, "bus_end");
                        API.sendNotificationToPlayer(player, "Маршрут окончен: выберите новый на автовокзале.");
                        API.sendNotificationToPlayer(player, "~g~Ваш заработок: " + money + "$");
                        player.resetData(endData);
                    }
                    else
                    {
                        API.triggerClientEvent(player, "bus_end");
                        API.triggerClientEvent(player, "bus_marker", nextX, nextY, nextZ);
                        player.resetData(startData);
                        player.setData(endData, null);
                        API.triggerClientEvent(player, "markonmap", nextX, nextY);
                        API.sendNotificationToPlayer(player, textStop);
                    }                    
                }
            }
        }

        private bool IsPlayerInBus(NetHandle entity)
        {
            var playersInCar = API.getVehicleOccupants(entity);
            var player = API.getPlayerFromHandle(entity);
            foreach (var playerInCar in playersInCar)
                if (playerInCar.vehicleSeat == -1) player = playerInCar;
            if (player == null) return false;

            var vehicle = EntityManager.GetVehicle(entity);
            return vehicle?.VehicleData.Model == -713569950;
        }

        public static void BusDriverStart (Client player, Character character, string startData, int traceNum,
            float firstPosX, float firstPosY, float firstPosZ)
        {
            if (character.Level < 2)
            {
                API.shared.sendNotificationToPlayer(player, "Вы не можете работать на этой работе. Она доступна со 2 уровня");
                return;
            }
            if (!player.hasData(startData))
            {
                switch (traceNum)
                {
                    case 1: character.JobId = JobsIdNonDataBase.BusDriver1; break;
                    case 2: character.JobId = JobsIdNonDataBase.BusDriver2; break;
                    case 3: character.JobId = JobsIdNonDataBase.BusDriver3; break;
                    case 4: character.JobId = JobsIdNonDataBase.BusDriver4; break;
                }
                ContextFactory.Instance.SaveChanges();
                API.shared.triggerClientEvent(player, "bus_marker", firstPosX, firstPosY, firstPosZ);
                API.shared.triggerClientEvent(player, "markonmap", firstPosX, firstPosY);
                player.setData(startData, null);
                var isMale = character.Model == 1885233650;
                ClothesManager.SetPlayerSkinClothes(player, isMale ? 701 : 7010, character, false);
            }
            else API.shared.sendNotificationToPlayer(player, "Вы уже выбрали свой маршрут! Садитесь в автобус.");
        }

        // Taxi Working
        public void UseTaxis(Client player)
        {
            _sen = player;
            _senderxcoords = API.getEntityPosition(player.handle).X;
            _senderycoords = API.getEntityPosition(player.handle).Y;

            foreach (var driver in API.getAllPlayers())
            {
                if (API.getEntityData(driver, "TAXI") != null && API.getEntityData(driver, "TASK") != 1.623482)
                {
                    API.sendPictureNotificationToPlayer(driver, player.name + " вызвал такси, вы заберете его?", "CHAR_TAXI", 0, 1, "Downtown Cab Co.", "Job");
                }
            }
        }
        
        public void Accepted(Client driver, double d)
        {
            foreach (var driver2 in API.getAllPlayers())
            {
                if (API.getEntityData(driver2, "TASK") == d)
                {
                    API.sendChatMessageToPlayer(driver2, "~r~Данный заказ уже был взят");
                    _i = 1;
                }
            }

            if (_i == 0)
            {
                try
                {
                    API.sendChatMessageToPlayer(driver, "~g~Вы согласились с заказом, следуйте по маршруту!");
                    API.triggerClientEvent(driver, "markonmap", _senderxcoords, _senderycoords);
                    API.setEntityData(driver, "TASK", d);
                    API.sendPictureNotificationToPlayer(_sen, driver.name + " выехал за вами, пожалуйста ждите его приезда!", "CHAR_TAXI", 0, 1, "Downtown Cab Co.", "Message");
                }
                catch (Exception e)
                {
                    API.sendNotificationToPlayer(driver, "У вас нет заявок. Ждите заказов.");
                }
            }
        }
        
        public string Type()
        {
            return JobData.Type.GetDisplayName();
        }

        // 24/7 Stores methods:
        public static void AddBuyedClothes(Client player, Character character, int slot, int numCloth, int numDraw)
        {
            var characterWardrobe = ContextFactory.Instance.Wardrobe.FirstOrDefault(x => x.CharacterId == character.Id);
            if (characterWardrobe == null) return;

            List<string> clothesInElement;

            switch (slot)
            {
                case 1: clothesInElement = characterWardrobe.Masks.Split(';').ToList();
                    characterWardrobe.Masks = AddToClothes(characterWardrobe.Masks, clothesInElement, numCloth, numDraw); break;
                case 3: clothesInElement = characterWardrobe.Torsos.Split(';').ToList();
                    characterWardrobe.Torsos = AddToClothes(characterWardrobe.Torsos, clothesInElement, numCloth, numDraw); break;
                case 4: clothesInElement = characterWardrobe.Legs.Split(';').ToList();
                    characterWardrobe.Legs = AddToClothes(characterWardrobe.Legs, clothesInElement, numCloth, numDraw); break;
                case 5: clothesInElement = characterWardrobe.Bags.Split(';').ToList();
                    characterWardrobe.Bags = AddToClothes(characterWardrobe.Bags, clothesInElement, numCloth, numDraw); break;
                case 6: clothesInElement = characterWardrobe.Feets.Split(';').ToList();
                    characterWardrobe.Feets = AddToClothes(characterWardrobe.Feets, clothesInElement, numCloth, numDraw); break;
                case 7: clothesInElement = characterWardrobe.Accesses.Split(';').ToList();
                    characterWardrobe.Accesses = AddToClothes(characterWardrobe.Accesses, clothesInElement, numCloth, numDraw); break;
                case 8: clothesInElement = characterWardrobe.Undershirts.Split(';').ToList();
                    characterWardrobe.Undershirts = AddToClothes(characterWardrobe.Undershirts, clothesInElement, numCloth, numDraw); break;
                case 11: clothesInElement = characterWardrobe.Tops.Split(';').ToList();
                    characterWardrobe.Tops = AddToClothes(characterWardrobe.Tops, clothesInElement, numCloth, numDraw); break;
                case 50: clothesInElement = characterWardrobe.Hats.Split(';').ToList();
                    characterWardrobe.Hats = AddToClothes(characterWardrobe.Hats, clothesInElement, numCloth, numDraw); break;
                case 51: clothesInElement = characterWardrobe.Glasses.Split(';').ToList();
                    characterWardrobe.Glasses = AddToClothes(characterWardrobe.Glasses, clothesInElement, numCloth, numDraw); break;
            }
            ContextFactory.Instance.SaveChanges();
        }
        private static string AddToClothes (string initData, List<string> clothesInElement, int numCloth, int numDraw)
        {
            foreach (var cloth in clothesInElement)
            {
                if (cloth == numCloth + "," + numDraw) return "";
            }

            initData += ";" + numCloth + "," + numDraw;
            return initData;
        }
    }
}
