using BudgetTracker.Defination;
using MongoDB.Bson;
using MongoDB.Driver;
using BudgetTracker.API.Transactions.List;
using BudgetTracker.Services;

using CategoryResult = BudgetTracker.API.Transactions.ByCategory.Result;

namespace BudgetTracker.Repository
{
    public class TransactionsByCategory
    {
        private readonly string _categoryId; 
        private IMongoCollection<Transaction> _collection;
        private QueryParams? _params;
        private string? _dateFilter;
        private readonly CategoryService _categoryService;
        private readonly string _currentMonth = DateTime.Now.Month.ToString("D2");
        private readonly string _currentYear = DateTime.Now.Year.ToString();

        public TransactionsByCategory(string categoryId, IMongoCollection<Transaction> collection, QueryParams? queryParams, CategoryService categoryService)
        {
            _categoryId = categoryId;
            _collection = collection;
            _params = queryParams;
            _categoryService = categoryService;
            _UpdateDateFilter();
        }

        private void _UpdateDateFilter()
        {
            if (!string.IsNullOrEmpty(_params?.Month) && !string.IsNullOrEmpty(_params?.Year))
            {
                _dateFilter = $"{_params?.Year}-{_params?.Month}";
            }
            else
            {
                _dateFilter = $"{_currentYear}-{_currentMonth}";
            }            
        }

        private BsonDocument _MatchStage()
        {
            BsonDocument pipeline = new BsonDocument() {
                {"$match", new BsonDocument {
                    {"category_id", $"{_categoryId }"},
                    {"date", new BsonDocument {
                        {"$regex", _dateFilter}
                    }}
                }}
            };
            
            return pipeline;
        }

        private BsonDocument _AddFieldsStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$addFields", new BsonDocument {
                    {"category_id", new BsonDocument {
                        {"$toObjectId", "$category_id"}
                    }}
                }}
            };
            
            return pipeline;
        }

        private BsonDocument _LookUpStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$lookup", new BsonDocument {
                    {"from", "categories"},
                    {"localField", "category_id"},
                    {"foreignField", "_id"},
                    {"as", "category"}
                }}
            };
            
            return pipeline;
        }

        private BsonDocument[] _GroupAndProjectStage()
        {
            BsonDocument[] pipelines = new[] {
                new BsonDocument {
                    {"$group", new BsonDocument {
                        {"_id", new BsonDocument { {"date", "$date"} }},
                        {"category", new BsonDocument { {"$first", "$category.name"} }},
                        {"date", new BsonDocument {{"$first", "$date"}}},
                        {"transactions", new BsonDocument {
                            {"$push", new BsonDocument {
                                {"amount", "$amount"},
                                {"type", "$type"},
                                {"description", "$description"},
                            }}
                        }}
                    }}
                },
                new BsonDocument {
                    {"$sort", new BsonDocument { {"date", 1} }}
                },
                new BsonDocument {
                    {"$project", new BsonDocument {
                        {"category", new BsonDocument {{"$first", "$category"}}},
                        {"_id", 0},
                        {"transactions", 1},
                        {"date", 1}
                    }}
                },
                new BsonDocument {
                    {"$group", new BsonDocument {
                        {"_id", "$category"},
                        {"data", new BsonDocument {
                            {"$push", new BsonDocument {
                                {"date", "$date"},
                                {"transactions", "$transactions"},
                            }}
                        }}
                    }}
                },
                new BsonDocument {
                    {"$project", new BsonDocument {
                        {"_id", 0},
                        {"category", "$_id"},
                        {"data", 1}
                    }}
                }
             };
            
            return pipelines;
        }

        public async Task<CategoryResult> GetData()
        {
            BsonDocument[] pipelines = new[]
            {
                _MatchStage(),
                _AddFieldsStage(),
                _LookUpStage(),
                _GroupAndProjectStage()[0],
                _GroupAndProjectStage()[1],
                _GroupAndProjectStage()[2],
                _GroupAndProjectStage()[3],
                _GroupAndProjectStage()[4],
            };
            
            List<CategoryResult> aggregate = await _collection.Aggregate<CategoryResult>(pipelines).ToListAsync();
            CategoryResult result = aggregate.FirstOrDefault() ?? new CategoryResult();

            if (string.IsNullOrEmpty(result.Category))
            {
                Category? category = await _categoryService.SearchById(_categoryId);
                result.Category = category?.Name ?? "";
            }
            
            return result;
        }
    }
}

