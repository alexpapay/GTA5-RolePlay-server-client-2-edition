using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using MpRpServer.Data;
using MpRpServer.Server.DBManager;
using System.Linq;

namespace MpRpServer.Server
{
    class ClothesManager : Script
    {
        // WORK WITH WARDROBE:
        public static void WardrobeInit(Character character)
        {
            var playerInitWardrobe = new Wardrobe
            {
                CharacterId = character.Id,
                Masks = "0,0",
                Torsos = "0,0",
                Legs = "0,0",
                Bags = "0,0",
                Feets = "0,0",
                Accesses = "0,0",
                Undershirts = "0,0",
                Tops = "0,0",
                Hats = "11,0",
                Glasses = "0,0"
            };
            ContextFactory.Instance.Wardrobe.Add(playerInitWardrobe);
            ContextFactory.Instance.SaveChanges();
        }
        public static void SetPlayerClothType(Character character, int type, int slot)
        {
            var characterClothes = ContextFactory.Instance.Clothes.FirstOrDefault(x => x.CharacterId == character.Id);
            if (characterClothes == null) return;
            switch (type)
            {
                case 1: characterClothes.MaskSlot = slot; break;
                case 3: characterClothes.TorsoSlot = slot; break;
                case 4: characterClothes.LegsSlot = slot; break;
                case 5: characterClothes.BagsSlot = slot; break;
                case 6: characterClothes.FeetSlot = slot; break;
                case 8: characterClothes.UndershirtSlot = slot; break;
                case 7: characterClothes.AccessSlot = slot; break;
                case 11: characterClothes.TopsSlot = slot; break;
                case 50: characterClothes.HatsSlot = slot; break;
                case 51: characterClothes.GlassesSlot = slot; break;
            }
            ContextFactory.Instance.SaveChanges();
        }
        public static void SetPlayerClothDraw(Character character, int type, int draw)
        {
            var characterClothes = ContextFactory.Instance.Clothes.FirstOrDefault(x => x.CharacterId == character.Id);
            if (characterClothes == null) return;
            switch (type)
            {
                case 1: characterClothes.MaskDraw = draw; break;
                case 3: characterClothes.TorsoDraw = draw; break;
                case 4: characterClothes.LegsDraw = draw; break;
                case 5: characterClothes.BagsDraw = draw; break;
                case 6: characterClothes.FeetDraw = draw; break;
                case 8: characterClothes.UndershirtDraw = draw; break;
                case 7: characterClothes.AccessDraw = draw; break;
                case 11: characterClothes.TopsDraw = draw; break;
                case 50: characterClothes.HatsDraw = draw; break;
                case 51: characterClothes.GlassesDraw = draw; break;
            }
            ContextFactory.Instance.SaveChanges();
        }

        // WORK WITH PRESETTED SKINS:
        public static void SetPlayerSkinClothesToDb(Client player, int type, Character character, bool isFirstTime)
        {
            if (character == null) return;

            var playerClothes = ContextFactory.Instance.Clothes.FirstOrDefault(x => x.CharacterId == character.Id);
            var typeClothes = ContextFactory.Instance.ClothesTypes.FirstOrDefault(x => x.Type == type);

            if (playerClothes == null || typeClothes == null) return;

            ClothesAssigment(playerClothes, typeClothes);
            ContextFactory.Instance.SaveChanges();

            SetPlayerSkinClothes(player, type, character, isFirstTime);
        }
        public static void SetPlayerSkinClothes(Client player, int type, Character character, bool isFirstTime)
        {
            Clothes playerClothes = null;
            ClothesTypes playerTypeClothes = null;

            if (type == 0) playerClothes = ContextFactory.Instance.Clothes.FirstOrDefault(x => x.CharacterId == character.Id);
            else playerTypeClothes = ContextFactory.Instance.ClothesTypes.FirstOrDefault(x => x.Type == type);

            // FIRST REGISTARTION of character clothes:
            if (isFirstTime)
            {
                var isMale = character.Model == 1885233650;

                playerClothes = new Clothes();
                var typeClothes = isMale ? 
                    ContextFactory.Instance.ClothesTypes.FirstOrDefault(x => x.Type == 999) : 
                    ContextFactory.Instance.ClothesTypes.FirstOrDefault(x => x.Type == 9990);
                if (typeClothes == null) return;

                playerClothes.CharacterId = character.Id;
                character.ActiveClothes = isMale ? 999 : 9990;
                ClothesAssigment(playerClothes, typeClothes);

                ContextFactory.Instance.Clothes.Add(playerClothes);
                ContextFactory.Instance.SaveChanges();

                //API.shared.setPlayerClothes(player, 0, 0, 0);
                SetPlayerClothesAccessory(player, playerClothes);
            }

            // PLAYER CLOTHES (table Clothes):
            if (!isFirstTime && type == 0) SetPlayerClothesAccessory(player, playerClothes);

            // FRACTION CLOTHES (table ClothesTypes):
            if (!isFirstTime && type != 0) SetPlayerTypeClothesAccessory(player, playerTypeClothes);
        }

        // PRIVATE methods:
        private static void SetPlayerClothesAccessory(Client player, Clothes playerClothes)
        {
            API.shared.setPlayerClothes(player, 1, playerClothes.MaskSlot, playerClothes.MaskDraw);
            API.shared.setPlayerClothes(player, 3, playerClothes.TorsoSlot, playerClothes.TorsoDraw);
            API.shared.setPlayerClothes(player, 4, playerClothes.LegsSlot, playerClothes.LegsDraw);
            API.shared.setPlayerClothes(player, 5, playerClothes.BagsSlot, playerClothes.BagsDraw);
            API.shared.setPlayerClothes(player, 6, playerClothes.FeetSlot, playerClothes.FeetDraw);
            API.shared.setPlayerClothes(player, 7, playerClothes.AccessSlot, playerClothes.AccessDraw);
            API.shared.setPlayerClothes(player, 8, playerClothes.UndershirtSlot, playerClothes.UndershirtDraw);
            API.shared.setPlayerClothes(player, 9, playerClothes.ArmorSlot, playerClothes.ArmorDraw);
            API.shared.setPlayerClothes(player, 11, playerClothes.TopsSlot, playerClothes.TopsDraw);
            API.shared.setPlayerAccessory(player, 0, playerClothes.HatsSlot, playerClothes.HatsDraw);
            API.shared.setPlayerAccessory(player, 1, playerClothes.GlassesSlot, playerClothes.GlassesDraw);
        }
        private static void SetPlayerTypeClothesAccessory(Client player, ClothesTypes playerClothes)
        {
            API.shared.setPlayerClothes(player, 1, playerClothes.MaskSlot, playerClothes.MaskDraw);
            API.shared.setPlayerClothes(player, 3, playerClothes.TorsoSlot, playerClothes.TorsoDraw);
            API.shared.setPlayerClothes(player, 4, playerClothes.LegsSlot, playerClothes.LegsDraw);
            API.shared.setPlayerClothes(player, 5, playerClothes.BagsSlot, playerClothes.BagsDraw);
            API.shared.setPlayerClothes(player, 6, playerClothes.FeetSlot, playerClothes.FeetDraw);
            API.shared.setPlayerClothes(player, 7, playerClothes.AccessSlot, playerClothes.AccessDraw);
            API.shared.setPlayerClothes(player, 8, playerClothes.UndershirtSlot, playerClothes.UndershirtDraw);
            API.shared.setPlayerClothes(player, 9, playerClothes.ArmorSlot, playerClothes.ArmorDraw);
            API.shared.setPlayerClothes(player, 11, playerClothes.TopsSlot, playerClothes.TopsDraw);
            API.shared.setPlayerAccessory(player, 0, playerClothes.HatsSlot, playerClothes.HatsDraw);
            API.shared.setPlayerAccessory(player, 1, playerClothes.GlassesSlot, playerClothes.GlassesDraw);
        }
        private static void ClothesAssigment (Clothes playerClothes, ClothesTypes typeClothes)
        {
            playerClothes.MaskSlot = typeClothes.MaskSlot;
            playerClothes.MaskDraw = typeClothes.MaskDraw;
            playerClothes.TorsoSlot = typeClothes.TorsoSlot;
            playerClothes.TorsoDraw = typeClothes.TorsoDraw;
            playerClothes.LegsSlot = typeClothes.LegsSlot;
            playerClothes.LegsDraw = typeClothes.LegsDraw;
            playerClothes.BagsSlot = typeClothes.BagsSlot;
            playerClothes.BagsDraw = typeClothes.BagsDraw;
            playerClothes.FeetSlot = typeClothes.FeetSlot;
            playerClothes.FeetDraw = typeClothes.FeetDraw;
            playerClothes.AccessSlot = typeClothes.AccessSlot;
            playerClothes.AccessDraw = typeClothes.AccessDraw;
            playerClothes.UndershirtSlot = typeClothes.UndershirtSlot;
            playerClothes.UndershirtDraw = typeClothes.UndershirtDraw;
            playerClothes.ArmorSlot = typeClothes.ArmorSlot;
            playerClothes.ArmorDraw = typeClothes.ArmorDraw;
            playerClothes.TopsSlot = typeClothes.TopsSlot;
            playerClothes.TopsDraw = typeClothes.TopsDraw;
            playerClothes.HatsSlot = typeClothes.HatsSlot;
            playerClothes.HatsDraw = typeClothes.HatsDraw;
            playerClothes.GlassesSlot = typeClothes.GlassesSlot;
            playerClothes.GlassesDraw = typeClothes.GlassesDraw;
        }
        
        // SETTING UP CLOTHES:
        public static int SetFractionClothes(Client player, int fractionId, Character character)
        {
            var isMale = character.Model == 1885233650;
            // POLICE:
            if (fractionId == 114)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 14 : 14011, character, false);
                return isMale ? 14 : 14011;
            }

            // MEDICS:
            if (fractionId == 210)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 22 : 220, character, false);
                return isMale ? 22 : 220;
            }

            // FBI:
            if (fractionId == 310)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 31 : 310, character, false);
                return isMale ? 31 : 310;
            }

            // MERIA:
            if (fractionId == 1106)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 115 : 1150, character, false);
                return isMale ? 115 : 1150;
            }

            // GANGS:
            if (fractionId == 1310)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 131 : 1310, character, false);
                return isMale ? 131 : 1310;
            }
            if (fractionId == 1410)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 141 : 1410, character, false);
                return isMale ? 141 : 1410;
            }
            if (fractionId == 1510)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 151 : 1510, character, false);
                return isMale ? 151 : 1510;
            }
            if (fractionId == 1610)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 161 : 1610, character, false);
                return isMale ? 161 : 1610;
            }
            if (fractionId == 1710)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 171 : 1710, character, false);
                return isMale ? 171 : 1710;
            }

            // ARMY:
            if (fractionId == 2001 || fractionId == 2101 ||
                fractionId == 2002 || fractionId == 2102)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 201 : 2010, character, false);
                return isMale ? 201 : 2010;
            }
            if (fractionId >= 2003 && fractionId == 2014 ||
                fractionId >= 2103 && fractionId == 2114)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 202 : 2020, character, false);
                return isMale ? 202 : 2020;
            }
            if (fractionId == 2015 || fractionId == 2115)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 203 : 2030, character, false);
                return isMale ? 203 : 2030;
            }

            // RUSSIAN MAFIA CLOTHES:
            if (fractionId == 3001 || fractionId == 3002)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 301 : 3010, character, false);
                return isMale ? 301 : 3010;
            }
            if (fractionId == 3003 || fractionId == 3004 || fractionId == 3005)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 303 : 3030, character, false);
                return isMale ? 303 : 3030;
            }
            if (fractionId == 3006 || fractionId == 3007)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 306 : 3060, character, false);
                return isMale ? 306 : 3060;
            }
            if (fractionId == 3008)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 307 : 3070, character, false);
                return isMale ? 307 : 3070;
            }
            if (fractionId == 3009)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 308 : 3080, character, false);
                return isMale ? 308 : 3080;
            }
            if (fractionId == 3010)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 309 : 3090, character, false);
                return isMale ? 309 : 3090;
            }

            // MAFIA LKN:
            if (fractionId == 3101 || fractionId == 3102)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 311 : 3110, character, false);
                return isMale ? 311 : 3110;
            }
            if (fractionId == 3103 || fractionId == 3104 || fractionId == 3105)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 313 : 3130, character, false);
                return isMale ? 313 : 3130;
            }
            if (fractionId == 3106 || fractionId == 3107)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 316 : 3160, character, false);
                return isMale ? 316 : 3160;
            }
            if (fractionId == 3108)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 317 : 3170, character, false);
                return isMale ? 317 : 3170;
            }
            if (fractionId == 3109)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 318 : 3180, character, false);
                return isMale ? 318 : 3180;
            }
            if (fractionId == 3110)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 319 : 3190, character, false);
                return isMale ? 319 : 3190;
            }

            // MAFIA YAKOODZA:
            if (fractionId == 3201 || fractionId == 3202)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 321 : 3210, character, false);
                return isMale ? 321 : 3210;
            }
            if (fractionId == 3203 || fractionId == 3204 || fractionId == 3205)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 323 : 3230, character, false);
                return isMale ? 323 : 3230;
            }
            if (fractionId == 3206 || fractionId == 3207)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 326 : 3260, character, false);
                return isMale ? 326 : 3260;
            }
            if (fractionId == 3208)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 327 : 3270, character, false);
                return isMale ? 327 : 3270;
            }
            if (fractionId == 3209)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 328 : 3280, character, false);
                return isMale ? 328 : 3280;
            }
            if (fractionId == 3210)
            {
                SetPlayerSkinClothesToDb(player, isMale ? 329 : 3290, character, false);
                return isMale ? 329 : 3290;
            }

            return isMale ? 999 : 9990; // Default clothes
        }
    }
}
