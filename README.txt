Tweakable Parameters
============================

This is a small part module allowing you to edit individual part's parameter in Editor mode of KSP.
And these settings will go with individual .craft file so you can have different settings for different ships.

* NOTE * 
Only numberic parameters can be tweaked by this plugin.
* NOTE * 
To obtain full list of parameters of part / part module, check the KSP_Data/Managed/Assembly-CSharp.dll in Visual Studio's Object Browser.

============================

To use this plugin, you first need to place the TweakableParam.dll into your KSP/Plugins folder.
After that, you need to add the following codes into part.cfg of parts that you want to use this feature.

Codes:

MODULE
{
	name = ModuleTweakableParam
	targetField = 
	setOnlyOnLaunchPad = 
	minValue = 
	maxValue = 
	stepValue = 
}

Then you need to enter the targetField of this module.
Here is the rule:
	For part's attribute, simply write the name of the attribute.
		eg. targetField = mass

		This will allow you to change the mass of a part in Editor mode.

	For an attribute of a part module, use Module(moduleName).attributeName.
		eg. targetField = Module(ModuleRCS).thrusterPower

		This will allow you to change the RCS block's thruster power level in Editor mode.
		So heavier ships can have higher thruster power while lighter ones can have lower power.

	For resources of the part, use Resource(resourceName).attributeName.
		eg. targetField = Resource(LiquidFuel).amount
		
		This will allow you to change the amount of the fuel tank.
		* NOTE * The setOnlyOnLaunchPad is optional (default value = false). It should be set to true in this fuel-tank amount case, otherwise it becomes cheaty because everytime you resume the flight it will refuel the tank.


Beside the targetField, the following three settings are quite self-explaining.
minValue/maxValue decide the range of adjustment.
stepValue decides how much you adjust in one click.