using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Reflection;
using System;
using System.Text.RegularExpressions;

namespace Sperlich.GameSettings.Editor {
	[CustomPropertyDrawer(typeof(ConfigEntry))]
	public class ConfigEntryDrawer : PropertyDrawer {

		public override VisualElement CreatePropertyGUI(SerializedProperty property) {
			var root = new VisualElement();
			
			var typeProp = property.FindPropertyRelative("type");
			var isValueListProp = property.FindPropertyRelative("isValueList");
			var stringListProp = property.FindPropertyRelative("stringList");
			var intValueProp = property.FindPropertyRelative("intValue");
			var stringValueProp = property.FindPropertyRelative("stringValue");
			var selectIndexProp = property.FindPropertyRelative("selectIndex");
			var isListValueIndexProp = property.FindPropertyRelative("isListValueIndex");

			Settings.Type valueType = (Settings.Type)property.FindPropertyRelative("type").intValue;
			var category = new PropertyField(property.FindPropertyRelative("category"));
			var setting = new PropertyField(property.FindPropertyRelative("setting"));
			var type = new PropertyField(typeProp);
			var intValueElement = new PropertyField(intValueProp);
			var stringValueElement = new PropertyField(stringValueProp);
			var floatValueElement = new PropertyField(property.FindPropertyRelative("floatValue"));
			var boolValueElement = new PropertyField(property.FindPropertyRelative("boolValue"));
			var isValueListElement = new PropertyField(isValueListProp);
			var stringListElement = new PropertyField(stringListProp);
			var selectValueDropdown = new DropdownField("Default");

			intValueElement.label = "Default";
			floatValueElement.label = "Default";
			boolValueElement.label = "Default";
			stringValueElement.label = "Default";

			selectValueDropdown.RegisterValueChangedCallback((ev) => {
				int index = selectValueDropdown.index;

				if (StringValues().Length > index) {
					switch (Type()) {
						case Settings.Type.String:
							selectIndexProp.intValue = index;
							stringValueProp.stringValue = StringValues()[index];
							break;
						case Settings.Type.Integer:
							bool isNumber = int.TryParse(Regex.Match(ev.newValue, @"(?<=\]\s*)\d+").Value, out int result);
							if (isNumber) {
								intValueProp.intValue = result;
								isListValueIndexProp.boolValue = false;
							} else {
								isListValueIndexProp.boolValue = true;
							}

							selectIndexProp.intValue = index;
							stringValueProp.stringValue = StringValues()[index];
							break;
						case Settings.Type.Float:
							break;
						case Settings.Type.Boolean:
							break;
					}

				} else {
					selectIndexProp.intValue = 0;
					intValueProp.intValue = 0;
					stringValueProp.stringValue = "";
				}

				property.serializedObject.ApplyModifiedProperties();
			});

			root.Add(isValueListElement);
			root.Add(category);
			root.Add(setting);
			root.Add(type);
			root.Add(intValueElement);
			root.Add(floatValueElement);
			root.Add(boolValueElement);
			root.Add(selectValueDropdown);
			root.Add(stringListElement);
			root.Add(stringValueElement);

			root.schedule.Execute(() => {
				intValueElement.style.display = DisplayStyle.None;
				floatValueElement.style.display = DisplayStyle.None;
				boolValueElement.style.display = DisplayStyle.None;
				selectValueDropdown.style.display = DisplayStyle.None;
				isValueListElement.style.display = DisplayStyle.None;
				stringListElement.style.display = DisplayStyle.None;
				stringValueElement.style.display = DisplayStyle.None;
				bool enableDropDownValueList = false;

				switch (Type()) {
					case Settings.Type.String:
						
						break;
					case Settings.Type.Integer:
						break;
					case Settings.Type.Float:
						floatValueElement.style.display = DisplayStyle.Flex;
						break;
					case Settings.Type.Boolean:
						boolValueElement.style.display = DisplayStyle.Flex;
						break;
				}

				// Display Checkbox to toggle String-List
				if (Type() == Settings.Type.String || Type() == Settings.Type.Integer) {
					isValueListElement.style.display = DisplayStyle.Flex;
					enableDropDownValueList = IsValueList();
				}
				
				if (enableDropDownValueList) {
					// Show Toggle-List & DropDown
					selectValueDropdown.style.display = DisplayStyle.Flex;
					stringListElement.style.display = DisplayStyle.Flex;

					if (StringValues().Length > 0) {
						switch (Type()) {
							case Settings.Type.String:
								selectValueDropdown.choices = new List<string>(StringValues());
								selectValueDropdown.index = selectIndexProp.intValue;
								break;
							case Settings.Type.Integer:
								string[] choices = new string[StringValues().Length];

								for (int i = 0; i < choices.Length; i++) {
									string value = stringListProp.GetArrayElementAtIndex(i).stringValue;
									choices[i] = $"[{i}] {value}";
								}

								selectValueDropdown.choices = choices.ToList();
								selectValueDropdown.index = selectIndexProp.intValue;
								break;
							case Settings.Type.Float:
							case Settings.Type.Boolean:
								break;
						}	
					}
				} else {
					switch (Type()) {
						case Settings.Type.String:
							stringValueElement.style.display = DisplayStyle.Flex;
							break;
						case Settings.Type.Integer:
							intValueElement.style.display = DisplayStyle.Flex;
							break;
						case Settings.Type.Float:
							break;
						case Settings.Type.Boolean:
							break;
					}
				}
			}).Every(100);

			Settings.Type Type() => (Settings.Type)typeProp.intValue;
			bool IsValueList() => property.FindPropertyRelative("isValueList").boolValue;
			string[] StringValues() {
				string[] values = new string[stringListProp.arraySize];
				for (int i = 0; i < values.Length; i++) {
					string value = stringListProp.GetArrayElementAtIndex(i).stringValue;
					values[i] = value;
				}

				return values;
			}	
			return root;
		}
	}
}