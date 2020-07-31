#region ReSharper
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
#endregion

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using VisualPinball.Engine.VPT.Kicker;
using VisualPinball.Unity.Game;
using VisualPinball.Unity.Extensions;
using VisualPinball.Unity.VPT.Table;

namespace VisualPinball.Unity.VPT.Kicker
{
	[ExecuteAlways]
	[AddComponentMenu("Visual Pinball/Kicker")]
	public class KickerBehavior : ItemBehavior<Engine.VPT.Kicker.Kicker, KickerData>, IConvertGameObjectToEntity
	{
		protected override string[] Children => null;

		protected override Engine.VPT.Kicker.Kicker GetItem() => new Engine.VPT.Kicker.Kicker(data);

		private void OnDestroy()
		{
			if (!Application.isPlaying) {
				_table.Kickers.Remove(Name);
			}
		}

		public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
		{
			Convert(entity, dstManager);

			var table = gameObject.GetComponentInParent<TableBehavior>().Item;
			Item.Init(table);

			dstManager.AddComponentData(entity, new KickerStaticData {
				Center = data.Center.ToUnityFloat2(),
				FallThrough = data.FallThrough,
				HitAccuracy = data.HitAccuracy,
				LegacyMode = data.LegacyMode,
				ZLow = table.GetSurfaceHeight(data.Surface, data.Center.X, data.Center.Y) * table.GetScaleZ()
			});
			dstManager.AddComponentData(entity, new KickerCollisionData());

			if (!data.LegacyMode) {
				using (var blobBuilder = new BlobBuilder(Allocator.Temp)) {
					ref var blobAsset = ref blobBuilder.ConstructRoot<KickerMeshVertexBlobAsset>();
					var vertices = blobBuilder.Allocate(ref blobAsset.Vertices, Item.KickerHit.HitMesh.Length);
					var normals = blobBuilder.Allocate(ref blobAsset.Normals, Item.KickerHit.HitMesh.Length);
					for (var i = 0; i < Item.KickerHit.HitMesh.Length; i++) {
						var v = Item.KickerHit.HitMesh[i];
						vertices[i] = new KickerMeshVertex { Vertex = v.ToUnityFloat3() };
						normals[i] = new KickerMeshVertex { Vertex = new float3(KickerHitMesh.Vertices[i].Nx, KickerHitMesh.Vertices[i].Ny, KickerHitMesh.Vertices[i].Nz) };
					}

					var blobAssetReference = blobBuilder.CreateBlobAssetReference<KickerMeshVertexBlobAsset>(Allocator.Persistent);
					dstManager.AddComponentData(entity, new ColliderMeshData { Value = blobAssetReference });
				}
			}

			// register
			transform.GetComponentInParent<Player>().RegisterKicker(Item, entity, gameObject);
		}

		public override ItemDataTransformType EditorPositionType => ItemDataTransformType.TwoD;
		public override Vector3 GetEditorPosition() => data.Center.ToUnityVector3(0f);
		public override void SetEditorPosition(Vector3 pos) => data.Center = pos.ToVertex2Dxy();

		public override ItemDataTransformType EditorRotationType => ItemDataTransformType.OneD;
		public override Vector3 GetEditorRotation() => new Vector3(data.Orientation, 0f, 0f);
		public override void SetEditorRotation(Vector3 rot) => data.Orientation = rot.x;

		public override void HandleMaterialRenamed(string undoName, string oldName, string newName)
		{
			TryRenameField(undoName, ref data.Material, oldName, newName);
		}
	}
}
