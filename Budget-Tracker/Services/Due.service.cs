using BudgetTracker.Defination;
using BudgetTracker.Interface;
using MongoDB.Bson;
using MongoDB.Driver;
using BudgetTracker.API.Dues;

namespace BudgetTracker.Services;

public class DueService : MongoServices<Due>, IDues
{
    private IBankService _bankService;
    
    public DueService(IBankService bankService, IMongoContext mongoContext) : base(mongoContext.Dues) 
    {
        _bankService = bankService;
    }

    public async Task InsertOneAsync(Due body)
    {
        await InserOne(body);
    }
    
    public async Task<DueTransactions> GetDueTransactionsAsync(string dueId)
    {
        BsonDocument[] pipelines = new BsonDocument[] {
            new BsonDocument {
                {"$match", new BsonDocument {
                    {"_id", ObjectId.Parse(dueId)}
                }}
            },
            new BsonDocument {
                {"$addFields", new BsonDocument {
                    {"id", new BsonDocument {
                        {"$toString", "$_id"}
                    }}
                }}
            },
            new BsonDocument {
                {"$lookup", new BsonDocument {
                    {"from", "transactions"},
                    {"localField", "id"},
                    {"foreignField", "due_id"},
                    {"as", "transactions"}
                }}
            },
            new BsonDocument {
                {"$addFields", new BsonDocument {
                    { "total", new BsonDocument {
                        {"$sum", "$transactions.amount"}
                    }}
                }}
            },
            new BsonDocument {
                {"$addFields", new BsonDocument {
                    { "current_principle", new BsonDocument {
                        {"$subtract", new BsonArray {"$principle_amount", "$total"}}
                    }}
                }}
            },
            new BsonDocument {
                {"$project", new BsonDocument {
                    {"_id", 0},
                    // {"description", 1},
                    // {"principle_amount", 1},
                    {"current_principle", 1},
                    // {"status", 1},
                    {"transactions", new BsonDocument {
                        {"amount", 1},
                        {"type", 1},
                        {"description", 1},                        
                    }},
                }}
            }
        };

        List<DueTransactions> list = await _collection.Aggregate<DueTransactions>(pipelines).ToListAsync();
        return list.FirstOrDefault() ?? new DueTransactions();
    }
}