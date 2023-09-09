namespace stats;

public class SprintOptions
{
    public int Count { get; set; }
    public string Pat { get; set; }
    public string Instance { get; set; }
    public string Project { get; set; }

    public int PlannedDays { get; set; }

    public int LateDays { get; set; }

    public string[] Teams { get; set; }

}