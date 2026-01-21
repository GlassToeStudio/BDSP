namespace BDSP.Core.Berries
{
    public readonly struct BerrySortSpec
    {
        public BerrySortField Field { get; }
        public SortDirection Direction { get; }

        public BerrySortSpec(BerrySortField field, SortDirection direction)
        {
            Field = field;
            Direction = direction;
        }
    }
}