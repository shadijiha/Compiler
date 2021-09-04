import fs from "fs";
import { Compiler } from "./Compiler";

type ClassInfo = { file: string; name: string; type?: "native" | "abstract" };

export default class CodeFormatter {
	private static readonly KEYWORDS = [
		"class",
		"native",
		"dynamic",
		"return",
		"if",
		"else",
		"while",
		"for",
		"operator",
		"public",
		"private",
		"C#",
		"implements",
		"interface",
		"new",
		"static",
		"this",
		"true",
		"false",
		"abstract",
	];
	private static readonly NATIVE_TYPES = [
		"int",
		"double",
		"float",
		"bool",
		"char",
		"string",
		"void",
		"object",
	];
	private static readonly PREPROCESSOR = ["#include", "#define"];

	private static readonly CONSTANTS = [
		"__LINE__",
		"__FILE__",
		"__PATH__",
		"__PATH_TO_CORE__",
	];

	private code: string;
	private classList: ClassInfo[];
	private interfaceList: ClassInfo[];
	private functionList: ClassInfo[] = [];
	private macroList: string[] = [];
	private core_lib_path = "";

	public constructor(
		private readonly filename: string,
		private formatted: string
	) {
		this.formatted = formatted;
		this.code = formatted;
		this.classList = [];
		this.interfaceList = [];
		this.filename = filename;

		this.core_lib_path = Compiler.getCoreLibPath();
		this.parseInfo(this.code, filename);
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
		return this.simpleRepalce(CodeFormatter.KEYWORDS, colour);
	}

	public nativeTypes(colour = "#569cd6") {
		return this.simpleRepalce(CodeFormatter.NATIVE_TYPES, colour);
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
		const regex = this.toeRegex(CodeFormatter.PREPROCESSOR);
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
		return this.simpleRepalce(CodeFormatter.CONSTANTS, colour);
	}

	public removeBr() {
		const regex = this.toeRegex(["\\n\\n"]);
		this.formatted = this.formatted.replace(regex, `\n`);
		return this;
	}

	public render() {
		return this.formatted;
	}

	private toeRegex(arr: string[], prefix = "", suffix = "") {
		let buffer = prefix + "(";
		for (const [i, temp] of arr.entries()) {
			buffer += temp;
			if (i != arr.length - 1) {
				buffer += "|";
			}
		}

		return new RegExp(buffer + ")" + suffix, "g");
	}

	private simpleRepalce(array: string[], colour: string): CodeFormatter {
		const regex = this.toeRegex(array, "\\b", "\\b");
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
				this.include(line.replace("#include", "").replace('"', ""));
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
		filepath = filepath.replace("\n", "").replace("\r", "");

		if (filepath != "") {
			let content = fs.readFileSync(filepath, "utf-8");
			this.parseInfo(content, filepath);
		}
	}
}
