using BudgetTracker.Defination;
using BudgetTracker.API.Transactions.List;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BudgetTracker.Repository
{
    public partial class TransactionList
    {
        private BsonDocument BankAddFieldsStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$addFields", new BsonDocument {
                    {"bankId", new BsonDocument {
                        {"$convert", new BsonDocument {
                            {"input", "$from_bank"},
                            {"to", "objectId"},
                            {"onError", "null"},
                            {"onNull", "null"}
                        }}
                    }}
                }}
            };

            return pipeline;
        }

        private BsonDocument BankLookUpStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$lookup", new BsonDocument {
                    {"from", "banks"},
                    {"localField", "bankId"},
                    {"foreignField", "_id"},
                    {"as", "bank"}
                }}
            };

            return pipeline;
        }

        private BsonDocument BanksGroupStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$group", new BsonDocument {
                    {"_id", "$from_bank"},
                    {"name", new BsonDocument {
                        {"$first", new BsonDocument {
                            {"$first", "$bank.name"}
                        }}
                    }},
                    {"amount", new BsonDocument {
                        {"$sum", new BsonDocument {
                            {"$cond", new BsonArray {
                                new BsonDocument { {"$eq", new BsonArray { "$type", TransactionType.Debit }} },
                                "$amount",
                                0
                            }}
                        }}
                    }}
                }}
            };

            return pipeline;
        }

        private BsonDocument NonEmptyMatchStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$match", new BsonDocument {
                    {"amount", new BsonDocument {
                        {"$gt", 0}
                    }}
                }}
            };

            return pipeline;
        }

        private BsonDocument BankProjectStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$project", new BsonDocument {
                    {"_id", 0},
                }}
            };

            return pipeline;
        }        

        public async Task<List<BankData>> GetByBanks()
        {
            BsonDocument[] pipelines = new BsonDocument[] {
                MatchStage(),
                BankAddFieldsStage(),
                BankLookUpStage(),
                BanksGroupStage(),
                NonEmptyMatchStage(),
                BankProjectStage(),
            };

            List<BankData> list = await _collection.Aggregate<BankData>(pipelines).ToListAsync();

            return list;
        }
    }
}