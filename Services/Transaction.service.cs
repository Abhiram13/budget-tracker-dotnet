using Microsoft.AspNetCore.Mvc;
using Defination;
using Services;
using MongoDB.Driver;

namespace Services;

public class TransactionService : MongoService<Transaction>, ITransactionService
{
    public TransactionService() : base(Collection.Transaction) { }
}