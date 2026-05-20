using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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

        protected Build(string name, int weight, int height, string texturePath)
            : base(name)
        {
            TexturePath = texturePath;
        }
    }
    public class Weapon : Entity
    {
        public override string TexturePath { get; protected set; }
        public Image WeaponImage { get; protected set; }
        public Image WeaponShooting {  get; protected set; }

        public bool IsShooting { get; set; }


        public int MaxAmmo { get; protected set; }
        public int AmmoCount { get; protected set; }

        public Weapon(string name, int ammo, string texturePath, string shootingTexturePath)
            : base(name)
        {
            TexturePath = texturePath;
            WeaponImage = Image.FromFile(TexturePath);
            WeaponShooting = Image.FromFile(shootingTexturePath);
            MaxAmmo = ammo;
            AmmoCount = ammo;
            IsShooting = false;
        }

        public void Shoot()
        {
            if (AmmoCount > 0)
            {
                AmmoCount--;
                IsShooting = true;
            }
        }
        public void Reload()
        {
            AmmoCount = MaxAmmo;
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

        public List<Image> Faces { get; protected set; }
      
        // ── Weapon collection ─────────────────────────
        public List<Weapon> Weapons { get; private set; }
        public Weapon CurrentWeapon { get; private set; }

        // -- Stats -------------------------------------
        public int Health {  get; private set; }

        // ── Constructor ───────────────────────────────
        public Player(float p_x, float p_y, int health) : base("Player1")
        {
            X = p_x;
            Y = p_y;
            Angle = 0;
            Health = health;
            TexturePath = "Assets\\Textures\\Other\\stfevl1.png";

            // Add faces 
            Faces = new List<Image> {
                Image.FromFile(TexturePath),
                Image.FromFile("Assets\\Textures\\Other\\stfkill0.png"),
                Image.FromFile("Assets\\Textures\\Other\\stfdead0.png"),
             };
            //Add basic weapons 
            Weapons = new List<Weapon>
            {
                new Weapon("Pistol", 10, "Assets\\Textures\\Weapons\\pisga0.png","Assets\\Textures\\Weapons\\pisfa0.png"),
                new Weapon("Shotgun", 25, "Assets\\Textures\\Weapons\\shtga0.png","Assets\\Textures\\Weapons\\pisfa0.png"),
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

        // ── Cycle to next weapon ───────────────────────
        public void NextWeapon()
        {
            int next = (Weapons.IndexOf(CurrentWeapon) + 1) % Weapons.Count;
            CurrentWeapon = Weapons[next];
        }

        // -- Get face expreasion ------------------------
        public Image GetFace()
        {
            if(Health == 100)
                return Faces[0];
            if(Health >= 50)
                return Faces[1];
            return Faces[2];
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

    public class SideBar : Entity
    {
        public override string TexturePath { get; protected set; }
        public List<Image> SideBarTextures { get; protected set; }
        public List<Image> Numbers { get; protected set; }
        public int Health { get; protected set; }
        public int AmmoCount { get; protected set; }
        public SideBar(string name, int health, int ammo, string texturePath)
            : base(name)
        {
            TexturePath = texturePath;

            SideBarTextures = new List<Image> {

                Image.FromFile(TexturePath),
                Image.FromFile("Assets\\Textures\\Other\\info.png"),
                Image.FromFile("Assets\\Textures\\Other\\SideArms.png"),
            };
            Numbers = new List<Image>
            {
                Image.FromFile("Assets\\Textures\\Numbers\\winum0.png"),
                Image.FromFile("Assets\\Textures\\Numbers\\winum1.png"),
                Image.FromFile("Assets\\Textures\\Numbers\\winum2.png"),
                Image.FromFile("Assets\\Textures\\Numbers\\winum3.png"),
                Image.FromFile("Assets\\Textures\\Numbers\\winum4.png"),
                Image.FromFile("Assets\\Textures\\Numbers\\winum5.png"),
                Image.FromFile("Assets\\Textures\\Numbers\\winum6.png"),
                Image.FromFile("Assets\\Textures\\Numbers\\winum7.png"),
                Image.FromFile("Assets\\Textures\\Numbers\\winum8.png"),
                Image.FromFile("Assets\\Textures\\Numbers\\winum9.png"),
                Image.FromFile("Assets\\Textures\\Numbers\\wipcnt.png"),
            };

            Health = health;
            AmmoCount = ammo;
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
        public int ScreenW;
        public int ScreenH;


        private int[] _pixels;
        private Wall _wall;
        private Wall _floor;
        private Wall _ceiling;
        private Bitmap _frame;

        private const double FOV = Math.PI / 3.0; // 60 deg, view

        public Render(Screen form)
        {
            ScreenW = form.Width;
            ScreenH = form.Height;

            _frame = new Bitmap(ScreenW, ScreenH, PixelFormat.Format32bppArgb);
            _pixels = new int[ScreenW * ScreenH];

            _wall = new Wall("Assets\\Textures\\Walls\\sw19_1.png","Wall1");
            _floor = new Wall("Assets\\Textures\\Walls\\floor4_8.png", "Floor1");
            _ceiling = new Wall("Assets\\Textures\\Walls\\ceil3_5.png", "Ceiling1");
        }

        public Bitmap DrawFrame(Player player, int[,] map)
        { 
            Array.Clear(_pixels, 0, _pixels.Length);

            DrawFloorCeiling(player);

            for (int screenX = 0; screenX < ScreenW; screenX++)
            {
                // Calculate ray angle for this column
                double rayAngle = player.Angle - FOV / 2 + (screenX / (double)ScreenW) * FOV;

                // Cast ray and get distance to wall
                RayHit hit = CastRay(player.X, player.Y, rayAngle, map);

                // ceiling + textured wall + floor
                DrawWalls(screenX,hit,rayAngle,player.Angle);

            }

            CopyPixelsToBitmap();
            return _frame;
        }

        private void DrawWalls(int screenX, RayHit hit, double rayAngle, double playerAngle)
        {
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

            // Draw wall
            for (int y = wallStart; y < wallEnd; y++)
            {
                double texPercent = (double)(y - wallStartRaw) / wallHeight;
                int texY = (int)(texPercent * _wall.Height);
                texY = Math.Max(0, Math.Min(texY, _wall.Height - 1));

                _pixels[y * ScreenW + screenX] = _wall.GetPixelColor(texX, texY);
            }
        }

        private void DrawFloorCeiling(Player player)
        {
            double rayDirX0 = Math.Cos(player.Angle - FOV / 2);
            double rayDirY0 = Math.Sin(player.Angle - FOV / 2);
            double rayDirX1 = Math.Cos(player.Angle + FOV / 2);
            double rayDirY1 = Math.Sin(player.Angle + FOV / 2);

            for (int y = 0; y < ScreenH; y++)
            {
                bool isCeiling = y < ScreenH / 2;
                int p = isCeiling ? (ScreenH / 2 - y) : (y - ScreenH / 2);
                if (p == 0) continue; // avoid div by zero at exact center

                double rowDist = (ScreenH / 2.0) / p;

                double stepX = rowDist * (rayDirX1 - rayDirX0) / ScreenW;
                double stepY = rowDist * (rayDirY1 - rayDirY0) / ScreenW;

                double worldX = player.X + rowDist * rayDirX0;
                double worldY = player.Y + rowDist * rayDirY0;

                Wall flat = isCeiling ? _ceiling : _floor;

                for (int x = 0; x < ScreenW; x++)
                {
                    int tx = (int)(Math.Abs(worldX - Math.Floor(worldX)) * flat.Width) % flat.Width;
                    int ty = (int)(Math.Abs(worldY - Math.Floor(worldY)) * flat.Height) % flat.Height;

                    _pixels[y * ScreenW + x] = flat.GetPixelColor(tx, ty);

                    worldX += stepX;
                    worldY += stepY;
                }
            }
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
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,1,1,1,0,0,0,1,1,1,0,0,1},
            {1,0,0,0,1,0,0,0,0,0,0,0,1,0,0,1},
            {1,0,0,0,1,0,0,0,1,1,0,0,1,0,0,1},
            {1,0,0,0,1,0,0,0,1,1,0,0,1,0,0,1},
            {1,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1},
            {1,0,1,1,1,0,1,1,1,1,1,0,1,1,0,1},
            {1,0,1,0,0,0,0,0,0,0,1,0,0,1,0,1},
            {1,0,1,0,0,0,0,0,0,0,1,0,0,1,0,1},
            {1,0,1,0,0,1,1,1,1,0,0,0,0,1,0,1},
            {1,0,0,0,0,1,0,0,1,0,1,1,0,0,0,1},
            {1,0,1,1,0,1,0,0,1,0,1,0,0,1,0,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1},
            {1,0,0,0,1,1,1,0,0,1,1,1,0,0,0,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
        };

        // ── Player ────────────────────────────────────
        private Player _player;

        // -- SideBar -----------------------------------
        private SideBar _sideBar;

        // ── Movement keys ─────────────────────────────
        private bool _keyW, _keyS, _keyA, _keyD;

        // ── Speed settings ────────────────────────────
        private const float MoveSpeed = 0.08f;
        private const float TurnSpeed = 0.04f;

        // -- Gun ---------------------------------------
        private int _shootTimer = 0;

        // ── Constructor ───────────────────────────────
        public GameLogic(Screen form)
        {
            _form = form;
            _renderer = new Render(form);
            _gameLoop = new System.Windows.Forms.Timer();
            _gameLoop.Interval = 16;
            _gameLoop.Tick += (s, e) =>
            {
                Update();
                _form.Invalidate();
            };
        }

        // -- Draw graphics ----------------------------
        public void Draw(Graphics g, Size clientSize)
        {
            if (_player == null)
                return;
            // Draw the main frame
            Bitmap frame = _renderer.DrawFrame(_player, _map);
            g.DrawImage(frame, 0, 0, clientSize.Width, clientSize.Height);

            // Add the side bar
            DrawSideBar(g,clientSize);
        }

        public void DrawSideBar(Graphics g, Size clientSize)
        {
        // Draw wepon
            if(_player.CurrentWeapon.IsShooting)
                g.DrawImage(_player.CurrentWeapon.WeaponShooting, (clientSize.Width / 2 - 60), (clientSize.Height / 2 + 30), 130, 150);
            else
                g.DrawImage(_player.CurrentWeapon.WeaponImage, (clientSize.Width / 2 - 70), (clientSize.Height / 2 + 70), 120, 140);

            // Draw sideBar BG
            g.DrawImage(_sideBar.SideBarTextures[0] , 0, clientSize.Height - 90, clientSize.Width, 90);

            // Draw stats images
            g.DrawImage(_sideBar.SideBarTextures[1], (clientSize.Width - 182), (clientSize.Height - 91), 145, 85); // Info image
            g.DrawImage(_sideBar.SideBarTextures[2], (clientSize.Width / 2 - 150), (clientSize.Height - 90), 100, 85); // Side arm image

            // Draw Health
            DrawNumber(g, _player.Health, (clientSize.Width / 3 - 140), clientSize.Height - 75,30,40,true);
            // Draw Ammo
            DrawNumber(g,_player.CurrentWeapon.AmmoCount,30,clientSize.Height - 75,30,40,false);

            // Draw Armor
            DrawNumber(g,50, clientSize.Width/2 + 90, clientSize.Height - 75, 30, 40, true);

            // Draw face 
            g.DrawImage(_player.GetFace(), (clientSize.Width / 2 - 43), (clientSize.Height - 86), 90, 80);


        }

        private void DrawNumber(Graphics g, int value, int x, int y, int digitW, int digitH, bool proc)
        {
            int i;
            string text = value.ToString();
            for (i = 0; i < text.Length; i++)
            {
                int digit = text[i] - '0';
                g.DrawImage( _sideBar.Numbers[digit],(x + i * digitW),y,digitW,digitH);
            }
            if (proc) 
                g.DrawImage(_sideBar.Numbers[10], (x + i * digitW), y, digitW, digitH);
        }

        // ── Start ─────────────────────────────────────
        public void Start()
        {
            _player = new Player(5, 5, 100); // fresh player
            _sideBar = new SideBar("sideBar", 100, _player.CurrentWeapon.AmmoCount, "Assets\\Textures\\Other\\stbar.png");
            _gameLoop.Start();
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
            if (dx != 0 || dy != 0)
                TryMove(dx, dy);

            if (_player.CurrentWeapon.IsShooting)
            {
                _shootTimer--;

                if (_shootTimer <= 0)
                    _player.CurrentWeapon.IsShooting = false;
            }
        }


        // -- Collision ---------------------------------
        private void TryMove(float dx, float dy)
        {
            float newX = _player.X + dx;
            float newY = _player.Y + dy;

            if (_map[(int)_player.Y, (int)newX] == 0)
                _player.X = newX;

            if (_map[(int)newY, (int)_player.X] == 0)
                _player.Y = newY;
        }

        // -- Keys handeling ----------------------------
        public void HandleKeys(KeyEventArgs e)
        {
            if (_player == null)
                return;

            switch (e.KeyCode)
            {
                case Keys.W:     _keyW = true; break;

                case Keys.S:     _keyS = true; break;

                case Keys.A:     _keyA = true; break;

                case Keys.D:     _keyD = true; break;

                case Keys.Escape: _form.GoToMenu();                                break;

                case Keys.Q:      _player.NextWeapon();                            break;
                     
                case Keys.R:      _player.CurrentWeapon.Reload();                  break;
                
                case Keys.Space:  _player.CurrentWeapon.Shoot(); _shootTimer = 10; break;

                
            }
        }
        public void HandleKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: _keyW = false; break;

                case Keys.S: _keyS = false; break;

                case Keys.A: _keyA = false; break;

                case Keys.D: _keyD = false; break;
            }
        }
    }
}