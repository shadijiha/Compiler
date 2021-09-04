import CodeFormatter from "./CodeFormatter";
import fs from "fs";
import { TabManager } from "./TabManager";
import { isCtrl } from "./index";

/**
 *
 */
export class Editor {
	private editor: HTMLDivElement;
	private undoStack: string[] = [];
	private indentation: number = 0;
	private deleteBuffer: number = 0;

	public constructor(private readonly filepath: string) {
		this.editor = document.createElement("div");
		this.editor.contentEditable = "true";
		this.editor.spellcheck = false;
		this.editor.classList.add("editor");
		this.filepath = filepath;

		document.body.appendChild(this.editor);

		// Prevent editor from adding div when pressing enter
		this.editor.addEventListener("keydown", (e) => {
			if (e.keyCode === 13) {
				// insert 2 br tags (if only one br tag is inserted the cursor won't go to the next line)
				document.execCommand("insertHTML", false, "\n");

				for (let i = 0; i < this.indentation; i++) {
					document.execCommand("insertHTML", false, "    ");
				}

				// prevent the default behaviour of return key pressed
				return false;
			} else if (e.keyCode === 9) {
				// tab key
				e.preventDefault(); // this will prevent us from tabbing out of the editor

				// now insert four non-breaking spaces for the tab key
				var doc = this.editor.ownerDocument.defaultView;
				var sel = doc?.getSelection();
				var range = sel?.getRangeAt(0);

				this.indentation += 1;
				var tabNode = document.createTextNode("    ");
				range?.insertNode(tabNode);

				range?.setStartAfter(tabNode);
				range?.setEndAfter(tabNode);
				sel?.removeAllRanges();
				sel?.addRange(range as Range);
			} else if (e.keyCode === 90 && isCtrl) {
				this.undo();
			} else if (e.keyCode == 8) {
				this.deleteBuffer = Math.max(this.deleteBuffer + 1, 0);
				if (this.deleteBuffer >= 4) {
					this.deleteBuffer = 0;
					this.indentation--;
				}
			} else if (e.key == "<") {
				e.preventDefault();
				document.execCommand("insertHTML", false, "&lt;");
				return false;
			} else if (e.key == ">") {
				e.preventDefault();
				document.execCommand("insertHTML", false, "&gt;");
				return false;
			} else {
				this.pushUndo();
			}
		});

		this.editor.addEventListener("input", (e) => {
			const restore = this.saveCaretPosition(this.editor);
			this.format();
			restore();
		});
	}

	public setContent(content: string) {
		this.editor.innerHTML = `<pre>${content}</pre>`;
		return this;
	}

	public format() {
		const formatter = new CodeFormatter(
			this.filepath,
			this.editor.innerText.replace(/</g, "&lt;").replace(/>/g, "&gt;")
		);

		formatter
			.numbers()
			.strings()
			.keywords()
			.functions()
			.preprocessor()
			.compilerConstants()
			.nativeTypes()
			.costumTypes()
			.comments()
			.jdocs();

		const data = `<pre>${formatter.render()}</pre>`;
		this.editor.innerHTML = data;

		// Now process the links
		const links = document.querySelectorAll(".link, span[classname=link]");
		links.forEach((link) => {
			const file = link.getAttribute("data-file");
			const clazz = link.getAttribute("data-clazz")?.replace(/\_/g, "");

			(<HTMLSpanElement>link).onclick = () => {
				if (isCtrl) {
					TabManager.load(file as string);
					window.location.href = "#__" + clazz + "__";
				}
			};
		});
	}

	public static fromFile(filepath: string): Editor {
		const content = fs.readFileSync(filepath, "utf-8");
		return new Editor(filepath).setContent(content);
	}

	public hide() {
		this.editor.style.display = "none";
	}

	public show() {
		this.editor.style.display = "inline-block";
	}

	public dispose() {
		document.body.removeChild(this.editor);
	}

	public getContent() {
		return this.editor.innerText;
	}

	public getFilepath() {
		return this.filepath;
	}

	private saveCaretPosition(context: any) {
		var selection = window.getSelection() as Selection;
		var range = selection.getRangeAt(0);
		range.setStart(context, 0);
		var len = range.toString().length;

		return () => {
			var pos = this.getTextNodeAtPosition(context, len);
			selection.removeAllRanges();
			var range = new Range();
			range.setStart(pos.node, pos.position);
			selection.addRange(range);
		};
	}

	private getTextNodeAtPosition(root: any, index: any) {
		const NODE_TYPE = NodeFilter.SHOW_TEXT;
		var treeWalker = document.createTreeWalker(
			root,
			NODE_TYPE,
			// @ts-ignore
			function next(elem: any) {
				if (index > elem.textContent.length) {
					index -= elem.textContent.length;
					return NodeFilter.FILTER_REJECT;
				}
				return NodeFilter.FILTER_ACCEPT;
			}
		);
		var c = treeWalker.nextNode();
		return {
			node: c ? c : root,
			position: index,
		};
	}

	private pushUndo() {
		if (this.undoStack.length >= 10) {
			this.undoStack.shift();
		}
		this.undoStack.push(this.editor.innerText);
	}

	private undo() {
		const content = this.undoStack.pop();
		if (content) {
			this.setContent(content);
			this.format();
		}
	}
}
