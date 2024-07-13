using System.Text.RegularExpressions;
using BudgetTracker.Defination;
using BudgetTracker.Injectors;

namespace BudgetTracker.Services
{
    public class DueService : MongoServices<Due>, IDueService
    {
        public DueService() : base(Collection.Due) {}

        public void Validate(Due payload)
        {
            if (!Enum.IsDefined(typeof(DueStatus), payload.Status))
            {
                throw new BadRequestException("Invalid due status defined");
            }

            if (string.IsNullOrEmpty(payload.From))
            {
                throw new BadRequestException("From field is required");
            }

            if (string.IsNullOrEmpty(payload.To))
            {
                throw new BadRequestException("To field is required");
            }

            // allowing alphabets and single quote
            string pattern = "^[a-zA-Z]*$";
            Regex nameRegex = new Regex(pattern);

            if (!nameRegex.IsMatch(payload.From))
            {
                throw new BadRequestException("From field contains invalid characters");
            }

            if (!nameRegex.IsMatch(payload.To))
            {
                throw new BadRequestException("To field contains invalid characters");
            }
        }
    }
}