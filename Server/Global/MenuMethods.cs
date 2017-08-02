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
    class MenuMethods
    {
        public static bool CheckTargetId(Client player, out Character targetCharacter, int targetUserId)
        {
            targetCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == targetUserId);

            if (targetCharacter == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~Вами был введен неверный ID!");
                return false;
            }
            return true;
        }
    }
}
