using MpRpServer.Data.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MpRpServer.Data.Enums
{
    public enum JobType
    {
        [Display(Name = "Авто заправка")]
        [BlipType(361)]
        GasStation = 1,

        [Display(Name = "Lawyer")]
        [BlipType(408)]
        Lawyer = 2,

        [Display(Name = "Автомеханик")]
        [BlipType(446)]
        Mechanic = 3,

        [Display(Name = "Bodyguard")]
        [BlipType(461)]
        Bodyguard = 4,

        [Display(Name = "Таксист")]
        [BlipType(198)]
        TaxiDriver = 5,

        [Display(Name = "Trucker")]
        [BlipType(85)]
        Trucker = 6,

        [Display(Name = "Грузчик")]
        [BlipType(311)]
        Loader = 7,

        [Display(Name = "Водитель автобуса")]
        [BlipType(513)]
        BusDriver = 8,

        [Display(Name = "Магазин одежды")]
        [BlipType(73)]
        ClothStore = 9,

        [Display(Name = "Аму-нация")]
        [BlipType(269)]
        AmmuNation = 10
    }
}
