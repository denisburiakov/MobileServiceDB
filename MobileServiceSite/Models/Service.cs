using System;
using System.Collections.Generic;

namespace MobileServiceSite.Models;

public partial class Service
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    public int? CategoryId { get; set; }

    public string TypeOfService { get; set; } = null!;

    public string? TimeOfDoing { get; set; }

    public int CostOfService { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Device Device { get; set; } = null!;
}
