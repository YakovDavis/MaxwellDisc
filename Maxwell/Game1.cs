using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Maxwell.Desktop
{
    public class Game1 : Game
    {
        private const int refl_radius = 175;
        private const int resolutionWidth = 900;
        private const int resolutionHeight = 480;
        private const int targetResolutionHeight = 720;
        private const int targetResolutionWidth = 1280;
        private const float mouseSensitivity = 1.5f;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;

        private int screenWidth;
        private int screenHeight;
        private int screenDiag;

        private Cursor cursor;

        private float delay;

        private int score;
        public float totalTime;

        private Random rnd;

        private Dictionary<string, AnimationInfo> animationControl;

        private GameState gamestate;

        private string musicstate;

        private SpriteFont scoreFont;
        private SpriteFont titleFont;

        private Dictionary<string, SoundEffect> sounds;
        private Dictionary<string, SoundEffectInstance> soundInstances;

        private Dictionary<string, Texture2D> textures;
        private readonly string[] textureNames = {"disc_maxwell",
                                                  "cursor",
                                                  "yana_sleeping1",
                                                  "yana_sleeping2",
                                                  "yasha_center",
                                                  "yasha_left",
                                                  "yasha_right",
                                                  "reflector_default",
                                                  "reflector_spark",
                                                  "reflector_hit",
                                                  "lose",
                                                  "tohadze",
                                                  "machine"};

        private string currentYashaTexture;
        private string currentYanaTexture;
        private string currentReflectorTexture;

        private Dictionary<string, float> cooldowns;

        private Rectangle trigger;
        private List<Disc> discs;
        private Reflector reflector;
        private Point target;
        private Point bottomCenter;

        private Vector2 discOrigin;
        private Vector2 reflectorOrigin;
        private MiscTools mt;

        private Point previousMousePosition;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferHeight = resolutionHeight;
            graphics.PreferredBackBufferWidth = resolutionWidth;
            graphics.PreferMultiSampling = true;

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            rnd = new Random();
            mt = new MiscTools(rnd);

            screenWidth = targetResolutionWidth;
            screenHeight = targetResolutionHeight;
            screenDiag = Convert.ToInt32(Math.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight));

            bottomCenter = new Point(screenWidth / 2, screenHeight);

            target = new Point(screenWidth / 2, screenHeight - 64);

            cursor = new Cursor(0, 0, 8, 1f);

            discs = new List<Disc>();
            reflector = new Reflector(bottomCenter, refl_radius, 96, Color.Aquamarine);

            trigger = new Rectangle(Convert.ToInt32(screenWidth / 2) - 32, screenHeight - 64, 64, 64);
            textures = new Dictionary<string, Texture2D>();
            cooldowns = new Dictionary<string, float>();
            sounds = new Dictionary<string, SoundEffect>();
            soundInstances = new Dictionary<string, SoundEffectInstance>();

            animationControl = new Dictionary<string, AnimationInfo>();
            animationControl.Add("yana", new AnimationInfo(0f, 0.25f, 2));

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            renderTarget = new RenderTarget2D(GraphicsDevice, targetResolutionWidth, targetResolutionHeight);

            foreach (string n in textureNames)
            {
                textures.Add(n, Content.Load<Texture2D>("Textures/" + n));
            }

            scoreFont = Content.Load<SpriteFont>("Fonts/score");
            titleFont = Content.Load<SpriteFont>("Fonts/title");


            sounds.Add("intro", Content.Load<SoundEffect>("Music/intro"));
            sounds.Add("theme", Content.Load<SoundEffect>("Music/theme"));
            sounds.Add("gameover", Content.Load<SoundEffect>("Music/gameover"));

            soundInstances.Add("intro", sounds["intro"].CreateInstance());
            soundInstances["intro"].Volume = 0.1f;
            soundInstances.Add("theme", sounds["theme"].CreateInstance());
            soundInstances["theme"].Volume = 0.1f;
            soundInstances.Add("gameover", sounds["gameover"].CreateInstance());
            soundInstances["gameover"].Volume = 0.1f;

            discOrigin = new Vector2(25, 25);
            reflectorOrigin = new Vector2(textures["reflector_default"].Width / 2, refl_radius / 1.67f);

            GameStart();
        }

        protected override void UnloadContent()
        {
            sounds["intro"].Dispose();
            sounds["theme"].Dispose();
            soundInstances["intro"].Dispose();
            soundInstances["theme"].Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if ((musicstate == "intro")&&(soundInstances["intro"].State == SoundState.Stopped))
            {
                musicstate = "theme";
                soundInstances["theme"].IsLooped = true;
                soundInstances["theme"].Play();
            }
            /*if ((musicstate == "theme") && (soundInstances["theme"].State == SoundState.Stopped))
                soundInstances["theme"].Play();*/

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                GameStart();
            }

            if (cursor.AccuratePosition.X > screenWidth - 8)
                cursor.AccuratePosition = new Vector2(screenWidth - 8, cursor.AccuratePosition.Y);
            if (cursor.AccuratePosition.X < 0)
                cursor.AccuratePosition = new Vector2(0, cursor.AccuratePosition.Y);
            if (cursor.AccuratePosition.Y > screenHeight - 8)
                cursor.AccuratePosition = new Vector2(cursor.AccuratePosition.X, screenHeight - 8);
            if (cursor.AccuratePosition.Y < 0)
                cursor.AccuratePosition = new Vector2(cursor.AccuratePosition.X, 0);
            Vector2 displacement = Mouse.GetState().Position.ToVector2() - previousMousePosition.ToVector2();
            cursor.Move((Mouse.GetState().Position.ToVector2() - previousMousePosition.ToVector2()) * mouseSensitivity);
            Vector2 rVector = cursor.AccuratePosition - new Vector2(mt.Div2(screenWidth), screenHeight);
            reflector.SetRotation(Convert.ToSingle(Math.Atan(rVector.X / Math.Abs(rVector.Y)) - 0.02f));

            if (gamestate == GameState.Playing)
            {
                foreach (Disc d in discs)
                {
                    d.Update(gameTime);
                    if ((reflector.Collides(d.GetCollisionRectangle())) && (!d.WasReversed()) && (d.DistanceToBottomCenter() > refl_radius + 35))
                    {
                        d.Reverse();
                        score++;
                        delay = 4 / (1 + score / 8) + 1;
                        //Debug.WriteLine(delay);
                        if (cooldowns.ContainsKey("hit"))
                            cooldowns.Remove("hit");
                        cooldowns.Add("hit", 0.0f);
                    }
                    if (trigger.Intersects(d.GetCollisionRectangle()))
                    {
                        gamestate = GameState.Lost;
                        StopSound();
                        soundInstances["gameover"].Play();
                    }
                }

                for (int i = 0; i < discs.Count; i++)
                    if (discs[i].DistanceToBottomCenter() > 2 * screenDiag)
                        discs.Remove(discs[i]);

                if (cooldowns["spawn"] >= delay)
                {
                    cooldowns["spawn"] = 0.0f;
                    discs.Add(new Disc("disc_maxwell", mt.RandomColor(), mt.RandomSpawn(target, screenDiag), target, bottomCenter));
                }

                if ((rnd.Next(200) == 55) && (!cooldowns.ContainsKey("spark")))
                {
                    cooldowns.Add("spark", 0.0f);
                    currentReflectorTexture = "reflector_spark";
                }

                if (cooldowns.ContainsKey("spark"))
                {
                    if ((cooldowns["spark"] < 0.2f) || (cooldowns["spark"] > 0.4f))
                        currentReflectorTexture = "reflector_spark";
                    else
                        currentReflectorTexture = "reflector_default";
                    if (cooldowns["spark"] > 0.6f)
                    {
                        currentReflectorTexture = "reflector_default";
                        cooldowns.Remove("spark");
                    }
                }

                if ((reflector.GetRot() > -Math.PI / 6) && (reflector.GetRot() < Math.PI / 6))
                    currentYashaTexture = "yasha_center";
                if (reflector.GetRot() < -Math.PI / 6)
                    currentYashaTexture = "yasha_left";
                if (reflector.GetRot() > Math.PI / 6)
                    currentYashaTexture = "yasha_right";
                if (cooldowns.ContainsKey("hit"))
                {
                    if (cooldowns["hit"] > 0.8f)
                    {
                        cooldowns.Remove("hit");
                        currentReflectorTexture = "reflector_default";
                    }
                    else
                        currentReflectorTexture = "reflector_hit";
                }
                switch (AnimationParser(gameTime, animationControl["yana"]))
                {
                    case 0:
                        currentYanaTexture = "yana_sleeping1";
                        break;
                    case 1:
                        currentYanaTexture = "yana_sleeping2";
                        break;
                }
                totalTime += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
            }

            Mouse.SetPosition(mt.Div2(screenWidth), mt.Div2(screenHeight));
            previousMousePosition = Mouse.GetState().Position;

            List<string> temp = new List<string>(cooldowns.Keys);

            foreach (string k in temp)
                cooldowns[k] += Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkCyan);
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.DarkCyan);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState { MultiSampleAntiAlias = true }, null, null);
            switch (gamestate)
            {
                case GameState.Playing:
                    {
                        foreach (Disc d in discs)
                            spriteBatch.Draw(textures["disc_maxwell"], d.GetRenderRectangle(), null, d.GetCol(), d.GetRot(), discOrigin, SpriteEffects.None, 0f);
                        spriteBatch.Draw(textures[currentReflectorTexture], new Rectangle(Convert.ToInt32(screenWidth / 2 + 8), Convert.ToInt32(screenHeight), 192, 192), null, reflector.GetCol(), reflector.GetRot(), reflectorOrigin, SpriteEffects.None, 0f);
                        spriteBatch.Draw(textures["machine"], new Rectangle(Convert.ToInt32(screenWidth / 2 - 120), Convert.ToInt32(screenHeight - 135), 270, 270), null, Color.White, 0f, new Vector2(0, 1), SpriteEffects.None, 0f);
                        spriteBatch.Draw(textures[currentYashaTexture], mt.BottomCenterRectangle(mt.Div2(screenWidth), screenHeight + 6, 192, 192), Color.White);
                        spriteBatch.Draw(textures[currentYanaTexture], mt.BottomCenterRectangle(mt.Div2(screenWidth), screenHeight + 6, 192, 192), null, Color.White, 0f, new Vector2(0, 0), SpriteEffects.None, 0f);
                        spriteBatch.DrawString(scoreFont, $"Score: {score}", new Vector2(20, 0), Color.Black);
                        spriteBatch.DrawString(scoreFont, $"Time: {totalTime.ToString("n2")}", new Vector2(screenWidth - 250, 0), Color.Black);
                        /*foreach (Rectangle r in reflector.GetColliders())
                            spriteBatch.Draw(textures["disc_maxwell"], r, null, Color.White, 0, discOrigin, SpriteEffects.None, 0f);*/
                        spriteBatch.Draw(textures["cursor"], cursor.GetRenderRectangle(), null, Color.White, 0f, new Vector2(0, 1), SpriteEffects.None, 0f);
                        break;
                    }
                case GameState.Lost:
                    {
                        spriteBatch.Draw(textures["tohadze"], new Rectangle(0, 0, screenWidth, screenHeight), null, Color.White, 0f, new Vector2(0, 1), SpriteEffects.None, 0f);
                        spriteBatch.DrawString(titleFont, $"Game over!", new Vector2(mt.Div2(screenWidth) - 100, mt.Div2(screenHeight)), Color.Black);
                        spriteBatch.DrawString(scoreFont, $"You survived for {totalTime.ToString("n2")} seconds\nAnd scored {score} points\nPress R to restart", new Vector2(mt.Div2(screenWidth) - 200, mt.Div2(screenHeight) + 50), Color.Black);
                        //spriteBatch.Draw(textures["lose"], new Rectangle(Convert.ToInt32(screenWidth / 2 - 64), Convert.ToInt32(screenHeight / 2 - 64), 128, 128), Color.White);
                        break;
                    }
            }
            spriteBatch.DrawString(scoreFont, $"Pre-Alpha", new Vector2(screenWidth - 200, screenHeight - 30), Color.DarkGray);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Begin(); //SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(renderTarget, new Rectangle(Convert.ToInt32((resolutionWidth - resolutionHeight * ((float)targetResolutionWidth / targetResolutionHeight)) / 2), 0, Convert.ToInt32(resolutionHeight * ((float)targetResolutionWidth / targetResolutionHeight)), resolutionHeight), Color.White);
            spriteBatch.End();

            Debug.WriteLine(resolutionHeight * ((float)targetResolutionWidth / targetResolutionHeight));

            base.Draw(gameTime);
        }

        private void GameStart()
        {
            delay = 5.0f;
            if (cooldowns.ContainsKey("spawn"))
                cooldowns.Remove("spawn");
            cooldowns.Add("spawn", 0.0f);
            currentYashaTexture = "yasha_center";
            currentYanaTexture = "yana_sleeping1";
            currentReflectorTexture = "reflector_default";
            gamestate = GameState.Playing;
            musicstate = "intro";
            StopSound();
            soundInstances["intro"].Play();
            score = 0;
            totalTime = 0;
            discs.Clear();
        }

        private void StopSound()
        {
            foreach (SoundEffectInstance s in soundInstances.Values)
                s.Stop();
        }

        private int AnimationParser(GameTime t, AnimationInfo a)
        {
            return Convert.ToInt32(Convert.ToSingle(t.TotalGameTime.TotalSeconds) * a.fps % a.frames);
        }
    }
}