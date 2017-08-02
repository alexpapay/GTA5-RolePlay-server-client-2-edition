using GrandTheftMultiplayer.Shared.Math;
using System.Collections.Generic;
using System.Linq;
using MpRpServer.Server.DBManager;

namespace MpRpServer.Data.Models
{
    class BusOne
    {
        public static readonly Vector3 Marker1 = new Vector3   (-1039.623, -2714.09,   12.66907);  // Airport: 1
        public static readonly Vector3 Marker2 = new Vector3   (304.7786,  -1381.645,  30.47842);  // Medical: 2
        public static readonly Vector3 Marker3 = new Vector3   (136.69,    -946.8344,  28.32286);  // Meria: 3 
        public static readonly Vector3 Marker4 = new Vector3   (-146.7599, -919.225,   27.87529);  // Work loader: 4         
        public static readonly Vector3 MarkerFin = new Vector3 (-798.1025, -2368.539,  13.57907);  // BusStation: 5
    }
    class BusTwo
    {
        public static readonly Vector3 Marker1 = new Vector3(406.535828, -669.474243, 28.2897167);
        public static readonly Vector3 Marker2 = new Vector3(300.441833, -763.4902, 29.0460339);
        public static readonly Vector3 Marker3 = new Vector3(197.890884, -1298.822, 28.3211689);
        public static readonly Vector3 Marker4 = new Vector3(383.281433, -1700.77319, 28.2629242);
        public static readonly Vector3 Marker5 = new Vector3(119.911224, -2028.39, 17.21);
        public static readonly Vector3 Marker6 = new Vector3(-1060.21936, -2703.28979, 12.9488535);
        public static readonly Vector3 Marker7 = new Vector3(-162.197861, -1791.395, 28.8501968);
        public static readonly Vector3 Marker8 = new Vector3(57.06164, -1542.65991, 28.4625015);
        public static readonly Vector3 Marker9 = new Vector3(238.45105, -1271.865, 28.278162);
        public static readonly Vector3 Marker10 = new Vector3(510.342957, -1064.939, 27.6294785);
        public static readonly Vector3 MarkerFin = new Vector3(508.7956, -801.224854, 23.8670654);
    }
    class BusThree
    {
        public static readonly Vector3 Marker1 = new Vector3(-1039.623, -2714.09, 12.66907);  // Airport: 1
    }
    class BusFour
    {
        public static readonly Vector3 Marker1 = new Vector3(-1039.623, -2714.09, 12.66907);  // Airport: 1
    }
    class GasStation_Id3
    {
        public static readonly Vector3 Marker1 = new Vector3   (-58.83617, -1761.643,  28.0);
        public static readonly Vector3 Marker2 = new Vector3   (-61.65413, -1768.636,  28.0);
        public static readonly Vector3 Marker3 = new Vector3   (-69.89056, -1765.871,  28.2);
        public static readonly Vector3 Marker4 = new Vector3   (-67.24121, -1759.187,  28.2);         
        public static readonly Vector3 Marker5 = new Vector3   (-77.71686, -1763.026,  28.4);
        public static readonly Vector3 Marker6 = new Vector3   (-75.38932, -1756.052,  28.4);
    }
    class GasStation_Id4
    {
        public static readonly Vector3 Marker1 = new Vector3(167.8455, -1561.029, 28.3);
        public static readonly Vector3 Marker2 = new Vector3(174.147, -1554.992, 28.3);
        public static readonly Vector3 Marker3 = new Vector3(180.5104, -1560.039, 28.3);
        public static readonly Vector3 Marker4 = new Vector3(173.2242, -1566.945, 28.3);
    }
    class GasStation_Id5
    {
        public static readonly Vector3 Marker1 = new Vector3(276.2667, -1268.816, 28.17181);
        public static readonly Vector3 Marker2 = new Vector3(277.0069, -1253.231, 28.17192);
        public static readonly Vector3 Marker3 = new Vector3(267.6013, -1252.983, 28.14289);
        public static readonly Vector3 Marker4 = new Vector3(268.2359, -1268.313, 28.14289);
        public static readonly Vector3 Marker5 = new Vector3(259.1979, -1268.278, 28.1429);
        public static readonly Vector3 Marker6 = new Vector3(259.8073, -1261.156, 28.1429);
        public static readonly Vector3 Marker7 = new Vector3(259.5443, -1253.138, 28.1429);
    }
    class GasStation_Id10
    {
        public static readonly Vector3 Marker1 = new Vector3(825.2615, -1026.292, 25.36443);
        public static readonly Vector3 Marker2 = new Vector3(825.3356, -1030.93, 25.39021);
        public static readonly Vector3 Marker3 = new Vector3(829.7487, -1026.036, 26.68172);
        public static readonly Vector3 Marker4 = new Vector3(816.9194, -1026.164, 26.26128);
        public static readonly Vector3 Marker5 = new Vector3(816.6171, -1030.894, 26.29573);
        public static readonly Vector3 Marker6 = new Vector3(808.9995, -1031.023, 26.28728);
        public static readonly Vector3 Marker7 = new Vector3(808.8928, -1025.842, 26.24821);
    }
    class GasStation_Id11
    {
        public static readonly Vector3 Marker1 = new Vector3(-712.6735, -932.7349, 18.017);
        public static readonly Vector3 Marker2 = new Vector3(-712.7229, -939.4697, 18.01702);
        public static readonly Vector3 Marker3 = new Vector3(-721.2331, -939.484, 18.01702);
        public static readonly Vector3 Marker4 = new Vector3(-721.2691, -931.5731, 18.01702);
        public static readonly Vector3 Marker5 = new Vector3(-730.4387, -932.5051, 18.01702);
        public static readonly Vector3 Marker6 = new Vector3(-730.3234,-939.1136, 18.01702);
    }
    class GasStation_Id12
    {
        public static readonly Vector3 Marker1 = new Vector3(-2085.496, -320.5703, 11.02331);
        public static readonly Vector3 Marker2 = new Vector3(-2084.824, -313.8617, 11.02278);
        public static readonly Vector3 Marker3 = new Vector3(-2098.197, -311.6304, 11.02516);
        public static readonly Vector3 Marker4 = new Vector3(-2099.033, -320.5072, 11.02565);
        public static readonly Vector3 Marker5 = new Vector3(-2099.872, -326.2254, 11.02558);
        public static readonly Vector3 Marker6 = new Vector3(-2108.639, -324.6943, 11.02213);
        public static readonly Vector3 Marker7 = new Vector3(-2107.67, -318.9186, 11.0235);
        public static readonly Vector3 Marker8 = new Vector3(-2106.838, -310.7905, 11.0235);
    }
    class GasStation_Id13
    {
        public static readonly Vector3 Marker1 = new Vector3(-2551.473, 2329.49, 32.07299);
        public static readonly Vector3 Marker2 = new Vector3(-2558.304, 2329.007, 32.07257);
        public static readonly Vector3 Marker3 = new Vector3(-2558.135, 2336.209, 32.06094);
        public static readonly Vector3 Marker4 = new Vector3(-2551.852, 2336.691, 32.07246);
        public static readonly Vector3 Marker5 = new Vector3(-2552.375, 2344.183, 32.1086);
        public static readonly Vector3 Marker6 = new Vector3(-2559.291, 2343.858, 32.10815);
    }
    class GasStation_Id14
    {
        public static readonly Vector3 Marker1 = new Vector3(1044.856, 2670.087, 38.54773);
        public static readonly Vector3 Marker2 = new Vector3(1034.56, 2676.557, 38.54387);
        public static readonly Vector3 Marker3 = new Vector3(1044.639, 2672.196, 38.55093);
    }
    class GasStation_Id15
    {
        public static readonly Vector3 Marker1 = new Vector3(2679.108, 3267.421, 54.24056);
        public static readonly Vector3 Marker2 = new Vector3(2676.364, 3262.776, 54.24056);
        public static readonly Vector3 Marker3 = new Vector3(2680.623, 3262.11, 54.24055);
        public static readonly Vector3 Marker4 = new Vector3(2682.73, 3265.447, 54.24055);
    }
    class GasStation_Id16
    {
        public static readonly Vector3 Marker1 = new Vector3(157.249, 6630.413, 30.70236);
        public static readonly Vector3 Marker2 = new Vector3(154.8527, 6628.082, 30.74187);
    }
    class WorkPay
    {
        public const int AllGusStationOwner = 10000;
        public const int BusDriver1Pay = 1000;
        public const int BusDriver2Pay = 3000;
        public const int BusDriver3Pay = 3000;
        public const int BusDriver4Pay = 3000;
        public const int TaxiDriver = 600;
        public const int Unemployer = 300;

        public const int Loader1Pay = 10;       // Стройка
        public const int Loader2Pay = 7;        // Порт
    }
    class RentModels
    {
        public const int ScooterModel = -1842748181;
        public const int TaxiModel = -956048545;
    }
    // TODO: linking with DB
    class AutoModels
    {
        public const int GangVagon = -810318068;
        public const int GangCar = 1507916787;
        public const int EmergencyCar = 1171614426;
    }

    class Materials
    {
        public const int GangVagonCapacity = 10000;
        public const int GangCarCapacity = 5000;
        public const int GangPersonCapacity = 500;
    }

    class Time
    {
        public const int ScooterRentTime = 30;
        public const int TaxiRentTime = 60;        
    }

    class Prices
    {
        public const int DrugSellPrice = 20;
        public const int ScooterRentPrice  = 30; 
        public const int TaxiRentPrice = 100;
        public const int DriverLicensePrice = 50;
        public const int MobilePhone = 200; // IF IT CHANGED - CHANGE TEXT IN MENU!!!
    }

    class JobsIdNonDataBase
    {
        public const int Homeless = 0;
        public const int Loader1 = 1;
        public const int Loader2 = 2;
        public const int Mechanic = 333;
        public const int TaxiDriver = 777;

        // BusDriver logic also in: VehicleController.cs | MenuManager.cs | JobController.cs | KeyManager.cs
        public const int BusDriver = 888;
        public const int BusDriver1 = 881;
        public const int BusDriver2 = 882;
        public const int BusDriver3 = 883;
        public const int BusDriver4 = 884;
        // 

        public const int Unemployer = 999;
    }
    // TAXES:
    class TaxesByType
    {
        public static int GetHouseClass(Taxes taxes, int houseCost)
        {
            if (houseCost >= 5000000) return taxes.HouseA;
            if (houseCost >= 3000000 && houseCost < 5000000) return taxes.HouseB;
            if (houseCost >= 500000 && houseCost < 3000000) return taxes.HouseC;
            if (houseCost >= 50000 && houseCost < 500000) return taxes.HouseD;

            return 0;
        }
    }
    // FUEL:
    class FuelByType
    {
        public static double GetFuel(int model)
        {
            switch (model)
            {
                case 1394036463: return 350;
                case -1600252419: return 250;
                case 321739290: return 80;
                case 1074326203: return 120;
                case -810318068: return 70;
                case 1507916787: return 50;
                case -825837129: return 50;
                case 2006667053: return 50;
                case 1923400478: return 50;
                case -713569950: return 80;
                case -956048545: return 40;
                case 782665360:  return 200;
                case 630371791:  return 120;
                case 1770332643: return 120;
                case -1627000575: return 80;
                case 1171614426: return 70;
                case 456714581: return 100;
                case -1647941228: return 120;
                case 1127131465: return 80;
                case -1281684762: return 180;
                case -2007026063: return 120;
                case 2071877360: return 130;
                case 1824333165: return 120;
                case 1981688531: return 200;
                case 970385471: return 140;
                case 745926877: return 100;
                case -34623805: return 40;
                case -1973172295: return 60;
                case 353883353: return 140;
                case 1922257928: return 100;
                case -1683328900: return 50;
                case 741586030: return 110;
                case 2046537925: return 80;
                case 1777363799: return 75;
                case -2098947590: return 80;
                case -1961627517: return 80;
                case -1671539132: return 140;
                case 666166960: return 90;
                case 758895617: return 70;
                case -1660945322: return 65;
                case 1123216662: return 90;
                default: return 0;
            }
        }
        public static double GetConsumption(int model)
        {
            switch (model)
            {
                case 1394036463: return 0.035;
                case -1600252419:return 0.020;                
                case 1074326203: return 0.015;
                case 321739290:  return 0.007;
                case -713569950: return 0.007;
                case -810318068: return 0.005;
                case 1770332643: return 0.005;
                case 1507916787: return 0.003;
                case -825837129: return 0.003;
                case 2006667053: return 0.003;
                case 1923400478: return 0.003;                
                case -956048545: return 0.002;
                case 782665360:  return 0.005;
                case 630371791:  return 0.015;
                default: return 0.001;
            }
        }
    }
    class Stocks
    {
        public static int GetStockCapacity(string stockName)
        {
            switch (stockName)
            {
                case "Army1_stock": return 200000;
                case "Army2_stock": return 500000;
                case "Police_stock": return 200000;
                case "FBI_stock": return 200000;
                default: return 200000;
            }
        }
    }
    class PayDayMoney
    {
        public static int GetPayDaYMoney(int groupId)
        {
            switch(groupId)
            {
                case 1: return 0;       // Homeless
                case 2: return 100;     // Unemployer
                // POLICE:
                case 101: return 5000;
                case 102: return 5500;
                case 103: return 6000;
                case 104: return 6500;
                case 105: return 7000;
                case 106: return 7500;
                case 107: return 8000;
                case 108: return 8500;
                case 109: return 9000;
                case 110: return 9500;
                case 111: return 10000;
                case 112: return 10500;
                case 113: return 11000;
                case 114: return 12000;
                // EMERGENCY:
                case 201: return 4900;
                case 202: return 5400;
                case 203: return 5900;
                case 204: return 6400;
                case 205: return 6900;
                case 206: return 7400;
                case 207: return 7900;
                case 208: return 8400;
                case 209: return 10400;
                case 210: return 12400;
                // FBI:
                case 301: return 4900;
                case 302: return 5400;
                case 303: return 5900;
                case 304: return 6400;
                case 305: return 6900;
                case 306: return 7400;
                case 307: return 7900;
                case 308: return 8400;
                case 309: return 10400;
                case 310: return 12400;
                // NEWS:
                case 701: return 4900;
                case 702: return 5400;
                case 703: return 5900;
                case 704: return 6400;
                case 705: return 6900;
                case 706: return 7400;
                case 707: return 7900;
                case 708: return 8400;
                case 709: return 10400;
                case 710: return 12400;
                // GOVERNMENT:
                case 1101: return 6000;
                case 1102: return 6000;
                case 1103: return 8000;
                case 1104: return 10000;
                case 1105: return 16000;
                case 1106: return 20000;
                // AUTOSCHOOL:
                case 1201: return 4000;
                case 1202: return 10000;
                case 1203: return 10500;
                case 1204: return 11000;
                case 1205: return 11500;
                case 1206: return 12000;
                case 1207: return 13000;
                case 1208: return 14000;
                case 1209: return 16000;
                case 1210: return 19000;
                // ARMY 1:
                case 2001: return 3000;
                case 2002: return 3500;
                case 2003: return 4000;
                case 2004: return 4500;
                case 2005: return 5000;
                case 2006: return 5500;
                case 2007: return 6000;
                case 2008: return 6500;
                case 2009: return 7000;
                case 2010: return 7500;
                case 2011: return 8000;
                case 2012: return 8500;
                case 2013: return 9000;
                case 2014: return 9500;
                case 2015: return 10000;
                // ARMY 2:
                case 2101: return 3000;
                case 2102: return 3500;
                case 2103: return 4000;
                case 2104: return 4500;
                case 2105: return 5000;
                case 2106: return 5500;
                case 2107: return 6000;
                case 2108: return 6500;
                case 2109: return 7000;
                case 2110: return 7500;
                case 2111: return 8000;
                case 2112: return 8500;
                case 2113: return 9000;
                case 2114: return 9500;
                case 2115: return 10000;
                // MAFIA 1:
                case 3001: return 3500;
                case 3002: return 4500;
                case 3003: return 5500;
                case 3004: return 6500;
                case 3005: return 7500;
                case 3006: return 8500;
                case 3007: return 9500;
                case 3008: return 10500;
                case 3009: return 11500;
                case 3010: return 12500;
                // MAFIA 2:
                case 3101: return 3500;
                case 3102: return 4500;
                case 3103: return 5500;
                case 3104: return 6500;
                case 3105: return 7500;
                case 3106: return 8500;
                case 3107: return 9500;
                case 3108: return 10500;
                case 3109: return 11500;
                case 3110: return 12500;
                // MAFIA 3:
                case 3201: return 3500;
                case 3202: return 4500;
                case 3203: return 5500;
                case 3204: return 6500;
                case 3205: return 7500;
                case 3206: return 8500;
                case 3207: return 9500;
                case 3208: return 10500;
                case 3209: return 11500;
                case 3210: return 12500;
            }
            return 0;
        }
    }
    class GroupsConst
    {
        public static readonly List<int> GroupsIndexes = new List<int> { 100, 200, 300, 1100, 1300, 1400, 1500, 1600, 1700, 2000, 2100, 3000, 3100 };
    }
}
