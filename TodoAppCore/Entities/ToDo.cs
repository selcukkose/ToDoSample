namespace TodoAppCore.Entities;

public class ToDo
{
    public string id { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string imageUrl { get; set; } = string.Empty;
    public DateTime lastUpdateDate { get; set; }
}