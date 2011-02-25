using System.ComponentModel.DataAnnotations;

namespace NhCodeFirst.ExampleDomain
{
    public class PhotoGallery
    {
        public virtual int Id { get; set; }
        [StringLength(100), Required]
        public virtual string Title { get; set; }
        public virtual User Owner { get; set; }
    }
}