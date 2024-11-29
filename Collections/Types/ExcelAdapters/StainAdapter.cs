namespace Collections;

[Sheet("Stain")]
public struct StainAdapter : IExcelRow<StainAdapter>
{
    public Stain Stain { get; set; }
    public uint RowId => Stain.RowId;
    public string HEXcolor { get; set; }
    public RGBColor RGBcolor { get; set; }
    public Vector4 VecColor { get; set; }
    public StainAdapter(ExcelPage page, uint offset, uint row)
    {
        Stain = new Stain(page, offset, row);

        if (Stain.Color != 0)
        {
            HEXcolor = StainColorConverter.DecimalToHex((int)Stain.Color);
            RGBcolor = StainColorConverter.HexToRGB(HEXcolor);
            VecColor = new Vector4(RGBcolor.R / 255f, RGBcolor.G / 255f, RGBcolor.B / 255f, 1);
        }
    }

    public static StainAdapter Create(ExcelPage page, uint offset, uint row) => new(page, offset, row);
}
