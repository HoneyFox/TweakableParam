using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace TweakableParam
{
	public class ModuleTweakableSubParam
	{
		public PartModule.StartState m_startState = PartModule.StartState.None;

		[KSPField(isPersistant = true)]
		public string targetField;

		[KSPField(isPersistant = true)]
		public float tweakedValue = -1.0f;

		public List<float> tweakedCurve = null;

		[KSPField(isPersistant = false)]
		public float minValue;
		[KSPField(isPersistant = false)]
		public float maxValue;
		[KSPField(isPersistant = false)]
		public float stepValue;

		[KSPField(isPersistant = true)]
		public bool setOnlyOnLaunchPad = false;

		public ModuleTweakableParam parentModule = null;

		public void OnStart(PartModule.StartState state)
		{
			m_startState = state;
			// Single Target Mode.
			Debug.Log("Trying to get field info of: " + targetField);
			SetInitialValueOfField(targetField);
		}

		public void SetInitialValueOfField(string targetField)
		{
			object obj = null;
			FieldInfo fi = GetFieldInfo(targetField, out obj);
			if (obj != null)
			{
				if (fi.FieldType.Equals(typeof(Single)) || fi.FieldType.Equals(typeof(Double)) || fi.FieldType.Equals(typeof(Int32)) || fi.FieldType.Equals(typeof(UInt32)))
				{
					Debug.Log("Field type is: " + fi.FieldType.Name);
					if (m_startState == PartModule.StartState.Editor)
					{
						TweakableParamGUI.GetInstance().CheckClear();
						TweakableParamGUIItem item = new TweakableParamGUIItem(TweakableParamGUI.GetInstance(), this);
						if (tweakedValue == -1.0f)
						{
							tweakedValue = Convert.ToSingle(fi.GetValue(obj));
							Debug.Log("Field is: " + tweakedValue.ToString());
						}

						if (this.parentModule != null)
						{
							this.parentModule.UpdateTweakedValue(this);
						}
					}
					else
					{
						TweakableParamGUI.GetInstance().ClearGUIItem();

						if (tweakedValue > maxValue) tweakedValue = maxValue;
						if (tweakedValue < minValue) tweakedValue = minValue;
						Debug.Log(String.Format("Setting tweakable parameter: {0} to {1}", fi.Name, tweakedValue));
						if (!setOnlyOnLaunchPad || ((int)m_startState & (int)(PartModule.StartState.PreLaunch)) != 0)
							fi.SetValue(obj, Convert.ChangeType(tweakedValue, fi.FieldType));
					}
				}
				else
				{ 
					// Float curve.
					Debug.Log("Field type is: " + fi.FieldType.Name);
					if (m_startState == PartModule.StartState.Editor)
					{
						TweakableParamGUI.GetInstance().CheckClear();
						TweakableParamGUIItem item = new TweakableParamGUIItem(TweakableParamGUI.GetInstance(), this);
						if (tweakedCurve == null)
						{
							Debug.Log("Starting analyzing FloatCurve.");
							tweakedCurve = GetKeysFromFloatCurve((FloatCurve)fi.GetValue(obj));
						}

						if (this.parentModule != null)
						{
							this.parentModule.UpdateTweakedValue(this);
						}
					}
					else
					{
						TweakableParamGUI.GetInstance().ClearGUIItem();

						Debug.Log(String.Format("Setting tweakable parameter: {0} to {1}", fi.Name, tweakedCurve));
						if (!setOnlyOnLaunchPad || ((int)m_startState & (int)(PartModule.StartState.PreLaunch)) != 0)
						{
							if (tweakedCurve == null)
							{ 
								fi.SetValue(obj, Convert.ChangeType(tweakedValue, fi.FieldType)); 
							}
							else
							{
								FloatCurve newFloatCurve = new FloatCurve();
								for (int i = 0; i < tweakedCurve.Count / 2; ++i)
									newFloatCurve.Add(tweakedCurve[i * 2], tweakedCurve[i * 2 + 1]);

								fi.SetValue(obj, newFloatCurve);
							}
						}
					}
				}
			}
		}

		public List<float> GetKeysFromFloatCurve(FloatCurve curve)
		{
			try
			{
				ConfigNode myConfigNode = new ConfigNode("CurveNode");
				curve.Save(myConfigNode);

				Debug.Log("Curve data acquired.");
				List<float> result = new List<float>(myConfigNode.values.Count * 2);
				for (int i = 0; i < myConfigNode.values.Count; ++i)
				{
					string keyValue = myConfigNode.values[i].value;
					Debug.Log(keyValue);
					string[] keyValueParts = keyValue.Split(' ');
					result.Add(Convert.ToSingle(keyValueParts[0]));
					result.Add(Convert.ToSingle(keyValueParts[1]));
				}
				return result;
			}
			catch (Exception e)
			{
				Debug.Log(e.Message);
				return null;
			}
		}

		public FieldInfo GetFieldInfo(string target, out object obj)
		{
			// target:
			// For Part: FieldName
			// For Module: Module(ModuleName).FieldName
			// Resource(ResourceName).FieldName

			object parent = this.parentModule.part;
			PartModule targetModule = null;
			PartResource targetResource = null;
			string[] hierarchies = target.Split('.');
			foreach (string hierarchy in hierarchies)
			{
				if (hierarchy.StartsWith("Module(") && hierarchy.EndsWith(")"))
				{
					if (targetModule != null || targetResource != null)
					{
						Debug.Log("Invalid target field.");
						obj = null;
						return null;
					}
					string moduleName = hierarchy.Substring("Module(".Length).TrimEnd(')');
					targetModule = this.parentModule.part.Modules[moduleName];
					parent = targetModule;
				}
				else if (hierarchy.StartsWith("Resource(") && hierarchy.EndsWith(")"))
				{
					if (targetModule != null || targetResource != null)
					{
						Debug.Log("Invalid target field.");
						obj = null;
						return null;
					}
					string resourceName = hierarchy.Substring("Resource(".Length).TrimEnd(')');
					targetResource = this.parentModule.part.Resources[resourceName];
					parent = targetResource;
				}
				else
				{
					string fieldName = hierarchy;
					if (targetModule != null)
					{
						obj = parent;
						Debug.Log("Type is: " + targetModule.GetType().FullName);
						return targetModule.GetType().GetField(fieldName);
					}
					else if (targetResource != null)
					{
						obj = parent;
						Debug.Log("Type is: " + targetResource.GetType().FullName);
						return targetResource.GetType().GetField(fieldName);
					}
					else
					{
						obj = parent;
						Debug.Log("Type is: " + this.parentModule.part.GetType().FullName);
						return this.parentModule.part.GetType().GetField(fieldName);
					}
				}
			}
			obj = null;
			return null;
		}

		public void DecreaseValue()
		{
			tweakedValue -= stepValue;
			if (tweakedValue < minValue) tweakedValue = minValue;
			if (tweakedValue > maxValue) tweakedValue = maxValue;

			if (this.parentModule != null)
			{
				this.parentModule.UpdateTweakedValue(this);
			}
		}

		public void IncreaseValue()
		{
			tweakedValue += stepValue;
			if (tweakedValue < minValue) tweakedValue = minValue;
			if (tweakedValue > maxValue) tweakedValue = maxValue;

			if (this.parentModule != null)
			{
				this.parentModule.UpdateTweakedValue(this);
			}
		}

		public void AddCurvePoint(float time, float value)
		{
			if (tweakedCurve != null)
			{
				tweakedCurve.Add(time);
				tweakedCurve.Add(value);

				if (this.parentModule != null)
				{
					this.parentModule.UpdateTweakedValue(this);
				}
			}
		}

		public void ClearCurvePoints()
		{
			if (tweakedCurve != null)
			{
				tweakedCurve.Clear();

				if (this.parentModule != null)
				{
					this.parentModule.UpdateTweakedValue(this);
				}
			}
		}
	}
}