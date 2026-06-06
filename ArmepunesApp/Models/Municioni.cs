namespace ArmepunesApp.Models;

public class Municioni
{
    public int Id { get; set; }
    public int TransaksioniId { get; set; }
    public string Emri { get; set; } = "";
    public string Lloji { get; set; } = "";
    public string Kalibri { get; set; } = "";
    public int Sasia { get; set; } = 1;
    public string Njesia { get; set; } = "copë";
    public string Shenime { get; set; } = "";
}
