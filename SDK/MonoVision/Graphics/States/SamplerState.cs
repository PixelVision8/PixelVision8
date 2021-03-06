// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class SamplerState : GraphicsResource
    {
        static SamplerState()
        {
            PointClamp = new SamplerState("SamplerState.PointClamp");
        }

        public static readonly SamplerState PointClamp;
        // private readonly bool _defaultStateObject;
        private Color _borderColor;
        // private int _maxAnisotropy;
        // private int _maxMipLevel;
        // private float _mipMapLevelOfDetailBias;
        // private CompareFunction _comparisonFunction;

        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                // ThrowIfBound();
                _borderColor = value;
            }
        }

        // public int MaxAnisotropy
        // {
        //     get { return _maxAnisotropy; }
        //     set
        //     {
        //         ThrowIfBound();
        //         _maxAnisotropy = value;
        //     }
        // }

        // public int MaxMipLevel
        // {
        //     get { return _maxMipLevel; }
        //     set
        //     {
        //         ThrowIfBound();
        //         _maxMipLevel = value;
        //     }
        // }

        // public float MipMapLevelOfDetailBias
        // {
        //     get { return _mipMapLevelOfDetailBias; }
        //     set
        //     {
        //         ThrowIfBound();
        //         _mipMapLevelOfDetailBias = value;
        //     }
        // }

        /// <summary>
        /// When using comparison sampling, also set <see cref="FilterMode"/> to <see cref="TextureFilterMode.Comparison"/>.
        /// </summary>
        // public CompareFunction ComparisonFunction
        // {
        //     get { return _comparisonFunction; }
        //     set
        //     {
        //         ThrowIfBound();
        //         _comparisonFunction = value;
        //     }
        // }

        // internal void BindToGraphicsDevice(GraphicsDevice device)
        // {
        //     if (_defaultStateObject)
        //         throw new InvalidOperationException("You cannot bind a default state object.");
        //     if (GraphicsDevice != null && GraphicsDevice != device)
        //         throw new InvalidOperationException("This sampler state is already bound to a different graphics device.");
        //     GraphicsDevice = device;
        // }

        // internal void ThrowIfBound()
        // {
        //     if (_defaultStateObject)
        //         throw new InvalidOperationException("You cannot modify a default sampler state object.");
        //     if (GraphicsDevice != null)
        //         throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");
        // }

        public SamplerState()
        {
            BorderColor = Color.White;
            // MaxAnisotropy = 4;
            // MaxMipLevel = 0;
            // MipMapLevelOfDetailBias = 0.0f;
            // ComparisonFunction = CompareFunction.Never;
        }

        private SamplerState(string name/*, TextureFilter filter, TextureAddressMode addressMode*/)
            : this()
        {
            Name = name;
            
            // _defaultStateObject = true;
        }

        private SamplerState(SamplerState cloneSource)
        {
            Name = cloneSource.Name;
            
            _borderColor = cloneSource._borderColor;
            // _maxAnisotropy = cloneSource._maxAnisotropy;
            // _maxMipLevel = cloneSource._maxMipLevel;
            // _mipMapLevelOfDetailBias = cloneSource._mipMapLevelOfDetailBias;
            // _comparisonFunction = cloneSource._comparisonFunction;
        }

        internal SamplerState Clone()
        {
            return new SamplerState(this);
        }

        partial void PlatformDispose();

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                PlatformDispose();
            }
            base.Dispose(disposing);
        }
    }
}
