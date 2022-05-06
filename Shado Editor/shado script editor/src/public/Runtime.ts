/**
 *
 */
import { Compiler } from "./Compiler";
import { Editor } from "./Editor";

type RunProps = {
	editor: Editor;
	consoleDOM: Element;
	cinInput: HTMLInputElement;
	stopBtn: HTMLElement | null;
	playBtn: HTMLElement | null;
};

export abstract class Runtime {
	public static run(props: RunProps) {
		switch (props.editor.getExtension()) {
			case "sscript":
				this.runSscript(props);
				break;
			case "js":
				this.runJS(props);
				break;
			default:
		}
	}

	private static runSscript({
		editor,
		consoleDOM,
		stopBtn,
		cinInput,
		playBtn,
	}: RunProps) {
		stopBtn!.onclick = () => {
			Compiler.killProcess();
		};

		cinInput!.onkeypress = (e) => {
			if (e.keyCode == 13) {
				Compiler.cin(cinInput?.value + "\n");
				cinInput!.value = "";
			}
		};

		Compiler.onProcessExit((code) => {
			stopBtn!.style.display = "none";
			playBtn!.style.display = "block";
			cinInput?.blur();
		});

		if (editor.getFilepath() != "untitled") {
			Compiler.runDynamicFile(editor.getFilepath(), (data) => {
				consoleDOM!.innerHTML += data.toString();
			});
		} else {
			Compiler.runDynamicCode(editor.getContent(), (data) => {
				consoleDOM!.innerHTML += data.toString();
			});
		}
	}

	private static async runJS({
		editor,
		consoleDOM,
		stopBtn,
		playBtn,
		cinInput,
	}: RunProps) {
		cinInput?.blur();
		stopBtn!.onclick = () => {
			console.log("Stop requested");
		};

		const oldLog = console.log;
		const oldError = console.error;

		console.log = (data) => {
			(consoleDOM as HTMLElement).innerText += data;
		};
		console.error = (data) => {
			(consoleDOM as HTMLElement).innerText += data;
		};

		function PostExecution() {
			console.log = oldLog;
			console.error = oldError;
		}

		try {
			const AsyncFunction = Object.getPrototypeOf(
				async function () {}
			).constructor;
			const func = new AsyncFunction(editor.getContent());
			await func();
		} catch (e) {
			const err = e as Error;
			(
				consoleDOM as HTMLElement
			).innerHTML += `<pre style='color:red;'>${err.message}</pre>`;
		} finally {
			stopBtn!.style.display = "none";
			playBtn!.style.display = "block";
			PostExecution();
		}
	}
}
