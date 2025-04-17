namespace DeviceTreeNode.Models
{
    public class CellSizes
    {
        public int AddressCells { get; }
        public int SizeCells { get; }

        public CellSizes(int addressCells, int sizeCells)
        {
            // 如果未指定，使用默认值
            AddressCells = addressCells <= 0 ? 2 : addressCells;
            SizeCells = sizeCells < 0 ? 1 : sizeCells;
        }

        // 默认单元大小
        public static readonly CellSizes Default = new CellSizes(2, 1);
    }
}
