using Discord;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;

namespace MpRpServer.Server.Global
{
    public class CEFController
    {
        private static DiscordClient _client;
        /*
        public static async void DoVoiceAsync()
        {
            _client = new DiscordClient();

            _client.UsingAudio(x => // Opens an AudioConfigBuilder so we can configure our AudioService
            {
                x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
            });

            await Task.Run(async () =>
             {
                 await Task.Delay(TimeSpan.FromSeconds(4));
                 try
                 {
                     var voiceChannel = _client.GetChannel(321011275502977025);
                    //_vClient = await voiceChannel.JoinAudio();
                    var vClient = await _client.GetService<AudioService>().Join(voiceChannel);
                 }
                 catch (Exception e)
                 {
                     Debug.Write(e + " Exception caught. [ALERT]");
                 }
             });

            //var voiceChannel = _client.FindServers("MP RP Server"); // Finds the first VoiceChannel on the server 'Music Bot Server'
            //if (voiceChannel == null) return;

            //var vClient = await _client.GetService<AudioService>() // We use GetService to find the AudioService that we installed earlier. In previous versions, this was equivelent to _client.Audio()
               // .Join(voiceChannel.VoiceChannels.FirstOrDefault()); // Join the Voice Channel, and return the IAudioClient.
        }
        */
        public static void OpenVoice(Client player)
        {
            API.shared.triggerClientEvent(player, "CEFVoiceOn");
            API.shared.sendChatMessageToPlayer(player, GlobalVars.WebRTCServerConnectionString);
        }

        public static void Close(Client player)
        {
            API.shared.triggerClientEvent(player, "CEFDestroy");
        }

        public static void ShowCursor(Client player)
        {
            if (player.getData("CURSOR") != null)
            {
                API.shared.triggerClientEvent(player, "CEFController_ShowCursor", false);
                player.resetData("CURSOR");
            }
            else
            {
                API.shared.triggerClientEvent(player, "CEFController_ShowCursor", true);
                player.setData("CURSOR", true);
            }

        }
    }
}
