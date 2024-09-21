using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bulky.Models
{
    public class Category
    {
        [Key] // Not required when the primary key is already named Id or CategoryId.
              // Entity Framework Core will consider it as a PK.
        public int Id { get; set; }

        [Required] // It will be NOT NULL in the DB settings of the field in the table.
        [MaxLength(30)]
        [DisplayName("Category Name")]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Must be in the 1-100")]
        public int DisplayOrder { get; set; }
    }
}
