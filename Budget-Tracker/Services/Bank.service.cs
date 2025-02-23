using BudgetTracker.Defination;
using BudgetTracker.Interface;

namespace BudgetTracker.Services;

public class BankService : MongoServices<Bank>, IBankService
{
    public BankService() : base(Collection.Bank) { }
}