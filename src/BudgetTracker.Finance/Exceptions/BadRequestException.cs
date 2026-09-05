using System;

namespace BudgetTracker.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }

    public BadRequestException(string message, params object[] args) : base(message) { }
}