using UnityEditor;
using Kryz.RPG.Items.Views;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Kryz.RPG.Items.Editor
{
	[CustomPropertyDrawer(typeof(SerializeInterface<>), useForChildren: true)]
	public class SerializeInterfacePropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			SerializedProperty unityObjectProperty = property.FindPropertyRelative("unityObject");
			PropertyField propertyField = new(unityObjectProperty, property.displayName);

			propertyField.RegisterCallbackOnce<GeometryChangedEvent>(_ =>
			{
				ObjectField objectField = propertyField.Q<ObjectField>();
				objectField.allowSceneObjects = true;
				objectField.objectType = fieldInfo.FieldType.GenericTypeArguments[0];
			});

			return propertyField;
		}
	}
}