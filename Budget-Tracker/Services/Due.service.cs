using BudgetTracker.Defination;
using BudgetTracker.Interface;
using MongoDB.Bson;
using MongoDB.Driver;
using BudgetTracker.API.Dues;
using System.Net;

namespace BudgetTracker.Services;

public class DueService : MongoServices<Due>, IDues
{
    private IBankService _bankService;
    private IMongoContext _context;

    public DueService(IBankService bankService, IMongoContext mongoContext) : base(mongoContext.Dues)
    {
        _bankService = bankService;
        _context = mongoContext;
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
                    { "current_principal", new BsonDocument {
                        {"$subtract", new BsonArray {"$principal_amount", "$total"}}
                    }}
                }}
            },
            new BsonDocument {
                {"$project", new BsonDocument {
                    {"_id", 0},
                    // {"description", 1},
                    // {"principle_amount", 1},
                    {"current_principal", 1},
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

    public async Task Validations(Due due)
    {
        if (!string.IsNullOrEmpty(due.ToBank))
        {
            await Task.CompletedTask;
            // bool hasBankData = await _bankService.CountByIdAsync()
        }
    }

    public async Task<List<DueList>> ListAsync(string? filter)
    {
        ProjectionDefinition<Due> projection = Builders<Due>.Projection
            .Exclude(d => d.Payee)
            .Exclude(d => d.Comment)
            .Exclude(d => d.CurrentPrincipal)
            .Exclude(d => d.EndDate)
            .Exclude(d => d.V)
            .Exclude(d => d.Type)
            .Exclude(d => d.PrincipalAmount);

        List<Due> list = await GetList(projection);
        List<DueList> dues;

        if (filter == "active")
        {
            dues = list.Where(l => l.Status == DueStatus.Active).Select(d => new DueList { Description = d.Description, Id = d.Id }).ToList();
        }
        else
        {
            dues = list.Select(d => new DueList { Description = d.Description, Id = d.Id }).ToList();
        }        

        return dues;
    }

    // TODO: Fix code structure
    // TODO: New Transactions cannot be added to Due if total sum of due transactions is equal to due amount/ current principle is 0
    // TODO: End date should be fetched only if the date is greater than start date and valid
    // TODO: 
    public async Task<Due> GetByIdAsync(string id)
    {
        Due due = await base.SearchById(id);

        TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        DateTime date = TimeZoneInfo.ConvertTimeFromUtc(due.StartDate, timeZone);

        due.StartDate = date;

        // BSON Query
        BsonDocument[] pipelines = new BsonDocument[] {
            new BsonDocument {
                {"$match", new BsonDocument {
                    {"_id", ObjectId.Parse(id)}
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

        // DueTransactions list = (await _collection.Aggregate<DueTransactions>(pipelines).ToListAsync()).FirstOrDefault() ?? new DueTransactions();

        // DueDetails details = new DueDetails
        // {
        //     Payee = due.Payee,
        //     PrincipleAmount = due.PrincipleAmount,
        //     StartDate = due.StartDate,
        //     Status = due.Status,
        //     ToBank = due.ToBank,
        //     Type = due.Type,
        //     Comment = due.Comment,
        //     CurrentPrinciple = list.CurrentPrinciple,
        //     Description = due.Description,
        //     EndDate = due.EndDate,
        //     Id = due.Id,
        //     Transactions = list.Transactions,
        // };

        return due;
    }

    public async Task<HttpStatusCode> DeleteByIdAsync(string id)
    {
        FilterDefinition<Transaction> filter = Builders<Transaction>.Filter.Eq(t => t.DueId, id);
        long count = await _context.Transaction.CountDocumentsAsync(filter);

        if (count > 0)
        {
            // TODO: Logger
            return HttpStatusCode.Ambiguous;
        }

        bool isDeleted = await base.DeleteById(id);

        if (isDeleted) return HttpStatusCode.OK;
        return HttpStatusCode.NotModified;
    }
}