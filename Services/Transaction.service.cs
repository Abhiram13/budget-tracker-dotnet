using Microsoft.AspNetCore.Mvc;
using Defination;
using Services;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Services;

public class TransactionService : MongoService<Transaction>, ITransactionService
{
    public TransactionService() : base(Collection.Transaction) { }
}