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

        private BsonDocument CategoryNonEmptyMatchStage()
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

        private BsonDocument CategoryProjectStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$project", new BsonDocument {
                    {"_id", 0},
                }}
            };

            return pipeline;
        }

        private BsonDocument CategorySortStage()
        {
            int sortOrder =_params.SortOrder == "ASC" ? 1 : -1;
            
            BsonDocument pipeline = new BsonDocument {
                {"$sort", new BsonDocument {
                    {"amount", sortOrder}
                }}
            };

            return pipeline;
        }

        public async Task<List<CategoryData>> GetByCategories()
        {
            BsonDocument[] pipelines = new BsonDocument[] {
                MatchStage(),
                AddFieldsStage(),
                CategoryLookUpStage(),
                CategoryGroupStage(),
                CategoryNonEmptyMatchStage(),
                CategoryProjectStage(),
                CategorySortStage(),
            };

            List<CategoryData> result = await _collection.Aggregate<CategoryData>(pipelines).ToListAsync();

            return result;
        }
    }
}