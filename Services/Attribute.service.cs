using System.Text.RegularExpressions;
using Global;
using Microsoft.AspNetCore.Mvc.Filters;

[AttributeUsage(AttributeTargets.Property)]
public class ValidateAttribute : Attribute
{
    private string regex { get; set; }

    public ValidateAttribute(string _regex)
    {
        Console.WriteLine("Hey");
        regex = _regex;
        throw new Exception();
    }
}