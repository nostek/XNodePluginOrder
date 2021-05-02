﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditorInternal;
using UnityEditor;
using XNode;
using XNodeEditor;

namespace XNodeEditor
{
	[CreateAssetMenu(menuName = "xNode/Order", fileName = "xNode Order")]
	public class XNodeOrder : ScriptableObject
	{
		static XNodeOrder instance = null;

		static XNodeOrder Instance
		{
			get
			{
				if (instance == null)
					instance = AssetDatabase.FindAssets("t:" + typeof(XNodeOrder))
					.Select(guid => AssetDatabase.LoadAssetAtPath<XNodeOrder>(AssetDatabase.GUIDToAssetPath(guid)))
					.First();
				return instance;
			}
		}

		[Serializable]
		struct Settings
		{
			public string Type;
			public string Path;
		}

		[SerializeField] Settings[] order = null;

		public static int GetNodeMenuOrder(Type type, int fallback)
		{
			if (Instance == null)
				return fallback;

			var stype = type.AssemblyQualifiedName;

			for (int i = 0; i < Instance.order.Length; i++)
				if (Instance.order[i].Type == stype)
					return i;

			return fallback;
		}

		public static string GetNodeMenuName(Type type, string fallback)
		{
			if (Instance == null)
				return fallback;

			var stype = type.AssemblyQualifiedName;

			for (int i = 0; i < Instance.order.Length; i++)
				if (Instance.order[i].Type == stype)
				{
					if (!String.IsNullOrEmpty(Instance.order[i].Path))
						return Instance.order[i].Path;
					break;
				}

			return fallback;
		}
	}

	[CustomEditor(typeof(XNodeOrder))]
	public class EditorXNodeOrder : Editor
	{
		const string PropertyOrder = "order";
		const string PropertyType = "Type";
		const string PropertyPath = "Path";

		ReorderableList list = null;

		void OnEnable()
		{
			Fill();

			var prop = serializedObject.FindProperty(PropertyOrder);
			list = new ReorderableList(serializedObject, prop, true, true, false, false)
			{
				drawHeaderCallback = (rect) =>
				{
					EditorGUI.LabelField(rect, prop.displayName);
				},
				drawElementCallback = (rect, index, isActive, isFocused) =>
				{
					if (prop.arraySize == 0)
						return;

					var element = prop.GetArrayElementAtIndex(index);
					if (element == null)
						return;

					element.isExpanded = true;

					var ptype = element.FindPropertyRelative(PropertyType);
					var ppath = element.FindPropertyRelative(PropertyPath);

					float w = rect.width / 3f;

					var type = Type.GetType(ptype.stringValue);
					EditorGUI.SelectableLabel(new Rect(rect.x, rect.y, w, rect.height), NodeEditorUtilities.NodeDefaultName(type));

					var fallback = GetNodeMenuName(type);

					if (string.IsNullOrEmpty(ppath.stringValue))
					{
						var newpath = EditorGUI.TextField(new Rect(rect.x + w, rect.y, w + w, rect.height), fallback);
						if (newpath != fallback)
							ppath.stringValue = newpath;
					}
					else
					{
						var newpath = EditorGUI.TextField(new Rect(rect.x + w, rect.y, w + w, rect.height), ppath.stringValue);
						if (newpath == fallback)
							ppath.stringValue = "";
						else
							ppath.stringValue = newpath;
					}
				},
				elementHeightCallback = (index) =>
				{
					return EditorGUIUtility.singleLineHeight;
				}
			};
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("Order is Top-Down");
			EditorGUILayout.LabelField("Clear Path to restore Node default path.");
			EditorGUILayout.Space();

			serializedObject.Update();

			list.DoLayoutList();

			serializedObject.ApplyModifiedProperties();
		}

		//Copy of NodeGraphEditor.cs
		/// <summary> Returns context node menu path. Null or empty strings for hidden nodes. </summary>
		public virtual string GetNodeMenuName(Type type)
		{
			//Check if type has the CreateNodeMenuAttribute
			XNode.Node.CreateNodeMenuAttribute attrib;
			if (NodeEditorUtilities.GetAttrib(type, out attrib)) // Return custom path
				return attrib.menuName;
			else // Return generated path
				return NodeEditorUtilities.NodeDefaultPath(type);
		}

		void Fill()
		{
			var nodes = GetAllNodes();

			var list = serializedObject.FindProperty(PropertyOrder);

			//remove old
			for (int i = list.arraySize - 1; i >= 0; i--)
			{
				var ptype = list.GetArrayElementAtIndex(i).FindPropertyRelative(PropertyType);

				var type = Type.GetType(ptype.stringValue);

				if (!nodes.Any(t => t == type))
					list.DeleteArrayElementAtIndex(i);
			}

			//add new
			nodes.ForEach(
				(t) =>
				{
					var stype = t.AssemblyQualifiedName;

					bool found = false;
					for (int i = 0; i < list.arraySize; i++)
					{
						var ptype = list.GetArrayElementAtIndex(i).FindPropertyRelative(PropertyType);
						if (ptype.stringValue == stype)
						{
							found = true;
							break;
						}
					}

					if (!found)
					{
						list.InsertArrayElementAtIndex(list.arraySize);

						var ele = list.GetArrayElementAtIndex(list.arraySize - 1);
						var ptype = ele.FindPropertyRelative(PropertyType);
						var ppath = ele.FindPropertyRelative(PropertyPath);

						ptype.stringValue = stype;
						ppath.stringValue = null;
					}
				}
			);

			serializedObject.ApplyModifiedProperties();
		}

		List<Type> GetAllNodes()
		{
			List<Type> types = new List<Type>();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				foreach (Type type in assembly.GetTypes())
					if (!type.IsAbstract && type.IsSubclassOf(typeof(Node)))
						types.Add(type);
			return types;
		}
	}
}
