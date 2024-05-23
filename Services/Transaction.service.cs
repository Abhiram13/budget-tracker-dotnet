using Defination;
using Global;
using MongoDB.Driver;
using System.Globalization;
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

    public async Task<List<TransactionList<string>>> ListByDate(string? date = null)
    {
        IAggregateFluent<Transaction> aggregate;

        if (string.IsNullOrEmpty(date))
        {
            aggregate = collection.Aggregate();
        }
        else
        {
            DateTime datetime = DateTime.Parse(date, CultureInfo.InvariantCulture);
            aggregate = collection.Aggregate().Match(Builders<Transaction>.Filter.Eq(t => t.Date, datetime));
        }

        Func<DateTime, string> ConvertDateToString = (DateTime dateTime) => {
            DateTime d = dateTime;
            TimeZoneInfo tz = TimeZoneInfo.Local;
            DateTime local = TimeZoneInfo.ConvertTimeFromUtc(d, tz);
            return local.ToString("D");
        };

        List<TransactionList<DateTime>> data =  await aggregate
        .Group(a => a.Date, b => new TransactionList<DateTime> () {
            Debit = b.Where(c => c.Type == TransactionType.Debit).Sum(d => d.Amount),
            Credit = b.Where(c => c.Type == TransactionType.Credit).Sum(d => d.Amount),
            Date = b.First().Date,
            Count = b.Count(),
        })
        .Sort(Builders<TransactionList<DateTime>>.Sort.Ascending(x => x.Date))
        .ToListAsync();

        List<TransactionList<string>> list = new List<TransactionList<string>>();

        foreach (TransactionList<DateTime> transaction in data)
        {
            list.Add(new TransactionList<string>() {
                Count = transaction.Count,
                Credit = transaction.Credit,
                Date = ConvertDateToString(transaction.Date),
                Debit = transaction.Debit,
            });
        }

        return list;
    }
}