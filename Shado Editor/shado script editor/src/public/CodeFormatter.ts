import fs from "fs";
import { Compiler } from "./Compiler";
import path from "path";
import { Resource } from "./Resource";

type ClassInfo = { file: string; name: string; type?: "native" | "abstract" };

export default class CodeFormatter {
	private KEYWORDS = [
		"return",
		"if",
		"else",
		"while",
		"for",
		"operator",
		"C#",
		"implements",
		"new",
		"this",
		"true",
		"false",
		"abstract",
	];

	private MODIFIERS = [
		"readonly",
		"static",
		"interface",
		"public",
		"private",
		"class",
		"native",
	];

	private NATIVE_TYPES = [
		"int",
		"double",
		"float",
		"bool",
		"char",
		"string",
		"void",
		"object",
		"dynamic",
	];
	private PREPROCESSOR = ["#include", "#define"];

	private CONSTANTS = ["__LINE__", "__FILE__", "__PATH__", "__PATH_TO_CORE__"];

	private code: string;
	private classList: ClassInfo[];
	private interfaceList: ClassInfo[];
	private functionList: ClassInfo[] = [];
	private macroList: string[] = [];
	private core_lib_path = "";

	private constructor(
		private readonly filename: string,
		private formatted: string
	) {
		this.formatted = formatted;
		this.code = formatted;
		this.classList = [];
		this.interfaceList = [];
		this.filename = filename;

		this.core_lib_path = Compiler.getCoreLibPath();

		if (filename.endsWith(".sscript")) this.parseInfo(this.code, filename);
	}

	public static loadWithTypesFile(
		_filename: string,
		content: string,
		extension: string
	) {
		const formatter = new CodeFormatter(_filename, content);
		const filename = "editor." + extension + ".types";
		let filecontent: string | null = null;

		// This file should be location in the %AppData%
		if (Resource.exists({ filename })) {
			filecontent = Resource.loadResource({ filename });
		}

		// If not then search in the compile Core.sscript path
		if (
			fs.existsSync(path.join(Compiler.getCoreLibPath(), filename)) &&
			!filecontent
		) {
			filecontent = fs.readFileSync(
				path.join(Compiler.getCoreLibPath(), filename),
				"utf-8"
			);
		}

		// Otherwise just default to the already defined keywords and constants
		if (!filecontent) return formatter;

		// If the file exists then parse it
		// and clear the default CodeFormatter keywords
		clearArray(formatter.KEYWORDS);
		clearArray(formatter.MODIFIERS);
		clearArray(formatter.NATIVE_TYPES);
		clearArray(formatter.CONSTANTS);

		const lines = filecontent.split("\n");
		for (const line of lines) {
			const tokens = line.trim().split(/\s+/);
			switch (tokens[0]) {
				case "keyword":
					formatter.KEYWORDS.push(tokens[1]);
					break;
				case "modifier":
					formatter.MODIFIERS.push(tokens[1]);
					break;
				case "constant":
					formatter.CONSTANTS.push(tokens[1]);
					break;
				case "native_type":
					formatter.NATIVE_TYPES.push(tokens[1]);
					break;
			}
		}
		return formatter;
	}

	public comments(colour = "#6a9955") {
		this.regexReplace(/\/\/.*/g, colour);
		return this.regexReplace(/\/\*[\s\S]*?\*\//g, colour);
	}

	public keywords(colour = "#c586c0") {
		this.formatted = this.formatted.replace(
			/\bC#/g,
			`<span style="color: ${colour};">$&</span>`
		);
		this.simpleRepalce(this.MODIFIERS, "#569cd6");
		return this.simpleRepalce(this.KEYWORDS, colour);
	}

	public nativeTypes(colour = "#569cd6") {
		return this.simpleRepalce(this.NATIVE_TYPES, colour);
	}

	public costumTypes(colour = "#4ec9b0", colourAbstract = "#208dc8") {
		if (this.classList.length > 0) {
			for (const info of this.classList) {
				this.formatted = this.formatted.replace(
					new RegExp(`\\b${info.name}\\b`, "g"),
					`<span class="link" style="color: ${
						info.type == "abstract" ? colourAbstract : colour
					};" data-file="${info.file}" data-clazz="__${info.name}__">$&</span>`
				);
			}
		}

		if (this.interfaceList.length > 0) {
			for (const info of this.interfaceList) {
				this.formatted = this.formatted.replace(
					new RegExp(`\\b${info.name}\\b`, "g"),
					`<span class="link" style="color: pink;" data-file="${info.file}" data-clazz="__${info.name}__">$&</span>`
				);
			}
		}

		return this;
	}

	public functions(colour = "#dcdcaa", nativeColour = "#f28779") {
		if (this.functionList.length > 0) {
			for (const info of this.functionList) {
				this.formatted = this.formatted.replace(
					new RegExp(`\\b${info.name}(?=\\()`, "g"),
					`<span class='link' style='color: ${
						info.type == "native" ? nativeColour : colour
					};' data-file='${info.file}' data-clazz='__${info.name}__'>$&</span>`
				);
			}
		}
		return this;
		//return this.regexReplace(/\b\w+(?=\()/g, colour);
	}

	public preprocessor(colour = "#ff9a41") {
		const regex = this.toeRegex(this.PREPROCESSOR);
		if (regex)
			this.formatted = this.formatted.replace(
				regex,
				`<span style="color: ${colour};">$&</span>`
			);

		if (this.macroList.length > 0) this.simpleRepalce(this.macroList, "purple");

		return this;
	}

	public strings(colour = "#bae67e") {
		return this.regexReplace(/(["])(?:(?=(\\?))\2.)*?\1/g, colour);
	}

	public jdocs(colour = "#569cd6") {
		return this.regexReplace(
			/@\w+/g,
			colour + "; font-weight: bold; font-style: italic"
		);
	}

	public numbers(colour = "#a7cea8") {
		return this.regexReplace(/\b(\d+)\b/g, colour);
	}

	public compilerConstants(colour = "#6a8ccc") {
		return this.simpleRepalce(this.CONSTANTS, colour);
	}

	public removeBr() {
		const regex = this.toeRegex(["\\n\\n"]);
		if (regex) this.formatted = this.formatted.replace(regex, `\n`);
		return this;
	}

	public render() {
		return this.formatted;
	}

	private toeRegex(arr: string[], prefix = "", suffix = "") {
		if (arr.length == 0) return null;
		let buffer = prefix + "(";
		for (const [i, temp] of arr.entries()) {
			buffer += temp;
			if (i != arr.length - 1) {
				buffer += "|";
			}
		}

		return new RegExp(buffer + ")" + suffix, "g");
	}

	public simpleRepalce(array: string[], colour: string): CodeFormatter {
		const regex = this.toeRegex(array, "\\b", "\\b");
		if (regex)
			this.formatted = this.formatted.replace(
				regex,
				`<span style="color: ${colour};">$&</span>`
			);
		return this;
	}

	private regexReplace(regex: RegExp, colour: string): CodeFormatter {
		this.formatted = this.formatted.replace(
			regex,
			`<span style='color: ${colour};'>$&</span>`
		);
		return this;
	}

	private parseInfo(code: string, filepath: string) {
		// Register all classes
		const lines = code.split("\n");

		for (const line of lines) {
			// If it is an include line, then parse the info of that file too
			if (line.trim().startsWith("#include")) {
				const filename = this.include(
					line.replace("#include", "").replace('"', "")
				);
				this.formatted = this.formatted.replace(
					line,
					`<span className='link' data-file='${filename}'>${line}</span>`
				);
			}

			const classMatch = line.match(/(?<=\bclass.*)\w+/g);
			if (classMatch) {
				this.classList.push({
					file: filepath,
					name: classMatch[0],
					type: line.includes("abstract") ? "abstract" : undefined,
				});
				this.formatted = this.formatted.replace(
					`class ${classMatch[0]}`,
					`<a name='__${classMatch[0]}__'>$&</a>`
				);
			}

			const interfaceMatch = line.match(/(?<=\binterface.*)\w+/g);
			if (interfaceMatch) {
				this.interfaceList.push({ file: filepath, name: interfaceMatch[0] });
				this.formatted = this.formatted.replace(
					`interface ${interfaceMatch[0]}`,
					`<a name='__${interfaceMatch[0]}__'>$&</a>`
				);
			}

			const macroMatch = line.match(/(?<=#define\s+)\w+/g);
			if (macroMatch) {
				this.macroList.push(macroMatch[0]);
			}

			const functionMatch = line.match(/\w+\s+\w+\(/g);
			if (functionMatch) {
				const funcName = line.match(/(?<=\w+\s+)\w+(?=\()/g);
				this.functionList.push({
					file: filepath,
					name: funcName![0],
					type: line.includes("native") ? "native" : undefined,
				});
				this.formatted = this.formatted.replace(
					`${functionMatch[0]}`,
					`<a name='__${funcName![0]}__'>$&</a>`
				);
			}
		}
	}

	private include(filename: string) {
		const lastIndex = filename.lastIndexOf("//");
		let filepath = "";

		if (lastIndex == -1) {
			filepath = filename.trim().replace(/"/g, "");
		} else {
			filepath = filename.substring(0, lastIndex).trim().replace(/"/g, "");
		}
		filepath = filepath.replace(/__PATH_TO_CORE__/g, this.core_lib_path);
		filepath = filepath.replace(/__PATH__/g, path.dirname(this.filename));
		filepath = filepath.replace("\n", "").replace("\r", "");

		if (filepath != "") {
			let content = fs.readFileSync(
				this.searchRelativePathes(filepath),
				"utf-8"
			);
			this.parseInfo(content, filepath);
		}

		return filepath;
	}

	public searchRelativePathes(filename: string): string {
		const currentpath = path.dirname(this.filename);
		if (fs.existsSync(path.join(currentpath, filename))) {
			return path.join(currentpath, filename);
		} else if (fs.existsSync(path.join(Compiler.getCoreLibPath(), filename))) {
			return path.join(Compiler.getCoreLibPath(), filename);
		} else {
			return filename;
		}
	}
}

function clearArray(array: any[]) {
	while (array.length) {
		array.pop();
	}
}
