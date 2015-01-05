﻿using OpenTK;
using System;
using System.Drawing;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Renderers;

namespace ClassicalSharp.Model {

	public class SheepModel : IModel {
		
		ModelSet Set;
		public SheepModel( Game window ) : base( window ) {
			vertices = new VertexPos3fTex2fCol4b[6 * 6];
			Set = new ModelSet();
			Set.Head = MakeHead();
			Set.Torso = MakeTorso();
			Set.LeftLegFront = MakeLeg( -0.3125f, -0.0625f, -0.4375f, -0.1875f );
			Set.RightLegFront = MakeLeg( 0.0625f, 0.3125f, -0.4375f, -0.1875f );
			Set.LeftLegBack = MakeLeg( -0.3125f, -0.0625f, 0.3125f, 0.5625f );
			Set.RightLegBack = MakeLeg( 0.0625f, 0.3125f, 0.3125f, 0.5625f );
			vertices = null;

			DefaultSkinTextureId = graphics.LoadTexture( "sheep.png" );
		}
		
		ModelPart MakeHead() {
			return MakePart( 0, 0, 8, 6, 6, 8, 6, 6, -0.1875f, 0.1875f, 1f, 1.375f, -0.875f, -0.375f, false );
		}
		
		ModelPart MakeTorso() {
			return MakeRotatedPart( 28, 8, 6, 16, 8, 6, 8, 16, -0.25f, 0.25f, 0.75f, 1.125f, -0.5f, 0.5f, false );
		}
		
		ModelPart MakeLeg( float x1, float x2, float z1, float z2 ) {
			return MakePart( 0, 16, 4, 12, 4, 4, 4, 12, x1, x2, 0f, 0.75f, z1, z2, false );
		}
		
		public override float NameYOffset {
			get { return 1.075f; }
		}
		
		protected override void DrawPlayerModel( Player player, PlayerRenderer renderer ) {
			graphics.Texturing = true;
			int texId = DefaultSkinTextureId;
			graphics.Bind2DTexture( texId );
			
			DrawRotate( 0, 1.125f, -0.5f, -pitch, 0, 0, Set.Head );
			Set.Torso.Render();
			DrawRotate( 0, 0.75f, -0.3125f, leftLegXRot, 0, 0, Set.LeftLegFront );
			DrawRotate( 0, 0.75f, -0.3125f, rightLegXRot, 0, 0, Set.RightLegFront );
			DrawRotate( 0, 0.75f, 0.4375f, rightLegXRot, 0, 0, Set.LeftLegBack );
			DrawRotate( 0, 0.75f, 0.4375f, leftLegXRot, 0, 0, Set.RightLegBack );
			graphics.AlphaTest = true;
		}
		
		public override void Dispose() {
			Set.Dispose();
			graphics.DeleteTexture( DefaultSkinTextureId );
		}
		
		class ModelSet {
			
			public ModelPart Head, Torso, LeftLegFront, RightLegFront, LeftLegBack, RightLegBack;
			
			public void Dispose() {
				RightLegFront.Dispose();
				LeftLegFront.Dispose();
				RightLegBack.Dispose();
				LeftLegBack.Dispose();
				Torso.Dispose();
				Head.Dispose();
			}
		}
	}
}