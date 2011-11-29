using System;
using Cyclotron2D.Graphics;
using Cyclotron2D.Mod;
using Cyclotron2D.Network;
using Cyclotron2D.Screens.Main;
using Cyclotron2D.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Cyclotron2D.Sounds;
using Microsoft.Xna.Framework.Media;

namespace Cyclotron2D
{
    /// <summary>
    /// This is the main Class for Cyclotron it instantiates the main modules and keeps track of GameState
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

        public RttUpdateService RttService { get; private set; }

        public GameTime GameTime { get; private set; }

        public ScreenManager ScreenManager { get; private set; }

        public GameState State { get; private set; }

        public InputState InputState { get; private set; }

        public SpriteBatch SpriteBatch { get; private set; }

        public NetworkCommunicator Communicator { get; private set; }

        #endregion

        #region Constructor

        public Cyclotron()
        {
			DebugMessages.LogMessages = true;
            IsFixedTimeStep = true;
            Content.RootDirectory = "Content";

            m_graphics = new GraphicsDeviceManager(this);
            RttService = new RttUpdateService(this) { Enabled = true};
            ScreenManager = new ScreenManager(this);
            InputState = new InputState(this);
            Communicator = new NetworkCommunicator(this);

            m_graphics.PreferredBackBufferHeight = 700;
            m_graphics.PreferredBackBufferWidth = 1150;
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

        public bool IsState(GameState state)
        {
            return (State & state) == State;
        }

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

                DebugMessages.AddLogOnly("Changing State: " + state);

                InvokeStateChanged(args);


                if(State == GameState.MainMenu)
                {
                    DebugMessages.FlushLog();
                    Communicator.ClearAll();
                }

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
			Sound.Initialize();
            LoadScreens();
            Settings.SinglePlayer.LoadFromFile();

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
			Sound.LoadContent(Content);
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

			MediaPlayer.IsMuted = Settings.SinglePlayer.Mute.Value;

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
			ScreenManager.AddScreen(new GameLobbyScreen(this));
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
				DebugMessages.FlushLog();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}