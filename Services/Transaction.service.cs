using Defination;
using MongoDB.Driver;
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

    public List<TransactionList> List()
    {
        List<TransactionList> list =  collection.AsQueryable()
            .GroupBy(t => t.Date)
            .Select(tr => new TransactionList() {
                Date = tr.First().Date,
                Debit = tr.Where(d => d.Type == TransactionType.Debit && d.Date == tr.First().Date).Sum(d => d.Amount),
                Credit = tr.Where(d => d.Type == TransactionType.Credit && d.Date == tr.First().Date).Sum(d => d.Amount),
            }).ToList();

        return list;
    }
}