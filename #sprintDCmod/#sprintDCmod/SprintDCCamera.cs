using System;
using Engine;
using Engine.Graphics;
using Game;

namespace sprintDCmod
{
	// Token: 0x02000003 RID: 3
	public class SprintDCCamera : BasePerspectiveCamera
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000006 RID: 6 RVA: 0x0000205D File Offset: 0x0000025D
		public override bool UsesMovementControls
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000007 RID: 7 RVA: 0x000024F1 File Offset: 0x000006F1
		public override bool IsEntityControlEnabled
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000008 RID: 8 RVA: 0x000024F4 File Offset: 0x000006F4
		public float FocalNum
		{
			get
			{
				if (this.m_componentSprintDC == null)
				{
					return 1f;
				}
				return this.m_componentSprintDC.m_focalNum;
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x0000250F File Offset: 0x0000070F
		public SprintDCCamera(GameWidget view) : base(view)
		{
			this.IsFocalControllable = false;
			this.m_componentSprintDC = view.Target.Entity.FindComponent<ComponentSprintDC>();
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002535 File Offset: 0x00000735
		public override void Activate(Camera previousCamera)
		{
			base.SetupPerspectiveCamera(previousCamera.ViewPosition, previousCamera.ViewDirection, previousCamera.ViewUp);
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002550 File Offset: 0x00000750
		public override void Update(float dt)
		{
			if (base.GameWidget.Target != null)
			{
				Vector3 eyePosition = base.GameWidget.Target.ComponentCreatureModel.EyePosition;
				Matrix matrix = Matrix.CreateFromQuaternion(base.GameWidget.Target.ComponentCreatureModel.EyeRotation);
				matrix.Translation = base.GameWidget.Target.ComponentCreatureModel.EyePosition;
				bool flag = false;
				TerrainRaycastResult? terrainRaycastResult = this.m_componentSprintDC.m_componentPlayer.ComponentMiner.Raycast<TerrainRaycastResult>(new Ray3(eyePosition, this.m_componentSprintDC.m_componentPlayer.ComponentBody.Matrix.Forward), 0, true, false, true);
				if (terrainRaycastResult != null && Vector3.Distance(terrainRaycastResult.Value.HitPoint(0f), eyePosition) <= 0.5f)
				{
					flag = true;
				}
				if (!flag)
				{
					matrix.Translation += this.m_componentSprintDC.m_componentPlayer.ComponentBody.Matrix.Forward * 0.1f + matrix.Forward * 0.25f;
				}
				base.SetupPerspectiveCamera(matrix.Translation, matrix.Forward, matrix.Up);
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600000C RID: 12 RVA: 0x00002698 File Offset: 0x00000898
		public override Matrix ProjectionMatrix
		{
			get
			{
				if (this.m_projectionMatrix == null)
				{
					this.m_projectionMatrix = new Matrix?(this.CalculateBaseProjectionMatrix(base.GameWidget.ViewWidget.ActualSize));
					ViewWidget viewWidget = base.GameWidget.ViewWidget;
					if (viewWidget.ScalingRenderTargetSize == null)
					{
						this.m_projectionMatrix *= SprintDCCamera.CreateScaleTranslation(0.5f * viewWidget.ActualSize.X, -0.5f * viewWidget.ActualSize.Y, viewWidget.ActualSize.X / 2f, viewWidget.ActualSize.Y / 2f) * viewWidget.GlobalTransform * SprintDCCamera.CreateScaleTranslation(2f / (float)Display.Viewport.Width, -2f / (float)Display.Viewport.Height, -1f, 1f);
					}
				}
				return this.m_projectionMatrix.Value;
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000027C0 File Offset: 0x000009C0
		public Matrix CalculateBaseProjectionMatrix(Vector2 wh)
		{
			float num = 90f;
			float num2 = 1f;
			
			// SOLUCIÓN: Reemplazar la referencia problemática
			// con valores fijos o lógica alternativa
			int viewAngleMode = 0; // Valor por defecto
			
			if (viewAngleMode == 1)
			{
				num2 = 0.8f;
			}
			else if (viewAngleMode == 0) // Cambiado de null a 0
			{
				num2 = 0.9f;
			}
			
			float num3 = this.FocalNum * num2;
			float num4 = wh.X / wh.Y;
			float num5 = MathUtils.Min(num * num4, num);
			float num6 = num5 * num4;
			if (num6 < 90f)
			{
				num5 *= 90f / num6;
			}
			else if (num6 > 175f)
			{
				num5 *= 175f / num6;
			}
			return Matrix.CreatePerspectiveFieldOfView(MathUtils.DegToRad(num5 * num3), num4, 0.1f, 2048f);
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002868 File Offset: 0x00000A68
		public static Matrix CreateScaleTranslation(float sx, float sy, float tx, float ty)
		{
			return new Matrix(sx, 0f, 0f, 0f, 0f, sy, 0f, 0f, 0f, 0f, 1f, 0f, tx, ty, 0f, 1f);
		}

		// Token: 0x0400000D RID: 13
		public ComponentSprintDC m_componentSprintDC;

		// Token: 0x0400000E RID: 14
		public bool IsFocalControllable;
	}
}
