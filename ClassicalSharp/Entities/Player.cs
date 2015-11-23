﻿using System;
using System.Drawing;
using ClassicalSharp.Model;
using ClassicalSharp.Network;
using OpenTK;

namespace ClassicalSharp {

	public abstract partial class Player : AnimatedEntity {
		
		/// <summary> Gets the position of the player's eye in the world. </summary>
		public Vector3 EyePosition {
			get { return new Vector3( Position.X, Position.Y + Model.GetEyeY( this ), Position.Z ); }
		}
		
		public string DisplayName, SkinName, SkinIdentifier;
		public SkinType SkinType;
		
		public Player( Game game ) : base( game ) {
			this.game = game;
			StepSize = 0.5f;
			SkinType = game.DefaultPlayerSkinType;
			SetModel( "humanoid" );
		}
		
		/// <summary> Gets the block just underneath the player's feet position. </summary>
		public Block BlockUnderFeet {
			get { return GetBlock( new Vector3( Position.X, Position.Y - 0.01f, Position.Z ) ); }
		}
		
		/// <summary> Gets the block at player's eye position. </summary>
		public Block BlockAtHead {
			get { return GetBlock( EyePosition ); }
		}
		
		protected Block GetBlock( Vector3 coords ) {
			return (Block)game.Map.SafeGetBlock( Vector3I.Floor( coords ) );
		}
		
		public abstract void Tick( double delta );
		
		public abstract void SetLocation( LocationUpdate update, bool interpolate );
		
		protected void CheckSkin() {
			DownloadedItem item;
			game.AsyncDownloader.TryGetItem( SkinIdentifier, out item );
			if( item != null && item.Data != null ) {
				Bitmap bmp = (Bitmap)item.Data;
				game.Graphics.DeleteTexture( ref PlayerTextureId );
				if( !FastBitmap.CheckFormat( bmp.PixelFormat ) )
					game.Drawer2D.ConvertTo32Bpp( ref bmp );
				
				try {
					SkinType = Utils.GetSkinType( bmp );
					if( Model is PlayerModel )
						ClearHat( bmp, SkinType );
					PlayerTextureId = game.Graphics.CreateTexture( bmp );
					MobTextureId = -1;
					
					// Custom mob textures.
					if( Utils.IsUrlPrefix( SkinName ) && item.TimeAdded > lastModelChange )
						MobTextureId = PlayerTextureId;					
				} catch( NotSupportedException ) {
					ResetSkin( bmp );
				}
				bmp.Dispose();
			}
		}
		
		void ResetSkin( Bitmap bmp ) {
			string formatString = "Skin {0} has unsupported dimensions({1}, {2}), reverting to default.";
			Utils.LogDebug( formatString, SkinName, bmp.Width, bmp.Height );
			MobTextureId = -1;
			PlayerTextureId = -1;
			SkinType = game.DefaultPlayerSkinType;
		}
		
		DateTime lastModelChange = new DateTime( 1, 1, 1 );
		public void SetModel( string modelName ) {
			ModelName = modelName;
			Model = game.ModelCache.GetModel( ModelName );
			lastModelChange = DateTime.UtcNow;
			MobTextureId = -1;
		}
		
		unsafe static void ClearHat( Bitmap bmp, SkinType skinType ) {
			using( FastBitmap fastBmp = new FastBitmap( bmp, true ) ) {
				int sizeX = (bmp.Width / 64) * 32;
				int yScale = skinType == SkinType.Type64x32 ? 32 : 64;
				int sizeY = (bmp.Height / yScale) * 16;
				
				// determine if we actually need filtering
				for( int y = 0; y < sizeY; y++ ) {
					int* row = fastBmp.GetRowPtr( y );
					row += sizeX;
					for( int x = 0; x < sizeX; x++ ) {
						byte alpha = (byte)(row[x] >> 24);
						if( alpha != 255 ) return;
					}
				}
				
				// only perform filtering when the entire hat is opaque
				int fullWhite = FastColour.White.ToArgb();
				int fullBlack = FastColour.Black.ToArgb();
				for( int y = 0; y < sizeY; y++ ) {
					int* row = fastBmp.GetRowPtr( y );
					row += sizeX;
					for( int x = 0; x < sizeX; x++ ) {
						int pixel = row[x];
						if( pixel == fullWhite || pixel == fullBlack ) row[x] = 0;
					}
				}
			}
		}
	}
}