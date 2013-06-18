Tweakable Parameters
============================

This is a small part module allowing you to edit individual part's parameter in Editor mode of KSP.
And these settings will go with individual .craft file so you can have different settings for different ships.

* NOTE * 
Only numberic parameters can be tweaked by this plugin.

============================

To use this plugin, you first need to place the TweakableParam.dll into your KSP/Plugins folder.
After that, you need to add the following codes into part.cfg of parts that you want to use this feature.

1) Single field mode:
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


2) Multi-field mode:
	Codes:
		MODULE
		{
			name = ModuleTweakableParam
			useMultipleParameterLogic = true
		}

	* NOTE *
	You can use the tool "TweakableParamConfigInfuser.exe" provided in the package to automatically insert the above module into your parts.
	See below for details.
	* NOTE *
	You can manually setup multiple target parameters in this mode by using:

	Codes:
		MODULE
		{
			name = ModuleTweakableParam
			useMultipleParameterLogic = true
			tweakableParamModulesData = <...>,<...>,<...>
		}

	Each <...> represents a target parameter. There are six fields inside: 
		targetField,
		tweakedValue(leave it blank so that the game will use the original value for this),
		minValue,
		maxValue,
		stepValue,
		setOnlyOnLaunchPad (use 0 for false, 1 for true)
	eg. tweakableParamModuleData = <Resource(MonoPropellant).amount,,0.0,100.0,10.0,1>,<Resource(MonoPropellant).maxAmount,,0.0,100.0,10.0,0>
	
	This example shows a adjustable RCS fuel tank with fuel amount ranging from 0 to 100. * NOTE * The field "amount" has setOnlyOnLaunchPad = true.

	
	In-Game Usage
		1) Enter editor mode.
		2) Build your rocket.
		3) Pick the part you want to add tweakable parameters by left clicking on it.
		4) Now press the "P" key to bring up the "Select Parameter" window.
		5) You can now place your part back to the rocket. The window will stay visible until you press "P" key again.
		6) Find parameters you want to tweak in the pop-up window.
		7) Enter the min/max/step values for the parameter, and check/uncheck the "setOnlyOnLaunchPad" toggle on the right.
		8) Click "Add" button on the right. You will now see this parameter appear in the "Tweakable Parameters" window.
		9) Adjust the value you want in the "Tweakable Parameters" window, and save your rocket.

=======================
TweakableParamConfigInfuser.exe

	This tool is simple to use.
	1) Launch the program.
	2) Click "..." button and select your part folder ( like Gamedata\Squad\Parts\ ).
	3) Now pick the parts you want to insert the module into. (with Check All/None buttons for convenience)
	4) Click "Add TweakableParam Capability to Selected Parts" button, and confirm your operation.
	5) Now (re)start your KSP game.