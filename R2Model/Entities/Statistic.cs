using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R2Model.Entities
{
    public class Statistic
    {
        public int Id { get; set; }
        public int RessourcesRead { get; set; }
        public int RessourcesCreated { get; set; }

        public ICollection<Ressource> Ressources { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
