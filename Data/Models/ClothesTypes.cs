using System.ComponentModel.DataAnnotations;

namespace MpRpServer.Data
{
    public class ClothesTypes
    {
        [Key]
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public int MaskSlot { get; set; }
        public int MaskDraw { get; set; }
        public int TorsoSlot { get; set; }
        public int TorsoDraw { get; set; }
        public int LegsSlot { get; set; }
        public int LegsDraw { get; set; }
        public int BagsSlot { get; set; }
        public int BagsDraw { get; set; }
        public int FeetSlot { get; set; }
        public int FeetDraw { get; set; }
        public int AccessSlot { get; set; }
        public int AccessDraw { get; set; }
        public int UndershirtSlot { get; set; }
        public int UndershirtDraw { get; set; }
        public int ArmorSlot { get; set; }
        public int ArmorDraw { get; set; }
        public int TopsSlot { get; set; }
        public int TopsDraw { get; set; }
        public int HatsSlot { get; set; }
        public int HatsDraw { get; set; }
        public int GlassesSlot { get; set; }
        public int GlassesDraw { get; set; }

        public int Type { get; set; }

        // Type == 888 | 8880 : Null (Трусы босяком)
        // Type == 999 | 9990 : Null (Default)

        // Type == 700 | 7000  : Worker_Loader
        // Type == 701 | 7010  : BusDriver

        // Type == 10 | 100 : Police
        // Type == 20 | 200 : Emergency
        // Type == 30 | 300 : FBI

        // Type == 111 | 1110 : Meria
        // Type == 121 | 1210 : Autoschool

        // Type == 131 | 1310 : Gangs_Start
        // ....
        // Type == 171 | 1710 : Gangs_End

        // Type == 201 | 2010 : Army_Soldier
        // Type == 202 | 2020 : Army_Officer
        // Type == 203 | 2030 : Army_General

        // Type == 301 | 3010 : Mafia_Start
        // ....
        // Type == 321 | 3210 : Mafia_End
    }
}
