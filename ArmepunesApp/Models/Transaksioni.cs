namespace ArmepunesApp.Models;

public class Transaksioni
{
    public int Id { get; set; }
    public int ArmaId { get; set; }
    public int PersoneliId { get; set; }
    public int KlientiId { get; set; }
    public string Tipi { get; set; } = string.Empty;
    public string DataOra { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    public string Qellimi { get; set; } = string.Empty;
    public string PersoneliQeDorzoi { get; set; } = string.Empty;
    public string PersoneliQeMorri { get; set; } = string.Empty;
    public string Shenime { get; set; } = string.Empty;
    public string Municioni { get; set; } = string.Empty;

    public string ArmaSerial { get; set; } = string.Empty;
    public string PersoneliEmri { get; set; } = string.Empty;
    public string KlientiEmri { get; set; } = string.Empty;
    public List<Aksesori> Aksesoret { get; set; } = new();
    public List<Municioni> Municionet { get; set; } = new();
}
