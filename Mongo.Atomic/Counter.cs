using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Mongo.Atomic
{
    internal class Counter
    {
        [BsonId]
        public ObjectId ID { get; set; }

        [BsonElement("name")]
        public string Name {get; set;}

        [BsonElement("value")]
        public int Value {get; set;}

        [BsonElement("max")]
        public int Max {get; set;}
    }
}