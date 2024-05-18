using Defination;

namespace Services;

public class BankService : MongoService<Bank>, IBankService
{
    public BankService() : base(Collection.Bank) { }
}