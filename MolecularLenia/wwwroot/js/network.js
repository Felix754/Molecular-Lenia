const connection = new signalR.HubConnectionBuilder()
    .withUrl("/simHub")
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .build();


export async function startNetwork(onFrameReceived) {
    connection.on("RenderFrame", (particles, settings) => {
        onFrameReceived(particles, settings);
    });

    try {
        await connection.start();
        console.log("SignalR connected via MessagePack");
    } catch (err) {
        console.error("Connection Error (SignalR):", err);
    }
}

export async function sendSettings(settings) {
    try {
        await connection.invoke("UpdateSettings", settings);
    } catch (err) {
        console.error("Settings Error:", err);
    }
}

export async function spawnParticle(x, y, type) {
    try {
        await connection.invoke("SpawnParticle", x, y, type);
    } catch (err) {
        console.error("Spawn Error:", err);
    }
}

export async function spawnBatch(count) {
    try {
        await connection.invoke("SpawnBatch", count);
    } catch (err) {
        console.error("Batch Error:", err);
    }
}

export async function clearCanvas() {
    try {
        await connection.invoke("Clear");
    } catch (err) {
        console.error("Clear Error:", err);
    }
}

export async function resetSettings() {
    try {
        await connection.invoke("ResetSettings");
    } catch (err) {
        console.error("Reset Error:", err);
    }
}