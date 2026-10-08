using System.ComponentModel.DataAnnotations;

namespace PresentationMVC.Models
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CategoryCreateModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }


    public class CategoryDeleteModel
    {
        public int Id { get; set; }
    }

    public class CategoryUpdateModel
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class AdminCategoryIndexViewModel
    {
        public IReadOnlyList<CategoryModel> Categories { get; init; } = [];
        public CategoryUpdateModel Form { get; init; } = new();
        public bool IsEditing { get; init; }
        public string Search { get; init; } = string.Empty;
    }
}
