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
        public abstract string TexturePath { get; }

        protected Entity(string name)
        {
            Name = name;
        }
    }
    public abstract class Weapon : Entity
    {
        public int Damage { get; protected set; }
        public int AmmoCount { get; protected set; }

        protected Weapon(string name, int damage, int ammo)
            : base(name)
        {
            Damage = damage;
            AmmoCount = ammo;
        }

        public abstract void Shoot();
        public abstract void Reload();
        public virtual string GetInfo()
        {
            return $"{Name} | Damage: {Damage} | Ammo: {AmmoCount}";
        }
    }
    public abstract class Build : Entity
    {
        public int Weight { get; protected set; }
        public int Height { get; protected set; }

        private string _texturePath;
        public override string TexturePath => _texturePath;

        protected Build(string name, int weight, int height, string texturePath)
            : base(name)
        {
            Weight = weight;
            Height = height;
            _texturePath = texturePath;
        }
    }


    public class Pistol : Weapon
    {
        public override string TexturePath => "Original_assets\\Textures\\Weapons\\pisga0.png";

        // Pistol starts with 10 damage and 50 bullets
        public Pistol() : base("Pistol", 10, 50) { }

        // Pistol fires one bullet at a time
        public override void Shoot()
        {
            if (AmmoCount <= 0)
            {
                Console.WriteLine("Pistol: out of ammo!");
                return;
            }
            AmmoCount--;
            Console.WriteLine($"Pistol: BANG! Ammo left: {AmmoCount}");
        }

        // Pistol reloads back to 50
        public override void Reload()
        {
            AmmoCount = 50;
            Console.WriteLine("Pistol: reloaded.");
        }
    }
    public class Shotgun : Weapon
    {
        public override string TexturePath => "Original_assets\\Textures\\Weapons\\shtga0.png";
        public Shotgun() : base("Shotgun", 30, 6) { }

        public override void Shoot()
        {
            if (AmmoCount > 0)
            {
                AmmoCount--;
                Console.WriteLine($"Shotgun:BOOM");
            }
            else
                Console.WriteLine("Out of ammo");
        }
        public override void Reload()
        {
            AmmoCount = 7;
        }
    }


    public class Player : Entity
    {
        // ── Position ──────────────────────────────────
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }

        // ── Sprite ────────────────────────────────────
        public override string TexturePath => "Original_assets\\Sprites\\face.png";

        // ── Weapon collection ─────────────────────────
        public List<Weapon> Weapons { get; private set; }
        public Weapon CurrentWeapon { get; private set; }

        // ── Constructor ───────────────────────────────
        public Player(float p_x, float p_y) : base("Player1")
        {
            X = p_x;
            Y = p_y;
            Angle = 0;

            // Start with both weapons
            Weapons = new List<Weapon>
            {
                new Pistol(),
                new Shotgun()
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
        private Bitmap _texture;
        public Wall(string texturePath, string name)
            : base(name, 100, 128, texturePath) 
        {
            _texture = new Bitmap(texturePath);
        }

        public int TextureWidth => _texture.Width;
        public int TextureHeight => _texture.Height;

        public int GetTexturePixel(int x, int y)
        {
            return _texture.GetPixel(x, y).ToArgb();
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
        private Bitmap _wall;
        private Bitmap _frame;
        private Bitmap _floor;
        private Bitmap _ceiling;

        private const double FOV = Math.PI / 3.0; // 60 deg, view

        public Render()
        {
            _frame = new Bitmap(ScreenW, ScreenH, PixelFormat.Format32bppArgb);
            _pixels = new int[ScreenW * ScreenH];

            // Load textures
            _wall = new Bitmap("Original_assets\\Textures\\Walls\\sw19_1.png");
            _floor = new Bitmap("Original_assets\\Textures\\Walls\\floor3_3.png");
            _ceiling = new Bitmap("Original_assets\\Textures\\Walls\\ceil3.png");
        }

        public Bitmap DrawFrame(Player player, int[,] map)
        { 
            Array.Clear(_pixels, 0, _pixels.Length);
            
            for (int x = 0; x < ScreenW; x++)
            {
                // Calculate ray angle for this column
                double rayAngle = player.Angle - FOV / 2 + (x / (double)ScreenW) * FOV;

                // Cast ray and get distance to wall
                RayHit hit = CastRay(player.X, player.Y, rayAngle, map);

                hit.Distance = hit.Distance * Math.Cos(rayAngle - player.Angle);

                // Simple perspective: wall height inversely proportional to distance
                int wallHeight = (int)(ScreenH / hit.Distance);

                // Calculate wall start/end on screen
                int wallStart = (ScreenH / 2) - (wallHeight / 2);
                int wallEnd = (ScreenH / 2) + (wallHeight / 2);

                // ceiling + textured wall + floor
                DrawColumn(x, wallStart, wallEnd, hit);

            }

            CopyPixelsToBitmap();
            return _frame;
        }

        private void DrawColumn(int screenX, int wallStart, int wallEnd, RayHit hit)
        {

        }

        private RayHit CastRay(float player_x, float player_y, double angle, int[,] map)
        {
            RayHit hit = new RayHit(20, 0, 0, false); // default max distance
            double x = player_x; 
            double y = player_y;
            double step_x = Math.Cos(angle) * 0.1;  // small step along ray
            double step_y = Math.Sin(angle) * 0.1;

            for (int i = 0; i < 200; i++)  // 200 steps * 0.1 = max depth 20
            {
                x += step_x;
                y += step_y;

                if (map[(int)y,(int)x] == 1)
                {
                    // return actual distance from player to hit point
                    hit.HitX = x;
                    hit.HitY = y;
                    hit.Distance = Math.Sqrt((x-player_x) * (x-player_x) + (y-player_y) * (y-player_y));
                    return hit;
                }
            }
            return hit; // nothing hit
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
            {1,1,1,1,1,1,1,1,1,1 },
            {1,0,0,0,0,0,0,0,0,1 },
            {1,0,0,0,0,0,0,0,0,1 },
            {1,0,0,0,0,0,0,0,0,1 },
            {1,0,0,0,0,0,0,0,0,1 },
            {1,0,0,0,0,0,0,0,0,1 },
            {1,1,1,1,1,1,1,1,1,1 }

        };

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
            //_player = new Player(5, 5);
            _renderer = new Render();
            _gameLoop = new System.Windows.Forms.Timer();
            _gameLoop.Interval = 16;
            _gameLoop.Tick += (s, e) => { Update(); _form.Invalidate(); }; //s, e are empty, but must be sent 
        }

        // ── Start ─────────────────────────────────────
        public void Start()
        {
            _player = new Player(5,5); // fresh player
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
            
        }

        public void Draw(Graphics g, Size clientSize)
        {
            if (_player == null)
                return;

            Bitmap frame = _renderer.DrawFrame(_player, _map);

            g.DrawImage(frame, 0, 0, clientSize.Width, clientSize.Height);
        }

        public void HandleKeys(KeyEventArgs e)
        {
           
        }

        public void HandleKeyUp(KeyEventArgs e)
        {
            
        }
    }
}