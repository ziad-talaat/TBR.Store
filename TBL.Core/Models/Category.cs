using System.ComponentModel.DataAnnotations;


namespace TBL.Core.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="the name can't be blank")]
        [MinLength(3,ErrorMessage ="the name is too short")]
        [MaxLength(100,ErrorMessage ="the name is too long")]
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
    }
}
