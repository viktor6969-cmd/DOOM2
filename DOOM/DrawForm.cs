using System;
using System.Drawing;
using System.Windows.Forms;

namespace DOOM
{
    public partial class DrawForm : Form
    {

        private Form _owner;
        private Color _currentColor = Color.Black;

        private readonly Color[] _palette = {
            Color.Black,      Color.White,
            Color.Gray,       Color.Silver,
            Color.DarkRed,    Color.Red,
            Color.Orange,     Color.Yellow,
            Color.DarkGreen,  Color.Green,
            Color.Teal,       Color.Cyan,
            Color.DarkBlue,   Color.Blue,
            Color.Purple,     Color.Magenta,
            Color.Brown,      Color.Pink
        };

        // -------- Constructor -----------// 
        public DrawForm(Form owner)
        {
            InitializeComponent();
            _owner = owner;
            this.KeyPreview = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.DoubleBuffered = true;
            canvas.Cursor = Cursors.Cross;
            BuildPalette();
        }
        public abstract class Shape
        {
            public Color Color { get; set; } = Color.Black;
            public bool IsSelected { get; set; } = false;

            public abstract void Draw(Graphics g);
            public abstract bool Contains(Point p);  // for mouse selection
            public abstract void Move(int dx, int dy); // for dragging
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _owner.Location = this.Location;
            _owner.Show();
        }

        private void BuildPalette()
        {

            int swatchSize = 35;
            int x = 20; // leave room for current color box on the left

            foreach (var color in _palette)
            {
                var c = color; // capture
                var swatch = new Panel
                {
                    Size = new Size(swatchSize, swatchSize),
                    Location = new Point(x, 8),
                    BackColor = color,
                    BorderStyle = BorderStyle.Fixed3D,
                    Cursor = Cursors.Hand
                };
                swatch.Click += (s, e) => {
                    _currentColor = c;
                   // currentColorBox.BackColor = c; // update the preview box
                };
                colorPlate.Controls.Add(swatch);
                x += swatchSize + 2;
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {

        }

        private void backToTheGameToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
