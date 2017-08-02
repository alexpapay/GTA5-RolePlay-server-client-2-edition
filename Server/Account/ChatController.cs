using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using MpRpServer.Data;
using MpRpServer.Server.Admin;
using MpRpServer.Server.Characters;
using MpRpServer.Server.DBManager;
using System.Collections.Generic;
using System.Linq;

namespace MpRpServer.Server
{
    public class ChatController : Script
    {         
        public ChatController()
        {
            API.onChatMessage += OnChatMessageHandler;
            API.onChatCommand += OnChatCommandHandler;
        }

        public static void SendMessageInMyGroup (Client player, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            var character = characterController.Character;
            var formatName = character.Name.Replace("_", " ");
            
            var allGroupPlayers = ContextFactory.Instance.Character.Where(x => x.GroupType == character.GroupType).ToList();
            
            foreach (var groupPlayer in allGroupPlayers)
            {
                if (groupPlayer.Online)
                {
                    Client target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == groupPlayer.SocialClub);
                    if (target == null) return;
                    API.shared.sendChatMessageToPlayer(target, "Игрок " + formatName + " говорит группе:\n" + message);
                }                
            }
        }

        public static void SendMessageInGroup(int groupType, string message)
        {
            var allGroupPlayers = ContextFactory.Instance.Character.Where(x => x.GroupType == groupType).ToList();

            foreach (var groupPlayer in allGroupPlayers)
            {
                if (groupPlayer.Online)
                {
                    var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == groupPlayer.SocialClub);
                    if (target == null) continue;
                    API.shared.sendChatMessageToPlayer(target, message);
                }
            }
        }

        public void OnChatMessageHandler(Client player, string message, CancelEventArgs e)
        {
            
            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            var formatName = characterController.Character.Name.Replace("_", " ");

            SendProxMessage(player, 15.0f, "~#FFFFFF~", formatName + " says: " + message);
            e.Cancel = true;
        }

        public void OnChatCommandHandler(Client player, string arg, CancelEventArgs ce)
        {
            if (API.getEntityData(player, "DOWNLOAD_FINISHED") != true) ce.Cancel = true;

            CharacterController characterController = player.getData("CHARACTER");
            if (characterController == null) return;
            if (!arg.Contains("/login") && !arg.Contains("/register"))
            {
                if (characterController == null) ce.Cancel = true;
            }
            
        }

		public static void LoginMessages(Client player, Character character)
		{
			if (character.Admin > 0)
			{
				API.shared.sendChatMessageToPlayer(player, "~#FFCCBB~You are a " + AdminController.GetAdminRank(character.Admin) + " on this server. Please respect fellow staff and players.");
			}
		}

        public static void SendProxMessage(Client player, float radius, string color, string msg)
        {
            var proxPlayers = API.shared.getPlayersInRadiusOfPlayer(radius, player);
            foreach (var target in proxPlayers)
            {
                API.shared.sendChatMessageToPlayer(target, color, msg);
            }
        }

        // Chat-related commands:
        [Command("me", GreedyArg = true)]
        public void ME_Command(Client player, string msg)
        {
            CharacterController characterController = player.getData("CHARACTER");            
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 15.0f, "~#C2A2DA~", formatName + " " + msg);
        }

        [Command("melow", GreedyArg = true)]
        public void MELow_Command(Client player, string msg)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 7.5f, "~#C2A2DA~", formatName + " " + msg);
        }

        [Command("do", GreedyArg = true)] // do command 
        public void DO_Command(Client player, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 15.0f, "~#C2A2DA~", "* " + message + " (( " + formatName + " ))");
        }

        [Command("dolow", GreedyArg = true)] // do command 
        public void DOLow_Command(Client player, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 7.5f, "~#C2A2DA~", "* " + message + " (( " + formatName + " ))");
        }

        [Command("s", Alias = "shout", GreedyArg = true)]
        public void S_Command(Client player, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 25.0f, "~#FFFFFF~", formatName + " shouts: " + message + "!");
        }
        [Command("w", Alias = "whisper", GreedyArg = true)]
        public void W_Command(Client player, string message)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 7.5f, "~#FFFFFF~", formatName + " whispers: " + message);
        }

        [Command("b", GreedyArg = true)]
        public void B_Command(Client player, string msg)
        {
            CharacterController characterController = player.getData("CHARACTER");
            var formatName = characterController.Character.Name.Replace("_", " ");
            SendProxMessage(player, 15.0f, "~#FFFFFF~", "(( " + formatName + ": " + msg + " ))");
        }

        [Command("o", GreedyArg = true)]
        public void OOC_Command(Client player, string msg)
        {
            foreach (var c in API.getAllPlayers())
            {
                CharacterController characterController = player.getData("CHARACTER");
                var formatName = characterController.Character.Name.Replace("_", " ");
                API.sendChatMessageToPlayer(c, "~#FFFFFF~", "(( " + formatName + ": " + msg + " ))");
            }
        }
        
        [Command("pm", "~y~usage: ~w~/[player id] [message]")]
        public void PmCommand(Client player, int oid, string message)
        {
            var targetCharacter = ContextFactory.Instance.Character.FirstOrDefault(x => x.OID == oid);
            if (targetCharacter == null)
            {
                API.sendNotificationToPlayer(player, "~r~[ОШИБКА]: ~w~Вы ввели неверный пользовательский ID.");
                return;
            }
            var target = API.shared.getAllPlayers().FirstOrDefault(x => x.socialClubName == targetCharacter.SocialClub);
            if (target == null) return;

            var formatName = targetCharacter.Name.Replace("_", " ");
            API.sendChatMessageToPlayer(target, "~#FFFFFF~", "(( " + formatName + ": " + message + " ))");
        }
    }
}
