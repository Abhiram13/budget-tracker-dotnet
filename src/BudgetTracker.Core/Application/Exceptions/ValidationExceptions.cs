using System;

namespace BudgetTracker.Core.Application.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base (message) { }
}