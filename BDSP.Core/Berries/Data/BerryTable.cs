using System.Diagnostics;

namespace BDSP.Core.Berries.Data
{
    /// <summary>
    /// Canonical, immutable berry data table for BDSP (Generation VIII).
    /// Index corresponds exactly to <see cref="BerryId.Value"/>.
    /// </summary>
    public static class BerryTable
    {
        /// <summary>Total number of berries supported by BDSP.</summary>
        public const int Count = 65;
        private static readonly Berry[] _berries =
        [
            // 🍐 aguav  (Bitter) 25 - Flavors [  0,   0,   0,  15,   0] Rarity:  3
            new Berry(new BerryId(0), spicy: 0, dry: 0, sweet: 0, bitter: 15, sour: 0, smoothness: 25, rarity: 3),
            // 🍇 apicot (Dry)    40 - Flavors [ 10,  30,   0,   0,  30] Rarity:  9
            new Berry(new BerryId(1), spicy: 10, dry: 30, sweet: 0, bitter: 0, sour: 30, smoothness: 40, rarity: 9),
            // 🍋 aspear (Sour)   25 - Flavors [  0,   0,   0,   0,  10] Rarity:  3
            new Berry(new BerryId(2), spicy: 0, dry: 0, sweet: 0, bitter: 0, sour: 10, smoothness: 25, rarity: 3),
            // 🌶️  babiri (Spicy)  35 - Flavors [ 25,  10,   0,   0,   0] Rarity:  7
            new Berry(new BerryId(3), spicy: 25, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 35, rarity: 7),
            // 🍋 belue  (Sour)   35 - Flavors [ 10,   0,   0,   0,  30] Rarity:  7
            new Berry(new BerryId(4), spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 30, smoothness: 35, rarity: 7),
            // 🍇 bluk   (Dry)    20 - Flavors [  0,  10,  10,   0,   0] Rarity:  1
            new Berry(new BerryId(5), spicy: 0, dry: 10, sweet: 10, bitter: 0, sour: 0, smoothness: 20, rarity: 1),
            // 🍇 charti (Dry)    35 - Flavors [ 10,  20,   0,   0,   0] Rarity:  7
            new Berry(new BerryId(6), spicy: 10, dry: 20, sweet: 0, bitter: 0, sour: 0, smoothness: 35, rarity: 7),
            // 🌶️  cheri  (Spicy)  25 - Flavors [ 10,   0,   0,   0,   0] Rarity:  3
            new Berry(new BerryId(7), spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 0, smoothness: 25, rarity: 3),
            // 🍇 chesto (Dry)    25 - Flavors [  0,  10,   0,   0,   0] Rarity:  3
            new Berry(new BerryId(8), spicy: 0, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 25, rarity: 3),
            // 🍇 chilan (Dry)    35 - Flavors [  0,  25,  10,   0,   0] Rarity:  7
            new Berry(new BerryId(9), spicy: 0, dry: 25, sweet: 10, bitter: 0, sour: 0, smoothness: 35, rarity: 7),
            // 🌶️ chople (Spicy)  30 - Flavors [ 15,   0,   0,  10,   0] Rarity:  5
            new Berry(new BerryId(10), spicy: 15, dry: 0, sweet: 0, bitter: 10, sour: 0, smoothness: 30, rarity: 5),
            // 🍐 coba   (Bitter) 30 - Flavors [  0,  10,   0,  15,   0] Rarity:  5
            new Berry(new BerryId(11), spicy: 0, dry: 10, sweet: 0, bitter: 15, sour: 0, smoothness: 30, rarity: 5),
            // 🍋 colbur (Sour)   35 - Flavors [  0,   0,   0,  10,  20] Rarity:  7
            new Berry(new BerryId(12), spicy: 0, dry: 0, sweet: 0, bitter: 10, sour: 20, smoothness: 35, rarity: 7),
            // 🍇 cornn  (Dry)    30 - Flavors [  0,  20,  10,   0,   0] Rarity:  5
            new Berry(new BerryId(13), spicy: 0, dry: 20, sweet: 10, bitter: 0, sour: 0, smoothness: 30, rarity: 5),
            // 🍑 custap (Sweet)  60 - Flavors [  0,   0,  40,  10,   0] Rarity: 15
            new Berry(new BerryId(14), spicy: 0, dry: 0, sweet: 40, bitter: 10, sour: 0, smoothness: 60, rarity: 15),
            // 🍐 durin  (Bitter) 35 - Flavors [  0,   0,   0,  30,  10] Rarity:  7
            new Berry(new BerryId(15), spicy: 0, dry: 0, sweet: 0, bitter: 30, sour: 10, smoothness: 35, rarity: 7),
            // 🌶️ enigma (Spicy)  60 - Flavors [ 40,  10,   0,   0,   0] Rarity: 15
            new Berry(new BerryId(16), spicy: 40, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 60, rarity: 15),
            // 🌶️ figy   (Spicy)  25 - Flavors [ 15,   0,   0,   0,   0] Rarity:  3
            new Berry(new BerryId(17), spicy: 15, dry: 0, sweet: 0, bitter: 0, sour: 0, smoothness: 25, rarity: 3),
            // 🍇 ganlon (Dry)    40 - Flavors [  0,  30,  10,  30,   0] Rarity:  9
            new Berry(new BerryId(18), spicy: 0, dry: 30, sweet: 10, bitter: 30, sour: 0, smoothness: 40, rarity: 9),
            // 🍇 grepa  (Dry)    20 - Flavors [  0,  10,  10,   0,  10] Rarity:  1
            new Berry(new BerryId(19), spicy: 0, dry: 10, sweet: 10, bitter: 0, sour: 10, smoothness: 20, rarity: 1),
            // 🍐 haban  (Bitter) 35 - Flavors [  0,   0,  10,  20,   0] Rarity:  7
            new Berry(new BerryId(20), spicy: 0, dry: 0, sweet: 10, bitter: 20, sour: 0, smoothness: 35, rarity: 7),
            // 🌶️ hondew (Spicy)  20 - Flavors [ 10,  10,   0,  10,   0] Rarity:  1
            new Berry(new BerryId(21), spicy: 10, dry: 10, sweet: 0, bitter: 10, sour: 0, smoothness: 20, rarity: 1),
            // 🍋 iapapa (Sour)   25 - Flavors [  0,   0,   0,   0,  15] Rarity:  3
            new Berry(new BerryId(22), spicy: 0, dry: 0, sweet: 0, bitter: 0, sour: 15, smoothness: 25, rarity: 3),
            // 🍐 jaboca (Bitter) 60 - Flavors [  0,   0,   0,  40,  10] Rarity: 15
            new Berry(new BerryId(23), spicy: 0, dry: 0, sweet: 0, bitter: 40, sour: 10, smoothness: 60, rarity: 15),
            // 🍑 kasib  (Sweet)  35 - Flavors [  0,  10,  20,   0,   0] Rarity:  7
            new Berry(new BerryId(24), spicy: 0, dry: 10, sweet: 20, bitter: 0, sour: 0, smoothness: 35, rarity: 7),
            // 🍇 kebia  (Dry)    30 - Flavors [  0,  15,   0,   0,  10] Rarity:  5
            new Berry(new BerryId(25), spicy: 0, dry: 15, sweet: 0, bitter: 0, sour: 10, smoothness: 30, rarity: 5),
            // 🍇 kelpsy (Dry)    20 - Flavors [  0,  10,   0,  10,  10] Rarity:  1
            new Berry(new BerryId(26), spicy: 0, dry: 10, sweet: 0, bitter: 10, sour: 10, smoothness: 20, rarity: 1),
            // 🌶️ lansat (Spicy)  50 - Flavors [ 30,  10,  30,  10,  30] Rarity: 11
            new Berry(new BerryId(27), spicy: 30, dry: 10, sweet: 30, bitter: 10, sour: 30, smoothness: 50, rarity: 11),
            // 🌶️ leppa  (Spicy)  20 - Flavors [ 10,   0,  10,  10,  10] Rarity:  1
            new Berry(new BerryId(28), spicy: 10, dry: 0, sweet: 10, bitter: 10, sour: 10, smoothness: 20, rarity: 1),
            // 🌶️ liechi (Spicy)  40 - Flavors [ 30,  10,  30,   0,   0] Rarity:  9
            new Berry(new BerryId(29), spicy: 30, dry: 10, sweet: 30, bitter: 0, sour: 0, smoothness: 40, rarity: 9),
            // 🌶️ lum    (Spicy)  20 - Flavors [ 10,  10,  10,  10,   0] Rarity:  1
            new Berry(new BerryId(30), spicy: 10, dry: 10, sweet: 10, bitter: 10, sour: 0, smoothness: 20, rarity: 1),
            // 🍑 mago   (Sweet)  25 - Flavors [  0,   0,  15,   0,   0] Rarity:  3
            new Berry(new BerryId(31), spicy: 0, dry: 0, sweet: 15, bitter: 0, sour: 0, smoothness: 25, rarity: 3),
            // 🍑 magost (Sweet)  30 - Flavors [  0,   0,  20,  10,   0] Rarity:  5
            new Berry(new BerryId(32), spicy: 0, dry: 0, sweet: 20, bitter: 10, sour: 0, smoothness: 30, rarity: 5),
            // 🍇 micle  (Dry)    60 - Flavors [  0,  40,  10,   0,   0] Rarity: 15
            new Berry(new BerryId(33), spicy: 0, dry: 40, sweet: 10, bitter: 0, sour: 0, smoothness: 60, rarity: 15),
            // 🍑 nanab  (Sweet)  20 - Flavors [  0,   0,  10,  10,   0] Rarity:  1
            new Berry(new BerryId(34), spicy: 0, dry: 0, sweet: 10, bitter: 10, sour: 0, smoothness: 20, rarity: 1),
            // 🍋 nomel  (Sour)   30 - Flavors [ 10,   0,   0,   0,  20] Rarity:  5
            new Berry(new BerryId(35), spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 20, smoothness: 30, rarity: 5),
            // 🌶️ occa   (Spicy)  30 - Flavors [ 15,   0,  10,   0,   0] Rarity:  5
            new Berry(new BerryId(36), spicy: 15, dry: 0, sweet: 10, bitter: 0, sour: 0, smoothness: 30, rarity: 5),
            // 🌶️ oran   (Spicy)  20 - Flavors [ 10,  10,   0,  10,  10] Rarity:  1
            new Berry(new BerryId(37), spicy: 10, dry: 10, sweet: 0, bitter: 10, sour: 10, smoothness: 20, rarity: 1),
            // 🍇 pamtre (Dry)    35 - Flavors [  0,  30,  10,   0,   0] Rarity:  7
            new Berry(new BerryId(38), spicy: 0, dry: 30, sweet: 10, bitter: 0, sour: 0, smoothness: 35, rarity: 7),
            // 🍇 passho (Dry)    30 - Flavors [  0,  15,   0,  10,   0] Rarity:  5
            new Berry(new BerryId(39), spicy: 0, dry: 15, sweet: 0, bitter: 10, sour: 0, smoothness: 30, rarity: 5),
            // 🍋 payapa (Sour)   30 - Flavors [  0,   0,  10,   0,  15] Rarity:  5
            new Berry(new BerryId(40), spicy: 0, dry: 0, sweet: 10, bitter: 0, sour: 15, smoothness: 30, rarity: 5),
            // 🍑 pecha  (Sweet)  25 - Flavors [  0,   0,  10,   0,   0] Rarity:  3
            new Berry(new BerryId(41), spicy: 0, dry: 0, sweet: 10, bitter: 0, sour: 0, smoothness: 25, rarity: 3),
            // 🌶️ persim (Spicy)  20 - Flavors [ 10,  10,  10,   0,  10] Rarity:  1
            new Berry(new BerryId(42), spicy: 10, dry: 10, sweet: 10, bitter: 0, sour: 10, smoothness: 20, rarity: 1),
            // 🌶️ petaya (Spicy)  40 - Flavors [ 30,   0,   0,  30,  10] Rarity:  9
            new Berry(new BerryId(43), spicy: 30, dry: 0, sweet: 0, bitter: 30, sour: 10, smoothness: 40, rarity: 9),
            // 🌶️ pinap  (Spicy)  20 - Flavors [ 10,   0,   0,   0,  10] Rarity:  1
            new Berry(new BerryId(44), spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 10, smoothness: 20, rarity: 1),
            // 🌶️ pomeg  (Spicy)  20 - Flavors [ 10,   0,  10,  10,   0] Rarity:  1
            new Berry(new BerryId(45), spicy: 10, dry: 0, sweet: 10, bitter: 10, sour: 0, smoothness: 20, rarity: 1),
            // 🌶️ qualot (Spicy)  20 - Flavors [ 10,   0,  10,   0,  10] Rarity:  1
            new Berry(new BerryId(46), spicy: 10, dry: 0, sweet: 10, bitter: 0, sour: 10, smoothness: 20, rarity: 1),
            // 🍐 rabuta (Bitter) 30 - Flavors [  0,   0,   0,  20,  10] Rarity:  5
            new Berry(new BerryId(47), spicy: 0, dry: 0, sweet: 0, bitter: 20, sour: 10, smoothness: 30, rarity: 5),
            // 🍐 rawst  (Bitter) 25 - Flavors [  0,   0,   0,  10,   0] Rarity:  3
            new Berry(new BerryId(48), spicy: 0, dry: 0, sweet: 0, bitter: 10, sour: 0, smoothness: 25, rarity: 3),
            // 🌶️  razz   (Spicy)  20 - Flavors [ 10,  10,   0,   0,   0] Rarity:  1
            new Berry(new BerryId(49), spicy: 10, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 20, rarity: 1),
            // 🍐 rindo  (Bitter) 30 - Flavors [ 10,   0,   0,  15,   0] Rarity:  5
            new Berry(new BerryId(50), spicy: 10, dry: 0, sweet: 0, bitter: 15, sour: 0, smoothness: 30, rarity: 5),
            // 🍑 roseli (Sweet)  35 - Flavors [  0,   0,  25,  10,   0] Rarity:  7
            new Berry(new BerryId(51), spicy: 0, dry: 0, sweet: 25, bitter: 10, sour: 0, smoothness: 35, rarity: 7),
            // 🍋 rowap  (Sour)   60 - Flavors [ 10,   0,   0,   0,  40] Rarity: 15
            new Berry(new BerryId(52), spicy: 10, dry: 0, sweet: 0, bitter: 0, sour: 40, smoothness: 60, rarity: 15),
            // 🍑 salac  (Sweet)  40 - Flavors [  0,   0,  30,  10,  30] Rarity:  9
            new Berry(new BerryId(53), spicy: 0, dry: 0, sweet: 30, bitter: 10, sour: 30, smoothness: 40, rarity: 9),
            // 🍑 shuca  (Sweet)  30 - Flavors [ 10,   0,  15,   0,   0] Rarity:  5
            new Berry(new BerryId(54), spicy: 10, dry: 0, sweet: 15, bitter: 0, sour: 0, smoothness: 30, rarity: 5),
            // 🍇 sitrus (Dry)    20 - Flavors [  0,  10,  10,  10,  10] Rarity:  1
            new Berry(new BerryId(55), spicy: 0, dry: 10, sweet: 10, bitter: 10, sour: 10, smoothness: 20, rarity: 1),
            // 🌶️ spelon (Spicy)  35 - Flavors [ 30,  10,   0,   0,   0] Rarity:  7
            new Berry(new BerryId(56), spicy: 30, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 35, rarity: 7),
            // 🌶️ starf  (Spicy)  50 - Flavors [ 30,  10,  30,  10,  30] Rarity: 11
            new Berry(new BerryId(57), spicy: 30, dry: 10, sweet: 30, bitter: 10, sour: 30, smoothness: 50, rarity: 11),
            // 🌶️ tamato (Spicy)  30 - Flavors [ 20,  10,   0,   0,   0] Rarity:  5
            new Berry(new BerryId(58), spicy: 20, dry: 10, sweet: 0, bitter: 0, sour: 0, smoothness: 30, rarity: 5),
            // 🌶️ tanga  (Spicy)  35 - Flavors [ 20,   0,   0,   0,  10] Rarity:  7
            new Berry(new BerryId(59), spicy: 20, dry: 0, sweet: 0, bitter: 0, sour: 10, smoothness: 35, rarity: 7),
            // 🍑 wacan  (Sweet)  30 - Flavors [  0,   0,  15,   0,  10] Rarity:  5
            new Berry(new BerryId(60), spicy: 0, dry: 0, sweet: 15, bitter: 0, sour: 10, smoothness: 30, rarity: 5),
            // 🍑 watmel (Sweet)  35 - Flavors [  0,   0,  30,  10,   0] Rarity:  7
            new Berry(new BerryId(61), spicy: 0, dry: 0, sweet: 30, bitter: 10, sour: 0, smoothness: 35, rarity: 7),
            // 🍐 wepear (Bitter) 20 - Flavors [  0,   0,   0,  10,  10] Rarity:  1
            new Berry(new BerryId(62), spicy: 0, dry: 0, sweet: 0, bitter: 10, sour: 10, smoothness: 20, rarity: 1),
            // 🍇 wiki   (Dry)    25 - Flavors [  0,  15,   0,   0,   0] Rarity:  3
            new Berry(new BerryId(63), spicy: 0, dry: 15, sweet: 0, bitter: 0, sour: 0, smoothness: 25, rarity: 3),
            // 🍋 yache  (Sour)   30 - Flavors [  0,  10,   0,   0,  15] Rarity:  5
            new Berry(new BerryId(64), spicy: 0, dry: 10, sweet: 0, bitter: 0, sour: 15, smoothness: 30, rarity: 5),
        ];

        /// <summary>
        /// Returns a read-only span of all berries in ID order.
        /// <include file='BerryDocs.xml' path='docs/members/member[@name="T:FullBerryTable"]/*' />
        /// </summary>
        /// <param name="id">The berry identifier.</param>
        /// <returns>The corresponding <see cref="Berry"/>.</returns>
        public static ReadOnlySpan<Berry> All => _berries;

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
            return ref _berries[id.Value];
        }
    }
}
