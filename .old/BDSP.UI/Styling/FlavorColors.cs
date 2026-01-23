using Avalonia.Media;

namespace BDSP.UI.Styling
{

    public static class FlavorColors
    {
        public static readonly IBrush Spicy = new SolidColorBrush(Color.Parse("#E53935"));
        public static readonly IBrush Dry = new SolidColorBrush(Color.Parse("#1E88E5"));
        public static readonly IBrush Sweet = new SolidColorBrush(Color.Parse("#F06292"));
        public static readonly IBrush Bitter = new SolidColorBrush(Color.Parse("#43A047"));
        public static readonly IBrush Sour = new SolidColorBrush(Color.Parse("#FBC02D"));

        public static readonly IBrush Common = new SolidColorBrush(Color.Parse("#B0BEC5"));
        public static readonly IBrush Uncommon = new SolidColorBrush(Color.Parse("#4CAF50"));
        public static readonly IBrush Rare = new SolidColorBrush(Color.Parse("#FFD54F"));
    }
}