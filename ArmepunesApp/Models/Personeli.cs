namespace ArmepunesApp.Models;

public class Personeli
{
    public int Id { get; set; }
    public string Emri { get; set; } = string.Empty;
    public string Mbiemri { get; set; } = string.Empty;
    public string Grada { get; set; } = string.Empty;
    public string Njesia { get; set; } = string.Empty;
    public string NrLegjitimacioni { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;

    public string EmriPlote => $"{Emri} {Mbiemri}";
}
