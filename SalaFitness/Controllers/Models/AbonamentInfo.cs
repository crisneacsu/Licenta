public enum AbonamentType
{
    Matinal = 1,
    MatinalParcare,
    AllHours,
    AllHoursParcare,
    None
}

public static class AbonamentInfo
{
    // Prețurile în lei
    private static readonly Dictionary<AbonamentType, decimal> _preturi = new Dictionary<AbonamentType, decimal>
    {
        { AbonamentType.Matinal, 140m },
        { AbonamentType.MatinalParcare, 170m },
        { AbonamentType.AllHours, 180m },
        { AbonamentType.AllHoursParcare, 210m }
    };

    public static decimal GetPret(AbonamentType tip)
    {
        return _preturi[tip];
    }

    public static string GetNume(AbonamentType tip)
    {
        // Poți personaliza cum vrei să apară numele
        return tip switch
        {
            AbonamentType.Matinal => "Matinal (până la ora 14)",
            AbonamentType.MatinalParcare => "Matinal + parcare privată",
            AbonamentType.AllHours => "All Hours",
            AbonamentType.AllHoursParcare => "All Hours + parcare privată",
            _ => "N/A"
        };
    }
}
