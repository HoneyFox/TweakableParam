using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace TweakableParamConfigInfuser
{
	public partial class FormMain : Form
	{
		public bool pathSelected = false;

		public FormMain()
		{
			InitializeComponent();
		}

		private void btnSelectPath_Click(object sender, EventArgs e)
		{
			if (folderDlg.ShowDialog() == DialogResult.OK)
			{
				lblPath.Text = folderDlg.SelectedPath;
				pathSelected = true;
				chkBoxList.Items.Clear();
				ShowPartList();
			}
		}

		private int findCorrespondingBracket(string content, int startIndex)
		{
			int bracketLevel = 1;
			int idx = startIndex + 1;
			while (bracketLevel > 0)
			{
				int bracketLevelAddIdx = content.IndexOf("{", idx);
				int bracketLevelSubIdx = content.IndexOf("}", idx);
				if (bracketLevelAddIdx < bracketLevelSubIdx)
				{
					bracketLevel++;
					idx = bracketLevelAddIdx + 1;
				}
				else
				{
					bracketLevel--;
					idx = bracketLevelSubIdx + 1;
				} 
			}

			return idx - 1;
		}

		private void ShowPartList()
		{
			string[] partFolders = Directory.GetDirectories(lblPath.Text);
			foreach (string partFolder in partFolders)
			{
				string partCfgFilePath = Path.Combine(lblPath.Text, Path.GetFileNameWithoutExtension(partFolder), "part.cfg");
				if(File.Exists(partCfgFilePath))
				{
					string partCfgFileContent = File.ReadAllText(partCfgFilePath);
					int useMultipleLogicIndex = partCfgFileContent.IndexOf("useMultipleParameterLogic", 0);
					if (useMultipleLogicIndex == -1)
					{
						chkBoxList.Items.Add(Path.GetFileNameWithoutExtension(partFolder), true);
					}
					else
					{
						int trueIndex = partCfgFileContent.IndexOf("true", useMultipleLogicIndex, StringComparison.CurrentCultureIgnoreCase);
						int falseIndex = partCfgFileContent.IndexOf("false", useMultipleLogicIndex, StringComparison.CurrentCultureIgnoreCase);
						if (trueIndex == -1)
						{
							chkBoxList.Items.Add(Path.GetFileNameWithoutExtension(partFolder), false);
						}
						else if (falseIndex == -1)
						{
							continue;
						}
						else if (trueIndex < falseIndex)
						{
							continue;
						}
						else
						{
							chkBoxList.Items.Add(Path.GetFileNameWithoutExtension(partFolder), false);
						}
					}
				}
			}
		}

		private void btnApply_Click(object sender, EventArgs e)
		{
			DialogResult dlgResult = MessageBox.Show("Are you sure to apply changes onto these selected parts?", "Confirm operation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (dlgResult == DialogResult.Yes)
			{
				ApplyChanges();
			}
		}

		private void ApplyChanges()
		{
			foreach (object o in chkBoxList.CheckedItems)
			{
				string folderName = o as string;
				string partCfgFilePath = Path.Combine(lblPath.Text, folderName, "part.cfg");
				if (File.Exists(partCfgFilePath))
				{
					string partCfgFileContent = File.ReadAllText(partCfgFilePath);
					int useMultipleLogicIndex = partCfgFileContent.IndexOf("useMultipleParameterLogic", 0);
					if (useMultipleLogicIndex == -1)
					{
						partCfgFileContent += Environment.NewLine + Environment.NewLine + 
@"MODULE
{
	name = ModuleTweakableParam
	useMultipleParameterLogic = true
}
";
						File.WriteAllText(partCfgFilePath, partCfgFileContent);
					}
					else
					{
						int falseIndex = partCfgFileContent.IndexOf("false", useMultipleLogicIndex, StringComparison.CurrentCultureIgnoreCase);
						partCfgFileContent = partCfgFileContent.Substring(0, falseIndex) + "true" + partCfgFileContent.Substring(falseIndex + 5);
						File.WriteAllText(partCfgFilePath, partCfgFileContent);
					}
				}
			}

			for(int i = 0; i < chkBoxList.Items.Count; ++i)
			{
				if (chkBoxList.GetItemChecked(i) == true)
				{
					chkBoxList.Items.RemoveAt(i);
					i--;
				}
			}
		}

		private void btnCheckAll_Click(object sender, EventArgs e)
		{
			for(int i = 0; i < chkBoxList.Items.Count; ++i)
				chkBoxList.SetItemChecked(i, true);
		}

		private void btnCheckNone_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < chkBoxList.Items.Count; ++i)
				chkBoxList.SetItemChecked(i, false);
		}

	}
}
