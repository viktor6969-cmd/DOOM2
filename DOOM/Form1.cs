using System;
using System.Drawing;
using System.Drawing.Text;
using System.Media;
using System.Security.Policy;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;

namespace DOOM
{
    public enum GameScreen { MainMenu, NewGame, Options, LoadGame }

    public partial class Form1 : Form
    {
        // Fonts
        private PrivateFontCollection _fonts = new PrivateFontCollection();
        private Font _menuFont;

        // Images
        private Image _skull = Image.FromFile("Assets\\skull.png");
        private Image _background = Image.FromFile("Assets\\BG.jpg");

        // Music
        private SoundPlayer _music = new SoundPlayer();

        // Screen state
        private GameScreen _screen = GameScreen.MainMenu;

        // Intro screen vars
        private bool _firstEnter = true;
        private bool _blinkVisible = true;
        private System.Windows.Forms.Timer _blinkTimer;

        // Menu vars
        private int _selected = 0;
        private RectangleF[] _menuBounds = new RectangleF[4]; // To mesure the text borders
        private struct MenuItem
        {
            public string Text;
            public Action OnSelect;
        }
        private MenuItem[] _menuItems;




        // ----------------- Constructor ------------------- //
        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.DoubleBuffered = true;

            _blinkTimer = new System.Windows.Forms.Timer();
            _blinkTimer.Interval = 500;
            _blinkTimer.Tick += (s, e) => { _blinkVisible = !_blinkVisible; Invalidate(); };
            _blinkTimer.Start();

            _menuItems = new MenuItem[]
            {
                new MenuItem { Text = "Slay Demons", OnSelect = () => SetScreen(GameScreen.NewGame)  },
                new MenuItem { Text = "Drawing Board",   OnSelect = () => { 
                    var drawForm = new DrawForm(this);
                    drawForm.StartPosition = FormStartPosition.Manual;
                    drawForm.Location = this.Location;
                    PlayMusic("Assets\\menu.wav");
                    drawForm.Show();
                    this.Hide();
                }},
                new MenuItem { Text = "Options",     OnSelect = () => SetScreen(GameScreen.Options)  },
                new MenuItem { Text = "Quit Game",   OnSelect = () => Application.Exit() },
            };

            // Set the text font
            _fonts.AddFontFile("Assets\\DooM.ttf");
            _menuFont = new Font(_fonts.Families[0], 32, FontStyle.Regular);

            // Start with main menu music
            PlayMusic("Assets\\TheyScared.wav");
        }



        // ── Screen switcher ──────────────────────
        private void SetScreen(GameScreen newScreen)
        {
            if (_screen == newScreen) return; // already here, do nothing

            _screen = newScreen;

            Invalidate();
        }

        private void PlayMusic(string path)
        {
            _music.Stop();
            _music.SoundLocation = path;
            _music.PlayLooping();
        }

        // ── Paint ────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_firstEnter) { 
                DrawIntroScreen(e.Graphics);
                return;
            }

            switch (_screen)
            {
                case GameScreen.MainMenu: DrawMainMenu(e); break;
                case GameScreen.NewGame: DrawNewGame(e); break;
                case GameScreen.Options: DrawOptions(e); break;
                case GameScreen.LoadGame: DrawLoadGame(e); break;
            }
        }

        // ── Intro Screen ─────────────────────────
        private void DrawIntroScreen(Graphics g)
        {
            g.DrawImage(_background, 0, 0, ClientSize.Width, ClientSize.Height);

            if (_blinkVisible)
            {
                string text = "- PRESS ANY KEY -";
                float y = (this.ClientSize.Height - 200);
                float x = (this.ClientSize.Width / 12);


                g.DrawString(text, _menuFont, new SolidBrush(Color.FromArgb(20, 0, 0)), x + 6, y + 6);
                g.DrawString(text, _menuFont, new SolidBrush(Color.FromArgb(90, 0, 0)), x + 3, y + 3);
                g.DrawString(text, _menuFont, Brushes.Red,x,y);
            }
        }

        // ── Main Menu ────────────────────────────
        private void DrawMainMenu(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_background, 0, 0, this.ClientSize.Width, this.ClientSize.Height); // Draw the BG

            int startY = (this.ClientSize.Height / 2) - 40;

            for (int i = 0; i < _menuItems.Length; i++)
            {
                float y = startY + i * 60;
                SizeF size = e.Graphics.MeasureString(_menuItems[i].Text, _menuFont); // Messure the text itself 
                float x = (this.ClientSize.Width - size.Width) / 2;
                _menuBounds[i] = new RectangleF(x, y, size.Width, size.Height); // Saves the text border sizes (For mouse)

                e.Graphics.DrawString(_menuItems[i].Text, _menuFont, new SolidBrush(Color.FromArgb(20, 0, 0)), x + 6, y + 6);
                e.Graphics.DrawString(_menuItems[i].Text, _menuFont, new SolidBrush(Color.FromArgb(90, 0, 0)), x + 3, y + 3);

                Color col = (i == _selected) ? Color.Red : Color.FromArgb(155, 15, 15);
                e.Graphics.DrawString(_menuItems[i].Text, _menuFont, new SolidBrush(col), x, y);
            }

            // Skulls
            SizeF selectedSize = e.Graphics.MeasureString(_menuItems[_selected].Text, _menuFont);
            float textX = (this.ClientSize.Width - selectedSize.Width) / 2;
            float textY = startY + _selected * 60;

            e.Graphics.DrawImage(_skull, textX - 65, textY, 60, 60);
            e.Graphics.DrawImage(_skull, textX + selectedSize.Width + 5, textY, 60, 60);
        }


        // ── Other Screens ────────────────────────
        private void DrawNewGame(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_background, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            DrawScreenTitle(e, "- NEW GAME -");
            DrawBackHint(e);
        }

        private void DrawOptions(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_background, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            DrawScreenTitle(e, "- OPTIONS -");
            DrawBackHint(e);
        }

        private void DrawLoadGame(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_background, 0, 0, this.ClientSize.Width, this.ClientSize.Height);
            DrawScreenTitle(e, "- LOAD GAME -");
            DrawBackHint(e);
        }

        // ── Helpers ──────────────────────────────
        private void DrawScreenTitle(PaintEventArgs e, string title)  //!!!!!!!!
        {
            SizeF size = e.Graphics.MeasureString(title, _menuFont);
            float x = (this.ClientSize.Width - size.Width) / 2;
            float y = (this.ClientSize.Height - size.Height) - 450;

            e.Graphics.DrawString(title, _menuFont, new SolidBrush(Color.FromArgb(20, 0, 0)), x + 6, y + 6);
            e.Graphics.DrawString(title, _menuFont, new SolidBrush(Color.FromArgb(90, 0, 0)), x + 3, y + 3);
            e.Graphics.DrawString(title, _menuFont, Brushes.Red, x, y);
        }

        private void DrawBackHint(PaintEventArgs e)
        {
            using (var f = new Font("Arial", 12))
                e.Graphics.DrawString("Press ESC to go back", f,
                    new SolidBrush(Color.FromArgb(150, 150, 150)),
                    10, this.ClientSize.Height - 30);
        }

        // ── Events ────────────────────────────────
        protected override void OnKeyDown(KeyEventArgs e)
        {
            //Exit the Intro screen 
            if (_firstEnter)
            {
                _firstEnter = false;
                _blinkTimer.Stop();
                _blinkTimer.Dispose();
                Invalidate();
                return;
            }

            if (_screen != GameScreen.MainMenu)
            {
                if (e.KeyCode == Keys.Escape)
                    SetScreen(GameScreen.MainMenu);
                return;
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
                _selected = (_selected - 1 + _menuItems.Length) % _menuItems.Length;

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
                _selected = (_selected + 1) % _menuItems.Length;

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
                _menuItems[_selected].OnSelect?.Invoke();

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            for (int i = 0; i < _menuBounds.Length; i++)
            {
                if (_menuBounds[i].Contains(e.X, e.Y))
                {
                    if (_selected != i) // only redraw if selection changed
                    {
                        _selected = i;
                        Invalidate();
                    }
                    break;
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_firstEnter || _screen != GameScreen.MainMenu) return; // Don't process if not on this screen
            if (_screen != GameScreen.MainMenu) return;

            for (int i = 0; i < _menuBounds.Length; i++)
            {
                if (_menuBounds[i].Contains(e.X, e.Y))
                {
                    _menuItems[i].OnSelect?.Invoke();
                    break;
                }
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible) // just became visible
                PlayMusic("Assets\\TheyScared.wav");
        }
        //protected override void OnFormClosed(FormClosedEventArgs e)
        //{
        //    base.OnFormClosed(e);
        //    Application.OpenForms[0].Show(); // bring menu back
        //}

    }
}