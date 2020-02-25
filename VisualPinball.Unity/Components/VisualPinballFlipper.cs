﻿#region ReSharper
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
#endregion

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using VisualPinball.Engine.VPT.Flipper;
using VisualPinball.Engine.VPT.Table;
using VisualPinball.Unity.Physics.Flipper;

namespace VisualPinball.Unity.Components
{
	[ExecuteInEditMode]
	[AddComponentMenu("Visual Pinball/Flipper")]
	public class VisualPinballFlipper : ItemComponent<Flipper, FlipperData>, IConvertGameObjectToEntity
	{
		protected override string[] Children => new []{"Base", "Rubber"};

		protected override Flipper GetItem()
		{
			return new Flipper(data);
		}

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{

		}

		public void AddMovementData(Table table)
		{
			float flipperRadius;
			if (data.FlipperRadiusMin > 0 && data.FlipperRadiusMax > data.FlipperRadiusMin) {
				flipperRadius = data.FlipperRadiusMax - (data.FlipperRadiusMax - data.FlipperRadiusMin) /* m_ptable->m_globalDifficulty*/;
				flipperRadius = math.max(data.FlipperRadius, data.BaseRadius - data.EndRadius + 0.05f);

			} else {
				flipperRadius = data.FlipperRadiusMax;
			}

			var endRadius = math.max(data.EndRadius, 0.01f); // radius of flipper end
			flipperRadius = math.max(data.FlipperRadius, 0.01f); // radius of flipper arc, center-to-center radius
			var angleStart = math.radians(data.StartAngle);
			var angleEnd = math.radians(data.EndAngle);

			if (angleEnd == angleStart) {
				// otherwise hangs forever in collisions/updates
				angleEnd += 0.0001f;
			}

			var direction = angleEnd >= angleStart;
			var angle = angleStart;

			// model inertia of flipper as that of rod of length flipr around its end
			var mass = data.GetFlipperMass(table.Data);
			var inertia = (float) (1.0 / 3.0) * mass * (flipperRadius * flipperRadius);

			var materialData = new FlipperMaterialData {
				Inertia = inertia,
				AngleStart = angleStart,
				AngleEnd = angleEnd,
				Strength = data.GetStrength(table.Data),
				ReturnRatio = data.GetReturnRatio(table.Data),
				TorqueDamping = data.GetTorqueDamping(table.Data),
				TorqueDampingAngle = data.GetTorqueDampingAngle(table.Data),
				RampUpSpeed = data.GetRampUpSpeed(table.Data)
			};

			var movementData = new FlipperMovementData {
				Angle = angle,
				AngleSpeed = 0f,
				AngularMomentum = 0f,
				EnableRotateEvent = 0
			};

			var velocityData = new FlipperVelocityData {
				AngularAcceleration = 0f,
				ContactTorque = 0f,
				CurrentTorque = 0f,
				Direction = direction,
				IsInContact = false
			};

			var solenoidData = new SolenoidStateData {
				Value = false
			};
		}
	}
}
