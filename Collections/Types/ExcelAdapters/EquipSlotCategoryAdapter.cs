namespace Collections;

[Sheet("EquipSlotCategory")]
public struct EquipSlotCategoryAdapter : IExcelRow<EquipSlotCategoryAdapter>
{
    public EquipSlotCategory EquipSlotCategory { get; set; }
    public uint RowId => EquipSlotCategory.RowId;
    public EquipSlot EquipSlot { get; set; }

    public static EquipSlotCategoryAdapter Create(ExcelPage page, uint offset, uint row) => new(page, offset, row);

    public EquipSlotCategoryAdapter(ExcelPage page, uint offset, uint row)
    {
        EquipSlotCategory = new EquipSlotCategory(page, offset, row);
        EquipSlot = GetEquipSlot();
    }

    private EquipSlot GetEquipSlot()
    {
        if (EquipSlotCategory.MainHand != 0)
            return EquipSlot.MainHand;
        if (EquipSlotCategory.OffHand != 0)
            return EquipSlot.OffHand;
        if (EquipSlotCategory.Head != 0)
            return EquipSlot.Head;
        if (EquipSlotCategory.Body != 0)
            return EquipSlot.Body;
        if (EquipSlotCategory.Gloves != 0)
            return EquipSlot.Gloves;
        if (EquipSlotCategory.Legs != 0)
            return EquipSlot.Legs;
        if (EquipSlotCategory.Feet != 0)
            return EquipSlot.Feet;
        return EquipSlot.None;
    }
}
