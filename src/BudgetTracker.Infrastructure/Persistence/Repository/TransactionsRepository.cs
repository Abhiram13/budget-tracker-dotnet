using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BudgetTracker.Core.Application.Interfaces;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.Enums;
using BudgetTracker.Core.Domain.ValueObject.Transaction;
using BudgetTracker.Core.Domain.ValueObject.Transaction.List;
using MongoDB.Bson;
using MongoDB.Driver;

using BankResult = BudgetTracker.Core.Domain.ValueObject.Transaction.ByBank.ResultByBank;
using CategoryResult = BudgetTracker.Core.Domain.ValueObject.Transaction.ByCategory.Result;
using ListResult = BudgetTracker.Core.Domain.ValueObject.Transaction.List.Result;

namespace BudgetTracker.Infrastructure.Repository;

public class TransactionRepository : MongoRepository<Transaction>, ITransactionRepository
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBankRepository _bankRepository;

    public TransactionRepository(
        IMongoContext context,
        ICategoryRepository categoryRepository,
        IBankRepository bankRepository
    ) : base(context.Transaction)
    {
        _categoryRepository = categoryRepository;
        _bankRepository = bankRepository;
    }

    public async Task<BankResult> GetByBankAsync(string bankId, QueryParams queryParams)
    {
        BankResult result = await new TransactionsByBank(bankId, _collection, queryParams, _bankRepository).GetDataAsync();
        return result;
    }

    public async Task<CategoryResult> GetByCategoryAsync(string categoryId, QueryParams queryParams)
    {
        CategoryResult result = await new TransactionsByCategory(categoryId, _collection, queryParams, _categoryRepository).GetDataAsync();
        return result;
    }

    public async Task<ListResult> ListAsync(QueryParams? queryParams, CancellationToken? cancellationToken = null)
    {
        TransactionList repository = new TransactionList(queryParams, _collection, cancellationToken);

        ListResult result = new ListResult
        {
            TotalCount = await repository.GetCountAsync()
        };

        if (queryParams?.Type == "transaction" || string.IsNullOrEmpty(queryParams?.Type))
        {
            result.Transactions = await repository.GetByTransactionsAsync();
        }

        if (queryParams?.Type == "category")
        {
            result.Categories = await repository.GetByCategoriesAsync();
        }

        if (queryParams?.Type == "bank")
        {
            result.Banks = await repository.GetByBanksAsync();
        }

        return result;
    }

    public async Task<ByDateTransactions> ListByDateAsync(string date)
    {
        ByDateTransactions data = await new TransactionsByDate(date, _collection).GetTransactionsAsync();
        return data;
    }
}

#region List by Date API Repository
internal sealed class TransactionsByDate
{
    private string _date;
    private IMongoCollection<Transaction> _collection;

    internal TransactionsByDate(string date, IMongoCollection<Transaction> collection)
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

    public async Task<ByDateTransactions> GetTransactionsAsync()
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

        List<ByDateTransactions> list = await _collection.Aggregate<ByDateTransactions>(pipelines).ToListAsync();
        return list.FirstOrDefault() ?? new ByDateTransactions();
    }
}
#endregion

#region Transactions By Bank Repository
internal sealed class TransactionsByBank
{
    private readonly string _bankId;
    private IMongoCollection<Transaction> _collection;
    private QueryParams? _params;
    private string? _dateFilter;
    private IBankRepository _bankRepository;
    private readonly string _currentMonth = DateTime.Now.Month.ToString("D2");
    private readonly string _currentYear = DateTime.Now.Year.ToString();

    public TransactionsByBank(string bankId, IMongoCollection<Transaction> collection, QueryParams? queryParams, IBankRepository bankRepository)
    {
        _bankId = bankId;
        _collection = collection;
        _params = queryParams;
        _bankRepository = bankRepository;
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
                }},
                {"pipeline", new BsonArray {
                    new BsonDocument {
                        {"$match", new BsonDocument {
                            {"$expr", new BsonDocument {
                                {"$or", new BsonArray {
                                    new BsonDocument { {"$eq", new BsonArray { "$_id", "$$fromBankId" }} },
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

    public async Task<BankResult> GetDataAsync()
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
            Bank? bank = await _bankRepository.SearchByIdAsync(_bankId);
            result.Bank = bank?.Name ?? "";
        }

        return result;
    }
}
#endregion

#region Transactions By Category Repository
internal sealed class TransactionsByCategory
{
    private readonly string _categoryId;
    private IMongoCollection<Transaction> _collection;
    private QueryParams? _params;
    private string? _dateFilter;
    private readonly ICategoryRepository _categoryRepository;
    private readonly string _currentMonth = DateTime.Now.Month.ToString("D2");
    private readonly string _currentYear = DateTime.Now.Year.ToString();

    internal TransactionsByCategory(
        string categoryId,
        IMongoCollection<Transaction> collection,
        QueryParams? queryParams,
        ICategoryRepository categoryRepository
    )
    {
        _categoryId = categoryId;
        _collection = collection;
        _params = queryParams;
        _categoryRepository = categoryRepository;
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

    private BsonDocument AddFieldsStage()
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

    private BsonDocument LookUpStage()
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

    private BsonDocument[] GroupAndProjectStage()
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

    public async Task<CategoryResult> GetDataAsync()
    {
        BsonDocument[] pipelines = new[]
        {
            MatchStage(),
            AddFieldsStage(),
            LookUpStage(),
            GroupAndProjectStage()[0],
            GroupAndProjectStage()[1],
            GroupAndProjectStage()[2],
            GroupAndProjectStage()[3],
            GroupAndProjectStage()[4],
        };

        List<CategoryResult> aggregate = await _collection.Aggregate<CategoryResult>(pipelines).ToListAsync();
        CategoryResult result = aggregate.FirstOrDefault() ?? new CategoryResult();

        if (string.IsNullOrEmpty(result.Category))
        {
            Category? category = await _categoryRepository.SearchByIdAsync(_categoryId);
            result.Category = category?.Name ?? "";
        }

        return result;
    }
}
#endregion

#region Transactions List Repository

#region Count
internal sealed partial class TransactionList
{
    private readonly string _currentMonth = DateTime.Now.Month.ToString("D2");
    private readonly string _currentYear = DateTime.Now.Year.ToString();
    private string? _dateFilter;
    private readonly IMongoCollection<Transaction> _collection;
    private readonly QueryParams? _params;
    private readonly CancellationToken? _cancellationToken;

    public TransactionList(QueryParams? queryParams, IMongoCollection<Transaction> collection, CancellationToken? cancellationToken = default)
    {
        _params = queryParams;
        _collection = collection;
        _cancellationToken = cancellationToken;
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

    public async Task<int> GetCountAsync()
    {
        BsonDocument[] pipelines = new BsonDocument[] {
            MatchStage(),
            ProjectStage(),
            CountStage()
        };

        List<TransactionCount> list = await _collection.Aggregate<TransactionCount>(pipelines).ToListAsync(_cancellationToken.GetValueOrDefault());
        int count = list.Count > 0 ? list[0].Count : 0;

        return count;
    }
}
#endregion

#region Transactions
internal sealed partial class TransactionList
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

    public async Task<List<TransactionDetails>> GetByTransactionsAsync()
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
#endregion

#region Category
internal sealed partial class TransactionList
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
                {"id", "$_id"},
                {"_id", 0},
                {"amount", 1},
                {"category", 1}
            }}
        };

        return pipeline;
    }

    private BsonDocument CategorySortStage()
    {
        int sortOrder = _params?.SortOrder == "ASC" ? 1 : -1;

        BsonDocument pipeline = new BsonDocument {
            {"$sort", new BsonDocument {
                {"amount", sortOrder}
            }}
        };

        return pipeline;
    }

    public async Task<List<CategoryData>> GetByCategoriesAsync()
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
#endregion

#region Bank
internal sealed partial class TransactionList
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
                {"id", "$_id"},
                {"_id", 0},
                {"name", 1},
                {"amount", 1}
            }}
        };

        return pipeline;
    }

    private BsonDocument BankSortStage()
    {
        int sortOrder = _params?.SortOrder == "ASC" ? 1 : -1;

        BsonDocument pipeline = new BsonDocument {
            {"$sort", new BsonDocument {
                { "amount", sortOrder }
            }}
        };

        return pipeline;
    }

    public async Task<List<BankData>> GetByBanksAsync()
    {
        BsonDocument[] pipelines = new BsonDocument[] {
            MatchStage(),
            BankAddFieldsStage(),
            BankLookUpStage(),
            BanksGroupStage(),
            NonEmptyMatchStage(),
            BankProjectStage(),
            BankSortStage()
        };

        List<BankData> list = await _collection.Aggregate<BankData>(pipelines).ToListAsync();

        return list;
    }
}
#endregion

#endregion