using MongoDB.Bson;
using MongoDB.Driver;
using BudgetTracker.Defination;
using BudgetTracker.API.Transactions.List;

namespace BudgetTracker.Repository
{
    public partial class TransactionList
    {
        private readonly string _currentMonth = DateTime.Now.Month.ToString("D2");
        private readonly string _currentYear = DateTime.Now.Year.ToString();
        private string? _dateFilter;
        private readonly IMongoCollection<Transaction> _collection;
        private readonly QueryParams? _params;

        public TransactionList(QueryParams? queryParams, IMongoCollection<Transaction> collection)
        {
            _params = queryParams;
            _collection = collection;
            UpdateDateFilter();
        }

        private void UpdateDateFilter()
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

        private BsonDocument MatchStage()
        {
            BsonDocument match = new BsonDocument {
                {"$match", new BsonDocument {
                    {"date", new BsonDocument {
                        {"$regex", _dateFilter}
                    }}
                }}
            };

            return match;
        }

        private BsonDocument ProjectStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$project", new BsonDocument {
                    {"_id", 0},
                    {"count", new BsonDocument {
                        {"$sum", 1}
                    }}
                }}                
            };

            return pipeline;     
        }

        private BsonDocument CountStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$count", "count"}
            };

            return pipeline;
        }

        public async Task<int> GetCount()
        {
            BsonDocument[] pipelines = new BsonDocument[] {
                MatchStage(),
                ProjectStage(),
                CountStage()
            };

            List<TransactionCount> list = await _collection.Aggregate<TransactionCount>(pipelines).ToListAsync();
            int count = list.Count > 0 ? list[0].Count : 0;

            return count;
        }
    }
}