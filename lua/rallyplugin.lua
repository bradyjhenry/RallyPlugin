local timingBaseUrl = "http://" .. ac.getServerIP() .. ":" .. ac.getServerPortHTTP() .. "/RallyPlugin/"

local flags = {
    OPEN = 1,
    OCCUPIED = 2,
    WAIT = 3,
    STOP = 4,
    READY = 5,
    PENALTY = 6
}

local colorRed = rgbm(255, 0, 0, 0.1)
local colorGreen = rgbm(0, 255, 0, 0.1)
local colorYellow = rgbm(255, 191, 0, 0.1)

local boxColor = colorGreen

local messages = "haven't gotten message from server"

local rallyFlagEvent = {
    [flags.OPEN] = function()
        boxColor = colorGreen
        messages = "BOX IS OPEN"
    end,
    [flags.OCCUPIED] = function()
        boxColor = colorRed
        messages = "BOX IS OCCUPIED"
    end,
    [flags.WAIT] = function()
        boxColor = colorYellow
        messages = "WAIT"
    end,
    [flags.STOP] = function()
        boxColor = colorYellow
        messages = "STOP"
    end,
    [flags.READY] = function()
        boxColor = colorGreen
        messages = "READY"
    end,
    [flags.PENALTY] = function()
        boxColor = colorRed
        physics.teleportCarTo(car.index, ac.SpawnSet.Pits)
        messages = "PENALTY, GO TO PITS"
    end
}

local rallyEvent = ac.OnlineEvent({
    ac.StructItem.key("rallyFlag"),
    flags = ac.StructItem.byte()
}, function(sender, message)
    if sender ~= nil then return end
    rallyFlagEvent[message.flags]()
end)


function script.drawUI()
    ui.sameLine()
    ui.pushFont(ui.Font.Huge)
    ui.text("Rally State:" .. messages)
    ui.popFont()
end

local function tableToVec3(table)
    return vec3(table.X, table.Y, table.Z)
end

local function normalize(v)
    local length = math.sqrt(v.x * v.x + v.y * v.y + v.z * v.z)
    return vec3(v.x / length, v.y / length, v.z / length)
end

local function cross(a, b)
    local x = a.y * b.z - a.z * b.y
    local y = a.z * b.x - a.x * b.z
    local z = a.x * b.y - a.y * b.x
    return vec3(x, y, z)
end

local function drawBox(startingBox, color)
    local position = startingBox.position
    local normal = startingBox.normal
    local width = startingBox.width
    local depth = startingBox.depth
    local height = startingBox.height

    -- Find an arbitrary vector perpendicular to the normal
    local randomVec = vec3(0, 1, 0) -- We know Y is up, so we can use it as the random vector
    local right = normalize(cross(normal, randomVec))
    local forward = normal

    -- Calculate the center points of the four side rectangles
    local halfWidth = width / 2
    local halfDepth = depth / 2
    local p1 = position + right * halfWidth - (forward * halfDepth)
    local p2 = position - right * halfWidth - (forward * halfDepth)
    local p3 = position -- Forward-facing side now matches the startingBox position
    local p4 = position - forward * depth

    -- Render the four sides
    render.rectangle(p1, -right, depth, height, color)  -- Side 1
    render.rectangle(p2, right, depth, height, color)   -- Side 2
    -- render.rectangle(p3, -forward, width, height, color) -- Side 3 (Forward-facing)
    render.rectangle(p4, forward, width, height, color) -- Side 4
end

local box = nil

web.get(timingBaseUrl .. "config", function(err, response)
    local parsed = stringify.parse(response.body)

    local position = tableToVec3(parsed.StartingPosition)
    local forward = tableToVec3(parsed.Forward)
    local normal = vec3():set(forward):sub(position):normalize()

    box = {
        position = position,
        normal = normal,
        width = parsed.Width,
        depth = parsed.Depth,
        height = parsed.Height,
        normalNeg = vec3():set(normal):scale(-1)
    }
end)


function script.draw3D(dt)
    render.setDepthMode(render.DepthMode.LessEqual)
    render.setCullMode(render.CullMode.WireframeAntialised)

    if box ~= nil then
        drawBox(box, boxColor)
    end
end
