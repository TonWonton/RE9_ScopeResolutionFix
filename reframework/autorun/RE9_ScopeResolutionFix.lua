--SCRIPT INFO
local s_GUID = "RE9_ScopeResolutionFix"
local s_version = "1.0.1"

local s_GUIDAndVVersion = s_GUID .. " v" .. s_version
local s_logPrefix = "[" .. s_GUID .. "] "
local s_configFileName = s_GUID .. ".lua.json"



--LOG
local function LogInfo(message)
	log.info(s_logPrefix .. message)
end



--CONFIG
local tbl_config =
{
	f_scopeBaseZoomMultiplier = 2.0,
}

local function LoadFromJson()
	local tblLoadedConfig = json.load_file(s_configFileName)

	if tblLoadedConfig ~= nil then
        for key, val in pairs(tblLoadedConfig) do
            tbl_config[key] = val
        end
    end
end

local function SaveToJson()
	json.dump_file(s_configFileName, tbl_config)
end



--VARIABLES
local c_scopeCameraControllerV3 = nil
local c_adsCameraController = nil
local tbl_zoomStepDataOriginalFOV = {}



--FUNCTIONS
local function GetAddress(object)
	return object:get_address()
end

local function TryGetOriginalFOVFromZoomStepData(cZoomStepData)
	local tblZoomStepDataOriginalFOV = tbl_zoomStepDataOriginalFOV

	local fOriginalFOV = tblZoomStepDataOriginalFOV[GetAddress(cZoomStepData)]
	if fOriginalFOV ~= nil then
		return fOriginalFOV
	else
		fOriginalFOV = cZoomStepData._ZoomFov
		if fOriginalFOV ~= nil and fOriginalFOV ~= 0.0 then
			tblZoomStepDataOriginalFOV[GetAddress(cZoomStepData)] = fOriginalFOV
			return fOriginalFOV
		end
	end

	return nil
end

local function GetScopeFOV(fDesiredFOV, iZoomStep)
	local fZoomDivisor = tbl_config.f_scopeBaseZoomMultiplier + 0.1 * iZoomStep
	return fDesiredFOV / fZoomDivisor
end



--HOOKS
local function PreSetDisplayScope(args)
	if c_scopeCameraControllerV3 == nil then c_scopeCameraControllerV3 = sdk.to_managed_object(args[2]) end
	if c_scopeCameraControllerV3 ~= nil then
		local cParamUserData = c_scopeCameraControllerV3._ParamUserData
		if cParamUserData ~= nil then
			cParamUserData._LensImageDefaultScale = 1.0
			cParamUserData._LensImageZoomRate = 0.0
		end
	end
end

local function PostSetDisplayScope(retVal)
end

local function PreADSCameraControllerUpdateFOV(args)
	if c_adsCameraController == nil then c_adsCameraController = sdk.to_managed_object(args[2]) end
	if c_adsCameraController ~= nil then
		local cADSZoomData = c_adsCameraController:call("get_CurrentZoomData")
		if cADSZoomData ~= nil then
			local cZoomSteps = cADSZoomData._ZoomSteps
			if cZoomSteps ~= nil then
				local iZoomStepsLength = cZoomSteps:get_size()
				for i = 0, iZoomStepsLength - 1 do
					local cZoomStepData = cZoomSteps:get_element(i)
					if cZoomStepData ~= nil then
						local fOriginalFOV = TryGetOriginalFOVFromZoomStepData(cZoomStepData)
						if fOriginalFOV ~= nil then
							cZoomStepData._ZoomFov = GetScopeFOV(fOriginalFOV, i)
						end
					end
				end
			end
		end
	end
end

local function PostADSCameraControllerUpdateFOV(retVal)
end



--CALLBACKS
local b_hooksInitialized = false

local function PreUpdateBehavior()
	if b_hooksInitialized == false then
		local mSetDisplayScope = nil
		local mUpdateFOV = nil

		local tdScopeCameraControllerV3 = sdk.find_type_definition("app.ScopeCameraControllerV3")
		if tdScopeCameraControllerV3 ~= nil then
			mSetDisplayScope = tdScopeCameraControllerV3:get_method("setDisplayScope")
		end

		local tdADSCameraController = sdk.find_type_definition("app.ADSCameraController")
		if tdADSCameraController ~= nil then
			mUpdateFOV = tdADSCameraController:get_method("updateFOV")
		end

		if mSetDisplayScope ~= nil and mUpdateFOV ~= nil then
			sdk.hook(mSetDisplayScope, PreSetDisplayScope, PostSetDisplayScope)
			sdk.hook(mUpdateFOV, PreADSCameraControllerUpdateFOV, PostADSCameraControllerUpdateFOV)
			b_hooksInitialized = true
			LogInfo("Initialized")
		end
	end
end



--MAIN
LoadFromJson()
re.on_config_save(SaveToJson)
re.on_pre_application_entry("UpdateBehavior", PreUpdateBehavior)
LogInfo("Loaded " .. s_version)



--SCRIPT GENERATED UI
re.on_draw_ui(function()
	if imgui.tree_node(s_GUIDAndVVersion) then
		local tblConfig = tbl_config

		imgui.text("Scope")
		_, tblConfig.f_scopeBaseZoomMultiplier = imgui.drag_float("Scope base zoom multiplier", tblConfig.f_scopeBaseZoomMultiplier, 0.01, 0.0, 100.0)

		imgui.tree_pop()
	end
end)