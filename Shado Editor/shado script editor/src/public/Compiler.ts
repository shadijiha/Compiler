import fs from "fs";
import path from "path";
import { exec, spawn } from "child_process";
import { Resource } from "./Resource";
import Settings from "./Settings";

export class Compiler {
	public static compiler_path = "";
	public static core_lib_path = "";
	private static currentProcess?: any;
	private static whenProcessExists: (code: any) => void;
	public static dump_output = false;

	public static async init() {
		this.compiler_path = Settings.get("compilerPath");
		this.core_lib_path = await Compiler.runCode(
			`
		int main()	{
			print("__PATH_TO_CORE__");
		}
		`
		);
	}

	/**
	 * @deprecated
	 */
	public static async runCode(code: string) {
		// Write it to a temp file
		const filename = "VM_temp_.sscript";
		Resource.saveResource({ filename, content: code });

		const data = await Compiler.runFile(
			Resource.toFullPath("/" + filename),
			false
		);

		return data;
	}

	/**
	 * @deprecated
	 */
	public static runFile(filepath: string, deleteFileWhenDone = false) {
		let cmd = `"${this.compiler_path}" --filepath "${filepath}"`;
		cmd += this.dump_output ? " -o" : "";

		return new Promise<string>((resolve, reject) => {
			exec(cmd, (error, stdout, stderr) => {
				if (error) {
					console.log(`error: ${error.message}`);
					reject(error);
				}
				if (stderr) {
					resolve(`<error>${stderr}</error>`);
				}

				// Delete file when done if wanted
				if (deleteFileWhenDone && fs.existsSync(filepath)) {
					fs.unlinkSync(filepath);
				}

				resolve(stdout);
			});
		});
	}

	public static runDynamicCode(code: string, onData: (data: any) => void) {
		// Write it to a temp file
		const filename = "VM_temp_" + Date.now() + ".sscript";
		Resource.saveResource({ filename, content: code });

		return Compiler.runDynamicFile(
			Resource.toFullPath("/" + filename),
			onData,
			true
		);
	}

	/**
	 * Used to run interactive commands
	 */
	public static runDynamicFile(
		filepath: string,
		onData: (data: any) => void,
		deleteFileWhenDone = false
	) {
		const childProcess = spawn(`${this.compiler_path}`, [
			"--filepath",
			filepath,
			this.dump_output ? "--output" : "",
		]);
		this.currentProcess = childProcess;

		childProcess.stdout.setEncoding("utf8");
		childProcess.stdout.on("data", onData);
		childProcess.stderr.on("data", (data) => {
			onData(`<error>${data}</error>`);
		});

		childProcess.on("error", (error) => {
			console.log(error);
		});
		childProcess.on("close", (code) => {
			if (this.whenProcessExists) {
				this.whenProcessExists(<number>code);
			}

			onData(`\nProcess exited with code ${code}`);

			// Delete file when done if wanted
			if (deleteFileWhenDone && fs.existsSync(filepath)) {
				fs.unlinkSync(filepath);
			}

			this.currentProcess = null;
		});
	}

	public static killProcess() {
		this.currentProcess.kill();
	}

	public static cin(data: string) {
		if (this.currentProcess) {
			this.currentProcess.stdin.write(data);
			//this.currentProcess.stdin.end();
		}
	}

	public static onProcessExit(func: (code: any) => void) {
		this.whenProcessExists = func;
	}

	public static getCoreLibPath() {
		return this.core_lib_path;
	}
}
