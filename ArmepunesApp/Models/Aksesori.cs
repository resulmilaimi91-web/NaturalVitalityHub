namespace ArmepunesApp.Models;

public class Aksesori
{
    public int Id { get; set; }
    public int TransaksioniId { get; set; }
    public string Emri { get; set; } = string.Empty;
    public int Sasia { get; set; } = 1;
    public string Shenime { get; set; } = string.Empty;
}
