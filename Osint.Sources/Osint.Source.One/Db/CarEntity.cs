namespace Osint.Source.One.Db;

public record CarEntity
{
    public int Id { get; set; }
    public string Owner { get; set; }
    public string Plate { get; set; }
}