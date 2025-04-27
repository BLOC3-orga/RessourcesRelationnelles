using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R2Model.Entities
{
    public class ProgressionStatus
    {
        public int Id { get; set; }
        public bool NotStarted { get; set; }
        public bool InProgress { get; set; }
        public bool Completed { get; set; }

        [ForeignKey(nameof(ProgressionStatus))]
        public virtual ProgressionStatus ProgressionStatusId { get; set; }

    }
}
