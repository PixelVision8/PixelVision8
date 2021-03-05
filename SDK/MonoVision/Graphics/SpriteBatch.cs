// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Helper class for drawing text strings and sprites in one or more optimized batches.
    /// </summary>
	public class SpriteBatch : GraphicsResource
	{
        #region Private Fields
        readonly SpriteBatcher _batcher;

		// SpriteSortMode _sortMode;
		// BlendState _blendState;
		SamplerState _samplerState;
		// DepthStencilState _depthStencilState;
		// RasterizerState _rasterizerState;
		Effect _effect;
        bool _beginCalled;

		SpriteEffect _spriteEffect;
        readonly EffectPass _spritePass;

		Rectangle _tempRect = new Rectangle (0,0,0,0);
		Vector2 _texCoordTL = new Vector2 (0,0);
		Vector2 _texCoordBR = new Vector2 (0,0);
        #endregion

        /// <summary>
        /// Constructs a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
        public SpriteBatch(GraphicsDevice graphicsDevice) : this(graphicsDevice, 0)
        {
        }

        /// <summary>
        /// Constructs a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>
        /// <param name="capacity">The initial capacity of the internal array holding batch items (the value will be rounded to the next multiple of 64).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
        public SpriteBatch (GraphicsDevice graphicsDevice, int capacity)
		{
			// if (graphicsDevice == null)
   //          {
			// 	throw new ArgumentNullException ("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
			// }

			this.GraphicsDevice = graphicsDevice;

            _spriteEffect = new SpriteEffect(graphicsDevice);
            _spritePass = _spriteEffect.CurrentTechnique.Passes[0];

            _batcher = new SpriteBatcher(graphicsDevice, capacity);

            _beginCalled = false;
		}

        /// <summary>
        /// Begins a new sprite and text batch with the specified render state.
        /// </summary>
        /// <param name="sortMode">The drawing order for sprite and text drawing. <see cref="SpriteSortMode.Deferred"/> by default.</param>
        /// <param name="blendState">State of the blending. Uses <see cref="BlendState.AlphaBlend"/> if null.</param>
        /// <param name="samplerState">State of the sampler. Uses <see cref="SamplerState.LinearClamp"/> if null.</param>
        /// <param name="depthStencilState">State of the depth-stencil buffer. Uses <see cref="DepthStencilState.None"/> if null.</param>
        /// <param name="rasterizerState">State of the rasterization. Uses <see cref="RasterizerState.CullCounterClockwise"/> if null.</param>
        /// <param name="effect">A custom <see cref="Effect"/> to override the default sprite effect. Uses default sprite effect if null.</param>
        /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix.Identity"/> if null.</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> is called next time without previous <see cref="End"/>.</exception>
        /// <remarks>This method uses optional parameters.</remarks>
        /// <remarks>The <see cref="Begin"/> Begin should be called before drawing commands, and you cannot call it again before subsequent <see cref="End"/>.</remarks>
        public void Begin
        (
             // SpriteSortMode sortMode = SpriteSortMode.Deferred,
             // BlendState blendState = null,
             // SamplerState samplerState = null,
             /*DepthStencilState depthStencilState = null,*/
             // RasterizerState rasterizerState = null,
             // Effect effect = null,
             // Matrix? transformMatrix = null
        )
        {
            if (_beginCalled)
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");

            // defaults
            // _sortMode = sortMode;
            // _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = /*samplerState ?? */SamplerState.PointClamp;
            // _depthStencilState = depthStencilState ?? DepthStencilState.None;
            // _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            // _effect = effect;
            // _spriteEffect.TransformMatrix = transformMatrix;

            // Setup things now so a user can change them.
            // if (sortMode == SpriteSortMode.Immediate)
            // {
                Setup();
            // }

            _beginCalled = true;
        }

        /// <summary>
        /// Flushes all batched text and sprites to the screen.
        /// </summary>
        /// <remarks>This command should be called after <see cref="Begin"/> and drawing commands.</remarks>
		public void End ()
		{
            if (!_beginCalled)
                throw new InvalidOperationException("Begin must be called before calling End.");

			_beginCalled = false;

			// if (_sortMode != SpriteSortMode.Immediate)
			// 	Setup();

            _batcher.DrawBatch(/*_sortMode,*/ _effect);
        }

		void Setup()
        {
            var gd = GraphicsDevice;
			// gd.BlendState = _blendState;
			// gd.DepthStencilState = _depthStencilState;
			// gd.RasterizerState = _rasterizerState;
			gd.SamplerStates[0] = _samplerState;

            _spritePass.Apply();
		}

        void CheckValid(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            if (!_beginCalled)
                throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
		public void Draw (Texture2D texture,
				Vector2 position,
				Rectangle? sourceRectangle,
				Color color,
				// float rotation,
				Vector2 origin,
				Vector2 scale//,
				// SpriteEffects effects,
                /*float layerDepth*/)
		{
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

                var srcRect = sourceRectangle.GetValueOrDefault();
                var w = srcRect.Width * scale.X;
                var h = srcRect.Height * scale.Y;
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;
            
                item.Set(position.X - origin.X,
                        position.Y - origin.Y,
                        w,
                        h,
                        color,
                        _texCoordTL,
                        _texCoordBR);
            
            FlushIfNeeded();
		}

		// Mark the end of a draw operation for Immediate SpriteSortMode.
		internal void FlushIfNeeded()
		{
			// if (_sortMode == SpriteSortMode.Immediate)
			// {
				_batcher.DrawBatch(/*_sortMode, */_effect);
			// }
		}

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_spriteEffect != null)
                    {
                        _spriteEffect.Dispose();
                        _spriteEffect = null;
                    }
                }
            }
            base.Dispose(disposing);
        }
	}
}

