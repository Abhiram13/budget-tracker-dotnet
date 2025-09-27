using System.Text.RegularExpressions;
using BudgetTracker.Interface;
using BudgetTracker.Defination;
using BudgetTracker.Repository;
using BudgetTracker.API.Transactions.List;
using BudgetTracker.API.Transactions.ByDate;
using ByBankResult = BudgetTracker.API.Transactions.ByBank.Result;

namespace BudgetTracker.Services
{
    public class TransactionService : MongoServices<Transaction>, ITransactionService
    {
        private readonly ICategoryService _categoryService;
        private readonly IBankService _bankService;
        private readonly IMongoContext _mongoContext;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            IBankService bankService, 
            ICategoryService categoryService, 
            ILogger<TransactionService> logger, 
            IMongoContext mongoContext
        ) : base(mongoContext.Transaction)
        {
            _logger = logger;
            _categoryService = categoryService;
            _bankService = bankService;
            _mongoContext = mongoContext;
        }

        public async Task Validations(Transaction transaction)
        {
            Category category = await _categoryService.SearchById(transaction.CategoryId);
            if (category == null || string.IsNullOrEmpty(category.Name))
            {
                // _logger.LogError("Invalid Category Id ({0}) provided", transaction.CategoryId);
                throw new BadRequestException($"Invalid Category Id ({transaction.CategoryId}) provided");
            }

            // throw error if both from and to bank fields are empty
            if (string.IsNullOrEmpty(transaction.FromBank) && string.IsNullOrEmpty(transaction.ToBank))
            {
                // _logger.LogError("Invalid From Bank ({0}) and To Bank ({1}) provided", transaction.FromBank, transaction.ToBank);
                throw new BadRequestException($"Invalid From Bank ({transaction.FromBank}) and To Bank ({transaction.ToBank}) provided");
            }

            await ValidateBanks(transaction.FromBank);
            await ValidateBanks(transaction.ToBank);
        }

        /// <summary>
        /// Validates if provided bank id is valid. If empty value is provided, simply returns.        
        /// </summary>
        /// <param name="bankId">Object ID of From bank or To bank</param>
        /// <returns></returns>
        /// <exception cref="BadRequestException">When invalid bank id is provided</exception>
        private async Task ValidateBanks(string? bankId)
        {
            if (string.IsNullOrEmpty(bankId)) return;

            Bank bank = await _bankService.SearchById(bankId);
            if (bank == null || string.IsNullOrEmpty(bank.Name))
            {
                // _logger.LogError("Invalid bank id ({0}) provided", bankId);
                throw new BadRequestException($"Invalid bank id ({bankId}) provided");
            }
        }

        public async Task<Result> List(QueryParams? queryParams, CancellationToken? cancellationToken)
        {
            TransactionList repository = new TransactionList(queryParams, _collection, cancellationToken);
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
            Data data = await new TransactionsByDate(date, _collection).GetTransactions();
            return data;
        }

        public async Task<API.Transactions.ByCategory.Result> GetByCategory(string categoryId, QueryParams? queryParams)
        {
            API.Transactions.ByCategory.Result result = await new TransactionsByCategory(categoryId, _collection, queryParams, _categoryService).GetData();
            return result;
        }

        public async Task<ByBankResult> GetByBank(string bankId, QueryParams queryParams)
        {
            ByBankResult result = await new TransactionsByBank(bankId, _collection, queryParams, _bankService).GetData();
            return result;
        }
    }
}