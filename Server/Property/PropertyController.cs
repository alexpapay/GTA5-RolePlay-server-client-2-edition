using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared.Math;
using MpRpServer.Data;
using MpRpServer.Data.Attributes;
using MpRpServer.Data.Enums;
using MpRpServer.Data.Extensions;
using MpRpServer.Server.Characters;
using MpRpServer.Server.DBManager;
using MpRpServer.Server.Vehicles;
using System;
using System.Linq;
using MpRpServer.Data.Models;

namespace MpRpServer.Server.Property
{
    public class PropertyController : Script
    {
        public Data.Property PropertyData;

        private string ownername = "None";
        public Groups.GroupController GroupController { get; private set; }

        public Marker ExteriorMarker { get; private set; }
        public ColShape ExteriorColShape { get; private set; }

        public Marker InteriorMarker { get; private set; }
        public ColShape InteteriorColShape { get; private set; }
        public Blip Blip { get; private set; }

        // Rent place fields
        public Marker RentPlaceMarker { get; private set; }
        public ColShape RentPlaceColshape { get; private set; }
        
        //Autoschool place fileds
        public Marker AutoschoolMarker { get; private set; }
        public ColShape AutoschoolColshape { get; private set; }

        //Meria place fileds
        public Marker MeriaMarker { get; private set; }
        public ColShape MeriaColshape { get; private set; }

        //Armys place fileds
        public Marker ArmysMarker { get; private set; }
        public ColShape ArmysColshapes { get; private set; }
        public ColShape ArmysGangColshape { get; private set; }
        public ColShape ArmyOneSourceColshape { get; private set; }
        public ColShape ArmysStocksColshape { get; private set; }

        //Gangs place fileds
        public Marker GangsMarker           { get; private set; }
        public ColShape GangsMainColshape   { get; private set; } // 2f
        public ColShape GangsStockColshape  { get; private set; } // 3f

        //Mafia place fileds
        public Marker MafiaMarker { get; private set; }
        public ColShape MafiaMainColshape { get; private set; } // 2f
        public ColShape MafiaStockColshape { get; private set; } // 3f

        //Police place fileds
        public Marker PoliceMarker          { get; private set; }
        public ColShape PoliceMainColshape      { get; private set; }
        public ColShape PoliceStockColshape { get; private set; }

        //FBI place fileds
        public Marker FbiMarker { get; private set; }
        public ColShape FbiMainColshape { get; private set; }
        public ColShape FbiStockColshape { get; private set; }

        //Emergency place fileds
        public Marker EmergencyMarker { get; private set; }
        public ColShape EmergencyColshape { get; private set; }

        public PropertyController() { }
        public PropertyController(Data.Property propertyData)
        {
            PropertyData = propertyData;
        }
        public static void LoadProperties()
        {
            foreach (var property in ContextFactory.Instance.Property.ToList())
            {
                var propertyController = new PropertyController(property);
                if (property.Group != null)
                {
                    propertyController.GroupController = EntityManager.GetGroup(property.Group.Id);
                    API.shared.consoleOutput("Загружен маркер номер " + property.PropertyID + " для фракции : " + propertyController.GroupController.Group.Name);
                    if (propertyController.GroupController != null)
                    {
                        var name = property.Group.Name;
                        propertyController.ownername = name == null ? "None" : property.Name;
                    }
                }
                else if (property.Character != null)
                {
                    var name = property.Character.Name;
                    propertyController.ownername = name?.Replace("_", " ") ?? "None";
                }

                propertyController.CreateWorldEntity();
                EntityManager.Add(propertyController);
            }
            API.shared.consoleOutput("[GM] Загружено маркеров: " + ContextFactory.Instance.Property.Count() + " шт.");
        }

        public void CreateWorldEntity()
        {
            if (PropertyData.Type == PropertyType.Invalid)
            {                
                ExteriorMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                                new Vector3(1f, 1f, 1f), 150, 255, 255, 0);
                var propertyName = "";
                switch (PropertyData.Name)
                {
                    case "Roof": propertyName = "Внутрь/На крышу"; break;
                    case "Meria_enter": propertyName = "Мэрия";
                        Blip = API.createBlip(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 50.0f);
                        Blip.shortRange = true;
                        Blip.sprite = 419;
                        Blip.name = "Мэрия"; break;
                }
                API.createTextLabel("~g~Вход в: " + propertyName, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);

                InteriorMarker = API.shared.createMarker(1, new Vector3(PropertyData.IntPosX, PropertyData.IntPosY, PropertyData.IntPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                                new Vector3(1f, 1f, 1f), 150, 255, 255, 0);
                API.createTextLabel("~w~Выход из: " + propertyName, new Vector3(PropertyData.IntPosX, PropertyData.IntPosY, PropertyData.IntPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
            }            
            if (PropertyData.Type == PropertyType.House)
            {
                var owner = ContextFactory.Instance.Character.FirstOrDefault(x => x.Id == PropertyData.CharacterId);
                
                ExteriorMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                    new Vector3(1f, 1f, 1f), 150, 255, 255, 50);
                
                if (owner == null)
                {
                    var cost = PropertyData.Stock;
                    API.createTextLabel("~g~Купить дом №"+ PropertyData.PropertyID +".\nСтоимость: " + cost + "$", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                }
                else
                {                    
                    API.createTextLabel("~g~Вход в дом №" + PropertyData.PropertyID + ".\nВладелец: " + owner.Name, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                }

                InteriorMarker = API.shared.createMarker(1, new Vector3(PropertyData.IntPosX, PropertyData.IntPosY, PropertyData.IntPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                                new Vector3(1f, 1f, 1f), 150, 255, 255, 0);
                API.createTextLabel("~w~Выход из дома.", new Vector3(PropertyData.IntPosX, PropertyData.IntPosY, PropertyData.IntPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);

                Blip = API.createBlip(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 50.0f);
                Blip.shortRange = true;
                Blip.sprite = 40;
                Blip.color = PropertyData.CharacterId == null ? 2 : 1;
                Blip.name = "Жилой дом";
            }
            if (PropertyData.Type == PropertyType.Rent)
            {
                switch (PropertyData.Name)
                {
                    case "Rent_scooter":
                        RentPlaceMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 150, 0, 255, 0);
                        CreateTextBlip(PropertyData, "~w~Прокат мопеда.\nВсего 30$ за полчаса", "Прокат мопеда", 512);
                        break;
                }
            }
            if (PropertyData.Type == PropertyType.Autoschool)
            {
                switch (PropertyData.Name)
                {
                    case "Autoschool_main":
                        AutoschoolMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 250, 25, 50, 200);
                        CreateTextBlip(PropertyData, "~w~Автошкола:\nГлавная", "Автошкола", 76);
                        break;
                }
            }
            if (PropertyData.Type == PropertyType.Meria)
            {
                switch (PropertyData.Name)
                {
                    case "Meria_main":
                        MeriaMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 250, 25, 50, 200);
                        //CreateTextBlip(PropertyData, "~w~Мэрия:\nГлавная", "Мэрия", 419);
                        break;
                    case "Meria_work":
                        MeriaMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Мэрия:\nДля сотрудников", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }
            if (PropertyData.Type == PropertyType.ArmyOne)
            {
                switch (PropertyData.Name)
                {
                    case "Army1_main":
                        ArmysMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Армия 1\nГлавный маркер", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;

                    case "Army1_source":
                        ArmysMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(15f, 15f, 15f), 50, 100, 40, 0);
                        CreateTextBlip(PropertyData, "~w~Армия 1\nМатериалы", "Армия 1 : Исходные материалы", 481);
                        break;

                    case "Army1_stock":
                        ArmysMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(10f, 10f, 10f), 100, 0, 80, 0);
                        CreateTextBlip(PropertyData, "~w~Армия 1\nГлавный склад", "Армия 1 : Главный склад", 421);
                        break;

                    case "Army1_weapon":
                        ArmysMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 125, 0, 100, 0);
                        CreateTextBlip(PropertyData, "~w~Армия 1\nОружие", "Армия 1 : Оружие", 150);
                        break;

                    case "Army1_gang":
                        ArmysMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 60, 255, 0, 0);
                        API.createTextLabel("~w~Армия 1\nЛичка бандита", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                    case "Army_fuel":
                        ArmysMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(10f, 10f, 10f), 40, 150, 0, 150);
                        API.createTextLabel("~w~Армия\nЗаправка авиатранспорта", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }
            if (PropertyData.Type == PropertyType.ArmyTwo)
            {
                switch (PropertyData.Name)
                {
                    case "Army2_main":
                        ArmysMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Армия 2\nГлавный маркер", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;

                    case "Army2_stock":
                        ArmysMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(10f, 10f, 10f), 100, 0, 80, 0);
                        CreateTextBlip(PropertyData, "~w~Армия 2\nГлавный склад", "Армия 2 : Главный склад", 421);
                        break;

                    case "Army2_weapon":
                        ArmysMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 125, 0, 100, 0);
                        CreateTextBlip(PropertyData, "~w~Армия 2\nОружие", "Армия 2 : Оружие", 150);
                        break;

                    case "Army2_gang":
                        ArmysMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 60, 255, 0, 0);
                        API.createTextLabel("~w~Армия 2\nЛичка бандита", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }
            if (PropertyData.Type == PropertyType.Gangs)
            {                
                if (PropertyData.Name == "Gangs_metall")
                {
                    GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                                                        new Vector3(1f, 1f, 1f), 70, 0, 100, 153);
                    CreateTextBlip(PropertyData, "~w~Сдача металла", "Сдача металла", 478);
                }
            }
            if (PropertyData.Type == PropertyType.GangBallas)
            {
                switch (PropertyData.Name)
                {
                    case "Ballas_main":
                        GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 250, 153, 0, 153);
                        CreateTextBlip(PropertyData, "~w~Балласы\nГлавная", "Балласы : Главная", 106);
                        break;
                    case "Ballas_stock":
                        GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Балласы\nСклад", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            } // 13
            if (PropertyData.Type == PropertyType.GangAzcas)
            {
                switch (PropertyData.Name)
                {
                    case "Azcas_main":
                        GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 250, 9, 15, 70);
                        CreateTextBlip(PropertyData, "~w~Azcas\nГлавная", "Azcas : Главная", 76);
                        break;
                    case "Azcas_stock":
                        GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Azcas\nСклад", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }  // 14
            if (PropertyData.Type == PropertyType.GangVagos)
            {
                switch (PropertyData.Name)
                {
                    case "Vagos_main":
                        GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 250, 100, 100, 0);
                        CreateTextBlip(PropertyData, "~w~Vagos\nГлавная", "Vagos : Главная", 120);
                        break;
                    case "Vagos_stock":
                        GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Vagos\nСклад", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }  // 15
            if (PropertyData.Type == PropertyType.GangGrove)
            {
                switch (PropertyData.Name)
                {
                    case "Grove_main":
                        GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 250, 0, 80, 0);
                        CreateTextBlip(PropertyData, "~w~Grove\nГлавная", "Grove : Главная", 77);
                        break;
                    case "Grove_stock":
                        GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Grove\nСклад", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }  // 16
            if (PropertyData.Type == PropertyType.GangRifa)
            {
                switch (PropertyData.Name)
                {
                    case "Rifa_main":
                        GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 250, 0, 100, 100);
                        CreateTextBlip(PropertyData, "~w~Rifa\nГлавная", "Rifa : Главная", 88);
                        break;
                    case "Rifa_stock":
                        GangsMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Rifa\nСклад", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }   // 17
            if (PropertyData.Type == PropertyType.RussianMafia)
            {
                switch (PropertyData.Name)
                {
                    case "RussianMafia_main":
                        MafiaMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 250, 0, 100, 100);
                        CreateTextBlip(PropertyData, "~w~Russian Mafia\nГлавная", "Russian Mafia : Главная", 78);
                        break;
                    case "RussianMafia_stock":
                        MafiaMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Russian Mafia\nСклад", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }   // 30
            if (PropertyData.Type == PropertyType.MafiaLKN)
            {
                switch (PropertyData.Name)
                {
                    case "MafiaLKN_main":
                        MafiaMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 250, 0, 100, 100);
                        CreateTextBlip(PropertyData, "~w~Mafia LKN\nГлавная", "Mafia LKN : Главная", 78);
                        break;
                    case "MafiaLKN_stock":
                        MafiaMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Mafia LKN\nСклад", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }       // 31
            if (PropertyData.Type == PropertyType.MafiaArmeny)
            {
                switch (PropertyData.Name)
                {
                    case "MafiaArmeny_main":
                        MafiaMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 250, 0, 100, 100);
                        CreateTextBlip(PropertyData, "~w~Mafia Armeny\nГлавная", "Mafia Armeny : Главная", 78);
                        break;
                    case "MafiaArmeny_stock":
                        MafiaMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 250, 25, 50, 200);
                        API.createTextLabel("~w~Mafia Armeny\nСклад", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }    // 32
            if (PropertyData.Type == PropertyType.Police)
            {
                switch (PropertyData.Name)
                {
                    case "Police_stock":
                        PoliceMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 150, 0, 0, 255);
                        break;

                    case "Police_main":
                        PoliceMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 100, 0, 0, 255);
                        API.createTextLabel("~w~Полиция\nГлавная", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        CreateTextBlip(PropertyData, "~w~Полиция\nГлавная", "Полиция : Главная", 60);
                        break;

                    case "Police_weapon":
                        PoliceMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 80, 0, 0, 255);
                        API.createTextLabel("~w~Полиция\nОружие", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }
            if (PropertyData.Type == PropertyType.FBI)
            {
                switch (PropertyData.Name)
                {
                    case "FBI_main":
                        FbiMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 150, 0, 255, 255);
                        CreateTextBlip(PropertyData, "~w~FBI\nГлавная", "FBI : Главная", 60);
                        break;

                    case "FBI_weapon":
                        FbiMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(1f, 1f, 1f), 100, 0, 255, 255);
                        API.createTextLabel("~w~FBI\nОружие", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;

                    case "FBI_stock":
                        FbiMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(3f, 3f, 3f), 120, 0, 255, 255);
                        API.createTextLabel("~w~FBI\nСклад", new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
                        break;
                }
            }
            if (PropertyData.Type == PropertyType.Emergency)
            {
                switch (PropertyData.Name)
                {
                    case "Emergency_main":
                        EmergencyMarker = API.shared.createMarker(1, new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ) - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                            new Vector3(2f, 2f, 2f), 250, 255, 10, 10);
                        CreateTextBlip(PropertyData, "~w~Emergency\nГлавная", "Emergency : Главная", 51);
                        break;
                }
            }

            CreateColShape();
        }

        public void CreateColShape()
        {
            ExteriorColShape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 1f, 1f);
            ExteriorColShape.onEntityEnterColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    CharacterController characterController = API.getPlayerFromHandle(entity).getData("CHARACTER");
                    if (PropertyData.Name == "House")
                    {
                        // It`s your house
                        if (PropertyData.Enterable && PropertyData.CharacterId == characterController.Character.Id)
                        {
                            API.shared.sendNotificationToPlayer(API.getPlayerFromHandle(entity), "Вы можете зайти сюда.\nНажмите N для входа.");
                            API.getPlayerFromHandle(entity).setData("AT_PROPERTY", this);
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), 
                                "house_menu", PropertyData.PropertyID, PropertyData.Stock, 1, 0);
                        }
                        // House for sale
                        if (PropertyData.CharacterId == null)
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), 
                                "house_menu", PropertyData.PropertyID, PropertyData.Stock, 1, 1);
                        //  Steal from the house
                        if (PropertyData.CharacterId != null && 
                        CharacterController.IsCharacterInGang(characterController) &&
                        PropertyData.Enterable && PropertyData.CharacterId != characterController.Character.Id)
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), 
                                "house_menu", PropertyData.PropertyID, PropertyData.Stock, 1, 2, PropertyData.CharacterId);
                    }
                    if (PropertyData.Name == "Roof")
                    {
                        API.shared.sendNotificationToPlayer(API.getPlayerFromHandle(entity), "Вы можете зайти сюда.\nНажмите N для входа.");
                        API.getPlayerFromHandle(entity).setData("AT_PROPERTY", this);
                    }
                    if (PropertyData.Name == "Meria_enter")
                    {
                        API.shared.sendNotificationToPlayer(API.getPlayerFromHandle(entity), "Вы можете зайти сюда.\nНажмите N для входа.");
                        API.getPlayerFromHandle(entity).setData("AT_PROPERTY", this);
                    }
                }                
            };
            ExteriorColShape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Name == "House")
                    {
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity),
                            "house_menu", PropertyData.PropertyID, PropertyData.Stock, 0, 0, 0);
                    }
                    
                    if (PropertyData.Enterable) API.getPlayerFromHandle(entity).resetData("AT_PROPERTY");
                }
            };

            InteteriorColShape = API.createCylinderColShape(new Vector3(PropertyData.IntPosX, PropertyData.IntPosY, PropertyData.IntPosZ), 1f, 1f);
            InteteriorColShape.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    if (PropertyData.IntPosZ == 24.5378f && player.getData("NEEDHEAL") == true)
                    {
                        API.sendNotificationToPlayer(player, "~r~Вы еще не выздоровели. Подлечитесь!");
                        return;
                    }
                    if (PropertyData.Enterable)
                    {
                        API.shared.sendNotificationToPlayer(player, "Выйти отсюда.\nНажмите N для выхода.");
                        player.setData("AT_PROPERTY", this);
                    }
                }
            };
            InteteriorColShape.onEntityExitColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) == null) return;
                if (PropertyData.Enterable) player.resetData("AT_PROPERTY");
            };

            RentPlaceColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 2f, 3f);
            RentPlaceColshape.onEntityEnterColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Name == "Rent_scooter")                     
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "scooter_rent_menu", 1, PropertyData.Name);
                }
            };
            RentPlaceColshape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) == null) return;
                if (PropertyData.Name == "Rent_scooter")
                    API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "scooter_rent_menu", 0, PropertyData.Name);
            };
            
            AutoschoolColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 2f, 3f);
            AutoschoolColshape.onEntityEnterColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Name == "Autoschool_main")
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "autoschool_menu", 1, PropertyData.Name);
                }
            };
            AutoschoolColshape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) == null) return;
                if (PropertyData.Name == "Autoschool_main")
                    API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "autoschool_menu", 0, PropertyData.Name);
            };
            
            MeriaColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 2f, 3f);
            MeriaColshape.onEntityEnterColShape += (shape, entity) =>
            {
                var player = API.getPlayerFromHandle(entity);

                if (player != null)
                {
                    CharacterController characterController = player.getData("CHARACTER");
                    if (characterController == null) return;
                    if (PropertyData.Name == "Meria_main")
                    {
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_meria_menu", 1, characterController.Character.Level, PropertyData.Name);
                    }
                    if (PropertyData.Name == "Meria_work" && characterController.Character.GroupType == 11)
                    {
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "meria_workers", 1, PropertyData.Name);
                    }
                }
            };
            MeriaColshape.onEntityExitColShape += (shape, entity) =>
            {
                var player = API.getPlayerFromHandle(entity);
                
                if (player != null)
                {
                    CharacterController characterController = player.getData("CHARACTER");
                    if (characterController == null) return;
                    if (PropertyData.Name == "Meria_main")
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "work_meria_menu", 0, 0, PropertyData.Name);
                    if (PropertyData.Name == "Meria_work" && characterController.Character.GroupType == 11)
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "meria_workers", 0, PropertyData.Name);
                }
            };

            // ARMY Stocks both (10.0f):
            ArmysStocksColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 10f, 5f);
            ArmysStocksColshape.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    try
                    {
                        CharacterController characterController = player.getData("CHARACTER");
                        var character = characterController.Character;

                        if (PropertyData.Type == PropertyType.ArmyOne || PropertyData.Type == PropertyType.ArmyTwo)
                        {
                            if (PropertyData.Name == "Army1_stock" && character.GroupType == 21 && player.isInVehicle)
                                API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 1, PropertyData.Name, character.GroupType, PropertyData.Stock);

                            if (PropertyData.Name == "Army2_stock" && character.GroupType == 20 && player.isInVehicle)
                                API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 1, PropertyData.Name, character.GroupType, PropertyData.Stock);

                            if (PropertyData.Name == "Army2_stock" && character.GroupType == 21 && player.isInVehicle)
                                API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 1, PropertyData.Name, character.GroupType, PropertyData.Stock);

                            if (PropertyData.Name == "Army1_stock" || PropertyData.Name == "Army2_stock")
                                if (CharacterController.IsCharacterInGang(character) && !player.isInVehicle)
                                    API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, PropertyData.Name);

                            if (player.isInVehicle && PropertyData.Name == "Army_fuel")
                            {
                                var playerVehicleController = EntityManager.GetVehicle(player.vehicle);
                                if (playerVehicleController.VehicleData.Model == -1281684762 ||
                                    playerVehicleController.VehicleData.Model == 782665360 ||
                                    playerVehicleController.VehicleData.Model == -1600252419 ||
                                    playerVehicleController.VehicleData.Model == 1394036463)
                                {
                                    playerVehicleController.VehicleData.Fuel =
                                        FuelByType.GetFuel(playerVehicleController.VehicleData.Model);
                                    ContextFactory.Instance.SaveChanges();
                                    API.sendNotificationToPlayer(player, "~g~ Вы успешно заправили полный бак.");
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            };
            ArmysStocksColshape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Name == "Army2_stock" || PropertyData.Name == "Army1_stock")
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 0, PropertyData.Name, 0, 0);
                    if (PropertyData.Name == "Army1_stock" || PropertyData.Name == "Army2_stock")
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 0, PropertyData.Name);
                }
            };

            // ARMY ONE Source loading (10.0f):
            ArmyOneSourceColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 10f, 5f);
            ArmyOneSourceColshape.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    if (PropertyData.Type == PropertyType.ArmyOne)
                    {
                        CharacterController characterController = player.getData("CHARACTER");
                        var character = characterController.Character;

                        if (PropertyData.Name == "Army1_source" && character.GroupType == 20 && player.isInVehicle)
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 1, PropertyData.Name, character.GroupType);
                    } 
                }
            };
            ArmyOneSourceColshape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Name == "Army1_source")
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 0, PropertyData.Name, 0);                    
                }
            };

            // ARMYs all colshapes (2.0f):
            ArmysColshapes = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 2f, 2f);
            ArmysColshapes.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    if (PropertyData.Type == PropertyType.ArmyOne || PropertyData.Type == PropertyType.ArmyTwo)
                    {
                        CharacterController characterController = player.getData("CHARACTER");
                        Character character = characterController.Character;

                        if (PropertyData.Name == "Army1_main" && character.ActiveGroupID > 2002 && character.ActiveGroupID <= 2015)
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 1, PropertyData.Name, character.GroupType);
                        if (PropertyData.Name == "Army2_main" && character.ActiveGroupID > 2102 && character.ActiveGroupID <= 2115)
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 1, PropertyData.Name, character.GroupType);
                        
                        if (PropertyData.Name == "Army1_weapon" && character.GroupType == 20)
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 1, PropertyData.Name, character.GroupType);
                        if (PropertyData.Name == "Army2_weapon" && character.GroupType == 21)
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 1, PropertyData.Name, character.GroupType);
                    }
                }
            };
            ArmysColshapes.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Type == PropertyType.ArmyOne || PropertyData.Type == PropertyType.ArmyTwo)
                    {
                        if (PropertyData.Name == "Army1_main" || PropertyData.Name == "Army2_main")
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 0, PropertyData.Name, 0);
                        if (PropertyData.Name == "Army1_weapon" || PropertyData.Name == "Army2_weapon")
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 0, PropertyData.Name, 0);
                    }
                }
            };

            // Army One shapes:
            ArmysGangColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 1f, 2f);
            ArmysGangColshape.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    CharacterController characterController = player.getData("CHARACTER");

                    // GANG STEaLING:
                    if (PropertyData.Name == "Army1_gang" || PropertyData.Name == "Army2_gang")
                        if (CharacterController.IsCharacterInGang(characterController))
                        {
                            Data.Property currentStock = new Data.Property();
                            switch (PropertyData.Name)
                            {
                                case "Army1_gang": currentStock = ContextFactory.Instance.Property.First(x => x.Name == "Army1_stock"); break;
                                case "Army2_gang": currentStock = ContextFactory.Instance.Property.First(x => x.Name == "Army2_stock"); break;
                            }
                            
                            if (currentStock.Stock - 500 < 0)
                                API.sendNotificationToPlayer(player, "~r~Вы не можете украсть! На данном складе нет материалов!");
                            else
                            {
                                if (characterController.Character.Material >= 500)
                                    API.sendNotificationToPlayer(player, "~r~Вы не можете украсть! Вы перегружены у вас: " + characterController.Character.Material + " материалов");
                                else
                                {
                                    currentStock.Stock -= 500;
                                    characterController.Character.Material = 500;
                                    ContextFactory.Instance.SaveChanges();
                                    API.sendNotificationToPlayer(player, "~g~Вы украли 500 материалов со склада!");
                                }
                            }
                        }                        
                }
            };

            // For all Gangs, MAIN (2f dimension):
            GangsMainColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 2f, 2f);
            GangsMainColshape.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    CharacterController characterController = player.getData("CHARACTER");
                    var character = characterController.Character;
                    var gangRank = characterController.Character.ActiveGroupID - characterController.Character.GroupType * 100;
                    var moneyInGang = new Group();
                    var moneyBankGroup = characterController.Character.GroupType * 100;
                    try { moneyInGang = ContextFactory.Instance.Group.First(x => x.Id == moneyBankGroup); }
                    catch (Exception)
                    {
                        // ignored
                    }

                    switch (PropertyData.Type)
                    {
                        case PropertyType.Gangs:
                            if (PropertyData.Name == "Gangs_metall" &&
                                CharacterController.IsCharacterInGang(characterController) &&
                                characterController.Character.TempVar == 111)                                
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, 
                                        PropertyData.Name, characterController.Character.ClothesTypes, 0, gangRank, moneyInGang.MoneyBank);
                            else    API.sendNotificationToPlayer(API.getPlayerFromHandle(entity), "~r~У вас нет металла для сдачи!"); break;

                        case PropertyType.GangBallas:                            
                            if (PropertyData.Name == "Ballas_main" && character.GroupType == 13)
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, 
                                        PropertyData.Name, characterController.Character.ClothesTypes,
                                        ContextFactory.Instance.Property.First(x => x.Name == "Ballas_stock").Stock,
                                        gangRank, moneyInGang.MoneyBank);
                            break;
                        case PropertyType.GangAzcas:
                            if (PropertyData.Name == "Azcas_main" && character.GroupType == 14)
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, 
                                        PropertyData.Name, characterController.Character.ClothesTypes,
                                        ContextFactory.Instance.Property.First(x => x.Name == "Azcas_stock").Stock, 
                                        gangRank, moneyInGang.MoneyBank);
                            break;
                        case PropertyType.GangVagos:
                            if (PropertyData.Name == "Vagos_main" && character.GroupType == 15)
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, 
                                        PropertyData.Name, characterController.Character.ClothesTypes,
                                        ContextFactory.Instance.Property.First(x => x.Name == "Vagos_stock").Stock,
                                        gangRank, moneyInGang.MoneyBank);
                            break;
                        case PropertyType.GangGrove:
                            if (PropertyData.Name == "Grove_main" && character.GroupType == 16)
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, 
                                        PropertyData.Name, characterController.Character.ClothesTypes,
                                        ContextFactory.Instance.Property.First(x => x.Name == "Grove_stock").Stock,
                                        gangRank, moneyInGang.MoneyBank);
                            break;
                        case PropertyType.GangRifa:
                            if (PropertyData.Name == "Rifa_main" && character.GroupType == 17)
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, 
                                        PropertyData.Name, characterController.Character.ClothesTypes,
                                        ContextFactory.Instance.Property.First(x => x.Name == "Rifa_stock").Stock, 
                                        gangRank, moneyInGang.MoneyBank);
                            break;
                    }                    
                }
            };
            GangsMainColshape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Name == "Gangs_metall" || PropertyData.Name == "Ballas_main" ||
                    PropertyData.Name == "Azcas_main" || PropertyData.Name == "Vagos_main" ||
                    PropertyData.Name == "Grove_main" || PropertyData.Name == "Rifa_main")
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 0, PropertyData.Name, 0, 0, 0, 0);
                }
            };

            // For all Gangs, STOCK (3f dimension):
            GangsStockColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 3f, 3f);
            GangsStockColshape.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    CharacterController characterController = player.getData("CHARACTER");
                    Character character = characterController.Character;

                    switch (PropertyData.Type)
                    {
                        case PropertyType.GangBallas:
                            if (PropertyData.Name == "Ballas_stock" && character.GroupType == 13 && 
                                player.isInVehicle)
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, PropertyData.Name);
                            break;
                        case PropertyType.GangAzcas:
                            if (PropertyData.Name == "Azcas_stock" && character.GroupType == 14 && 
                                player.isInVehicle)
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, PropertyData.Name);
                            break;
                        case PropertyType.GangVagos:
                            if (PropertyData.Name == "Vagos_stock" && character.GroupType == 15 && 
                                player.isInVehicle)
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, PropertyData.Name);
                            break;
                        case PropertyType.GangGrove:
                            if (PropertyData.Name == "Grove_stock" && character.GroupType == 16 && 
                                player.isInVehicle)
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, PropertyData.Name);
                            break;
                        case PropertyType.GangRifa:
                            if (PropertyData.Name == "Rifa_stock" && character.GroupType == 17 && 
                                player.isInVehicle)
                                    API.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 1, PropertyData.Name);
                            break;
                    }
                }
            };
            GangsStockColshape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Name == "Ballas_stock" || PropertyData.Name == "Azcas_stock" ||
                    PropertyData.Name == "Vagos_stock" || PropertyData.Name == "Grove_stock" || PropertyData.Name == "Rifa_stock")
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "gang_menu", 0, PropertyData.Name);
                }
            };

            // For all Mafias, MAIN (2f dimension):
            MafiaMainColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 2f, 2f);
            MafiaMainColshape.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    CharacterController characterController = player.getData("CHARACTER");
                    Character character = characterController.Character;

                    switch (PropertyData.Type)
                    {                        
                        case PropertyType.RussianMafia:
                            if (PropertyData.Name == "RussianMafia_main" && character.GroupType == 30)
                                API.triggerClientEvent(API.getPlayerFromHandle(entity), "mafia_menu", 1, PropertyData.Name);
                            break;
                        case PropertyType.MafiaLKN:
                            if (PropertyData.Name == "MafiaLKN_main" && character.GroupType == 31)
                                API.triggerClientEvent(API.getPlayerFromHandle(entity), "mafia_menu", 1, PropertyData.Name);
                            break;
                        case PropertyType.MafiaArmeny:
                            if (PropertyData.Name == "MafiaArmeny_main" && character.GroupType == 32)
                                API.triggerClientEvent(API.getPlayerFromHandle(entity), "mafia_menu", 1, PropertyData.Name);
                            break;                        
                    }
                }
            };
            MafiaMainColshape.onEntityExitColShape += (shape, entity) =>
            {
                var player = API.getPlayerFromHandle(entity);
                if (player != null)
                {
                    if (PropertyData.Name == "RussianMafia_main" || 
                        PropertyData.Name == "MafiaLKN_main" || 
                        PropertyData.Name == "MafiaArmeny_main")
                        API.shared.triggerClientEvent(player, "mafia_menu", 0, PropertyData.Name);
                }
            };

            // For all Mafias, MAIN (3f dimension):
            MafiaStockColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 3f, 3f);
            MafiaStockColshape.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    CharacterController characterController = player.getData("CHARACTER");
                    Character character = characterController.Character;
                    var mafiaRank = Convert.ToInt32(characterController.Character.ActiveGroupID) - characterController.Character.GroupType * 100;

                    switch (PropertyData.Type)
                    {
                        case PropertyType.RussianMafia:
                            if (PropertyData.Name == "RussianMafia_stock" && character.GroupType == 30)
                                API.triggerClientEvent(API.getPlayerFromHandle(entity), "mafia_menu", 1, 
                                    PropertyData.Name, mafiaRank, CharacterController.IsCharacterInMafia(character));
                            break;
                        case PropertyType.MafiaLKN:
                            if (PropertyData.Name == "MafiaLKN_stock" && character.GroupType == 31)
                                API.triggerClientEvent(API.getPlayerFromHandle(entity), "mafia_menu", 1,
                                    PropertyData.Name, mafiaRank, CharacterController.IsCharacterInMafia(character));
                            break;
                        case PropertyType.MafiaArmeny:
                            if (PropertyData.Name == "MafiaArmeny_stock" && character.GroupType == 32)
                                API.triggerClientEvent(API.getPlayerFromHandle(entity), "mafia_menu", 1,
                                    PropertyData.Name, mafiaRank, CharacterController.IsCharacterInMafia(character));
                            break;
                    }
                }
            };
            MafiaStockColshape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Name == "RussianMafia_stock" || PropertyData.Name == "MafiaLKN_stock"
                    || PropertyData.Name == "MafiaArmeny_stock")
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "mafia_menu", 0, PropertyData.Name);
                }
            };

            // Police main (1f dimension)
            PoliceMainColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 1f, 1f);
            PoliceMainColshape.onEntityEnterColShape += (shape, entity) =>
            {
                var player= API.getPlayerFromHandle(entity);
                if (player != null)
                {
                    if (PropertyData.Type == PropertyType.Police)
                    {
                        CharacterController characterController = player.getData("CHARACTER");

                        if (PropertyData.Name == "Police_main" && CharacterController.IsCharacterInPolice(characterController))
                            API.shared.triggerClientEvent(player, "police_menu", 1, PropertyData.Name, 1);

                        if (PropertyData.Name == "Police_main" && CharacterController.IsCharacterInFbi(characterController))
                            API.shared.triggerClientEvent(player, "fbi_menu", 1, PropertyData.Name, 1);

                        if (PropertyData.Name == "Police_weapon" &&
                            CharacterController.IsCharacterInPolice(characterController))
                        {
                            var policeStock = ContextFactory.Instance.Property.FirstOrDefault(x => x.Name == "Police_stock");
                            if (policeStock == null) return;
                            API.shared.triggerClientEvent(player, "police_menu", 1, PropertyData.Name, 1, policeStock.Stock);
                        }
                    }
                }
            };
            PoliceMainColshape.onEntityExitColShape += (shape, entity) =>
            {
                var player = API.getPlayerFromHandle(entity);
                if (player == null) return;

                CharacterController characterController = API.getPlayerFromHandle(entity).getData("CHARACTER");
                if (characterController == null) return;
                
                    if (PropertyData.Name == "Police_main" && CharacterController.IsCharacterInFbi(characterController))
                        API.shared.triggerClientEvent(player, "fbi_menu", 0, PropertyData.Name, 1);

                    if (PropertyData.Name == "Police_main")
                        API.shared.triggerClientEvent(player, "police_menu", 0, PropertyData.Name, 0, 0);
            };

            // Police stock (3f dimension)
            PoliceStockColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 3f, 3f);
            PoliceStockColshape.onEntityEnterColShape += (shape, entity) =>
            {
                var player = API.getPlayerFromHandle(entity);
                if (player != null)
                {
                    if (PropertyData.Type == PropertyType.Police)
                    {
                        CharacterController characterController = player.getData("CHARACTER");

                        if (PropertyData.Name == "Police_stock" &&
                        CharacterController.IsCharacterInArmy(characterController) && player.isInVehicle)
                            API.shared.triggerClientEvent(player, "army_menu", 1, PropertyData.Name, characterController.Character.GroupType);
                    }
                }
            };
            PoliceStockColshape.onEntityExitColShape += (shape, entity) =>
            {
                var player = API.getPlayerFromHandle(entity);
                if (player == null) return;
                if (PropertyData.Name == "Police_stock")
                    API.shared.triggerClientEvent(player, "army_menu", 0, PropertyData.Name, 0);
            };

            // FBI main (1f dimension)
            FbiMainColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 1f, 1f);
            FbiMainColshape.onEntityEnterColShape += (shape, entity) =>
            {
                var player = API.getPlayerFromHandle(entity);
                if (player != null)
                {
                    if (PropertyData.Type == PropertyType.FBI)
                    {
                        CharacterController characterController = player.getData("CHARACTER");

                        if (PropertyData.Name == "FBI_main" && CharacterController.IsCharacterInFbi(characterController))
                            API.shared.triggerClientEvent(player, "fbi_menu", 1, PropertyData.Name, 0);

                        if (PropertyData.Name == "FBI_weapon" && CharacterController.IsCharacterInFbi(characterController))
                            API.shared.triggerClientEvent(player, "fbi_menu", 1, PropertyData.Name, 0);
                    }
                }
            };
            FbiMainColshape.onEntityExitColShape += (shape, entity) =>
            {
                var player = API.getPlayerFromHandle(entity);
                if (player != null)
                {
                    if (PropertyData.Name == "FBI_main" || PropertyData.Name == "FBI_weapon")
                        API.shared.triggerClientEvent(player, "fbi_menu", 0, PropertyData.Name, 0);
                }
            };

            // FBI stock (3f dimension)
            FbiStockColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 3f, 3f);
            FbiStockColshape.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    if (PropertyData.Type == PropertyType.FBI)
                    {
                        CharacterController characterController = player.getData("CHARACTER");

                        if (PropertyData.Name == "FBI_stock" && 
                        CharacterController.IsCharacterInArmy(characterController) && player.isInVehicle)
                            API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 1, PropertyData.Name, characterController.Character.GroupType);                        
                    }
                }
            };
            FbiStockColshape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Name == "FBI_stock")
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "army_menu", 0, PropertyData.Name, 0);
                }
            };
            
            // Emergency
            EmergencyColshape = API.createCylinderColShape(new Vector3(PropertyData.ExtPosX, PropertyData.ExtPosY, PropertyData.ExtPosZ), 2f, 2f);
            EmergencyColshape.onEntityEnterColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    Client player = API.getPlayerFromHandle(entity);
                    if (player != null)
                    {
                        if (PropertyData.Type == PropertyType.Emergency)
                        {
                            CharacterController characterController = player.getData("CHARACTER");
                            if (PropertyData.Name == "Emergency_main" &&
                                CharacterController.IsCharacterInEmergency(characterController))
                                API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "emergency_menu", 1,
                                    PropertyData.Name, 0);


                            ContextFactory.Instance.SaveChanges();
                        }
                    }
                }
            };
            EmergencyColshape.onEntityExitColShape += (shape, entity) =>
            {
                if (API.getPlayerFromHandle(entity) != null)
                {
                    if (PropertyData.Name == "Emergency_main")
                    {
                        API.shared.triggerClientEvent(API.getPlayerFromHandle(entity), "emergency_menu", 0,
                            PropertyData.Name, 0);
                    }
                }
            };
        }

        public static void CreateHome(Client player, int cost)
        {
            var propertyData = new Data.Property
            {
                CharacterId = null,
                GroupId = null
            };

            var propertyController = new PropertyController(propertyData);

            propertyData.Name = "House";

            propertyData.Stock = cost;
            propertyData.Type = PropertyType.House;
            propertyData.ExtPosX = player.position.X;
            propertyData.ExtPosY = player.position.Y;
            propertyData.ExtPosZ = player.position.Z;
            propertyData.Enterable = true;

            ContextFactory.Instance.Property.Add(propertyData);
            ContextFactory.Instance.SaveChanges();
            propertyController.CreateWorldEntity();
            API.shared.sendChatMessageToPlayer(player, "~g~[СЕРВЕР]: ~w~ Добавлен новый дом!");
        }
        public static void CreateProperty(Client player, string ownerType, PropertyType type, string name, int cost)
        {
            var propertyData = new Data.Property();
            string ownerName;
            switch (ownerType)
            {
                case "player":
                    var targetCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.SocialClub == player.socialClubName);
                    if (targetCharacter == null) return;
                    propertyData.Character = targetCharacter;
                    ownerName = targetCharacter.Name;
                    break;
                case "group":
                    var groupController = EntityManager.GetGroup(player, name);
                    if (groupController == null) return;

                    propertyData.Group = groupController.Group;
                    ownerName = groupController.Group.Name;
                    break;
                case "null":
                    propertyData.GroupId = null;
                    ownerName = null;
                    break;
                default:
                    API.shared.sendChatMessageToPlayer(player, "~r~[ОШИБКА]: ~w~Вы указали неверный тип (player/group");
                    return;
            }

            var propertyController = new PropertyController(propertyData);

            if ((int)type == 100) propertyData.Name = "House";

            propertyData.Stock = cost;
            propertyData.Type = type;
            propertyData.ExtPosX = player.position.X;
            propertyData.ExtPosY = player.position.Y;
            propertyData.ExtPosZ = player.position.Z;
            propertyData.Enterable = true;
            propertyController.ownername = ownerName;

            ContextFactory.Instance.Property.Add(propertyData);
            ContextFactory.Instance.SaveChanges();
            propertyController.CreateWorldEntity();
            API.shared.sendChatMessageToPlayer(player, "~g~[СЕРВЕР]: ~w~ Добавлен:  " + propertyController.Type() + "\nпринадлежащий: " + ownerName);
        }
        public int GetBlip()
        {
            const int defaultBlipId = 1;

            if (PropertyData.Character != null) return 40;
            if(GroupController.Group.ExtraType != 0)
            {
                return GroupController.Group.ExtraType.GetAttributeOfType<BlipTypeAttribute>()?.BlipId ?? defaultBlipId;
            }
            return GroupController.Group.Type.GetAttributeOfType<BlipTypeAttribute>()?.BlipId ?? defaultBlipId;
        }
        public string Type()
        {
            return PropertyData.Type.GetDisplayName();
        }
        public void PropertyDoor(Client player)
        {
            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            if (player.isInVehicle) return;
           
            if (!PropertyData.Enterable)
            {
                API.shared.sendNotificationToPlayer(player, "~g~Server: ~w~Вы не можете сюда войти.");
                return;
            }

            if (player.getData("IN_PROP") == this)
            {
                if (PropertyData.IPL != null) API.shared.sendNativeToPlayer(player, Hash.REMOVE_IPL, PropertyData.IPL); // API.removeIpl(property.IPL);
                player.resetData("IN_PROP");
                player.dimension = 0;
                player.position = ExteriorMarker.position;
            }
            else
            {
                if (PropertyData.IPL != null) API.shared.sendNativeToPlayer(player, Hash.REQUEST_IPL, PropertyData.IPL);  // API.requestIpl(property.IPL);
                player.setData("IN_PROP", this);
                player.position = InteriorMarker.position;
                player.dimension = PropertyData.PropertyID;
            }
        }

        private void CreateTextBlip(Data.Property data, string name, string blipName, int blipSprite)
        {
            API.createTextLabel(name, new Vector3(data.ExtPosX, data.ExtPosY, data.ExtPosZ) + new Vector3(0.0f, 0.0f, 0.5f), 15.0f, 0.5f);
            Blip = API.createBlip(new Vector3(data.ExtPosX, data.ExtPosY, data.ExtPosZ));
            Blip.shortRange = true;
            Blip.sprite = blipSprite;
            Blip.name = blipName;
        }
        [Command]
        public void Door(Client player)
        {
            PropertyDoor(player);
        }
    }
}
