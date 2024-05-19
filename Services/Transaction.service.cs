using Defination;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace Services;

public class TransactionService : MongoService<Transaction>, ITransactionService
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
    }

    public List<TransactionList> List()
    {
        List<TransactionList> list =  collection.AsQueryable()
            .GroupBy(t => t.Date)
            .Select(tr => new TransactionList() {
                Date = tr.First().Date.ToString(),
                Debit = tr.Where(d => d.Type == TransactionType.Debit && d.Date == tr.First().Date).Sum(d => d.Amount),
                Credit = tr.Where(d => d.Type == TransactionType.Credit && d.Date == tr.First().Date).Sum(d => d.Amount),
            }).ToList();

        return list;
    }
}