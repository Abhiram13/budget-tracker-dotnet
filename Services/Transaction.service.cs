using Defination;
using Global;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text.Json;
using System.Text.RegularExpressions;
using TransactionsByDate;

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

        public async Task<List<TransactionList<string>>> List(Defination.TransactionsList.QueryParams? queryParams)
        {
            IAggregateFluent<Transaction> aggregate = collection.Aggregate();
            FilterDefinition<Transaction> filter;

            if (!string.IsNullOrEmpty(queryParams?.month) && !string.IsNullOrEmpty(queryParams?.year))
            {
                BsonRegularExpression regex = new BsonRegularExpression($"{queryParams?.year}-{queryParams?.month}");                
                filter = Builders<Transaction>.Filter.Regex("date", regex);
            }
            else
            {
                string currentMonth = DateTime.Now.Month.ToString("D2");
                string currentYear = DateTime.Now.Year.ToString();
                BsonRegularExpression regex = new BsonRegularExpression($"{currentYear}-{currentMonth}");
                filter = Builders<Transaction>.Filter.Regex("date", regex);
            }

            List<TransactionList<double>> data = await aggregate.Match(filter).Group(a => a.Date, b => new TransactionList<double>() {
                Debit = b.Where(c => c.Type == TransactionType.Debit).Sum(d => d.Amount),
                Credit = b.Where(c => c.Type == TransactionType.Credit).Sum(d => d.Amount),
                Date = b.First().Date,
                Count = b.Count(),
                DateLink = b.First().Date,
            })
            .Sort(Builders<TransactionList<double>>.Sort.Ascending(x => x.Date))
            .ToListAsync();

            List<TransactionList<string>> list = new List<TransactionList<string>>();

            foreach (TransactionList<double> transaction in data)
            {
                list.Add(new TransactionList<string>()
                {
                    Count = transaction.Count,
                    Credit = string.Format("{0:#,##0.##}", transaction.Credit),
                    Debit = string.Format("{0:#,##0.##}", transaction.Debit),
                    Date = DateTime.Parse(transaction.Date).ToString("D"),
                    DateLink = transaction.Date,
                });
            }

            return list;
        }

        public async Task<Result> ListByDate(string date)
        {
            BsonDocument match = new BsonDocument {
                {"$match", new BsonDocument {
                    {"date", $"{date}"}
                }}
            };

            BsonDocument addfields = new BsonDocument {
                {"$addFields", new BsonDocument {
                    {"categoryId", new BsonDocument {
                        {"$convert", new BsonDocument{
                            {"input", "$category_id"},
                            {"to", "objectId"},
                            {"onError", ""},
                            {"onNull", ""}
                        }}
                    }},
                    {"fromBankId", new BsonDocument {
                        {"$convert", new BsonDocument{
                            {"input", "$from_bank"},
                            {"to", "objectId"},
                            {"onError", ""},
                            {"onNull", ""}
                        }}
                    }},
                    {"toBankId", new BsonDocument {
                        {"$convert", new BsonDocument{
                            {"input", "$to_bank"},
                            {"to", "objectId"},
                            {"onError", ""},
                            {"onNull", ""}
                        }}
                    }},
                }}
            };

            BsonDocument lookup1 = new BsonDocument {
                {"$lookup", new BsonDocument {
                    {"from", "banks"},
                    {"localField", "fromBankId"},
                    {"foreignField", "_id"},
                    {"as", "from_banks"}
                }},               
            };

            BsonDocument lookup2 = new BsonDocument {
                {"$lookup", new BsonDocument {
                    {"from", "banks"},
                    {"localField", "toBankId"},
                    {"foreignField", "_id"},
                    {"as", "to_banks"}
                }},               
            };

            BsonDocument lookup3 = new BsonDocument {
                {"$lookup", new BsonDocument {
                    {"from", "categories"},
                    {"localField", "categoryId"},
                    {"foreignField", "_id"},
                    {"as", "categories"}
                }},                
            };

            BsonDocument group = new BsonDocument {
                {
                    "$group", new BsonDocument {
                        {"_id", "$date" },
                        {"credit", new BsonDocument {
                            {"$sum", new BsonDocument {
                                {"$cond", new BsonArray {
                                    new BsonDocument { {"$eq", new BsonArray { "$type", TransactionType.Credit }} },                                    
                                    "$amount",
                                    0
                                }}
                            }}
                        }},
                        {"debit", new BsonDocument {
                            {"$sum", new BsonDocument {
                                {"$cond", new BsonArray {
                                    new BsonDocument { {"$eq", new BsonArray { "$type", TransactionType.Debit }} },
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
                        {"partial_debit", new BsonDocument {
                            {"$sum", new BsonDocument {
                                {"$cond", new BsonArray {
                                    new BsonDocument { {"$eq", new BsonArray { "$type", TransactionType.PartialDebit }} },
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
                                {"from_bank", new BsonDocument {
                                    {"$reduce", new BsonDocument {
                                        {"input", "$from_banks.name"},
                                        {"initialValue", ""},
                                        {"in", new BsonDocument {
                                            {"$concat", new BsonArray{ "$$value", "$$this" }}
                                        }}
                                    }}
                                }},
                                {"to_bank", new BsonDocument {
                                    {"$reduce", new BsonDocument {
                                        {"input", "$to_banks.name"},
                                        {"initialValue", ""},
                                        {"in", new BsonDocument {
                                            {"$concat", new BsonArray{ "$$value", "$$this" }}
                                        }}
                                    }}
                                }},
                                {"category", new BsonDocument {
                                    {"$reduce", new BsonDocument {
                                        {"input", "$categories.name"},
                                        {"initialValue", ""},
                                        {"in", new BsonDocument {
                                            {"$concat", new BsonArray{ "$$value", "$$this" }}
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    }
                }
            };

            BsonDocument project = new BsonDocument {
                {"$project", new BsonDocument {
                    {"credit", 1},
                    {"debit", 1},
                    {"partial_credit", 1},
                    {"partial_debit", 1},
                    {"transactions", 1},
                    {"_id", 0}
                }}
            };

            BsonDocument[] pipeline = new[] {match, addfields, lookup1, lookup2, lookup3, group, project};
            List<BsonDocument> results = await collection.Aggregate<BsonDocument>(pipeline).ToListAsync();
            string document = results[0].ToBsonDocument().ToJson();            
            Result test = JsonSerializer.Deserialize<Result>(document) ?? new Result();

            return test;
        }
    }
}