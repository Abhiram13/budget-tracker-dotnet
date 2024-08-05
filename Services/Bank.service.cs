using Defination;

namespace Services;

public class BankService : MongoServices<Bank>, IBankService
{
    public BankService() : base(Collection.Bank) { }
}