using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace com.b_velop.XmlRpc.Models
{
    [Table("State")]
    public class StateEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool State { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
