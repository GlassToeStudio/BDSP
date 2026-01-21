using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BDSP.Core.Berries.Data;
using BDSP.Core.Berries.Extensions;
using Avalonia.Media;
using BDSP.UI.Styling;
namespace BDSP.UI.ViewModels
{

    public sealed class BerryViewModel(Berry berry)
    {
        IBrush PrimaryFlavorBrush => PrimaryFlavor switch
        {
            "Spicy" => FlavorColors.Spicy,
            "Dry" => FlavorColors.Dry,
            "Sweet" => FlavorColors.Sweet,
            "Bitter" => FlavorColors.Bitter,
            _ => FlavorColors.Sour
        };

        IBrush RarityBrush => Rarity switch
        {
            "Common" => FlavorColors.Common,
            "Uncommon" => FlavorColors.Uncommon,
            _ => FlavorColors.Rare
        };

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

