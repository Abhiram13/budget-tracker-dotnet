using BudgetTracker.Defination;
using BudgetTracker.API.Transactions.List;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace BudgetTracker.Repository
{
    public partial class TransactionList
    {
        private BsonDocument AddFieldsStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$addFields", new BsonDocument {
                    {"categoryId", new BsonDocument {
                        {"$toObjectId", "$category_id"}
                    }}
                }}
            };

            return pipeline;
        }

        private BsonDocument CategoryLookUpStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$lookup", new BsonDocument {
                    {"from", "categories"},
                    {"localField", "categoryId"},
                    {"foreignField", "_id"},
                    {"as", "category"}
                }}
            };

            return pipeline;
        }

        private BsonDocument CategoryGroupStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$group", new BsonDocument {
                    {"_id", "$category_id"},
                    {"category", new BsonDocument {
                        {"$first", new BsonDocument {
                            {"$first", "$category.name"}
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

        private BsonDocument CategoryArrayPushGroupStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$group", new BsonDocument {
                    {"_id", "$$ROOT._id"},
                    {"categories", new BsonDocument {
                        {"$push", new BsonDocument {
                            {"$cond", new BsonArray {
                                new BsonDocument { {"$gt", new BsonArray { "$$ROOT.amount", 0 }} },
                                new BsonDocument {
                                    {"id", "$$ROOT._id"},
                                    {"category", "$$ROOT.category"},
                                    {"amount", "$$ROOT.amount"}
                                },
                                "$$REMOVE"
                            }},                                
                        }}
                    }}
                }}
            };

            return pipeline;
        }

        private BsonDocument CategoryProjectStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$project", new BsonDocument {
                    {"_id", 0},
                    {"categories", new BsonDocument {
                        {"$cond", new BsonArray {
                            new BsonDocument { {"$eq", new BsonArray { new BsonDocument {{"$size", "$categories"}}, 0 }} },
                            "$$REMOVE",
                            "$categories"
                        }}
                    }},
                }}
            };

            return pipeline;
        }

        private BsonDocument CategoryFinalProjectStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$project", new BsonDocument {
                    {"categories.id", 0}
                }}
            };

            return pipeline;
        }

        public async Task<List<CategoryData>> GetCategories()
        {
            BsonDocument[] pipelines = new BsonDocument[] {
                MatchStage(),
                AddFieldsStage(),
                CategoryLookUpStage(),
                CategoryGroupStage(),
                CategoryArrayPushGroupStage(),
                CategoryProjectStage(),
                CategoryFinalProjectStage(),
            };

            List<BsonDocument> result = await _collection.Aggregate<BsonDocument>(pipelines).ToListAsync();
            string json = result.ToJson();
            List<CategoryWiseData>? data = JsonSerializer.Deserialize<List<CategoryWiseData>>(json);
            List<CategoryData>? categories = data?.Where(result => result.Categories.Length > 0).Select(result => result.Categories[0]).ToList();

            return categories ?? new List<CategoryData>();
        }
    }
}