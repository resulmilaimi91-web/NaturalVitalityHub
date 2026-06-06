namespace ArmepunesApp.Models;

public class Arma
{
    public int Id { get; set; }
    public string NumerSerial { get; set; } = string.Empty;
    public string Lloji { get; set; } = string.Empty;
    public string Marka { get; set; } = string.Empty;
    public string Modeli { get; set; } = string.Empty;
    public string Kalibri { get; set; } = string.Empty;
    public string Vendlindja { get; set; } = string.Empty;
    public string Statusi { get; set; } = "Ne Magazine";
    public string Shenime { get; set; } = string.Empty;
    public string DataRegjistrimit { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public string NrInventari { get; set; } = string.Empty;
}
