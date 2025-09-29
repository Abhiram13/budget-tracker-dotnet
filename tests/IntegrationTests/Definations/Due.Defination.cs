using BudgetTracker.Core.Domain.Entities;

namespace BudgetTracker.Tests.IntegrationTests.Definations.Dues;

public record DueInsertTestDef(
    int ExpectedStatusCode,
    int ExpectedHttpStatusCode,
    string ExpectedMessage,
    Due DueBody
);