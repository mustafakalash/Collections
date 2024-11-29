namespace Collections;

[Sheet("Item")]
public struct ItemAdapter : IExcelRow<ItemAdapter>
{
    public Item Item { get; set; }
    public uint RowId => Item.RowId;
    public List<Job> Jobs { get; set; }
    public EquipSlot EquipSlot { get; set; }
    public bool IsEquipment { get; set; }

    public static ItemAdapter Create(ExcelPage page, uint offset, uint row) => new(page, offset, row);

    public ItemAdapter(ExcelPage page, uint offset, uint row)
    {
        Item = new Item(page, offset, row);
        InitializeEquipSlot();
        InitializeJobs();
    }

    public void InitializeEquipSlot()
    {
        var equipSlotCategory = ExcelCache<EquipSlotCategoryAdapter>.GetSheet().GetRow(Item.EquipSlotCategory.RowId).Value;
        EquipSlot = equipSlotCategory.EquipSlot;
        IsEquipment = EquipSlot != EquipSlot.None;
    }

    public void InitializeJobs()
    {
        if (IsEquipment)
        {
            var classJobCategory = ExcelCache<ClassJobCategoryAdapter>.GetSheet().GetRow(Item.ClassJobCategory.RowId).Value;
            Jobs = classJobCategory.Jobs;
        } else
        {
            Jobs = new List<Job>();
        }
    }
}
