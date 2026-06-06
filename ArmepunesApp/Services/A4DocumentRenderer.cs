using System.Text.Json;
using System.Drawing.Printing;

namespace ArmepunesApp.Services;

public class A4DocumentRenderer : IDisposable
{
    public const int A4_W = 827;
    public const int A4_H = 1169;

    public Graphics G { get; }
    public float M { get; }
    public float X0 { get; }
    public float Y { get; set; }
    public float W { get; }
    public float PageH { get; }

    public Font Title { get; }
    public Font Sub { get; }
    public Font Section { get; }
    public Font Normal { get; }
    public Font Label { get; }
    public Font Small { get; }
    public Font Footer { get; }

    public Pen BorderPen { get; }
    public Pen LightPen { get; }
    public SolidBrush SectionBg { get; }
    public SolidBrush HeaderBg { get; }
    public SolidBrush TextBrush { get; }

    public float LH { get; set; } = 20;

    // Template texts
    public string TmpHeaderTitle { get; } = "POLIGONI DRENI";
    public string TmpHeaderSub { get; } = "Qendra e Deponimit dhe Menaxhimit te Armeve";
    public string TmpHeaderAddr { get; } = "Prishtine, Republika e Kosoves";
    public string TmpSig1 { get; } = "Pergjegjesi i Depos";
    public string TmpSig2 { get; } = "Personeli Pranues";
    public string TmpSig3 { get; } = "Drejtuesi";
    public string TmpFooter { get; } = "Dokument i gjeneruar nga Sistemi Deponim i Armeve";

    public A4DocumentRenderer(Graphics g, Rectangle page, float margin = 20)
        : this(g, page, margin, null) { }

    public A4DocumentRenderer(Graphics g, Rectangle page, float margin, string? templateJson)
    {
        G = g;
        M = margin;
        X0 = margin;
        Y = margin;
        W = page.Width - margin * 2;
        PageH = page.Height;

        int fontTitle = 18, fontSection = 12, fontNormal = 10;
        var headerColor = Color.FromArgb(0, 60, 110);
        var textColor = Color.Black;

        if (!string.IsNullOrEmpty(templateJson))
        {
            try
            {
                var t = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(templateJson);
                if (t != null)
                {
                    if (t.TryGetValue("fontTitle", out var ft)) fontTitle = ft.GetInt32();
                    if (t.TryGetValue("fontSection", out var fs)) fontSection = fs.GetInt32();
                    if (t.TryGetValue("fontNormal", out var fn)) fontNormal = fn.GetInt32();
                    if (t.TryGetValue("headerColor", out var hc)) headerColor = Color.FromArgb(hc.GetInt32());
                    if (t.TryGetValue("textColor", out var tc)) textColor = Color.FromArgb(tc.GetInt32());
                    if (t.TryGetValue("txtHeaderTitle", out var v)) TmpHeaderTitle = v.GetString() ?? TmpHeaderTitle;
                    if (t.TryGetValue("txtHeaderSub", out v)) TmpHeaderSub = v.GetString() ?? TmpHeaderSub;
                    if (t.TryGetValue("txtHeaderAddr", out v)) TmpHeaderAddr = v.GetString() ?? TmpHeaderAddr;
                    if (t.TryGetValue("txtSig1", out v)) TmpSig1 = v.GetString() ?? TmpSig1;
                    if (t.TryGetValue("txtSig2", out v)) TmpSig2 = v.GetString() ?? TmpSig2;
                    if (t.TryGetValue("txtSig3", out v)) TmpSig3 = v.GetString() ?? TmpSig3;
                    if (t.TryGetValue("txtFooter", out v)) TmpFooter = v.GetString() ?? TmpFooter;
                }
            }
            catch { }
        }

        Title = new Font("Segoe UI", fontTitle, FontStyle.Bold);
        Sub = new Font("Segoe UI", fontSection, FontStyle.Bold);
        Section = new Font("Segoe UI", fontSection, FontStyle.Bold);
        Label = new Font("Segoe UI", fontNormal, FontStyle.Bold);
        Normal = new Font("Segoe UI", fontNormal);
        Small = new Font("Segoe UI", fontNormal - 1);
        Footer = new Font("Segoe UI", fontNormal - 2);

        BorderPen = new Pen(headerColor, 2);
        LightPen = new Pen(Color.FromArgb(180, 190, 200), 0.5f);
        SectionBg = new SolidBrush(headerColor);
        HeaderBg = new SolidBrush(Color.FromArgb(
            Math.Max(0, headerColor.R - 20),
            Math.Max(0, headerColor.G - 20),
            Math.Max(0, headerColor.B - 20)));
        TextBrush = new SolidBrush(textColor);
    }

    public void DrawSection(string title)
    {
        G.FillRectangle(SectionBg, X0, Y, W, LH + 4);
        G.DrawString(title, Section, Brushes.White, X0 + 8, Y + 2);
        Y += LH + 6;
    }

    public void DrawField(string label, string? value, float colX)
    {
        G.DrawString(label, Label, TextBrush, colX, Y);
        G.DrawString(value ?? "-", Normal, TextBrush, colX + 150, Y);
    }

    public void DrawFieldAt(string label, string? value, float colX, float colX2)
    {
        G.DrawString(label, Label, TextBrush, colX, Y);
        G.DrawString(value ?? "-", Normal, TextBrush, colX + 120, Y);
    }

    public void DrawHeader(string docTitle = "")
    {
        G.DrawString(TmpHeaderTitle, Title, TextBrush, X0 + 5, Y);
        Y += 28;
        G.DrawString(TmpHeaderSub, Sub, TextBrush, X0 + 5, Y);
        Y += 16;
        G.DrawLine(BorderPen, X0, Y, X0 + W, Y);
        Y += 10;
        if (!string.IsNullOrEmpty(docTitle))
        {
            G.DrawString(docTitle, Sub, TextBrush, X0 + 5, Y);
            Y += 24;
        }
    }

    public void DrawSignatureBlock(string[] labels, string[] names, string[] subs)
    {
        float sbw = (W - 60) / 3;
        float[] sx = { X0 + 10, X0 + sbw + 30, X0 + sbw * 2 + 50 };
        Y += 4;
        for (int i = 0; i < 3; i++)
        {
            G.DrawRectangle(LightPen, sx[i], Y, sbw, 68);
            G.DrawString(labels[i], Section, TextBrush, sx[i] + 5, Y + 3);
            if (!string.IsNullOrEmpty(names[i]))
                G.DrawString(names[i], Label, TextBrush, sx[i] + 5, Y + 20);
            G.DrawString(subs[i], Small, Brushes.DimGray, sx[i] + 5, Y + 34);
            using var linePen = new Pen(Color.FromArgb(80, 80, 80), 1);
            G.DrawLine(linePen, sx[i] + 5, Y + 50, sx[i] + sbw - 5, Y + 50);
            G.DrawString("Nenshkrimi / Data", Small, Brushes.DimGray, sx[i] + 5, Y + 52);
        }
        Y += 78;
    }

    public void DrawFooter(string kopja, string? serial = null)
    {
        G.DrawLine(BorderPen, X0, Y, X0 + W, Y);
        Y += 6;
        G.DrawString($"Kjo kopje eshte per {kopja.ToLower()}", Small, Brushes.DimGray, X0 + 5, Y);
        Y += 14;
        if (!string.IsNullOrEmpty(serial))
        {
            G.DrawString($"Pranova per armen me serial: {serial}", Small, TextBrush, X0 + 5, Y);
            Y += 14;
        }
        G.DrawString($"{TmpFooter} - {DateTime.Now:dd.MM.yyyy HH:mm:ss}", Small, Brushes.DimGray, X0 + 5, Y);
        Y += 14;
    }

    public void DrawTable(string[] headers, float[] colW, int rowCount, Action<int, float[]> drawRow)
    {
        float tw = colW.Sum();
        float[] cx = new float[colW.Length];
        cx[0] = X0;
        for (int i = 1; i < colW.Length; i++)
            cx[i] = cx[i - 1] + colW[i - 1];

        G.FillRectangle(HeaderBg, X0, Y, tw, LH + 4);
        for (int i = 0; i < headers.Length; i++)
            G.DrawString(headers[i], Label, Brushes.White, cx[i] + 2, Y + 1);
        Y += LH + 4;

        float rh = LH + 2;
        for (int i = 0; i < rowCount; i++)
        {
            if (Y > PageH - 80) break;
            if (i % 2 == 1)
            {
                using var rowBrush = new SolidBrush(Color.FromArgb(235, 240, 248));
                G.FillRectangle(rowBrush, X0, Y, tw, rh);
            }
            drawRow(i, cx);
            Y += rh;
        }
    }

    public void DrawDocInfoBox(string docTitle, string nrDok, string data, string ora, string tipi)
    {
        bool isHyrje = tipi.Contains("HYRJE");
        using var bgBrush = new SolidBrush(Color.FromArgb(240, 243, 248));
        G.FillRectangle(bgBrush, X0, Y, W, 46);
        G.DrawRectangle(LightPen, X0, Y, W, 46);

        using var brType = new SolidBrush(isHyrje ? Color.FromArgb(0, 120, 60) : Color.FromArgb(180, 60, 0));
        G.FillRectangle(brType, X0 + W - 140, Y + 4, 130, 38);
        G.DrawString(tipi ?? "", Section, Brushes.White, X0 + W - 130, Y + 10);

        using var docFont = new Font("Segoe UI", 14, FontStyle.Bold);
        G.DrawString($"{docTitle} Nr. {nrDok}", docFont, TextBrush, X0 + 10, Y + 4);
        G.DrawString($"Data: {data}    Ora: {ora}", Normal, TextBrush, X0 + 10, Y + 26);
        Y += 54;
    }

    public void Dispose()
    {
        Title.Dispose(); Sub.Dispose(); Section.Dispose();
        Label.Dispose(); Normal.Dispose(); Small.Dispose(); Footer.Dispose();
        BorderPen.Dispose(); LightPen.Dispose();
        SectionBg.Dispose(); HeaderBg.Dispose(); TextBrush.Dispose();
    }
}
