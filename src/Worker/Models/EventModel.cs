namespace Worker.Models;

public class EventModel
{
    public required string EventId { get; set;}
    public required string TenantId { get; set;}
    public string EventName { get; set;}
    public string EventData { get; set;}
}
