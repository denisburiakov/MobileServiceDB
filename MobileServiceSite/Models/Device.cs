using System;
using System.Collections.Generic;

namespace MobileServiceSite.Models;

public partial class Device
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string TypeOfDevice { get; set; } = null!;

    public string Producer { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string SerialNumber { get; set; } = null!;

    public string DefDescriotion { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<Detail> Details { get; set; } = new List<Detail>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
