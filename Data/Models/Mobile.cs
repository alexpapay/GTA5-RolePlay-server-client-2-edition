using System.ComponentModel.DataAnnotations;

namespace MpRpServer.Data
{
    public class Mobile
    {
        [Key]
        public int Id { get; set; }

        public int From { get; set; }
        public int To { get; set; }
        public string Sms { get; set; }
    }
}
