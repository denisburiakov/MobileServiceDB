using System;
using System.Collections.Generic;

namespace MobileServiceSite.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Categories { get; set; } = null!;

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
