using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace DOOM
{

    public abstract class Entity
    {
        public string Name { get; protected set; } // Anyone can read and get, only sons can set 
        public abstract string TexturePath { get; protected set; }

        protected Entity(string name)
        {
            Name = name;
        }
    }
    public abstract class Build : Entity
    {
        public override string TexturePath { get; protected set; }
        public int Weight { get; protected set; }
        public int Height { get; protected set; }

        protected Build(string name, int weight, int height, string texturePath)
            : base(name)
        {
            Weight = weight;
            Height = height;
            TexturePath = texturePath;
        }
    }
    public class Weapon : Entity
    {
        public override string TexturePath { get; protected set; }
        public int Damage { get; protected set; }
        public int MaxAmmo { get; protected set; }
        public int AmmoCount { get; protected set; }

        

        public Weapon(string name, int damage, int ammo, string texture)
            : base(name)
        {
            TexturePath = texture;
            Damage = damage;
            AmmoCount = ammo;
        }

        public void Shoot()
        {
            if (AmmoCount > 0)
            {
                AmmoCount--;
                Console.WriteLine("Bang! Ammo left: " + AmmoCount);
            }
        }
        public void Reload(){

        }
        public virtual string GetInfo()
        {
            return $"{Name} | Damage: {Damage} | Ammo: {AmmoCount}";
        }
    }


    public class Player : Entity
    {
        // ── Position ──────────────────────────────────
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }

        // ── Texture ────────────────────────────────────
        public override string TexturePath { get; protected set; }
      
        // ── Weapon collection ─────────────────────────
        public List<Weapon> Weapons { get; private set; }
        public Weapon CurrentWeapon { get; private set; }

        // ── Constructor ───────────────────────────────
        public Player(float p_x, float p_y) : base("Player1")
        {
            X = p_x;
            Y = p_y;
            Angle = 0;
            TexturePath = "Original_assets\\Textures\\Weapons\\pisga0.png";
            // Start with both weapons
            Weapons = new List<Weapon>
            {
                new Weapon("Pistol", 10, 15, "Original_assets\\Textures\\Weapons\\pisga0.png"),
                new Weapon("Shotgun", 25, 8, "Original_assets\\Textures\\Weapons\\shtga0.png"),
            };


            // Default weapon is Pistol
            CurrentWeapon = Weapons[0];
        }

        // -- Move player to x,y -------------------------
        public void MovePlayer(float x, float y)
        {
            X += x;
            Y += y;
        }

        // ── Switch weapon by index ─────────────────────
        public void SwitchWeapon(int index)
        {
            if (index >= 0 && index < Weapons.Count)
                CurrentWeapon = Weapons[index];
        }

        // ── Cycle to next weapon ───────────────────────
        public void NextWeapon()
        {
            int next = (Weapons.IndexOf(CurrentWeapon) + 1) % Weapons.Count;
            CurrentWeapon = Weapons[next];
        }
    }
    public class Wall : Build
    {
        private int[] _pixels;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Wall(string texturePath, string name)
            : base(name, 100, 128, texturePath)
        {
            Bitmap bmp = new Bitmap(texturePath);

            Width = bmp.Width;
            Height = bmp.Height;

            _pixels = new int[Width * Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _pixels[y * Width + x] = bmp.GetPixel(x, y).ToArgb();
                }
            }

            bmp.Dispose();
        }

        public int GetPixelColor(int x, int y)
        {
            return _pixels[y * Width + x];
        }
    }

    public struct RayHit
    {
        public double Distance;
        public double HitX;
        public double HitY;
        public bool HitVertical;

        public RayHit(double distance, double hitX, double hitY, bool hitVertical)
        {
            Distance = distance;
            HitX = hitX;
            HitY = hitY;
            HitVertical = hitVertical;
        }
    }

    public class Render
    {
        public const int ScreenW = 640;
        public const int ScreenH = 400;


        private int[] _pixels;
        private Wall _wall;
        private Bitmap _frame;

        private const double FOV = Math.PI / 3.0; // 60 deg, view

        public Render()
        {
            _frame = new Bitmap(ScreenW, ScreenH, PixelFormat.Format32bppArgb);
            _pixels = new int[ScreenW * ScreenH];

            // Load textures
            _wall = new Wall("Original_assets\\Textures\\Walls\\sw19_1.png","Wall1");
          
        }

        public Bitmap DrawFrame(Player player, int[,] map)
        { 
            Array.Clear(_pixels, 0, _pixels.Length);

            for (int screenX = 0; screenX < ScreenW; screenX++)
            {
                // Calculate ray angle for this column
                double rayAngle = player.Angle - FOV / 2 + (screenX / (double)ScreenW) * FOV;

                // Cast ray and get distance to wall
                RayHit hit = CastRay(player.X, player.Y, rayAngle, map);

                // ceiling + textured wall + floor
                DrawColumn(screenX,hit,rayAngle,player.Angle);

            }

            DrawGun(player);

            CopyPixelsToBitmap();
            return _frame;
        }

        private void DrawColumn(int screenX, RayHit hit, double rayAngle, double playerAngle)
        {
            // Simple colors for ceiling and floor
            int ceilingColor = Color.SlateGray.ToArgb();
            int floorColor = Color.DarkGray.ToArgb();

            // Correct distance for fish-eye effect
            double distance = hit.Distance * Math.Cos(rayAngle - playerAngle);
            if (distance < 0.0001) distance = 0.0001;

            // Calculate wall height on screen
            int wallHeight = (int)(ScreenH / distance);
            int wallStartRaw = (ScreenH / 2) - (wallHeight / 2);
            int wallEndRaw = (ScreenH / 2) + (wallHeight / 2);
            int wallStart = Math.Max(wallStartRaw, 0);
            int wallEnd = Math.Min(wallEndRaw, ScreenH - 1);

            // Calculate texture X coordinate
            double wallOffset = hit.HitVertical
                ? hit.HitY - Math.Floor(hit.HitY)
                : hit.HitX - Math.Floor(hit.HitX);

            // If the ray hit a vertical wall, we use the Y offset; otherwise, we use the X offset.
            int texX = (int)(wallOffset * _wall.Width);
            texX = Math.Max(0, Math.Min(texX, _wall.Width - 1));

            // Draw ceiling
            for (int i = 0; i < wallStart; i++)
                _pixels[i * ScreenW + screenX] = ceilingColor;

            // Draw wall
            for (int y = wallStart; y < wallEnd; y++)
            {
                double texPercent = (double)(y - wallStartRaw) / wallHeight;
                int texY = (int)(texPercent * _wall.Height);
                texY = Math.Max(0, Math.Min(texY, _wall.Height - 1));

                _pixels[y * ScreenW + screenX] = _wall.GetPixelColor(texX, texY);
            }

            // Draw floor
            for (int y = wallEnd; y < ScreenH; y++)
                _pixels[y * ScreenW + screenX] = floorColor;
        }


        private RayHit CastRay(float px, float py, double angle, int[,] map)
        {
            // Ray direction
            double dirX = Math.Cos(angle);
            double dirY = Math.Sin(angle);

            // Current grid cell
            int mapX = (int)px;
            int mapY = (int)py;

            // Calculate distance to next grid lines
            double deltaX = Math.Abs(1.0 / dirX);
            double deltaY = Math.Abs(1.0 / dirY);

            // Calculate step direction
            int stepX = dirX < 0 ? -1 : 1;
            int stepY = dirY < 0 ? -1 : 1;

            // Calculate initial side distances
            double sideX = dirX < 0 
                ? (px - mapX) * deltaX 
                : (mapX + 1.0 - px) * deltaX;

            double sideY = dirY < 0 
                ? (py - mapY) * deltaY 
                : (mapY + 1.0 - py) * deltaY;

            bool hitVertical = false;

            // DDA loop to find wall hit
            while (map[mapY, mapX] == 0)
            {
                if (sideX < sideY)
                {
                    sideX += deltaX;
                    mapX += stepX;
                    hitVertical = true;
                }
                else
                {
                    sideY += deltaY;
                    mapY += stepY;
                    hitVertical = false;
                }
            }

            // Calculate distance to wall hit
            double dist = hitVertical
                ? sideX - deltaX
                : sideY - deltaY;

            // Calculate exact hit position
            double hitX = px + dirX * dist;
            double hitY = py + dirY * dist;

            return new RayHit(dist, hitX, hitY, hitVertical);
        }

        private void CopyPixelsToBitmap()
        {
            Rectangle rect = new Rectangle(0, 0, ScreenW, ScreenH);

            BitmapData data = _frame.LockBits(
                rect,
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb
            );

            Marshal.Copy(_pixels, 0, data.Scan0, _pixels.Length);

            _frame.UnlockBits(data);
        }

        public void DrawGun(Player player)
        {
            Bitmap gunTexture = new Bitmap(player.CurrentWeapon.TexturePath);
            int gunW = gunTexture.Width;
            int gunH = gunTexture.Height;
            int posX = (ScreenW - gunW) / 2;
            int posY = ScreenH - gunH - 10;
            using (Graphics g = Graphics.FromImage(_frame))
            {
                g.DrawImage(gunTexture, posX, posY, gunW, gunH);
            }
            gunTexture.Dispose();
        }
    }

    public class GameLogic
    {

        // ── References ────────────────────────────────
        private Screen _form;
        private System.Windows.Forms.Timer _gameLoop;
        private Render _renderer;

        // ── Save path ─────────────────────────────────
        private const string _savePath = "save.json";

        // ── Map ───────────────────────────────────────
        private int[,] _map =
        {
        {1,1,1,1,1,1,1,1,1,1 },
        {1,0,0,0,0,0,0,0,0,1 },
        {1,0,0,0,0,0,0,0,0,1 },
        {1,0,0,0,0,0,0,0,0,1 },
        {1,0,0,0,0,0,0,0,0,1 },
        {1,0,0,0,0,0,0,0,0,1 },
        {1,1,1,1,1,1,1,1,1,1 }
    };

        public Image _weapon;

        public void Draw(Graphics g, Size clientSize)
        {
            if (_player == null)
                return;

            Bitmap frame = _renderer.DrawFrame(_player, _map);

            g.DrawImage(frame, 0, 0, clientSize.Width, clientSize.Height);
            g.DrawImage()
        }


        // ── Player ────────────────────────────────────
        private Player _player;

        // ── Movement keys ─────────────────────────────
        private bool _keyW, _keyS, _keyA, _keyD;

        // ── Speed settings ────────────────────────────
        private const float MoveSpeed = 0.08f;
        private const float TurnSpeed = 0.04f;

        // ── Constructor ───────────────────────────────
        public GameLogic(Screen form)
        {
            _form = form;
            _renderer = new Render();

            _gameLoop = new System.Windows.Forms.Timer();
            _gameLoop.Interval = 16;
            _gameLoop.Tick += (s, e) =>
            {
                Update();
                _form.Invalidate();
            };
        }

        // ── Start ─────────────────────────────────────
        public void Start()
        {
            _player = new Player(5, 5); // fresh player
            _gameLoop.Start();
            _form.Invalidate();
        }

        // ── Stop ──────────────────────────────────────
        public void Stop()
        {
            _gameLoop.Stop();
            _keyW = _keyS = _keyA = _keyD = false;
        }

        // ── Update ────────────────────────────────────
        private void Update()
        {
            if (_player == null)
                return;

            if (_keyA)
                _player.Angle -= TurnSpeed;

            if (_keyD)
                _player.Angle += TurnSpeed;

            float dx = 0;
            float dy = 0;

            if (_keyW)
            {
                dx += (float)Math.Cos(_player.Angle) * MoveSpeed;
                dy += (float)Math.Sin(_player.Angle) * MoveSpeed;
            }

            if (_keyS)
            {
                dx -= (float)Math.Cos(_player.Angle) * MoveSpeed;
                dy -= (float)Math.Sin(_player.Angle) * MoveSpeed;
            }

            TryMove(dx, dy);
        }

        public void HandleKeys(KeyEventArgs e)
        {
            if (_player == null)
                return;

            switch (e.KeyCode)
            {
                case Keys.W:
                case Keys.Up:
                    _keyW = true;
                    break;

                case Keys.S:
                case Keys.Down:
                    _keyS = true;
                    break;

                case Keys.A:
                case Keys.Left:
                    _keyA = true;
                    break;

                case Keys.D:
                case Keys.Right:
                    _keyD = true;
                    break;

                case Keys.Q:
                    _player.NextWeapon();
                    break;
            }
        }

        private void TryMove(float dx, float dy)
        {
            float newX = _player.X + dx;
            float newY = _player.Y + dy;

            if (_map[(int)_player.Y, (int)newX] == 0)
                _player.X = newX;

            if (_map[(int)newY, (int)_player.X] == 0)
                _player.Y = newY;
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W:
                case Keys.Up:
                    _keyW = false;
                    break;

                case Keys.S:
                case Keys.Down:
                    _keyS = false;
                    break;

                case Keys.A:
                case Keys.Left:
                    _keyA = false;
                    break;

                case Keys.D:
                case Keys.Right:
                    _keyD = false;
                    break;
            }
        }
    }
}