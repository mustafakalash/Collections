using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace Collections;

[Sheet("SpecialShop")]
public struct SpecialShopAdapter : IExcelRow<SpecialShopAdapter>
{
    public SpecialShop SpecialShop { get; set; }
    public uint RowId => SpecialShop.RowId;
    public static SpecialShopAdapter Create(ExcelPage page, uint offset, uint row) => new SpecialShopAdapter(page, offset, row);
    public SpecialShopAdapter(ExcelPage page, uint offset, uint row)
    {
        SpecialShop = new SpecialShop(page, offset, row);
        for (var i = 0; i < SpecialShop.Item.Count; i++)
        {
            var item = SpecialShop.Item[i];
            var itemEntry = new Entry();
            itemEntry.Result = new List<ResultEntry>();
            itemEntry.Cost = new List<CostEntry>();
            for (var j = 0; j < item.ReceiveItems.Count; j++)
            {
                var receiveItem = item.ReceiveItems[j];
                itemEntry.Result.Add(new ResultEntry
                {
                    Item = new RowRef<ItemAdapter>(page.Module, receiveItem.Item.RowId, page.Language),
                    Count = receiveItem.ReceiveCount,
                    HQ = receiveItem.ReceiveHq
                });
            }
            for (var j = 0; j < item.ItemCosts.Count; j++)
            {
                var itemCost = item.ItemCosts[j];
                itemEntry.Cost.Add(new CostEntry
                {
                    Item = new RowRef<ItemAdapter>(page.Module, itemCost.ItemCost.RowId, page.Language),
                    Count = itemCost.CurrencyCost,
                    HQ = itemCost.HqCost > 0,
                    Collectability = itemCost.CollectabilityCost
                });
            }
            Entries.Add(itemEntry);
        }

    }
    public List<Entry> Entries = new();


    // Credit to https://github.com/Critical-Impact/CriticalCommonLib/blob/54b594f459ba1479a3bc67ad18ca65d206c63571/Sheets/SpecialShopListing.cs#L9
    // No idea why this is a thing...
    private int FixItemId(int itemId)
    {
        if (itemId == 0 || itemId > 7)
        {
            return itemId;
        }

        switch (SpecialShop.UseCurrencyType)
        {
            // Scrips
            case 16:
                switch (itemId)
                {
                    case 1: return 28;
                    case 2: return 25199;
                    case 4: return 25200;
                    case 6: return 33913;
                    case 7: return 33914;
                    default: return itemId;
                }
            // Gil
            case 8:
                return 1;
            case 4:
                var tomestones = BuildTomestones();
                return tomestones[itemId];
            default:
                return itemId;
                // Tomestones
                //case 4:
                //    if (TomeStones.ContainsKey(itemId))
                //    {
                //        return TomeStones[itemId];
                //    }

                //    return itemId;
        }
    }

    private static Dictionary<int, int> BuildTomestones()
    {
        var tomestoneItems = ExcelCache<TomestonesItem>.GetSheet()
            .Where(t => t.Tomestones.RowId > 0)
            .OrderBy(t => t.Tomestones.RowId)
            .ToArray();

        var tomeStones = new Dictionary<int, int>();

        for (var i = 0; i < tomestoneItems.Length; i++)
        {
            tomeStones[i + 1] = (int)tomestoneItems[i].Item.RowId;
        }

        return tomeStones;
    }

    public struct Entry
    {
        public List<ResultEntry> Result;
        public List<CostEntry> Cost;
    }

    public struct ResultEntry
    {
        public RowRef<ItemAdapter> Item;
        public uint Count;
        //public LazyRow<SpecialShopItemCategory> SpecialShopItemCategory;
        public bool HQ;
    }

    public struct CostEntry
    {
        public RowRef<ItemAdapter> Item;
        public uint Count;
        public bool HQ;
        public ushort Collectability;
    }
}
