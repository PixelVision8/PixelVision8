// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using MonoGame.Utilities;
using System.Runtime.InteropServices;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice : IDisposable
    {
        /// <summary>
        /// Indicates if DX9 style pixel addressing or current standard
        /// pixel addressing should be used. This flag is set to
        /// <c>false</c> by default. If `UseHalfPixelOffset` is
        /// `true` you have to add half-pixel offset to a Projection matrix.
        /// See also <see cref="GraphicsDeviceManager.PreferHalfPixelOffset"/>.
        /// </summary>
        /// <remarks>
        /// XNA uses DirectX9 for its graphics. DirectX9 interprets UV
        /// coordinates differently from other graphics API's. This is
        /// typically referred to as the half-pixel offset. MonoGame
        /// replicates XNA behavior if this flag is set to <c>true</c>.
        /// </remarks>
        // public bool UseHalfPixelOffset { get; private set; }

        private Viewport _viewport;

        private bool _isDisposed;

        // On Intel Integrated graphics, there is a fast hw unit for doing
        // clears to colors where all components are either 0 or 255.
        // Despite XNA4 using Purple here, we use black (in Release) to avoid
        // performance warnings on Intel/Mesa
        private static Color _discardColor = new Color(0, 0, 0, 255);
 
        private bool _blendFactorDirty;

        private bool _rasterizerStateDirty;

        private Rectangle _scissorRectangle;
        private bool _scissorRectangleDirty;

        private VertexBufferBindings _vertexBuffers;

        // private IndexBuffer _indexBuffer;
        private bool _indexBufferDirty;

        // private readonly RenderTargetBinding[] _currentRenderTargetBindings = new RenderTargetBinding[4];
        private int _currentRenderTargetCount;
        // private readonly RenderTargetBinding[] _tempRenderTargetBinding = new RenderTargetBinding[1];

        internal GraphicsCapabilities GraphicsCapabilities { get; private set; }

        // public TextureCollection VertexTextures { get; private set; }

        public SamplerStateCollection VertexSamplerStates { get; private set; }

        public TextureCollection Textures { get; private set; }

        public SamplerStateCollection SamplerStates { get; private set; }

        /// <summary>
        /// Get or set the color a <see cref="RenderTarget2D"/> is cleared to when it is set.
        /// </summary>
        public static Color DiscardColor {
			get { return _discardColor; }
			set { _discardColor = value; }
		}

        /// <summary>
        /// The active vertex shader.
        /// </summary>
        private Shader _vertexShader;
        private bool _vertexShaderDirty;

        /// <summary>
        /// The active pixel shader.
        /// </summary>
        private Shader _pixelShader;
        private bool _pixelShaderDirty;

        private readonly ConstantBufferCollection _vertexConstantBuffers = new ConstantBufferCollection(16);
        private readonly ConstantBufferCollection _pixelConstantBuffers = new ConstantBufferCollection(16);

        /// <summary>
        /// The cache of effects from unique byte streams.
        /// </summary>
        internal Dictionary<int, Effect> EffectCache;

        // Resources may be added to and removed from the list from many threads.
        // private readonly object _resourcesLock = new object();

		// TODO Graphics Device events need implementing
		// public event EventHandler<EventArgs> DeviceLost;
		public event EventHandler<EventArgs> DeviceReset;
		public event EventHandler<EventArgs> DeviceResetting;

        public event EventHandler<EventArgs> Disposing;

        internal event EventHandler<PresentationEventArgs> PresentationChanged;

        private int _maxVertexBufferSlots;
        internal int MaxTextureSlots;
        internal int MaxVertexTextureSlots;

        internal bool IsRenderTargetBound
        {
            get
            {
                return _currentRenderTargetCount > 0;
            }
        }

        public GraphicsAdapter Adapter
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDevice" /> class.
        /// </summary>
        /// <param name="adapter">The graphics adapter.</param>
        /// <param name="graphicsProfile">The graphics profile.</param>
        /// <param name="preferHalfPixelOffset"> Indicates if DX9 style pixel addressing or current standard pixel addressing should be used. This value is passed to <see cref="GraphicsDevice.UseHalfPixelOffset"/></param>
        /// <param name="presentationParameters">The presentation options.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="presentationParameters"/> is <see langword="null"/>.
        /// </exception>
        public GraphicsDevice(GraphicsAdapter adapter/*, GraphicsProfile graphicsProfile*/, /*bool preferHalfPixelOffset,*/ PresentationParameters presentationParameters)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");

            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");

            Adapter = adapter;

            PresentationParameters = presentationParameters;
            Setup();
            GraphicsCapabilities = new GraphicsCapabilities();
            GraphicsCapabilities.Initialize(this);

            Initialize();
        }

        private void Setup()
        {
#if DEBUG
            if (DisplayMode == null)
            {
                throw new Exception(
                    "Unable to determine the current display mode.  This can indicate that the " +
                    "game is not configured to be HiDPI aware under Windows 10 or later.  See " +
                    "https://github.com/MonoGame/MonoGame/issues/5040 for more information.");
            }
#endif

            // Initialize the main viewport
            _viewport = new Viewport (0, 0,
			                         DisplayMode.Width, DisplayMode.Height);
			_viewport.MaxDepth = 1.0f;

            PlatformSetup();

            VertexSamplerStates = new SamplerStateCollection(this, MaxVertexTextureSlots, true);

            Textures = new TextureCollection(this, MaxTextureSlots, false);
            SamplerStates = new SamplerStateCollection(this, MaxTextureSlots, false);

            EffectCache = new Dictionary<int, Effect>();
        }

        ~GraphicsDevice()
        {
            Dispose(false);
        }

        internal void Initialize()
        {
            PlatformInitialize();

            VertexSamplerStates.Clear();
            Textures.Clear();
            SamplerStates.Clear();

            // Clear constant buffers
            _vertexConstantBuffers.Clear();
            _pixelConstantBuffers.Clear();

            // Force set the buffers and shaders on next ApplyState() call
            _vertexBuffers = new VertexBufferBindings(_maxVertexBufferSlots);
            _indexBufferDirty = true;
            _vertexShaderDirty = true;
            _pixelShaderDirty = true;

            // Set the default scissor rect.
            _scissorRectangleDirty = true;
            ScissorRectangle = _viewport.Bounds;

            // Set the default render target.
            ApplyRenderTargets();
        }

        internal void ApplyState(bool applyShaders)
        {
            PlatformBeginApplyState();
            PlatformApplyState(applyShaders);
        }

        public void Clear(Color color)
        {
            PlatformClear(/*options,*/ color, _viewport.MaxDepth, 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {

                    // Clear the effect cache.
                    EffectCache.Clear();

                    PlatformDispose();
                }

                _isDisposed = true;
                EventHelpers.Raise(this, Disposing, EventArgs.Empty);
            }
        }

        public void Present()
        {
            // We cannot present with a RT set on the device.
            if (_currentRenderTargetCount != 0)
                throw new InvalidOperationException("Cannot call Present when a render target is active.");

            // _graphicsMetrics = new GraphicsMetrics();
            PlatformPresent();
        }

        public void Reset()
        {
            EventHelpers.Raise(this, DeviceResetting, EventArgs.Empty);

            // Update the back buffer.
            OnPresentationChanged();

            EventHelpers.Raise(this, PresentationChanged, new PresentationEventArgs(PresentationParameters));
            EventHelpers.Raise(this, DeviceReset, EventArgs.Empty);
       }

        public void Reset(PresentationParameters presentationParameters)
        {
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");

            PresentationParameters = presentationParameters;
            Reset();
        }

        public DisplayMode DisplayMode
        {
            get
            {
                var displayIndex = Sdl.Display.GetWindowDisplayIndex(SdlGameWindow.Instance.Handle);

                Sdl.Display.Mode mode;
                Sdl.Display.GetCurrentDisplayMode(displayIndex, out mode);

                return new DisplayMode(mode.Width, mode.Height/*, SurfaceFormat.Color*/);

            }
        }

        public PresentationParameters PresentationParameters
        {
            get;
            private set;
        }

        public Viewport Viewport
        {
            get
            {
                return _viewport;
            }

            set
            {
                _viewport = value;
                PlatformSetViewport(ref value);
            }
        }

        public Rectangle ScissorRectangle
        {
            get
            {
                return _scissorRectangle;
            }

            set
            {
                if (_scissorRectangle == value)
                    return;

                _scissorRectangle = value;
                _scissorRectangleDirty = true;
            }
        }

        internal void ApplyRenderTargets(/*RenderTargetBinding[] renderTargets*/)
        {
            var clearTarget = false;

            int renderTargetWidth;
            int renderTargetHeight;

                _currentRenderTargetCount = 0;

                PlatformApplyDefaultRenderTarget();
                clearTarget = true;//PresentationParameters.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = PresentationParameters.BackBufferWidth;
                renderTargetHeight = PresentationParameters.BackBufferHeight;
           
            // Set the viewport to the size of the first render target.
            Viewport = new Viewport(0, 0, renderTargetWidth, renderTargetHeight);

            // Set the scissor rectangle to the size of the first render target.
            ScissorRectangle = new Rectangle(0, 0, renderTargetWidth, renderTargetHeight);

            // In XNA 4, because of hardware limitations on Xbox, when
            // a render target doesn't have PreserveContents as its usage
            // it is cleared before being rendered to.
            if (clearTarget)
                Clear(DiscardColor);
        }

        internal Shader VertexShader
        {
            get { return _vertexShader; }

            set
            {
                if (_vertexShader == value)
                    return;

                _vertexShader = value;
                _vertexConstantBuffers.Clear();
                _vertexShaderDirty = true;
            }
        }

        internal Shader PixelShader
        {
            get { return _pixelShader; }

            set
            {
                if (_pixelShader == value)
                    return;

                _pixelShader = value;
                _pixelConstantBuffers.Clear();
                _pixelShaderDirty = true;
            }
        }

        internal void SetConstantBuffer(ShaderStage stage, int slot, ConstantBuffer buffer)
        {
            if (stage == ShaderStage.Vertex)
                _vertexConstantBuffers[slot] = buffer;
            else
                _pixelConstantBuffers[slot] = buffer;
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given array of vertices with 16-bit indices.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex to draw.</param>
        /// <param name="indexOffset">The index in the array of indices of the first index to use</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="numVertices">The number of vertices to draw.</param>
        /// <param name="indexData">The index data.</param>
        /// <param name="vertexDeclaration">The layout of the vertices.</param>
        /// <remarks>All indices in the vertex buffer are interpreted relative to the specified <paramref name="vertexOffset"/>.
        /// For example a value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset"/>
        /// in the array of vertices.</remarks>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            // These parameter checks are a duplicate of the checks in the int[] overload of DrawUserIndexedPrimitives.
            // Inlined here for efficiency.

            if (vertexData == null || vertexData.Length == 0)
                throw new ArgumentNullException("vertexData");

            if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
                throw new ArgumentOutOfRangeException("vertexOffset");

            if (numVertices <= 0 || numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices");

            if (vertexOffset + numVertices > vertexData.Length)
                throw new ArgumentOutOfRangeException("numVertices");

            if (indexData == null || indexData.Length == 0)
                throw new ArgumentNullException("indexData");

            if (indexOffset < 0 || indexOffset >= indexData.Length)
                throw new ArgumentOutOfRangeException("indexOffset");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (indexOffset + GetElementCountArray(primitiveType, primitiveCount) > indexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            if (vertexDeclaration.VertexStride < ReflectionHelpers.SizeOf<T>.Get())
                throw new ArgumentOutOfRangeException("vertexDeclaration", "Vertex stride of vertexDeclaration should be at least as big as the stride of the actual vertices.");

            PlatformDrawUserIndexedPrimitives<T>(primitiveType, vertexData, vertexOffset, numVertices, indexData, indexOffset, primitiveCount, vertexDeclaration);

        }

        private static int GetElementCountArray(PrimitiveType primitiveType, int primitiveCount)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return primitiveCount * 2;
                case PrimitiveType.LineStrip:
                    return primitiveCount + 1;
                case PrimitiveType.TriangleList:
                    return primitiveCount * 3;
                case PrimitiveType.TriangleStrip:
                    return primitiveCount + 2;
            }

            throw new NotSupportedException();
        }

    }
}
