using System.ComponentModel.DataAnnotations;

namespace MpRpServer.Data
{
    public class Wardrobe
    {
        [Key]
        public int Id { get; set; }

        public int CharacterId { get; set; }
        public string Masks { get; set; }
        public string Torsos { get; set; }
        public string Legs { get; set; }
        public string Bags { get; set; }
        public string Feets { get; set; }
        public string Accesses { get; set; }
        public string Undershirts { get; set; }
        public string Tops { get; set; }
        public string Hats { get; set; }
        public string Glasses { get; set; }
    }
}
