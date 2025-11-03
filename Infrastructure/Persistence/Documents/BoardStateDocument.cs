using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConwayGameLifeApi.Infrastructure.Persistence.Documents;

public sealed class BoardStateDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string BoardId { get; set; } = default!;

    public int Generation { get; set; }

    public List<List<int>> Cells { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}
