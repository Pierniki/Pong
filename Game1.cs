using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading;

namespace PPong
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Bar playerBar;
        private Bar enemyBar;
        private Ball ball;
        private Net net;

        private ScoreLabel playerScoreLabel;
        private ScoreLabel enemyScoreLabel;

        private BallMovementHandler ballMovementHandler;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;


            Scaler.setWindowSizes(1920, 1200);

            ballMovementHandler = new BallMovementHandler(Scaler.getWindowWidth() / 240, Scaler.getWindowWidth() / 240);

            graphics.PreferredBackBufferWidth = Scaler.getWindowWidth();
            graphics.PreferredBackBufferHeight = Scaler.getWindowHeight();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Texture2D barTexture = Content.Load<Texture2D>("BAR");
            Texture2D ballTexture = Content.Load<Texture2D>("BALL");

            playerBar = new Bar(barTexture, new Vector2(Scaler.getWindowWidth() - barTexture.Width * Scaler.getWindowHeight() / 30 * 2, Scaler.getWindowHeight() / 2), Scaler.getWindowHeight() / 40);
            enemyBar = new Bar(barTexture, new Vector2(barTexture.Width * Scaler.getWindowHeight() / 30, Scaler.getWindowHeight() / 2), Scaler.getWindowHeight() / 40);
            ball = new Ball(ballTexture, new Vector2(Scaler.getWindowWidth() / 2, Scaler.getWindowHeight() / 2), Scaler.getWindowHeight() / 100);
            net = new Net(barTexture, Scaler.getWindowHeight() / 80);

            playerScoreLabel = new ScoreLabel(Content.Load<SpriteFont>("Munro"), new Vector2(Scaler.getWindowWidth() / 2 + Scaler.getWindowWidth() / 6, Scaler.getWindowHeight() / 25), Scaler.getWindowHeight() / 600);
            enemyScoreLabel = new ScoreLabel(Content.Load<SpriteFont>("Munro"), new Vector2(Scaler.getWindowWidth() / 2 - Scaler.getWindowWidth() / 6, Scaler.getWindowHeight() / 25), Scaler.getWindowHeight() / 600);
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

            KeyboardHandler.UpdateMovement(playerBar, Scaler.getWindowHeight() / 100, Keyboard.GetState());
            AIHandler.React(ball, enemyBar, Scaler.getWindowHeight() / 100, ballMovementHandler);

            CollisionHandler.HandleBarCollision(ball, playerBar, enemyBar, ballMovementHandler, playerScoreLabel, enemyScoreLabel);
            CollisionHandler.HandleWallCollision(ball, ballMovementHandler);
            CollisionHandler.HandleScoreCollision(ball, playerBar, enemyBar, ballMovementHandler, playerScoreLabel, enemyScoreLabel, spriteBatch);

            ballMovementHandler.BallPosUpdate(ball);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);

            playerBar.Draw(spriteBatch);
            enemyBar.Draw(spriteBatch);
            ball.Draw(spriteBatch);
            net.Draw(spriteBatch);

            playerScoreLabel.DrawLabel(spriteBatch);
            enemyScoreLabel.DrawLabel(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    //OBJECTS//
    public abstract class ColissionObject
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public int Scale { get; set; }
        public Rectangle Rect { get; set; }

        public ColissionObject(Texture2D texture, Vector2 position, int scale)
        {
            Texture = texture;
            Position = position;
            Scale = scale;
            Rect = new Rectangle((int)position.X - texture.Width / 2 * scale, (int)position.Y - texture.Height / 2 * scale, texture.Width * scale, texture.Height * scale);
        }

        public void UpdateRect()
        {
            Rect = new Rectangle((int)Position.X - Texture.Width / 2 * Scale, (int)Position.Y - Texture.Height / 2 * Scale, Texture.Width * Scale, Texture.Height * Scale);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, null, Color.White, 0, new Vector2(Texture.Width / 2, Texture.Height / 2), Scale, SpriteEffects.None, 0);
        }

        public abstract void ResetPosition();
    }

    public class Bar : ColissionObject
    {
        public int MvDirection { get; set; }

        public Bar(Texture2D texture, Vector2 position, int scale) : base(texture, position, scale)
        {
            MvDirection = 0;
        }

        public override void ResetPosition()
        {
            Position = new Vector2(Position.X, Scaler.getWindowHeight() / 2);
        }
    }

    public class Ball : ColissionObject
    {
        public Ball(Texture2D texture, Vector2 position, int scale) : base(texture, position, scale) { }

        public override void ResetPosition()
        {
            Position = new Vector2(Scaler.getWindowWidth() / 2, Scaler.getWindowHeight() / 2);
        }
    }

    public class Net
    {
        public Texture2D Texture { get; set; }
        public int Amount { get; set; }
        public int Scale { get; set; }

        public Net(Texture2D texture, int scale)
        {
            Texture = texture;
            Scale = scale;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int sh = Scaler.GetScaledHeight(Texture, Scale);
            Amount = Scaler.getWindowHeight() / (sh + sh / 3);

            float gapLength = (Scaler.getWindowHeight() - Amount * (sh + sh / 2)) / 2;

            for (int i = 0; i < Amount; i++)
            {
                spriteBatch.Draw
                (
                    Texture,
                    new Vector2(Scaler.getWindowWidth() / 2 - Scaler.GetScaledWidth(Texture, Scale) / 2, i * (sh + sh / 2) + gapLength),
                    null,
                    Color.White,
                    0,
                    new Vector2(Texture.Width / 2, Texture.Height / 2),
                    Scale,
                    SpriteEffects.None,
                    0
                );
            }
        }
    }

    public class ScoreLabel
    {
        public SpriteFont Font { get; set; }
        public Vector2 Position { get; set; }
        public float Scale { get; set; }
        public int Value;

        public ScoreLabel(SpriteFont font, Vector2 position, float scale)
        {
            Font = font;
            Position = position;
            Scale = scale;
            Value = 0;
        }

        public void DrawLabel(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Font, Value.ToString(), Position, Color.White, 0, new Vector2(Font.MeasureString(Value.ToString()).X / 2 - Font.MeasureString(Value.ToString()).X / 10, 0), Scale, SpriteEffects.None, 0f);
        }
    }

    //BAR MOVEMENT HANDLERS//
    public static class KeyboardHandler
    {
        public static void UpdateMovement(Bar playerBar, float movementSpeed, KeyboardState state)
        {
            //going up
            if (state.IsKeyDown(Keys.Up) && playerBar.Position.Y > Scaler.GetScaledHeight(playerBar.Texture, playerBar.Scale) / 2)
            {
                playerBar.MvDirection = -1;
                playerBar.Position = playerBar.Position - new Vector2(0, movementSpeed);
                playerBar.UpdateRect();
            }

            //going down
            if (state.IsKeyDown(Keys.Down) && playerBar.Position.Y < Scaler.getWindowHeight() - Scaler.GetScaledHeight(playerBar.Texture, playerBar.Scale) / 2)
            {
                playerBar.MvDirection = 1;
                playerBar.Position = playerBar.Position + new Vector2(0, movementSpeed);
                playerBar.UpdateRect();
            }

            if (state.IsKeyUp(Keys.Up) && state.IsKeyUp(Keys.Down))
            {
                playerBar.MvDirection = 0;
            }
        }
    }

    public static class AIHandler
    {
        public static void React(Ball ball, Bar enemyBar, float movementSpeed, BallMovementHandler ballMovementHandler)
        {
            ////GUARD
            if (ball.Position.X > Scaler.getWindowWidth() / 6)
            {
                //GOING DOWN
                if (enemyBar.Position.Y > ballMovementHandler.EstLanding && enemyBar.Position.Y > Scaler.GetScaledHeight(enemyBar.Texture, enemyBar.Scale) / 2)
                {
                    enemyBar.Position = enemyBar.Position - new Vector2(0, movementSpeed);     
                }

                //GOING UP
                if (enemyBar.Position.Y < ballMovementHandler.EstLanding && enemyBar.Position.Y < Scaler.getWindowHeight() - Scaler.GetScaledHeight(enemyBar.Texture, enemyBar.Scale) / 2)
                {
                    enemyBar.Position = enemyBar.Position + new Vector2(0, movementSpeed);
                }
            }
            //CATCH
            if (ball.Position.X <= Scaler.getWindowWidth() / 6)
            {
                //GOING UP
                if (ball.Position.Y < enemyBar.Position.Y && enemyBar.Position.Y > Scaler.GetScaledHeight(enemyBar.Texture, enemyBar.Scale) / 2)
                {
                    enemyBar.Position = enemyBar.Position - new Vector2(0, movementSpeed);
                }

                //GOING DOWN
                if (ball.Position.Y > enemyBar.Position.Y && enemyBar.Position.Y < Scaler.getWindowHeight() - Scaler.GetScaledHeight(enemyBar.Texture, enemyBar.Scale) / 2)
                {
                    enemyBar.Position = enemyBar.Position + new Vector2(0, movementSpeed);
                }
            }

            enemyBar.UpdateRect();
        }
    }

    //BALL MOVEMENT HANDLERS//
    public class BallMovementHandler
    {
        public float Velocity { get; set; }
        public float XMovement { get; set; }
        public float YMovement { get; set; }
        public float EstLanding { get; set; }

        public BallMovementHandler(float x, float y)
        {
            XMovement = x;
            YMovement = y;
            Velocity = (float)System.Math.Sqrt(x * x + y * y);
            EstLanding = Scaler.getWindowHeight() / 2;
        }

        public void BallPosUpdate(Ball ball)
        {
            ball.Position = ball.Position + new Vector2(XMovement, YMovement);
            ball.UpdateRect();
        }

        public void ResetMovements()
        {
            System.Random rnd = new System.Random();

            XMovement = Scaler.getWindowWidth() / 240;
            YMovement = (rnd.Next(0, 2) * 2 - 1) * Scaler.getWindowWidth() / 240;

            Velocity = (float)System.Math.Sqrt(XMovement * XMovement + YMovement * YMovement);

            EstLanding = Scaler.getWindowHeight() / 2;
        }

        public void StopMovements()
        {
            XMovement = 0;
            YMovement = 0;
        }

        public void EstPos(float ballY)
        {
            int bounces = 0;
            float Y = ballY;

            if (YMovement > 0)
            {
                for (float i = Scaler.getWindowWidth(); i > 0; i = i - System.Math.Abs(XMovement))
                {
                    Y = Y + (float)System.Math.Pow(-1, bounces) * YMovement;
                    if (Y > Scaler.getWindowHeight() || Y < 0)
                        bounces++;
                }
            }
            else
            {
                for (float i = Scaler.getWindowWidth(); i > 0; i = i - System.Math.Abs(XMovement))
                {
                    Y = Y - (float)System.Math.Pow(-1, bounces) * System.Math.Abs(YMovement);
                    if (Y > Scaler.getWindowHeight() || Y < 0)
                        bounces++;
                }
            }

            EstLanding = Y;
        }
    }

    public static class CollisionHandler
    {
        public static void HandleBarCollision(Ball ball, Bar playerBar, Bar enemyBar, BallMovementHandler ballMovementHandler, ScoreLabel playerScoreLabel, ScoreLabel enemyScoreLabel)
        {
            if (ball.Rect.Intersects(playerBar.Rect))
            {
                ballMovementHandler.Velocity += 2;
                ballMovementHandler.YMovement = ballMovementHandler.YMovement + playerBar.MvDirection * (float)System.Math.Sqrt(System.Math.Pow(ballMovementHandler.Velocity - System.Math.Abs(ballMovementHandler.YMovement), 2) / 2);
                ballMovementHandler.XMovement = (float)System.Math.Sqrt(System.Math.Pow(ballMovementHandler.Velocity, 2) - System.Math.Pow(ballMovementHandler.YMovement, 2)) * -1;
                ballMovementHandler.EstPos(ball.Position.Y);
            }

            if (ball.Rect.Intersects(enemyBar.Rect))
            {
                ballMovementHandler.Velocity += 2;
                ballMovementHandler.XMovement = ballMovementHandler.XMovement * -1;
                ballMovementHandler.EstLanding = Scaler.getWindowHeight() / 2;
            }

        }

        public static void HandleWallCollision(Ball ball, BallMovementHandler ballMovementHandler)
        {
            if (ball.Position.Y < 0 || ball.Position.Y > Scaler.getWindowHeight())
            {
                ballMovementHandler.YMovement = ballMovementHandler.YMovement * -1;
            }
        }

        public static void HandleScoreCollision(Ball ball, Bar playerBar, Bar enemyBar, BallMovementHandler ballMovementHandler, ScoreLabel playerScoreLabel, ScoreLabel enemyScoreLabel, SpriteBatch spriteBatch)
        {
            if (ball.Position.X < 0)
            {
                playerScoreLabel.Value++;
                GameplayHandler.ResetRound(playerBar, enemyBar, ball, ballMovementHandler);
            }

            if (ball.Position.X > Scaler.getWindowWidth())
            {
                enemyScoreLabel.Value++;
                GameplayHandler.ResetRound(playerBar, enemyBar, ball, ballMovementHandler);
            }
        }
    }

    //GAMEMANAGEMENT//
    public static class GameplayHandler
    {
        public static void ResetRound(Bar playerBar, Bar enemyBar, Ball ball, BallMovementHandler ballMovementHandler)
        {
            playerBar.ResetPosition();
            enemyBar.ResetPosition();

            ball.ResetPosition();

            ballMovementHandler.ResetMovements();
        }
    }

    //UTILITY//
    public static class Scaler
    {
        public static int windowWidth;
        public static int windowHeight;

        public static void setWindowSizes(int ww, int wh)
        {
            windowWidth = ww;
            windowHeight = wh;
        }

        public static int getWindowWidth()
        {
            return windowWidth;
        }

        public static int getWindowHeight()
        {
            return windowHeight;
        }

        public static int GetScaledWidth(Texture2D texture, int scale)
        {
            return texture.Width * scale;
        }

        public static int GetScaledHeight(Texture2D texture, int scale)
        {
            return texture.Height * scale;
        }
    }

}
