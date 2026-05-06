using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DOOM
{
    public class GameLogic
    {
        private Screen _form;
        private System.Windows.Forms.Timer _gameLoop;

        // ── E1M1 — Hangar (starting area) ────────────────
        // 0 = empty, 1/2/3/4 = wall texture ID
        private int[,] _map =
        {
            { 2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,1,1,1,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,2 },
            { 2,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,2 },
            { 2,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,3,3,3,3,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,3,0,0,3,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,3,0,0,3,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,1,0,0,0,0,0,3,0,0,3,0,0,0,0,1,0,0,0,0,0,2 },
            { 2,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,2 },
            { 2,0,0,1,1,1,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,2 },
            { 2,0,0,0,0,0,4,4,4,4,0,0,4,4,4,4,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,4,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,4,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,4,0,0,0,0,0,0,0,0,4,0,0,0,0,1,1,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,2,2,2,0,2,2,2,2,2,2,2,2,2,2,2,2,2,0,2,2,2,2,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2 },
            { 2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2 },
        };

        // ── Player ────────────────────────────────────────
        private float _playerX = 2.5f;
        private float _playerY = 2.5f;
        private float _playerAngle = 0f;
        private float _moveSpeed = 0.08f;
        private float _turnSpeed = 0.04f;

        // ── Held keys ─────────────────────────────────────
        private bool _keyW, _keyS, _keyA, _keyD;

        // ── Wall textures array (one per texture ID) ──────
        private int[][] _wallPixelsArr;
        private int[] _wallWidths;
        private int[] _wallHeights;

        // ── Floor / ceiling textures ──────────────────────
        private int[] _floorPixels;
        private int[] _ceilPixels;
        private int _floorW, _floorH;
        private int _ceilW, _ceilH;

        // ── Raycaster settings ────────────────────────────
        private const double FOV = Math.PI / 3; // 60 degrees

        // ----------------- Constructor ------------------- //
        public GameLogic(Screen form)
        {
            _form = form;

            // ── Load wall textures — SW19_1 through SW19_4 ─
            string[] wallFiles =
            {
                "Original_assets\\Textures\\Walls\\SW19_1.png",
                "Original_assets\\Textures\\Walls\\SW19_2.png",
                "Original_assets\\Textures\\Walls\\SW19_3.png",
                "Original_assets\\Textures\\Walls\\SW19_4.png",
            };

            _wallPixelsArr = new int[wallFiles.Length][];
            _wallWidths = new int[wallFiles.Length];
            _wallHeights = new int[wallFiles.Length];

            for (int i = 0; i < wallFiles.Length; i++)
            {
                var bmp = new Bitmap(wallFiles[i]);
                _wallPixelsArr[i] = LockTexture(bmp, out _wallWidths[i], out _wallHeights[i]);
            }

            // ── Load floor and ceiling textures ────────────
            var floorBmp = new Bitmap("Original_assets\\Textures\\Walls\\floor4_8.png");
            var ceilBmp = new Bitmap("Original_assets\\Textures\\Walls\\ceil3_5.png");
            _floorPixels = LockTexture(floorBmp, out _floorW, out _floorH);
            _ceilPixels = LockTexture(ceilBmp, out _ceilW, out _ceilH);

            // ── Game loop ~60fps ────────────────────────────
            _gameLoop = new System.Windows.Forms.Timer();
            _gameLoop.Interval = 16;
            _gameLoop.Tick += (s, e) => { Update(); _form.Invalidate(); };
        }

        // ── Load texture pixels into int[] via LockBits ───
        private int[] LockTexture(Bitmap bmp, out int width, out int height)
        {
            width = bmp.Width;
            height = bmp.Height;
            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            int[] pixels = new int[width * height];
            Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
            bmp.UnlockBits(data);
            return pixels;
        }

        // ── Start / Stop ──────────────────────────────────
        public void Start()
        {
            _playerX = 2.5f;
            _playerY = 2.5f;
            _playerAngle = 0f;
            _gameLoop.Start();
        }

        public void Stop()
        {
            _gameLoop.Stop();
            _keyW = _keyS = _keyA = _keyD = false;
        }

        // ── Update — runs every tick ──────────────────────
        private void Update()
        {
            float dx = 0, dy = 0;

            // W/S = move forward/backward
            if (_keyW)
            {
                dx += (float)Math.Cos(_playerAngle) * _moveSpeed;
                dy += (float)Math.Sin(_playerAngle) * _moveSpeed;
            }
            if (_keyS)
            {
                dx -= (float)Math.Cos(_playerAngle) * _moveSpeed;
                dy -= (float)Math.Sin(_playerAngle) * _moveSpeed;
            }

            // A/D = turn left/right
            if (_keyA) _playerAngle -= _turnSpeed;
            if (_keyD) _playerAngle += _turnSpeed;

            // Collision X and Y separately (allows wall sliding)
            float nextX = _playerX + dx;
            if (_map[(int)_playerY, (int)(nextX + (dx >= 0 ? 0.25f : -0.25f))] == 0)
                _playerX = nextX;

            float nextY = _playerY + dy;
            if (_map[(int)(nextY + (dy >= 0 ? 0.25f : -0.25f)), (int)_playerX] == 0)
                _playerY = nextY;
        }

        // ── Draw — called by Form1.OnPaint ────────────────
        public void Draw(Graphics g, Size clientSize)
        {
            int W = clientSize.Width;
            int H = clientSize.Height;

            using (Bitmap frame = new Bitmap(W, H, PixelFormat.Format32bppArgb))
            {
                BitmapData frameData = frame.LockBits(
                    new Rectangle(0, 0, W, H),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                int[] pixels = new int[W * H];

                // ── Textured floor and ceiling ────────────
                for (int y = 0; y < H; y++)
                {
                    bool isCeiling = y < H / 2;
                    float rowDist = (H / 2.0f) / Math.Abs(y - H / 2.0f + 0.0001f);

                    float rayDirX0 = (float)Math.Cos(_playerAngle - FOV / 2.0);
                    float rayDirY0 = (float)Math.Sin(_playerAngle - FOV / 2.0);
                    float rayDirX1 = (float)Math.Cos(_playerAngle + FOV / 2.0);
                    float rayDirY1 = (float)Math.Sin(_playerAngle + FOV / 2.0);

                    float stepX = rowDist * (rayDirX1 - rayDirX0) / W;
                    float stepY = rowDist * (rayDirY1 - rayDirY0) / W;
                    float floorX = _playerX + rowDist * rayDirX0;
                    float floorY = _playerY + rowDist * rayDirY0;

                    for (int x = 0; x < W; x++)
                    {
                        if (isCeiling)
                        {
                            float ceilFloorX = 2 * _playerX - floorX;
                            float ceilFloorY = 2 * _playerY - floorY;
                            int tx = Math.Abs((int)(ceilFloorX * _ceilW)) % _ceilW;
                            int ty = Math.Abs((int)(ceilFloorY * _ceilH)) % _ceilH;

                            Color c = Color.FromArgb(_ceilPixels[ty * _ceilW + tx]);
                            pixels[y * W + x] = Color.FromArgb(
                                (int)(c.R * 0.6f),
                                (int)(c.G * 0.6f),
                                (int)(c.B * 0.6f)).ToArgb();
                        }
                        else
                        {
                            int tx = Math.Abs((int)(floorX * _floorW)) % _floorW;
                            int ty = Math.Abs((int)(floorY * _floorH)) % _floorH;

                            Color c = Color.FromArgb(_floorPixels[ty * _floorW + tx]);
                            pixels[y * W + x] = Color.FromArgb(
                                (int)(c.R * 0.7f),
                                (int)(c.G * 0.7f),
                                (int)(c.B * 0.7f)).ToArgb();
                        }

                        floorX += stepX;
                        floorY += stepY;
                    }
                }

                // ── Cast one ray per screen column ────────
                for (int col = 0; col < W; col++)
                {
                    double rayAngle = (_playerAngle - FOV / 2.0) + ((double)col / W) * FOV;
                    double rayDirX = Math.Cos(rayAngle);
                    double rayDirY = Math.Sin(rayAngle);

                    double dist = 0;
                    bool hitWall = false;
                    int mapX = 0, mapY = 0;

                    while (!hitWall && dist < 20)
                    {
                        dist += 0.02;
                        mapX = (int)(_playerX + rayDirX * dist);
                        mapY = (int)(_playerY + rayDirY * dist);

                        if (mapX < 0 || mapX >= _map.GetLength(1) ||
                            mapY < 0 || mapY >= _map.GetLength(0))
                        { hitWall = true; dist = 20; }
                        else if (_map[mapY, mapX] != 0)
                            hitWall = true;
                    }

                    // Fix fisheye
                    dist *= Math.Cos(rayAngle - _playerAngle);

                    // Wall slice height
                    int wallH = Math.Min(H, (int)(H / dist));
                    int wallTop = (H - wallH) / 2;
                    int wallBottom = wallTop + wallH;

                    // ── Pick texture based on map cell value ──
                    int texID = _map[mapY, mapX] - 1; // 1→0, 2→1, 3→2, 4→3
                    texID = Math.Max(0, Math.Min(_wallPixelsArr.Length - 1, texID));

                    int tw = _wallWidths[texID];
                    int th = _wallHeights[texID];

                    // Texture X coordinate
                    double hitX = (_playerX + rayDirX * dist) % 1.0;
                    double hitY = (_playerY + rayDirY * dist) % 1.0;
                    int texX = (int)((Math.Abs(hitX) > Math.Abs(hitY) ? hitX : hitY) * tw);
                    texX = Math.Abs(texX) % tw;

                    // Draw wall slice
                    for (int row = Math.Max(0, wallTop); row < Math.Min(H, wallBottom); row++)
                    {
                        int texY = (int)((double)(row - wallTop) / wallH * th);
                        texY = Math.Max(0, Math.Min(th - 1, texY));

                        Color c = Color.FromArgb(_wallPixelsArr[texID][texY * tw + texX]);

                        // Shade by distance — far walls darker
                        float shade = Math.Max(0.2f, 1f - (float)(dist / 8f));
                        pixels[row * W + col] = Color.FromArgb(
                            (int)(c.R * shade),
                            (int)(c.G * shade),
                            (int)(c.B * shade)).ToArgb();
                    }
                }

                Marshal.Copy(pixels, 0, frameData.Scan0, pixels.Length);
                frame.UnlockBits(frameData);
                g.DrawImage(frame, 0, 0);
            }

            // ── Minimap ───────────────────────────────────
            DrawMinimap(g);

            // ── Controls hint ─────────────────────────────
            using (var f = new Font("Arial", 10))
                g.DrawString("WASD   ESC = Menu",
                    f, Brushes.White, 10, clientSize.Height - 22);
        }

        // ── Minimap ───────────────────────────────────────
        private void DrawMinimap(Graphics g)
        {
            int scale = 10;
            int rows = _map.GetLength(0);
            int cols = _map.GetLength(1);

            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                {
                    // Different colors per wall type on minimap
                    Color c;
                    switch (_map[y, x])
                    {
                        case 0: c = Color.FromArgb(30, 30, 30); break;
                        case 1: c = Color.Gray; break;
                        case 2: c = Color.DimGray; break;
                        case 3: c = Color.SlateGray; break;
                        default: c = Color.DarkGray; break;
                    }
                    g.FillRectangle(new SolidBrush(c),
                        x * scale, y * scale, scale - 1, scale - 1);
                }

            // Player dot
            g.FillEllipse(Brushes.Red,
                (int)(_playerX * scale) - 3,
                (int)(_playerY * scale) - 3,
                6, 6);
        }

        // ── Input ─────────────────────────────────────────
        public void HandleKeys(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { _form.GoToMenu(); return; }
            if (e.KeyCode == Keys.W) _keyW = true;
            if (e.KeyCode == Keys.S) _keyS = true;
            if (e.KeyCode == Keys.A) _keyA = true;
            if (e.KeyCode == Keys.D) _keyD = true;
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W) _keyW = false;
            if (e.KeyCode == Keys.S) _keyS = false;
            if (e.KeyCode == Keys.A) _keyA = false;
            if (e.KeyCode == Keys.D) _keyD = false;
        }

        public void HandleMouseMove(MouseEventArgs e) { }
        public void HandleMouseClick(MouseEventArgs e) { }
    }
}