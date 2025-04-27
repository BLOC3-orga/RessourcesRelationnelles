using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R2Model.Entities
{
    public class Progression
    {
        public int Id { get; set; }
        public float Percentage { get; set; }
        public DateTime LastInteractionDate { get; set; }
    }
}
