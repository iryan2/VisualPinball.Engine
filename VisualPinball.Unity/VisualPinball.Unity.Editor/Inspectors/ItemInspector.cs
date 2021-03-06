// Visual Pinball Engine
// Copyright (C) 2020 freezy and VPE Team
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VisualPinball.Engine.Math;
using VisualPinball.Engine.VPT.Surface;

namespace VisualPinball.Unity.Editor
{
	public abstract class ItemInspector : UnityEditor.Editor
    {
		protected TableAuthoring _table;
		protected SurfaceAuthoring _surface;

		protected string[] _allMaterials = new string[0];
		protected string[] _allTextures = new string[0];

		public static event Action<IIdentifiableItemAuthoring, string, string> ItemRenamed;

		protected virtual void OnEnable()
		{

#if UNITY_EDITOR
			// for convenience move item behavior to the top of the list
			// we're opting to due this here as opposed to at import time since modifying objects
			// in this way caused them to not be part of the created object undo stack
			if (target != null && target is MonoBehaviour mb) {
				int numComp = mb.GetComponents<MonoBehaviour>().Length;
				for (int i = 0; i <= numComp; i++) {
					UnityEditorInternal.ComponentUtility.MoveComponentUp(mb);
				}
			}
#endif
			_table = (target as MonoBehaviour)?.gameObject.GetComponentInParent<TableAuthoring>();
			PopulateDropDownOptions();
		}
		protected virtual void OnDisable()
		{
		}
		
		protected void PopulateDropDownOptions()
		{
			if (_table == null) return;

			if (_table.data.Materials != null) {
				_allMaterials = new string[_table.data.Materials.Length + 1];
				_allMaterials[0] = "- none -";
				for (int i = 0; i < _table.data.Materials.Length; i++) {
					_allMaterials[i + 1] = _table.data.Materials[i].Name;
				}
				Array.Sort(_allMaterials, 1, _allMaterials.Length - 1);
			}
			if (_table.Textures != null) {
				_allTextures = new string[_table.Textures.Count + 1];
				_allTextures[0] = "- none -";
				_table.Textures.Select(tex => tex.Name).ToArray().CopyTo(_allTextures, 1);
				Array.Sort(_allTextures, 1, _allTextures.Length - 1);
			}
		}

		protected void OnPreInspectorGUI()
		{
			if (!(target is IEditableItemAuthoring item)) {
				return;
			}

			EditorGUI.BeginChangeCheck();
			var val = EditorGUILayout.TextField("Name", item.ItemData.GetName());
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit("Name", false);
				item.ItemData.SetName(val);
			}

			EditorGUI.BeginChangeCheck();
			bool newLock = EditorGUILayout.Toggle("IsLocked", item.IsLocked);
			if (EditorGUI.EndChangeCheck())
			{
				FinishEdit("IsLocked");
				item.IsLocked = newLock;
				SceneView.RepaintAll();
			}

			if (target is IIdentifiableItemAuthoring identity && target is MonoBehaviour bh) {
				if (identity.Name != bh.gameObject.name) {
					var oldName = identity.Name;
					identity.Name = bh.gameObject.name;
					ItemRenamed?.Invoke(identity, oldName, bh.gameObject.name);
				}
			}
		}

		public override void OnInspectorGUI()
		{
			var item = target as IEditableItemAuthoring;
			if (item == null) return;

			GUILayout.Space(10);
			if( GUILayout.Button( "Force Update Mesh" ) ) {
				item.MeshDirty = true;
			}

			if (item.MeshDirty) {
				item.RebuildMeshes();
			}
		}

		protected void ItemDataField(string label, ref float field, bool dirtyMesh = true)
		{
			EditorGUI.BeginChangeCheck();
			float val = EditorGUILayout.FloatField(label, field);
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit(label, dirtyMesh);
				field = val;
			}
		}

		public void ItemDataSlider(string label, ref float field, float leftVal, float rightVal, bool dirtyMesh = true)
		{
			EditorGUI.BeginChangeCheck();
			float val = EditorGUILayout.Slider(label, field, leftVal, rightVal);
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit(label, dirtyMesh);
				field = val;
			}
		}

		protected void ItemDataField(string label, ref int field, bool dirtyMesh = true)
		{
			EditorGUI.BeginChangeCheck();
			int val = EditorGUILayout.IntField(label, field);
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit(label, dirtyMesh);
				field = val;
			}
		}

		public void ItemDataSlider(string label, ref int field, int leftVal, int rightVal, bool dirtyMesh = true)
		{
			EditorGUI.BeginChangeCheck();
			int val = EditorGUILayout.IntSlider(label, field, leftVal, rightVal);
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit(label, dirtyMesh);
				field = val;
			}
		}

		protected void ItemDataField(string label, ref string field, bool dirtyMesh = true)
		{
			EditorGUI.BeginChangeCheck();
			string val = EditorGUILayout.TextField(label, field);
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit(label, dirtyMesh);
				field = val;
			}
		}

		protected void ItemDataField(string label, ref bool field, bool dirtyMesh = true)
		{
			EditorGUI.BeginChangeCheck();
			bool val = EditorGUILayout.Toggle(label, field);
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit(label, dirtyMesh);
				field = val;
			}
		}

		protected void ItemDataField(string label, ref Vertex2D field, bool dirtyMesh = true)
		{
			EditorGUI.BeginChangeCheck();
			Vertex2D val = EditorGUILayout.Vector2Field(label, field.ToUnityVector2()).ToVertex2D();
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit(label, dirtyMesh);
				field = val;
			}
		}

		protected void ItemDataField(string label, ref Vertex3D field, bool dirtyMesh = true)
		{
			EditorGUI.BeginChangeCheck();
			Vertex3D val = EditorGUILayout.Vector3Field(label, field.ToUnityVector3()).ToVertex3D();
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit(label, dirtyMesh);
				field = val;
			}
		}

		protected void ItemDataField(string label, ref Engine.Math.Color field, bool dirtyMesh = true)
		{
			EditorGUI.BeginChangeCheck();
			Engine.Math.Color val = EditorGUILayout.ColorField(label, field.ToUnityColor()).ToEngineColor();
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit(label, dirtyMesh);
				field = val;
			}
		}

		protected void SurfaceField(string label, ref string field, bool dirtyMesh = true)
		{
			if (_surface?.name != field) {
				_surface = null;
			}

			var mb = target as MonoBehaviour;
			if (_surface == null && _table != null) {
				string currentFieldName = field;
				if (currentFieldName != null && _table.Table.Has<Surface>(currentFieldName)) {
					_surface = _table.gameObject.GetComponentsInChildren<SurfaceAuthoring>(true)
						.FirstOrDefault(s => s.name == currentFieldName);
				}
			}

			EditorGUI.BeginChangeCheck();
			_surface = (SurfaceAuthoring)EditorGUILayout.ObjectField(label, _surface, typeof(SurfaceAuthoring), true);
			if (EditorGUI.EndChangeCheck()) {
				FinishEdit(label, dirtyMesh);
				field = _surface != null ? _surface.name : "";
			}
		}

		protected void DropDownField<T>(string label, ref T field, string[] optionStrings, T[] optionValues, bool dirtyMesh = true) where T : IEquatable<T>
		{
			if (optionStrings == null || optionValues == null || optionStrings.Length != optionValues.Length) {
				return;
			}

			int selectedIndex = 0;
			for (int i = 0; i < optionValues.Length; i++) {
				if (optionValues[i].Equals(field)) {
					selectedIndex = i;
					break;
				}
			}
			EditorGUI.BeginChangeCheck();
			selectedIndex = EditorGUILayout.Popup(label, selectedIndex, optionStrings);
			if (EditorGUI.EndChangeCheck() && selectedIndex >= 0 && selectedIndex < optionValues.Length) {
				FinishEdit(label, dirtyMesh);
				field = optionValues[selectedIndex];
			}
		}

		protected void TextureField(string label, ref string field, bool dirtyMesh = true)
		{
			if (_table == null) return;

			// if the field is set, but the tex isn't in our list, maybe it was added after this
			// inspector was instantiated, so re-grab our options from the table data
			if (!string.IsNullOrEmpty(field) && !_allTextures.Contains(field)) {
				PopulateDropDownOptions();
			}

			int selectedIndex = 0;
			for (int i = 0; i < _allTextures.Length; i++) {
				if (_allTextures[i].ToLower() == field.ToLower()) {
					selectedIndex = i;
					break;
				}
			}
			EditorGUI.BeginChangeCheck();
			selectedIndex = EditorGUILayout.Popup(label, selectedIndex, _allTextures);
			if (EditorGUI.EndChangeCheck() && selectedIndex >= 0 && selectedIndex < _allTextures.Length) {
				FinishEdit(label, dirtyMesh);
				field = selectedIndex == 0 ? "" : _allTextures[selectedIndex];
			}
		}

		protected void MaterialField(string label, ref string field, bool dirtyMesh = true)
		{
			// if the field is set, but the material isn't in our list, maybe it was added after this
			// inspector was instantiated, so re-grab our mat options from the table data
			if (!string.IsNullOrEmpty(field) && !_allMaterials.Contains(field)) {
				PopulateDropDownOptions();
			}

			DropDownField(label, ref field, _allMaterials, _allMaterials, dirtyMesh);
			if (_allMaterials.Length > 0 && field == _allMaterials[0]) {
				field = ""; // don't store the none value string in our data
			}
		}

		protected virtual void FinishEdit(string label, bool dirtyMesh = true)
		{
			string undoLabel = $"[{target?.name}] Edit {label}";
			if (dirtyMesh) {
				// set dirty flag true before recording object state for the undo so meshes will rebuild after the undo as well
				var item = target as IEditableItemAuthoring;
				if (item != null) {
					item.MeshDirty = true;
					Undo.RecordObject(this, undoLabel);
				}
			}
			Undo.RecordObject(target, undoLabel);
		}

	}
}
