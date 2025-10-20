using System.Collections.Generic;

namespace EVehicleManagementAPI.Models
{
    public class PostPackage
    {
        public int PackageId { get; set; }
        public string Name { get; set; }
        public int DurationDay { get; set; }
        public decimal Price { get; set; }
        public int PriorityLevel { get; set; }
        public string Description { get; set; }

        // Navigation properties
        public ICollection<PostPackageSub> PostPackageSubs { get; set; } = new List<PostPackageSub>();
    }
}
