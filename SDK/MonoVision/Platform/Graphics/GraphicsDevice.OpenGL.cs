// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

#if ANGLE
using OpenTK.Graphics;
#else
using MonoGame.OpenGL;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
#if DESKTOPGL || ANGLE
        internal IGraphicsContext Context { get; private set; }
#endif

#if !GLES
        private DrawBuffersEnum[] _drawBuffers;
#endif

        enum ResourceType
        {
            Texture,
            Buffer,
            Shader,
            Program,
            Query,
            Framebuffer
        }

        struct ResourceHandle
        {
            public ResourceType type;
            public int handle;

            public static ResourceHandle Texture(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Texture, handle = handle };
            }

            public static ResourceHandle Buffer(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Buffer, handle = handle };
            }

            public static ResourceHandle Shader(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Shader, handle = handle };
            }

            public static ResourceHandle Program(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Program, handle = handle };
            }

            public static ResourceHandle Query(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Query, handle = handle };
            }

            public static ResourceHandle Framebuffer(int handle)
            {
                return new ResourceHandle() { type = ResourceType.Framebuffer, handle = handle };
            }

            public void Free()
            {
                switch (type)
                {
                    case ResourceType.Texture:
                        GL.DeleteTextures(1, ref handle);
                        break;
                    case ResourceType.Buffer:
                        GL.DeleteBuffers(1, ref handle);
                        break;
                    case ResourceType.Shader:
                        if (GL.IsShader(handle))
                            GL.DeleteShader(handle);
                        break;
                    case ResourceType.Program:
                        if (GL.IsProgram(handle))
                        {
                            GL.DeleteProgram(handle);
                        }
                        break;
                    case ResourceType.Query:
#if !GLES
                        GL.DeleteQueries(1, ref handle);
#endif
                        break;
                    case ResourceType.Framebuffer:
                        GL.DeleteFramebuffers(1, ref handle);
                        break;
                }
                GraphicsExtensions.CheckGLError();
            }
        }

        List<ResourceHandle> _disposeThisFrame = new List<ResourceHandle>();
        List<ResourceHandle> _disposeNextFrame = new List<ResourceHandle>();
        object _disposeActionsLock = new object();

        static List<IntPtr> _disposeContexts = new List<IntPtr>();
        static object _disposeContextsLock = new object();

        private ShaderProgramCache _programCache;
        private ShaderProgram _shaderProgram = null;

        static readonly float[] _posFixup = new float[4];

        private static BufferBindingInfo[] _bufferBindingInfos;
        private static bool[] _newEnabledVertexAttributes;
        internal static readonly List<int> _enabledVertexAttributes = new List<int>();
        internal static bool _attribsDirty;


        internal int glMajorVersion = 0;
        internal int glMinorVersion = 0;
        internal int glFramebuffer = 0;
        internal int MaxVertexAttributes;
        internal int _maxTextureSize = 0;

        // Keeps track of last applied state to avoid redundant OpenGL calls
        // internal bool _lastBlendEnable = false;
        // internal BlendState _lastBlendState = new BlendState();
        // internal DepthStencilState _lastDepthStencilState = new DepthStencilState();
        // internal RasterizerState _lastRasterizerState = new RasterizerState();
        private Color _lastClearColor = Color.Magenta;
        private float _lastClearDepth = 1.0f;
        private int _lastClearStencil = 0;

        // Get a hashed value based on the currently bound shaders
        // throws an exception if no shaders are bound
        private int ShaderProgramHash
        {
            get
            {
                if (_vertexShader == null && _pixelShader == null)
                    throw new InvalidOperationException("There is no shader bound!");
                if (_vertexShader == null)
                    return _pixelShader.HashKey;
                if (_pixelShader == null)
                    return _vertexShader.HashKey;
                return _vertexShader.HashKey ^ _pixelShader.HashKey;
            }
        }

        internal void SetVertexAttributeArray(bool[] attrs)
        {
            for (var x = 0; x < attrs.Length; x++)
            {
                if (attrs[x] && !_enabledVertexAttributes.Contains(x))
                {
                    _enabledVertexAttributes.Add(x);
                    GL.EnableVertexAttribArray(x);
                    GraphicsExtensions.CheckGLError();
                }
                else if (!attrs[x] && _enabledVertexAttributes.Contains(x))
                {
                    _enabledVertexAttributes.Remove(x);
                    GL.DisableVertexAttribArray(x);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        private void ApplyAttribs(Shader shader, int baseVertex)
        {
            var programHash = ShaderProgramHash;
            var bindingsChanged = false;

            for (var slot = 0; slot < _vertexBuffers.Count; slot++)
            {
                var vertexBufferBinding = _vertexBuffers.Get(slot);
                var vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
                var attrInfo = vertexDeclaration.GetAttributeInfo(shader, programHash);

                var vertexStride = vertexDeclaration.VertexStride;
                var offset = (IntPtr)(vertexDeclaration.VertexStride * (baseVertex + vertexBufferBinding.VertexOffset));

                if (!_attribsDirty &&
                    _bufferBindingInfos[slot].VertexOffset == offset &&
                    ReferenceEquals(_bufferBindingInfos[slot].AttributeInfo, attrInfo) &&
                    _bufferBindingInfos[slot].InstanceFrequency == vertexBufferBinding.InstanceFrequency &&
                    _bufferBindingInfos[slot].Vbo == vertexBufferBinding.VertexBuffer.vbo)
                    continue;

                bindingsChanged = true;

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferBinding.VertexBuffer.vbo);
                GraphicsExtensions.CheckGLError();

                // If instancing is not supported, but InstanceFrequency of the buffer is not zero, throw an exception
                if (!GraphicsCapabilities.SupportsInstancing && vertexBufferBinding.InstanceFrequency > 0)
                    throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics drivers.");

                foreach (var element in attrInfo.Elements)
                {
                    GL.VertexAttribPointer(element.AttributeLocation,
                        element.NumberOfElements,
                        element.VertexAttribPointerType,
                        element.Normalized,
                        vertexStride,
                        (IntPtr)(offset.ToInt64() + element.Offset));

                    // only set the divisor if instancing is supported
                    if (GraphicsCapabilities.SupportsInstancing)
                        GL.VertexAttribDivisor(element.AttributeLocation, vertexBufferBinding.InstanceFrequency);

                    GraphicsExtensions.CheckGLError();
                }

                _bufferBindingInfos[slot].VertexOffset = offset;
                _bufferBindingInfos[slot].AttributeInfo = attrInfo;
                _bufferBindingInfos[slot].InstanceFrequency = vertexBufferBinding.InstanceFrequency;
                _bufferBindingInfos[slot].Vbo = vertexBufferBinding.VertexBuffer.vbo;
            }

            _attribsDirty = false;

            if (bindingsChanged)
            {
                Array.Clear(_newEnabledVertexAttributes, 0, _newEnabledVertexAttributes.Length);
                for (var slot = 0; slot < _vertexBuffers.Count; slot++)
                {
                    foreach (var element in _bufferBindingInfos[slot].AttributeInfo.Elements)
                        _newEnabledVertexAttributes[element.AttributeLocation] = true;
                }
            }
            SetVertexAttributeArray(_newEnabledVertexAttributes);
        }

        private void PlatformSetup()
        {
            _programCache = new ShaderProgramCache(this);
#if DESKTOPGL || ANGLE
            var windowInfo = new WindowInfo(SdlGameWindow.Instance.Handle);

            if (Context == null || Context.IsDisposed)
            {
                Context = GL.CreateContext(windowInfo);
            }

            Context.MakeCurrent(windowInfo);
            // Context.SwapInterval = -1;//PresentationParameters.PresentationInterval.GetSwapInterval();

            Context.MakeCurrent(windowInfo);
#endif
            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out MaxTextureSlots);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(GetPName.MaxTextureSize, out _maxTextureSize);
            GraphicsExtensions.CheckGLError();

            GL.GetInteger(GetPName.MaxVertexAttribs, out MaxVertexAttributes);
            GraphicsExtensions.CheckGLError();

            _maxVertexBufferSlots = MaxVertexAttributes;
            _newEnabledVertexAttributes = new bool[MaxVertexAttributes];


            // try getting the context version
            // GL_MAJOR_VERSION and GL_MINOR_VERSION are GL 3.0+ only, so we need to rely on the GL_VERSION string
            // for non GLES this string always starts with the version number in the "major.minor" format, but can be followed by
            // multiple vendor specific characters
            // For GLES this string is formatted as: OpenGL<space>ES<space><version number><space><vendor-specific information>
#if GLES
            try
            {
                string version = GL.GetString(StringName.Version);

                if (string.IsNullOrEmpty(version))
                    throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version");

                string[] versionSplit = version.Split(' ');
                if (versionSplit.Length > 2 && versionSplit[0].Equals("OpenGL") && versionSplit[1].Equals("ES"))
                {
                    glMajorVersion = Convert.ToInt32(versionSplit[2].Substring(0, 1));
                    glMinorVersion = Convert.ToInt32(versionSplit[2].Substring(2, 1));
                }
                else
                {
                    glMajorVersion = 1;
                    glMinorVersion = 1;
                }
            }
            catch (FormatException)
            {
                //if it fails we default to 1.1 context
                glMajorVersion = 1;
                glMinorVersion = 1;
            }
#else
                try
            {
                string version = GL.GetString(StringName.Version);

                if (string.IsNullOrEmpty(version))
                    throw new NoSuitableGraphicsDeviceException("Unable to retrieve OpenGL version");

                glMajorVersion = Convert.ToInt32(version.Substring(0, 1));
                glMinorVersion = Convert.ToInt32(version.Substring(2, 1));
            }
            catch (FormatException)
            {
                // if it fails, we assume to be on a 1.1 context
                glMajorVersion = 1;
                glMinorVersion = 1;
            }
#endif

#if !GLES
			// Initialize draw buffer attachment array
			int maxDrawBuffers;
            GL.GetInteger(GetPName.MaxDrawBuffers, out maxDrawBuffers);
            GraphicsExtensions.CheckGLError ();
			_drawBuffers = new DrawBuffersEnum[maxDrawBuffers];
			for (int i = 0; i < maxDrawBuffers; i++)
				_drawBuffers[i] = (DrawBuffersEnum)(FramebufferAttachment.ColorAttachment0Ext + i);
#endif
        }

        private void PlatformInitialize()
        {
            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            _enabledVertexAttributes.Clear();

            // Free all the cached shader programs.
            _programCache.Clear();
            _shaderProgram = null;

            _bufferBindingInfos = new BufferBindingInfo[_maxVertexBufferSlots];
            for (int i = 0; i < _bufferBindingInfos.Length; i++)
                _bufferBindingInfos[i] = new BufferBindingInfo(null, IntPtr.Zero, 0, -1);
        }

        // private DepthStencilState clearDepthStencilState = new DepthStencilState { StencilEnable = true };

        private void PlatformClear(/*ClearOptions options,*/ Color color, float depth, int stencil)
        {
            
		    var prevScissorRect = ScissorRectangle;
            ScissorRectangle = _viewport.Bounds;

            ApplyState(false);

            ClearBufferMask bufferMask = 0;
            
                if (color != _lastClearColor)
                {
                    
                    // (R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f)
                        
                    GL.ClearColor(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
                    GraphicsExtensions.CheckGLError();
                    _lastClearColor = color;
                }
                bufferMask = bufferMask | ClearBufferMask.ColorBufferBit;
            
                GL.Clear(bufferMask);
                GraphicsExtensions.CheckGLError();

            // Restore the previous render state.
		    ScissorRectangle = prevScissorRect;
		    // DepthStencilState = prevDepthStencilState;
		    // BlendState = prevBlendState;
        }

        private void PlatformDispose()
        {
            // Free all the cached shader programs.
            _programCache.Dispose();

#if DESKTOPGL || ANGLE
            Context.Dispose();
            Context = null;
#endif
        }

        internal void DisposeTexture(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Texture(handle));
                }
            }
        }

        internal void DisposeBuffer(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Buffer(handle));
                }
            }
        }

        internal void DisposeShader(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Shader(handle));
                }
            }
        }

        internal void DisposeProgram(int handle)
        {
            if (!_isDisposed)
            {
                lock (_disposeActionsLock)
                {
                    _disposeNextFrame.Add(ResourceHandle.Program(handle));
                }
            }
        }

#if DESKTOPGL || ANGLE
        static internal void DisposeContext(IntPtr resource)
        {
            lock (_disposeContextsLock)
            {
                _disposeContexts.Add(resource);
            }
        }

        static internal void DisposeContexts()
        {
            lock (_disposeContextsLock)
            {
                int count = _disposeContexts.Count;
                for (int i = 0; i < count; ++i)
                    Sdl.GL.DeleteContext(_disposeContexts[i]);
                _disposeContexts.Clear();
            }
        }
#endif

        private void PlatformPresent()
        {
#if DESKTOPGL || ANGLE
            Context.SwapBuffers();
#endif
            GraphicsExtensions.CheckGLError();

            // Dispose of any GL resources that were disposed in another thread
            int count = _disposeThisFrame.Count;
            for (int i = 0; i < count; ++i)
                _disposeThisFrame[i].Free();
            _disposeThisFrame.Clear();

            lock (_disposeActionsLock)
            {
                // Swap lists so resources added during this draw will be released after the next draw
                var temp = _disposeThisFrame;
                _disposeThisFrame = _disposeNextFrame;
                _disposeNextFrame = temp;
            }
        }

        private void PlatformSetViewport(ref Viewport value)
        {
            // if (IsRenderTargetBound)
                GL.Viewport(value.X, value.Y, value.Width, value.Height);
            // else
            //     GL.Viewport(value.X, PresentationParameters.BackBufferHeight - value.Y - value.Height, value.Width, value.Height);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

            GL.DepthRange(value.MinDepth, value.MaxDepth);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");

            // In OpenGL we have to re-apply the special "posFixup"
            // vertex shader uniform if the viewport changes.
            _vertexShaderDirty = true;

        }

        private void PlatformApplyDefaultRenderTarget()
        {
            // this.framebufferHelper.BindFramebuffer(this.glFramebuffer);

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            Textures.Dirty();
        }

        private static GLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    return GLPrimitiveType.Lines;
                case PrimitiveType.LineStrip:
                    return GLPrimitiveType.LineStrip;
                case PrimitiveType.TriangleList:
                    return GLPrimitiveType.Triangles;
                case PrimitiveType.TriangleStrip:
                    return GLPrimitiveType.TriangleStrip;
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Activates the Current Vertex/Pixel shader pair into a program.
        /// </summary>
        private unsafe void ActivateShaderProgram()
        {
            // Lookup the shader program.
            var shaderProgram = _programCache.GetProgram(VertexShader, PixelShader);
            if (shaderProgram.Program == -1)
                return;
            // Set the new program if it has changed.
            if (_shaderProgram != shaderProgram)
            {
                GL.UseProgram(shaderProgram.Program);
                GraphicsExtensions.CheckGLError();
                _shaderProgram = shaderProgram;
            }

            var posFixupLoc = shaderProgram.GetUniformLocation("posFixup");
            if (posFixupLoc == -1)
                return;

            // Apply vertex shader fix:
            // The following two lines are appended to the end of vertex shaders
            // to account for rendering differences between OpenGL and DirectX:
            //
            // gl_Position.y = gl_Position.y * posFixup.y;
            // gl_Position.xy += posFixup.zw * gl_Position.ww;
            //
            // (the following paraphrased from wine, wined3d/state.c and wined3d/glsl_shader.c)
            //
            // - We need to flip along the y-axis in case of offscreen rendering.
            // - D3D coordinates refer to pixel centers while GL coordinates refer
            //   to pixel corners.
            // - D3D has a top-left filling convention. We need to maintain this
            //   even after the y-flip mentioned above.
            // In order to handle the last two points, we translate by
            // (63.0 / 128.0) / VPw and (63.0 / 128.0) / VPh. This is equivalent to
            // translating slightly less than half a pixel. We want the difference to
            // be large enough that it doesn't get lost due to rounding inside the
            // driver, but small enough to prevent it from interfering with any
            // anti-aliasing.
            //
            // OpenGL coordinates specify the center of the pixel while d3d coords specify
            // the corner. The offsets are stored in z and w in posFixup. posFixup.y contains
            // 1.0 or -1.0 to turn the rendering upside down for offscreen rendering. PosFixup.x
            // contains 1.0 to allow a mad.

            _posFixup[0] = 1.0f;
            _posFixup[1] = 1.0f;
            
            fixed (float* floatPtr = _posFixup)
            {
                GL.Uniform4(posFixupLoc, 1, floatPtr);
            }
            GraphicsExtensions.CheckGLError();
        }

        internal void PlatformBeginApplyState()
        {
            Threading.EnsureUIThread();
        }

        internal void PlatformApplyState(bool applyShaders)
        {
            if ( _scissorRectangleDirty )
	        {
                var scissorRect = _scissorRectangle;
                // if (!IsRenderTargetBound)
                    scissorRect.Y = PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);
                GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
                GraphicsExtensions.CheckGLError();
	            _scissorRectangleDirty = false;
	        }

            // If we're not applying shaders then early out now.
            if (!applyShaders)
                return;

            if (_indexBufferDirty)
            {
                
                _indexBufferDirty = false;
            }

            if (_vertexShader == null)
                throw new InvalidOperationException("A vertex shader must be set!");
            if (_pixelShader == null)
                throw new InvalidOperationException("A pixel shader must be set!");

            if (_vertexShaderDirty || _pixelShaderDirty)
            {
                ActivateShaderProgram();

                _vertexShaderDirty = _pixelShaderDirty = false;
            }

            _vertexConstantBuffers.SetConstantBuffers(this, _shaderProgram);
            _pixelConstantBuffers.SetConstantBuffers(this, _shaderProgram);

            Textures.SetTextures(this);
            SamplerStates.PlatformSetSamplers(this);
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            ApplyState(true);

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _indexBufferDirty = true;

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            var vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            vertexDeclaration.Apply(_vertexShader, vertexAddr, ShaderProgramHash);

            //Draw
            GL.DrawElements(    PrimitiveTypeGL(primitiveType),
                                GetElementCountArray(primitiveType, primitiveCount),
                                DrawElementsType.UnsignedShort,
                                (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(short))));
            GraphicsExtensions.CheckGLError();

            // Release the handles.
            ibHandle.Free();
            vbHandle.Free();
        }

        internal void OnPresentationChanged()
        {
#if DESKTOPGL || ANGLE
            Context.MakeCurrent(new WindowInfo(SdlGameWindow.Instance.Handle));
            // Context.SwapInterval = -1;//PresentationParameters.PresentationInterval.GetSwapInterval();
#endif

            ApplyRenderTargets();
        }

        // Holds information for caching
        private class BufferBindingInfo
        {
            public VertexDeclaration.VertexDeclarationAttributeInfo AttributeInfo;
            public IntPtr VertexOffset;
            public int InstanceFrequency;
            public int Vbo;

            public BufferBindingInfo(VertexDeclaration.VertexDeclarationAttributeInfo attributeInfo, IntPtr vertexOffset, int instanceFrequency, int vbo)
            {
                AttributeInfo = attributeInfo;
                VertexOffset = vertexOffset;
                InstanceFrequency = instanceFrequency;
                Vbo = vbo;
            }
        }

    }
}
