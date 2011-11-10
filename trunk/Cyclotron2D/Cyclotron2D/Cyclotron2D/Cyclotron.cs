using System;
using Cyclotron2D.Screens.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cyclotron2D
{
    /// <summary>
    /// This is the main Class for Cyclotron it instanciates the main modules and keeps track of GameState
    /// </summary>
    public class Cyclotron : Game
    {
        #region Constants

        public readonly string Name = System.Environment.MachineName;

        #endregion

        #region Fields

        private GraphicsDeviceManager m_graphics;

        #endregion

        #region Properties

        public GameTime GameTime { get; private set; }

        public ScreenManager ScreenManager { get; private set; }

        public GameState State { get; private set; }

        public InputState InputState { get; private set; }

        public SpriteBatch SpriteBatch { get; private set; }

        #endregion

        #region Constructor

        public Cyclotron()
        {
            Content.RootDirectory = "Content";

            m_graphics = new GraphicsDeviceManager(this);
            ScreenManager = new ScreenManager(this);
            InputState = new InputState(this);
        }

        #endregion

        #region Events

        public event EventHandler<StateChangedEventArgs> StateChanged;

        private void InvokeStateChanged(StateChangedEventArgs e)
        {
            EventHandler<StateChangedEventArgs> handler = StateChanged;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This is the method that should be called to transition  between states.
        /// A Call to this should be sufficient to completely change states. If certain Screens need
        /// more than the generic reaction to this they should subscribe to the StateChanged event.
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(GameState state)
        {
            if (State != state)
            {
                var args = new StateChangedEventArgs(State, state);
                State = state;
                InvokeStateChanged(args);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true;
            Art.Initalize(this);
            LoadScreens();

            ChangeState(GameState.MainMenu);
            base.Initialize();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            Art.LoadContent(Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Art.UnloadContent(Content);
        }

        protected override void Update(GameTime gameTime)
        {
            //so that some random places that need it can always access it
            GameTime = gameTime;
            //updates the rest of the game
            base.Update(gameTime);

            DebugMessages.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //  return;
            GraphicsDevice.Clear(Color.Black);

            SpriteBatch.Begin();

            ScreenManager.Draw(gameTime);

            DebugMessages.Draw(SpriteBatch, gameTime);

            SpriteBatch.End();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Instanciates and loads all the MainScreens into the ScreenManager.
        /// </summary>
        private void LoadScreens()
        {
            ScreenManager.AddScreen(new MainMenuScreen(this));
            ScreenManager.AddScreen(new GameScreen(this));
            ScreenManager.AddScreen(new JoinGameScreen(this));
            ScreenManager.AddScreen(new SettingsScreen(this));
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ScreenManager.Dispose();
                ScreenManager = null;
                InputState.Dispose();
                InputState = null;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}