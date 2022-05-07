/**
 *
 */

import { Resource } from "./Resource";

export class Extensions {
	private static readonly extensionsCache: string[] = [];

	public static async syncWithCloud() {
		// Login
		await request("auth/login", "POST", {
			email: "shado_editor_extensions@shado.com",
			password: "shadi1234",
		});

		// Get extensions in cloud
		const files = (await request("directory/list/", "GET")).data;
		for (const file of files) {
			if (file.extension != ".types") continue;

			const tokens = file.name.split(".");
			const languages = tokens[tokens.length - 2];

			this.extensionsCache.push(languages);
		}
		// Local files
		const insatalled = await this.installedExtensions();

		// Sync the content of local extensios with cloud
		for (const lang in insatalled) {
			const content = await requestToText(`file/editor.${lang}.types`, "GET");

			Resource.saveResource({
				filename: insatalled[lang],
				content,
			});
		}
	}

	public static async installedExtensions() {
		const result: Record<string, string> = {};
		const files = await Resource.listFiles();

		for (const file of files) {
			const extension = Resource.getExtensionOf(file);
			if (extension == ".types") {
				const tokens = file.split(".");
				const lang = tokens[tokens.length - 2];
				result[lang] = file;
			}
		}

		return result;
	}

	public static async isInstalled(extName: string) {
		return (await this.installedExtensions())[extName] != undefined;
	}

	public static async isOnClouud(extName: string) {
		return this.extensionsCache.find((e) => e == extName) != undefined;
	}

	public static async install(languageName: string) {
		if (this.extensionsCache.find((e) => e == languageName) == undefined)
			throw new Error(
				"Could not install " +
					languageName +
					" because extension does not exist on cloud"
			);

		const content = await requestToText(
			`file/editor.${languageName}.types`,
			"GET"
		);
		Resource.saveResource({
			filename: `editor.${languageName}.types`,
			content,
		});
	}
}

async function request(
	relativeUrl: string,
	method: "GET" | "POST" | "PATCH" | "PUT" | "DELETE",
	body: Record<string, string> | null = null
) {
	let options: any = {
		credentials: "include",
		method,
		headers: {
			"Content-Type": "application/json",
		},
	};

	if (body != null) {
		options = { ...options, body: JSON.stringify(body) };
	}

	const response = await fetch(
		"http://shadijiha.ddns.net/apinest/" + relativeUrl,
		options as any
	);
	return await response.json();
}

async function requestToText(
	relativeUrl: string,
	method: "GET" | "POST" | "PATCH" | "PUT" | "DELETE",
	body: Record<string, string> | null = null
) {
	let options: any = {
		credentials: "include",
		method,
		headers: {
			"Content-Type": "application/json",
		},
	};

	if (body != null) {
		options = { ...options, body: JSON.stringify(body) };
	}

	const response = await fetch(
		"http://shadijiha.ddns.net/apinest/" + relativeUrl,
		options as any
	);
	return await response.text();
}
