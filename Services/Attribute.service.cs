using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BudgetTracker.Services;
using BudgetTracker.Defination;
using BudgetTracker.Security.Jwt;

namespace BudgetTracker.Attributes
{
    public class MaxDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string date = (string)value;
            DateTime dateTime = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime now = DateTime.Now;
            return now > dateTime;
        }
    }
}