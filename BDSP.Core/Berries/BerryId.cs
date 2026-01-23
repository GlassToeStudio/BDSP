namespace BDSP.Core.Berries
{
    public readonly struct BerryId
    {
        public readonly ushort Value;

        public BerryId(ushort value) => Value = value;

        public override string ToString() => Value.ToString();
    }
}
