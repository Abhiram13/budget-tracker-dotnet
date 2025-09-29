using BudgetTracker.Tests.IntegrationTests.Definations.Dues;
using BudgetTracker.Core.Domain.Entities;
using BudgetTracker.Core.Domain.Enums;

namespace BudgetTracker.Tests.IntegrationTests.Data.Dues;

public abstract class DueTheoryTestData<T> : TheoryData<T> where T : class { }

public class DueInsertTestData : DueTheoryTestData<DueInsertTestDef>
{
    public DueInsertTestData()
    {
        Add(new(
            ExpectedHttpStatusCode: 201,
            ExpectedStatusCode: 201,
            ExpectedMessage: "Due created successfully",
            DueBody: new Due
            {
                Name = "Sample Test Due",
                Description = "This is just a sample test due description",
                Payee = "Abhi",
                PrincipalAmount = 10000,
                Status = DueStatus.Active,
                StartDate = DateTime.Today,
                Comment = "Random comments :)"
            }
        ));

        Add(new(
            ExpectedHttpStatusCode: 400,
            ExpectedStatusCode: 400,
            ExpectedMessage: "Start date cannot be greater than the current date",
            DueBody: new Due
            {
                Name = "Future Due",
                Description = "This is just a sample future test due",
                Payee = "Ram",
                PrincipalAmount = 10000,
                Status = DueStatus.Active,
                StartDate = DateTime.Today.AddDays(2),
                Comment = "Random comments :)"
            }
        ));

        Add(new(
            ExpectedHttpStatusCode: 400,
            ExpectedStatusCode: 400,
            ExpectedMessage: "Invalid Due Name provided",
            DueBody: new Due
            {
                Name = "Invalid Due @-!",
                Description = "This is just an Invalid test due",
                Payee = "Ram",
                PrincipalAmount = 10000,
                Status = DueStatus.Active,
                StartDate = DateTime.Today,
                Comment = "Random comments :)"
            }
        ));

        Add(new(
            ExpectedHttpStatusCode: 400,
            ExpectedStatusCode: 400,
            ExpectedMessage: "The Name field is required.",
            DueBody: new Due
            {
                Name = "",
                Description = "This is just an Empty due name test due",
                Payee = "Ram",
                PrincipalAmount = 10000,
                Status = DueStatus.Active,
                StartDate = DateTime.Today,
                Comment = "Random comments :)"
            }
        ));

        Add(new(
            ExpectedHttpStatusCode: 400,
            ExpectedStatusCode: 400,
            ExpectedMessage: "Principle Amount should be greater than 0",
            DueBody: new Due
            {
                Name = "Invalid Principle amount",
                Description = "This is just an Invalid principle amount test due",
                Payee = "Ram",
                PrincipalAmount = 0,
                Status = DueStatus.Active,
                StartDate = DateTime.Today,
                Comment = "Random comments :)"
            }
        ));

        Add(new(
            ExpectedHttpStatusCode: 400,
            ExpectedStatusCode: 400,
            ExpectedMessage: "Invalid Due status provided",
            DueBody: new Due
            {
                Name = "Invalid Due status",
                Description = "This is just an Invalid status test due",
                Payee = "Ram",
                PrincipalAmount = 10000,
                Status = DueStatus.Ended,
                StartDate = DateTime.Today,
                Comment = "Random comments :)"
            }
        ));
    }
}

public class DueDetailsTestData : DueTheoryTestData<DueDetailsTestDef>
{
    public DueDetailsTestData()
    {
        Add(new(
            ExpectedStatusCode: 200,
            ExpectedHttpStatusCode: 200
        ));
    }
}