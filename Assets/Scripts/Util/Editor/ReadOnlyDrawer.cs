using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    // public override VisualElement CreatePropertyGUI(SerializedProperty property)
    // {
    //     var container = new VisualElement();
    //     var propField = new PropertyField(property);
    //     // propField.SetEnabled(false);
    //     // var propField = new Label(property);
    //     container.Add(new Label("XXXXXXXXXXXXXXXX!"));
    //     // container.Add(propField);
    //     return container;
    //     // return base.CreatePropertyGUI(property);
    // }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // First get the attribute since it contains the range for the slider
        // RangeAttribute range = attribute as RangeAttribute;
        // EditorGUILayout.PropertyField(property,label, GUILayout)
        EditorGUI.BeginDisabledGroup(true);
        EditorGUI.PropertyField(position, property, label);
        EditorGUI.EndDisabledGroup();
        // EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }
}
