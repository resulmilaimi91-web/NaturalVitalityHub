#r "System.Drawing.Common.dll"
using System.Drawing;
using System.Drawing.Imaging;

// Krijo nje ikone 64x64 me ngjyre blu dhe nje simbol arme
var bmp = new Bitmap(64, 64);
using var g = Graphics.FromImage(bmp);
g.Clear(Color.FromArgb(45, 65, 90));

// Rreth i bardhe
g.FillEllipse(Brushes.White, 4, 4, 56, 56);
g.DrawEllipse(new Pen(Color.FromArgb(30, 50, 70), 2), 4, 4, 56, 56);

// Shkronja "A" e stilizuar
using var font = new Font("Segoe UI", 28, FontStyle.Bold);
g.DrawString("A", font, Brushes.FromArgb(45, 65, 90), 12, 10);

// Rreth i jashtem
g.DrawEllipse(new Pen(Color.FromArgb(200, 200, 200), 1), 1, 1, 62, 62);

bmp.Save("app.ico", ImageFormat.Png);
Console.WriteLine("Icon generated: app.ico");
