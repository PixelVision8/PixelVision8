// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Utilities;

// using Microsoft.Xna.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
	public class Effect : GraphicsResource
    {
        struct MGFXHeader 
        {
            /// <summary>
            /// The MonoGame Effect file format header identifier ("MGFX"). 
            /// </summary>
            public static readonly int MGFXSignature = (BitConverter.IsLittleEndian) ? 0x5846474D: 0x4D474658;

            /// <summary>
            /// The current MonoGame Effect file format versions
            /// used to detect old packaged content.
            /// </summary>
            /// <remarks>
            /// We should avoid supporting old versions for very long if at all 
            /// as users should be rebuilding content when packaging their game.
            /// </remarks>
            public const int MGFXVersion = 9;

            public int Signature;
            public int Version;
            public int Profile;
            public int EffectKey;
            public int HeaderSize;
        }

        public EffectParameterCollection Parameters { get; private set; }

        public EffectTechniqueCollection Techniques { get; private set; }

        public EffectTechnique CurrentTechnique { get; set; }
  
        internal ConstantBuffer[] ConstantBuffers { get; private set; }

        private Shader[] _shaders;

	    private readonly bool _isClone;

        internal Effect(GraphicsDevice graphicsDevice)
		{
            // if (graphicsDevice == null)
            // {
            //     throw new ArgumentNullException("graphicsDevice", FrameworkResources.ResourceCreationWhenDeviceIsNull);
            // }
            this.GraphicsDevice = graphicsDevice;
		}
			
		protected Effect(Effect cloneSource)
            : this(cloneSource.GraphicsDevice)
		{
            _isClone = true;
            Clone(cloneSource);
		}

        public Effect(GraphicsDevice graphicsDevice, byte[] effectCode)
            : this(graphicsDevice, effectCode, 0, effectCode.Length)
        {
        }


        public Effect (GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count)
            : this(graphicsDevice)
		{
			// By default we currently cache all unique byte streams
			// and use cloning to populate the effect with parameters,
			// techniques, and passes.
			//
			// This means all the immutable types in an effect:
			//
			//  - Shaders
			//  - Annotations
			//  - Names
			//  - State Objects
			//
			// Are shared for every instance of an effect while the 
			// parameter values and constant buffers are copied.
			//
			// This might need to change slightly if/when we support
			// shared constant buffers as 'new' should return unique
			// effects without any shared instance state.
 
            //Read the header
            MGFXHeader header = ReadHeader(effectCode, index);
			var effectKey = header.EffectKey;
			int headerSize = header.HeaderSize;

            // First look for it in the cache.
            //
            Effect cloneSource;
            if (!graphicsDevice.EffectCache.TryGetValue(effectKey, out cloneSource))
            {
                using (var stream = new MemoryStream(effectCode, index + headerSize, count - headerSize, false))
            	using (var reader = new BinaryReaderEx(stream))
            {
                // Create one.
                cloneSource = new Effect(graphicsDevice);
                    cloneSource.ReadEffect(reader);

                // Cache the effect for later in its original unmodified state.
                    graphicsDevice.EffectCache.Add(effectKey, cloneSource);
                }
            }

            // Clone it.
            _isClone = true;
            Clone(cloneSource);
        }

        private MGFXHeader ReadHeader(byte[] effectCode, int index)
        {
            MGFXHeader header;
            header.Signature = BitConverter.ToInt32(effectCode, index); index += 4;
            header.Version = (int)effectCode[index++];
            header.Profile = (int)effectCode[index++];
            header.EffectKey = BitConverter.ToInt32(effectCode, index); index += 4;
            header.HeaderSize = index;

            if (header.Signature != MGFXHeader.MGFXSignature)
                throw new Exception("This does not appear to be a MonoGame MGFX file!");
            if (header.Version < MGFXHeader.MGFXVersion)
                throw new Exception("This MGFX effect is for an older release of MonoGame and needs to be rebuilt.");
            if (header.Version > MGFXHeader.MGFXVersion)
                throw new Exception("This MGFX effect seems to be for a newer release of MonoGame.");

            if (header.Profile != Shader.Profile)
                throw new Exception("This MGFX effect was built for a different platform!");          
            
            return header;
        }

        /// <summary>
        /// Clone the source into this existing object.
        /// </summary>
        /// <remarks>
        /// Note this is not overloaded in derived classes on purpose.  This is
        /// only a reason this exists is for caching effects.
        /// </remarks>
        /// <param name="cloneSource">The source effect to clone from.</param>
        private void Clone(Effect cloneSource)
        {
            Debug.Assert(_isClone, "Cannot clone into non-cloned effect!");

            // Copy the mutable members of the effect.
            Parameters = cloneSource.Parameters.Clone();
            Techniques = cloneSource.Techniques.Clone(this);

            // Make a copy of the immutable constant buffers.
            ConstantBuffers = new ConstantBuffer[cloneSource.ConstantBuffers.Length];
            for (var i = 0; i < cloneSource.ConstantBuffers.Length; i++)
                ConstantBuffers[i] = new ConstantBuffer(cloneSource.ConstantBuffers[i]);

            // Find and set the current technique.
            for (var i = 0; i < cloneSource.Techniques.Count; i++)
            {
                if (cloneSource.Techniques[i] == cloneSource.CurrentTechnique)
                {
                    CurrentTechnique = Techniques[i];
                    break;
                }
            }

            // Take a reference to the original shader list.
            _shaders = cloneSource._shaders;
        }

        /// <summary>
        /// Returns a deep copy of the effect where immutable types 
        /// are shared and mutable data is duplicated.
        /// </summary>
        /// <remarks>
        /// See "Cloning an Effect" in MSDN:
        /// http://msdn.microsoft.com/en-us/library/windows/desktop/ff476138(v=vs.85).aspx
        /// </remarks>
        /// <returns>The cloned effect.</returns>
		public virtual Effect Clone()
		{
            return new Effect(this);
		}

        protected internal virtual void OnApply()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (!_isClone)
                    {
                        // Only the clone source can dispose the shaders.
                        if (_shaders != null)
                        {
                            foreach (var shader in _shaders)
                                shader.Dispose();
                        }
                    }

                    if (ConstantBuffers != null)
                    {
                        foreach (var buffer in ConstantBuffers)
                            buffer.Dispose();
                        ConstantBuffers = null;
                    }
                }
            }

            base.Dispose(disposing);
        }

        #region Effect File Reader

		private void ReadEffect (BinaryReaderEx reader)
		{
			// TODO: Maybe we should be reading in a string 
			// table here to save some bytes in the file.

			// Read in all the constant buffers.
			var buffers = (int)reader.ReadByte ();
			ConstantBuffers = new ConstantBuffer[buffers];
			for (var c = 0; c < buffers; c++) 
            {				
				var name = reader.ReadString ();               

				// Create the backing system memory buffer.
				var sizeInBytes = (int)reader.ReadInt16 ();

				// Read the parameter index values.
				var parameters = new int[reader.ReadByte ()];
				var offsets = new int[parameters.Length];
				for (var i = 0; i < parameters.Length; i++) 
                {
					parameters [i] = (int)reader.ReadByte ();
					offsets [i] = (int)reader.ReadUInt16 ();
				}

                var buffer = new ConstantBuffer(GraphicsDevice,
				                                sizeInBytes,
				                                parameters,
				                                offsets,
				                                name);
                ConstantBuffers[c] = buffer;
            }

            // Read in all the shader objects.
            var shaders = (int)reader.ReadByte();
            _shaders = new Shader[shaders];
            for (var s = 0; s < shaders; s++)
                _shaders[s] = new Shader(GraphicsDevice, reader);

            // Read in the parameters.
            Parameters = ReadParameters(reader);

            // Read the techniques.
            var techniqueCount = (int)reader.ReadByte();
            var techniques = new EffectTechnique[techniqueCount];
            for (var t = 0; t < techniqueCount; t++)
            {
                var name = reader.ReadString();

                ReadAnnotations(reader);

                techniques[t] = new EffectTechnique(name, ReadPasses(reader, this, _shaders));
            }

            Techniques = new EffectTechniqueCollection(techniques);
            CurrentTechnique = Techniques[0];
        }

        private static void ReadAnnotations(BinaryReader reader)
        {
            var count = (int)reader.ReadByte();
            // if (count == 0)
            //     return EffectAnnotationCollection.Empty;
            //
            // var annotations = new EffectAnnotation[count];
            //
            // // TODO: Annotations are not implemented!
            //
            // return new EffectAnnotationCollection(annotations);
        }

        private static EffectPassCollection ReadPasses(BinaryReader reader, Effect effect, Shader[] shaders)
        {
            var count = (int)reader.ReadByte();
            var passes = new EffectPass[count];

            for (var i = 0; i < count; i++)
            {
                var name = reader.ReadString();
                ReadAnnotations(reader);

                // Get the vertex shader.
                Shader vertexShader = null;
                var shaderIndex = (int)reader.ReadByte();
                if (shaderIndex != 255)
                    vertexShader = shaders[shaderIndex];

                // Get the pixel shader.
                Shader pixelShader = null;
                shaderIndex = (int)reader.ReadByte();
                if (shaderIndex != 255)
                    pixelShader = shaders[shaderIndex];


                passes[i] = new EffectPass(effect, name, vertexShader, pixelShader/*, blend, depth,*/ /*raster,*//* annotations*/);
			}

            return new EffectPassCollection(passes);
		}

		private static EffectParameterCollection ReadParameters(BinaryReaderEx reader)
		{
			var count = reader.Read7BitEncodedInt();
            if (count == 0)
                return EffectParameterCollection.Empty;

            var parameters = new EffectParameter[count];
			for (var i = 0; i < count; i++)
			{
				var class_ = /*(EffectParameterClass)*/reader.ReadByte();				
                var type = (EffectParameterType)reader.ReadByte();
				var name = reader.ReadString();
				var semantic = reader.ReadString();
				ReadAnnotations(reader);
				var rowCount = (int)reader.ReadByte();
				var columnCount = (int)reader.ReadByte();

				var elements = ReadParameters(reader);
				var structMembers = ReadParameters(reader);

				object data = null;
				if (elements.Count == 0 && structMembers.Count == 0)
				{
					
					var buffer = new float[rowCount * columnCount];
					for (var j = 0; j < buffer.Length; j++)
                        buffer[j] = reader.ReadSingle();
                    data = buffer;
                            
                }

				parameters[i] = new EffectParameter(
					/*class_,*/ type, name, rowCount, columnCount, semantic, 
					/*annotations,*/ elements, /*structMembers,*/ data);
			}

			return new EffectParameterCollection(parameters);
		}
        #endregion // Effect File Reader
	}
}
