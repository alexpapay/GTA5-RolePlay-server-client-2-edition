using GrandTheftMultiplayer.Server.Elements;
using MpRpServer.Server.Characters;

namespace MpRpServer.Server.Extensions
{
    public static class ClientExtensions
    {
        public static CharacterController GetCharacterController(this Client client)
        {
            return client.getData("CHARACTER") as CharacterController;
        }
    }
}
