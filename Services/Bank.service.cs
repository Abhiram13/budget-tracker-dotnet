using BudgetTracker.Defination;
using BudgetTracker.Injectors;

namespace BudgetTracker.Services;

public class BankService : MongoServices<Bank>, IBankService
{
    public BankService() : base(Collection.Bank) { }
}