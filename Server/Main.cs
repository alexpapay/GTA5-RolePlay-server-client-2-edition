using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using MpRpServer.Data.Enums;
using MpRpServer.Data.Models;
using MpRpServer.Server.DBManager;
using MpRpServer.Server.Groups;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MpRpServer.Server.Global;

namespace MpRpServer.Server
{
    class Main : Script
    {
        public Main()
        {
            API.onResourceStart += OnResourceStart;
            API.onResourceStop += OnResourceStop;
            API.onUpdate += OnUpdateHandler;
            API.onPlayerWeaponSwitch += OnPlayerWeaponSwitchHandler;
            API.onPlayerEnterVehicle += OnPlayerEnterVehicle;
            InteriorsLocations();
        }

        internal static bool LoadingFinished = false;
        private DateTime _secHeal = DateTime.Now;
        private DateTime _mLastTick = DateTime.Now;
        private DateTime _minuteAnnounce = DateTime.Now;
        private DateTime _hourAnnounce = DateTime.Now;
        private DateTime _afkAnnounce = DateTime.Now;
        private int _tick;
        private int _vehiclesCount;

        public void OnUpdateHandler()
        {
            if (LoadingFinished)
            {
                // 1 MINUTES EVENTS:
                if (DateTime.Now.Subtract(_minuteAnnounce).TotalMinutes >= 1)
                {
                    // Уровень пользователя
                    try
                    {
                        var characters = ContextFactory.Instance.Character.Where(x => x.Online).ToList();
                        var governmentBase = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 1100);

                        foreach (var character in characters)
                        {
                            character.PlayMinutes++;
                            // Уровень за 4 часа пребывания в игре
                            if (character.PlayMinutes % 240 == 0)
                            {
                                character.Level = character.PlayMinutes / 240;
                                try
                                {
                                    var currentPlayer = API.shared.getAllPlayers()
                                        .FirstOrDefault(x => x.socialClubName == character.SocialClub);
                                    API.shared.sendChatMessageToPlayer(currentPlayer,
                                        "~g~[СЕРВЕР]: ~w~ Поздравляем! Теперь вы достигли " + character.Level +
                                        " уровня.");
                                    EntityManager.Success("Игрок " + character.Name + " достиг уровня: " + character.Level);
                                }
                                catch (Exception e)
                                {
                                    EntityManager.Log(e, "Уровень за 4 часа пребывания в игре.");
                                }
                            }
                            // Зарплата за час пребывания в игре
                            if (character.PlayMinutes % 60 == 0)
                            {
                                var taxes = ContextFactory.Instance.Taxes.FirstOrDefault(x => x.Id == 1);
                                var isTaxiVehicle =
                                    ContextFactory.Instance.Vehicle.FirstOrDefault(x => x.CharacterId == character.Id);
                                var money = 0;
                                var taxesMoney = 0;
                                if (character.JobId == JobsIdNonDataBase.TaxiDriver && isTaxiVehicle != null)
                                    money += WorkPay.TaxiDriver; // TaxiDrivers
                                if (character.JobId == JobsIdNonDataBase.Unemployer)
                                    money += WorkPay.Unemployer; // Unemployers

                                money += PayDayMoney.GetPayDaYMoney(character.ActiveGroupID);

                                if (taxes != null)
                                {
                                    if (governmentBase != null)
                                    {
                                        taxesMoney = money * taxes.PersonalIncomeTax / 100;
                                        governmentBase.MoneyBank += taxesMoney;
                                    }
                                    money = money - money * taxes.PersonalIncomeTax / 100;
                                }
                                character.Cash += money;
                                

                                try
                                {
                                    var currentPlayer = API.shared.getAllPlayers()
                                        .FirstOrDefault(x => x.socialClubName == character.SocialClub);
                                    API.shared.sendChatMessageToPlayer(currentPlayer,
                                        "~g~[СЕРВЕР]: ~w~ Вы получили деньги: " + money + "$.");
                                    API.shared.sendChatMessageToPlayer(currentPlayer,
                                        "~y~[СЕРВЕР]: ~w~ Вы заплатили налогов: " + taxesMoney + "$.");
                                    API.shared.triggerClientEvent(currentPlayer, "update_money_display",
                                        character.Cash);
                                    EntityManager.Success("Игрок " + character.Name + " получил зарплату: " + money + "$");
                                }
                                catch (Exception e)
                                {
                                    EntityManager.Log(e, "Зарплата. Отображение полученных денег.");
                                }
                            }
                        }
                        ContextFactory.Instance.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        EntityManager.Log(e, "Уровень пользователя");
                    }
                    // Капт сектора
                    try
                    {
                        var startCapting = ContextFactory.Instance.Caption.FirstOrDefault(x => x.Id == 1);
                        if (startCapting == null) return;
                        if (startCapting.Sector != "0;0")
                        {
                            if (_tick == 0) API.sendChatMessageToAll("~s~Начался ~g~капт ~s~сектора: ~g~" + startCapting.Sector);
                            else API.sendChatMessageToAll("~s~Идет ~g~капт ~s~сектора: ~g~" + startCapting.Sector + " ~s~|~g~ " + _tick + " ~s~минут");
                            var sector = startCapting.Sector.Split(';');
                            GroupController.SetGangSectorData(Convert.ToInt32(sector[0]),Convert.ToInt32(sector[1]), 111);

                            if (_tick == 10)
                            {
                                var getAttackGroup =
                                    ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == startCapting.GangAttack * 100);
                                if (getAttackGroup == null) return;
                                var groupAttackType = (GroupType) Enum.Parse(typeof(GroupType), getAttackGroup.Type.ToString());
                                ContextFactory.Instance.SaveChangesAsync();

                                var getDefendGroup =
                                    ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == startCapting.GangDefend * 100);
                                if (getDefendGroup == null) return;
                                var groupDefendType = (GroupType) Enum.Parse(typeof(GroupType), getDefendGroup.Type.ToString());
                                string message;
                                ContextFactory.Instance.SaveChangesAsync();

                                if (startCapting.FragsAttack > startCapting.FragsDefend)
                                {
                                    GroupController.SetGangSectorData(Convert.ToInt32(sector[0]),
                                        Convert.ToInt32(sector[1]), startCapting.GangAttack);
                                    message = "Банда: ~g~" + EntityManager.GetDisplayName(groupAttackType) +
                                              "~g~захватила ~s~у банды: \n~g~" + EntityManager.GetDisplayName(groupDefendType) +
                                              "~s~сектор: ~g~" + startCapting.Sector;
                                    API.shared.sendChatMessageToAll(message);
                                    EntityManager.Success(message);
                                }
                                else if (startCapting.FragsAttack <= startCapting.FragsDefend)
                                {
                                    GroupController.SetGangSectorData(Convert.ToInt32(sector[0]),
                                        Convert.ToInt32(sector[1]), startCapting.GangDefend);
                                    message = "Банда: ~g~" + EntityManager.GetDisplayName(groupAttackType) +
                                              "~r~не смогла захватить ~s~у банды: \n~g~" + EntityManager.GetDisplayName(groupDefendType) + 
                                              "~s~сектор: ~g~" + startCapting.Sector;
                                    API.shared.sendChatMessageToAll(message);
                                    EntityManager.Success(message);
                                }
                                
                                GroupController.SetDefaultCaption(1);
                                _tick = 0;
                            }
                            _tick++;
                        }
                    }
                    catch (Exception e)
                    {
                        EntityManager.Log(e, "Капт сектора");
                    }

                    // PRISON:
                    try
                    {
                        var characters = ContextFactory.Instance.Character.Where(x => x.PrisonTime != 0).ToList();
                        foreach (var character in characters)
                        {
                            character.PrisonTime--;

                            if (character.PrisonTime != 0)
                            {
                                var currentPlayer = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == character.SocialClub);
                                API.freezePlayer(currentPlayer, true);
                                API.sendNotificationToPlayer(currentPlayer, "~g~Вам осталось сидеть в тюрьме еще " + character.PrisonTime + " мин.");
                            }
                            if (character.IsPrisoned && character.PrisonTime == 0)
                            {
                                var currentPlayer = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == character.SocialClub);
                                character.PrisonTime = 0;
                                character.IsPrisoned = false;
                                API.freezePlayer(currentPlayer, false);
                                API.setEntityPosition(currentPlayer, new Vector3(432.43, -981.47, 30.71));
                            }
                            ContextFactory.Instance.SaveChangesAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        EntityManager.Log(e, "PRISON DEBUG");
                    }

                    // WRITE PLAYER POS FOR AFK:
                    try
                    {
                        var players = API.getAllPlayers();
                        foreach (var player in players)
                        {
                            player.setData("AFKPOSITION", player.position);
                            player.setData("AFKROTATION", player.rotation);
                        }
                    }
                    catch (Exception e)
                    {
                        EntityManager.Log(e, "AFK WRITE POSITION");
                    }

                    // Тестовая зона (ежеминутный тик)
                    try
                    {
                        
                    }
                    catch (Exception e)
                    {
                        EntityManager.Log(e, "Тестовая зона (ежеминутный тик)");
                    }

                    _minuteAnnounce = DateTime.Now;
                }
                // 60 MINUTES EVENTS:
                if (DateTime.Now.Subtract(_hourAnnounce).TotalMinutes >= 60)
                {
                    // Начисление зарплаты в банк банд каждый час по количеству квадратов.
                    try
                    {
                        var numInc = 0;
                        for (var i = 1300; i <= 1700; i += 100)
                        {
                            var currentGang = ContextFactory.Instance.Group.First(x => x.Id == i);
                            var numOfSectors = GroupController.GetCountOfGangsSectors();
                            var money = numOfSectors[numInc] * 50;
                            currentGang.MoneyBank += money;
                            ContextFactory.Instance.SaveChangesAsync();
                            numInc++;
                            EntityManager.Success("Начислено " + money + "$ банде: " + currentGang.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        EntityManager.Log(e, "Начисление зарплаты за квадраты банд");
                    }
                    // Начисление зарплаты за каждый бизнес.
                    try
                    {
                        var characters = ContextFactory.Instance.Character.Where(x => x.Online).ToList();
                        var governmentBase = ContextFactory.Instance.Group.FirstOrDefault(x => x.Id == 1100);

                        foreach (var character in characters)
                        {
                            var jobs = ContextFactory.Instance.Job.Where(y => y.CharacterId == character.Id).ToList();
                            var taxes = ContextFactory.Instance.Taxes.FirstOrDefault(x => x.Id == 1);
                            var characterHouses = ContextFactory.Instance.Property.Where(x => x.CharacterId == character.Id).ToList();

                            if (characterHouses!= null)
                            {
                                var taxesForHouses = ContextFactory.Instance.Taxes.FirstOrDefault(x => x.Id == 1);

                                if (taxesForHouses != null)
                                {
                                    foreach (var house in characterHouses)
                                    {
                                        var taxesMoney = house.Stock * TaxesByType.GetHouseClass(taxesForHouses, house.Stock)/ 100;
                                        character.Cash -= taxesMoney;
                                        ContextFactory.Instance.SaveChangesAsync();

                                        var currentPlayer = API.shared.getAllPlayers()
                                            .FirstOrDefault(x => x.socialClubName == character.SocialClub);
                                        API.shared.sendChatMessageToPlayer(currentPlayer,
                                            "~y~[СЕРВЕР]: ~w~ Вы заплатили за дом номер " + house.PropertyID + " налогов: " + taxesMoney + "$.");
                                        API.shared.triggerClientEvent(currentPlayer, "update_money_display", character.Cash);
                                    }
                                }
                            }

                            foreach (var job in jobs)
                            {
                                var taxesMoney = 0;
                                if (taxes != null)
                                {
                                    if (governmentBase != null)
                                    {
                                        taxesMoney = job.Cost * taxes.PersonalIncomeTax / 100;
                                        governmentBase.MoneyBank += taxesMoney;
                                    }
                                }
                                job.Cost = job.Cost - taxesMoney;
                                character.Cash += job.Cost;
                                ContextFactory.Instance.SaveChanges();

                                var currentPlayer = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == character.SocialClub);

                                if (currentPlayer == null) continue;
                                API.shared.sendChatMessageToPlayer(currentPlayer,
                                    "~g~[СЕРВЕР]: ~w~ Вы получили за " + job.Id + " бизнес: " + job.Cost + "$.");
                                API.shared.sendChatMessageToPlayer(currentPlayer,
                                    "~y~[СЕРВЕР]: ~w~ Вы заплатили налогов за " + job.Id + " бизнес: " + taxesMoney + "$.");
                                API.shared.triggerClientEvent(currentPlayer, "update_money_display", character.Cash);
                                EntityManager.Success("Начислено " + job.Cost + "$ владельцу бизнеса: " + character.Name);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        EntityManager.Log(e, "Начисление зарплаты в за каждый бизнес");
                    }
                    // Наркотическая ломка.
                    try
                    {
                        var characters = ContextFactory.Instance.Character.Where(x => x.Online).ToList();

                        foreach (var character in characters)
                        {
                            if (character.NarcoDep > 3000)
                            {
                                // TODO: ANIMATION OF THIS
                                var currentPlayer = API.shared.getAllPlayers()
                                    .FirstOrDefault(x => x.socialClubName == character.SocialClub);
                                API.playPlayerAnimation(currentPlayer, 1, "mp_basejump", "base_jump_spot");
                                API.sendNotificationToPlayer(currentPlayer, "~r~У вас наркотическая ломка!");
                            }
                        }
                        ContextFactory.Instance.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        EntityManager.Log(e, "Наркотическая ломка");
                    }

                    _hourAnnounce = DateTime.Now;
                }
                // AFK KICK:
                if (DateTime.Now.Subtract(_afkAnnounce).TotalMinutes >= 15)
                {
                    var players = API.getAllPlayers();

                    foreach (var player in players)
                    {
                        Vector3 playerPos = player.getData("AFKPOSITION");
                        if (playerPos == null) continue;
                        Vector3 playerRot = player.getData("AFKROTATION");
                        if (playerRot == null) continue;
                        if (playerPos != player.position && playerRot != player.rotation) continue;
                        API.sendNotificationToPlayer(player, "~r~Вас кикнули с сервера за АФК");
                        API.kickPlayer(player, "AFK");
                    }
                    _afkAnnounce = DateTime.Now;
                }
                // LIST OF PLAYERS:
                if (DateTime.Now.Subtract(_mLastTick).TotalMilliseconds >= 100)
                {
                    var changedNames = new List<string>();
                    var players = API.getAllPlayers();
                    foreach (var player in players)
                    {
                        string lastName = player.getData("playerlist_lastname");

                        if (lastName == null)
                        {
                            player.setData("playerlist_lastname", player.name);
                            continue;
                        }

                        if (lastName != player.name)
                        {
                            player.setData("playerlist_lastname", player.name);
                            var characterToList = ContextFactory.Instance.Character.FirstOrDefault(x => x.SocialClub == player.socialClubName);
                            if (characterToList == null) return;
                            var dic = new Dictionary<string, object>
                            {
                                ["userName"] = characterToList.Name,
                                ["userID"] = characterToList.OID,
                                ["socialClubName"] = player.socialClubName,
                                ["newName"] = player.name
                            };
                            changedNames.Add(API.toJson(dic));
                        }
                    }
                    if (changedNames.Count > 0)
                    {
                        API.triggerClientEventForAll("playerlist_changednames", changedNames);
                    }

                    _mLastTick = DateTime.Now;
                }
                // HEAL | VOICE:
                if (DateTime.Now.Subtract(_secHeal).TotalSeconds >= 1)
                {
                    //string bigSql = "";
                    var players = API.getAllPlayers();
                    foreach (var player in players)
                    {
                        if (player.getData("ISLOGINSUCCES") == true)
                        {
                            try
                            {
                                var pos = API.getEntityPosition(player);
                                var vehicleNumber = -1;
                                if (player.isInVehicle) vehicleNumber = API.getEntityData(player.vehicle, "vehicle_number");

                                var character = ContextFactory.Instance.Character.FirstOrDefault(
                                    x => x.SocialClub == player.socialClubName);

                                character.X = pos.X;
                                character.Y = pos.Y;
                                character.Z = pos.Z;
                                character.player_vehicle = vehicleNumber;
                                character.button_voice = player.getData("BUTTON_VOICE");
                                ContextFactory.Instance.SaveChangesAsync();
                                /*
                                big_sql = big_sql +
                                          "UPDATE `godfather`.`character` SET `X` = '" + pos.X +
                                          "', `Y` = '" + pos.Y + "', Z = '" + pos.Z +
                                          "', `player_vehicle` = " + vehicle_number +
                                          ", `button_voice` = " + player.getData("BUTTON_VOICE") +
                                          " WHERE `character`.`SocialClub` = '" + player.socialClubName + "';";*/
                            }
                            catch (Exception e)
                            {
                                EntityManager.Log(e, "Голосовая связь");
                            }
                        }
                        if (player.getData("NEEDHEAL") == true)
                        {
                            if (player.health == 100) 
                            {
                                player.setData("NEEDHEAL", false);
                                continue;
                            }
                            player.health++;
                        }
                    }
                    //if (big_sql != "") mysql_query(big_sql);
                    _secHeal = DateTime.Now;
                }
            }
        }

        private void OnPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            if (API.getEntityData(vehicle, "vehicle_number") == null)
            {
                API.setEntityData(vehicle, "vehicle_number", _vehiclesCount);
                _vehiclesCount++;
            }
        }
        private void OnPlayerWeaponSwitchHandler(Client player, WeaponHash oldWeapon)
        {
            var weapons = API.getPlayerWeapons(player);
            foreach (var weapon in weapons)
            {
                if (weapon == WeaponHash.Minigun) player.kick("This weapon are restricted!");
            }
        }

        // RESOURCE METHODS:
        private void OnResourceStart()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-GB");
            Console.BackgroundColor = ConsoleColor.Blue;
            API.consoleOutput(GlobalVars.ServerName + " was started at " + DateTime.Now);
            Console.ResetColor();
            //Thread.Sleep(3000);
            //mysql_query("UPDATE `godfather`.`character` SET `player_voice_hash` = ''");
        }
        private void OnResourceStop()
        {
            var characters = ContextFactory.Instance.Character.Where(x => x.Online).ToList();
            foreach (var character in characters) character.Online = false;
            ContextFactory.Instance.SaveChangesAsync();

            API.consoleOutput("Сброс активных сессий...");
            Task dbTerminate = Task.Run(() =>
            {
                //DatabaseManager.ResetSessions();
            });
            dbTerminate.Wait();
            API.consoleOutput(GlobalVars.ServerName + " был остановлен в " + DateTime.Now);
        }

        public void mysql_query(string text)
        {
            API.exported.database.executeQuery(text);
        }

        // INTERIORS & OTHER METHODS:
        public void InteriorsLocations()
        {
            API.createObject(552807189, new Vector3(-1397.168, 5815.977, 20.0), new Vector3(0.0f, 0.0f, 0.0f));
            API.requestIpl("hei_carrier");
            API.requestIpl("hei_Carrier_int1");
            API.requestIpl("hei_Carrier_int2");
            API.requestIpl("hei_Carrier_int3");
            API.requestIpl("hei_Carrier_int4");
            API.requestIpl("hei_Carrier_int5");
            API.requestIpl("hei_Carrier_int6");
            API.requestIpl("hei_carrier_LODLights");
            // Gunrunning Heist Yacht
            API.requestIpl("gr_grdlc_yacht_lod");
            API.requestIpl("gr_grdlc_yacht_placement");
            API.requestIpl("gr_heist_yacht2");
            API.requestIpl("gr_heist_yacht2_bar");
            API.requestIpl("gr_heist_yacht2_bar_lod");
            API.requestIpl("gr_heist_yacht2_bedrm");
            API.requestIpl("gr_heist_yacht2_bedrm_lod");
            API.requestIpl("gr_heist_yacht2_bridge");
            API.requestIpl("gr_heist_yacht2_bridge_lod");
            API.requestIpl("gr_heist_yacht2_enginrm");
            API.requestIpl("gr_heist_yacht2_enginrm_lod");
            API.requestIpl("gr_heist_yacht2_lod");
            API.requestIpl("gr_heist_yacht2_lounge");
            API.requestIpl("gr_heist_yacht2_lounge_lod");
            API.requestIpl("gr_heist_yacht2_slod");
            // Dignity Heist Yacht
            API.requestIpl("smboat");
            API.requestIpl("smboat_lod");
            API.requestIpl("hei_yacht_heist");
            API.requestIpl("hei_yacht_heist_enginrm");
            API.requestIpl("hei_yacht_heist_Lounge");
            API.requestIpl("hei_yacht_heist_Bridge");
            API.requestIpl("hei_yacht_heist_Bar");
            API.requestIpl("hei_yacht_heist_Bedrm");
            API.requestIpl("hei_yacht_heist_DistantLights");
            API.requestIpl("hei_yacht_heist_LODLights");
            // Dignity Party Yacht
            API.requestIpl("smboat");
            API.requestIpl("smboat_lod");
            API.requestIpl("hei_yacht_heist");
            API.requestIpl("hei_yacht_heist_enginrm");
            API.requestIpl("hei_yacht_heist_Lounge");
            API.requestIpl("hei_yacht_heist_Bridge");
            API.requestIpl("hei_yacht_heist_Bar");
            API.requestIpl("hei_yacht_heist_Bedrm");
            API.requestIpl("hei_yacht_heist_DistantLights");
            API.requestIpl("hei_yacht_heist_LODLights");
            // Bridge Train Crash 
            API.requestIpl("canyonriver01_traincrash");
            API.requestIpl("railing_end");
            // Bridge Train Normal
            API.requestIpl("canyonriver01");
            API.requestIpl("railing_start");
            // ONeils Farm Burnt 
            API.requestIpl("farmint");
            API.requestIpl("farm_burnt");
            API.requestIpl("farm_burnt_props");
            API.requestIpl("des_farmhs_endimap");
            API.requestIpl("des_farmhs_end_occl");
            // ONeils Farm
            API.requestIpl("farm");
            API.requestIpl("farm_props");
            API.requestIpl("farm_int");
            // Morgue 
            API.requestIpl("coronertrash");
            API.requestIpl("Coroner_Int_On");
            // Clubhouse & Warehouses
            API.requestIpl("bkr_biker_interior_placement_interior_0_biker_dlc_int_01_milo");
            API.requestIpl("bkr_biker_interior_placement_interior_1_biker_dlc_int_02_milo");
            API.requestIpl("bkr_biker_interior_placement_interior_2_biker_dlc_int_ware01_milo");
            API.requestIpl("bkr_biker_interior_placement_interior_3_biker_dlc_int_ware02_milo");
            API.requestIpl("bkr_biker_interior_placement_interior_4_biker_dlc_int_ware03_milo");
            API.requestIpl("bkr_biker_interior_placement_interior_5_biker_dlc_int_ware04_milo");
            API.requestIpl("bkr_biker_interior_placement_interior_6_biker_dlc_int_ware05_milo");
            API.requestIpl("ex_exec_warehouse_placement_interior_1_int_warehouse_s_dlc_milo");
            API.requestIpl("ex_exec_warehouse_placement_interior_0_int_warehouse_m_dlc_milo");
            API.requestIpl("ex_exec_warehouse_placement_interior_2_int_warehouse_l_dlc_milo");
            API.requestIpl("imp_impexp_interior_placement_interior_1_impexp_intwaremed_milo_");
            // Special Locations
            API.requestIpl("cargoship");
            API.requestIpl("FINBANK");
            API.requestIpl("SP1_10_real_interior");
            API.requestIpl("refit_unload");
            API.requestIpl("FIBlobby");
            API.requestIpl("post_hiest_unload");
            // Maze Bank West
            API.requestIpl("ex_sm_15_office_02b");
            API.requestIpl("ex_sm_15_office_02c");
            API.requestIpl("ex_sm_15_office_02a");
            API.requestIpl("ex_sm_15_office_01a");
            API.requestIpl("ex_sm_15_office_01b");
            API.requestIpl("ex_sm_15_office_01c");
            API.requestIpl("ex_sm_15_office_03a");
            API.requestIpl("ex_sm_15_office_03b");
            API.requestIpl("ex_sm_15_office_03c");
            // Lom Bank
            API.requestIpl("ex_sm_13_office_02b");
            API.requestIpl("ex_sm_13_office_02c");
            API.requestIpl("ex_sm_13_office_02a");
            API.requestIpl("ex_sm_13_office_01a");
            API.requestIpl("ex_sm_13_office_01b");
            API.requestIpl("ex_sm_13_office_01c");
            API.requestIpl("ex_sm_13_office_03a");
            API.requestIpl("ex_sm_13_office_03b");
            API.requestIpl("ex_sm_13_office_03c");
            // Maze Bank Building
            API.requestIpl("ex_dt1_11_office_02b");
            API.requestIpl("ex_dt1_11_office_02c");
            API.requestIpl("ex_dt1_11_office_02a");
            API.requestIpl("ex_dt1_11_office_01a");
            API.requestIpl("ex_dt1_11_office_01b");
            API.requestIpl("ex_dt1_11_office_01c");
            API.requestIpl("ex_dt1_11_office_03a");
            API.requestIpl("ex_dt1_11_office_03b");
            API.requestIpl("ex_dt1_11_office_03c");
            // Arcadius Business Centre
            API.requestIpl("ex_dt1_02_office_02b");
            API.requestIpl("ex_dt1_02_office_02c");
            API.requestIpl("ex_dt1_02_office_02a");
            API.requestIpl("ex_dt1_02_office_01a");
            API.requestIpl("ex_dt1_02_office_01b");
            API.requestIpl("ex_dt1_02_office_01c");
            API.requestIpl("ex_dt1_02_office_03a");
            API.requestIpl("ex_dt1_02_office_03b");
            API.requestIpl("ex_dt1_02_office_03c");
            // Online Apartments
            API.requestIpl("apa_v_mp_h_01_a");
            API.requestIpl("apa_v_mp_h_01_c");
            API.requestIpl("apa_v_mp_h_01_b");
            API.requestIpl("apa_v_mp_h_02_a");
            API.requestIpl("apa_v_mp_h_02_c");
            API.requestIpl("apa_v_mp_h_02_b");
            API.requestIpl("apa_v_mp_h_03_a");
            API.requestIpl("apa_v_mp_h_03_c");
            API.requestIpl("apa_v_mp_h_03_b");
            API.requestIpl("apa_v_mp_h_04_a");
            API.requestIpl("apa_v_mp_h_04_c");
            API.requestIpl("apa_v_mp_h_04_b");
            API.requestIpl("apa_v_mp_h_05_a");
            API.requestIpl("apa_v_mp_h_05_c");
            API.requestIpl("apa_v_mp_h_05_b");
            API.requestIpl("apa_v_mp_h_06_a");
            API.requestIpl("apa_v_mp_h_06_c");
            API.requestIpl("apa_v_mp_h_06_b");
            API.requestIpl("apa_v_mp_h_07_a");
            API.requestIpl("apa_v_mp_h_07_c");
            API.requestIpl("apa_v_mp_h_07_b");
            API.requestIpl("apa_v_mp_h_08_a");
            API.requestIpl("apa_v_mp_h_08_c");
            API.requestIpl("apa_v_mp_h_08_b");
            // Online Bunkers Exterior
            API.requestIpl("gr_case0_bunkerclosed");
            API.requestIpl("gr_case1_bunkerclosed");
            API.requestIpl("gr_case2_bunkerclosed");
            API.requestIpl("gr_case3_bunkerclosed");
            API.requestIpl("gr_case4_bunkerclosed");
            API.requestIpl("gr_case5_bunkerclosed");
            API.requestIpl("gr_case6_bunkerclosed");
            API.requestIpl("gr_case7_bunkerclosed");
            API.requestIpl("gr_case8_bunkerclosed");
            API.requestIpl("gr_case9_bunkerclosed");
            API.requestIpl("gr_case10_bunkerclosed");
            API.requestIpl("gr_case11_bunkerclosed");
        }
    }
}
