using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ConwayGameLifeApi.Infrastructure.Persistence.Documents;

public sealed class BoardDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = default!;

    public int Rows { get; set; }

    public int Columns { get; set; }

    public DateTime CreatedAt { get; set; }
}
