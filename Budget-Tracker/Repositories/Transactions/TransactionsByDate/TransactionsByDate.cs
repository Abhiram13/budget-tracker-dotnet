using MongoDB.Bson;
using MongoDB.Driver;
using BudgetTracker.Defination;
using BudgetTracker.API.Transactions.ByDate;
using System.Text.Json;

namespace BudgetTracker.Repository
{
    public class TransactionsByDate
    {
        private string _date;
        private IMongoCollection<Transaction> _collection;

        public TransactionsByDate(string date, IMongoCollection<Transaction> collection)
        {
            _date = date;
            _collection = collection;
        }

        private BsonDocument MatchStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$match", new BsonDocument {
                    {"date", $"{_date}"}
                }}
            };

            return pipeline;
        }

        private BsonDocument AddFieldsStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$addFields", new BsonDocument {
                    {"category_id", new BsonDocument {
                        {"$toObjectId", "$category_id"}
                    }},
                    {"from_bank", new BsonDocument {
                        {"$convert", new BsonDocument {
                            {"input", "$from_bank"},
                            {"to", "objectId"},
                            {"onError", "null"},
                            {"onNull", "null"}
                        }}
                    }},
                    {"to_bank", new BsonDocument {
                        {"$convert", new BsonDocument {
                            {"input", "$to_bank"},
                            {"to", "objectId"},
                            {"onError", "null"},
                            {"onNull", "null"}
                        }}
                    }}
                }}
            };

            return pipeline;
        }

        private BsonDocument[] LookUpStages()
        {
            BsonDocument[] pipelines = new BsonDocument[] {
                new BsonDocument {
                    {"$lookup", new BsonDocument {
                        {"from", "categories"},
                        {"localField", "category_id"},
                        {"foreignField", "_id"},
                        {"as", "category"}
                    }}
                },
                new BsonDocument {
                    {"$lookup", new BsonDocument {
                        {"from", "banks"},
                        {"localField", "from_bank"},
                        {"foreignField", "_id"},
                        {"as", "from_bank"}
                    }}
                },
                new BsonDocument {
                    {"$lookup", new BsonDocument {
                        {"from", "banks"},
                        {"localField", "to_bank"},
                        {"foreignField", "_id"},
                        {"as", "to_bank"}
                    }}
                },
            };

            return pipelines;
        }

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
                    {"partial_debit", new BsonDocument {
                        {"$sum", new BsonDocument {
                            {"$cond", new BsonArray {
                                new BsonDocument { {"$eq", new BsonArray { "$type", TransactionType.PartialDebit }} },
                                "$amount",
                                0
                            }}
                        }}
                    }},
                    {"partial_credit", new BsonDocument {
                        {"$sum", new BsonDocument {
                            {"$cond", new BsonArray {
                                new BsonDocument { {"$eq", new BsonArray { "$type", TransactionType.PartialCredit }} },
                                "$amount",
                                0
                            }}
                        }}
                    }},
                    {"transactions", new BsonDocument {
                        {"$push", new BsonDocument {
                            {"amount", "$amount"},
                            {"description", "$description"},
                            {"type", "$type"},
                            {"id", new BsonDocument { {"$toString", "$_id"} }},
                            {"category", new BsonDocument {
                                {"$first", "$category.name"}
                            }},
                            {"from_bank", new BsonDocument {
                                {"$first", "$from_bank.name"}
                            }},
                            {"to_bank", new BsonDocument {
                                {"$first", "$to_bank.name"}
                            }}
                        }}
                    }}
                }}
            };

            return pipeline;
        }

        private BsonDocument ProjectStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$project", new BsonDocument {
                    {"_id", 0},
                }}
            };

            return pipeline;
        }

        public async Task<Data> GetTransactions()
        {
            BsonDocument[] pipelines = new BsonDocument[] {
                MatchStage(),
                AddFieldsStage(),
                LookUpStages()[0],
                LookUpStages()[1],
                LookUpStages()[2],
                GroupStage(),
                ProjectStage(),
            };

            List<Data> list = await _collection.Aggregate<Data>(pipelines).ToListAsync();
            return list.FirstOrDefault() ?? new Data();
        }
    }
}