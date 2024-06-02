using Defination;
using Global;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Services
{
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

            Category? category = await SearchById(transaction.CategoryId, Collection.Category);

            if (category == null || string.IsNullOrEmpty(category.Name))
            {
                throw new InvalidDataException("Invalid category id provided");
            }

            // Bank? fromBank =  await SearchById(transaction.FromBank, Collection.Bank);

            // if (fromBank == null || string.IsNullOrEmpty(fromBank.Name))
            // {
            //     throw new InvalidDataException("Invalid from bank id provided");
            // }

            // Bank? toBank =  await SearchById(transaction.ToBank, Collection.Bank);

            // if (toBank == null || string.IsNullOrEmpty(toBank.Name))
            // {
            //     throw new InvalidDataException("Invalid to bank id provided");
            // }
        }

        public async Task<List<TransactionList>> List()
        {
            IAggregateFluent<Transaction> aggregate = collection.Aggregate();

            List<TransactionList> data = await aggregate
            .Group(a => a.Date, b => new TransactionList()
            {
                Debit = b.Where(c => c.Type == TransactionType.Debit).Sum(d => d.Amount),
                Credit = b.Where(c => c.Type == TransactionType.Credit).Sum(d => d.Amount),
                Date = b.First().Date,
                Count = b.Count(),
            })
            .Sort(Builders<TransactionList>.Sort.Ascending(x => x.Date))
            .ToListAsync();

            List<TransactionList> list = new List<TransactionList>();

            foreach (TransactionList transaction in data)
            {
                list.Add(new TransactionList()
                {
                    Count = transaction.Count,
                    Credit = transaction.Credit,
                    Date = transaction.Date,
                    Debit = transaction.Debit,
                });
            }

            return list;
        }

        public async Task<TransactionsByDate.Detail> ListByDate(string date)
        {
            Func<Task<TransactionsByDate.GroupAmounts>> fetchGroupAmounts = async () => {
                IAggregateFluent<Transaction> aggregate = collection.Aggregate().Match(Builders<Transaction>.Filter.Eq(t => t.Date, date));

                List<TransactionsByDate.GroupAmounts> list = await aggregate
                    .Group(a => a.Date, b => new TransactionsByDate.GroupAmounts(
                        b.Where(c => c.Type == TransactionType.Debit).Sum(d => d.Amount),
                        b.Where(c => c.Type == TransactionType.Credit).Sum(d => d.Amount),
                        b.Where(c => c.Type == TransactionType.PartialDebit).Sum(d => d.Amount),
                        b.Where(c => c.Type == TransactionType.PartialCredit).Sum(d => d.Amount)
                    ))
                    .ToListAsync();

                TransactionsByDate.GroupAmounts group = list.Count > 0 ? list[0] : new TransactionsByDate.GroupAmounts(0, 0, 0, 0);

                return group;
            };

            Func<Task<List<TransactionsByDate.List>>> fetchTransactions = async () => {
                IAggregateFluent<Transaction> aggregate = collection.Aggregate().Match(Builders<Transaction>.Filter.Eq(t => t.Date, date));
                ProjectionDefinition<Transaction, TransactionsByDate.List> projection = Builders<Transaction>.Projection
                    .Include(t => t.Amount)
                    .Include(t => t.Description)
                    .Include(t => t.Type);

                List<TransactionsByDate.List> list = await aggregate.Project(projection).ToListAsync();

                return list;
            };

            TransactionsByDate.GroupAmounts? group = await fetchGroupAmounts();
            List<TransactionsByDate.List> list = await fetchTransactions();

            return new TransactionsByDate.Detail(group, list);
        }
    }
}