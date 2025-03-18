using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BudgetTracker.Services;
using BudgetTracker.Defination;

namespace BudgetTracker.Attributes
{
    public class MaxDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            string date = (string)value;
            Regex regex = new Regex(@"^\d{4}-\d{2}-\d{2}$");
            Match match = regex.Match(date);
            
            if (!match.Success) return false;
            
            DateTime dateTime = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime now = DateTime.Now;
            return now > dateTime;
        }
    }
}