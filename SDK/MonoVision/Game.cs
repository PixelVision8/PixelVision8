// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Framework
{
    public partial class Game : IDisposable
    {
        // private GameComponentCollection _components;
        // private GameServiceContainer _services;
        // private ContentManager _content;
        internal GamePlatform Platform;

        // private SortingFilteringCollection<IDrawable> _drawables =
        //     new SortingFilteringCollection<IDrawable>(
        //         d => d.Visible,
        //         (d, handler) => d.VisibleChanged += handler,
        //         (d, handler) => d.VisibleChanged -= handler,
        //         (d1 ,d2) => Comparer<int>.Default.Compare(d1.DrawOrder, d2.DrawOrder),
        //         (d, handler) => d.DrawOrderChanged += handler,
        //         (d, handler) => d.DrawOrderChanged -= handler);
        //
        // private SortingFilteringCollection<IUpdateable> _updateables =
        //     new SortingFilteringCollection<IUpdateable>(
        //         u => u.Enabled,
        //         (u, handler) => u.EnabledChanged += handler,
        //         (u, handler) => u.EnabledChanged -= handler,
        //         (u1, u2) => Comparer<int>.Default.Compare(u1.UpdateOrder, u2.UpdateOrder),
        //         (u, handler) => u.UpdateOrderChanged += handler,
        //         (u, handler) => u.UpdateOrderChanged -= handler);

        private GraphicsDeviceManager _graphicsDeviceManager;
        private IGraphicsDeviceService _graphicsDeviceService;

        private bool _initialized = false;
        // private bool _isFixedTimeStep = true;

        private TimeSpan _targetElapsedTime = TimeSpan.FromTicks(166667); // 60fps
        private TimeSpan _inactiveSleepTime = TimeSpan.FromSeconds(0.02);

        private TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);

        private bool _shouldExit;
        private bool _suppressDraw;

        // partial void PlatformConstruct();

        public Game()
        {
            
            // _services = new GameServiceContainer();
            Platform = new SdlGamePlatform(this);
            Platform.Activated += OnActivated;
            Platform.Deactivated += OnDeactivated;
            // _services.AddService(typeof(GamePlatform), Platform);

        }

        ~Game()
        {
            Dispose(false);
        }

		[System.Diagnostics.Conditional("DEBUG")]
		internal void Log(string Message)
		{
			if (Platform != null) Platform.Log(Message);
		}

        #region IDisposable Implementation

        private bool _isDisposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            // EventHelpers.Raise(this, Disposed, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    
                    if (_graphicsDeviceManager != null)
                    {
                        (_graphicsDeviceManager as GraphicsDeviceManager).Dispose();
                        _graphicsDeviceManager = null;
                    }

                    if (Platform != null)
                    {
                        Platform.Activated -= OnActivated;
                        Platform.Deactivated -= OnDeactivated;
                        // _services.RemoveService(typeof(GamePlatform));

                        Platform.Dispose();
                        Platform = null;
                    }

                    if (SoundEffect._systemState == SoundEffect.SoundSystemState.Initialized)
                        SoundEffect.PlatformShutdown();
                }

                _isDisposed = true;
                
            }
        }

        [System.Diagnostics.DebuggerNonUserCode]
        private void AssertNotDisposed()
        {
            if (_isDisposed)
            {
                string name = GetType().Name;
                throw new ObjectDisposedException(
                    name, string.Format("The {0} object was used after being Disposed.", name));
            }
        }

        #endregion IDisposable Implementation

        #region Properties

        public TimeSpan InactiveSleepTime
        {
            get { return _inactiveSleepTime; }
            set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("The time must be positive.", default(Exception));

                _inactiveSleepTime = value;
            }
        }

        public bool IsActive
        {
            get { return Platform.IsActive; }
        }

        public bool IsMouseVisible
        {
            get { return Platform.IsMouseVisible; }
            set { Platform.IsMouseVisible = value; }
        }

        public TimeSpan TargetElapsedTime
        {
            get { return _targetElapsedTime; }
            set
            {
                // Give GamePlatform implementations an opportunity to override
                // the new value.
                value = Platform.TargetElapsedTimeChanging(value);

                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException(
                        "The time must be positive and non-zero.", default(Exception));

                if (value != _targetElapsedTime)
                {
                    _targetElapsedTime = value;
                    Platform.TargetElapsedTimeChanged();
                }
            }
        }

        // public GameServiceContainer Services {
        //     get { return _services; }
        // }

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (_graphicsDeviceService == null)
                {
                    _graphicsDeviceService = graphicsDeviceManager;
                    // (IGraphicsDeviceService)
                    //     Services.GetService(typeof(IGraphicsDeviceService));

                    if (_graphicsDeviceService == null)
                        throw new InvalidOperationException("No Graphics Device Service");
                }
                return _graphicsDeviceService.GraphicsDevice;
            }
        }

        [CLSCompliant(false)]
        public GameWindow Window
        {
            get { return Platform.Window; }
        }

        #endregion Properties

        #region Public Methods

        public void Run()
        {
            AssertNotDisposed();
            if (!Platform.BeforeRun())
            {
                BeginRun();
                _gameTimer = Stopwatch.StartNew();
                return;
            }

            if (!_initialized) {
                DoInitialize ();
                _initialized = true;
            }

            BeginRun();
            _gameTimer = Stopwatch.StartNew();

            // XNA runs one Update even before showing the window
            DoUpdate(new GameTime());

            Platform.RunLoop();
            EndRun();
			DoExiting();

        }

        private TimeSpan _accumulatedElapsedTime;
        private readonly GameTime _gameTime = new GameTime();
        private Stopwatch _gameTimer;
        private long _previousTicks = 0;
        private int _updateFrameLag;

        public void Tick()
        {
            // NOTE: This code is very sensitive and can break very badly
            // with even what looks like a safe change.  Be sure to test
            // any change fully in both the fixed and variable timestep
            // modes across multiple devices and platforms.

        RetryTick:

            if (!IsActive && (InactiveSleepTime.TotalMilliseconds >= 1.0))
            {

                System.Threading.Thread.Sleep((int)InactiveSleepTime.TotalMilliseconds);

            }

            // Advance the accumulated elapsed time.
            var currentTicks = _gameTimer.Elapsed.Ticks;
            _accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - _previousTicks);
            _previousTicks = currentTicks;

            if (_accumulatedElapsedTime < TargetElapsedTime)
            {
                // Sleep for as long as possible without overshooting the update time
                var sleepTime = (TargetElapsedTime - _accumulatedElapsedTime).TotalMilliseconds;
                // We only have a precision timer on Windows, so other platforms may still overshoot

                if (sleepTime >= 2.0)
                    System.Threading.Thread.Sleep(1);

                // Keep looping until it's time to perform the next update
                goto RetryTick;
            }

            // Do not allow any update to take longer than our maximum.
            if (_accumulatedElapsedTime > _maxElapsedTime)
                _accumulatedElapsedTime = _maxElapsedTime;

            // if (IsFixedTimeStep)
            // {
                _gameTime.ElapsedGameTime = TargetElapsedTime;
                var stepCount = 0;

                // Perform as many full fixed length time steps as we can.
                while (_accumulatedElapsedTime >= TargetElapsedTime && !_shouldExit)
                {
                    _gameTime.TotalGameTime += TargetElapsedTime;
                    _accumulatedElapsedTime -= TargetElapsedTime;
                    ++stepCount;

                    DoUpdate(_gameTime);
                }

                //Every update after the first accumulates lag
                _updateFrameLag += Math.Max(0, stepCount - 1);

                //If we think we are running slowly, wait until the lag clears before resetting it
                if (_gameTime.IsRunningSlowly)
                {
                    if (_updateFrameLag == 0)
                        _gameTime.IsRunningSlowly = false;
                }
                else if (_updateFrameLag >= 5)
                {
                    //If we lag more than 5 frames, start thinking we are running slowly
                    _gameTime.IsRunningSlowly = true;
                }

                //Every time we just do one update and one draw, then we are not running slowly, so decrease the lag
                if (stepCount == 1 && _updateFrameLag > 0)
                    _updateFrameLag--;

                // Draw needs to know the total elapsed time
                // that occured for the fixed length updates.
                _gameTime.ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
            // }
            // else
            // {
            //     // Perform a single variable length update.
            //     _gameTime.ElapsedGameTime = _accumulatedElapsedTime;
            //     _gameTime.TotalGameTime += _accumulatedElapsedTime;
            //     _accumulatedElapsedTime = TimeSpan.Zero;
            //
            //     DoUpdate(_gameTime);
            // }

            // Draw unless the update suppressed it.
            if (_suppressDraw)
                _suppressDraw = false;
            else
            {
                DoDraw(_gameTime);
            }

            if (_shouldExit)
            {
                Platform.Exit();
                _shouldExit = false; //prevents perpetual exiting on platforms supporting resume.
            }
        }

        #endregion

        #region Protected Methods

        protected virtual bool BeginDraw() { return true; }
        protected virtual void EndDraw()
        {
            Platform.Present();
        }

        protected virtual void BeginRun() { }
        protected virtual void EndRun() { }

        // protected virtual void LoadContent() { }
        // protected virtual void UnloadContent() { }

        protected virtual void Initialize()
        {
            // TODO: This should be removed once all platforms use the new GraphicsDeviceManager
// #if !(WINDOWS && DIRECTX)
//             applyChanges(graphicsDeviceManager);
// #endif

            // According to the information given on MSDN (see link below), all
            // GameComponents in Components at the time Initialize() is called
            // are initialized.
            // http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.game.initialize.aspx
            // Initialize all existing components
            // InitializeExistingComponents();

            // _graphicsDeviceService = (IGraphicsDeviceService)
            //     Services.GetService(typeof(IGraphicsDeviceService));

            // if (_graphicsDeviceService != null &&
            //     _graphicsDeviceService.GraphicsDevice != null)
            // {
            //     // LoadContent();
            // }
        }

        // private static readonly Action<IDrawable, GameTime> DrawAction =
            // (drawable, gameTime) => drawable.Draw(gameTime);

        protected virtual void Draw(GameTime gameTime)
        {

            // _drawables.ForEachFilteredItem(DrawAction, gameTime);
        }

        // private static readonly Action<IUpdateable, GameTime> UpdateAction =
        //     (updateable, gameTime) => updateable.Update(gameTime);

        protected virtual void Update(GameTime gameTime)
        {
            // _updateables.ForEachFilteredItem(UpdateAction, gameTime);
		}

        protected virtual void OnExiting(object sender, EventArgs args)
        {
            // EventHelpers.Raise(sender, Exiting, args);
        }

		protected virtual void OnActivated(object sender, EventArgs args)
		{
			AssertNotDisposed();
            // EventHelpers.Raise(sender, Activated, args);
		}

		protected virtual void OnDeactivated(object sender, EventArgs args)
		{
			AssertNotDisposed();
            // EventHelpers.Raise(sender, Deactivated, args);
		}

        #endregion Protected Methods

        #region Internal Methods

        internal void DoUpdate(GameTime gameTime)
        {
            AssertNotDisposed();
            if (Platform.BeforeUpdate(gameTime))
            {
                Update(gameTime);
            }
        }

        internal void DoDraw(GameTime gameTime)
        {
            AssertNotDisposed();
            // Draw and EndDraw should not be called if BeginDraw returns false.
            // http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
            // http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
            if (Platform.BeforeDraw(gameTime) && BeginDraw())
            {
                Draw(gameTime);
                EndDraw();
            }
        }

        internal void DoInitialize()
        {
            AssertNotDisposed();
            if (GraphicsDevice == null && graphicsDeviceManager != null)
                _graphicsDeviceManager.CreateDevice();

            Platform.BeforeInitialize();
            Initialize();

            // We need to do this after virtual Initialize(...) is called.
            // 1. Categorize components into IUpdateable and IDrawable lists.
            // 2. Subscribe to Added/Removed events to keep the categorized
            //    lists synced and to Initialize future components as they are
            //    added.
            // CategorizeComponents();
            // _components.ComponentAdded += Components_ComponentAdded;
            // _components.ComponentRemoved += Components_ComponentRemoved;
        }

		internal void DoExiting()
		{
			OnExiting(this, EventArgs.Empty);
			// UnloadContent();
		}

        #endregion Internal Methods

        internal GraphicsDeviceManager graphicsDeviceManager
        {
            get
            {
                // if (_graphicsDeviceManager == null)
                // {
                //     _graphicsDeviceManager = (IGraphicsDeviceManager)
                //         Services.GetService(typeof(IGraphicsDeviceManager));
                // }
                return (GraphicsDeviceManager)_graphicsDeviceManager;
            }
            set
            {
                if (_graphicsDeviceManager != null)
                    throw new InvalidOperationException("GraphicsDeviceManager already registered for this Game object");
                _graphicsDeviceManager = value;
            }
        }

    }
}
