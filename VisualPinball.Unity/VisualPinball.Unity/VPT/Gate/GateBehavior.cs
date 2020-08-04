#region ReSharper
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
#endregion

using UnityEngine;
using VisualPinball.Engine.VPT.Gate;
using VisualPinball.Unity.Extensions;

namespace VisualPinball.Unity.VPT.Gate
{
	[ExecuteAlways]
	[AddComponentMenu("Visual Pinball/Gate")]
	public class GateBehavior : ItemBehavior<Engine.VPT.Gate.Gate, GateData>
	{
		protected override string[] Children => new []{"Wire", "Bracket"};

		protected override Engine.VPT.Gate.Gate GetItem() => new Engine.VPT.Gate.Gate(data);

		private void OnDestroy()
		{
			if (!Application.isPlaying) {
				_table.Remove<Engine.VPT.Gate.Gate>(Name);
			}
		}

		public override ItemDataTransformType EditorPositionType => ItemDataTransformType.ThreeD;
		public override Vector3 GetEditorPosition() => data.Center.ToUnityVector3(data.Height);
		public override void SetEditorPosition(Vector3 pos)
		{
			data.Center = pos.ToVertex2Dxy();
			data.Height = pos.z;
		}

		public override ItemDataTransformType EditorRotationType => ItemDataTransformType.OneD;
		public override Vector3 GetEditorRotation() => new Vector3(data.Rotation, 0f, 0f);
		public override void SetEditorRotation(Vector3 rot) => data.Rotation = rot.x;

		public override ItemDataTransformType EditorScaleType => ItemDataTransformType.OneD;
		public override Vector3 GetEditorScale() => new Vector3(data.Length, 0f, 0f);
		public override void SetEditorScale(Vector3 scale) => data.Length = scale.x;

		public override void HandleMaterialRenamed(string undoName, string oldName, string newName)
		{
			TryRenameField(undoName, ref data.Material, oldName, newName);
		}
	}
}
