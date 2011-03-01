using System.ComponentModel.DataAnnotations;
using Iesi.Collections.Generic;

namespace NhCodeFirst.ExampleDomain
{
    public class PhotoGallery
    {
        public virtual int Id { get; set; }
        [StringLength(100), Required]
        public virtual string Title { get; set; }
        public virtual User Owner { get; set; }
        public virtual ISet<Photo> Photos { get; set; }
    }
    public class Photo
    {
        public virtual int Id { get; set; }
        [StringLength(100), Required]
        public virtual string Caption { get; set; }
        public virtual PhotoGallery Gallery{ get; set; }
    }
}