using System.Drawing;
using System.Windows.Forms;

namespace DOOM
{
    // ── GameLogic ────────────────────────────────────────────────────────────
    // Bare minimum stub — no functionality yet.
    // Just enough so Form1 and MenuLogic can compile and run.
    public class GameLogic
    {
        private Screen _form;
        private System.Windows.Forms.Timer _gameLoop;

        // ----------------- Constructor ------------------- //
        public GameLogic(Screen form)
        {
            _form = form;

            // Game loop — wired up but does nothing yet
            _gameLoop = new System.Windows.Forms.Timer();
            _gameLoop.Interval = 16;
            _gameLoop.Tick += (s, e) => _form.Invalidate();
        }

        // ── Start / Stop ──────────────────────────────────
        public void Start() => _gameLoop.Start();
        public void Stop() => _gameLoop.Stop();

        // ── Draw ──────────────────────────────────────────
        public void Draw(Graphics g, Size clientSize)
        {
            g.Clear(System.Drawing.Color.Black); // black screen for now
        }

        // ── Input ─────────────────────────────────────────
        public void HandleKeys(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                _form.GoToMenu(); // ESC always works
        }

        public void HandleKeyUp(KeyEventArgs e) { }
        public void HandleMouseMove(MouseEventArgs e) { }
        public void HandleMouseClick(MouseEventArgs e) { }
    }
}