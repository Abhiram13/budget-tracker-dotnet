using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BudgetTracker.Core.Domain.Enums;
using BudgetTracker.Shared.Utility;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Core.Domain.Entities;

public class Due : MongoObject
{
    [Required, JsonPropertyName("name"), BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("payee"), BsonElement("payee"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Payee { get; set; } = string.Empty;

    [JsonPropertyName("principal_amount"), BsonElement("principal_amount")]
    public double PrincipalAmount { get; set; }

    [Required, JsonPropertyName("description"), BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("start_date"), BsonElement("start_date"), JsonConverter(typeof(UtcJsonConvertor))]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("end_date"), BsonElement("end_date"), JsonConverter(typeof(UtcJsonConvertor)), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? EndDate { get; set; } = null;

    [JsonPropertyName("comment"), BsonElement("comment")]
    public string Comment { get; set; } = string.Empty;

    [JsonPropertyName("status"), BsonElement("status")]
    public DueStatus Status { get; set; } = DueStatus.Active;
}