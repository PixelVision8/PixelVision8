// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
// using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Used to initialize and control the presentation of the graphics device.
    /// </summary>
    public partial class GraphicsDeviceManager : IGraphicsDeviceService, IDisposable
    {
        private readonly Game _game;
        private GraphicsDevice _graphicsDevice;
        private bool _initialized = false;
        private int _preferredBackBufferHeight;
        private int _preferredBackBufferWidth;
        private SurfaceFormat _preferredBackBufferFormat;
        private bool _preferMultiSampling;
        private bool _synchronizedWithVerticalRetrace = true;
        private bool _drawBegun;
        private bool _disposed;
        private bool _hardwareModeSwitch = true;
        private bool _preferHalfPixelOffset = false;
        private bool _wantFullScreen;
        private bool _shouldApplyChanges;

        /// <summary>
        /// The default back buffer width.
        /// </summary>
        public static readonly int DefaultBackBufferWidth = 512;

        /// <summary>
        /// The default back buffer height.
        /// </summary>
        public static readonly int DefaultBackBufferHeight = 480;

        /// <summary>
        /// Associates this graphics device manager to a game instances.
        /// </summary>
        /// <param name="game">The game instance to attach.</param>
        public GraphicsDeviceManager(Game game)
        {
            if (game == null)
                throw new ArgumentNullException("game", "Game cannot be null.");

            _game = game;

            _preferredBackBufferFormat = SurfaceFormat.Color;
            _synchronizedWithVerticalRetrace = true;

            // Assume the window client size as the default back
            // buffer resolution in the landscape orientation.
            var clientBounds = _game.Window.ClientBounds;
            if (clientBounds.Width >= clientBounds.Height)
            {
                _preferredBackBufferWidth = clientBounds.Width;
                _preferredBackBufferHeight = clientBounds.Height;
            }
            else
            {
                _preferredBackBufferWidth = clientBounds.Height;
                _preferredBackBufferHeight = clientBounds.Width;
            }

            // Default to windowed mode... this is ignored on platforms that don't support it.
            _wantFullScreen = false;

            _game.graphicsDeviceManager = this;

        }

        ~GraphicsDeviceManager()
        {
            Dispose(false);
        }

        public void CreateDevice()
        {
            if (_graphicsDevice != null)
                return;

            try
            {
                if (!_initialized)
                    Initialize();

                var gdi = DoPreparingDeviceSettings();
                CreateDevice(gdi);
            }
            catch (NoSuitableGraphicsDeviceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NoSuitableGraphicsDeviceException("Failed to create graphics device!", ex);
            }
        }

        private void CreateDevice(GraphicsDeviceInformation gdi)
        {
            if (_graphicsDevice != null)
                return;

            _graphicsDevice = new GraphicsDevice(gdi.Adapter/*, gdi.GraphicsProfile, this.PreferHalfPixelOffset*/, gdi.PresentationParameters);
            _shouldApplyChanges = false;

            _graphicsDevice.PresentationChanged += OnPresentationChanged;

        }

        #region IGraphicsDeviceService Members


        /// <summary>
        /// This populates a GraphicsDeviceInformation instance and invokes PreparingDeviceSettings to
        /// allow users to change the settings. Then returns that GraphicsDeviceInformation.
        /// Throws NullReferenceException if users set GraphicsDeviceInformation.PresentationParameters to null.
        /// </summary>
        private GraphicsDeviceInformation DoPreparingDeviceSettings()
        {
            var gdi = new GraphicsDeviceInformation();
            PrepareGraphicsDeviceInformation(gdi);
            return gdi;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_graphicsDevice != null)
                    {
                        _graphicsDevice.Dispose();
                        _graphicsDevice = null;
                    }
                }
                _disposed = true;
                // EventHelpers.Raise(this, Disposed, EventArgs.Empty);
            }
        }

        #endregion

 
        private void PreparePresentationParameters(PresentationParameters presentationParameters)
        {
            presentationParameters.BackBufferWidth = _preferredBackBufferWidth;
            presentationParameters.BackBufferHeight = _preferredBackBufferHeight;
            presentationParameters.IsFullScreen = _wantFullScreen;
            presentationParameters.HardwareModeSwitch = _hardwareModeSwitch;
            // presentationParameters.PresentationInterval = _synchronizedWithVerticalRetrace ? PresentInterval.One : PresentInterval.Immediate;
        }

        private void PrepareGraphicsDeviceInformation(GraphicsDeviceInformation gdi)
        {
            gdi.Adapter = GraphicsAdapter.DefaultAdapter;
            // gdi.GraphicsProfile = GraphicsProfile;
            var pp = new PresentationParameters();
            PreparePresentationParameters(pp);
            gdi.PresentationParameters = pp;
        }

        /// <summary>
        /// Applies any pending property changes to the graphics device.
        /// </summary>
        public void ApplyChanges()
        {
            // If the device hasn't been created then create it now.
            if (_graphicsDevice == null)
                CreateDevice();

            if (!_shouldApplyChanges)
                return;

            _shouldApplyChanges = false;

            // populates a gdi with settings in this gdm and allows users to override them with
            // PrepareDeviceSettings event this information should be applied to the GraphicsDevice
            var gdi = DoPreparingDeviceSettings();

            GraphicsDevice.Reset(gdi.PresentationParameters);
        }

        partial void PlatformInitialize(/*PresentationParameters presentationParameters*/);

        private void Initialize()
        {
           
            // Allow for any per-platform changes to the presentation.
            PlatformInitialize(/*presentationParameters*/);

            _initialized = true;
        }

        private void OnPresentationChanged(object sender, PresentationEventArgs args)
        {
            _game.Platform.OnPresentationChanged(args.PresentationParameters);
        }

        /// <summary>
        /// Returns the graphics device for this manager.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return _graphicsDevice;
            }
        }

        /// <summary>
        /// Indicates the desire to switch into fullscreen mode.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set fullscreen mode during initialization.  If
        /// set after startup you must call ApplyChanges() for the fullscreen mode to be changed.
        /// Note that for some platforms that do not support windowed modes this property has no affect.
        /// </remarks>
        public bool IsFullScreen
        {
            get { return _wantFullScreen; }
            set
            {
                _shouldApplyChanges = true;
                _wantFullScreen = value;
            }
        }

        /// <summary>
        /// Gets or sets the boolean which defines how window switches from windowed to fullscreen state.
        /// "Hard" mode(true) is slow to switch, but more effecient for performance, while "soft" mode(false) is vice versa.
        /// The default value is <c>true</c>.
        /// </summary>
        public bool HardwareModeSwitch
        {
            get { return _hardwareModeSwitch;}
            set
            {
                _shouldApplyChanges = true;
                _hardwareModeSwitch = value;
            }
        }

        /// <summary>
        /// Indicates the desired back buffer height in pixels.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set the height during initialization.  If
        /// set after startup you must call ApplyChanges() for the height to be changed.
        /// </remarks>
        public int PreferredBackBufferHeight
        {
            get
            {
                return _preferredBackBufferHeight;
            }
            set
            {
                _shouldApplyChanges = true;
                _preferredBackBufferHeight = value;
            }
        }

        /// <summary>
        /// Indicates the desired back buffer width in pixels.
        /// </summary>
        /// <remarks>
        /// When called at startup this will automatically set the width during initialization.  If
        /// set after startup you must call ApplyChanges() for the width to be changed.
        /// </remarks>
        public int PreferredBackBufferWidth
        {
            get
            {
                return _preferredBackBufferWidth;
            }
            set
            {
                _shouldApplyChanges = true;
                _preferredBackBufferWidth = value;
            }
        }

    }
}
