using BDSP.Core.Poffins;

namespace BDSP.UI.Models
{
    public sealed class ContestRadarData
    {
        public double[] Values { get; }
        public string[] Labels { get; }
        public double MaxValue { get; }

        public ContestRadarData(double[] values, string[] labels, double maxValue = 100)
        {
            Values = values;
            Labels = labels;
            MaxValue = maxValue;
        }
    }
}