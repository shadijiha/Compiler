import path from "path";
import { Editor } from "./Editor";
import { Compiler } from "./Compiler";
import { TabManager } from "./TabManager";
import Settings from "./Settings";
import { getScrollbarWidth, hasScrollbarX } from "./util";
const ipcRenderer = require("electron").ipcRenderer;
const dialog = require("electron").remote.dialog;

export let isCtrl = false;

async function main() {
	Settings.init();

	// TODO: You cannot use __dirname because it causes bugs when building
	// Save the app data to %App Data% instead
	await Compiler.init();

	setupConsole();

	// Get events from the main process
	ipcRenderer.on("cmd", function (evt, message) {
		switch (message) {
			case "open":
				openFiles();
				break;
		}
	});

	// Setup save event
	document.onkeyup = function (e) {
		if (e.keyCode == 17) isCtrl = false;
	};
	document.onkeydown = function (e) {
		if (e.keyCode == 17) isCtrl = true;
		if (isCtrl == true) {
			if (e.keyCode == 83) {
				TabManager.saveCurrent();
				return false;
			}

			if (e.key == "O") {
				openFiles();
			}
		}
	};

	// Load work space if it exists
	Settings.loadWorkSpace();
}

window.onbeforeunload = (e) => {
	Settings.saveWorkspace();
	Settings.saveSettings();
};

main();

function setupConsole() {
	const toggler = document.getElementById("console_toggle");
	const container = <HTMLDivElement>(
		document.getElementById("console_container")
	);
	const cons = document.querySelector("#console pre");
	const cinInput = <HTMLInputElement>document.getElementById("stdcin");

	let toggled = false;
	const animateConsoleToggler = () => {
		let width = container?.clientWidth;
		if (hasScrollbarX(container)) {
			width += getScrollbarWidth();
		}

		if (!toggled) {
			toggler!.style.right = width + "px";
		} else toggler!.style.right = "0px";
		toggled = !toggled;
	};

	toggler!.onclick = () => {
		container?.classList.toggle("tranlate-out");
		animateConsoleToggler();
	};

	const newTab = document.getElementById("new");
	newTab!.onclick = () => {
		TabManager.open("untitled", new Editor("untitled"));
	};

	const play = document.getElementById("play");
	const stop = document.getElementById("stop");
	play!.onclick = async () => {
		container?.classList.remove("tranlate-out");

		if (!toggled) animateConsoleToggler();

		const active = TabManager.getActive();

		cons!.innerHTML = `Running ${
			active.name
		} ${new Date().toLocaleString()}\n\n`;

		cinInput?.focus();

		Compiler.onProcessExit((code) => {
			stop!.style.display = "none";
			play!.style.display = "block";
			cinInput?.blur();
		});

		Compiler.runDynamicCode(active.editor.getContent(), (data) => {
			cons!.innerHTML += data.toString();
		});

		stop!.style.display = "block";
		play!.style.display = "none";
	};

	stop!.style.display = "none";
	stop!.onclick = () => {
		Compiler.killProcess();
	};

	cinInput!.onkeypress = (e) => {
		if (e.keyCode == 13) {
			Compiler.cin(cinInput?.value + "\n");
			cinInput!.value = "";
		}
	};
}

export function pathToFilename(filepath: string) {
	return path.basename(filepath);
}

function openFiles() {
	const files = dialog.showOpenDialogSync({
		properties: ["openFile", "multiSelections"],
	});

	for (const file of files || []) {
		console.log(file);
		TabManager.load(file);
	}
	return false;
}
