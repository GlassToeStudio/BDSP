namespace BDSP.Core.Optimization.Core
{
    /// <summary>
    /// High-level optimization results with ranked feeding plans.
    /// </summary>
    public sealed class OptimizationResult
    {
        /// <summary>Ranked feeding plans (best first).</summary>
        public FeedingPlanResult[] Plans { get; }
        /// <summary>Optional summary or notes.</summary>
        public string? Notes { get; }

        public OptimizationResult(FeedingPlanResult[] plans, string? notes = null)
        {
            Plans = plans;
            Notes = notes;
        }
    }
}
