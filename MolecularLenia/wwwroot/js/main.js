import { initRenderer, renderScene } from './renderer.js';
import * as net from './network.js';

const canvasElement = document.getElementById('simCanvas');
const { ctx, canvas } = initRenderer(canvasElement);

// DOM Elements
const radiusInput = document.getElementById('radiusInput');
const attrInput = document.getElementById('attrInput');
const repInput = document.getElementById('repInput');
const speedInput = document.getElementById('speedInput');
const densityInput = document.getElementById('densityInput');
const reproductionInput = document.getElementById('reproductionInput');
const moleculeTypeSelect = document.getElementById('moleculeTypeSelect');

const playPauseBtn = document.getElementById('playPauseBtn');
const statsVal = document.getElementById('statsVal');
const settingsPanel = document.getElementById('settingsPanel');

// State variables
let isPaused = false;
let isUserInteracting = false;
let frameCount = 0;
let lastFpsTime = performance.now();
let currentFps = 0;
let currentTransform = { scale: 1, offsetX: 0, offsetY: 0 };

// Render & Network Buffer
let latestParticles = [];
let pendingSettings = null;

// Network connection listener (Only updates internal data buffer, does NOT draw)
net.startNetwork((particles, settings) => {
    latestParticles = particles;
    if (settings && !isUserInteracting) {
        pendingSettings = settings;
    }
});

// Decoupled Main Render Loop synced with display refresh rate via requestAnimationFrame
function renderLoop() {
    // Sync UI with pending server settings if user is not actively tweaking inputs
    if (pendingSettings && !isUserInteracting) {
        isPaused = pendingSettings.IsPaused;
        playPauseBtn.innerText = isPaused ? "Play" : "Pause";
        
        radiusInput.value = pendingSettings.InteractionRadius;
        attrInput.value = pendingSettings.AttractionForce;
        repInput.value = pendingSettings.RepulsionForce;
        speedInput.value = pendingSettings.SpeedMultiplier;
        densityInput.value = pendingSettings.TargetDensity;
        reproductionInput.value = pendingSettings.ReproductionRate;
        
        updateDisplayValues();
        pendingSettings = null;
    }

    // Draw scene with the latest available particle data
    const radius = parseFloat(radiusInput.value);
    currentTransform = renderScene(ctx, canvas, latestParticles, radius);

    // Calculate actual display FPS
    frameCount++;
    const now = performance.now();
    if (now - lastFpsTime >= 1000) {
        currentFps = frameCount;
        frameCount = 0;
        lastFpsTime = now;
        if (statsVal) {
            statsVal.innerText = `FPS: ${currentFps} | Particles: ${latestParticles.length}`;
        }
    }

    requestAnimationFrame(renderLoop);
}

// Start render loop
requestAnimationFrame(renderLoop);

function updateDisplayValues() {
    document.getElementById('radiusVal').innerText = radiusInput.value;
    document.getElementById('attrVal').innerText = attrInput.value;
    document.getElementById('repVal').innerText = repInput.value;
    document.getElementById('speedVal').innerText = `${parseFloat(speedInput.value).toFixed(1)}x`;
    document.getElementById('densityVal').innerText = densityInput.value;
    document.getElementById('reproductionVal').innerText = `${parseFloat(reproductionInput.value).toFixed(1)}x`;
}

function collectAndSendSettings() {
    updateDisplayValues();
    net.sendSettings({
        InteractionRadius: parseFloat(radiusInput.value),
        AttractionForce: parseFloat(attrInput.value),
        RepulsionForce: parseFloat(repInput.value),
        IsPaused: isPaused,
        SpeedMultiplier: parseFloat(speedInput.value),
        TargetDensity: parseInt(densityInput.value),
        ReproductionRate: parseFloat(reproductionInput.value)
    });
}

// UI Setup
document.getElementById('togglePanelBtn').addEventListener('click', () => settingsPanel.classList.remove('hidden'));
document.getElementById('closePanelBtn').addEventListener('click', () => settingsPanel.classList.add('hidden'));

playPauseBtn.addEventListener('click', () => {
    isPaused = !isPaused;
    collectAndSendSettings();
});

document.getElementById('clearBtn')?.addEventListener('click', net.clearCanvas);
document.getElementById('resetSettingsBtn')?.addEventListener('click', net.resetSettings);

document.getElementById('spawnBatchBtn')?.addEventListener('click', () => {
    const count = parseInt(document.getElementById('batchCountInput').value) || 50;
    net.spawnBatch(count);
});

canvas.addEventListener('click', (e) => {
    const { scale, offsetX, offsetY } = currentTransform;
    const x = (e.clientX - offsetX) / scale;
    const y = (e.clientY - offsetY) / scale;

    if (x < 0 || x > 1920 || y < 0 || y > 1080) return;

    let selectedType = parseInt(moleculeTypeSelect.value);
    if (selectedType === -1) selectedType = Math.floor(Math.random() * 4);

    net.spawnParticle(x, y, selectedType);
});

// Input event listeners
const inputs = [radiusInput, attrInput, repInput, speedInput, densityInput, reproductionInput];
inputs.forEach(input => {
    input.addEventListener('mousedown', () => { isUserInteracting = true; });
    input.addEventListener('touchstart', () => { isUserInteracting = true; });
    input.addEventListener('change', () => { isUserInteracting = false; collectAndSendSettings(); });
    input.addEventListener('mouseup', () => { isUserInteracting = false; collectAndSendSettings(); });
    input.addEventListener('touchend', () => { isUserInteracting = false; collectAndSendSettings(); });
    input.addEventListener('input', collectAndSendSettings);
});

// Custom Dropdown Integration
const customDropdown = document.getElementById('customMoleculeDropdown');
const nativeSelect = document.getElementById('moleculeTypeSelect');
const selectedView = customDropdown.querySelector('.dropdown-selected');
const menuItems = customDropdown.querySelectorAll('.dropdown-item');

selectedView.addEventListener('click', (e) => {
    e.stopPropagation();
    customDropdown.classList.toggle('open');
});

menuItems.forEach(item => {
    item.addEventListener('click', () => {
        const val = item.getAttribute('data-value');
        nativeSelect.value = val;
        nativeSelect.dispatchEvent(new Event('change'));

        selectedView.innerHTML = item.innerHTML;
        menuItems.forEach(i => i.classList.remove('selected'));
        item.classList.add('selected');

        customDropdown.classList.remove('open');
    });
});

document.addEventListener('click', () => customDropdown.classList.remove('open'));