using Defination;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

using ListAPI = API.Transactions.List;

namespace Services
{
    public class TransactionService : MongoServices<Transaction>, ITransactionService
    {
        public TransactionService() : base(Collection.Transaction) { }

        public async Task Validations(Transaction transaction)
        {
            if (!Enum.IsDefined(typeof(TransactionType), transaction.Type))
            {
                throw new InvalidDataException("Transaction type is invalid");
            }

            Regex descriptionRegex = new Regex(@"^[a-zA-Z0-9 ]*$");

            if (!descriptionRegex.IsMatch(transaction.Description))
            {
                throw new InvalidDataException("Description contains invalid characters");
            }

            Category? category = await SearchById(transaction.CategoryId, Collection.Category);

            if (category == null || string.IsNullOrEmpty(category.Name))
            {
                throw new InvalidDataException("Invalid category id provided");
            }

            // Bank? fromBank =  await SearchById(transaction.FromBank, Collection.Bank);

            // if (fromBank == null || string.IsNullOrEmpty(fromBank.Name))
            // {
            //     throw new InvalidDataException("Invalid from bank id provided");
            // }

            // Bank? toBank =  await SearchById(transaction.ToBank, Collection.Bank);

            // if (toBank == null || string.IsNullOrEmpty(toBank.Name))
            // {
            //     throw new InvalidDataException("Invalid to bank id provided");
            // }
        }

        public async Task<ListAPI.Result> List(ListAPI.QueryParams? queryParams)
        {
            string currentMonth = DateTime.Now.Month.ToString("D2");
            string currentYear = DateTime.Now.Year.ToString();
            string dateFilter;

            if (!string.IsNullOrEmpty(queryParams?.month) && !string.IsNullOrEmpty(queryParams?.year))
            {
                dateFilter = $"{queryParams?.year}-{queryParams?.month}";
            }
            else
            {
                dateFilter = $"{currentYear}-{currentMonth}";
            }

            BsonDocument match = new BsonDocument {
                {"$match", new BsonDocument {
                    {"date", new BsonDocument {
                        {"$regex", dateFilter}
                    }}
                }}
            };

            BsonDocument[] pipelines = new BsonDocument[] {
                match,
                new BsonDocument {
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
                },
                new BsonDocument {
                    {"$sort", new BsonDocument {
                        {"_id", 1}
                    }}
                },                
                new BsonDocument {
                    {"$group", new BsonDocument {
                        {"_id", $"{currentYear}-{currentMonth}" },                        
                        {"transactions", new BsonDocument {
                            {"$push", new BsonDocument {
                                {"debit", "$$ROOT.debit"},
                                {"credit", "$$ROOT.credit"},
                                {"count", "$$ROOT.count"},
                                // {"date", new BsonDocument {
                                //     {"$dateToString", new BsonDocument {
                                //         {"format", "%B %d, %G"},
                                //         {"date", new BsonDocument {
                                //             {"$dateFromString", new BsonDocument {
                                //                 { "dateString", "$$ROOT._id" },
                                //                 { "format", "%Y-%m-%d" },
                                //             }}
                                //         }}
                                //     }}                            
                                // }},
                                {"date", "$$ROOT._id"}
                            }}
                        }}
                    }}
                },
                new BsonDocument {
                    {"$project", new BsonDocument {
                        {"_id", 0},
                        {"total_count", new BsonDocument {
                            {"$sum", "$transactions.count"}
                        }},
                        {"transactions", 1},
                    }}
                }
            };
            
            // ListAPI.CategoryData[] categories = await CategoryWise(match);
            List<ListAPI.TransactionStage> results = await collection.Aggregate<ListAPI.TransactionStage>(pipelines).ToListAsync();

            if (results.Any())
            {
                ListAPI.Result result = new ListAPI.Result() {
                    TotalCount = results[0].TotalCount,
                    Transactions = results[0].Transactions,
                    // Categories = categories
                };

                return result;
            }

            return new ListAPI.Result();
        }

        private async Task<ListAPI.CategoryData[]> CategoryWise(BsonDocument matchStage)
        {
            BsonDocument[] pipelines = new BsonDocument[] {
                matchStage,
                new BsonDocument {
                    {"$addFields", new BsonDocument {
                        {"categoryId", new BsonDocument {
                            {"$toObjectId", "$category_id"}
                        }}
                    }}
                },
                new BsonDocument {
                    {"$lookup", new BsonDocument {
                        {"from", "categories"},
                        {"localField", "categoryId"},
                        {"foreignField", "_id"},
                        {"as", "category"}
                    }}
                },
                new BsonDocument {
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
                },
                new BsonDocument {
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
                },
                new BsonDocument {
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
                },                
                new BsonDocument {
                    {"$project", new BsonDocument {
                        {"categories.id", 0}
                    }}
                }
            };

            List<ListAPI.CategoryWiseData> results = await collection.Aggregate<ListAPI.CategoryWiseData>(pipelines).ToListAsync();            
            ListAPI.CategoryData[] data = results.Where(result => result.Categories.Count() > 0).Select(result => result.Categories[0]).ToArray();

            return data;
        }

        public async Task<API.Transactions.ByDate.Detail> ListByDate(string date)
        {
            IAggregateFluent<Transaction> aggregate = collection.Aggregate().Match(Builders<Transaction>.Filter.Eq(t => t.Date, date));
            
            Func<Task<API.Transactions.ByDate.GroupAmounts>> fetchGroupAmounts = async () => {
                List<API.Transactions.ByDate.GroupAmounts> list = await aggregate
                    .Group(a => a.Date, b => new API.Transactions.ByDate.GroupAmounts(
                        b.Where(c => c.Type == TransactionType.Debit).Sum(d => d.Amount),
                        b.Where(c => c.Type == TransactionType.Credit).Sum(d => d.Amount),
                        b.Where(c => c.Type == TransactionType.PartialDebit).Sum(d => d.Amount),
                        b.Where(c => c.Type == TransactionType.PartialCredit).Sum(d => d.Amount)
                    ))
                    .ToListAsync();

                API.Transactions.ByDate.GroupAmounts group = list.Count > 0 ? list[0] : new API.Transactions.ByDate.GroupAmounts(0, 0, 0, 0);

                return group;
            };

            Func<Task<List<Bank>>> banks = async () => {
                List<Bank> list = await Collection.Bank.Aggregate().ToListAsync();
                return list;
            };

            Func<Task<List<Category>>> categories = async () => {
                List<Category> list = await Collection.Category.Aggregate().ToListAsync();
                return list;
            };

            Func<Task<List<Transaction>>> transactions = async () => {
                ProjectionDefinition<Transaction> projection = Builders<Transaction>.Projection
                    .Include(t => t.Amount)
                    .Include(t => t.Description)
                    .Include(t => t.Type)
                    .Include(t => t.CategoryId)
                    .Include(t => t.FromBank)
                    .Include(t => t.ToBank);

                List<Transaction> list = await aggregate.Project<Transaction>(projection).ToListAsync();

                return list;
            };

            API.Transactions.ByDate.GroupAmounts? group = await fetchGroupAmounts();
            List<Transaction> list = await transactions();
            List<API.Transactions.ByDate.Data> data = new List<API.Transactions.ByDate.Data>();
            List<Bank> listOfBanks = await banks();
            List<Category> listOfCategories = await categories();

            foreach (Transaction transaction in list)
            {
                Bank? fromBank = listOfBanks.Where(b => b.Id == transaction.FromBank).FirstOrDefault();
                Bank? toBank = listOfBanks.Where(b => b.Id == transaction.ToBank).FirstOrDefault();
                Category? category = listOfCategories.Where(c => c.Id == transaction.CategoryId).FirstOrDefault();

                data.Add(new API.Transactions.ByDate.Data(
                    amount: transaction.Amount,
                    description: transaction.Description,
                    type: transaction.Type,
                    fromBank: fromBank?.Name,
                    toBank: toBank?.Name,
                    category: category?.Name ?? "",
                    transactionId: transaction.Id
                ));
            }

            return new API.Transactions.ByDate.Detail(group, data);
        }
    }
}