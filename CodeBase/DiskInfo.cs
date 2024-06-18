using System;

public class DiskInfo
{
    public string Name { get; set; }
    public double TotalSpace { get; set; }
    public double UsedSpace { get; set; }
    public int UsagePercentage => Convert.ToInt32((UsedSpace / TotalSpace) * 100);
}
