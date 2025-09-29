using System;
using System.Text.Json.Serialization;
using BudgetTracker.Core.Domain.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Core.Domain.Entities;

public class Bill : MongoObject
{
    [JsonPropertyName("name"), BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description"), BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("billing_period"), BsonElement("billing_period")]
    public BillingPeriod BillingPeriod { get; set; } = new BillingPeriod();

    [JsonPropertyName("start_date"), BsonElement("start_date")]
    public DateTime? BillingStartDate { get; set; } = null;

    [JsonPropertyName("end_date"), BsonElement("end_date")]
    public DateTime? BillingEndDate { get; set; } = null;

    [JsonPropertyName("principal_amount"), BsonElement("principal_amount"), BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    public decimal PrincipalAmount { get; set; }

    [JsonPropertyName("installment_amount"), BsonElement("installment_amount"), BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    public decimal? InstallmentAmount { get; set; } = null;

    [JsonPropertyName("status"), BsonElement("status"), BsonRepresentation(MongoDB.Bson.BsonType.String), JsonConverter(typeof(JsonStringEnumConverter))]
    public DueStatus Status { get; set; } = DueStatus.Active;

    [JsonPropertyName("type"), BsonElement("type"), BsonRepresentation(MongoDB.Bson.BsonType.String), JsonConverter(typeof(JsonStringEnumConverter))]
    public BillingType Type { get; set; }
}

public class BillingPeriod
{
    [JsonPropertyName("value"), BsonElement("value")]
    public int Value { get; set; }

    [JsonPropertyName("unit"), BsonElement("unit"), JsonConverter(typeof(JsonStringEnumConverter)), BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public BillingUnit Unit { get; set; }
}