using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Worker.Models;

public class RuleModel
{
    [BsonId] // Maps this property to MongoDB's _id field
    [BsonRepresentation(BsonType.ObjectId)] // Allows storing and retrieving ObjectId as a string
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // Generate a new Id if not provided
    public required string Name { get; set; }
    public string? Event { get; set; }
    public List<string> Actions { get; set; } = [];
    public string WorkflowId { get; set; }
}
