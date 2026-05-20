using System;
using System.Drawing;
using System.Drawing.Text;
using System.Media;
using System.Windows.Forms;

namespace DOOM
{
    // ── MenuLogic ────────────────────────────────────────────────────────────
    public class MenuItem
    {
        public string Text { get; private set; }
        public Image Image { get; private set; }  // the menu item texture
        public Action OnSelect { get; private set; }

        public MenuItem(string text, string texturePath, Action onSelect)
        {
            Text = text;
            Image = Image.FromFile(texturePath);
            OnSelect = onSelect;
        }
    }
    public class MenuLogic
    {
        private Screen _form;

        // Fonts
        private PrivateFontCollection _fonts = new PrivateFontCollection();
        private Font _menuFont;

        // Images
        private Image _skull = Image.FromFile("Assets\\MainMenu\\m_skull1.png");
        private Image _background = Image.FromFile("Assets\\MainMenu\\titlepic.png");

        // Intro screen vars
        private bool _firstEnter = true;
        private bool _blinkVisible = true;
        private System.Windows.Forms.Timer _blinkTimer;

        // Menu vars
        private int _selected = 0;
        private RectangleF[] _menuBounds = new RectangleF[4]; // To measure the text borders
        
        private MenuItem[] _menuItems;

        // Options state
        private bool _inOptions = false;
        private int _optionSelected = 0;

        // ----------------- Constructor ------------------- //
        public MenuLogic(Screen form)
        {
            _form = form;

            // Blink timer for "PRESS ANY KEY"
            _blinkTimer = new System.Windows.Forms.Timer();
            _blinkTimer.Interval = 500;
            _blinkTimer.Tick += (s, e) =>
            {
                _blinkVisible = !_blinkVisible;
                _form.Invalidate(); 
            };
            _blinkTimer.Start();

            _menuItems = new MenuItem[]
             {
                new MenuItem("Slay Demons",    "Assets\\MainMenu\\m_newg.png",  () => _form.GoToGame()),
                new MenuItem("Options",        "Assets\\MainMenu\\m_lgttl.png", () => _form.GoToGame()),
                new MenuItem("Options",        "Assets\\MainMenu\\m_optttl.png", () => { _inOptions = true; _form.Invalidate(); }),
                new MenuItem("Quit Game",      "Assets\\MainMenu\\m_endgam.png",  () => Application.Exit()),
             };

            // Set the text font
            _fonts.AddFontFile("Assets\\MainMenu\\DooM.ttf");
            _menuFont = new Font(_fonts.Families[0], 32, FontStyle.Regular);
        }

        // ── Draw — called by Form1.OnPaint ────────────────
        public void Draw(Graphics g, Size clientSize)
        {
            if (_firstEnter) { DrawIntroScreen(g, clientSize); return;}

            if (_inOptions) { DrawOptions(g, clientSize); return; }

            else DrawMainMenu(g, clientSize);
        }

        // ── Intro Screen ──────────────────────────────────
        private void DrawIntroScreen(Graphics g, Size clientSize)
        {
            g.DrawImage(_background, 0, 0, clientSize.Width, clientSize.Height);

            if (_blinkVisible)
            {
                string text = "- PRESS ANY KEY -";
                float y = (clientSize.Height - 200);
                float x = (clientSize.Width / 12);

                g.DrawString(text, _menuFont, new SolidBrush(Color.FromArgb(20, 0, 0)), x + 6, y + 6);
                g.DrawString(text, _menuFont, new SolidBrush(Color.FromArgb(90, 0, 0)), x + 3, y + 3);
                g.DrawString(text, _menuFont, Brushes.Red, x, y);
            }
        }

        // ── Main Menu ─────────────────────────────────────
        private void DrawMainMenu(Graphics g, Size clientSize)
        {
            g.DrawImage(_background, 0, 0, clientSize.Width, clientSize.Height);

            int startY = (clientSize.Height / 2) - 40;

            for (int i = 0; i < _menuItems.Length; i++)
            {
                float y = startY + i * 60;
                float x = (clientSize.Width / 2) - 200;

                _menuBounds[i] = new RectangleF(x, y, 350, 50);

                if (i == _selected)
                    g.DrawImage(_menuItems[i].Image, x, y, 350, 50);
                else
                    DrawDimmed(g, _menuItems[i].Image, _menuBounds[i]);
            }

            // Skulls on selected item
            g.DrawImage(_skull, _menuBounds[_selected].X - 70, _menuBounds[_selected].Y-2, 60, 60);
            g.DrawImage(_skull, _menuBounds[_selected].Right + 10, _menuBounds[_selected].Y-2, 60, 60);
        }

        private void DrawDimmed(Graphics g, Image img, RectangleF bounds)
        {
            var cm = new System.Drawing.Imaging.ColorMatrix();
            cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = 0.4f; // 40% brightness

            var attr = new System.Drawing.Imaging.ImageAttributes();
            attr.SetColorMatrix(cm);

            g.DrawImage(img,
                Rectangle.Round(bounds),
                0, 0, img.Width, img.Height,
                GraphicsUnit.Pixel,
                attr);
        }

        // ── Options Screen ────────────────────────────────
        private void DrawOptions(Graphics g, Size clientSize)
        {
            g.DrawImage(_background, 0, 0, clientSize.Width, clientSize.Height); // Draw the BG

            // Big title
            string title = "- Options -";
            SizeF titleSize = g.MeasureString(title, _menuFont);
            float titleX = (clientSize.Width - titleSize.Width) / 2;
            float titleY = (clientSize.Height / 2) - 160;

            g.DrawString(title, _menuFont, new SolidBrush(Color.FromArgb(20, 0, 0)), titleX + 6, titleY + 6);
            g.DrawString(title, _menuFont, new SolidBrush(Color.FromArgb(90, 0, 0)), titleX + 3, titleY + 3);
            g.DrawString(title, _menuFont, Brushes.Red, titleX, titleY);

            // Option items
            string[] items = { "Sound :  < ON >", "Back" };

            int startY = clientSize.Height / 2 - 40;

            for (int i = 0; i < items.Length; i++)
            {
                float y = startY + i * 60;
                SizeF size = g.MeasureString(items[i], _menuFont);
                float x = (clientSize.Width - size.Width) / 2;

                g.DrawString(items[i], _menuFont, new SolidBrush(Color.FromArgb(20, 0, 0)), x + 6, y + 6);
                g.DrawString(items[i], _menuFont, new SolidBrush(Color.FromArgb(90, 0, 0)), x + 3, y + 3);

                Color col = (i == _optionSelected) ? Color.Red : Color.FromArgb(155, 15, 15);
                g.DrawString(items[i], _menuFont, new SolidBrush(col), x, y);
            }
        }

        // -- Run the DrawingBoard form -------
        private void RunDrawingBoard(Screen from)
        {
            var drawForm = new DrawForm(_form);
            drawForm.StartPosition = FormStartPosition.Manual;
            drawForm.Location = _form.Location;
            _form.PlayMusic("Assets\\Music\\in_game.wav");
            drawForm.Show();
            _form.Hide();
        }
        // ── HandleKeys — called by Form1.OnKeyDown ────────
        public void HandleKeys(KeyEventArgs e)
        {
            // Exit the intro screen on any key press
            if (_firstEnter)
            {
                _firstEnter = false;
                _blinkTimer.Stop();
                _blinkTimer.Dispose();
                _form.Invalidate();
                return;
            }

            // ── Options screen input ───────────────────────
            if (_inOptions)
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                    _optionSelected = (_optionSelected - 1 + 2) % 2;

                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                    _optionSelected = (_optionSelected + 1) % 2;

                if (e.KeyCode == Keys.Escape)
                    _inOptions = false;

                // Enter/Space on "Back" closes options
                if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) && _optionSelected == 1)
                    _inOptions = false;

                _form.Invalidate();
                return; // ← stop here, don't fall into menu logic
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                _selected = (_selected - 1 + _menuItems.Length) % _menuItems.Length;

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                _selected = (_selected + 1) % _menuItems.Length;

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                _menuItems[_selected].OnSelect?.Invoke();

            _form.Invalidate();
        }

        // ── HandleMouseMove — called by Form1.OnMouseMove ─
        public void HandleMouseMove(MouseEventArgs e)
        {
            for (int i = 0; i < _menuBounds.Length; i++)
            {
                if (_menuBounds[i].Contains(e.X, e.Y))
                {
                    if (_selected != i) // only redraw if selection changed
                    {
                        _selected = i;
                        _form.Invalidate();
                    }
                    break;
                }
            }
        }

        // ── HandleMouseClick — called by Form1.OnMouseClick
        public void HandleMouseClick(MouseEventArgs e)
        {
            if (_firstEnter) return; // Don't process clicks on intro screen


            for (int i = 0; i < _menuBounds.Length; i++)
            {
                if (_menuBounds[i].Contains(e.X, e.Y))
                {
                    _menuItems[i].OnSelect?.Invoke();
                    break;
                }
            }
        }
    }
}