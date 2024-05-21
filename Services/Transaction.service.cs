using Defination;
using Global;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Services;

public class TransactionService : MongoServices<Transaction>, ITransactionService
{
    public TransactionService() : base(Collection.Transaction) { }

    public async Task Validations(Transaction transaction)
    {
        if (!Enum.IsDefined(typeof(TransactionType), transaction.Type))
        {
            throw new InvalidDataException("Transaction type is invalid");
        }

        Regex descriptionRegex = new Regex(@"^[a-zA-Z0-9 ]*$");

        if (!descriptionRegex.IsMatch(transaction.Description))
        {
            throw new InvalidDataException("Description contains invalid characters");
        }

        Category? category =  await SearchById(transaction.CategoryId, Collection.Category);

        if (category == null || string.IsNullOrEmpty(category.Name))
        {
            throw new InvalidDataException("Invalid category id provided");
        }

        Bank? fromBank =  await SearchById(transaction.FromBank, Collection.Bank);

        if (fromBank == null || string.IsNullOrEmpty(fromBank.Name))
        {
            throw new InvalidDataException("Invalid from bank id provided");
        }

        Bank? toBank =  await SearchById(transaction.ToBank, Collection.Bank);

        if (toBank == null || string.IsNullOrEmpty(toBank.Name))
        {
            throw new InvalidDataException("Invalid to bank id provided");
        }
    }

    public async Task<List<TransactionList>> List()
    {
        List<TransactionList> list =  await collection.Aggregate()
        .Group(a => a.Date, b => new TransactionList () {
            Debit = b.Where(c => c.Type == TransactionType.Debit).Sum(d => d.Amount),
            Credit = b.Where(c => c.Type == TransactionType.Credit).Sum(d => d.Amount),
            Date = b.First().Date.Date,
            Count = b.Count()
        })
        .Sort(Builders<TransactionList>.Sort.Ascending(x => x.Date))
        .ToListAsync();

        return list;
    }
}