using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Column (TypeName = "varchar(200)")]
        public string Title { get; set; }

        [Column(TypeName = "nvarchar(200)")]
        public string Icon { get; set; } = "";

        [Column(TypeName = "varchar(200)")]
        public string Type { get; set; } = "Expense";

        [NotMapped]
        public string? TitleWithIcon { get {
                return this.Icon + " "
                    + this.Title; } }

    }
}
