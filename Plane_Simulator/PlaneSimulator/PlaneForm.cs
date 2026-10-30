using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;
using FormsTimer = System.Windows.Forms.Timer;

namespace PlaneSimulator;

/// <summary>
/// Təyyarə simulyatorunun əsas pəncərəsi.
/// Görünüş göndərilən nümunəyə əsasən qurulub.
/// </summary>
public sealed class PlaneForm : Form
{
    private static readonly Color ButtonOrange = Color.FromArgb(255, 130, 0);
    private static readonly Color BackgroundGray = Color.FromArgb(105, 105, 105);
    private static readonly Color ControlBackground = Color.FromArgb(238, 238, 238);

    private readonly PlaneCanvas planeCanvas;
    private readonly FormsTimer animationTimer;
    private readonly Dictionary<string, SoundPlayer> sounds = new();
    private readonly Label engineLabel;
    private readonly Label speedLabel;
    private readonly Button gasButton;
    private readonly Button brakeButton;
    private readonly Button powerButton;

    private bool engineOn;
    private bool gasHeld;
    private bool brakeHeld;
    private bool spaceHeld;
    private float speed;
    private int turbulenceFrames;
    private int frameCounter;

    public PlaneForm()
    {
        Text = "Plane";
        ClientSize = new Size(572, 418);
        MinimumSize = Size;
        MaximumSize = Size;
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = ControlBackground;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        KeyPreview = true;
        AutoScaleMode = AutoScaleMode.None;

        // Sol tərəfdə təyyarə və yanıb-sönən işıq yerləşir.
        planeCanvas = new PlaneCanvas(BackgroundGray)
        {
            Bounds = new Rectangle(0, 0, 230, 418)
        };
        Controls.Add(planeCanvas);

        engineLabel = new Label
        {
            AutoSize = false,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(225, 225, 225),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Bounds = new Rectangle(12, 12, 135, 20),
            Text = "ENGINE  OFF"
        };
        speedLabel = new Label
        {
            AutoSize = false,
            BackColor = Color.Transparent,
            ForeColor = Color.FromArgb(225, 225, 225),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Bounds = new Rectangle(155, 12, 65, 20),
            TextAlign = ContentAlignment.TopRight,
            Text = "000 KT"
        };
        planeCanvas.Controls.Add(engineLabel);
        planeCanvas.Controls.Add(speedLabel);

        // Nümunədəki 5 narıncı düymə. Hər biri öz səs faylını səsləndirir.
        AddAudioButton("Eniş hazırlığı", 8, "inishazirliklari.wav", null);
        AddAudioButton("Turbulans", 59, "turbulans.wav", () => turbulenceFrames = 140);
        AddAudioButton("Kəmərlər", 110, "kemerler.wav", null);
        AddAudioButton("Təşəkkürlər", 161, "tesekkurler.wav", null);
        AddAudioButton("Beep", 212, "beep.wav", null);

        gasButton = CreateFramedButton("GAS", new Rectangle(249, 297, 123, 69), Color.Green, Color.White);
        brakeButton = CreateFramedButton("Break", new Rectangle(419, 297, 126, 69), Color.FromArgb(198, 0, 0), Color.White);
        powerButton = CreateFramedButton("ON/OFF", new Rectangle(230, 373, 342, 45), Color.FromArgb(0, 0, 209), Color.White, inset: 2);

        gasButton.MouseDown += (_, args) => { if (args.Button == MouseButtons.Left) { gasHeld = true; } };
        gasButton.MouseUp += (_, _) => gasHeld = false;
        gasButton.MouseLeave += (_, _) => gasHeld = false;
        gasButton.Click += (_, _) => { if (engineOn) speed = Math.Min(220, speed + 8); };

        brakeButton.MouseDown += (_, args) => { if (args.Button == MouseButtons.Left) { brakeHeld = true; } };
        brakeButton.MouseUp += (_, _) => brakeHeld = false;
        brakeButton.MouseLeave += (_, _) => brakeHeld = false;
        brakeButton.Click += (_, _) => speed = Math.Max(0, speed - 14);

        powerButton.Click += (_, _) => ToggleEngine();

        LoadSounds();
        animationTimer = new FormsTimer { Interval = 33 };
        animationTimer.Tick += (_, _) => AdvanceSimulation();
        animationTimer.Start();

        KeyDown += HandleKeyDown;
        KeyUp += HandleKeyUp;
        FormClosing += (_, _) => animationTimer.Stop();
        UpdateIndicators();
    }

    private void AddAudioButton(string title, int top, string audioFile, Action? afterPlayback)
    {
        Button button = CreateFramedButton(title, new Rectangle(241, top, 323, 46), ButtonOrange, Color.Black, inset: 2);
        button.Click += (_, _) =>
        {
            Play(audioFile);
            afterPlayback?.Invoke();
        };
        var tip = new ToolTip();
        tip.SetToolTip(button, "Səsli elan: " + title);
    }

    private Button CreateFramedButton(string title, Rectangle frameBounds, Color back, Color fore, int inset = 3)
    {
        Panel frame = new Panel
        {
            Bounds = frameBounds,
            BackColor = Color.FromArgb(198, 198, 198),
            BorderStyle = BorderStyle.FixedSingle
        };
        Button button = new Button
        {
            Text = title,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point),
            BackColor = back,
            ForeColor = fore,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            Cursor = Cursors.Hand,
            TabStop = true,
            Bounds = new Rectangle(inset - 1, inset - 1, frameBounds.Width - 2 * inset, frameBounds.Height - 2 * inset)
        };
        button.FlatAppearance.BorderColor = Color.White;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(back, 0.1f);
        button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(back, 0.1f);
        frame.Controls.Add(button);
        Controls.Add(frame);
        return button;
    }

    private void LoadSounds()
    {
        string[] filenames =
        {
            "inishazirliklari.wav", "turbulans.wav", "kemerler.wav",
            "tesekkurler.wav", "beep.wav", "isikacmakapamasesi.wav"
        };
        string directory = Path.Combine(AppContext.BaseDirectory, "Assets");
        foreach (string name in filenames)
        {
            string fullPath = Path.Combine(directory, name);
            if (!File.Exists(fullPath)) continue;
            try
            {
                var player = new SoundPlayer(fullPath);
                player.Load();
                sounds.Add(name, player);
            }
            catch (Exception)
            {
                // Audio problemi proqramın qalan hissəsini dayandırmır.
            }
        }
    }

    private void Play(string file)
    {
        if (sounds.TryGetValue(file, out SoundPlayer? player))
        {
            try { player.Play(); }
            catch (Exception ex) { MessageBox.Show("Səs səsləndirilə bilmədi: " + ex.Message); }
        }
        else
        {
            MessageBox.Show("Səs faylı tapılmadı: " + file, "Xəbərdarlıq", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ToggleEngine()
    {
        engineOn = !engineOn;
        if (!engineOn)
        {
            gasHeld = false;
            brakeHeld = false;
            turbulenceFrames = 0;
            speed = 0;
        }
        Play("isikacmakapamasesi.wav");
        UpdateIndicators();
        planeCanvas.Invalidate();
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
        {
            gasHeld = true;
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
        {
            brakeHeld = true;
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Space)
        {
            if (!spaceHeld) ToggleEngine();
            spaceHeld = true;
            e.SuppressKeyPress = true;
        }
    }

    private void HandleKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W) gasHeld = false;
        if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S) brakeHeld = false;
        if (e.KeyCode == Keys.Space) spaceHeld = false;
    }

    private void AdvanceSimulation()
    {
        frameCounter++;
        if (engineOn)
        {
            if (gasHeld) speed = Math.Min(220, speed + 2.0f);
            if (brakeHeld) speed = Math.Max(0, speed - 4.2f);
            if (!gasHeld && !brakeHeld) speed = Math.Max(0, speed - 0.12f);
        }
        else
        {
            speed = 0;
        }
        if (turbulenceFrames > 0) turbulenceFrames--;

        UpdateIndicators();
        planeCanvas.UpdateState(engineOn, speed, turbulenceFrames > 0, frameCounter);
    }

    private void UpdateIndicators()
    {
        engineLabel.Text = engineOn ? "ENGINE  ON" : "ENGINE  OFF";
        engineLabel.ForeColor = engineOn ? Color.LightGreen : Color.Gainsboro;
        speedLabel.Text = ((int)speed).ToString("000") + " KT";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            animationTimer?.Dispose();
            foreach (var sound in sounds.Values) sound.Dispose();
        }
        base.Dispose(disposing);
    }
}
