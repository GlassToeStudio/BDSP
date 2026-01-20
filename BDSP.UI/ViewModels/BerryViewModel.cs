using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BDSP.Core.Berries;

namespace BDSP.UI.ViewModels
{

    public sealed class BerryViewModel(Berry berry)
    {
        public Berry Berry { get; } = berry;
        public string Name => $"{Berry.Id.GetName()}";
        public string PrimaryFlavor => GetPrimaryFlavor(Berry);
        public int Smoothness => Berry.Smoothness;
        public string Rarity => GetRarity(Berry);
        public int Spicy => Berry.Spicy;    
        public int Dry => Berry.Dry;
        public int Sweet => Berry.Sweet;    
        public int Bitter => Berry.Bitter;  
        public int Sour => Berry.Sour;  
        public Bitmap Image
        {
            get
            {
                var uri = new Uri(
                    $"avares://BDSP.UI/Assets/Berries/{Berry.Id.Value}.png");

                return new Bitmap(
                    AssetLoader.Open(uri));
            }
        }

        private static string GetPrimaryFlavor(Berry b)
        {
            var max = Math.Max(
                Math.Max(b.Spicy, b.Dry),
                Math.Max(Math.Max(b.Sweet, b.Bitter), b.Sour));

            if (max == b.Spicy) return "Spicy";
            if (max == b.Dry) return "Dry";
            if (max == b.Sweet) return "Sweet";
            if (max == b.Bitter) return "Bitter";
            return "Sour";
        }

        private static string GetRarity(Berry b)
        {
            // Placeholder – refine later
            return b.Smoothness <= 20 ? "Common" :
                   b.Smoothness <= 30 ? "Uncommon" :
                   "Rare";
        }
    }
}
