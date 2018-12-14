﻿using System;
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

        private string gamestate;

        private Texture2D discTexture;
        private Texture2D yana_sleeping1;
        private Texture2D yana_sleeping2;
        private Texture2D yasha_center;
        private Texture2D yasha_left;
        private Texture2D yasha_right;
        private Texture2D reflector_default;
        private Texture2D reflector_spark;
        private Texture2D reflector_hit;
        private Texture2D lose;
        private Texture2D tohadze;

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
            discTexture = Content.Load<Texture2D>("Textures/maxwell");
            discOrigin = new Vector2(discTexture.Width / 2, discTexture.Height / 2);
            reflector_default = Content.Load<Texture2D>("Textures/reflector_default");
            reflector_spark = Content.Load<Texture2D>("Textures/reflector_spark");
            reflector_hit = Content.Load<Texture2D>("Textures/reflector_hit");
            reflectorOrigin = new Vector2(reflector_default.Width / 2, refl_radius / 1.67f );
            yana_sleeping1 = Content.Load<Texture2D>("Textures/yana_sleeping1");
            yana_sleeping2 = Content.Load<Texture2D>("Textures/yana_sleeping2");
            yasha_center = Content.Load<Texture2D>("Textures/yasha_center");
            yasha_left = Content.Load<Texture2D>("Textures/yasha_left");
            yasha_right = Content.Load<Texture2D>("Textures/yasha_right");
            lose = Content.Load<Texture2D>("Textures/lose");
            tohadze = Content.Load<Texture2D>("Textures/tohadze");
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
            spriteBatch.Draw(tohadze, new Rectangle(0, 0, screenWidth, screenHeight), null, Color.White, 0f, new Vector2(0, 1), SpriteEffects.None, 0f);
            if (gamestate == "play")
            {
                foreach (Disc d in discs)
                    spriteBatch.Draw(discTexture, d.GetRenderRectangle(), null, d.GetCol(), d.GetRot(), discOrigin, SpriteEffects.None, 0f);
                if (hitFrames < 20)
                {
                    spriteBatch.Draw(reflector_hit, new Rectangle(Convert.ToInt32(screenWidth / 2 + 8), Convert.ToInt32(screenHeight), 128, 128), null, reflector.GetCol(), reflector.GetRot(), reflectorOrigin, SpriteEffects.None, 0f);
                    hitFrames++;
                }
                else
                {
                    if (shouldSpark == true)
                    {
                        if ((sparkFrames < 5) || (sparkFrames > 10))
                            spriteBatch.Draw(reflector_spark, new Rectangle(Convert.ToInt32(screenWidth / 2 + 8), Convert.ToInt32(screenHeight), 128, 128), null, reflector.GetCol(), reflector.GetRot(), reflectorOrigin, SpriteEffects.None, 0f);
                        else
                            spriteBatch.Draw(reflector_default, new Rectangle(Convert.ToInt32(screenWidth / 2 + 8), Convert.ToInt32(screenHeight), 128, 128), null, reflector.GetCol(), reflector.GetRot(), reflectorOrigin, SpriteEffects.None, 0f);
                        if (sparkFrames > 15)
                            shouldSpark = false;
                        sparkFrames++;
                    }
                    else
                        spriteBatch.Draw(reflector_default, new Rectangle(Convert.ToInt32(screenWidth / 2 + 8), Convert.ToInt32(screenHeight), 128, 128), null, reflector.GetCol(), reflector.GetRot(), reflectorOrigin, SpriteEffects.None, 0f);
                }
                if ((reflector.GetRot() > -Math.PI / 6) && (reflector.GetRot() < Math.PI / 6))
                    spriteBatch.Draw(yasha_center, new Rectangle(Convert.ToInt32(screenWidth / 2) - 64, screenHeight - 128, 128, 128), Color.White);
                if (reflector.GetRot() < -Math.PI / 6)
                    spriteBatch.Draw(yasha_left, new Rectangle(Convert.ToInt32(screenWidth / 2) - 64, screenHeight - 128, 128, 128), Color.White);
                if (reflector.GetRot() > Math.PI / 6)
                    spriteBatch.Draw(yasha_right, new Rectangle(Convert.ToInt32(screenWidth / 2) - 64, screenHeight - 128, 128, 128), Color.White);
                if (animationTimer < 1000)
                    spriteBatch.Draw(yana_sleeping1, new Rectangle(Convert.ToInt32(screenWidth / 2) - 64, screenHeight - 128, 128, 128), null, Color.White, 0f, new Vector2(0, 0), SpriteEffects.None, 0f);
                if (animationTimer > 1000)
                    spriteBatch.Draw(yana_sleeping2, new Rectangle(Convert.ToInt32(screenWidth / 2 - 64), screenHeight - 128, 128, 128), null, Color.White, 0f, new Vector2(0, 0), SpriteEffects.None, 0f);
                /*foreach (Rectangle r in reflector.GetColliders())
                    spriteBatch.Draw(discTexture, r, null, Color.White, 0, discOrigin, SpriteEffects.None, 0f);*/
            }

            if (gamestate == "lose")
                spriteBatch.Draw(lose, new Rectangle(Convert.ToInt32(screenWidth / 2 - 64), Convert.ToInt32(screenHeight / 2 - 64), 128, 128), Color.White);

            spriteBatch.End();

            animationTimer += gameTime.ElapsedGameTime.Milliseconds;

            if (animationTimer > 2000)
                animationTimer -= 2000;

            base.Draw(gameTime);
        }
    }
}