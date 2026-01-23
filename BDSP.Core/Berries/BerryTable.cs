using System;
using System.Diagnostics;

namespace BDSP.Core.Berries
{
    public static class BerryTable
    {
                /// <summary>Total number of berries supported by BDSP.</summary>
        public const int Count = 65;

    /// <summary>
    /// Canonical, immutable berry data table for BDSP (Generation VIII).
    /// Index corresponds exactly to <see cref="BerryId.Value"/>.
    /// </summary>
        private static readonly Berry[] Berries =
        {
            //  aguav   (Bitter) 25 - Flavors [  0,   0,   0,  15,   0] Rarity:  3, Main: Bitter(15) Secondary: None(0) NumFlavors: 1
            new Berry(new BerryId(0), spicy: 0, dry: 0, sweet: 0, bitter: 15, sour: 0, smoothness: 25, rarity: 3, mainFlavor: Flavor.Bitter, secondaryFlavor: Flavor.None, mainFlavorValue: 15, secondaryFlavorValue: 0, numFlavors: 1),
            //  apicot  (Dry) 40 - Flavors [ 10,  30,   0,   0,  30] Rarity:  9, Main: Dry(30) Secondary: Sour(30) NumFlavors: 3
            new Berry(new BerryId(1), spicy: 10, dry: 30, sweet: 0, bitter: 0, sour: 30, smoothness: 40, rarity: 9, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Sour, mainFlavorValue: 30, secondaryFlavorValue: 30, numFlavors: 3),
            //  aspear  (Sour) 25 - Flavors [  0,   0,   0,   0,  10] Rarity:  3, Main: Sour(10) Secondary: None(0) NumFlavors: 1
            new Berry(new BerryId(2), spicy: 0, dry: 0, sweet: 0, bitter: 0, sour: 10, smoothness: 25, rarity: 3, mainFlavor: Flavor.Sour, secondaryFlavor: Flavor.None, mainFlavorValue: 10, secondaryFlavorValue: 0, numFlavors: 1),
            //  babiri  (Spicy) 35 - Flavors [ 25,  10,   0,   0,   0] Rarity:  7, Main: Spicy(25) Secondary: Dry(10) NumFlavors: 2
            new Berry(new BerryId(3), spicy: 25, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 35, rarity: 7, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Dry, mainFlavorValue: 25, secondaryFlavorValue: 10, numFlavors: 2),
            //  belue   (Sour) 35 - Flavors [ 10,   0,   0,   0,  30] Rarity:  7, Main: Sour(30) Secondary: Spicy(10) NumFlavors: 2
            new Berry(new BerryId(4), spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 30, smoothness: 35, rarity: 7, mainFlavor: Flavor.Sour, secondaryFlavor: Flavor.Spicy, mainFlavorValue: 30, secondaryFlavorValue: 10, numFlavors: 2),
            //  bluk    (Dry) 20 - Flavors [  0,  10,  10,   0,   0] Rarity:  1, Main: Dry(10) Secondary: Sweet(10) NumFlavors: 2
            new Berry(new BerryId(5), spicy: 0, dry: 10, sweet: 10, bitter: 0, sour: 0, smoothness: 20, rarity: 1, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 2),
            //  charti  (Dry) 35 - Flavors [ 10,  20,   0,   0,   0] Rarity:  7, Main: Dry(20) Secondary: Spicy(10) NumFlavors: 2
            new Berry(new BerryId(6), spicy: 10, dry: 20, sweet: 0, bitter: 0, sour: 0, smoothness: 35, rarity: 7, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Spicy, mainFlavorValue: 20, secondaryFlavorValue: 10, numFlavors: 2),
            //   cheri   (Spicy) 25 - Flavors [ 10,   0,   0,   0,   0] Rarity:  3, Main: Spicy(10) Secondary: None(0) NumFlavors: 1
            new Berry(new BerryId(7), spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 0, smoothness: 25, rarity: 3, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.None, mainFlavorValue: 10, secondaryFlavorValue: 0, numFlavors: 1),
            //   chesto  (Dry) 25 - Flavors [  0,  10,   0,   0,   0] Rarity:  3, Main: Dry(10) Secondary: None(0) NumFlavors: 1
            new Berry(new BerryId(8), spicy: 0, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 25, rarity: 3, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.None, mainFlavorValue: 10, secondaryFlavorValue: 0, numFlavors: 1),
            //   chilan  (Dry) 35 - Flavors [  0,  25,  10,   0,   0] Rarity:  7, Main: Dry(25) Secondary: Sweet(10) NumFlavors: 2
            new Berry(new BerryId(9), spicy: 0, dry: 25, sweet: 10, bitter: 0, sour: 0, smoothness: 35, rarity: 7, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 25, secondaryFlavorValue: 10, numFlavors: 2),
            //   chople  (Spicy) 30 - Flavors [ 15,   0,   0,  10,   0] Rarity:  5, Main: Spicy(15) Secondary: Bitter(10) NumFlavors: 2
            new Berry(new BerryId(10), spicy: 15, dry: 0, sweet: 0, bitter: 10, sour: 0, smoothness: 30, rarity: 5, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 15, secondaryFlavorValue: 10, numFlavors: 2),
            //   coba    (Bitter) 30 - Flavors [  0,  10,   0,  15,   0] Rarity:  5, Main: Bitter(15) Secondary: Dry(10) NumFlavors: 2
            new Berry(new BerryId(11), spicy: 0, dry: 10, sweet: 0, bitter: 15, sour: 0, smoothness: 30, rarity: 5, mainFlavor: Flavor.Bitter, secondaryFlavor: Flavor.Dry, mainFlavorValue: 15, secondaryFlavorValue: 10, numFlavors: 2),
            //   colbur  (Sour) 35 - Flavors [  0,   0,   0,  10,  20] Rarity:  7, Main: Sour(20) Secondary: Bitter(10) NumFlavors: 2
            new Berry(new BerryId(12), spicy: 0, dry: 0, sweet: 0, bitter: 10, sour: 20, smoothness: 35, rarity: 7, mainFlavor: Flavor.Sour, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 20, secondaryFlavorValue: 10, numFlavors: 2),
            //   cornn   (Dry) 30 - Flavors [  0,  20,  10,   0,   0] Rarity:  5, Main: Dry(20) Secondary: Sweet(10) NumFlavors: 2
            new Berry(new BerryId(13), spicy: 0, dry: 20, sweet: 10, bitter: 0, sour: 0, smoothness: 30, rarity: 5, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 20, secondaryFlavorValue: 10, numFlavors: 2),
            //   custap  (Sweet) 60 - Flavors [  0,   0,  40,  10,   0] Rarity: 15, Main: Sweet(40) Secondary: Bitter(10) NumFlavors: 2
            new Berry(new BerryId(14), spicy: 0, dry: 0, sweet: 40, bitter: 10, sour: 0, smoothness: 60, rarity: 15, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 40, secondaryFlavorValue: 10, numFlavors: 2),
            //   durin   (Bitter) 35 - Flavors [  0,   0,   0,  30,  10] Rarity:  7, Main: Bitter(30) Secondary: Sour(10) NumFlavors: 2
            new Berry(new BerryId(15), spicy: 0, dry: 0, sweet: 0, bitter: 30, sour: 10, smoothness: 35, rarity: 7, mainFlavor: Flavor.Bitter, secondaryFlavor: Flavor.Sour, mainFlavorValue: 30, secondaryFlavorValue: 10, numFlavors: 2),
            //   enigma  (Spicy) 60 - Flavors [ 40,  10,   0,   0,   0] Rarity: 15, Main: Spicy(40) Secondary: Dry(10) NumFlavors: 2
            new Berry(new BerryId(16), spicy: 40, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 60, rarity: 15, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Dry, mainFlavorValue: 40, secondaryFlavorValue: 10, numFlavors: 2),
            //   figy    (Spicy) 25 - Flavors [ 15,   0,   0,   0,   0] Rarity:  3, Main: Spicy(15) Secondary: None(0) NumFlavors: 1
            new Berry(new BerryId(17), spicy: 15, dry: 0, sweet: 0, bitter: 0, sour: 0, smoothness: 25, rarity: 3, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.None, mainFlavorValue: 15, secondaryFlavorValue: 0, numFlavors: 1),
            //   ganlon  (Dry) 40 - Flavors [  0,  30,  10,  30,   0] Rarity:  9, Main: Dry(30) Secondary: Bitter(30) NumFlavors: 3
            new Berry(new BerryId(18), spicy: 0, dry: 30, sweet: 10, bitter: 30, sour: 0, smoothness: 40, rarity: 9, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 30, secondaryFlavorValue: 30, numFlavors: 3),
            //   grepa   (Dry) 20 - Flavors [  0,  10,  10,   0,  10] Rarity:  1, Main: Dry(10) Secondary: Sweet(10) NumFlavors: 3
            new Berry(new BerryId(19), spicy: 0, dry: 10, sweet: 10, bitter: 0, sour: 10, smoothness: 20, rarity: 1, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 3),
            //   haban   (Bitter) 35 - Flavors [  0,   0,  10,  20,   0] Rarity:  7, Main: Bitter(20) Secondary: Sweet(10) NumFlavors: 2
            new Berry(new BerryId(20), spicy: 0, dry: 0, sweet: 10, bitter: 20, sour: 0, smoothness: 35, rarity: 7, mainFlavor: Flavor.Bitter, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 20, secondaryFlavorValue: 10, numFlavors: 2),
            //   hondew  (Spicy) 20 - Flavors [ 10,  10,   0,  10,   0] Rarity:  1, Main: Spicy(10) Secondary: Dry(10) NumFlavors: 3
            new Berry(new BerryId(21), spicy: 10, dry: 10, sweet: 0, bitter: 10, sour: 0, smoothness: 20, rarity: 1, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Dry, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 3),
            //   iapapa  (Sour) 25 - Flavors [  0,   0,   0,   0,  15] Rarity:  3, Main: Sour(15) Secondary: None(0) NumFlavors: 1
            new Berry(new BerryId(22), spicy: 0, dry: 0, sweet: 0, bitter: 0, sour: 15, smoothness: 25, rarity: 3, mainFlavor: Flavor.Sour, secondaryFlavor: Flavor.None, mainFlavorValue: 15, secondaryFlavorValue: 0, numFlavors: 1),
            //   jaboca  (Bitter) 60 - Flavors [  0,   0,   0,  40,  10] Rarity: 15, Main: Bitter(40) Secondary: Sour(10) NumFlavors: 2
            new Berry(new BerryId(23), spicy: 0, dry: 0, sweet: 0, bitter: 40, sour: 10, smoothness: 60, rarity: 15, mainFlavor: Flavor.Bitter, secondaryFlavor: Flavor.Sour, mainFlavorValue: 40, secondaryFlavorValue: 10, numFlavors: 2),
            //   kasib   (Sweet) 35 - Flavors [  0,  10,  20,   0,   0] Rarity:  7, Main: Sweet(20) Secondary: Dry(10) NumFlavors: 2
            new Berry(new BerryId(24), spicy: 0, dry: 10, sweet: 20, bitter: 0, sour: 0, smoothness: 35, rarity: 7, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.Dry, mainFlavorValue: 20, secondaryFlavorValue: 10, numFlavors: 2),
            //   kebia   (Dry) 30 - Flavors [  0,  15,   0,   0,  10] Rarity:  5, Main: Dry(15) Secondary: Sour(10) NumFlavors: 2
            new Berry(new BerryId(25), spicy: 0, dry: 15, sweet: 0, bitter: 0, sour: 10, smoothness: 30, rarity: 5, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Sour, mainFlavorValue: 15, secondaryFlavorValue: 10, numFlavors: 2),
            //   kelpsy  (Dry) 20 - Flavors [  0,  10,   0,  10,  10] Rarity:  1, Main: Dry(10) Secondary: Bitter(10) NumFlavors: 3
            new Berry(new BerryId(26), spicy: 0, dry: 10, sweet: 0, bitter: 10, sour: 10, smoothness: 20, rarity: 1, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 3),
            //   lansat  (Spicy) 50 - Flavors [ 30,  10,  30,  10,  30] Rarity: 11, Main: Spicy(30) Secondary: Sweet(30) NumFlavors: 5
            new Berry(new BerryId(27), spicy: 30, dry: 10, sweet: 30, bitter: 10, sour: 30, smoothness: 50, rarity: 11, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 30, secondaryFlavorValue: 30, numFlavors: 5),
            //   leppa   (Spicy) 20 - Flavors [ 10,   0,  10,  10,  10] Rarity:  1, Main: Spicy(10) Secondary: Sweet(10) NumFlavors: 4
            new Berry(new BerryId(28), spicy: 10, dry: 0, sweet: 10, bitter: 10, sour: 10, smoothness: 20, rarity: 1, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 4),
            //   liechi  (Spicy) 40 - Flavors [ 30,  10,  30,   0,   0] Rarity:  9, Main: Spicy(30) Secondary: Sweet(30) NumFlavors: 3
            new Berry(new BerryId(29), spicy: 30, dry: 10, sweet: 30, bitter: 0, sour: 0, smoothness: 40, rarity: 9, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 30, secondaryFlavorValue: 30, numFlavors: 3),
            //   lum     (Spicy) 20 - Flavors [ 10,  10,  10,  10,   0] Rarity:  1, Main: Spicy(10) Secondary: Dry(10) NumFlavors: 4
            new Berry(new BerryId(30), spicy: 10, dry: 10, sweet: 10, bitter: 10, sour: 0, smoothness: 20, rarity: 1, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Dry, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 4),
            //   mago    (Sweet) 25 - Flavors [  0,   0,  15,   0,   0] Rarity:  3, Main: Sweet(15) Secondary: None(0) NumFlavors: 1
            new Berry(new BerryId(31), spicy: 0, dry: 0, sweet: 15, bitter: 0, sour: 0, smoothness: 25, rarity: 3, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.None, mainFlavorValue: 15, secondaryFlavorValue: 0, numFlavors: 1),
            //   magost  (Sweet) 30 - Flavors [  0,   0,  20,  10,   0] Rarity:  5, Main: Sweet(20) Secondary: Bitter(10) NumFlavors: 2
            new Berry(new BerryId(32), spicy: 0, dry: 0, sweet: 20, bitter: 10, sour: 0, smoothness: 30, rarity: 5, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 20, secondaryFlavorValue: 10, numFlavors: 2),
            //   micle   (Dry) 60 - Flavors [  0,  40,  10,   0,   0] Rarity: 15, Main: Dry(40) Secondary: Sweet(10) NumFlavors: 2
            new Berry(new BerryId(33), spicy: 0, dry: 40, sweet: 10, bitter: 0, sour: 0, smoothness: 60, rarity: 15, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 40, secondaryFlavorValue: 10, numFlavors: 2),
            //   nanab   (Sweet) 20 - Flavors [  0,   0,  10,  10,   0] Rarity:  1, Main: Sweet(10) Secondary: Bitter(10) NumFlavors: 2
            new Berry(new BerryId(34), spicy: 0, dry: 0, sweet: 10, bitter: 10, sour: 0, smoothness: 20, rarity: 1, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 2),
            //   nomel   (Sour) 30 - Flavors [ 10,   0,   0,   0,  20] Rarity:  5, Main: Sour(20) Secondary: Spicy(10) NumFlavors: 2
            new Berry(new BerryId(35), spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 20, smoothness: 30, rarity: 5, mainFlavor: Flavor.Sour, secondaryFlavor: Flavor.Spicy, mainFlavorValue: 20, secondaryFlavorValue: 10, numFlavors: 2),
            //   occa    (Spicy) 30 - Flavors [ 15,   0,  10,   0,   0] Rarity:  5, Main: Spicy(15) Secondary: Sweet(10) NumFlavors: 2
            new Berry(new BerryId(36), spicy: 15, dry: 0, sweet: 10, bitter: 0, sour: 0, smoothness: 30, rarity: 5, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 15, secondaryFlavorValue: 10, numFlavors: 2),
            //   oran    (Spicy) 20 - Flavors [ 10,  10,   0,  10,  10] Rarity:  1, Main: Spicy(10) Secondary: Dry(10) NumFlavors: 4
            new Berry(new BerryId(37), spicy: 10, dry: 10, sweet: 0, bitter: 10, sour: 10, smoothness: 20, rarity: 1, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Dry, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 4),
            //   pamtre  (Dry) 35 - Flavors [  0,  30,  10,   0,   0] Rarity:  7, Main: Dry(30) Secondary: Sweet(10) NumFlavors: 2
            new Berry(new BerryId(38), spicy: 0, dry: 30, sweet: 10, bitter: 0, sour: 0, smoothness: 35, rarity: 7, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 30, secondaryFlavorValue: 10, numFlavors: 2),
            //   passho  (Dry) 30 - Flavors [  0,  15,   0,  10,   0] Rarity:  5, Main: Dry(15) Secondary: Bitter(10) NumFlavors: 2
            new Berry(new BerryId(39), spicy: 0, dry: 15, sweet: 0, bitter: 10, sour: 0, smoothness: 30, rarity: 5, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 15, secondaryFlavorValue: 10, numFlavors: 2),
            //   payapa  (Sour) 30 - Flavors [  0,   0,  10,   0,  15] Rarity:  5, Main: Sour(15) Secondary: Sweet(10) NumFlavors: 2
            new Berry(new BerryId(40), spicy: 0, dry: 0, sweet: 10, bitter: 0, sour: 15, smoothness: 30, rarity: 5, mainFlavor: Flavor.Sour, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 15, secondaryFlavorValue: 10, numFlavors: 2),
            //   pecha   (Sweet) 25 - Flavors [  0,   0,  10,   0,   0] Rarity:  3, Main: Sweet(10) Secondary: None(0) NumFlavors: 1
            new Berry(new BerryId(41), spicy: 0, dry: 0, sweet: 10, bitter: 0, sour: 0, smoothness: 25, rarity: 3, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.None, mainFlavorValue: 10, secondaryFlavorValue: 0, numFlavors: 1),
            //   persim  (Spicy) 20 - Flavors [ 10,  10,  10,   0,  10] Rarity:  1, Main: Spicy(10) Secondary: Dry(10) NumFlavors: 4
            new Berry(new BerryId(42), spicy: 10, dry: 10, sweet: 10, bitter: 0, sour: 10, smoothness: 20, rarity: 1, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Dry, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 4),
            //   petaya  (Spicy) 40 - Flavors [ 30,   0,   0,  30,  10] Rarity:  9, Main: Spicy(30) Secondary: Bitter(30) NumFlavors: 3
            new Berry(new BerryId(43), spicy: 30, dry: 0, sweet: 0, bitter: 30, sour: 10, smoothness: 40, rarity: 9, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 30, secondaryFlavorValue: 30, numFlavors: 3),
            //   pinap   (Spicy) 20 - Flavors [ 10,   0,   0,   0,  10] Rarity:  1, Main: Spicy(10) Secondary: Sour(10) NumFlavors: 2
            new Berry(new BerryId(44), spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 10, smoothness: 20, rarity: 1, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Sour, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 2),
            //   pomeg   (Spicy) 20 - Flavors [ 10,   0,  10,  10,   0] Rarity:  1, Main: Spicy(10) Secondary: Sweet(10) NumFlavors: 3
            new Berry(new BerryId(45), spicy: 10, dry: 0, sweet: 10, bitter: 10, sour: 0, smoothness: 20, rarity: 1, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 3),
            //   qualot  (Spicy) 20 - Flavors [ 10,   0,  10,   0,  10] Rarity:  1, Main: Spicy(10) Secondary: Sweet(10) NumFlavors: 3
            new Berry(new BerryId(46), spicy: 10, dry: 0, sweet: 10, bitter: 0, sour: 10, smoothness: 20, rarity: 1, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 3),
            //   rabuta  (Bitter) 30 - Flavors [  0,   0,   0,  20,  10] Rarity:  5, Main: Bitter(20) Secondary: Sour(10) NumFlavors: 2
            new Berry(new BerryId(47), spicy: 0, dry: 0, sweet: 0, bitter: 20, sour: 10, smoothness: 30, rarity: 5, mainFlavor: Flavor.Bitter, secondaryFlavor: Flavor.Sour, mainFlavorValue: 20, secondaryFlavorValue: 10, numFlavors: 2),
            //   rawst   (Bitter) 25 - Flavors [  0,   0,   0,  10,   0] Rarity:  3, Main: Bitter(10) Secondary: None(0) NumFlavors: 1
            new Berry(new BerryId(48), spicy: 0, dry: 0, sweet: 0, bitter: 10, sour: 0, smoothness: 25, rarity: 3, mainFlavor: Flavor.Bitter, secondaryFlavor: Flavor.None, mainFlavorValue: 10, secondaryFlavorValue: 0, numFlavors: 1),
            //   razz    (Spicy) 20 - Flavors [ 10,  10,   0,   0,   0] Rarity:  1, Main: Spicy(10) Secondary: Dry(10) NumFlavors: 2
            new Berry(new BerryId(49), spicy: 10, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 20, rarity: 1, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Dry, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 2),
            //   rindo   (Bitter) 30 - Flavors [ 10,   0,   0,  15,   0] Rarity:  5, Main: Bitter(15) Secondary: Spicy(10) NumFlavors: 2
            new Berry(new BerryId(50), spicy: 10, dry: 0, sweet: 0, bitter: 15, sour: 0, smoothness: 30, rarity: 5, mainFlavor: Flavor.Bitter, secondaryFlavor: Flavor.Spicy, mainFlavorValue: 15, secondaryFlavorValue: 10, numFlavors: 2),
            //   roseli  (Sweet) 35 - Flavors [  0,   0,  25,  10,   0] Rarity:  7, Main: Sweet(25) Secondary: Bitter(10) NumFlavors: 2
            new Berry(new BerryId(51), spicy: 0, dry: 0, sweet: 25, bitter: 10, sour: 0, smoothness: 35, rarity: 7, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 25, secondaryFlavorValue: 10, numFlavors: 2),
            //   rowap   (Sour) 60 - Flavors [ 10,   0,   0,   0,  40] Rarity: 15, Main: Sour(40) Secondary: Spicy(10) NumFlavors: 2
            new Berry(new BerryId(52), spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 40, smoothness: 60, rarity: 15, mainFlavor: Flavor.Sour, secondaryFlavor: Flavor.Spicy, mainFlavorValue: 40, secondaryFlavorValue: 10, numFlavors: 2),
            //   salac   (Sweet) 40 - Flavors [  0,   0,  30,  10,  30] Rarity:  9, Main: Sweet(30) Secondary: Sour(30) NumFlavors: 3
            new Berry(new BerryId(53), spicy: 0, dry: 0, sweet: 30, bitter: 10, sour: 30, smoothness: 40, rarity: 9, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.Sour, mainFlavorValue: 30, secondaryFlavorValue: 30, numFlavors: 3),
            //   shuca   (Sweet) 30 - Flavors [ 10,   0,  15,   0,   0] Rarity:  5, Main: Sweet(15) Secondary: Spicy(10) NumFlavors: 2
            new Berry(new BerryId(54), spicy: 10, dry: 0, sweet: 15, bitter: 0, sour: 0, smoothness: 30, rarity: 5, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.Spicy, mainFlavorValue: 15, secondaryFlavorValue: 10, numFlavors: 2),
            //   sitrus  (Dry) 20 - Flavors [  0,  10,  10,  10,  10] Rarity:  1, Main: Dry(10) Secondary: Sweet(10) NumFlavors: 4
            new Berry(new BerryId(55), spicy: 0, dry: 10, sweet: 10, bitter: 10, sour: 10, smoothness: 20, rarity: 1, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 4),
            //   spelon  (Spicy) 35 - Flavors [ 30,  10,   0,   0,   0] Rarity:  7, Main: Spicy(30) Secondary: Dry(10) NumFlavors: 2
            new Berry(new BerryId(56), spicy: 30, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 35, rarity: 7, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Dry, mainFlavorValue: 30, secondaryFlavorValue: 10, numFlavors: 2),
            //   starf   (Spicy) 50 - Flavors [ 30,  10,  30,  10,  30] Rarity: 11, Main: Spicy(30) Secondary: Sweet(30) NumFlavors: 5
            new Berry(new BerryId(57), spicy: 30, dry: 10, sweet: 30, bitter: 10, sour: 30, smoothness: 50, rarity: 11, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Sweet, mainFlavorValue: 30, secondaryFlavorValue: 30, numFlavors: 5),
            //   tamato  (Spicy) 30 - Flavors [ 20,  10,   0,   0,   0] Rarity:  5, Main: Spicy(20) Secondary: Dry(10) NumFlavors: 2
            new Berry(new BerryId(58), spicy: 20, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 30, rarity: 5, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Dry, mainFlavorValue: 20, secondaryFlavorValue: 10, numFlavors: 2),
            //   tanga   (Spicy) 35 - Flavors [ 20,   0,   0,   0,  10] Rarity:  7, Main: Spicy(20) Secondary: Sour(10) NumFlavors: 2
            new Berry(new BerryId(59), spicy: 20, dry: 0, sweet: 0, bitter: 0, sour: 10, smoothness: 35, rarity: 7, mainFlavor: Flavor.Spicy, secondaryFlavor: Flavor.Sour, mainFlavorValue: 20, secondaryFlavorValue: 10, numFlavors: 2),
            //   wacan   (Sweet) 30 - Flavors [  0,   0,  15,   0,  10] Rarity:  5, Main: Sweet(15) Secondary: Sour(10) NumFlavors: 2
            new Berry(new BerryId(60), spicy: 0, dry: 0, sweet: 15, bitter: 0, sour: 10, smoothness: 30, rarity: 5, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.Sour, mainFlavorValue: 15, secondaryFlavorValue: 10, numFlavors: 2),
            //   watmel  (Sweet) 35 - Flavors [  0,   0,  30,  10,   0] Rarity:  7, Main: Sweet(30) Secondary: Bitter(10) NumFlavors: 2
            new Berry(new BerryId(61), spicy: 0, dry: 0, sweet: 30, bitter: 10, sour: 0, smoothness: 35, rarity: 7, mainFlavor: Flavor.Sweet, secondaryFlavor: Flavor.Bitter, mainFlavorValue: 30, secondaryFlavorValue: 10, numFlavors: 2),
            //   wepear  (Bitter) 20 - Flavors [  0,   0,   0,  10,  10] Rarity:  1, Main: Bitter(10) Secondary: Sour(10) NumFlavors: 2
            new Berry(new BerryId(62), spicy: 0, dry: 0, sweet: 0, bitter: 10, sour: 10, smoothness: 20, rarity: 1, mainFlavor: Flavor.Bitter, secondaryFlavor: Flavor.Sour, mainFlavorValue: 10, secondaryFlavorValue: 10, numFlavors: 2),
            //   wiki    (Dry) 25 - Flavors [  0,  15,   0,   0,   0] Rarity:  3, Main: Dry(15) Secondary: None(0) NumFlavors: 1
            new Berry(new BerryId(63), spicy: 0, dry: 15, sweet: 0, bitter: 0, sour: 0, smoothness: 25, rarity: 3, mainFlavor: Flavor.Dry, secondaryFlavor: Flavor.None, mainFlavorValue: 15, secondaryFlavorValue: 0, numFlavors: 1),
            //   yache   (Sour) 30 - Flavors [  0,  10,   0,   0,  15] Rarity:  5, Main: Sour(15) Secondary: Dry(10) NumFlavors: 2
            new Berry(new BerryId(64), spicy: 0, dry: 10, sweet: 0, bitter: 0, sour: 15, smoothness: 30, rarity: 5, mainFlavor: Flavor.Sour, secondaryFlavor: Flavor.Dry, mainFlavorValue: 15, secondaryFlavorValue: 10, numFlavors: 2),
        };

        /// <summary>
        /// Returns a read-only span of all berries in ID order.
        /// <include file='BerryDocs.xml' path='docs/members/member[@name="T:FullBerryTable"]/*' />
        /// </summary>
        public static ReadOnlySpan<Berry> All => Berries;

        /// <summary>
        /// Returns a read-only span of base berries in ID order.
        /// Base berries include only the five flavor values and smoothness.
        /// </summary>
        public static ReadOnlySpan<BerryBase> BaseAll => BaseBerries;

        /// <summary>
        /// Retrieves a berry by its ID.
        /// <include file='BerryDocs.xml' path='docs/members/member[@name="T:FullBerryTable"]/*' />
        /// </summary>
        /// <param name="id">The berry identifier.</param>
        /// <returns>The corresponding <see cref="Berry"/>.</returns>
        public static ref readonly Berry Get(in BerryId id)
        {
#if DEBUG
            Debug.Assert(id.Value < Count, "Invalid BerryId.");
#endif
            return ref Berries[id.Value];
        }

        /// <summary>
        /// Retrieves the base berry by its ID.
        /// </summary>
        public static ref readonly BerryBase GetBase(in BerryId id)
        {
#if DEBUG
            Debug.Assert(id.Value < Count, "Invalid BerryId.");
#endif
            return ref BaseBerries[id.Value];
        }

        private static readonly BerryBase[] BaseBerries = CreateBaseBerries();

        private static BerryBase[] CreateBaseBerries()
        {
            var bases = new BerryBase[Count];
            for (var i = 0; i < Count; i++)
            {
                ref readonly var berry = ref Berries[i];
                bases[i] = new BerryBase(
                    berry.Id,
                    berry.Spicy,
                    berry.Dry,
                    berry.Sweet,
                    berry.Bitter,
                    berry.Sour,
                    berry.Smoothness);
            }
            return bases;
        }
    }
}

