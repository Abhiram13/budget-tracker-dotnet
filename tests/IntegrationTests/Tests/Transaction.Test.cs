using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using IntegrationTests.Data.Transactions;
using IntegrationTests.Definations.Transactions;
using IntegrationTests.Utils;
using BudgetTracker.Core.Domain.ValueObject;
using BudgetTracker.Core.Domain.ValueObject.Transaction.List;
using BudgetTracker.Core.Domain.ValueObject.Transaction;
using BudgetTracker.Core.Domain.Entities;

using TransactionByCategoryResult = BudgetTracker.Core.Domain.ValueObject.Transaction.ByCategory.Result;
using CategoryTypeTransactionsResult = BudgetTracker.Core.Domain.ValueObject.Transaction.List.Result;
using TransactionsByCategoryId = BudgetTracker.Core.Domain.ValueObject.Transaction.ByCategory.CategoryData;
using ByDateTransactions = BudgetTracker.Core.Domain.ValueObject.Transaction.ByDateTransactions;
using ByBankResult = BudgetTracker.Core.Domain.ValueObject.Transaction.ByBank.ResultByBank;
using CategoryData = BudgetTracker.Core.Domain.ValueObject.Transaction.List.CategoryData;

namespace IntegrationTests;

[Collection("transaction")]
[Trait("Category", "Transaction")]
public class TransactionIntegrationTests : IntegrationTests
{
    private const string _categoryId = "665aa29b930ad7888c6766fa";
    
    public TransactionIntegrationTests(MongoDBFixture fixture) : base(fixture)
    {
        _client.DefaultRequestHeaders.Add("API_KEY", _API_KEY);
    }

    [Theory]
    [ClassData(typeof(TransactionsInsertTestData))]
    public async Task Insert_Transactions(TransactionsInsertTestDef data)
    {
        await using (TestDisposal _ = new TestDisposal(_fixture))
        {
            string payload = JsonSerializer.Serialize(data.Transaction);
            StringContent payload1 = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpResponseMessage httpResponse = await _client.PostAsync("transactions", payload1);
            string response = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<string>? apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(response);

            Assert.Equal(data.ExpectedStatusCode, (int) apiResponse!.StatusCode);
            Assert.NotNull(apiResponse.Message);
            Assert.Equal(data.ExpectedMessage, apiResponse.Message);
            Assert.Equal(data.ExpectedHttpStatusCode, (int) httpResponse.StatusCode);
        }
    }

    #region Transactions List APIs
    [Theory]
    [ClassData(typeof(TransactionsListByTypeCategoryTestData))]
    public async Task Transactions_List_Type_Categories(TransactionsListByCategoryTypeTestDef data)
    {
        await using (TransactionsInsertDisposals disposableTests = new TransactionsInsertDisposals(_fixture, _client))
        {
            await disposableTests.InsertManyAsync();
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions?month={data.Month}&year={data.Year}&type=category");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<CategoryTypeTransactionsResult>? apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryTypeTransactionsResult>>(jsonResponse);
            PropertyInfo? transactionsProp = apiResponse?.Result?.GetType().GetProperty("transactions");
            PropertyInfo? banksProp = apiResponse?.Result?.GetType().GetProperty("banks");
            Assert.Equal(data.ExpectedStatusCode, (int) apiResponse!.StatusCode);
            Assert.NotNull(apiResponse.Result);
            Assert.Equal(data.ExpectedTotalTransactions, apiResponse.Result.TotalCount);
            Assert.Equal(data.ShouldHaveData, apiResponse.Result.Categories?.Count > 0);
            Assert.Null(transactionsProp);
            Assert.Null(banksProp);
    
            if (apiResponse.Result.TotalCount > 0)
            {
                Assert.NotNull(apiResponse.Result.Categories);
                if (apiResponse.Result.Categories.Count > 0)
                {
                    foreach (CategoryData category in apiResponse.Result.Categories)
                    {
                        Assert.NotNull(category.Name);
                        Assert.NotEmpty(category.Name);
                        Assert.True(category.Amount > 0);
                        Assert.NotEmpty(category.CategoryId);
                        Assert.NotNull(category.CategoryId);
                        Assert.Equal(data.CategoryName, category.Name);
                        Assert.Equal(data.ExpectedDebit, category.Amount);
                    }
                }
            }
        }
    }
    
    [Theory]
    [ClassData(typeof(TransactionsListTestData))]
    public async Task Transactions_List_Type_Transactions(TransactionsListTestDef data)
    {
        await using (TransactionsInsertDisposals disposableTest = new TransactionsInsertDisposals(_fixture, _client))
        {
            await disposableTest.InsertManyAsync();
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions?month={data.Month}&year={data.Year}");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<CategoryTypeTransactionsResult>? apiResponse = JsonSerializer.Deserialize<ApiResponse<CategoryTypeTransactionsResult>>(jsonResponse);
            PropertyInfo? categoryProp = apiResponse?.Result?.GetType().GetProperty("categories");
            PropertyInfo? banksProp = apiResponse?.Result?.GetType().GetProperty("banks");
    
            Assert.NotNull(apiResponse?.Result);
            Assert.Equal(data.ExpectedStatusCode, (int)apiResponse.StatusCode);
            Assert.NotNull(apiResponse.Result.Transactions);
            Assert.NotNull(apiResponse.Result?.TotalCount);
            Assert.Null(categoryProp);
            Assert.Null(banksProp);
            Assert.Equal(data.ExpectedTotalCount, apiResponse.Result.TotalCount);
    
            if (apiResponse.Result.Transactions.Count > 0)
            {
                List<TransactionDetails> details = apiResponse.Result.Transactions;
                Assert.Contains(details, t => t.Date == data.ExpectedDate);
                int index = details.FindIndex(t => t.Date == data.ExpectedDate);
    
                Assert.Equal(data.ExpectedDebit, details[index].Debit);
                Assert.Equal(data.ExpectedDate, details[index].Date);
                Assert.Equal(data.ExpectedTotalTransactions, details[index].Count);
            }
        }
    }

    [Theory]
    [ClassData(typeof(TransactionsListByTypeBankTestData))]
    public async Task Transactions_List_Type_Banks(TransactionsListByBankTypeTestDef data)
    {
        await using (TransactionsInsertDisposals disposableTests = new TransactionsInsertDisposals(_fixture, _client))
        {
            await disposableTests.InsertManyAsync();
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions?month={data.Month}&year={data.Year}&type=bank");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<Result>? apiResponse = JsonSerializer.Deserialize<ApiResponse<Result>>(jsonResponse);
            PropertyInfo? categoryProp = apiResponse?.Result?.GetType().GetProperty("categories");
            PropertyInfo? transactionProp = apiResponse?.Result?.GetType().GetProperty("transactions");
            
            Assert.NotNull(apiResponse?.Result);
            Assert.Equal(data.ExpectedStatusCode, (int)apiResponse.StatusCode);
            Assert.True(apiResponse.Result.TotalCount > -1);
            Assert.NotNull(apiResponse.Result.Banks);
            Assert.Null(categoryProp);
            Assert.Null(transactionProp);
            Assert.Equal(data.ExpectedTotalTransactions, apiResponse.Result.TotalCount);
            Assert.Equal(data.ShouldHaveData, apiResponse.Result.Banks.Count > 0);

            if (apiResponse.Result.Banks.Count > 0)
            {
                foreach (var bank in apiResponse.Result.Banks)
                {
                    Assert.NotNull(bank.Name);
                    Assert.Equal(data.ExpectedBankName, bank.Name);
                    Assert.Equal(data.ExpectedDebit, bank.Amount);
                    Assert.Equal(data.ExpectedBankId, bank.BankId);
                }
            }
        }
    }
    #endregion

    [Theory]
    [ClassData(typeof(TransactionsByCategoryIdTestData))]
    public async Task Transactions_By_Category(TransactionsByCategoryIdTestDef data)
    {
        await using (TransactionsInsertDisposals disposableTests = new TransactionsInsertDisposals(_fixture, _client))
        {
            await disposableTests.InsertManyAsync();
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions/category/{_categoryId}?month={data.Month}&year={data.Year}");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<TransactionByCategoryResult>? apiResponse = JsonSerializer.Deserialize<ApiResponse<TransactionByCategoryResult>>(jsonResponse);
    
            Assert.Equal(200, (int) apiResponse!.StatusCode);
            Assert.NotEmpty(apiResponse.Result!.Category);
            Assert.Equal(data.ExpectedCategoryName, apiResponse.Result!.Category);
            Assert.Equal(data.ExpectedTotalDates, apiResponse.Result!.CategoryData.Count);
    
            if (apiResponse.Result.CategoryData.Count > 0)
            {
                foreach (TransactionsByCategoryId categoryData in apiResponse.Result.CategoryData)
                {
                    Assert.Equal(data.ExpectedTotalTransactionsForDate, categoryData.Transactions.Count);
                    Assert.Contains(data.ExpectedTransactions, d => d.Date == categoryData.Date);
                }
            }
        }
    }

    [Theory]
    [ClassData(typeof(TransactionsByDateTestData))]
    public async Task Transactions_By_Date(ByDateTestDef data)
    {
        await using (TransactionsInsertDisposals disposableTests = new TransactionsInsertDisposals(_fixture, _client))
        {
            await disposableTests.InsertManyAsync();
            string dateInput = string.IsNullOrEmpty(data.Date) ? data.Date : "";
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions/date/{dateInput}");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<ByDateTransactions>? apiResponse = JsonSerializer.Deserialize<ApiResponse<ByDateTransactions>>(jsonResponse);
            PropertyInfo? debitProp = apiResponse?.Result?.GetType().GetProperty("Debit");
            PropertyInfo? creditProp = apiResponse?.Result?.GetType().GetProperty("Credit");
            PropertyInfo? partialDebitProp = apiResponse?.Result?.GetType().GetProperty("PartialDebit");
            PropertyInfo? partialCreditProp = apiResponse?.Result?.GetType().GetProperty("PartialCredit");
            PropertyInfo? transactionsProp = apiResponse?.Result?.GetType().GetProperty("Transactions");
            PropertyInfo? messageProp = apiResponse?.GetType().GetProperty("Message");
    
            if (apiResponse?.StatusCode != HttpStatusCode.OK)
            {
                Assert.NotNull(messageProp);
                Assert.NotNull(apiResponse?.Message);
                return;
            }
            
            Assert.Equal(data.ExpectedHttpStatusCode, (int) httpResponse.StatusCode);
            Assert.Equal(data.ExpectedStatusCode, (int) apiResponse.StatusCode);
            Assert.Equal(data.ExpectedCredit, apiResponse.Result?.Credit);
            Assert.Equal(data.ExpectedDebit, apiResponse.Result?.Debit);
            Assert.Equal(data.ExpectedPartialCredit, apiResponse.Result?.PartialCredit);
            Assert.Equal(data.ExpectedPartialDebit, apiResponse.Result?.PartialDebit);
            Assert.Equal(data.ExpectedTotalTransactions, apiResponse.Result?.Transactions.Count);
            Assert.NotNull(debitProp);
            Assert.NotNull(creditProp);
            Assert.NotNull(partialDebitProp);
            Assert.NotNull(partialCreditProp);
            Assert.NotNull(transactionsProp);
    
            if (apiResponse.Result?.Transactions.Count > 0)
            {
                foreach (TransactionsList transaction in apiResponse.Result.Transactions)
                {
                    Assert.Contains(data.ExpectedTransactions, expTransaction => expTransaction.Description == transaction.Description);
                    Assert.Contains(data.ExpectedTransactions, expTransaction => expTransaction.Category == transaction.Category);
                    Assert.Contains(data.ExpectedTransactions, expTransaction => expTransaction.Amount == transaction.Amount);
                    Assert.Contains(data.ExpectedTransactions, expTransaction => expTransaction.FromBank == transaction.FromBank);
                    Assert.Contains(data.ExpectedTransactions, expTransaction => expTransaction.Type == transaction.Type);
                    Assert.NotNull(transaction.TransactionId);
                    
                }
            }
        }
    }

    [Theory]
    [ClassData(typeof(TransactionsByBankTestData))]
    public async Task Transactions_By_Bank(ByBankTestDef data)
    {
        await using (TransactionsInsertDisposals disposableTests = new TransactionsInsertDisposals(_fixture, _client))
        {
            await disposableTests.InsertManyAsync();
            HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions/bank/{data.BankId}?month={data.Month}&year={data.Year}");
            string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            ApiResponse<ByBankResult>? apiResponse = JsonSerializer.Deserialize<ApiResponse<ByBankResult>>(jsonResponse);
            PropertyInfo? _ = apiResponse?.Result?.GetType().GetProperty("Bank");
            PropertyInfo? __ = apiResponse?.Result?.GetType().GetProperty("BankData");
            
            Assert.Equal(data.ExpectedStatusCode, (int) apiResponse!.StatusCode);
            Assert.Equal(data.ExcpectedHttpStatusCode, (int) httpResponse.StatusCode);
            // Assert.Equal(data.ExpectedResult.BankData.Count, apiResponse.Result?.BankData.Count);
            // Assert.True(Enumerable.SequenceEqual(apiResponse.Result.BankData, data.ExpectedResult.BankData));
    
            foreach (TransactionsByCategoryId bankData in apiResponse.Result!.BankData)
            {
                Assert.Contains(data.ExpectedResult.BankData, expectedBank => expectedBank.Date == bankData.Date);
            }
        }
    }

    [Theory]
    [ClassData(typeof(TransactionsByIdTestData))]
    public async Task Transaction_By_Id(TransactionByIdTestDef data)
    {
        await using (TransactionsInsertDisposals disposableTests = new TransactionsInsertDisposals(_fixture, _client))
        {
            await disposableTests.InsertManyAsync();
            string transactionId;
            
            // Fetch id based on date
            {
                // string date = DateTime.Now.ToString("yyyy-MM-dd");
                HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions/date/{data.date}");
                string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                ApiResponse<ByDateTransactions>? apiResponse = JsonSerializer.Deserialize<ApiResponse<ByDateTransactions>>(jsonResponse);
                transactionId = apiResponse?.Result?.Transactions?.FirstOrDefault()?.TransactionId ?? "";
            }
            
            // Call :searchById API
            {
                HttpResponseMessage httpResponse = await _client.GetAsync($"/transactions/{transactionId}");
                string jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                ApiResponse<Transaction>? apiResponse = JsonSerializer.Deserialize<ApiResponse<Transaction>>(jsonResponse);
                Transaction transaction = apiResponse.Result;
                Transaction expectedTransaction = data.Transaction;
                
                // Assert
                Assert.Equal(expectedTransaction.Amount, transaction.Amount);
                Assert.Equal(expectedTransaction.Description, transaction.Description);
                Assert.Equal(expectedTransaction.CategoryId, transaction.CategoryId);
                Assert.Equal(expectedTransaction.Date, transaction.Date);
                Assert.Equal(expectedTransaction.Due, transaction.Due);
                Assert.Equal(expectedTransaction.DueId, transaction.DueId);
                Assert.Equal(expectedTransaction.FromBank, transaction.FromBank);
                Assert.Equal(expectedTransaction.ToBank, transaction.ToBank);
                Assert.Equal(expectedTransaction.Type, transaction.Type);
            }
        }
    }
}