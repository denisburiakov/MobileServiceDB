using System;
using System.Collections.Generic;

namespace MobileServiceSite.Models;

public partial class Detail
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    public string TypeOfDetail { get; set; } = null!;

    public int CostOfDetail { get; set; }

    public string ProducerDet { get; set; } = null!;

    public virtual Device Device { get; set; } = null!;
}
