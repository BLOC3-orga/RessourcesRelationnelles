using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R2Model.Entities
{
    public class Ressource
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime CreationDate { get; set; }

        [ForeignKey(nameof(Category))]
        public virtual ProgressionStatus CategoryId { get; set; }

        [ForeignKey(nameof(RessourceType))]
        public virtual ProgressionStatus RessourceTypeId { get; set; }

        [ForeignKey(nameof(RessourceStatus))]
        public virtual ProgressionStatus RessourceStatusId { get; set; }

    }
}
