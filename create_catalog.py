from fpdf import FPDF
import os

img_dir = r"D:\ANDROID\opencode\product_images"
logo_path = os.path.join(img_dir, "row2_0.png")

products = [
    (1, "Delta Standard", "AREX", "Pistolete", 1200, "row3_1.png",
     [("Kalibri", "9x19mm Parabellum"), ("Sistemi", "Striker Double Action"),
      ("Gjatesia totale", "180 mm"), ("Lartesia", "128 mm"), ("Gjeresia", "30 mm"),
      ("Gjatesia e tytes", '102 mm (4")'), ("Pesha pa mag", "545 g"),
      ("Pesha me mag", "628 g"), ("Kapaciteti", "15+1 / 17+1"),
      ("Korniza", "Polimer i zi"), ("Shina", "Picatinny")]),

    (2, "Delta M", "AREX", "Pistolete", 1200, "row4_19.png",
     [("Kalibri", "9x19mm Parabellum"), ("Sistemi", "Striker Double Action"),
      ("Gjatesia totale", "183 mm"), ("Lartesia", "128 mm"), ("Gjeresia", "30 mm"),
      ("Gjatesia e tytes", '102 mm (4")'), ("Pesha pa mag", "506 g"),
      ("Pesha me mag", "579 g"), ("Kapaciteti", "15+1 / 17+1"),
      ("Korniza", "Polimer Compact"), ("Opsioni", "Optic Ready")]),

    (3, "Delta L", "AREX", "Pistolete", 1450, "row5_20.png",
     [("Kalibri", "9x19mm Parabellum"), ("Sistemi", "Striker Double Action"),
      ("Gjatesia totale", "196 mm"), ("Lartesia", "131 mm"), ("Gjeresia", "30 mm"),
      ("Gjatesia e tytes", '114 mm (4.5")'), ("Pesha pa mag", "596 g"),
      ("Pesha me mag", "677 g"), ("Kapaciteti", "17+1 / 19+1"),
      ("Korniza", "Polimer i zi"), ("Opsioni", "Optic Ready")]),

    (4, "Delta L FDE / Olive", "AREX", "Pistolete", 1550, "row6_21.png",
     [("Kalibri", "9x19mm Parabellum"), ("Sistemi", "Striker Double Action"),
      ("Gjatesia totale", "196 mm"), ("Lartesia", "131 mm"), ("Gjeresia", "30 mm"),
      ("Gjatesia e tytes", '114 mm (4.5")'), ("Pesha pa mag", "565 g"),
      ("Pesha me mag", "646 g"), ("Kapaciteti", "17+1 / 19+1"),
      ("Korniza", "Polimer FDE/Olive"), ("Opsioni", "Optic Ready")]),

    (5, "Zero 1 S Nickel", "AREX", "Pistolete", 1700, "row7_2.png",
     [("Kalibri", "9x19mm Parabellum"), ("Sistemi", "DA/SA"),
      ("Gjatesia totale", "195 mm"), ("Lartesia", "144 mm"), ("Gjeresia", "38 mm"),
      ("Gjatesia e tytes", '108 mm (4.25")'), ("Pesha", "833 g"),
      ("Kapaciteti", "17+1"), ("Korniza", "Alumini i anodizuar"),
      ("Slide", "Nikeli"), ("Siguresa", "Ambidextrous + Decocker")]),

    (6, "Delta X FDE", "AREX", "Pistolete", 1450, "row8_3.png",
     [("Kalibri", "9x19mm Parabellum"), ("Sistemi", "Striker Double Action"),
      ("Gjatesia totale", "183 mm"), ("Lartesia", "131 mm"), ("Gjeresia", "30 mm"),
      ("Gjatesia e tytes", '102 mm (4")'), ("Pesha pa mag", "541 g"),
      ("Pesha me mag", "625 g"), ("Kapaciteti", "17+1 / 19+1"),
      ("Korniza", "Polimer FDE"), ("Opsioni", "Optic Ready + Magwell")]),

    (7, "Stock I Black 2025", "TANFOGLIO", "Pistolete", 2000, "row9_4.png",
     [("Kalibri", "9x19mm"), ("Sistemi", "DA (Double Action)"),
      ("Gjatesia totale", "214 mm"), ("Gjatesia e tytes", "113 mm"),
      ("Pesha", "1190 g"), ("Kapaciteti", "17"),
      ("Korniza", "Celik (Steel)"), ("Slide", "Celik"),
      ("Siguresa", "Manual on Frame"), ("Shelbimi", "IPSC Production / IDPA")]),

    (8, "Stock III Special Black 2025", "TANFOGLIO", "Pistolete", 2000, "stock3_special.jpg",
     [("Kalibri", "9x19mm"), ("Sistemi", "DA (Double Action)"),
      ("Gjatesia totale", "225 mm"), ("Gjatesia e tytes", "121 mm"),
      ("Pesha", "1298 g"), ("Kapaciteti", "17"),
      ("Korniza", "Celik (Steel)"), ("Rifling", "Polygonal"),
      ("Trigger", "Next Flat DA"), ("Shelbimi", "IPSC Production")]),

    (9, "Stock III Special Black .45ACP", "TANFOGLIO", "Pistolete", 2200, "stock3_special.jpg",
     [("Kalibri", ".45 ACP"), ("Sistemi", "DA (Double Action)"),
      ("Gjatesia totale", "225 mm"), ("Gjatesia e tytes", "121 mm"),
      ("Pesha", "1300 g"), ("Kapaciteti", "10"),
      ("Korniza", "Celik (Steel)"), ("Rifling", "Polygonal")]),

    (10, "FT 1911 .45ACP Black 2024", "TANFOGLIO", "Pistolete", 1550, "row12_8.png",
     [("Kalibri", ".45 ACP"), ("Sistemi", "SA (Single Action 1911)"),
      ("Gjatesia totale", "222 mm"), ("Gjatesia e tytes", '128 mm (5.03")'),
      ("Pesha", "1100 g"), ("Kapaciteti", "8+1"),
      ("Korniza", "Celik (Steel)"), ("Siguresa", "Grip + Manual")]),

    (11, "FT 1911 .45ACP Silver 2024", "TANFOGLIO", "Pistolete", 1750, "row13_7.png",
     [("Kalibri", ".45 ACP"), ("Sistemi", "SA (Single Action 1911)"),
      ("Gjatesia totale", "222 mm"), ("Gjatesia e tytes", '128 mm (5.03")'),
      ("Pesha", "1100 g"), ("Kapaciteti", "8+1"),
      ("Korniza", "Celik (Steel)"), ("Finish", "Silver/Chrome")]),

    (12, "FT 1911 Custom CHR 2024", "TANFOGLIO", "Pistolete", 1900, "row14_9.png",
     [("Kalibri", ".45 ACP"), ("Sistemi", "SA (Single Action 1911)"),
      ("Gjatesia totale", "222 mm"), ("Gjatesia e tytes", '128 mm (5.03")'),
      ("Pesha", "1215 g"), ("Kapaciteti", "8+1"),
      ("Korniza", "Celik (Steel)"), ("Finish", "Chrome")]),

    (13, "FT 1911 Pugio 2025", "TANFOGLIO", "Pistolete", 1500, "row15_10.png",
     [("Kalibri", "9x19mm"), ("Sistemi", "SA (Single Action 1911)"),
      ("Gjatesia totale", "185 mm"), ("Gjatesia e tytes", '90 mm (3.5")'),
      ("Pesha", "850 g"), ("Kapaciteti", "10+1"),
      ("Korniza", "Polimer (Polymer)"), ("Slide", "Celik")]),

    (14, "Hexagon Tactical K FDE 2025", "TANFOGLIO", "Pistolete", 2500, "row16_11.png",
     [("Kalibri", "9x19mm"), ("Sistemi", "DA/SA"),
      ("Gjatesia totale", '215 mm (8.46")'), ("Gjatesia e tytes", '112 mm (4.41")'),
      ("Pesha", "1260 g (2.78 lbs)"), ("Kapaciteti", "18"),
      ("Korniza", "Celik Large Frame K"), ("Finish", "Cerakote FDE"),
      ("Kompenzator", "HeXagon Integral")]),

    (15, "Combat F Black", "TANFOGLIO", "Pistolete", 1500, "row17_12.png",
     [("Kalibri", "9x19mm"), ("Sistemi", "DA (Double Action)"),
      ("Gjatesia totale", "211 mm"), ("Gjatesia e tytes", "112 mm"),
      ("Pesha", "1125 g"), ("Kapaciteti", "16"),
      ("Korniza", "Celik (Steel)"), ("Siguresa", "Manual on frame")]),

    (16, "Stock III Black 2025", "TANFOGLIO", "Pistolete", 2200, "row18_13.png",
     [("Kalibri", "9x19mm"), ("Sistemi", "DA (Double Action)"),
      ("Gjatesia totale", "227 mm"), ("Gjatesia e tytes", "121 mm"),
      ("Pesha", "1280 g"), ("Kapaciteti", "17"),
      ("Korniza", "Celik (Steel)"), ("Rifling", "Polygonal")]),

    (17, 'TSR 223R 16"', "TANFOGLIO", "Fytek (Rifle)", 2500, "row19_14.png",
     [("Kalibri", ".223 Rem / 5.56x45mm NATO"), ("Sistemi", "Direct Gas Impingement"),
      ("Gjatesia totale", "880 mm"), ("Gjatesia e tytes", '406 mm (16")'),
      ("Pesha", "3500 g"), ("Kapaciteti", "30 (STANAG)"),
      ("Upper/Lower", "Alumini 7075-T6"), ("Gryka", '1/2"x28 UNF')]),

    (18, 'TSR 223 12"', "TANFOGLIO", "Fytek (Rifle)", 2500, "row20_15.png",
     [("Kalibri", ".223 Rem / 5.56x45mm NATO"), ("Sistemi", "Direct Gas Impingement"),
      ("Gjatesia totale", "~760 mm"), ("Gjatesia e tytes", '305 mm (12")'),
      ("Pesha", "~3300 g"), ("Kapaciteti", "30 (STANAG)"),
      ("Upper/Lower", "Alumini 7075-T6")]),

    (19, "Stock I Green 2023", "TANFOGLIO", "Pistolete", 2000, "row21_16.png",
     [("Kalibri", "9x19mm"), ("Sistemi", "DA (Double Action)"),
      ("Gjatesia totale", "214 mm"), ("Gjatesia e tytes", "113 mm"),
      ("Pesha", "1190 g"), ("Kapaciteti", "17"),
      ("Korniza", "Celik (Steel)"), ("Ngjyra", "Green")]),

    (20, "Stock I 2023", "TANFOGLIO", "Pistolete", 2000, "row22_17.png",
     [("Kalibri", "9x19mm"), ("Sistemi", "DA (Double Action)"),
      ("Gjatesia totale", "214 mm"), ("Gjatesia e tytes", "113 mm"),
      ("Pesha", "1190 g"), ("Kapaciteti", "17"),
      ("Korniza", "Celik (Steel)"), ("Ngjyra", "Zi (Black)")]),

    (21, "Zero 1 CP Nickel", "AREX", "Pistolete", 1650, None,
     [("Kalibri", "9x19mm Parabellum"), ("Sistemi", "DA/SA"),
      ("Gjatesia totale", "184 mm"), ("Gjeresia", "38 mm"),
      ("Gjatesia e tytes", '98 mm (3.85")'), ("Pesha", "783 g"),
      ("Kapaciteti", "15+1"), ("Korniza", "Alumini i anodizuar"),
      ("Slide", "Nikeli (Nickel)")]),

    (22, "Zero 2 S", "AREX", "Pistolete", 1800, "row24_18.png",
     [("Kalibri", "9x19mm Parabellum"), ("Sistemi", "DA/SA"),
      ("Gjatesia totale", '198 mm (7.8")'), ("Gjatesia e tytes", '104 mm (4.1")'),
      ("Pesha", "907 g (32 oz)"), ("Kapaciteti", "18"),
      ("Korniza", "Alumini"), ("Siguresa", "Ambidextrous + Decocker")]),

    (23, "MS-15 / 5.56", "SECKIN", "Fytek (Rifle)", 1800, None,
     [("Kalibri", "5.56x45mm / .223 Rem"), ("Sistemi", "Semi-Auto AR-15"),
      ("Kapaciteti", "30 (STANAG)"), ("Materiali", "Alumini 7075-T6"),
      ("Vendi", "Turqi")]),
]

def fmt_price(p):
    return f"EUR {p:,.2f}".replace(",",".")

pdf = FPDF('P', 'mm', 'A4')
pdf.set_auto_page_break(auto=False)

# ===================== COVER PAGE =====================
pdf.add_page()
pdf.set_fill_color(20,35,60)
pdf.rect(0,0,210,297,'F')
pdf.set_fill_color(200,170,80)
pdf.rect(30,40,150,2,'F')

if os.path.exists(logo_path):
    pdf.image(logo_path, x=70, y=55, w=70)

pdf.set_y(120)
pdf.set_font('Helvetica','B',38)
pdf.set_text_color(255,255,255)
pdf.cell(0,16,'KATALOGU I',align='C',new_x='LMARGIN',new_y='NEXT')
pdf.cell(0,16,'PRODUKTEVE',align='C',new_x='LMARGIN',new_y='NEXT')

pdf.ln(5)
pdf.set_font('Helvetica','',16)
pdf.set_text_color(200,170,80)
pdf.cell(0,10,'2025',align='C',new_x='LMARGIN',new_y='NEXT')

pdf.ln(8)
pdf.set_draw_color(200,170,80)
pdf.set_line_width(0.5)
pdf.line(70,pdf.get_y(),140,pdf.get_y())

pdf.ln(10)
pdf.set_font('Helvetica','',12)
pdf.set_text_color(180,180,180)
pdf.cell(0,8,'Cmimet Market Price',align='C',new_x='LMARGIN',new_y='NEXT')
pdf.cell(0,8,'AREX  |  TANFOGLIO',align='C')

pdf.set_fill_color(200,170,80)
pdf.rect(30,250,150,2,'F')

# ===================== PRODUCT PAGES =====================
for nr, name, brand, category, price, img_file, specs in products:
    pdf.add_page()

    # Left dark panel
    pdf.set_fill_color(20,35,60)
    pdf.rect(0,0,85,297,'F')

    # Right light panel
    pdf.set_fill_color(245,246,250)
    pdf.rect(85,0,125,297,'F')

    # Brand
    pdf.set_font('Helvetica','',8)
    pdf.set_text_color(200,170,80)
    pdf.set_xy(12,18)
    pdf.cell(0,5,brand)

    # Category
    pdf.set_font('Helvetica','I',7)
    pdf.set_text_color(150,150,150)
    pdf.set_xy(12,24)
    pdf.cell(0,4,category)

    # Product number big
    pdf.set_font('Helvetica','B',52)
    pdf.set_text_color(200,170,80)
    pdf.set_xy(12,38)
    pdf.cell(0,20,f'{nr:02d}')

    # Product name
    pdf.set_font('Helvetica','B',11)
    pdf.set_text_color(255,255,255)
    pdf.set_xy(12,68)
    pdf.multi_cell(68,6,name)

    # Divider
    pdf.set_draw_color(200,170,80)
    pdf.set_line_width(0.5)
    pdf.line(12,90,80,90)

    # Price
    pdf.set_font('Helvetica','B',22)
    pdf.set_text_color(200,170,80)
    pdf.set_xy(12,96)
    pdf.cell(70,10,fmt_price(price))

    pdf.set_font('Helvetica','',7)
    pdf.set_text_color(150,150,150)
    pdf.set_xy(12,108)
    pdf.cell(70,5,'Cmimi Market')

    # Image on right
    if img_file:
        img_path = os.path.join(img_dir, img_file)
        if os.path.exists(img_path):
            try:
                pdf.image(img_path, x=92, y=15, w=108)
            except:
                pass

    # Specs title
    pdf.set_font('Helvetica','B',10)
    pdf.set_text_color(20,35,60)
    pdf.set_xy(92,132)
    pdf.cell(0,7,'SPECIFIKAT TEKNIKE')

    pdf.set_draw_color(20,35,60)
    pdf.set_line_width(0.3)
    pdf.line(92,141,200,141)

    # Specs
    y = 145
    pdf.set_font('Helvetica','',8)
    for i, (key, val) in enumerate(specs):
        if y > 275:
            break
        if i % 2 == 0:
            pdf.set_fill_color(235,237,242)
        else:
            pdf.set_fill_color(255,255,255)
        pdf.set_text_color(50,50,50)
        pdf.set_xy(92,y)
        pdf.cell(48,7,f'  {key}',fill=True)
        pdf.set_font('Helvetica','B',8)
        pdf.cell(57,7,val,fill=True)
        pdf.set_font('Helvetica','',8)
        y += 7

    # Bottom bar
    pdf.set_fill_color(200,170,80)
    pdf.rect(0,285,210,12,'F')
    pdf.set_font('Helvetica','I',6)
    pdf.set_text_color(20,35,60)
    pdf.set_xy(10,287)
    pdf.cell(0,4,f'Arex & Tanfoglio Katalogu 2025 | Produkti {nr}')
    pdf.set_xy(0,287)
    pdf.cell(200,4,f'Faqja {pdf.page_no()-1}',align='R')

# Save
output_path = r'D:\ANDROID\opencode\Katalogu_Arex_2025.pdf'
pdf.output(output_path)
print(f'PDF created: {output_path}')
print(f'Total pages: {pdf.page_no()}')
