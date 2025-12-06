using System.ComponentModel.DataAnnotations;

namespace Tbilink_BE.Domain.Entities
{
    public class Group (string name)
    {
        [Key]
        public string Name { get; set; } = name;

        public List<Connection> Connections { get; set; } = new List<Connection>();
    }
}
