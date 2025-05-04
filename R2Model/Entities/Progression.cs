using R2Model.Enums;

namespace R2Model.Entities;

public class Progression
{
    public int Id { get; set; }
    public float Percentage { get; set; }
    public DateTime LastInteractionDate { get; set; }
    public ProgressionStatus Status { get; set; }
}
