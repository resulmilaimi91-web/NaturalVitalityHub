using System.Data;
using System.Drawing.Printing;
using System.Text.Json;
using ArmepunesApp.Data;

namespace ArmepunesApp.Forms;

public partial class TemplateEditorForm : Form
{
    private readonly DatabaseHelper _db;
    private readonly DataRow? _existing;
    private readonly string _lloji;
    private readonly Panel pnlPreview;
    private readonly TextBox txtEmri;
    private readonly NumericUpDown nudKopje;
    private readonly Panel pnlNgjyraKrye, pnlNgjyraTekst;
    private readonly CheckedListBox lstSeksione;
    private readonly NumericUpDown _nudYOffset;
    private readonly ComboBox cmbFontTitle, cmbFontNormal, cmbFontSection;
    private readonly ComboBox cmbFormati;
    private readonly TextBox txtHeaderTitle, txtHeaderSub, txtHeaderAddr;
    private readonly TextBox txtSectionArmes, txtSectionKlient;
    private readonly TextBox txtSig1, txtSig2, txtSig3;
    private readonly TextBox txtFooterText;
    private readonly RadioButton rbPortrait, rbLandscape;
    private readonly NumericUpDown nudMarginTop, nudMarginBottom, nudMarginLeft, nudMarginRight;

    private Color _headerColor = Color.FromArgb(0, 70, 130);
    private Color _textColor = Color.Black;

    private static readonly (string Name, float W, float H)[] Formatet = {
        ("A4", 210f, 297f),
        ("A5", 148f, 210f),
        ("A3", 297f, 420f),
        ("Letter", 215.9f, 279.4f),
        ("Legal", 215.9f, 355.6f)
    };

    private static readonly string[] SeksioneDefault = {
        "1. Te dhenat e armes", "2. Zyrtari pranues", "3. Klienti / Pronesi",
        "4. Qellimi", "5. Aksesoret", "6. Municioni",
        "7. Pranim / Dorzim", "8. Shenime", "9. Nenshkrimet"
    };

    public TemplateEditorForm(DatabaseHelper db, string lloji, DataRow? existing)
    {
        _db = db;
        _lloji = lloji;
        _existing = existing;

        Text = existing != null ? "Ndrysho Template" : "Krijo Template te Ri";
        WindowState = FormWindowState.Maximized;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(30, 33, 40);
        Font = new Font("Segoe UI", 9);
        MinimizeBox = false;
        ShowIcon = false;
        ShowInTaskbar = false;

        int leftW, rightX, cw;
        leftW = 700;
        rightX = leftW + 24;
        cw = 600;

        // ── Left: Preview ──
        var lblPreview = new Label
        {
            Text = "PARAPAMJE (print preview i gjalle)",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 200, 255),
            Size = new Size(700, 24),
            Location = new Point(18, 12)
        };

        pnlPreview = new Panel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
            Size = new Size(leftW, 600),
            Location = new Point(18, 40),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        pnlPreview.Resize += (_, _) => pnlPreview.Invalidate();
        pnlPreview.Paint += VizatoPreview;

        // ── Right: Properties ──
        var propPanel = new Panel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left,
            Size = new Size(cw + 20, 600),
            Location = new Point(rightX, 40),
            AutoScroll = true,
            BackColor = Color.Transparent
        };

        int ry = 4;

        propPanel.Controls.Add(new Label
        {
            Text = "Emri i Template:",
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(200, 20),
            Location = new Point(0, ry)
        });
        txtEmri = new TextBox
        {
            Size = new Size(cw, 26),
            Location = new Point(0, ry + 20),
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10)
        };
        propPanel.Controls.Add(txtEmri);
        ry += 52;

        // ── Formati ──
        var gbFormati = new GroupBox
        {
            Text = " Formati i Letres ",
            ForeColor = Color.FromArgb(0, 200, 255),
            Size = new Size(cw, 80),
            Location = new Point(0, ry),
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        cmbFormati = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size = new Size(150, 24),
            Location = new Point(8, 22),
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9)
        };
        foreach (var f in Formatet) cmbFormati.Items.Add(f.Name);
        cmbFormati.SelectedIndex = 0;
        cmbFormati.SelectedIndexChanged += (_, _) => pnlPreview.Invalidate();

        rbPortrait = new RadioButton
        {
            Text = "Portrait (Vertikal)",
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(140, 20),
            Location = new Point(170, 22),
            Checked = true,
            Font = new Font("Segoe UI", 9)
        };
        rbPortrait.CheckedChanged += (_, _) => pnlPreview.Invalidate();
        rbLandscape = new RadioButton
        {
            Text = "Landscape (Horizontal)",
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(160, 20),
            Location = new Point(170, 46),
            Font = new Font("Segoe UI", 9)
        };
        rbLandscape.CheckedChanged += (_, _) => pnlPreview.Invalidate();
        gbFormati.Controls.AddRange(new Control[] { cmbFormati, rbPortrait, rbLandscape });
        propPanel.Controls.Add(gbFormati);
        ry += 86;

        // ── Margjinat ──
        var gbMargjina = new GroupBox
        {
            Text = " Margjinat (mm) ",
            ForeColor = Color.FromArgb(0, 200, 255),
            Size = new Size(cw, 72),
            Location = new Point(0, ry),
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };
        nudMarginTop = KrijoMarginNud(gbMargjina, "Sipër:", 10, 10);
        nudMarginBottom = KrijoMarginNud(gbMargjina, "Poshtë:", 150, 10);
        nudMarginLeft = KrijoMarginNud(gbMargjina, "Majtas:", 290, 15);
        nudMarginRight = KrijoMarginNud(gbMargjina, "Djathtas:", 430, 15);
        propPanel.Controls.Add(gbMargjina);
        ry += 78;

        // ── Rregullimi Vertikal ──
        propPanel.Controls.Add(new Label
        {
            Text = "Rregullimi Vertikal (Y-offset mm):",
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(220, 20),
            Location = new Point(0, ry)
        });
        propPanel.Controls.Add(new Label
        {
            Text = "vlera negative = lart, pozitive = posht\u00EB",
            ForeColor = Color.FromArgb(120, 130, 145),
            Size = new Size(350, 20),
            Location = new Point(220, ry),
            TextAlign = ContentAlignment.MiddleLeft
        });
        var nudYOffset = new NumericUpDown
        {
            Minimum = -30, Maximum = 30, Value = 0,
            DecimalPlaces = 1, Increment = 1m,
            Size = new Size(70, 24),
            Location = new Point(0, ry + 20),
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            BorderStyle = BorderStyle.FixedSingle
        };
        nudYOffset.ValueChanged += (_, _) => pnlPreview.Invalidate();
        propPanel.Controls.Add(nudYOffset);
        _nudYOffset = nudYOffset;
        ry += 48;

        // ── Kopje ──
        propPanel.Controls.Add(new Label
        {
            Text = "Numri i kopjeve:",
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(200, 20),
            Location = new Point(0, ry)
        });
        nudKopje = new NumericUpDown
        {
            Minimum = 1, Maximum = 5, Value = 3,
            Size = new Size(60, 24),
            Location = new Point(0, ry + 20),
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            BorderStyle = BorderStyle.FixedSingle
        };
        nudKopje.ValueChanged += (_, _) => pnlPreview.Invalidate();
        propPanel.Controls.Add(nudKopje);
        ry += 50;

        // ── Ngjyrat ──
        propPanel.Controls.Add(new Label
        {
            Text = "Ngjyra e header-it:",
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(200, 20),
            Location = new Point(0, ry)
        });
        pnlNgjyraKrye = new Panel
        {
            Size = new Size(40, 28),
            Location = new Point(0, ry + 20),
            BackColor = _headerColor,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        pnlNgjyraKrye.Click += (_, _) => ZgjedhNgjyren(ref _headerColor, pnlNgjyraKrye);
        propPanel.Controls.Add(pnlNgjyraKrye);
        propPanel.Controls.Add(new Label
        {
            Text = "Ngjyra e tekstit:",
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(200, 20),
            Location = new Point(160, ry)
        });
        pnlNgjyraTekst = new Panel
        {
            Size = new Size(40, 28),
            Location = new Point(160, ry + 20),
            BackColor = _textColor,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = Cursors.Hand
        };
        pnlNgjyraTekst.Click += (_, _) => ZgjedhNgjyren(ref _textColor, pnlNgjyraTekst);
        propPanel.Controls.Add(pnlNgjyraTekst);
        ry += 55;

        // ── Font ──
        propPanel.Controls.Add(new Label
        {
            Text = "Madhesia e fontit:",
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(200, 20),
            Location = new Point(0, ry)
        });
        ry += 20;
        var flpFont = new FlowLayoutPanel
        {
            Size = new Size(cw, 28),
            Location = new Point(0, ry),
            BackColor = Color.Transparent
        };
        cmbFontTitle = KrijoComboFont(10, 26, 18);
        cmbFontNormal = KrijoComboFont(8, 16, 10);
        cmbFontSection = KrijoComboFont(9, 18, 11);
        flpFont.Controls.AddRange(new Control[] {
            new Label { Text = "Titulli:", ForeColor = Color.FromArgb(200, 205, 216), Size = new Size(45, 24), TextAlign = ContentAlignment.MiddleLeft },
            cmbFontTitle,
            new Label { Text = " Normal:", ForeColor = Color.FromArgb(200, 205, 216), Size = new Size(52, 24), TextAlign = ContentAlignment.MiddleLeft },
            cmbFontNormal,
            new Label { Text = " Seksioni:", ForeColor = Color.FromArgb(200, 205, 216), Size = new Size(58, 24), TextAlign = ContentAlignment.MiddleLeft },
            cmbFontSection,
        });
        propPanel.Controls.Add(flpFont);
        ry += 34;

        // ── Tekstet ──
        var gbTekstet = new GroupBox
        {
            Text = " Tekstet ne Forme ",
            ForeColor = Color.FromArgb(0, 200, 255),
            Size = new Size(cw, 220),
            Location = new Point(0, ry),
            Font = new Font("Segoe UI", 9, FontStyle.Bold)
        };

        string[] txtLabels = { "Titulli kryesor:", "Nentitulli:", "Adresa:", "Seksioni Armet:", "Seksioni Klienti:", "Nenshkrimi 1:", "Nenshkrimi 2:", "Nenshkrimi 3:", "Footer:" };
        string[] txtDefs  = { "POLIGONI DRENI", "Qendra e Deponimit dhe Menaxhimit te Armeve", "Prishtine, Republika e Kosoves", "TE DHENAT E ARMIT", "KLIENTI / PRONESI", "Pergjegjesi i Depos", "Personeli Pranues", "Drejtuesi", "Dokument i gjeneruar nga Sistemi Deponim i Armeve" };
        var txtBoxes = new TextBox[9];
        for (int i = 0; i < 9; i++)
        {
            int col = i < 5 ? 0 : 300;
            int row = i < 5 ? i : i - 5;
            int yy = 20 + row * 38;
            gbTekstet.Controls.Add(new Label
            {
                Text = txtLabels[i],
                ForeColor = Color.FromArgb(200, 205, 216),
                Size = new Size(120, 20),
                Location = new Point(col + 2, yy),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8)
            });
            var tb = new TextBox
            {
                Text = txtDefs[i],
                Size = new Size(280, 22),
                Location = new Point(col + 2, yy + 16),
                BackColor = Color.FromArgb(40, 42, 48),
                ForeColor = Color.FromArgb(200, 205, 216),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 8)
            };
            tb.TextChanged += (_, _) => pnlPreview.Invalidate();
            gbTekstet.Controls.Add(tb);
            txtBoxes[i] = tb;
        }
        txtHeaderTitle = txtBoxes[0]; txtHeaderSub = txtBoxes[1]; txtHeaderAddr = txtBoxes[2];
        txtSectionArmes = txtBoxes[3]; txtSectionKlient = txtBoxes[4];
        txtSig1 = txtBoxes[5]; txtSig2 = txtBoxes[6]; txtSig3 = txtBoxes[7];
        txtFooterText = txtBoxes[8];
        propPanel.Controls.Add(gbTekstet);
        ry += 226;

        // ── Seksionet ──
        propPanel.Controls.Add(new Label
        {
            Text = "Seksionet (zgjidh cilat te shfaqen):",
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(400, 20),
            Location = new Point(0, ry)
        });
        ry += 22;
        lstSeksione = new CheckedListBox
        {
            Size = new Size(cw, 150),
            Location = new Point(0, ry),
            BackColor = Color.FromArgb(35, 37, 43),
            ForeColor = Color.FromArgb(200, 205, 216),
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 9),
            CheckOnClick = true
        };
        foreach (var s in SeksioneDefault) lstSeksione.Items.Add(s, true);
        propPanel.Controls.Add(lstSeksione);
        ry += 158;

        // ── Butonat e Print Preview ──
        var btnShikoPrint = new Button
        {
            Text = "🖨 Shiko Print (A4 real)",
            Size = new Size(240, 44),
            Location = new Point(0, ry + 6),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnShikoPrint.Click += (_, _) => ShfaqPrintPreview();
        propPanel.Controls.Add(btnShikoPrint);

        var btnRuaj = new Button
        {
            Text = "✓ Ruaj",
            Size = new Size(160, 44),
            Location = new Point(260, ry + 6),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnRuaj.Click += (_, _) => RuajTemplate();

        var btnAnulo = new Button
        {
            Text = "✕ Mbyll",
            Size = new Size(120, 44),
            Location = new Point(440, ry + 6),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            BackColor = Color.FromArgb(120, 130, 145),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnAnulo.Click += (_, _) => Close();
        propPanel.Controls.AddRange(new Control[] { btnRuaj, btnAnulo });

        Controls.AddRange(new Control[] { lblPreview, pnlPreview, propPanel });

        // Load existing
        if (existing != null)
        {
            txtEmri.Text = existing["Emri"]?.ToString() ?? "";
            var paramStr = existing["Parametrat"]?.ToString() ?? "{}";
            try
            {
                var json = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(paramStr);
                if (json != null)
                {
                    if (json.TryGetValue("kopje", out var k)) nudKopje.Value = k.GetInt32();
                    if (json.TryGetValue("headerColor", out var hc)) _headerColor = Color.FromArgb(hc.GetInt32());
                    if (json.TryGetValue("textColor", out var tc)) _textColor = Color.FromArgb(tc.GetInt32());
                    if (json.TryGetValue("fontTitle", out var ft)) cmbFontTitle.SelectedItem = ft.GetInt32();
                    if (json.TryGetValue("fontNormal", out var fn)) cmbFontNormal.SelectedItem = fn.GetInt32();
                    if (json.TryGetValue("fontSection", out var fs)) cmbFontSection.SelectedItem = fs.GetInt32();
                    if (json.TryGetValue("formati", out var fo))
                        for (int i = 0; i < Formatet.Length; i++)
                            if (Formatet[i].Name == fo.GetString()) { cmbFormati.SelectedIndex = i; break; }
                    if (json.TryGetValue("orientation", out var ori))
                        rbLandscape.Checked = ori.GetString() == "landscape";
                    if (json.TryGetValue("marginTop", out var mt)) nudMarginTop.Value = Math.Clamp((decimal)mt.GetDouble(), 0, 50);
                    if (json.TryGetValue("marginBottom", out var mb)) nudMarginBottom.Value = Math.Clamp((decimal)mb.GetDouble(), 0, 50);
                    if (json.TryGetValue("marginLeft", out var ml)) nudMarginLeft.Value = Math.Clamp((decimal)ml.GetDouble(), 0, 50);
                    if (json.TryGetValue("marginRight", out var mr)) nudMarginRight.Value = Math.Clamp((decimal)mr.GetDouble(), 0, 50);
                    if (json.TryGetValue("yOffset", out var yo)) _nudYOffset.Value = Math.Clamp((decimal)yo.GetDouble(), -30m, 30m);
                    if (json.TryGetValue("seksione", out var seks))
                    {
                        var seksStr = seks.GetString() ?? "";
                        for (int i = 0; i < lstSeksione.Items.Count; i++)
                            lstSeksione.SetItemChecked(i, seksStr.Contains(SeksioneDefault[i]));
                    }
                    var txtMap = new (string jsonKey, TextBox ctrl)[] {
                        ("txtHeaderTitle", txtHeaderTitle), ("txtHeaderSub", txtHeaderSub), ("txtHeaderAddr", txtHeaderAddr),
                        ("txtSectionArmes", txtSectionArmes), ("txtSectionKlient", txtSectionKlient),
                        ("txtSig1", txtSig1), ("txtSig2", txtSig2), ("txtSig3", txtSig3), ("txtFooter", txtFooterText)
                    };
                    foreach (var kv in txtMap)
                        if (json.TryGetValue(kv.jsonKey, out var jv)) kv.ctrl.Text = jv.GetString() ?? kv.ctrl.Text;
                    pnlNgjyraKrye.BackColor = _headerColor;
                    pnlNgjyraTekst.BackColor = _textColor;
                }
            }
            catch { }
        }

        txtEmri.TextChanged += (_, _) => pnlPreview.Invalidate();
        foreach (var nud in new[] { nudKopje, nudMarginTop, nudMarginBottom, nudMarginLeft, nudMarginRight })
            nud.ValueChanged += (_, _) => pnlPreview.Invalidate();
        lstSeksione.ItemCheck += (_, _) => pnlPreview.Invalidate();
    }

    private NumericUpDown KrijoMarginNud(GroupBox gb, string label, int x, int def)
    {
        gb.Controls.Add(new Label
        {
            Text = label,
            ForeColor = Color.FromArgb(200, 205, 216),
            Size = new Size(55, 20),
            Location = new Point(x, 6),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 8)
        });
        var nud = new NumericUpDown
        {
            Minimum = 0, Maximum = 50, Value = def,
            DecimalPlaces = 1, Increment = 0.5m,
            Size = new Size(65, 22),
            Location = new Point(x, 28),
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            BorderStyle = BorderStyle.FixedSingle
        };
        gb.Controls.Add(nud);
        return nud;
    }

    private ComboBox KrijoComboFont(int min, int max, int def)
    {
        var cmb = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Size = new Size(52, 22),
            BackColor = Color.FromArgb(40, 42, 48),
            ForeColor = Color.FromArgb(200, 205, 216),
            FlatStyle = FlatStyle.Flat
        };
        for (int i = min; i <= max; i++) cmb.Items.Add(i);
        cmb.SelectedItem = def;
        cmb.SelectedIndexChanged += (_, _) => pnlPreview.Invalidate();
        return cmb;
    }

    private void ZgjedhNgjyren(ref Color color, Panel panel)
    {
        using var dlg = new ColorDialog { Color = color, FullOpen = true };
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            color = dlg.Color;
            panel.BackColor = color;
            pnlPreview.Invalidate();
        }
    }

    // ── Preview në panel ──
    private void VizatoPreview(object? s, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(Color.White);
        var pw = pnlPreview.Width - 10f;
        var ph = pnlPreview.Height - 10f;

        int idx = cmbFormati.SelectedIndex;
        if (idx < 0) idx = 0;
        float fw = Formatet[idx].W / 25.4f * 100;
        float fh = Formatet[idx].H / 25.4f * 100;
        if (rbLandscape.Checked) { (fw, fh) = (fh, fw); }

        float scale = Math.Min(pw / fw, ph / fh);
        float pw2 = fw * scale;
        float ph2 = fh * scale;
        float ox = (pnlPreview.Width - pw2) / 2f;
        float oy = (pnlPreview.Height - ph2) / 2f;

        float mTop = (float)nudMarginTop.Value / 25.4f * 100 * scale;
        float mBot = (float)nudMarginBottom.Value / 25.4f * 100 * scale;
        float mLef = (float)nudMarginLeft.Value / 25.4f * 100 * scale;
        float mRig = (float)nudMarginRight.Value / 25.4f * 100 * scale;

        using var borderPen = new Pen(_headerColor, 2.5f);
        using var marginPen = new Pen(Color.FromArgb(160, 190, 220), 1f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
        using var shadeBrush = new SolidBrush(Color.FromArgb(18, 0, 120, 200));
        using var headerBrush = new SolidBrush(_headerColor);
        using var textBrush = new SolidBrush(_textColor);
        using var whiteBr = new SolidBrush(Color.White);

        float sf = scale * 0.55f;
        var titleFont = new Font("Segoe UI", Math.Max(8, GetFontSize(cmbFontTitle) * sf));
        var subFont = new Font("Segoe UI", Math.Max(7, GetFontSize(cmbFontSection) * sf));
        var normFont = new Font("Segoe UI", Math.Max(6, GetFontSize(cmbFontNormal) * sf));
        var smallFont = new Font("Segoe UI", Math.Max(5, GetFontSize(cmbFontNormal) * sf - 1));

        // Page border
        g.DrawRectangle(borderPen, ox, oy, pw2, ph2);

        // Shadow margin areas
        g.FillRectangle(shadeBrush, ox, oy, pw2, mTop);
        g.FillRectangle(shadeBrush, ox, oy + ph2 - mBot, pw2, mBot);
        g.FillRectangle(shadeBrush, ox, oy, mLef, ph2);
        g.FillRectangle(shadeBrush, ox + pw2 - mRig, oy, mRig, ph2);

        // Printable area border
        g.DrawRectangle(marginPen, ox + mLef, oy + mTop, pw2 - mLef - mRig, ph2 - mTop - mBot);

        float yOff = (float)_nudYOffset.Value / 25.4f * 100 * scale;
        float cx = ox + mLef + 4;
        float cy = oy + mTop + 4 + yOff;
        float cw = pw2 - mLef - mRig - 8;
        float lh = normFont.Height + 2;

        // Header
        g.DrawString(txtHeaderTitle.Text, titleFont, textBrush, cx, cy);
        cy += titleFont.Height + 2;
        g.DrawString(txtHeaderSub.Text, subFont, textBrush, cx, cy);
        cy += subFont.Height + 2;
        g.DrawLine(borderPen, cx, cy, cx + cw, cy);
        cy += 5;

        // Section box
        g.FillRectangle(headerBrush, cx, cy, cw, lh + 4);
        g.DrawString("  TITULLI I SEKSIONIT", subFont, whiteBr, cx + 2, cy + 2);
        cy += lh + 8;

        // Fields
        for (int i = 0; i < 3; i++)
        {
            g.DrawString($"Fusha {i + 1}:", subFont, textBrush, cx, cy);
            g.DrawString($"Vlera shembull {i + 1}", normFont, textBrush, cx + 50, cy);
            cy += lh;
        }
        cy += 4;

        // Signatures
        float sbw = (cw - 20) / 3;
        for (int i = 0; i < 3; i++)
        {
            float sx = cx + i * (sbw + 10);
            g.DrawRectangle(new Pen(Color.FromArgb(160, 170, 185)), sx, cy, sbw, 26 * sf);
            g.DrawString($"Nenshkrimi {i + 1}", smallFont, Brushes.DimGray, sx + 2, cy + 2);
            g.DrawLine(new Pen(Color.FromArgb(80, 80, 80)), sx + 2, cy + 18 * sf, sx + sbw - 2, cy + 18 * sf);
        }
        cy += 26 * sf + 6;

        // Footer
        g.DrawLine(borderPen, cx, cy, cx + cw, cy);
        cy += 4;
        g.DrawString($"Dokument i gjeneruar nga Sistemi - {DateTime.Now:dd.MM.yyyy}", smallFont, Brushes.Gray, cx, cy);

        titleFont.Dispose(); subFont.Dispose(); normFont.Dispose(); smallFont.Dispose();
    }

    // ── Print Preview (A4 real) ──
    private void ShfaqPrintPreview()
    {
        var pd = new PrintDocument();
        pd.PrintPage += PrintoFaqenPreview;
        pd.DefaultPageSettings.Landscape = rbLandscape.Checked;
        pd.DefaultPageSettings.Margins = new Margins(
            (int)((float)nudMarginLeft.Value * 100f / 25.4f),
            (int)((float)nudMarginRight.Value * 100f / 25.4f),
            (int)((float)nudMarginTop.Value * 100f / 25.4f),
            (int)((float)nudMarginBottom.Value * 100f / 25.4f));

        // Set paper size
        int idx = cmbFormati.SelectedIndex;
        if (idx < 0) idx = 0;
        int pw = (int)(Formatet[idx].W / 25.4f * 100);
        int ph = (int)(Formatet[idx].H / 25.4f * 100);
        pd.DefaultPageSettings.PaperSize = new PaperSize(Formatet[idx].Name, pw, ph);

        using var dlg = new PrintPreviewDialog
        {
            Document = pd,
            Width = 1200,
            Height = 800,
            Text = "Parashiko - Shiko si del ne print",
            StartPosition = FormStartPosition.CenterScreen,
            UseAntiAlias = true
        };
        dlg.ShowDialog(this);
    }

    private void PrintoFaqenPreview(object? sender, PrintPageEventArgs e)
    {
        var g = e.Graphics!;
        g.PageUnit = GraphicsUnit.Display;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        var page = e.PageBounds;
        int idx = cmbFormati.SelectedIndex;
        if (idx < 0) idx = 0;
        float fw = Formatet[idx].W / 25.4f * 100;
        float fh = Formatet[idx].H / 25.4f * 100;
        bool landscape = rbLandscape.Checked;
        float pageW = landscape ? fh : fw;
        float pageH = landscape ? fw : fh;

        float margin = 40;
        float x0 = margin;
        float y0 = margin + (float)_nudYOffset.Value / 25.4f * 100;
        float w = pageW - margin * 2;
        float y = y0;

        using var headerBg = new SolidBrush(_headerColor);
        using var textBrush = new SolidBrush(_textColor);
        using var whiteBr = new SolidBrush(Color.White);
        using var borderPen = new Pen(_headerColor, 2);
        using var lightPen = new Pen(Color.FromArgb(180, 190, 200));

        var titleFont = new Font("Segoe UI", GetFontSize(cmbFontTitle), FontStyle.Bold);
        var subFont = new Font("Segoe UI", GetFontSize(cmbFontSection), FontStyle.Bold);
        var normFont = new Font("Segoe UI", GetFontSize(cmbFontNormal));
        var smallFont = new Font("Segoe UI", GetFontSize(cmbFontNormal) - 1);
        var labelFont = new Font("Segoe UI", GetFontSize(cmbFontNormal), FontStyle.Bold);
        float lh = normFont.Height + 4;

        // Header
        g.DrawString(txtHeaderTitle.Text, titleFont, textBrush, x0 + 5, y);
        y += titleFont.Height + 4;
        g.DrawString(txtHeaderSub.Text, subFont, textBrush, x0 + 5, y);
        y += subFont.Height + 4;
        g.DrawString(txtHeaderAddr.Text, normFont, textBrush, x0 + 5, y);
        y += normFont.Height + 2;
        g.DrawLine(borderPen, x0, y, x0 + w, y);
        y += 10;

        // Titulli i dokumentit
        string docTitle = _lloji.ToUpper();
        g.DrawString(docTitle, subFont, textBrush, x0 + 5, y);
        y += subFont.Height + 8;

        // Section: Te dhenat
        g.FillRectangle(headerBg, x0, y, w, lh + 2);
        g.DrawString("  " + txtSectionArmes.Text, subFont, whiteBr, x0 + 6, y + 1);
        y += lh + 6;

        // Rows
        string[] labels = { "Seriali:", "Marka:", "Modeli:", "Kalibri:", "Nr. Inventari:" };
        foreach (var lbl in labels)
        {
            g.DrawString(lbl, labelFont, textBrush, x0 + 8, y);
            g.DrawString("________________", normFont, Brushes.Gray, x0 + 100, y);
            y += lh;
        }

        y += 6;

        // Section: Klienti
        g.FillRectangle(headerBg, x0, y, w, lh + 2);
        g.DrawString("  " + txtSectionKlient.Text, subFont, whiteBr, x0 + 6, y + 1);
        y += lh + 6;

        string[] klLabels = { "Emri:", "Mbiemri:", "Nr. Leternjoftimit:", "Adresa:" };
        foreach (var lbl in klLabels)
        {
            g.DrawString(lbl, labelFont, textBrush, x0 + 8, y);
            g.DrawString("________________", normFont, Brushes.Gray, x0 + 120, y);
            y += lh;
        }

        y += 6;

        // Signature block
        string[] sigNames = { txtSig1.Text, txtSig2.Text, txtSig3.Text };
        g.DrawLine(lightPen, x0, y, x0 + w, y);
        y += 6;
        float sbw = (w - 40) / 3;
        for (int i = 0; i < 3; i++)
        {
            float sx = x0 + i * (sbw + 20);
            g.DrawRectangle(lightPen, sx, y, sbw, 65);
            g.DrawString(sigNames[i], labelFont, textBrush, sx + 4, y + 2);
            g.DrawLine(new Pen(Color.FromArgb(80, 80, 80)), sx + 4, y + 48, sx + sbw - 4, y + 48);
            g.DrawString("Nenshkrimi / Data", smallFont, Brushes.Gray, sx + 4, y + 50);
        }
        y += 75;

        // Footer
        g.DrawLine(borderPen, x0, y, x0 + w, y);
        y += 6;
        g.DrawString($"{txtFooterText.Text} - {DateTime.Now:dd.MM.yyyy HH:mm:ss}", smallFont, Brushes.Gray, x0 + 5, y);
        y += normFont.Height;
        g.DrawString($"Template: {txtEmri.Text} | Kopje: {(int)nudKopje.Value}", smallFont, Brushes.Gray, x0 + 5, y);
        y += normFont.Height;

        // Total pages
        float pageHmm = pageH - margin * 2;
        g.DrawString($"Faqe: 1", smallFont, Brushes.DarkGray, x0 + w - 60, y);

        e.HasMorePages = false;
        titleFont.Dispose(); subFont.Dispose(); normFont.Dispose(); smallFont.Dispose(); labelFont.Dispose();
    }

    private static int GetFontSize(ComboBox cmb) => cmb.SelectedItem is int v ? v : 10;

    private void RuajTemplate()
    {
        var emri = txtEmri.Text.Trim();
        if (string.IsNullOrWhiteSpace(emri))
        {
            MessageBox.Show("Ju lutem jepni nje emer per template.", "Kerkese", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var seksionetAktive = new List<string>();
        for (int i = 0; i < lstSeksione.Items.Count; i++)
            if (lstSeksione.GetItemChecked(i))
                seksionetAktive.Add(SeksioneDefault[i]);

        var formatName = Formatet[cmbFormati.SelectedIndex].Name;

        var paramsDict = new Dictionary<string, object>
        {
            ["kopje"] = (int)nudKopje.Value,
            ["headerColor"] = _headerColor.ToArgb(),
            ["textColor"] = _textColor.ToArgb(),
            ["fontTitle"] = GetFontSize(cmbFontTitle),
            ["fontNormal"] = GetFontSize(cmbFontNormal),
            ["fontSection"] = GetFontSize(cmbFontSection),
            ["formati"] = formatName,
            ["orientation"] = rbLandscape.Checked ? "landscape" : "portrait",
            ["marginTop"] = (double)nudMarginTop.Value,
            ["marginBottom"] = (double)nudMarginBottom.Value,
            ["marginLeft"] = (double)nudMarginLeft.Value,
            ["marginRight"] = (double)nudMarginRight.Value,
            ["yOffset"] = (double)_nudYOffset.Value,
            ["txtHeaderTitle"] = txtHeaderTitle.Text,
            ["txtHeaderSub"] = txtHeaderSub.Text,
            ["txtHeaderAddr"] = txtHeaderAddr.Text,
            ["txtSectionArmes"] = txtSectionArmes.Text,
            ["txtSectionKlient"] = txtSectionKlient.Text,
            ["txtSig1"] = txtSig1.Text,
            ["txtSig2"] = txtSig2.Text,
            ["txtSig3"] = txtSig3.Text,
            ["txtFooter"] = txtFooterText.Text,
            ["seksione"] = string.Join(",", seksionetAktive)
        };

        var paramStr = JsonSerializer.Serialize(paramsDict);

        try
        {
            if (_existing != null)
            {
                var id = Convert.ToInt32(_existing["Id"]);
                _db.NdryshoParametratTemplate(id, paramStr);
            }
            else
                _db.ShtoTemplate(emri, _lloji, paramStr);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Gabim: {ex.Message}", "Gabim", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
