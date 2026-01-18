using BDSP.Core.Poffins;

namespace BDSP.Core.Runner
{

    public readonly struct PoffinSearchResult
    {
        public readonly Poffin[] TopPoffins;

        public PoffinSearchResult(Poffin[] topPoffins)
        {
            TopPoffins = topPoffins;
        }
    }
}