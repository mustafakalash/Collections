using System.Collections;
using System.Collections.Concurrent;
using FFXIVClientStructs.FFXIV.Component.Excel;

namespace Collections;

public class ExcelCache<T> : IEnumerable<T> where T : struct, IExcelRow<T>
{
    private static ConcurrentDictionary<Dalamud.Game.ClientLanguage, ExcelCache<T>> InternalInstance = new();

    private ExcelSheet<T> excelSheet { get; set; }
    //private readonly ConcurrentDictionary<uint, T> rowCache = new();
    //private readonly ConcurrentDictionary<Tuple<uint, uint>, T> subRowCache = new();

    private ExcelCache(Dalamud.Game.ClientLanguage language)
    {
        excelSheet = Services.DataManager.GetExcelSheet<T>(language);
    }

    public static ExcelCache<T> GetSheet(Dalamud.Game.ClientLanguage? language = null)
    {
        var sheetLanguage = language is not null ? (Dalamud.Game.ClientLanguage)language : Services.DataManager.Language;
        if (InternalInstance.TryGetValue(sheetLanguage, out var instance))
        {
            return instance;
        }
        return InternalInstance[sheetLanguage] = new ExcelCache<T>(sheetLanguage);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return excelSheet.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public T? GetRow(uint id)
    {
        return excelSheet.GetRow(id);
        //if (rowCache.TryGetValue(id, out var value))
        //{
        //    return value;
        //}
        //if (excelSheet.GetRow(id) is not { } result) return null;

        //return rowCache[id] = result;
    }
}

public class SubrowExcelCache<T> : IEnumerable<SubrowCollection<T>> where T : struct, IExcelSubrow<T>
{

    private static ConcurrentDictionary<Dalamud.Game.ClientLanguage, SubrowExcelCache<T>> InternalInstance = new();

    private SubrowExcelSheet<T> excelSheet { get; set; }
    //private readonly ConcurrentDictionary<uint, T> rowCache = new();
    //private readonly ConcurrentDictionary<Tuple<uint, uint>, T> subRowCache = new();

    private SubrowExcelCache(Dalamud.Game.ClientLanguage language)
    {
        excelSheet = Services.DataManager.GetSubrowExcelSheet<T>(language);
    }

    public static SubrowExcelCache<T> GetSheet(Dalamud.Game.ClientLanguage? language = null)
    {
        var sheetLanguage = language is not null ? (Dalamud.Game.ClientLanguage)language : Services.DataManager.Language;
        if (InternalInstance.TryGetValue(sheetLanguage, out var instance))
        {
            return instance;
        }
        return InternalInstance[sheetLanguage] = new SubrowExcelCache<T>(sheetLanguage);
    }

    public IEnumerator<SubrowCollection<T>> GetEnumerator()
    {
        return excelSheet.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    public T? GetRow(uint row, ushort subRow)
    {
        return excelSheet.GetSubrow(row, subRow);
    }
}
