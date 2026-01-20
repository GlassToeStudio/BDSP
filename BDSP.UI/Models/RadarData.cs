namespace BDSP.UI.Models;

public sealed class RadarData
{
    public double[] Values { get; }
    public string[] Labels { get; }
    public double MaxValue { get; }

    public RadarData(double[] values, string[] labels, double maxValue = 100)
    {
        Values = values;
        Labels = labels;
        MaxValue = maxValue;
    }
}
