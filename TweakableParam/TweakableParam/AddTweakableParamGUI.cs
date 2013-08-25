using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine;

namespace TweakableParam
{
	class AddTweakableParamGUI : MonoBehaviour
	{
		public static AddTweakableParamGUI s_singleton = null;
		public static GameObject s_gameObjectInstance = null;
		public static AddTweakableParamGUI GetInstance() { return CreateInstance(); }
		private static AddTweakableParamGUI CreateInstance() 
		{
			if (s_gameObjectInstance == null)
			{
				s_gameObjectInstance = new GameObject("AddTweakableParamGUI", typeof(AddTweakableParamGUI));
				UnityEngine.Object.DontDestroyOnLoad(s_gameObjectInstance);
				s_singleton = s_gameObjectInstance.GetComponent<AddTweakableParamGUI>();
			}
			return s_singleton; 
		}

		public void Awake()
		{
			DontDestroyOnLoad(this);
		}

		public void Update()
		{
			if (EditorLogic.fetch != null)
			{
				if (Input.GetKeyDown(KeyCode.P))
				{
					Debug.Log("P Received.");
					if (m_selectedPart != EditorLogic.fetch.PartSelected)
					{
						//Debug.Log("Part selected changed.");
						m_selectedPart = EditorLogic.fetch.PartSelected;
						if (m_selectedPart != null)
						{
							//Debug.Log("We've selected a part.");
							bool suitableModuleFound = false;
							for (int i = 0; i < m_selectedPart.Modules.Count; ++i)
							{
								PartModule module = m_selectedPart.Modules.GetModule(i);
								if (module is ModuleTweakableParam)
								{
									if ((module as ModuleTweakableParam).useMultipleParameterLogic == true)
									{
										suitableModuleFound = true;
										break;
									}
								}
							}

							ClearGUINode();
							if (suitableModuleFound)
								GenerateGUINodes();
						}
						else
						{
							//Debug.Log("We've selected nothing.");
							ClearGUINode();
						}
					}
					else
					{ 
						//Debug.Log("Part selected unchanged: " + ((m_selectedPart == null) ? "null" : m_selectedPart.name));
					}
				}
			}
			else
			{
				m_selectedPart = null;
				ClearGUINode();
			}
		}

		private void GenerateGUINodes()
		{
			if (m_selectedPart == null) return;

			//Debug.Log("Generating nodes...");
			FieldInfo[] fields = m_selectedPart.GetType().GetFields();
			foreach (FieldInfo fi in fields)
			{
				//Debug.Log("Iterating all fields: " + fi.FieldType.Name + " " + fi.Name + " (" + (fi.IsPublic ? "public" : "non-public") + ")");
				if (fi.IsPublic == false)
				{
					//Debug.Log("Non-public field, skipping...");
					continue;
				}
				if
				(
					fi.FieldType.Equals(typeof(Double)) ||
					fi.FieldType.Equals(typeof(Single))
				)
				{ 
					// This is a valid node.
					ReflectedObjectGUINode node = new ReflectedObjectGUINode(null, this, fi, fi.Name, true, "0.0", "1.0", "0.1", false);
					RegisterGUINode(node);
				}
				else if
				(
					fi.FieldType.Equals(typeof(UInt32)) ||
					fi.FieldType.Equals(typeof(Int32))
				)
				{
					// This is a valid node.
					ReflectedObjectGUINode node = new ReflectedObjectGUINode(null, this, fi, fi.Name, true, "0", "10", "1", false);
					RegisterGUINode(node);
				}
				else if
				(
					fi.FieldType.Equals(typeof(FloatCurve))
				)
				{ 
					// This is a float-curve node.
					Debug.Log("Float curve detected.");
					ReflectedCurveGUINode node = new ReflectedCurveGUINode(null, this, fi, fi.Name, true, "1", "-1", "0", false);
					RegisterGUINode(node);
				}
			}

			for (int i = 0; i < m_selectedPart.Modules.Count; ++i)
			{
				PartModule partModule = m_selectedPart.Modules.GetModule(i);
				//Debug.Log("Iterating all part modules: " + partModule.GetType().Name);
				string partModuleTypeName = partModule.GetType().Name;
				if (partModuleTypeName != "ModuleTweakableParam")
				{
					// This is a valid node.
					ReflectedObjectGUINode node = new ReflectedObjectGUINode(null, this, null, "Module(" + partModule.GetType().Name + ")", false, null, null, null, false);
					
					FieldInfo[] moduleFields = partModule.GetType().GetFields();
					foreach (FieldInfo fi in moduleFields)
					{
						//Debug.Log("Iterating all fields: " + fi.FieldType.ToString() + " " + fi.Name + " (" + (fi.IsPublic ? "public" : "non-public") + ")");
						if (fi.IsPublic == false)
						{
							//Debug.Log("Non-public field, skipping...");
							continue;
						}
						if
						(
							fi.FieldType.Equals(typeof(Double)) ||
							fi.FieldType.Equals(typeof(Single))
						)
						{
							// This is a valid node.
							ReflectedObjectGUINode subNode = new ReflectedObjectGUINode(node, this, fi, fi.Name, true, "0.0", "1.0", "0.1", false);
							node.m_subNodes.Add(subNode);
						}
						else if
						(
							fi.FieldType.Equals(typeof(UInt32)) ||
							fi.FieldType.Equals(typeof(Int32))
						)
						{
							// This is a valid node.
							ReflectedObjectGUINode subNode = new ReflectedObjectGUINode(node, this, fi, fi.Name, true, "0", "10", "1", false);
							node.m_subNodes.Add(subNode);
						}
						else if
						(
							fi.FieldType.Equals(typeof(FloatCurve))
						)
						{
							// This is a float-curve node.
							Debug.Log("Float curve detected.");
							ReflectedCurveGUINode subNode = new ReflectedCurveGUINode(node, this, fi, fi.Name, true, "1", "-1", "0", false);
							node.m_subNodes.Add(subNode);
						}
					}
					RegisterGUINode(node);
				}
			}

			for (int i = 0; i < m_selectedPart.Resources.Count; ++i)
			{
				PartResource partResource = m_selectedPart.Resources.list[i];
				string resourceName = partResource.resourceName;
				//Debug.Log("Iterating all part resources: " + resourceName);
				// This is a valid node.
				ReflectedObjectGUINode node = new ReflectedObjectGUINode(null, this, null, "Resource(" + resourceName + ")", false, null, null, null, false);

				FieldInfo[] resourceFields = partResource.GetType().GetFields();
				foreach (FieldInfo fi in resourceFields)
				{
					//Debug.Log("Iterating all fields: " + fi.FieldType.Name + " " + fi.Name);
					if (fi.IsPublic == false)
					{
						//Debug.Log("Non-public field, skipping...");
						continue;
					} 
					if
					(
						fi.FieldType.Equals(typeof(Double)) ||
						fi.FieldType.Equals(typeof(Single))
					)
					{
						if (fi.Name == "amount")
						{
							// This is a special node.
							double maxValue = partResource.maxAmount;
							double stepValue = maxValue / 10.0;
							ReflectedObjectGUINode subNode = new ReflectedObjectGUINode(node, this, fi, fi.Name, true, "0.0", maxValue.ToString(), stepValue.ToString(), true);
							node.m_subNodes.Add(subNode);
						}
						else if (fi.Name == "maxAmount")
						{
							// This is a special node.
							double maxValue = partResource.maxAmount;
							double stepValue = maxValue / 10.0;
							ReflectedObjectGUINode subNode = new ReflectedObjectGUINode(node, this, fi, fi.Name, true, "0.0", maxValue.ToString(), stepValue.ToString(), false);
							node.m_subNodes.Add(subNode);
						}
						else
						{
							// This is a valid node.
							ReflectedObjectGUINode subNode = new ReflectedObjectGUINode(node, this, fi, fi.Name, true, "0.0", "1.0", "0.1", false);
							node.m_subNodes.Add(subNode);
						}
					}
				}
				RegisterGUINode(node);
			}
		}

		public void AddModuleToSelectedPart(string targetField, string min, string max, string step, bool setOnlyOnLaunchPad)
		{
			Debug.Log("Start iterating modules.");
			for(int i = 0; i < m_selectedPart.Modules.Count; ++i)
			{
				if (m_selectedPart.Modules.GetModule(i) is ModuleTweakableParam)
				{
					Debug.Log("Find a ModuleTweakableParam.");
					ModuleTweakableParam module = m_selectedPart.Modules.GetModule(i) as ModuleTweakableParam;
					if (module.useMultipleParameterLogic)
					{
						Debug.Log("Got it.");
						// We've found the one.
						AddModuleToSelectedPart(module, module.m_startState, module.part, targetField, min, max, step, setOnlyOnLaunchPad);
						break;
					}
					else
					{
						Debug.Log("It's using single parameter mode.");
					}
				}
			}

			Debug.Log("Now for all these symmetric counterparts");
			for (int h = 0; h < m_selectedPart.symmetryCounterparts.Count; ++h)
			{
				for (int i = 0; i < m_selectedPart.symmetryCounterparts[h].Modules.Count; ++i)
				{
					if (m_selectedPart.symmetryCounterparts[h].Modules.GetModule(i) is ModuleTweakableParam)
					{
						Debug.Log("Find a ModuleTweakableParam.");
						ModuleTweakableParam module = m_selectedPart.symmetryCounterparts[h].Modules.GetModule(i) as ModuleTweakableParam;
						if (module.useMultipleParameterLogic)
						{
							Debug.Log("Got it.");
							// We've found the one.
							AddModuleToSelectedPart(module, module.m_startState, module.part, targetField, min, max, step, setOnlyOnLaunchPad);
							break;
						}
						else
						{
							Debug.Log("It's using single parameter mode.");
						}
					}
				}
			}

		}

		public void AddModuleToSelectedPart(ModuleTweakableParam module, PartModule.StartState state, Part part, string targetField, string min, string max, string step, bool setOnlyOnLaunchPad)
		{
			Debug.Log("AddModuleToSelectedPart()");
			if (state != PartModule.StartState.Editor) return;

			Debug.Log(targetField + ": " + min + " - " + max + ", " + step + " " + (setOnlyOnLaunchPad ? "T" : "F"));
			float minValue = Convert.ToSingle(min);
			float maxValue = Convert.ToSingle(max);
			float stepValue = Convert.ToSingle(step);

			ModuleTweakableSubParam targetModule = null;
			int index = -1;
			for (int i = 0; i < module.tweakableParams.Count; ++i)
			{
				if (module.tweakableParams[i].targetField == targetField)
				{
					Debug.Log("Already have the module.");
					targetModule = module.tweakableParams[i];
					index = i;
				}
			}

			if (minValue == 1 && maxValue == -1 && stepValue == 0)
			{
				if (targetModule == null)
				{
					Debug.Log("Create a new module.");
					targetModule = new ModuleTweakableSubParam();
					targetModule.parentModule = module;

					targetModule.targetField = targetField;
					targetModule.minValue = minValue;
					targetModule.maxValue = maxValue;
					targetModule.stepValue = stepValue;
					targetModule.setOnlyOnLaunchPad = setOnlyOnLaunchPad;

					targetModule.tweakedCurve = null;

					targetModule.OnStart(PartModule.StartState.Editor);

					module.tweakableParams.Add(targetModule);
					if (module.tweakableParamModulesData != "")
						module.tweakableParamModulesData = module.tweakableParamModulesData.TrimEnd(',') + ",";
					module.tweakableParamModulesData += "<" +
						targetModule.targetField + "," +
						targetModule.parentModule.SerializeFloatCurve(targetModule.tweakedCurve) + "," +
						targetModule.minValue.ToString("F4") + "," +
						targetModule.maxValue.ToString("F4") + "," +
						targetModule.stepValue.ToString("F4") + "," +
						(targetModule.setOnlyOnLaunchPad ? "1" : "0") +
						">,";
				}
				else
				{
					targetModule.minValue = minValue;
					targetModule.maxValue = maxValue;
					targetModule.stepValue = stepValue;

					targetModule.ClearCurvePoints();

					int curIdx = -1;
					int curPos = -1;
					while (curIdx != index)
					{
						curPos = module.tweakableParamModulesData.IndexOf("<", curPos + 1);
						curIdx++;
						if (curPos == -1)
							break;
					}

					if (curIdx == index)
					{
						int endPos = module.tweakableParamModulesData.IndexOf(">", curPos) + 1;
						module.tweakableParamModulesData =
							module.tweakableParamModulesData.Substring(0, curPos) +
							"<" +
							targetModule.targetField + "," +
							targetModule.parentModule.SerializeFloatCurve(targetModule.tweakedCurve) + "," +
							targetModule.minValue.ToString("F4") + "," +
							targetModule.maxValue.ToString("F4") + "," +
							targetModule.stepValue.ToString("F4") + "," +
							(targetModule.setOnlyOnLaunchPad ? "1" : "0") +
							">"
							+ module.tweakableParamModulesData.Substring(endPos);
					}
				}
			}
			else
			{
				if (targetModule == null)
				{
					Debug.Log("Create a new module.");
					targetModule = new ModuleTweakableSubParam();
					targetModule.parentModule = module;

					targetModule.targetField = targetField;
					targetModule.minValue = minValue;
					targetModule.maxValue = maxValue;
					targetModule.stepValue = stepValue;
					targetModule.setOnlyOnLaunchPad = setOnlyOnLaunchPad;

					targetModule.tweakedValue = maxValue;

					targetModule.OnStart(PartModule.StartState.Editor);

					module.tweakableParams.Add(targetModule);
					if (module.tweakableParamModulesData != "")
						module.tweakableParamModulesData = module.tweakableParamModulesData.TrimEnd(',') + ",";
					module.tweakableParamModulesData += "<" +
						targetModule.targetField + "," +
						targetModule.tweakedValue.ToString("F4") + "," +
						targetModule.minValue.ToString("F4") + "," +
						targetModule.maxValue.ToString("F4") + "," +
						targetModule.stepValue.ToString("F4") + "," +
						(targetModule.setOnlyOnLaunchPad ? "1" : "0") +
						">,";
				}
				else
				{
					targetModule.minValue = minValue;
					targetModule.maxValue = maxValue;
					targetModule.stepValue = stepValue;

					if (targetModule.tweakedValue > maxValue)
						targetModule.tweakedValue = maxValue;
					else if (targetModule.tweakedValue < minValue)
						targetModule.tweakedValue = minValue;

					int curIdx = -1;
					int curPos = -1;
					while (curIdx != index)
					{
						curPos = module.tweakableParamModulesData.IndexOf("<", curPos + 1);
						curIdx++;
						if (curPos == -1)
							break;
					}

					if (curIdx == index)
					{
						int endPos = module.tweakableParamModulesData.IndexOf(">", curPos) + 1;
						module.tweakableParamModulesData =
							module.tweakableParamModulesData.Substring(0, curPos) +
							"<" +
							targetModule.targetField + "," +
							targetModule.tweakedValue.ToString("F4") + "," +
							targetModule.minValue.ToString("F4") + "," +
							targetModule.maxValue.ToString("F4") + "," +
							targetModule.stepValue.ToString("F4") + "," +
							(targetModule.setOnlyOnLaunchPad ? "1" : "0") +
							">"
							+ module.tweakableParamModulesData.Substring(endPos);
					}
				}
			}
		}

		#region GUI Related
		Rect WindowPos = new Rect(300, 50, 400, 600);
		bool windowUpdated = false;
		bool editorLocked = false;
		Vector2 verticalScroll = Vector2.zero;
		#endregion

		private Part m_selectedPart = null;
		private List<ReflectedObjectGUINode> m_guiNodes = new List<ReflectedObjectGUINode>();

		public void RegisterGUINode(ReflectedObjectGUINode node)
		{
			if (m_guiNodes.Count == 0)
				AddGUI();

			if (m_guiNodes.Contains(node)) return;

			m_guiNodes.Add(node);
		}

		public void ClearGUINode()
		{
			if (m_guiNodes.Count != 0)
			{
				m_guiNodes.Clear();
				DeleteGUI();
			}
		}

		public void CheckClear()
		{
			if (m_selectedPart == null)
			{
				//Debug.Log("CheckClear: clearing.");
				ClearGUINode();
			}
		}

		private void AddGUI()
		{
			//Debug.Log("AddGUI of AddTweakableParamGUI");
			RenderingManager.AddToPostDrawQueue(3, DrawGUI);
		}

		private void DeleteGUI()
		{
			//Debug.Log("DeleteGUI of AddTweakableParamGUI");
			RenderingManager.RemoveFromPostDrawQueue(3, DrawGUI);
		}

		public void DrawGUI()
		{
			GUI.skin = HighLogic.Skin;

			if (windowUpdated)
			{
				WindowPos.width = 400.0f;
				WindowPos.height = 600.0f;
				windowUpdated = false;
			}

			CheckClear();

			WindowPos = GUILayout.Window(2121318, WindowPos, WindowFunc, "Select Parameter", GUILayout.Width(400.0f), GUILayout.Height(600.0f));
			Vector3 mousePos = Input.mousePosition;         //Mouse location; based on Kerbal Engineer Redux code
			mousePos.y = Screen.height - mousePos.y;
			bool cursorInGUI = WindowPos.Contains(mousePos);
			//This locks and unlocks the editor as necessary; cannot constantly call the lock or unlock functions as that causes the editor to be constantly locked
			if (cursorInGUI && !editorLocked && !EditorLogic.editorLocked)
			{
				EditorLogic.fetch.Lock(true, true, true);
				editorLocked = true;
			}
			else if (!cursorInGUI && editorLocked && EditorLogic.editorLocked)
			{
				EditorLogic.fetch.Unlock();
				editorLocked = false;
			}
		}

		public void WindowFunc(int id)
		{
			GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			{
				verticalScroll = GUILayout.BeginScrollView(verticalScroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				{
					//Debug.Log("Displaying nodes: " + m_guiNodes.Count.ToString());
					foreach (ReflectedObjectGUINode node in m_guiNodes)
					{
						node.RenderGUI();
					}
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();

			GUI.DragWindow(new Rect(0, 0, 2000, 30));
		}
	}

	class ReflectedObjectGUINode
	{
		public ReflectedObjectGUINode m_parent = null;
		public AddTweakableParamGUI m_gui = null;
		public FieldInfo m_fieldInfo = null;
		public string m_field = "";
		public List<ReflectedObjectGUINode> m_subNodes = new List<ReflectedObjectGUINode>();
		public bool m_isLeaf = true;
		public string m_inputMin = "0.0";
		public string m_inputMax = "1.0";
		public string m_inputStep = "0.1";
		public bool m_setOnlyOnLaunchPad = false;

		#region GUI Related
		protected bool isExpanded = false;
		#endregion

		public ReflectedObjectGUINode(ReflectedObjectGUINode parent, AddTweakableParamGUI gui, FieldInfo fieldInfo, string field, bool isLeaf, string inputMin, string inputMax, string inputStep, bool setOnlyOnLaunchPad)
		{
			m_parent = parent;
			m_gui = gui;
			m_fieldInfo = fieldInfo;
			m_field = field;
			m_isLeaf = isLeaf;
			if (inputMin != null)
				m_inputMin = inputMin;
			if (inputMax != null)
				m_inputMax = inputMax;
			if (inputStep != null)
				m_inputStep = inputStep;
			m_setOnlyOnLaunchPad = setOnlyOnLaunchPad;
		}

		public virtual void RenderGUI()
		{
			//Debug.Log("Rendering node: " + m_field);

			bool isExpandButtonClicked = false;

			if (m_gui == null || m_field == "") return;

			GUIStyle sty = new GUIStyle(GUI.skin.button);
			sty.normal.textColor = sty.focused.textColor = Color.white;
			sty.hover.textColor = sty.active.textColor = Color.yellow;
			sty.onNormal.textColor = sty.onFocused.textColor = sty.onHover.textColor = sty.onActive.textColor = Color.green;
			sty.padding = new RectOffset(4, 4, 4, 4);

			GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				{
					if (m_isLeaf == false)
						isExpandButtonClicked = GUILayout.Button((isExpanded ? "<" : ">"), sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
					GUILayout.Label(m_field, sty, GUILayout.ExpandWidth(true));
				}
				GUILayout.EndHorizontal();

				if(m_isLeaf == true)
				{
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					{ 
						GUILayout.Label("Min/Max/Step", GUILayout.ExpandWidth(true));
						m_inputMin = GUILayout.TextField(m_inputMin, GUILayout.Width(50));
						m_inputMin = Regex.Replace(m_inputMin, @"[^\d+-.]", "");
						m_inputMax = GUILayout.TextField(m_inputMax, GUILayout.Width(50));
						m_inputMax = Regex.Replace(m_inputMax, @"[^\d+-.]", "");
						m_inputStep = GUILayout.TextField(m_inputStep, GUILayout.Width(50));
						m_inputStep = Regex.Replace(m_inputStep, @"[^\d+.]", "");
						m_setOnlyOnLaunchPad = GUILayout.Toggle(m_setOnlyOnLaunchPad, " ", GUILayout.Width(30));
						if (GUILayout.Button("Add", GUILayout.ExpandWidth(true)))
						{ 
							// We need to add a module onto that part.
							string fullFieldName = m_field;
							Debug.Log(fullFieldName); 
							ReflectedObjectGUINode curNode = this;
							while(curNode.m_parent != null)
							{
								curNode = curNode.m_parent;
								fullFieldName = curNode.m_field + "." + fullFieldName;
								Debug.Log(fullFieldName);
							}

							if(Convert.ToSingle(m_inputMax) >= Convert.ToSingle(m_inputMin))
								AddTweakableParamGUI.GetInstance().AddModuleToSelectedPart(fullFieldName, m_inputMin, m_inputMax, m_inputStep, m_setOnlyOnLaunchPad);
						}
					}
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndVertical();

			if (isExpanded)
			{
				foreach (ReflectedObjectGUINode subNode in m_subNodes)
				{
					subNode.RenderGUI();
				}
			}

			if (isExpandButtonClicked)
				isExpanded = !isExpanded;
		}
	}

	class ReflectedCurveGUINode : ReflectedObjectGUINode
	{
		public ReflectedCurveGUINode(ReflectedObjectGUINode parent, AddTweakableParamGUI gui, FieldInfo fieldInfo, string field, bool isLeaf, string inputMin, string inputMax, string inputStep, bool setOnlyOnLaunchPad)
		: base(parent, gui, fieldInfo, field, isLeaf, inputMin, inputMax,inputStep, setOnlyOnLaunchPad)
		{
			inputMin = "0";
			inputMax = "0";
			inputStep = "0";
			setOnlyOnLaunchPad = false;
		}

		public override void RenderGUI()
		{
			//Debug.Log("Rendering node: " + m_field);

			bool isExpandButtonClicked = false;

			if (m_gui == null || m_field == "") return;

			GUIStyle sty = new GUIStyle(GUI.skin.button);
			sty.normal.textColor = sty.focused.textColor = Color.white;
			sty.hover.textColor = sty.active.textColor = Color.yellow;
			sty.onNormal.textColor = sty.onFocused.textColor = sty.onHover.textColor = sty.onActive.textColor = Color.green;
			sty.padding = new RectOffset(4, 4, 4, 4);

			GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			{
				GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
				{
					if (m_isLeaf == false)
						isExpandButtonClicked = GUILayout.Button((isExpanded ? "<" : ">"), sty, GUILayout.ExpandWidth(true), GUILayout.MaxWidth(30.0f));
					GUILayout.Label(m_field, sty, GUILayout.ExpandWidth(true));
				}
				GUILayout.EndHorizontal();

				if (m_isLeaf == true)
				{
					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					{
						if (GUILayout.Button("Replace Curve", GUILayout.ExpandWidth(true)))
						{
							// We need to add a module onto that part.
							string fullFieldName = m_field;
							Debug.Log(fullFieldName);
							ReflectedObjectGUINode curNode = this;
							while (curNode.m_parent != null)
							{
								curNode = curNode.m_parent;
								fullFieldName = curNode.m_field + "." + fullFieldName;
								Debug.Log(fullFieldName);
							}

							AddTweakableParamGUI.GetInstance().AddModuleToSelectedPart(fullFieldName, "1", "-1", "0", false);
						}
					}
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndVertical();

			if (isExpanded)
			{
				foreach (ReflectedObjectGUINode subNode in m_subNodes)
				{
					subNode.RenderGUI();
				}
			}

			if (isExpandButtonClicked)
				isExpanded = !isExpanded;
		}
	}
}
