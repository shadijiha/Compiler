import { app, BrowserWindow, ipcMain, Menu } from "electron";
import { is } from "electron-util";
function createWindow() {
	const win = new BrowserWindow({
		width: 1280,
		height: 720,
		webPreferences: {
			nodeIntegration: true,
			contextIsolation: false,
			enableRemoteModule: true,
			devTools: is.development,
		},
	});

	win.loadFile("index.html");
	const menu = Menu.buildFromTemplate(createMenuBar(win) as any);
	Menu.setApplicationMenu(menu);
	return win;
}

app.whenReady().then(() => {
	const window = createWindow();

	app.on("activate", function () {
		if (BrowserWindow.getAllWindows().length === 0) createWindow();
	});
});

app.on("window-all-closed", function () {
	if (process.platform !== "darwin") app.quit();
});

function createMenuBar(window: BrowserWindow) {
	const isMac = process.platform == "darwin";
	const send = (cmd: string) => window.webContents.send("cmd", cmd);
	const template = [
		// { role: 'appMenu' }
		...(isMac
			? [
					{
						label: app.name,
						submenu: [
							{ role: "about" },
							{ type: "separator" },
							{ role: "services" },
							{ type: "separator" },
							{ role: "hide" },
							{ role: "hideOthers" },
							{ role: "unhide" },
							{ type: "separator" },
							{ role: "quit" },
						],
					},
			  ]
			: []),
		// { role: 'fileMenu' }
		{
			label: "File",
			submenu: [
				{
					label: "open",
					accelerator: "CommandOrControl+O",
					click() {
						send("open");
					},
				},
				{ type: "separator" },
				isMac ? { role: "close" } : { role: "quit" },
			],
		},
		// { role: 'editMenu' }
		{
			label: "Edit",
			submenu: [
				{ role: "undo" },
				{ role: "redo" },
				{ type: "separator" },
				{ role: "cut" },
				{ role: "copy" },
				{ role: "paste" },
				...(isMac
					? [
							{ role: "pasteAndMatchStyle" },
							{ role: "delete" },
							{ role: "selectAll" },
							{ type: "separator" },
							{
								label: "Speech",
								submenu: [{ role: "startSpeaking" }, { role: "stopSpeaking" }],
							},
					  ]
					: [{ role: "delete" }, { type: "separator" }, { role: "selectAll" }]),
			],
		},
		// { role: 'viewMenu' }
		{
			label: "View",
			submenu: [
				{ role: "reload" },
				{ role: "forceReload" },
				{ role: "toggleDevTools" },
				{ type: "separator" },
				{ role: "resetZoom" },
				{ role: "zoomIn" },
				{ role: "zoomOut" },
				{ type: "separator" },
				{ role: "togglefullscreen" },
			],
		},
		// { role: 'windowMenu' }
		{
			label: "Window",
			submenu: [
				{ role: "minimize" },
				{ role: "zoom" },
				...(isMac
					? [
							{ type: "separator" },
							{ role: "front" },
							{ type: "separator" },
							{ role: "window" },
					  ]
					: [{ role: "close" }]),
			],
		},
		{
			role: "help",
			submenu: [
				{
					label: "Learn More",
					click: async () => {
						const { shell } = require("electron");
						await shell.openExternal("https://electronjs.org");
					},
				},
			],
		},
	];
	return template;
}
