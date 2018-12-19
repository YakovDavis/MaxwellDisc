using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Maxwell.Desktop
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private const float sensitivity = 0.015f;
        private const int refl_radius = 175;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private int screenWidth;
        private int screenHeight;
        private int screenDiag;

        private int timer;
        private int delay;

        private Random rnd;

        private float animationTimer;

        private Dictionary<string, AnimationInfo> animationControl;

        private string gamestate;

        private Dictionary<string, Texture2D> textures;
        private readonly string[] textureNames = {"disc_maxwell",
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
        //private string currentReflectorTexture;

        private Rectangle trigger;
        private List<Disc> discs;
        private Reflector reflector;
        private Point target;
        private Point bottomCenter;

        private Vector2 discOrigin;
        private Vector2 reflectorOrigin;
        private MiscTools mt;

        private bool shouldSpark;
        private int sparkFrames;
        private int hitFrames;

        private Point previousMousePosition;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            rnd = new Random();
            mt = new MiscTools(rnd);
            screenWidth = Window.ClientBounds.Width;
            screenHeight = Window.ClientBounds.Height;
            screenDiag = Convert.ToInt32(Math.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight));
            bottomCenter = new Point(screenWidth / 2, screenHeight);
            target = new Point(screenWidth / 2, screenHeight - 64);
            discs = new List<Disc>();
            reflector = new Reflector(bottomCenter, refl_radius, 64, Color.Aquamarine);
            timer = 0;
            delay = 100;
            shouldSpark = false;
            sparkFrames = 0;
            hitFrames = 20;
            trigger = new Rectangle(Convert.ToInt32(screenWidth / 2) - 32, screenHeight - 64, 64, 64);
            textures = new Dictionary<string, Texture2D>();

            animationControl = new Dictionary<string, AnimationInfo>();
            animationControl.Add("yana", new AnimationInfo(0f, 0.25f, 2));

            currentYashaTexture = "yasha_center";
            currentYanaTexture = "yana_sleeping1";
            currentReflectorTexture = "reflector_default";

            gamestate = "play";

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            foreach (string n in textureNames)
            {
                textures.Add(n, Content.Load<Texture2D>("Textures/" + n));
            }

            discOrigin = new Vector2(textures["disc_maxwell"].Width / 2, textures["disc_maxwell"].Height / 2);
            reflectorOrigin = new Vector2(textures["reflector_default"].Width / 2, refl_radius / 1.67f );
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            reflector.Rotate((Mouse.GetState().Position.X - previousMousePosition.X) * sensitivity);

            if (gamestate == "play")
            {
                foreach (Disc d in discs)
                {
                    d.Update(gameTime);
                    if ((reflector.Collides(d.GetCollisionRectangle())) && (!d.WasReversed()) && (d.DistanceToBottomCenter() > refl_radius + 40))
                    {
                        d.Reverse();
                        hitFrames = 0;
                    }
                    if (trigger.Intersects(d.GetCollisionRectangle()))
                        gamestate = "lose";
                }

                for (int i = 0; i < discs.Count; i++)
                    if (discs[i].DistanceToBottomCenter() > 2 * screenDiag)
                        discs.Remove(discs[i]);

                if (timer < delay)
                    timer++;
                else
                {
                    timer = 0;
                    discs.Add(new Disc(mt.RandomColor(), mt.RandomSpawn(target, screenDiag), target, bottomCenter));
                }

                if ((rnd.Next(200) == 55)&&(!shouldSpark))
                {
                    shouldSpark = true;
                    sparkFrames = 0;
                }
            }

            if ((reflector.GetRot() > -Math.PI / 6) && (reflector.GetRot() < Math.PI / 6))
                currentYashaTexture = "yasha_center";
            if (reflector.GetRot() < -Math.PI / 6)
                currentYashaTexture = "yasha_left";
            if (reflector.GetRot() > Math.PI / 6)
                currentYashaTexture = "yasha_right";
            switch (AnimationParser(gameTime, animationControl["yana"]))
            {
                case 0:
                    currentYanaTexture = "yana_sleeping1";
                    break;
                case 1:
                    currentYanaTexture = "yana_sleeping2";
                    break;
            }

            previousMousePosition = Mouse.GetState().Position;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkCyan);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
            //spriteBatch.Draw(textures["tohadze"], new Rectangle(0, 0, screenWidth, screenHeight), null, Color.White, 0f, new Vector2(0, 1), SpriteEffects.None, 0f);
            if (gamestate == "play")
            {
                foreach (Disc d in discs)
                    spriteBatch.Draw(textures["disc_maxwell"], d.GetRenderRectangle(), null, d.GetCol(), d.GetRot(), discOrigin, SpriteEffects.None, 0f);
                if (hitFrames < 20)
                {
                    spriteBatch.Draw(textures["reflector_hit"], new Rectangle(Convert.ToInt32(screenWidth / 2 + 8), Convert.ToInt32(screenHeight), 128, 128), null, reflector.GetCol(), reflector.GetRot(), reflectorOrigin, SpriteEffects.None, 0f);
                    hitFrames++;
                }
                else
                {
                    if (shouldSpark == true)
                    {
                        if ((sparkFrames < 5) || (sparkFrames > 10))
                            spriteBatch.Draw(textures["reflector_spark"], new Rectangle(Convert.ToInt32(screenWidth / 2 + 8), Convert.ToInt32(screenHeight), 128, 128), null, reflector.GetCol(), reflector.GetRot(), reflectorOrigin, SpriteEffects.None, 0f);
                        else
                            spriteBatch.Draw(textures["reflector_default"], new Rectangle(Convert.ToInt32(screenWidth / 2 + 8), Convert.ToInt32(screenHeight), 128, 128), null, reflector.GetCol(), reflector.GetRot(), reflectorOrigin, SpriteEffects.None, 0f);
                        if (sparkFrames > 15)
                            shouldSpark = false;
                        sparkFrames++;
                    }
                    else
                        spriteBatch.Draw(textures["reflector_default"], new Rectangle(Convert.ToInt32(screenWidth / 2 + 8), Convert.ToInt32(screenHeight), 128, 128), null, reflector.GetCol(), reflector.GetRot(), reflectorOrigin, SpriteEffects.None, 0f);
                }
                spriteBatch.Draw(textures["machine"], new Rectangle(Convert.ToInt32(screenWidth / 2 - 80), Convert.ToInt32(screenHeight - 90), 180, 180), null, Color.White, 0f, new Vector2(0, 1), SpriteEffects.None, 0f);
                spriteBatch.Draw(textures[currentYashaTexture], mt.BottomCenterRectangle(mt.Div2(screenWidth), screenHeight + 6, 128, 128), Color.White);
                spriteBatch.Draw(textures[currentYanaTexture], mt.BottomCenterRectangle(mt.Div2(screenWidth), screenHeight + 6, 128, 128), null, Color.White, 0f, new Vector2(0, 0), SpriteEffects.None, 0f);
                /*foreach (Rectangle r in reflector.GetColliders())
                    spriteBatch.Draw(discTexture, r, null, Color.White, 0, discOrigin, SpriteEffects.None, 0f);*/
            }

            if (gamestate == "lose")
            {
                spriteBatch.Draw(textures["tohadze"], new Rectangle(0, 0, screenWidth, screenHeight), null, Color.White, 0f, new Vector2(0, 1), SpriteEffects.None, 0f);
                spriteBatch.Draw(textures["lose"], new Rectangle(Convert.ToInt32(screenWidth / 2 - 64), Convert.ToInt32(screenHeight / 2 - 64), 128, 128), Color.White);
            }
            spriteBatch.End();

            animationTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (animationTimer > 2000)
                animationTimer -= 2000;

            base.Draw(gameTime);
        }

        private int AnimationParser(GameTime t, AnimationInfo a)
        {
            float time = (t.TotalGameTime.Milliseconds - a.start) / 1000;
            int n = Convert.ToInt32(time / (1 / a.fps));
            return Convert.ToInt32(n % a.frames);
        }

        private class AnimationInfo
        {
            public float start;
            public float fps;
            public int frames;
            public AnimationInfo(float start1, float fps1, int frames1)
            {
                start = start1;
                fps = fps1;
                frames = frames1 - 1;
            }
        }
    }
}