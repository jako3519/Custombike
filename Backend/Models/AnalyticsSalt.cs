namespace Backend.Models;

// Dagens hemmelige salt til besoegs-hashen. Kun dagens raekke beholdes; aeldre slettes,
// saa en hash ikke kan foeres tilbage til en IP-adresse eller kobles paa tvaers af dage.
public class AnalyticsSalt
{
    public DateOnly Day { get; set; }              // dato i dansk tid (Europe/Copenhagen)
    public byte[] Salt { get; set; } = [];
}
