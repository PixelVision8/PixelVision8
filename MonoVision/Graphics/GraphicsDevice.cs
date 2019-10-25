// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Utilities;
using System;
using System.Collections.Generic;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice : IDisposable
    {
        private Viewport _viewport;

        private bool _isDisposed;

        //        private Color _blendFactor = Color.White;
        //        private bool _blendFactorDirty;

        //        private BlendState _blendState;
        //        private BlendState _actualBlendState;
        //        private bool _blendStateDirty;
        //
        //        private BlendState _blendStateAdditive;
        //        private BlendState _blendStateAlphaBlend;
        //        private BlendState _blendStateNonPremultiplied;
        //        private BlendState _blendStateOpaque;

        //        private DepthStencilState _depthStencilState;
        //        private DepthStencilState _actualDepthStencilState;
        //        private bool _depthStencilStateDirty;

        //        private DepthStencilState _depthStencilStateDefault;
        //        private DepthStencilState _depthStencilStateDepthRead;
        //        private DepthStencilState _depthStencilStateNone;

        //        private RasterizerState _rasterizerState;
        //        private RasterizerState _actualRasterizerState;
        //        private bool _rasterizerStateDirty;

        //        private RasterizerState _rasterizerStateCullClockwise;
        //        private RasterizerState _rasterizerStateCullCounterClockwise;
        //        private RasterizerState _rasterizerStateCullNone;

        private Rectangle _scissorRectangle;
        private bool _scissorRectangleDirty;

        private VertexBufferBindings _vertexBuffers;
        //        private bool _vertexBuffersDirty;

        private IndexBuffer _indexBuffer;
        private bool _indexBufferDirty;

        private readonly RenderTargetBinding[] _currentRenderTargetBindings = new RenderTargetBinding[4];
        private int _currentRenderTargetCount;
        private readonly RenderTargetBinding[] _tempRenderTargetBinding = new RenderTargetBinding[1];

        internal GraphicsCapabilities GraphicsCapabilities { get; private set; }

        public TextureCollection VertexTextures { get; private set; }

        public SamplerStateCollection VertexSamplerStates { get; private set; }

        public TextureCollection Textures { get; private set; }

        public SamplerStateCollection SamplerStates { get; private set; }

        // On Intel Integrated graphics, there is a fast hw unit for doing
        // clears to colors where all components are either 0 or 255.
        // Despite XNA4 using Purple here, we use black (in Release) to avoid
        // performance warnings on Intel/Mesa
        private static readonly Color DiscardColor = new Color(0, 0, 0, 255);

        /// <summary>
        /// The active vertex shader.
        /// </summary>
        private Shader _vertexShader;
        private bool _vertexShaderDirty;
        private bool VertexShaderDirty
        {
            get { return _vertexShaderDirty; }
        }

        /// <summary>
        /// The active pixel shader.
        /// </summary>
        private Shader _pixelShader;
        private bool _pixelShaderDirty;
        private bool PixelShaderDirty
        {
            get { return _pixelShaderDirty; }
        }

        private readonly ConstantBufferCollection _vertexConstantBuffers = new ConstantBufferCollection(ShaderStage.Vertex, 16);
        private readonly ConstantBufferCollection _pixelConstantBuffers = new ConstantBufferCollection(ShaderStage.Pixel, 16);

        /// <summary>
        /// The cache of effects from unique byte streams.
        /// </summary>
        internal Dictionary<int, Effect> EffectCache;

        // Resources may be added to and removed from the list from many threads.
        private readonly object _resourcesLock = new object();

        // Use WeakReference for the global resources list as we do not know when a resource
        // may be disposed and collected. We do not want to prevent a resource from being
        // collected by holding a strong reference to it in this list.
        private readonly List<WeakReference> _resources = new List<WeakReference>();

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

        internal GraphicsMetrics _graphicsMetrics;

        /// <summary>
        /// The rendering information for debugging and profiling.
        /// The metrics are reset every frame after draw within <see cref="GraphicsDevice.Present"/>. 
        /// </summary>
        public GraphicsMetrics Metrics { get { return _graphicsMetrics; } set { _graphicsMetrics = value; } }

        internal GraphicsDevice(GraphicsDeviceInformation gdi)
            : this(gdi.Adapter, gdi.GraphicsProfile, gdi.PresentationParameters)
        {
        }

        internal GraphicsDevice()
        {
            PresentationParameters = new PresentationParameters();
            //            PresentationParameters.DepthStencilFormat = DepthFormat.Depth24;
            Setup();
            GraphicsCapabilities = new GraphicsCapabilities();
            GraphicsCapabilities.Initialize(this);
            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDevice" /> class.
        /// </summary>
        /// <param name="adapter">The graphics adapter.</param>
        /// <param name="graphicsProfile">The graphics profile.</param>
        /// <param name="presentationParameters">The presentation options.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="presentationParameters"/> is <see langword="null"/>.
        /// </exception>
        public GraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, PresentationParameters presentationParameters)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");
            if (!adapter.IsProfileSupported(graphicsProfile))
                throw new NoSuitableGraphicsDeviceException(String.Format("Adapter '{0}' does not support the {1} profile.", adapter.Description, graphicsProfile));
            if (presentationParameters == null)
                throw new ArgumentNullException("presentationParameters");
            Adapter = adapter;
            PresentationParameters = presentationParameters;
            _graphicsProfile = graphicsProfile;
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
            _viewport = new Viewport(0, 0,
                                     DisplayMode.Width, DisplayMode.Height);
            _viewport.MaxDepth = 1.0f;

            PlatformSetup();

            VertexTextures = new TextureCollection(this, MaxVertexTextureSlots, true);
            VertexSamplerStates = new SamplerStateCollection(this, MaxVertexTextureSlots, true);

            Textures = new TextureCollection(this, MaxTextureSlots, false);
            SamplerStates = new SamplerStateCollection(this, MaxTextureSlots, false);

            EffectCache = new Dictionary<int, Effect>();
        }

        ~GraphicsDevice()
        {
            Dispose(false);
        }

        internal int GetClampedMultisampleCount(int multiSampleCount)
        {
            if (multiSampleCount > 1)
            {
                // Round down MultiSampleCount to the nearest power of two
                // hack from http://stackoverflow.com/a/2681094
                // Note: this will return an incorrect, but large value
                // for very large numbers. That doesn't matter because
                // the number will get clamped below anyway in this case.
                var msc = multiSampleCount;
                msc = msc | (msc >> 1);
                msc = msc | (msc >> 2);
                msc = msc | (msc >> 4);
                msc -= (msc >> 1);
                // and clamp it to what the device can handle
                if (msc > GraphicsCapabilities.MaxMultiSampleCount)
                    msc = GraphicsCapabilities.MaxMultiSampleCount;

                return msc;
            }
            else return 0;
        }

        internal void Initialize()
        {
            PlatformInitialize();

            // Clear the texture and sampler collections forcing
            // the state to be reapplied.
            VertexTextures.Clear();
            VertexSamplerStates.Clear();
            Textures.Clear();
            SamplerStates.Clear();

            // Clear constant buffers
            _vertexConstantBuffers.Clear();
            _pixelConstantBuffers.Clear();

            // Force set the buffers and shaders on next ApplyState() call
            _vertexBuffers = new VertexBufferBindings(_maxVertexBufferSlots);
            //            _vertexBuffersDirty = true;
            _indexBufferDirty = true;
            _vertexShaderDirty = true;
            _pixelShaderDirty = true;

            // Set the default scissor rect.
            _scissorRectangleDirty = true;
            ScissorRectangle = _viewport.Bounds;

            // Set the default render target.
            ApplyRenderTargets(null);
        }

        internal void ApplyState(bool applyShaders)
        {
            PlatformBeginApplyState();

            PlatformApplyBlend();


            PlatformApplyState(applyShaders);
        }

        public void Clear(Color color)
        {
            //            var options = ClearOptions.Target;
            //            options |= ClearOptions.DepthBuffer;
            //            options |= ClearOptions.Stencil;
            PlatformClear(/*options,*/ color.ToVector4(), _viewport.MaxDepth, 0);

            unchecked
            {
                _graphicsMetrics._clearCount++;
            }
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
                    // Dispose of all remaining graphics resources before disposing of the graphics device
                    lock (_resourcesLock)
                    {
                        foreach (var resource in _resources.ToArray())
                        {
                            var target = resource.Target as IDisposable;
                            if (target != null)
                                target.Dispose();
                        }
                        _resources.Clear();
                    }

                    // Clear the effect cache.
                    EffectCache.Clear();

                    PlatformDispose();
                }

                _isDisposed = true;
                EventHelpers.Raise(this, Disposing, EventArgs.Empty);
            }
        }

        internal void AddResourceReference(WeakReference resourceReference)
        {
            lock (_resourcesLock)
            {
                _resources.Add(resourceReference);
            }
        }

        internal void RemoveResourceReference(WeakReference resourceReference)
        {
            lock (_resourcesLock)
            {
                _resources.Remove(resourceReference);
            }
        }

        public void Present()
        {
            // We cannot present with a RT set on the device.
            if (_currentRenderTargetCount != 0)
                throw new InvalidOperationException("Cannot call Present when a render target is active.");

            _graphicsMetrics = new GraphicsMetrics();
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
                return Adapter.CurrentDisplayMode;
            }
        }

        public GraphicsDeviceStatus GraphicsDeviceStatus
        {
            get
            {
                return GraphicsDeviceStatus.Normal;
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

        private readonly GraphicsProfile _graphicsProfile;
        public GraphicsProfile GraphicsProfile
        {
            get { return _graphicsProfile; }
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

        public void SetRenderTarget(RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _tempRenderTargetBinding[0] = new RenderTargetBinding(renderTarget);
                SetRenderTargets(_tempRenderTargetBinding);
            }
        }

        public void SetRenderTargets(params RenderTargetBinding[] renderTargets)
        {
            // Avoid having to check for null and zero length.
            var renderTargetCount = 0;
            if (renderTargets != null)
            {
                renderTargetCount = renderTargets.Length;
                if (renderTargetCount == 0)
                {
                    renderTargets = null;
                }
            }

            // Try to early out if the current and new bindings are equal.
            if (_currentRenderTargetCount == renderTargetCount)
            {
                return;
            }

            ApplyRenderTargets(renderTargets);

            if (renderTargetCount == 0)
            {
                unchecked
                {
                    _graphicsMetrics._targetCount++;
                }
            }
            else
            {
                unchecked
                {
                    _graphicsMetrics._targetCount += renderTargetCount;
                }
            }
        }

        internal void ApplyRenderTargets(RenderTargetBinding[] renderTargets)
        {
            var clearTarget = false;

            PlatformResolveRenderTargets();

            // Clear the current bindings.
            Array.Clear(_currentRenderTargetBindings, 0, _currentRenderTargetBindings.Length);

            int renderTargetWidth;
            int renderTargetHeight;
            if (renderTargets == null)
            {
                _currentRenderTargetCount = 0;

                PlatformApplyDefaultRenderTarget();
                clearTarget = PresentationParameters.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = PresentationParameters.BackBufferWidth;
                renderTargetHeight = PresentationParameters.BackBufferHeight;
            }
            else
            {
                // Copy the new bindings.
                Array.Copy(renderTargets, _currentRenderTargetBindings, renderTargets.Length);
                _currentRenderTargetCount = renderTargets.Length;

                var renderTarget = PlatformApplyRenderTargets();

                // We clear the render target if asked.
                clearTarget = renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents;

                renderTargetWidth = renderTarget.Width;
                renderTargetHeight = renderTarget.Height;
            }

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

        private void SetIndexBuffer(IndexBuffer indexBuffer)
        {
            if (_indexBuffer == indexBuffer)
                return;

            _indexBuffer = indexBuffer;
            _indexBufferDirty = true;
        }

        public IndexBuffer Indices { set { SetIndexBuffer(value); } get { return _indexBuffer; } }

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
        /// Draw geometry by indexing into the vertex buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives to render from the index buffer.</param>
        public void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            if (_vertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawIndexedPrimitives.");

            if (_vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawIndexedPrimitives.");

            if (_indexBuffer == null)
                throw new InvalidOperationException("Index buffer must be set before calling DrawIndexedPrimitives.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            PlatformDrawIndexedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount);

            unchecked
            {
                _graphicsMetrics._drawCount++;
                _graphicsMetrics._primitiveCount += primitiveCount;
            }
        }

        /// <summary>
        /// Draw primitives of the specified type from the data in the given array of vertices without indexing.
        /// </summary>
        /// <typeparam name="T">The type of the vertices.</typeparam>
        /// <param name="primitiveType">The type of primitives to draw with the vertices.</param>
        /// <param name="vertexData">An array of vertices to draw.</param>
        /// <param name="vertexOffset">The index in the array of the first vertex that should be rendered.</param>
        /// <param name="primitiveCount">The number of primitives to draw.</param>
        /// <param name="vertexDeclaration">The layout of the vertices.</param>
        public void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            if (vertexData == null)
                throw new ArgumentNullException("vertexData");

            if (vertexData.Length == 0)
                throw new ArgumentOutOfRangeException("vertexData");

            if (vertexOffset < 0 || vertexOffset >= vertexData.Length)
                throw new ArgumentOutOfRangeException("vertexOffset");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            var vertexCount = GetElementCountArray(primitiveType, primitiveCount);

            if (vertexOffset + vertexCount > vertexData.Length)
                throw new ArgumentOutOfRangeException("primitiveCount");

            if (vertexDeclaration == null)
                throw new ArgumentNullException("vertexDeclaration");

            PlatformDrawUserPrimitives<T>(primitiveType, vertexData, vertexOffset, vertexDeclaration, vertexCount);

            unchecked
            {
                _graphicsMetrics._drawCount++;
                _graphicsMetrics._primitiveCount += primitiveCount;
            }
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

            unchecked
            {
                _graphicsMetrics._drawCount++;
                _graphicsMetrics._primitiveCount += primitiveCount;
            }
        }

        /// <summary>
        /// Draw primitives of the specified type by indexing into the given array of vertices with 32-bit indices.
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
        /// For example value of zero in the array of indices points to the vertex at index <paramref name="vertexOffset"/>
        /// in the array of vertices.</remarks>
        public void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            // These parameter checks are a duplicate of the checks in the short[] overload of DrawUserIndexedPrimitives.
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

            unchecked
            {
                _graphicsMetrics._drawCount++;
                _graphicsMetrics._primitiveCount += primitiveCount;
            }
        }

        /// <summary>
        /// Draw instanced geometry from the bound vertex buffers and index buffer.
        /// </summary>
        /// <param name="primitiveType">The type of primitives in the index buffer.</param>
        /// <param name="baseVertex">Used to offset the vertex range indexed from the vertex buffer.</param>
        /// <param name="startIndex">The index within the index buffer to start drawing from.</param>
        /// <param name="primitiveCount">The number of primitives in a single instance.</param>
        /// <param name="instanceCount">The number of instances to render.</param>
        /// <remarks>Draw geometry with data from multiple bound vertex streams at different frequencies.</remarks>
        public void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int instanceCount)
        {
            if (_vertexShader == null)
                throw new InvalidOperationException("Vertex shader must be set before calling DrawInstancedPrimitives.");

            if (_vertexBuffers.Count == 0)
                throw new InvalidOperationException("Vertex buffer must be set before calling DrawInstancedPrimitives.");

            if (_indexBuffer == null)
                throw new InvalidOperationException("Index buffer must be set before calling DrawInstancedPrimitives.");

            if (primitiveCount <= 0)
                throw new ArgumentOutOfRangeException("primitiveCount");

            PlatformDrawInstancedPrimitives(primitiveType, baseVertex, startIndex, primitiveCount, instanceCount);

            unchecked
            {
                _graphicsMetrics._drawCount++;
                _graphicsMetrics._primitiveCount += (primitiveCount * instanceCount);
            }
        }

        /// <summary>
        /// Gets the Pixel data of what is currently drawn on screen.
        /// The format is whatever the current format of the backbuffer is.
        /// </summary>
        /// <typeparam name="T">A byte[] of size (ViewPort.Width * ViewPort.Height * 4)</typeparam>
        public void GetBackBufferData<T>(T[] data) where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");
            GetBackBufferData(null, data, 0, data.Length);
        }

        public void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            if (data == null)
                throw new ArgumentNullException("data");

            int width, height;
            if (rect.HasValue)
            {
                var rectangle = rect.Value;
                width = rectangle.Width;
                height = rectangle.Height;

                if (rectangle.X < 0 || rectangle.Y < 0 || rectangle.Width <= 0 || rectangle.Height <= 0 ||
                    rectangle.Right > PresentationParameters.BackBufferWidth || rectangle.Top > PresentationParameters.BackBufferHeight)
                    throw new ArgumentException("Rectangle must fit in BackBuffer dimensions");
            }
            else
            {
                width = PresentationParameters.BackBufferWidth;
                height = PresentationParameters.BackBufferHeight;
            }

            var tSize = ReflectionHelpers.SizeOf<T>.Get();
            var fSize = PresentationParameters.BackBufferFormat.GetSize();
            if (tSize > fSize || fSize % tSize != 0)
                throw new ArgumentException("Type T is of an invalid size for the format of this texture.", "T");
            if (startIndex < 0 || startIndex >= data.Length)
                throw new ArgumentException("startIndex must be at least zero and smaller than data.Length.", "startIndex");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data array is too small.");
            var dataByteSize = width * height * fSize;

            if (elementCount * tSize != dataByteSize)
                throw new ArgumentException(string.Format("elementCount is not the right size, " +
                                            "elementCount * sizeof(T) is {0}, but data size is {1} bytes.",
                                            elementCount * tSize, dataByteSize), "elementCount");

            PlatformGetBackBufferData(rect, data, startIndex, elementCount);
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

        internal static Rectangle GetTitleSafeArea(int x, int y, int width, int height)
        {
            return PlatformGetTitleSafeArea(x, y, width, height);
        }
    }
}
