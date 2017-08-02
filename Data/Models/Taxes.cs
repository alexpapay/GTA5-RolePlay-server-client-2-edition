using System.ComponentModel.DataAnnotations;

namespace MpRpServer.Data
{
    public class Taxes
    {
        [Key]
        public int Id { get; set; }

        public int PersonalIncomeTax { get; set; }
        public int HouseA { get; set; }
        public int HouseB { get; set; }
        public int HouseC { get; set; }
        public int HouseD { get; set; }
    }
}
