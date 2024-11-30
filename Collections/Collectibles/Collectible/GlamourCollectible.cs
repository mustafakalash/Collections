namespace Collections;

public class GlamourCollectible : Collectible<ItemAdapter>, ICreateable<GlamourCollectible, ItemAdapter>
{
    public new static string CollectionName => "Glamour";

    public GlamourCollectible(ItemAdapter excelRow) : base(excelRow)
    {
    }

    public static GlamourCollectible Create(ItemAdapter excelRow)
    {
        return new(excelRow);
    }

    protected override string GetCollectionName()
    {
        return CollectionName;
    }

    protected override string GetName()
    {
        return ExcelRow.Item.Name.ToString();
    }

    protected override uint GetId()
    {
        return ExcelRow.RowId;
    }

    protected override string GetDescription()
    {
        return "";
    }

    protected override HintModule GetPrimaryHint()
    {
        return new HintModule($"Lv. {ExcelRow.Item.LevelEquip}", null);
    }

    protected override HintModule GetSecondaryHint()
    {
        return new HintModule($"{ExcelRow.Item.ClassJobCategory.Value.Name.ToString()}", null);
    }

    public override void UpdateObtainedState()
    {
        isObtained = Services.ItemFinder.IsItemInInventory(ExcelRow.RowId)
                    || Services.ItemFinder.IsItemInArmoireCache(ExcelRow.RowId)
                    || Services.ItemFinder.IsItemInDresser(ExcelRow.RowId);
    }

    protected override int GetIconId()
    {
        return ExcelRow.Item.Icon;
    }

    public override void Interact()
    {
        // Do nothing - taken care of by event service (should probably unify this in some way)
    }
}
