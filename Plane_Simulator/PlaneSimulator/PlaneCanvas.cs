using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace PlaneSimulator;

/// <summary>Şəffaf PNG təyyarəsini və yanıb-sönən işığı çəkir.</summary>
public sealed class PlaneCanvas : Control
{
    private readonly Image? planeImage;
    private readonly Image? lightImage;
    private bool engineOn;
    private float speed;
    private bool turbulence;
    private int frameNumber;

    public PlaneCanvas(Color background)
    {
        BackColor = background;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        planeImage = TryLoad("plane.png");
        lightImage = TryLoad("light.png");
    }

    private static Image? TryLoad(string filename)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Assets", filename);
        return File.Exists(path) ? Image.FromFile(path) : null;
    }

    public void UpdateState(bool poweredOn, float currentSpeed, bool isTurbulent, int frame)
    {
        engineOn = poweredOn;
        speed = currentSpeed;
        turbulence = isTurbulent;
        frameNumber = frame;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(BackColor);

        // Sürətləndikcə təyyarə yuxarıya doğru hərəkət edir.
        float ascent = speed / 220f * 43f;
        float shakeX = turbulence ? (float)Math.Sin(frameNumber * 1.8f) * 4.0f : 0;
        float shakeY = turbulence ? (float)Math.Cos(frameNumber * 2.1f) * 2.5f : 0;
        float bob = engineOn ? (float)Math.Sin(frameNumber * 0.08f) * 1.4f : 0;
        const float planeWidth = 105;
        const float planeHeight = 138;
        float x = (Width - planeWidth) / 2f + shakeX;
        float y = Height - 152 - ascent + shakeY + bob;

        if (planeImage != null)
        {
            g.DrawImage(planeImage, x, y, planeWidth, planeHeight);
        }
        else
        {
            // Fayl yüklənməsə, sadə ehtiyat təyyarə silueti göstərilir.
            using var brush = new SolidBrush(Color.Gainsboro);
            PointF[] body = { new(x + 52, y), new(x + 63, y + 110), new(x + 52, y + 138), new(x + 42, y + 110) };
            g.FillPolygon(brush, body);
            PointF[] wings = { new(x, y + 95), new(x + 52, y + 58), new(x + 105, y + 95), new(x + 52, y + 79) };
            g.FillPolygon(brush, wings);
        }

        // Nümunədə təyyarənin burnu üzərində görünən parlaq işıq.
        bool flashOn = engineOn && (frameNumber / 14) % 2 == 0;
        if (flashOn && lightImage != null)
        {
            float lightSize = 33 + (float)Math.Sin(frameNumber * 0.32f) * 3;
            float center = Width / 2f + shakeX;
            g.DrawImage(lightImage, center - lightSize / 2f,
                y - 22 - lightSize / 2f, lightSize, lightSize);
        }

        if (engineOn && speed > 5)
        {
            // Qanad uclarında zəif naviqasiya işıqları.
            using var green = new SolidBrush(Color.FromArgb(200, 95, 255, 124));
            using var red = new SolidBrush(Color.FromArgb(200, 255, 78, 78));
            g.FillEllipse(red, x + 1, y + 94, 4, 4);
            g.FillEllipse(green, x + planeWidth - 5, y + 94, 4, 4);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            planeImage?.Dispose();
            lightImage?.Dispose();
        }
        base.Dispose(disposing);
    }
}
