using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MOEX.Portfolio
{
    public class RecordConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Record).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var record = Activator.CreateInstance(objectType) as Record;
            if (reader.TokenType == JsonToken.StartArray)
            {
                if (reader.Read() && reader.TokenType == JsonToken.String) record.Ticker = reader.Value as string;
                if (reader.Read() && reader.TokenType == JsonToken.String) record.Date = DateTime.Parse(reader.Value as string);
                if (reader.Read() && reader.TokenType == JsonToken.String) record.Item2 = reader.Value as string;
                if (reader.Read() && reader.TokenType == JsonToken.String) record.Item3 = reader.Value as string;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item4 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item5 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item6 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item7 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item8 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item9 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item10 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item11 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item12 = (int)(long)reader.Value;
                if (reader.Read()) switch (reader.TokenType)
                    {
                        case JsonToken.Integer:
                            record.Price = (long)reader.Value;
                            break;
                        case JsonToken.Float:
                            record.Price = (double)reader.Value;
                            break;
                    }
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item14 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item15 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item16 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item17 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item18 = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Volume = (int)(long)reader.Value;
                if (reader.Read() && reader.TokenType == JsonToken.Integer) record.Item20 = (int)(long)reader.Value;
                while (reader.Read() && reader.TokenType != JsonToken.EndArray) ;
            }
            return record;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
    }

    [JsonConverter(typeof(RecordConverter))]
    public class Record
    {
        public string Ticker { get; set; }
        public DateTime Date { get; set; }
        public string Item2 { get; set; }
        public string Item3 { get; set; }
        public int Item4 { get; set; }
        public int Item5 { get; set; }
        public int Item6 { get; set; }
        public int Item7 { get; set; }
        public int Item8 { get; set; }
        public int Item9 { get; set; }
        public int Item10 { get; set; }
        public int Item11 { get; set; }
        public int Item12 { get; set; }
        public double Price { get; set; }
        public int Item14 { get; set; }
        public int Item15 { get; set; }
        public int Item16 { get; set; }
        public int Item17 { get; set; }
        public int Item18 { get; set; }
        public double Volume { get; set; }
        public int Item20 { get; set; }
    }

    public class Subset
    {
        public IList<Record> Data { get; } = new List<Record>();
    }

    public class Data
    {
        public Subset History { get; } = new Subset();
    }
}
