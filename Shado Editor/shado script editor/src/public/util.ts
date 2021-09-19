/**
 *
 */

export function getScrollbarWidth() {
	// Creating invisible container
	const outer = document.createElement("div");
	outer.style.visibility = "hidden";
	outer.style.overflow = "scroll"; // forcing scrollbar to appear
	// @ts-ignore
	outer.style.msOverflowStyle = "scrollbar"; // needed for WinJS apps
	document.body.appendChild(outer);

	// Creating inner element and placing it in the container
	const inner = document.createElement("div");
	outer.appendChild(inner);

	// Calculating difference between container's full width and the child width
	const scrollbarWidth = outer.offsetWidth - inner.offsetWidth;

	// Removing temporary elements from the DOM
	outer.parentNode?.removeChild(outer);

	return scrollbarWidth;
}

export function hasScrollbarY(div: HTMLElement) {
	return div.scrollHeight > div.clientHeight;
}

export function hasScrollbarX(div: HTMLElement) {
	return div.scrollWidth > div.clientWidth;
}

type MenuItem = { lable: string; onClick: (e: any) => void };
export function openContextMenu(e: any, menuItems: MenuItem[]) {
	const menu = document.getElementById("context_menu");
	menu!.style.display = "block";
	menu!.style.left = e.clientX + "px";
	menu!.style.top = e.clientY + "px";
	menu!.innerHTML = "";
	menu!.onclick = (event: Event) => {
		event.stopPropagation();
	};

	document.body.onclick = () => {
		menu!.style.display = "none";
	};

	for (const el of menuItems) {
		const div = document.createElement("div");
		div.onclick = (e: any) => {
			el.onClick(e);
			menu!.style.display = "none";
		};
		div.innerText = el.lable;
		menu?.appendChild(div);
	}
}
