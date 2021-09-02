import { app, BrowserWindow, Menu } from "electron";

function createWindow() {
	const win = new BrowserWindow({
		width: 1280,
		height: 720,
		webPreferences: {
			nodeIntegration: true,
			contextIsolation: false,
			enableRemoteModule: true,
		},
	});

	win.loadFile("index.html");

	return win;
}

app.whenReady().then(() => {
	const window = createWindow();

	app.on("activate", function () {
		if (BrowserWindow.getAllWindows().length === 0) createWindow();
	});

	window.webContents.send("cmd", { SAVED: "File Saved" });
});

app.on("window-all-closed", function () {
	if (process.platform !== "darwin") app.quit();
});
