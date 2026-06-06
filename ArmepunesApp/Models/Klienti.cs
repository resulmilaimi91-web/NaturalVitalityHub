namespace ArmepunesApp.Models;

public class Klienti
{
    public int Id { get; set; }
    public string Emri { get; set; } = string.Empty;
    public string Mbiemri { get; set; } = string.Empty;
    public string Adresa { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NrLeternjoftimit { get; set; } = string.Empty;
    public string Shenime { get; set; } = string.Empty;

    public string EmriPlote => $"{Emri} {Mbiemri}";
}
