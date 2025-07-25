using BudgetTracker.Defination;
using MongoDB.Bson;
using MongoDB.Driver;
using BudgetTracker.API.Transactions.List;
using BudgetTracker.Application;
using BudgetTracker.Interface;
using BudgetTracker.Services;
using BankResult = BudgetTracker.API.Transactions.ByBank.Result;

namespace BudgetTracker.Repository
{
    public class TransactionsByBank
    {
        private readonly string _bankId;
        private IMongoCollection<Transaction> _collection;
        private QueryParams? _params;
        private string? _dateFilter;
        private IBankService _bankService;
        private readonly string _currentMonth = DateTime.Now.Month.ToString("D2");
        private readonly string _currentYear = DateTime.Now.Year.ToString();

        public TransactionsByBank(string bankId, IMongoCollection<Transaction> collection, QueryParams? queryParams, IBankService bankService)
        {
            _bankId = bankId;
            _collection = collection;
            _params = queryParams;
            _bankService = bankService;
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
                    {"$and", new BsonArray {
                        new BsonDocument {
                            // {"$or", new BsonArray {
                            //     new BsonDocument {{"from_bank", _bankId}},
                            //     new BsonDocument {{"to_bank", _bankId}}
                            // }},
                            {"from_bank", _bankId},
                            {"date", new BsonDocument {
                                {"$regex", _dateFilter}
                            }}
                        }
                    }}
                }}
            };

            return pipeline;
        }

        private BsonDocument _AddFieldsStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$addFields", new BsonDocument {
                    {"from_bank", new BsonDocument {
                        {"$convert", new BsonDocument {
                            {"input", "$from_bank"},
                            {"to", "objectId"},
                            {"onError", "null"},
                            {"onNull", "null"}                            
                        }}
                    }},
                    // {"to_bank", new BsonDocument {
                    //     {"$convert", new BsonDocument {
                    //         {"input", "$to_bank"},
                    //         {"to", "objectId"},
                    //         {"onError", "null"},
                    //         {"onNull", "null"}
                    //     }}
                    // }}
                }}
            };

            return pipeline;
        }

        private BsonDocument _LookUpStage()
        {
            BsonDocument pipeline = new BsonDocument {
                {"$lookup", new BsonDocument {
                    {"from", "banks"},
                    {"let", new BsonDocument {
                        {"fromBankId", "$from_bank"},
                        // {"toBankId", "$to_bank"}
                    }},
                    {"pipeline", new BsonArray {
                        new BsonDocument {
                            {"$match", new BsonDocument {
                                {"$expr", new BsonDocument {
                                    {"$or", new BsonArray {
                                        new BsonDocument { {"$eq", new BsonArray { "$_id", "$$fromBankId" }} },
                                        // new BsonDocument { {"$eq", new BsonArray { "$_id", "$$toBankId" }} }
                                    }}
                                }}
                            }}
                        }
                    }},
                    {"as", "bank"}                    
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
                        {"bank", new BsonDocument { {"$first", "$bank.name"} }},
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
                        {"bank", new BsonDocument {{"$first", "$bank"}}},
                        {"_id", 0},
                        {"transactions", 1},
                        {"date", 1}
                    }}
                },
                new BsonDocument {
                    {"$group", new BsonDocument {
                        {"_id", "$bank"},
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
                        {"bank", "$_id"},
                        {"data", 1}
                    }}
                }
             };

            return pipelines;
        }

        public async Task<BankResult> GetData()
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

            List<BankResult> aggregate = await _collection.Aggregate<BankResult>(pipelines).ToListAsync();
            BankResult result = aggregate.FirstOrDefault() ?? new BankResult();

            if (string.IsNullOrEmpty(result.Bank))
            {
                Bank? bank = await _bankService.SearchById(_bankId);
                result.Bank = bank?.Name ?? "";
            }

            return result;
        }
    }
}

