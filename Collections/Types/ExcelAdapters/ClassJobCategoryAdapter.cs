namespace Collections;

[Sheet("ClassJobCategory")]
public struct ClassJobCategoryAdapter : IExcelRow<ClassJobCategoryAdapter>
{
    ClassJobCategory ClassJobCategory { get; set; }
    public uint RowId => ClassJobCategory.RowId;
    public List<Job> Jobs { get; set; }

    public static ClassJobCategoryAdapter Create(ExcelPage page, uint offset, uint row) => new(page, offset, row);

    public ClassJobCategoryAdapter(ExcelPage page, uint offset, uint row)
    {
        ClassJobCategory = new ClassJobCategory(page, offset, row);
        InitializeJobs();
    }

    private void InitializeJobs()
    {
        Jobs = new List<Job>();
        foreach (var job in GetEnumValues<Job>())
        {
            if (ClassJobCategory.GetProperty<bool>(job.GetEnumName()))
            {
                Jobs.Add(job);
            }
        }
    }
}
