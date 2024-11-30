namespace Collections;

public class ShopsDataGenerator : BaseDataGenerator<Shop>
{
    private Dictionary<uint, uint> NpcDataToNpcBase { get; set; } = new();

    private ExcelCache<ItemAdapter> ItemSheet { get; set; }
    private ExcelCache<ENpcBase> ENpcBaseSheet { get; set; }
    private ExcelCache<SpecialShopAdapter> SpecialShopEntitySheet { get; set; }
    private ExcelCache<CustomTalk> CustomTalkSheet { get; set; }
    private SubrowExcelCache<CustomTalkNestHandlers> CustomTalkNestHandlersSheet { get; set; }
    private ExcelCache<InclusionShop> InclusionShopSheet { get; set; }
    private SubrowExcelCache<InclusionShopSeries> InclusionShopSeriesSheet { get; set; }
    private ExcelCache<TopicSelect> TopicSelectSheet { get; set; }
    private ExcelCache<PreHandler> PreHandlerSheet { get; set; }

    protected override void InitializeData()
    {
        //Dev.Start();
        ItemSheet = ExcelCache<ItemAdapter>.GetSheet();
        ENpcBaseSheet = ExcelCache<ENpcBase>.GetSheet();
        SpecialShopEntitySheet = ExcelCache<SpecialShopAdapter>.GetSheet();
        CustomTalkSheet = ExcelCache<CustomTalk>.GetSheet();
        CustomTalkNestHandlersSheet = SubrowExcelCache<CustomTalkNestHandlers>.GetSheet();
        InclusionShopSheet = ExcelCache<InclusionShop>.GetSheet();
        InclusionShopSeriesSheet = SubrowExcelCache<InclusionShopSeries>.GetSheet();
        TopicSelectSheet = ExcelCache<TopicSelect>.GetSheet();
        PreHandlerSheet = ExcelCache<PreHandler>.GetSheet();

        PopulateNpcDataToNpcBase();
        PopulateGilShop();
        PopulateGCShop();
        PopulateSpecialShop();
        //Dev.Stop();
    }

    internal enum EventHandlerType : uint
    {
        GilShop = 0x0004,
        CustomTalk = 0x000B,
        GcShop = 0x0016,
        SpecialShop = 0x001B,
        FcShop = 0x002A,
    }

    private void PopulateNpcDataToNpcBase()
    {
        //Dev.Start();

        var FirstSpecialShopId = SpecialShopEntitySheet.First().RowId;
        var LastSpecialShopId = SpecialShopEntitySheet.Last().RowId;

        foreach (var ENpcBase in ENpcBaseSheet)
        {
            foreach (var ENpcData in ENpcBase.ENpcData)
            {
                NpcDataToNpcBase[ENpcData.RowId] = ENpcBase.RowId;

                var npcData = ENpcData.RowId;
                if (npcData == 0)
                {
                    continue;
                }

                // CustomTalk
                if (MatchEventHandlerType(npcData, EventHandlerType.CustomTalk))
                {
                    var customTalkNullable = CustomTalkSheet.GetRow(npcData);
                    if (!customTalkNullable.HasValue)
                    {
                        continue;
                    }
                    var customTalk = customTalkNullable.Value;

                    // CustomTalk - SpecialLinks
                    if (customTalk.SpecialLinks.RowId != 0)
                    {
                        try
                        {
                            for (ushort index = 0; index <= 30; index++)
                            {
                                var customTalkNestHandler = CustomTalkNestHandlersSheet.GetRow(customTalk.SpecialLinks.RowId, index);
                                if (customTalkNestHandler != null)
                                {
                                    var specialShop = SpecialShopEntitySheet.GetRow(customTalkNestHandler.Value.NestHandler.RowId);
                                    if (specialShop.HasValue)
                                    {
                                        NpcDataToNpcBase[specialShop.Value.RowId] = ENpcBase.RowId;
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    // CustomTalk - ScriptArg
                    foreach (var arg in customTalk.Script)
                    {
                        if (arg.ScriptArg < FirstSpecialShopId || arg.ScriptArg > LastSpecialShopId)
                        {
                            continue;
                        }
                        var specialShop = SpecialShopEntitySheet.GetRow(arg.ScriptArg);
                        if (specialShop != null)
                        {
                            NpcDataToNpcBase[specialShop.Value.RowId] = ENpcBase.RowId;
                        }
                    }
                }

                // InclusionShops
                var inclusionShop = InclusionShopSheet.GetRow(npcData).Value;
                addInclusionShop(inclusionShop, ENpcBase.RowId);

                // PreHandler
                var preHandler = PreHandlerSheet.GetRow(npcData).Value;
                addPreHandler(preHandler, ENpcBase.RowId);

                // TopicSelect
                var topicSelectNullable = TopicSelectSheet.GetRow(npcData);
                if (topicSelectNullable.HasValue)
                {
                    var topicSelect = topicSelectNullable.Value;
                    foreach (var data in topicSelect.Shop)
                    {
                        if (data.RowId == 0)
                        {
                            continue;
                        }

                        if (MatchEventHandlerType(data.RowId, EventHandlerType.SpecialShop))
                        {
                            var specialShop = SpecialShopEntitySheet.GetRow(data.RowId);
                            if (specialShop.HasValue)
                            {
                                NpcDataToNpcBase[specialShop.Value.RowId] = ENpcBase.RowId;
                            }
                            continue;
                        }

                        // TopicSelect -> PreHandler
                        preHandler = PreHandlerSheet.GetRow(data.RowId).Value;
                        addPreHandler(preHandler, ENpcBase.RowId);
                    }
                }
            }
        }

        // Inject manual data
        foreach (var (NpcBaseId, gilShopRowIds) in DataOverrides.GilShopToNpcBase)
        {
            foreach (var gilShopRowId in gilShopRowIds)
            {
                NpcDataToNpcBase.TryAdd(gilShopRowId, NpcBaseId);
            }
        }

        foreach (var (specialShopRowId, NpcBaseId) in DataOverrides.SpecialShopToNpcBase)
        {
            NpcDataToNpcBase.TryAdd(specialShopRowId, NpcBaseId);
        }
        //Dev.Stop();
    }

    private uint? GetNpcBaseFromNpcData(uint npcDataId)
    {
        if (NpcDataToNpcBase.ContainsKey(npcDataId))
        {
            // Map Journeyman Salvager to Calamity Salvager (locateable)
            var npcBaseId = NpcDataToNpcBase[npcDataId];
            if (npcBaseId == 1025924)
                return 1006004;

            return npcBaseId;
        }
        return null;
    }

    private static bool MatchEventHandlerType(uint data, EventHandlerType type)
    {
        return ((data >> 16) & (uint)type) == (uint)type;
    }

    private void addPreHandler(PreHandler? preHandler, uint ENpcBaseId)
    {
        if (!preHandler.HasValue)
        {
            return;
        }

        var target = preHandler.Value.Target.RowId;
        if (target == 0)
        {
            return;
        }

        if (MatchEventHandlerType(target, EventHandlerType.SpecialShop))
        {
            var specialShop = SpecialShopEntitySheet.GetRow(target);
            if (specialShop.HasValue)
            {
                NpcDataToNpcBase[specialShop.Value.RowId] = ENpcBaseId;
            }
            return;
        }

        var inclusionShop = InclusionShopSheet.GetRow(target);
        addInclusionShop(inclusionShop, ENpcBaseId);
    }

    private void addInclusionShop(InclusionShop? inclusionShop, uint ENpcBaseId)
    {
        if (!inclusionShop.HasValue)
        {
            return;
        }

        foreach (var category in inclusionShop.Value.Category)
        {
            if (category.Value.RowId == 0)
            {
                continue;
            }

            for (ushort i = 0; ; i++)
            {
                try
                {
                    var series = InclusionShopSeriesSheet.GetRow(category.Value.InclusionShopSeries.RowId, i);
                    if (!series.HasValue)
                    {
                        break;
                    }

                    var specialShop = SpecialShopEntitySheet.GetRow(series.Value.SpecialShop.RowId);
                    if (specialShop.HasValue)
                    {
                        NpcDataToNpcBase[specialShop.Value.RowId] = ENpcBaseId;
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }
    }

    private void PopulateGilShop()
    {
        //Dev.Start();
        var gilShopSheet = ExcelCache<GilShop>.GetSheet()!;
        var gilShopItemSheet = SubrowExcelCache<GilShopItem>.GetSheet()!;
        var gilItem = ItemSheet.GetRow((uint)Currency.Gil);
        foreach (var gilShop in gilShopSheet)
        {
            if (DataOverrides.IgnoreGilShopId.Contains(gilShop.RowId))
            {
                continue;
            }

            for (ushort i = 0; i < 100; i++)
            {
                try
                {
                    var gilShopItem = gilShopItemSheet.GetRow(gilShop.RowId, i);
                    var item = gilShopItem?.Item.Value;
                    if (item == null)
                    {
                        break;
                    }
                    
                    var ENpcDataId = gilShop.RowId;
                    uint? ENpcBaseId = null;
                    if (NpcDataToNpcBase.ContainsKey(ENpcDataId))
                    {
                        ENpcBaseId = GetNpcBaseFromNpcData(ENpcDataId);
                    }
                    var costList = new List<(ItemAdapter Item, int Amount)> { (gilItem.Value, (int)item.Value.PriceMid) };
                    var shopEntry = new Shop(costList, ENpcBaseId, gilShop.RowId);
                    AddEntry(item.Value.RowId, shopEntry);
                }
                catch
                {
                    continue;
                }
            }
        }
        //Dev.Stop();
    }

    private void PopulateGCShop()
    {
        //Dev.Start();
        var GCShopSheet = ExcelCache<GCShop>.GetSheet()!; // 4 rows
        var GCScripShopCategorySheet = ExcelCache<GCScripShopCategory>.GetSheet()!; // 31 rows
        var GCScripShopItemSheet = SubrowExcelCache<GCScripShopItem>.GetSheet()!; // 36.x
        var GCItem = ItemSheet.GetRow((uint)Currency.CompanySeals);

        foreach (var gcShop in GCShopSheet)
        {
            var gcShopCategories = GCScripShopCategorySheet.Where(i => i.GrandCompany.RowId == gcShop.GrandCompany.RowId).ToList();
            if (gcShopCategories.Count == 0)
            {
                return;
            }

            foreach (var category in gcShopCategories)
            {
                for (ushort i = 0; ; i++)
                {
                    try
                    {
                        var GCScripShopItem = GCScripShopItemSheet.GetRow(category.RowId, i);
                        if (GCScripShopItem == null)
                        {
                            break;
                        }

                        var item = GCScripShopItem.Value.Item.ValueNullable;
                        if (!item.HasValue)
                        {
                            break;
                        }

                        var ENpcDataId = gcShop.RowId;
                        var ENpcBaseId = GetNpcBaseFromNpcData(ENpcDataId);
                        var costList = new List<(ItemAdapter Item, int Amount)> { (GCItem.Value, (int)item.Value.PriceMid) };
                        var shopEntry = new Shop(costList, ENpcBaseId, gcShop.RowId);
                        AddEntry(item.Value.RowId, shopEntry);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
        }
        //Dev.Stop();
    }

    private void PopulateSpecialShop()
    {
        //Dev.Start();

        foreach (var specialShop in SpecialShopEntitySheet)
        {
            if (DataOverrides.IgnoreSpecialShopId.Contains(specialShop.RowId))
            {
                continue;
            }

            uint? ENpcBaseId = null;
            if (NpcDataToNpcBase.ContainsKey(specialShop.RowId))
            {
                ENpcBaseId = GetNpcBaseFromNpcData(specialShop.RowId);
            }

            foreach (var entry in specialShop.Entries)
            {
                var costList = new List<(ItemAdapter Item, int Amount)>();

                foreach (var cost in entry.Cost)
                {
                    if (cost.Item.RowId == 0)
                    {
                        continue;
                    }

                    costList.Add((cost.Item.Value, (int)cost.Count));
                }
                foreach (var result in entry.Result)
                {
                    var itemId = result.Item.Value.RowId;
                    if (itemId == 0)
                    {
                        break;
                    }

                    var shopEntry = new Shop(costList, ENpcBaseId, specialShop.RowId);
                    AddEntry(itemId, shopEntry);
                }
            }
        }
        //Dev.Stop();
    }
}
