// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public class PresentationParameters
    {
        #region Constants

        #endregion Constants

        #region Private Fields

        private int backBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
        private int backBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
        private bool isFullScreen;
        private bool hardwareModeSwitch = true;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Create a <see cref="PresentationParameters"/> instance with default values for all properties.
        /// </summary>
        public PresentationParameters()
        {
            Clear();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Get or set the height of the back buffer.
        /// </summary>
        public int BackBufferHeight
        {
            get { return backBufferHeight; }
            set { backBufferHeight = value; }
        }

        /// <summary>
        /// Get or set the width of the back buffer.
        /// </summary>
        public int BackBufferWidth
        {
            get { return backBufferWidth; }
            set { backBufferWidth = value; }
        }

        /// <summary>
        /// Get or set a value indicating if we are in full screen mode.
        /// </summary>
        public bool IsFullScreen
        {
			get
            {
				 return isFullScreen;
            }
            set
            {
                isFullScreen = value;
			}
        }

        /// <summary>
        /// If <code>true</code> the <see cref="GraphicsDevice"/> will do a mode switch
        /// when going to full screen mode. If <code>false</code> it will instead do a
        /// soft full screen by maximizing the window and making it borderless.
        /// </summary>
        public bool HardwareModeSwitch
        {
            get { return hardwareModeSwitch; }
            set { hardwareModeSwitch = value; }
        }

        /// <summary>
        /// Get or set the presentation interval.
        /// </summary>
        // public PresentInterval PresentationInterval { get; set; }

        #endregion Properties


        #region Methods

        /// <summary>
        /// Reset all properties to their default values.
        /// </summary>
        public void Clear()
        {
            backBufferWidth = GraphicsDeviceManager.DefaultBackBufferWidth;
            backBufferHeight = GraphicsDeviceManager.DefaultBackBufferHeight;
            // PresentationInterval = PresentInterval.Default;
        }

        #endregion Methods

    }
}
