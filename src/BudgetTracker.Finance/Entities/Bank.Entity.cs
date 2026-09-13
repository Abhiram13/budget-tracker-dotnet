using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Entities;

public class Bank : MongoObject
{
    [BsonElement("name")]
    public string Name { get; private set; }
    
    private Bank() { }

    public static Bank Create(string name)
    {
        Bank bank = new Bank { Name = name };
        bank.SetModifiedAt();

        return bank;
    }
}