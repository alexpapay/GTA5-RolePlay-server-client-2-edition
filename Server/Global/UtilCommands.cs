using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using System.IO;

namespace MpRpServer.Server.Global
{
    public class UtilCMD : Script
    {
        [Command("gp")]
        public void GetPosition(Client player)
        {
            var posX = player.position.X;
            var posY = player.position.Y;
            var posZ = player.position.Z;
            var rot = player.rotation.Z;

            API.shared.sendChatMessageToPlayer(player, string.Format("~g~Server: ~w~PosX = {0}; PosY = {1}; PosZ={2}; Rot={3}", posX, posY, posZ, rot));
        }

        [Command("save", GreedyArg = true)]
        public void Command_Save(Client sender)
        {
            var pos = API.getEntityPosition(sender);
            var angle = API.getEntityRotation(sender);
            File.AppendAllText("savepos.txt", string.Format("{0}: {1} {2} {3} {4}\n", sender.name, pos.X, pos.Y, pos.Z, angle));
            API.sendNotificationToPlayer(sender, string.Format("Position saved in savepos.txt as: {0}", sender.name), true);
        }

        [Command("savecam", GreedyArg = true)]
        public void Command_SaveCam(Client sender)
        {
            var pos = API.getEntityPosition(sender);
            var angle = API.getEntityRotation(sender);
            File.AppendAllText("savepos.txt", string.Format("{0}: {1} {2} {3} {4}\n", sender.name, pos.X, pos.Y, pos.Z, angle));
            API.sendNotificationToPlayer(sender, string.Format("Position saved in savepos.txt as: {0}", sender.name), true);
        }
    }
}