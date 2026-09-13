using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace BudgetTracker.Entities;

public class Category : MongoObject
{
    [Required]
    [BsonElement("name")]
    [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "Please provide valid category name.")]
    public string Name { get; private set; }
    
    private Category() { }

    public static Category Create(string name)
    {
        Category category = new Category { Name = name };
        category.SetModifiedAt();
        return category;
    }
}