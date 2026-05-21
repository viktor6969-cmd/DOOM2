using System;
using System.Media;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace DOOM
{
    // ── MenuLogic ────────────────────────────────────────────────────────────
    public class MenuItem
    {
        public string Text { get; private set; }
        public RectangleF Bounds { get; set; }
        public Image Image { get; private set; }  // the menu item texture
        public Action OnSelect { get; private set; }
        public MenuItem(string text, string texturePath, Action onSelect,float x, float y)
        {
            Text = text;
            Image = Image.FromFile(texturePath);
            OnSelect = onSelect;
            Bounds = new RectangleF(x, y, 350, 50);
        }
    }

    public struct Song
    {
        public string Name { get; private set; }
        public string Path { get; private set; }
        public Song(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }

    public class MenuLogic
    {
        private Screen _form; 


        // Images
        private Image _skull;
        private Image _mainBackground;
        private Image _introBackground;
        private Image _doomLogo;
        private Image _introStartButton;

        // Intro screen vars
        private bool _blinkVisible = true;
        private System.Windows.Forms.Timer _blinkTimer;
        
        // Menu vars
        private int _selected = 0;
        private const int ItemW = 350;
        private const int ItemH = 50;
        private const int ItemGap = 60;
        private MenuItem[] _mainMenuItems;
        private MenuItem[] _optionsMenuItems;

        // Options state
        private bool _inOptions = false;
        private int _optionSelected = 0;

        // Music 
        private bool _musicOn = true;
        private SoundPlayer _music;
        private Song[] _songs;

        // ----------------- Constructor ------------------- //
        public MenuLogic(Screen form)
        {
            _form = form;

            _skull            = Image.FromFile("Assets\\MainMenu\\m_skull1.png");
            _mainBackground   = Image.FromFile("Assets\\MainMenu\\w94_1.png");
            _introBackground  = Image.FromFile("Assets\\MainMenu\\titlepic.png");
            _doomLogo         = Image.FromFile("Assets\\MainMenu\\m_doom.png");
            _introStartButton = Image.FromFile("Assets\\MainMenu\\presstostart.png");


            float x = (form.ClientSize.Width / 2) - (350 / 2);
            int startY = (form.ClientSize.Height / 2) - 40;
            int itemGap = 60;
            _mainMenuItems = new MenuItem[]{
                new MenuItem("New Game",    "Assets\\MainMenu\\m_newg.png",  () => { PlayMusic(_songs[1].Path); _form.GoToGame(); }, x, startY),
                new MenuItem("LoadGame",        "Assets\\MainMenu\\m_lgttl.png", () => _form.GoToGame(), x, startY + itemGap),
                new MenuItem("Options",        "Assets\\MainMenu\\m_optttl.png", () => { _inOptions = true; _form.Invalidate(); }, x, startY + 2 * itemGap),
                new MenuItem("Quit Game",      "Assets\\MainMenu\\m_endgam.png",  () => Application.Exit(), x, startY + 3 * itemGap),
             };

            _optionsMenuItems = new MenuItem[]{
                new MenuItem("Sound :  < ON >", "Assets\\MainMenu\\m_optttl.png", null, x, startY),
                new MenuItem("Sound :  < OFF >", "Assets\\MainMenu\\m_optttl.png", null, x, startY + itemGap),
                new MenuItem("Back",            "Assets\\MainMenu\\m_endgam.png", () => { _inOptions = false; _form.Invalidate(); }, x, startY + 2 * itemGap),
            };

            // Music
            _music = new SoundPlayer();
            _songs = new Song[]{
                 new Song("Menu Music", "Assets\\Music\\TheyScared.wav"),
                 new Song("Game Music", "Assets\\Music\\in_game.wav")
            };

            // Start intro blink loop
            _blinkTimer = new System.Windows.Forms.Timer();
            _blinkTimer.Interval = 500;
            _blinkTimer.Tick += (s, e) => { _blinkVisible = !_blinkVisible; _form.Invalidate(); };
            _blinkTimer.Start();

            PlayMusic(_songs[0].Path);
        }

        // ── Draw — called by Form1.OnPaint ────────────────
        public void Draw(Graphics g, Size clientSize)
        {
            if (_blinkTimer != null && _blinkTimer.Enabled) { DrawIntroScreen(g, clientSize); return;}

            if (_inOptions) { DrawOptions(g, clientSize); return; }

            else DrawMainMenu(g, clientSize);
        }

        // ── Intro Screen ──────────────────────────────────
        private void DrawIntroScreen(Graphics g, Size clientSize)
        {
            g.DrawImage(_introBackground, 0, 0, clientSize.Width, clientSize.Height);

            if (_blinkVisible)
            {
                g.DrawImage(_introStartButton, (clientSize.Width - _introStartButton.Width) / 2, (clientSize.Height - _introStartButton.Height) - 100, _introStartButton.Width, _introStartButton.Height);
            }
        }

        // ── Main Menu ─────────────────────────────────────
        private void DrawMainMenu(Graphics g, Size clientSize)
        {
            g.DrawImage(_mainBackground, 0, 0, clientSize.Width, clientSize.Height);
            g.DrawImage(_doomLogo, 150 , 60,525, 200);
            for (int i = 0; i < _mainMenuItems.Length; i++)
                 g.DrawImage(_mainMenuItems[i].Image, _mainMenuItems[i].Bounds);


            float selectedY = _mainMenuItems[_selected].Bounds.Y;
            g.DrawImage(_skull, _mainMenuItems[_selected].Bounds.X - 80, selectedY - 2, 60, 60);
            g.DrawImage(_skull, _mainMenuItems[_selected].Bounds.X + _mainMenuItems[_selected].Bounds.Width + 20, selectedY - 2, 60, 60);
            PlayMusic(_songs[0].Path);
        }

        // ── Options Screen ────────────────────────────────
        private void DrawOptions(Graphics g, Size clientSize)
        {
            
        }

        // ── PlayMusic ------------------------------───────
        public void PlayMusic(string path)
        {
            if (!_musicOn) return;
            if(_music.SoundLocation == path) return;

            _music.Stop();
            _music.SoundLocation = path;
            _music.PlayLooping();
            
        }

        // ── HandleKeys — called by Form1.OnKeyDown ────────
        public void HandleKeys(KeyEventArgs e)
        {
            // Exit intro screen on any key press
            if (_blinkTimer != null && _blinkTimer.Enabled)
            {
                _blinkTimer.Stop();
                _blinkTimer.Dispose();
                _blinkTimer = null;
                _form.Invalidate();
                return;
            }

            // ── Options screen ────────────────────────────
            if (_inOptions)
            {
                switch (e.KeyCode)
                {
                    case Keys.W:      _optionSelected = (_optionSelected - 1 + _optionsMenuItems.Length) % _optionsMenuItems.Length; break;

                    case Keys.S:      _optionSelected = (_optionSelected + 1) % _optionsMenuItems.Length;                            break;
                            
                    case Keys.Space:  _optionsMenuItems[_optionSelected].OnSelect.Invoke();                                          break;

                    case Keys.Escape: _inOptions = false;                                                                            break;

                }
                _form.Invalidate();
                return;
            }

            // ── Main menu ─────────────────────────────────
            switch (e.KeyCode)
            {
                case Keys.W:     _selected = (_selected - 1 + _mainMenuItems.Length) % _mainMenuItems.Length; break;

                case Keys.S:     _selected = (_selected + 1) % _mainMenuItems.Length;                         break;

                case Keys.Space: _mainMenuItems[_selected].OnSelect.Invoke();                                 break;
            }

            _form.Invalidate();
        }

        // ── HandleMouseMove — called by Form1.OnMouseMove ─
        public void HandleMouseMove(MouseEventArgs e)
        {
            for (int i = 0; i < _mainMenuItems.Length; i++)
            {
                if (_mainMenuItems[i].Bounds.Contains(e.X, e.Y))
                {
                    if (_selected != i) 
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
            if(_blinkTimer != null && _blinkTimer.Enabled) return;


            for (int i = 0; i < _mainMenuItems.Length; i++)
            {
                if (_mainMenuItems[i].Bounds.Contains(e.X, e.Y))
                {
                    _mainMenuItems[i].OnSelect.Invoke();
                    break;
                }
            }
        }
    }
}