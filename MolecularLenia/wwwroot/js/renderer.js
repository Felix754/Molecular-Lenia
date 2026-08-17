const VIRTUAL_WIDTH = 1920;
const VIRTUAL_HEIGHT = 1080;

const particleColors = {
    "AlphaPredator": "#e74c3c",
    "BetaCollector": "#2ecc71",
    "GammaCatalyst": "#9b59b6",
    "CellFuel":      "#3498db",
    "Pathogen":      "#f1c40f"
};

const particleStamps = {};
let lastStampR = -1;
let lastStampScale = -1;

export function initRenderer(canvasElement) {
    const ctx = canvasElement.getContext('2d', { alpha: false });
    ctx.imageSmoothingEnabled = false;

    function resize() {
        canvasElement.width = window.innerWidth || document.documentElement.clientWidth;
        canvasElement.height = window.innerHeight || document.documentElement.clientHeight;
        ctx.imageSmoothingEnabled = false;
    }
    
    window.addEventListener('resize', resize);
    resize();

    return { ctx, canvas: canvasElement };
}


 // Pre-renders particle shapes onto offscreen canvases
function generateStamps(r, scale) {
    const auraRadius = (r / 2) * scale;
    const coreRadius = Math.max(3, 5 * scale);
    const size = Math.ceil(auraRadius * 2 + 4);
    const center = size / 2;

    for (const [type, color] of Object.entries(particleColors)) {
        const offCanvas = document.createElement('canvas');
        offCanvas.width = size;
        offCanvas.height = size;
        const offCtx = offCanvas.getContext('2d');

        // Draw outer aura
        offCtx.beginPath();
        offCtx.arc(center, center, auraRadius, 0, Math.PI * 2);
        offCtx.fillStyle = "rgba(137, 180, 250, 0.015)";
        offCtx.fill();

        // Draw particle core
        offCtx.beginPath();
        offCtx.arc(center, center, coreRadius, 0, Math.PI * 2);
        offCtx.fillStyle = color;
        offCtx.fill();

        particleStamps[type] = { canvas: offCanvas, offset: center };
    }
}

function getStamp(type, r, scale) {
    if (r !== lastStampR || scale !== lastStampScale) {
        lastStampR = r;
        lastStampScale = scale;
        generateStamps(r, scale);
    }
    return particleStamps[type] || particleStamps["AlphaPredator"];
}


// Renders the full scene frame onto the canvas
export function renderScene(ctx, canvas, particles, radius) {
    const scale = Math.min(canvas.width / VIRTUAL_WIDTH, canvas.height / VIRTUAL_HEIGHT);
    const offsetX = (canvas.width - VIRTUAL_WIDTH * scale) / 2;
    const offsetY = (canvas.height - VIRTUAL_HEIGHT * scale) / 2;


    ctx.fillStyle = "#09090d";
    ctx.fillRect(0, 0, canvas.width, canvas.height);


    ctx.fillStyle = "rgba(17, 17, 27, 0.85)";
    ctx.fillRect(offsetX, offsetY, VIRTUAL_WIDTH * scale, VIRTUAL_HEIGHT * scale);

    ctx.strokeStyle = "rgba(137, 180, 250, 0.3)";
    ctx.lineWidth = 2;
    ctx.setLineDash([8, 8]);
    ctx.strokeRect(offsetX, offsetY, VIRTUAL_WIDTH * scale, VIRTUAL_HEIGHT * scale);
    ctx.setLineDash([]);


    for (let i = 0; i < particles.length; i++) {
        const p = particles[i];
        const screenX = offsetX + p.X * scale;
        const screenY = offsetY + p.Y * scale;
        const stamp = getStamp(p.Type, radius, scale);

        ctx.drawImage(stamp.canvas, screenX - stamp.offset, screenY - stamp.offset);
    }

    return { scale, offsetX, offsetY };
}