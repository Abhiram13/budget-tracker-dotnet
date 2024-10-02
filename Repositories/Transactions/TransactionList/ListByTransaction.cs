using BudgetTracker.Defination;
using BudgetTracker.API.Transactions.List;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace BudgetTracker.Repository
{
    public partial class TransactionList
    {
        private BsonDocument GroupStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$group", new BsonDocument {
                    {"_id", "$date"},
                    {"debit", new BsonDocument {
                        {"$sum", new BsonDocument {
                            {"$cond", new BsonArray {
                                new BsonDocument { {"$eq", new BsonArray { "$type", TransactionType.Debit }} },
                                "$amount",
                                0
                            }}
                        }}
                    }},
                    {"credit", new BsonDocument {
                        {"$sum", new BsonDocument {
                            {"$cond", new BsonArray {
                                new BsonDocument { {"$eq", new BsonArray { "$type", TransactionType.Credit }} },
                                "$amount",
                                0
                            }}
                        }}
                    }},                             
                    {"count", new BsonDocument {
                        {"$sum", 1}
                    }},
                }}
            };

            return pipeline;
        }

        private BsonDocument SortStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$sort", new BsonDocument {
                    {"_id", 1}
                }}
            };

            return pipeline;
        }

        private BsonDocument TransactionsPushGroupStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$group", new BsonDocument {
                    {"_id", $"{_currentYear}-{_currentMonth}" },
                    {"transactions", new BsonDocument {
                        {"$push", new BsonDocument {
                            {"debit", "$$ROOT.debit"},
                            {"credit", "$$ROOT.credit"},
                            {"count", "$$ROOT.count"},
                            {"date", "$$ROOT._id"}
                        }}
                    }}
                }}
            };

            return pipeline;
        }

        private BsonDocument TransactionsProjectStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$project", new BsonDocument {
                    {"_id", 0},                        
                    {"transactions", 1},
                }}
            };

            return pipeline;
        }

        public async Task<List<TransactionDetails>> GetByTransactions()
        {
            BsonDocument[] pipelines = new BsonDocument[] {
                MatchStage(),
                GroupStage(),
                SortStage(),
                TransactionsPushGroupStage(),
                TransactionsProjectStage(),                
            };

            List<BsonDocument> results = await _collection.Aggregate<BsonDocument>(pipelines).ToListAsync();
            BsonDocument document = results.Count > 0 ? results[0] : new BsonDocument();

            if (document.Count() == 0)
            {
                return new List<TransactionDetails>();
            }

            string json = document["transactions"].ToJson();
            List<TransactionDetails>? list = JsonSerializer.Deserialize<List<TransactionDetails>>(json);
            return list ?? new List<TransactionDetails>();
        }
    }
}