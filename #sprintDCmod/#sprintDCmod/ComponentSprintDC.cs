using System;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace sprintDCmod
{
	// Token: 0x02000002 RID: 2
	public class ComponentSprintDC : Component, IUpdateable
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public ComponentVitalStats ComponentVitalStats
		{
			get
			{
				return this.m_componentPlayer.ComponentVitalStats;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000002 RID: 2 RVA: 0x0000205D File Offset: 0x0000025D
		public UpdateOrder UpdateOrder
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002060 File Offset: 0x00000260
		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			base.Load(valuesDictionary, idToEntityMap);
			this.m_componentPlayer = base.Entity.FindComponent<ComponentPlayer>(true);
			this.m_componentLocomotion = base.Entity.FindComponent<ComponentLocomotion>(true);
			ValuesDictionary valuesDictionary2 = DatabaseManager.FindEntityValuesDictionary(base.Entity.ValuesDictionary.DatabaseObject.Name, true);
			this.WalkSpeed = valuesDictionary2.GetValue<ValuesDictionary>("Locomotion").GetValue<float>("WalkSpeed");
			this.CreativeFlySpeed = valuesDictionary2.GetValue<ValuesDictionary>("Locomotion").GetValue<float>("CreativeFlySpeed");
			this.Speed = valuesDictionary.GetValue<float>("Speed", 1f);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002104 File Offset: 0x00000304
		public void Update(float dt)
		{
			if (this.m_componentPlayer.ComponentInput.PlayerInput.Move.Z > 0f)
			{
				if (this.m_pressed && this.m_released)
				{
					this.m_pressed = (this.m_released = false);
					if (Time.RealTime - this.m_lastForwardTime < 0.3)
					{
						this.m_isSprinting = true;
						this.m_lastForwardTime = 0.0;
					}
					else
					{
						this.m_lastForwardTime = Time.RealTime;
					}
				}
				this.m_pressed = true;
			}
			else
			{
				this.m_isSprinting = false;
				this.m_componentLocomotion.WalkSpeed = this.WalkSpeed;
				this.m_componentLocomotion.CreativeFlySpeed = this.CreativeFlySpeed;
				if (this.m_pressed)
				{
					this.m_released = true;
				}
			}
			if (this.ComponentVitalStats.Food <= 0.3f || this.ComponentVitalStats.Stamina <= 0.1f || this.m_componentPlayer.ComponentSickness.IsSick || this.m_componentPlayer.ComponentFlu.HasFlu || this.m_componentPlayer.ComponentBody.ImmersionFactor > 0.1f || this.m_componentPlayer.ComponentRider.Mount != null || this.m_componentPlayer.ComponentBody.IsSneaking)
			{
				this.m_isSprinting = false;
			}
			if (this.m_isSprinting)
			{
				this.m_componentLocomotion.WalkSpeed = this.WalkSpeed * this.Speed;
				this.m_componentLocomotion.CreativeFlySpeed = this.CreativeFlySpeed * this.Speed;
				float num = (this.m_componentPlayer.ComponentLocomotion.LastWalkOrder != null) ? this.m_componentPlayer.ComponentLocomotion.LastWalkOrder.Value.Length() : 0f;
				float num2 = 1f / MathUtils.Max(this.m_componentPlayer.ComponentLevel.SpeedFactor, 0.75f);
				this.ComponentVitalStats.Stamina -= dt * (0.07f + 0.006f * num2) * num;
				this.ComponentVitalStats.Food -= dt * 0.01f;
				float environmentTemperature = this.ComponentVitalStats.m_environmentTemperature;
				if (environmentTemperature >= 4f)
				{
					this.ComponentVitalStats.Temperature = MathUtils.Min(21f, environmentTemperature + 10f, this.ComponentVitalStats.Temperature + dt * 1f);
				}
			}
			if (this.FocalOrder != null)
			{
				FppCamera fppCamera = this.m_componentPlayer.GameWidget.FindCamera<FppCamera>(true);
				if (this.m_componentPlayer.GameWidget.ActiveCamera == fppCamera)
				{
					this.m_focalNum = 1f;
					this.m_componentPlayer.GameWidget.ActiveCamera = new SprintDCCamera(this.m_componentPlayer.GameWidget);
				}
				else if (this.m_componentPlayer.GameWidget.ActiveCamera is SprintDCCamera)
				{
					float num3 = 0.05f;
					if (MathUtils.Abs(this.FocalOrder.Value - this.m_focalNum) <= num3)
					{
						this.m_focalNum = this.FocalOrder.Value;
						this.FocalOrder = null;
						if (this.m_focalNum <= 1f)
						{
							this.m_componentPlayer.GameWidget.ActiveCamera = fppCamera;
						}
					}
					else
					{
						this.m_focalNum += (this.FocalOrder.Value - this.m_focalNum) / MathUtils.Abs(this.FocalOrder.Value - this.m_focalNum) * dt * 0.75f;
					}
				}
				else
				{
					this.FocalOrder = null;
				}
			}
			if (this.m_lastIsSprinting != this.m_isSprinting)
			{
				if (this.m_isSprinting)
				{
					this.FocalOrder = new float?(1.25f);
				}
				else
				{
					this.FocalOrder = new float?(1f);
				}
			}
			this.m_lastIsSprinting = this.m_isSprinting;
		}

		// Token: 0x04000001 RID: 1
		public ComponentPlayer m_componentPlayer;

		// Token: 0x04000002 RID: 2
		public ComponentLocomotion m_componentLocomotion;

		// Token: 0x04000003 RID: 3
		public float WalkSpeed;

		// Token: 0x04000004 RID: 4
		public float CreativeFlySpeed;

		// Token: 0x04000005 RID: 5
		public float Speed;

		// Token: 0x04000006 RID: 6
		public bool m_isSprinting;

		// Token: 0x04000007 RID: 7
		public bool m_pressed;

		// Token: 0x04000008 RID: 8
		public bool m_released;

		// Token: 0x04000009 RID: 9
		public double m_lastForwardTime;

		// Token: 0x0400000A RID: 10
		public float m_focalNum;

		// Token: 0x0400000B RID: 11
		public float? FocalOrder;

		// Token: 0x0400000C RID: 12
		private bool m_lastIsSprinting;
	}
}
