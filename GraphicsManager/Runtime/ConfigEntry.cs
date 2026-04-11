using Sperlich.GameSettings;
using static Sperlich.GameSettings.Settings;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Linq;

[System.Serializable]
public class ConfigEntry {

	[SerializeField]
	private Categories category;
	[SerializeField]
	private GameSetting setting;
	[SerializeField]
	private Type type;
	[SerializeField]
	internal List<string> stringList;

	[SerializeField]
	[Min(0)]
	private int intValue;
	[SerializeField]
	[Range(0f, 1f)]
	private float floatValue;
	[SerializeField]
	private string stringValue;
	[SerializeField]
	private bool boolValue;
	[SerializeField]
	private int selectIndex;

	[SerializeField]
	private bool isValueList;
	[SerializeField]
	private bool isListValueIndex;

	public bool IsValueList => isValueList;
	public bool IsListValueIndex => isListValueIndex;
	public Type Type => type;
	public GameSetting Setting => setting;
	public Categories Category => category;
	public object Value {
		get {
			switch (type) {
				default:
				case Type.String:
					return stringValue;
				case Type.Integer:
					return isListValueIndex ? selectIndex : intValue;
				case Type.Float:
					return floatValue;
				case Type.Boolean:
					return boolValue;
			}
		}
	}
	public List<string> StringList => stringList;

	public bool TryGetListValueAsIntegerByIndex(int index, out int result) {
		if (index < stringList.Count) {
			if (GameSettings.TryParse(stringList[index], out result)) {
				return true;
			} else {
				Debug.LogError($"Failed to parse {stringList[index]} to an Integer.");
			}
		} else {
			Debug.LogError($"Index exceeded StringList of length {stringList.Count}");
		}

		result = 0;
		return false;
	}
	public string GetByIndex(int index) {
		return stringList[index];
	}
	public bool TryGetIndex(string value, out int index) {
		for(index = 0; index < stringList.Count; index++) {
			if (stringList[index].ToLower() == value.ToLower()) {
				return true;
			}
		}

		return false;
	}
	public T ParseValue<T>(object value) {
		switch (type) {
			case Type.String:
				return (T)(object)value.ToString();
			case Type.Integer:
				break;
			case Type.Float:
				break;
			case Type.Boolean:
				break;
			default:
				break;
		}
		/*switch (desiredType) {
			case Type.String:
				return (T)(object)value.ToString();
			case Type.Integer:
				if (value is string istringValue) {
					return (T)(object)int.Parse(istringValue, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.CurrentInfo);
				} else if (value is float iintValue) {
					return (T)(object)Mathf.RoundToInt(iintValue);
				}
				break;
			case Type.Float:
				if(value is string fstringValue) {
					return (T)(object)float.Parse(fstringValue, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.CurrentInfo);
				} else if(value is int fintValue) {
					return (T)(object)fintValue;
				}
				break;
			case Type.Boolean:
				if(value is string bstringValue) {
					return (T)(object)(bstringValue.ToLower() == "true" ? 1 : 0);
				} else if(value is int intValue) {
					return (T)(object)(intValue == 1);
				}
				break;
		}*/

		return (T)value;
	}
}