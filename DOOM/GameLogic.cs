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
        // Finally answers Entity's abstract SpritePath
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
        public Wall(string texturePath)
            : base("Wall", 100, 128, texturePath) { }
    }


    public class Render
    {
        public const int ScreenW = 640;
        public const int ScreenH = 400;


        private Bitmap _frame;
        private int[] _pixels;


        private const double FOV = Math.PI / 3.0; // 60 deg, view

        public Render()
        {
            _frame = new Bitmap(ScreenW, ScreenH, PixelFormat.Format32bppArgb);
            _pixels = new int[ScreenW * ScreenH];
        }

        public Bitmap DrawFrame(Player player, int[,] map)
        { 
            Array.Clear(_pixels, 0, _pixels.Length); // Clean the array
            int ceilColor = Color.SlateGray.ToArgb();
            int floorColor = Color.DarkGray.ToArgb();

            // 1. draw ceiling/floor basic colors
            for (int y = 0; y < ScreenH; y++)
            {
                for (int x = 0; x < ScreenW; x++)
                {
                    _pixels[y * ScreenW + x] = y < ScreenH / 2 ? ceilColor : floorColor;
                }
            }

            // 2. raycast walls here

           CastRay(player.X, player.Y,player.Angle,map);

            // DrawVerticalTextureColumn(...)

            CopyPixelsToBitmap();
            return _frame;
        }

        private double CastRay(float player_x, float player_y, double angle, int[,] map)
        {
            double x = player_x;
            double y = player_y;
            double deg_x = Math.Cos(angle) * 0.1;  // small step along ray
            double deg_y = Math.Sin(angle) * 0.1;

            for (int i = 0; i < 200; i++)  // 200 steps * 0.1 = max depth 20
            {
                x += deg_x;
                y += deg_y;

                if (map[(int)y,(int)x] == 1)
                {
                    // return actual distance from player to hit point
                    double distX = x - player_x;
                    double distY = y - player_y;
                    return Math.Sqrt(distX * distX + distY * distY);
                }
            }

            return 20; // max depth, nothing hit
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