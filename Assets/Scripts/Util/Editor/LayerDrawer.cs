using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(Layer))]
public class LayerDrawer : PropertyDrawer {
    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        var root = new VisualElement();
        var field = new LayerField();
        field.BindProperty(property);
        root.Add(field);
        return root;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        property.serializedObject.Update();
        property.Next(true);
        int val = property.intValue;
        property.intValue = EditorGUI.LayerField(position, label, val);
        property.serializedObject.ApplyModifiedProperties();
    }
}