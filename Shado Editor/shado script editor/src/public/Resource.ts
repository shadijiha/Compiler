import path from "path";
import fs from "fs";
const dialog = require("electron").remote.dialog;

export class Resource {
	public static saveResource(options: {
		relativePath?: string;
		content: string;
		filename: string;
	}) {
		const appData = this.isMac() ? process.env.HOME : process.env.APPDATA;
		if (!appData) {
			dialog.showErrorBox("Error", "Erro locationg App data directory");
			return;
		}
		const resourcePath = path.join(
			appData,
			"/shado_editor/",
			options.relativePath || ""
		);
		if (!fs.existsSync(resourcePath)) {
			fs.mkdirSync(resourcePath);
		}

		// Save the requested file
		fs.writeFileSync(
			path.join(resourcePath, options.filename),
			options.content
		);
	}

	public static appendResource(options: {
		relativePath?: string;
		content: string;
		filename: string;
	}) {
		const appData = this.isMac() ? process.env.HOME : process.env.APPDATA;
		if (!appData) {
			dialog.showErrorBox("Error", "Erro locationg App data directory");
			return;
		}
		const resourceFile = path.join(
			appData,
			"/shado_editor/",
			options.relativePath || "",
			options.filename
		);
		if (!fs.existsSync(resourceFile)) {
			dialog.showErrorBox("Error", "No such file " + resourceFile);
			return;
		}

		// Save the requested file
		fs.appendFileSync(resourceFile, options.content);
	}

	public static loadResource(options: {
		relativePath?: string;
		filename: string;
	}): string {
		const appData = this.isMac() ? process.env.HOME : process.env.APPDATA;
		if (!appData) {
			dialog.showErrorBox("Error", "Erro locationg App data directory");
			return "";
		}
		const resourceFile = path.join(
			appData,
			"/shado_editor/",
			options.relativePath || "",
			options.filename
		);
		if (!fs.existsSync(resourceFile)) {
			dialog.showErrorBox("Error", "No such file " + resourceFile);
			return "";
		}

		// Save the requested file
		return fs.readFileSync(resourceFile, "utf-8");
	}

	public static toFullPath(relativePath: string) {
		const appData = this.isMac() ? process.env.HOME : process.env.APPDATA;
		if (!appData) {
			dialog.showErrorBox("Error", "Erro locationg App data directory");
			return "";
		}
		return path.join(appData, "/shado_editor/", relativePath);
	}

	public static exists(options: { relativePath?: string; filename: string }) {
		const appData = this.isMac() ? process.env.HOME : process.env.APPDATA;
		if (!appData) {
			dialog.showErrorBox("Error", "Erro locationg App data directory");
			return;
		}
		const resourceFile = path.join(
			appData,
			"/shado_editor/",
			options.relativePath || "",
			options.filename
		);
		return fs.existsSync(resourceFile);
	}

	public static isMac() {
		return process.platform === "darwin";
	}
}
