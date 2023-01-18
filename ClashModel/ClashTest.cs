using System;
using System.Collections.Generic;
using Speckle.Core.Models;

namespace ClashModel;

public class ClashTest : Base
{
    
    public string Guid { set; get; }
    public string Name { get; set; }
    public string Status { get; set; }
    public List<Clash> Clashes { get; set; } = new List<Clash>();
    public DateTime? LastRun { get; set; }
    public int ClashCount { get; set; }
}

public class Clash : Base
{
    public string Guid { set; get; }
    public string Name { get; set; }
    public string Level { get; set; }
    public string Status { get; set; }
    public double Distance { set; get; }
    public string GridIntersect { get; set; }
}

