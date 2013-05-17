using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace TweakableParam
{
	public class ModuleTweakableParam : PartModule
	{
		public StartState m_startState = StartState.None;

		[KSPField(isPersistant = true)]
		public string targetField;

		[KSPField(isPersistant = true)]
		public float tweakedValue = -1.0f;
		
		[KSPField(isPersistant = false)]
		public float minValue;
		[KSPField(isPersistant = false)]
		public float maxValue;
		[KSPField(isPersistant = false)]
		public float stepValue;

		[KSPField(isPersistant = true)]
		public bool setOnlyOnLaunchPad = false;

		[KSPField(isPersistant = false)]
		public bool useMultipleParameterLogic = false;
		[KSPField(isPersistant = true)]
		public string tweakableParamModulesData = "";

		public List<ModuleTweakableParam> tweakableParams = new List<ModuleTweakableParam>();

		public override void OnAwake()
		{
			//Debug.Log("TweakableParam OnAwake()");
			if (AddTweakableParamGUI.s_gameObjectInstance == null)
			{
				AddTweakableParamGUI.s_gameObjectInstance = GameObject.Find("AddTweakableParamGUI") ?? new GameObject("AddTweakableParamGUI", typeof(AddTweakableParamGUI));
				AddTweakableParamGUI.s_singleton = AddTweakableParamGUI.s_gameObjectInstance.GetComponent<AddTweakableParamGUI>();
				if (AddTweakableParamGUI.s_singleton != null)
					Debug.Log("Initialization of AddTweakableParamGUI complete.");
			}
		}

		public override void OnStart(StartState state)
		{
			m_startState = state;
			if (useMultipleParameterLogic == false)
			{
				// Single Target Mode.
				Debug.Log("Trying to get field info of: " + targetField);
				SetInitialValueOfField(targetField);
				base.OnStart(state);
			}
			else
			{ 
				// Multiple Target Mode.
				Debug.Log("Now we'll parse multiple submodules.");
				ParseMultipleModules(tweakableParams, tweakableParamModulesData);
				foreach (ModuleTweakableParam module in tweakableParams)
				{
					module.OnStart(m_startState);
				}
				base.OnStart(state); 
			}
		}

		public void SetInitialValueOfField(string targetField)
		{
			object obj = null;
			FieldInfo fi = GetFieldInfo(targetField, out obj);
			if (obj != null)
			{
				Debug.Log("Field type is: " + fi.FieldType.Name);
				if (m_startState == StartState.Editor)
				{
					TweakableParamGUI.GetInstance().CheckClear();
					TweakableParamGUIItem item = new TweakableParamGUIItem(TweakableParamGUI.GetInstance(), this);
					if (tweakedValue == -1.0f)
					{
						Debug.Log("Field is: " + fi.GetValue(obj).ToString());
						tweakedValue = Convert.ToSingle(fi.GetValue(obj));
					}
				}
				else
				{
					TweakableParamGUI.GetInstance().ClearGUIItem();

					if (tweakedValue > maxValue) tweakedValue = maxValue;
					if (tweakedValue < minValue) tweakedValue = minValue;
					Debug.Log(String.Format("Setting tweakable parameter: {0} to {1}", fi.Name, tweakedValue));
					if (!setOnlyOnLaunchPad || ((int)m_startState & (int)(StartState.PreLaunch)) != 0)
						fi.SetValue(obj, Convert.ChangeType(tweakedValue, fi.FieldType));
				}
			}
		}

		public void ParseMultipleModules(List<ModuleTweakableParam> list, string data)
		{
			// Format: <Module(ModuleRCS).thrusterPower,,0.0,5.0,0.1,1>,<.....>,<.....>
			string firstPass = tweakableParamModulesData.Replace(">,", ">").Replace(">", "");
			string[] modules = tweakableParamModulesData.Split('<');
			foreach (string module in modules)
			{
				string[] fields = module.Split(',');
				ModuleTweakableParam newModule = (this.part.AddModule("ModuleTweakableParam") as ModuleTweakableParam);
				
				tweakableParams.Add(newModule);
				
				newModule.targetField = fields[0];
				newModule.minValue = Convert.ToSingle(fields[2]);
				newModule.maxValue = Convert.ToSingle(fields[3]);
				newModule.stepValue = Convert.ToSingle(fields[4]);
				newModule.setOnlyOnLaunchPad = (Convert.ToInt32(fields[5]) != 0);
				newModule.tweakedValue = (fields[1] == "" ? -1 : Convert.ToSingle(fields[1]));
			}
		}

		public FieldInfo GetFieldInfo(string target, out object obj)
		{ 
			// target:
			// For Part: FieldName
			// For Module: Module(ModuleName).FieldName
			// Resource(ResourceName).FieldName
			
			object parent = this.part;
			PartModule targetModule = null;
			PartResource targetResource = null;
			string[] hierarchies = target.Split('.');
			foreach(string hierarchy in hierarchies)
			{
				if(hierarchy.StartsWith("Module(") && hierarchy.EndsWith(")"))
				{
					if(targetModule != null || targetResource != null)
					{
						Debug.Log("Invalid target field.");
						obj = null;
						return null;
					}
					string moduleName = hierarchy.Substring("Module(".Length).TrimEnd(')');
					targetModule = this.part.Modules[moduleName];
					parent = targetModule;
				}
				else if(hierarchy.StartsWith("Resource(") && hierarchy.EndsWith(")"))
				{
					if(targetModule != null || targetResource != null)
					{
						Debug.Log("Invalid target field.");
						obj = null;
						return null;
					}
					string resourceName = hierarchy.Substring("Resource(".Length).TrimEnd(')');
					targetResource = this.part.Resources[resourceName];
					parent = targetResource;
				}
				else
				{
					string fieldName = hierarchy;
					if(targetModule != null)
					{
						obj = parent;
						Debug.Log("Type is: " + targetModule.GetType().FullName);
						return targetModule.GetType().GetField(fieldName);
					}
					else if(targetResource != null)
					{
						obj = parent;
						Debug.Log("Type is: " + targetResource.GetType().FullName);
						return targetResource.GetType().GetField(fieldName);
					}
					else
					{
						obj = parent;
						Debug.Log("Type is: " + this.part.GetType().FullName);
						return this.part.GetType().GetField(fieldName);
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
		}

		public void IncreaseValue()
		{
			tweakedValue += stepValue;
			if (tweakedValue < minValue) tweakedValue = minValue;
			if (tweakedValue > maxValue) tweakedValue = maxValue;
		}

		public override void OnFixedUpdate()
		{
			base.OnFixedUpdate();
		}
	}
}
