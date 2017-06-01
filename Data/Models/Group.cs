using MpRpServer.Data.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MpRpServer.Data
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [StringLength(32)]
        public string Name { get; set; }

        public GroupType Type { get; set; }
        public GroupExtraType ExtraType { get; set; }
        public int MoneyBank { get; set; }

        public virtual ICollection<Vehicle> Vehicles { get; set; }
        public virtual ICollection<Property> Properties { get; set; }
        
    }
}