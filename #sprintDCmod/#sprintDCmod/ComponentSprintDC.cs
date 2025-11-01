using System;
using Engine;
using Game;
using GameEntitySystem;
using TemplatesDatabase;

namespace sprintDCmod
{
    public class ComponentSprintDC : Component, IUpdateable
    {
        public ComponentVitalStats ComponentVitalStats
        {
            get
            {
                return this.m_componentPlayer.ComponentVitalStats;
            }
        }

        public UpdateOrder UpdateOrder
        {
            get
            {
                return (UpdateOrder)0;
            }
        }

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

        [Obsolete]
        public void Update(float dt)
        {
            if (this.m_componentPlayer.ComponentInput.PlayerInput.Move.Z > 0f)
            {
                if (this.m_pressed && this.m_released)
                {
                    this.m_pressed = false;
                    this.m_released = false;
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

            if (this.ComponentVitalStats.Food <= 0.3f ||
                this.ComponentVitalStats.Stamina <= 0.1f ||
                (this.m_componentPlayer.ComponentSickness != null && this.m_componentPlayer.ComponentSickness.IsSick) ||
                (this.m_componentPlayer.ComponentFlu != null && this.m_componentPlayer.ComponentFlu.HasFlu) ||
                (this.m_componentPlayer.ComponentBody != null && this.m_componentPlayer.ComponentBody.ImmersionFactor > 0.1f) ||
                (this.m_componentPlayer.ComponentRider != null && this.m_componentPlayer.ComponentRider.Mount != null) ||
                !(this.m_componentPlayer.ComponentBody == null || !this.m_componentPlayer.ComponentBody.IsSneaking))
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

            if (this.FocalOrder.HasValue)
            {
                FppCamera fppCamera = this.m_componentPlayer.GameWidget.FindCamera<FppCamera>(true);
                if (fppCamera != null && this.m_componentPlayer.GameWidget.ActiveCamera == fppCamera)
                {
                    this.m_focalNum = 1f;
                    this.m_componentPlayer.GameWidget.ActiveCamera = new SprintDCCamera(this.m_componentPlayer.GameWidget);
                }
                else if (this.m_componentPlayer.GameWidget.ActiveCamera is SprintDCCamera)
                {
                    float num3 = 0.05f;
                    float focalOrderValue = this.FocalOrder.Value;
                    float difference = MathUtils.Abs(focalOrderValue - this.m_focalNum);
                    if (difference <= num3)
                    {
                        this.m_focalNum = focalOrderValue;
                        this.FocalOrder = null;
                        if (this.m_focalNum <= 1f && fppCamera != null)
                        {
                            this.m_componentPlayer.GameWidget.ActiveCamera = fppCamera;
                        }
                    }
                    else
                    {
                        this.m_focalNum += Math.Sign(focalOrderValue - this.m_focalNum) * dt * 0.75f;
                    }
                }
                else
                {
                    this.FocalOrder = null;
                }
            }

            if (this.m_lastIsSprinting != this.m_isSprinting)
            {
                this.FocalOrder = this.m_isSprinting ? 1.25f : 1f;
            }
            this.m_lastIsSprinting = this.m_isSprinting;
        }

        public ComponentPlayer m_componentPlayer;
        public ComponentLocomotion m_componentLocomotion;
        public float WalkSpeed;
        public float CreativeFlySpeed;
        public float Speed;
        public bool m_isSprinting;
        public bool m_pressed;
        public bool m_released;
        public double m_lastForwardTime;
        public float m_focalNum;
        public float? FocalOrder;
        private bool m_lastIsSprinting;
    }
}
