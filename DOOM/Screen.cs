using System;
using System.Drawing;
using System.Windows.Forms;

namespace DOOM
{
    
    public partial class Screen : Form
    {
        private MenuLogic _menu;
        private GameLogic _game;
        private bool _inGame = false;
        private bool _gameOver = false;

        public Screen()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.DoubleBuffered = true;

            _menu = new MenuLogic(this);
            _game = new GameLogic(this);
        }

        public void GoToGame()
        {
            _inGame = true;
            _game.Start();
            _menu.PlayMusic(1);
            Cursor.Hide();
            Invalidate();
        }
        public void GoToMenu()
        {
            _inGame = false;
            Cursor.Show();
            _menu.PlayMusic(0);
            _game.Stop();       // stop the game loop
            Invalidate();
        }
        public void GoToGameOver()
        {
            _inGame = false;
            _gameOver = true;
            _game.Stop();
            Cursor.Show();
            Invalidate();
        }

        public void LoadGame()
        {
            _game.LoadGame();
        }
     

        // ── Paint — who's in charge draws ────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
             
            if (_inGame)  _game.Draw(e.Graphics, ClientSize);

            else if (_gameOver) _menu.DrawGameOver(e.Graphics, ClientSize);

            else  _menu.Draw(e.Graphics, ClientSize);
        }

        // ── Keys — who's in charge handles ───────────────
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_gameOver) { _gameOver = false; Invalidate(); _menu.PlayMusic(0); return; }
            if (_inGame) _game.HandleKeys(e);
            else _menu.HandleKeys(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (_inGame)
                _game.HandleKeyUp(e);
        }


        // ── Mouse  — who's in charge handles ----─────────
        protected override void OnMouseMove(MouseEventArgs e)
        {
                _menu.HandleMouseMove(e);
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (_inGame)
                return;

            _menu.HandleMouseClick(e);
        }

        private void Screen_Load(object sender, EventArgs e)
        {

        }
    }
}
