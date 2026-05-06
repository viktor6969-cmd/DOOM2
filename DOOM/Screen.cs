using System;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace DOOM
{
    public partial class Screen : Form
    {
        private MenuLogic _menu;
        private GameLogic _game;
        private bool _inGame = false;


        private SoundPlayer _music = new SoundPlayer();
        private string _music_menu = "Assets\\Music\\TheyScared.wav";
        private string _music_game = "Assets\\Music\\in_game.wav";
        public Screen()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.DoubleBuffered = true;

            // Create both logic classes — pass this form so they can call back
            _menu = new MenuLogic(this);
            _game = new GameLogic(this);
        }

        public void GoToGame()
        {
            _inGame = true;
            _game.Start();      // start the game loop
            PlayMusic(_music_game);
            Invalidate();
        }

        // ── Switch to Menu ───────────────────────────────
        public void GoToMenu()
        {
            _inGame = false;
            _game.Stop();       // stop the game loop
            PlayMusic(_music_menu);
            Invalidate();
        }

        // ── Shared: play music ───────────────────────────
        public void PlayMusic(string path)
        {
            _music.Stop();
            _music.SoundLocation = path;
            _music.PlayLooping();
        }

        // ── Paint — who's in charge draws ────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_inGame)
                _game.Draw(e.Graphics, ClientSize);
            else
                _menu.Draw(e.Graphics, ClientSize);
        }

        // ── Keys — who's in charge handles ───────────────
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_inGame)
                _game.HandleKeys(e);
            else
                _menu.HandleKeys(e);
        }

        // ── Mouse move — who's in charge handles ─────────
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_inGame)
                _game.HandleMouseMove(e);
            else
                _menu.HandleMouseMove(e);
        }

        // ── Mouse click — who's in charge handles ────────
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_inGame)
                _game.HandleMouseClick(e);
            else
                _menu.HandleMouseClick(e);
        }

        // ── Restore menu music when form becomes visible ─
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible && !_inGame)
                PlayMusic(_music_menu);
        }

        private void Screen_Load(object sender, EventArgs e)
        {

        }
    }
}
