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
        if (!string.IsNullOrEmpty(body.ToBank))
        {
            Bank toBank = await _bankService.SearchById(body.ToBank);
            if (string.IsNullOrEmpty(toBank.Name))
            {
                throw new BadRequestException("Invalid To bank id provided");
            }
        }

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
                {"$project", new BsonDocument {
                    {"_id", 0},
                    {"description", 1},
                    {"amount", 1},
                    {"status", 1},
                    {"transactions", new BsonDocument {
                        {"amount", 1},
                        {"type", 1},
                        {"description", 1},
                        {"date", 1},
                        {"from_bank", 1},
                        {"to_bank", 1},
                    }},
                }}
            }
        };

        List<DueTransactions> list = await _collection.Aggregate<DueTransactions>(pipelines).ToListAsync();
        return list.FirstOrDefault() ?? new DueTransactions();
    }
}