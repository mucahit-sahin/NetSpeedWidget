const { app, BrowserWindow, ipcMain } = require("electron");
const path = require("path");
const si = require("systeminformation");
const fs = require("fs");

let mainWindow;
let isDetailsWindowOpen = false;
let detailsWindow;
let isSettingsWindowOpen = false;
let settingsWindow;

// Load settings
let settings = {
  theme: {
    mode: "light",
    backgroundColor: "#FFFFFF",
    textColor: "#333333",
  },
  size: {
    width: 190,
    height: 40,
  },
  font: {
    family: "Arial",
    size: 12,
  },
};

const SETTINGS_FILE = path.join(__dirname, "settings.json");

function loadSettings() {
  try {
    if (fs.existsSync(SETTINGS_FILE)) {
      const data = fs.readFileSync(SETTINGS_FILE, "utf8");
      settings = JSON.parse(data);
    }
  } catch (error) {
    console.error("Error loading settings:", error);
  }
}

function createWindow() {
  // Create the widget window
  mainWindow = new BrowserWindow({
    width: settings.size.width,
    height: settings.size.height,
    frame: false,
    transparent: true,
    alwaysOnTop: true,
    resizable: false,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
    },
  });

  mainWindow.loadFile("index.html");

  // Make the window draggable
  mainWindow.setAlwaysOnTop(true, "screen-saver");

  // Apply settings
  mainWindow.webContents.on("did-finish-load", () => {
    mainWindow.webContents.send("apply-settings", settings);
  });
}

function createDetailsWindow() {
  if (isDetailsWindowOpen) {
    detailsWindow.focus();
    return;
  }

  detailsWindow = new BrowserWindow({
    width: 400,
    height: 500,
    frame: false,
    resizable: false,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
    },
  });

  detailsWindow.loadFile("details.html");
  isDetailsWindowOpen = true;

  detailsWindow.on("closed", () => {
    isDetailsWindowOpen = false;
    detailsWindow = null;
  });
}

function createSettingsWindow() {
  if (isSettingsWindowOpen) {
    settingsWindow.focus();
    return;
  }

  settingsWindow = new BrowserWindow({
    width: 400,
    height: 600,
    frame: false,
    resizable: false,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
    },
  });

  settingsWindow.loadFile("settings.html");
  isSettingsWindowOpen = true;

  settingsWindow.on("closed", () => {
    isSettingsWindowOpen = false;
    settingsWindow = null;
  });
}

// Format speed to appropriate unit
function formatSpeed(speed) {
  if (speed >= 1024 * 1024) {
    return `${(speed / (1024 * 1024)).toFixed(1)}G`;
  } else if (speed >= 1024) {
    return `${(speed / 1024).toFixed(1)}M`;
  }
  return `${speed.toFixed(0)}K`;
}

// Network speed monitoring
let lastDownload = 0;
let lastUpload = 0;

async function getNetworkSpeed() {
  try {
    const networkStats = await si.networkStats();
    const stats = networkStats[0]; // Primary network interface

    const downloadSpeed = stats.rx_sec / 1024; // Convert to KB/s
    const uploadSpeed = stats.tx_sec / 1024; // Convert to KB/s

    lastDownload = downloadSpeed;
    lastUpload = uploadSpeed;

    if (mainWindow) {
      mainWindow.webContents.send("network-speed", {
        download: formatSpeed(downloadSpeed),
        upload: formatSpeed(uploadSpeed),
      });
    }
  } catch (error) {
    console.error("Error getting network speed:", error);
  }
}

app.whenReady().then(() => {
  // Load settings before creating window
  loadSettings();
  createWindow();
  setInterval(getNetworkSpeed, 1000);

  // Listen for show-details event
  ipcMain.on("show-details", () => {
    createDetailsWindow();
  });

  // Listen for show-settings event
  ipcMain.on("show-settings", () => {
    createSettingsWindow();
  });

  // Listen for close-details event
  ipcMain.on("close-details", () => {
    if (detailsWindow) {
      detailsWindow.close();
    }
  });

  // Listen for close-settings event
  ipcMain.on("close-settings", () => {
    if (settingsWindow) {
      settingsWindow.close();
    }
  });

  // Listen for settings-updated event
  ipcMain.on("settings-updated", (event, newSettings) => {
    settings = newSettings;

    // Update main window size
    if (mainWindow) {
      mainWindow.setSize(settings.size.width, settings.size.height);
      mainWindow.webContents.send("apply-settings", settings);
    }
  });

  app.on("activate", () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

app.on("window-all-closed", () => {
  if (process.platform !== "darwin") {
    app.quit();
  }
});
