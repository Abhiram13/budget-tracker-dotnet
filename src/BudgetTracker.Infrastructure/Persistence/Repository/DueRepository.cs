using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.Enums;
using BudgetTracker.Core.Domain.ValueObject.Dues;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BudgetTracker.Infrastructure.Repository;

public class DueRepository : MongoRepository<Due>, IDueRepository
{
    public DueRepository(IMongoContext context) : base(context.Dues) { }

    public async Task<DueDetails> GetDueDetailsAsync(string id)
    {
        BsonDocument[] pipelines = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument {
                {"_id", ObjectId.Parse(id)}
            }),
            new BsonDocument("$addFields", new BsonDocument {
                {"id", new BsonDocument("$toString", "$_id")}
            }),
            new BsonDocument("$lookup", new BsonDocument {
                {"from", "transactions"},
                {"localField", "id"},
                {"foreignField", "due_id"},
                {"as", "transactions"}
            }),
            new BsonDocument("$addFields", new BsonDocument {
                {"current_principle", new BsonDocument("$sum", "$transactions.amount")}
            }),
            new BsonDocument("$project", new BsonDocument {
                {"id", 0},
                { "transactions",  0}
            })
        };

        DueDetails details = await _collection.Aggregate<DueDetails>(pipelines).FirstOrDefaultAsync();

        return details;
    }

    public async Task<List<DueTransactions>> GetDueTranasactionsAsync(string id)
    {
        BsonDocument[] pipelines = new BsonDocument[] {
            new BsonDocument("$match", new BsonDocument {
                {"_id", ObjectId.Parse(id)}
            }),
            new BsonDocument("$addFields", new BsonDocument {
                {"id", new BsonDocument("$toString", "$_id")}
            }),
            new BsonDocument("$lookup", new BsonDocument {
                {"from", "transactions"},
                {"localField", "id"},
                {"foreignField", "due_id"},
                {"as", "transactions"}
            }),
            new BsonDocument("$unwind", "$transactions"),
            new BsonDocument("$project", new BsonDocument {
                {"_id", 0},
                {"amount", "$transactions.amount"},
                {"description", "$transactions.description"},
                {"transaction_id", new BsonDocument("$toString", "$transactions._id")},            
                { "type", "$transactions.type"}
            })
        };

        List<DueTransactions> dueTransactions = await _collection.Aggregate<DueTransactions>(pipelines).ToListAsync();

        return dueTransactions;
    }

    public async Task<List<DueList>> ListOfDuesAsync(DueStatus? dueStatus = null)
    {
        BsonDocument projectStage = new BsonDocument
        {
            {"$project", new BsonDocument {
                {"_id", 1},
                {"name", 1}
            }}
        };

        BsonDocument[] pipelines = new BsonDocument[] { projectStage };
        List<DueList> dues = await _collection.Aggregate<DueList>(pipelines).ToListAsync();

        return dues;
    }
}