using System.Text.RegularExpressions;
using BudgetTracker.Injectors;
using BudgetTracker.Defination;
using BudgetTracker.Repository;
using BudgetTracker.API.Transactions.List;
using BudgetTracker.API.Transactions.ByDate;

namespace BudgetTracker.Services
{
    public class TransactionService : MongoServices<Transaction>, ITransactionService
    {
        public TransactionService() : base(Collection.Transaction) { }

        public async Task Validations(Transaction transaction)
        {
            if (!Enum.IsDefined(typeof(TransactionType), transaction.Type))
            {
                throw new BadRequestException("Transaction type is invalid");
            }

            Regex descriptionRegex = new Regex(@"^[a-zA-Z0-9 ]*$");

            if (!descriptionRegex.IsMatch(transaction.Description))
            {
                throw new BadRequestException($"Description contains invalid characters. Recieved value: {transaction.Description}");
            }

            Category? category = await SearchById(transaction.CategoryId, Collection.Category);

            if (category == null || string.IsNullOrEmpty(category.Name))
            {
                throw new BadRequestException("Invalid category id provided");
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

        public async Task<Result> List(QueryParams? queryParams)
        {
            TransactionList repository = new TransactionList(queryParams, collection);
            Result result = new Result
            {
                TotalCount = await repository.GetCount()
            };

            if (queryParams?.Type == "transaction" || string.IsNullOrEmpty(queryParams?.Type))
            {
                result.Transactions = await repository.GetByTransactions();
            }

            if (queryParams?.Type == "category")
            {
                result.Categories = await repository.GetByCategories();
            }

            if (queryParams?.Type == "bank")
            {
                result.Banks = await repository.GetByBanks();
            }

            return result;
        }

        public async Task<Data> ListByDate(string date)
        {
            Data data = await new TransactionsByDate(date, collection).GetTransactions();

            return data;
        }
    }
}