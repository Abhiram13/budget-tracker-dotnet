using Defination;
using Global;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using TransactionsByDate;

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

        public async Task<List<TransactionList<string>>> List(Defination.TransactionsList.QueryParams? queryParams)
        {
            IAggregateFluent<Transaction> aggregate = collection.Aggregate();
            FilterDefinition<Transaction> filter;

            if (!string.IsNullOrEmpty(queryParams?.month) && !string.IsNullOrEmpty(queryParams?.year))
            {
                BsonRegularExpression regex = new BsonRegularExpression($"{queryParams?.year}-{queryParams?.month}");                
                filter = Builders<Transaction>.Filter.Regex("date", regex);
            }
            else
            {
                string currentMonth = DateTime.Now.Month.ToString("D2");
                string currentYear = DateTime.Now.Year.ToString();
                BsonRegularExpression regex = new BsonRegularExpression($"{currentYear}-{currentMonth}");
                filter = Builders<Transaction>.Filter.Regex("date", regex);
            }

            List<TransactionList<double>> data = await aggregate.Match(filter).Group(a => a.Date, b => new TransactionList<double>() {
                Debit = b.Where(c => c.Type == TransactionType.Debit).Sum(d => d.Amount),
                Credit = b.Where(c => c.Type == TransactionType.Credit).Sum(d => d.Amount),
                Date = b.First().Date,
                Count = b.Count(),
                DateLink = b.First().Date,
            })
            .Sort(Builders<TransactionList<double>>.Sort.Ascending(x => x.Date))
            .ToListAsync();

            List<TransactionList<string>> list = new List<TransactionList<string>>();

            foreach (TransactionList<double> transaction in data)
            {
                list.Add(new TransactionList<string>()
                {
                    Count = transaction.Count,
                    Credit = string.Format("{0:#,##0.##}", transaction.Credit),
                    Debit = string.Format("{0:#,##0.##}", transaction.Debit),
                    Date = DateTime.Parse(transaction.Date).ToString("D"),
                    DateLink = transaction.Date,
                });
            }

            return list;
        }

        public async Task<Detail> ListByDate(string date)
        {
            IAggregateFluent<Transaction> aggregate = collection.Aggregate().Match(Builders<Transaction>.Filter.Eq(t => t.Date, date));
            
            Func<Task<GroupAmounts>> fetchGroupAmounts = async () => {
                List<GroupAmounts> list = await aggregate
                    .Group(a => a.Date, b => new GroupAmounts(
                        b.Where(c => c.Type == TransactionType.Debit).Sum(d => d.Amount),
                        b.Where(c => c.Type == TransactionType.Credit).Sum(d => d.Amount),
                        b.Where(c => c.Type == TransactionType.PartialDebit).Sum(d => d.Amount),
                        b.Where(c => c.Type == TransactionType.PartialCredit).Sum(d => d.Amount)
                    ))
                    .ToListAsync();

                GroupAmounts group = list.Count > 0 ? list[0] : new GroupAmounts(0, 0, 0, 0);

                return group;
            };

            Func<Task<List<Bank>>> banks = async () => {
                List<Bank> list = await Collection.Bank.Aggregate().ToListAsync();
                return list;
            };

            Func<Task<List<Category>>> categories = async () => {
                List<Category> list = await Collection.Category.Aggregate().ToListAsync();
                return list;
            };

            Func<Task<List<Transaction>>> transactions = async () => {
                ProjectionDefinition<Transaction> projection = Builders<Transaction>.Projection
                    .Include(t => t.Amount)
                    .Include(t => t.Description)
                    .Include(t => t.Type)
                    .Include(t => t.CategoryId)
                    .Include(t => t.FromBank)
                    .Include(t => t.ToBank);

                List<Transaction> list = await aggregate.Project<Transaction>(projection).ToListAsync();

                return list;
            };

            GroupAmounts? group = await fetchGroupAmounts();
            List<Transaction> list = await transactions();
            List<Data> data = new List<Data>();
            List<Bank> listOfBanks = await banks();
            List<Category> listOfCategories = await categories();

            foreach (Transaction transaction in list)
            {
                Bank? fromBank = listOfBanks.Where(b => b.Id == transaction.FromBank).FirstOrDefault();
                Bank? toBank = listOfBanks.Where(b => b.Id == transaction.ToBank).FirstOrDefault();
                Category? category = listOfCategories.Where(c => c.Id == transaction.CategoryId).FirstOrDefault();

                data.Add(new Data(
                    amount: transaction.Amount,
                    description: transaction.Description,
                    type: transaction.Type,
                    fromBank: fromBank?.Name,
                    toBank: toBank?.Name,
                    category: category?.Name ?? "",
                    transactionId: transaction.Id
                ));
            }

            return new Detail(group, data);
        }
    }
}