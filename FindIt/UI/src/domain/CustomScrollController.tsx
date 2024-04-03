import { ScrollController, Element, ScrollControllerCallback } from "cs2/ui";

export class CustomScrollController extends ScrollController {
  scrollTo(x: number, y: number) {
    console.log("scrollTo " + x + " " + y);
    super.scrollTo(x, y);
  }
  scrollBy(x: number, y: number) {
    console.log("scrollBy " + x + " " + y);
    super.scrollBy(x, y);
  }
  smoothScrollTo(x: number, y: number) {
    console.log("smoothScrollTo " + x + " " + y);
    super.smoothScrollTo(x, y);
  }
  scrollIntoView(element: Element) {
    console.log("scrollIntoView " + element);
    super.scrollIntoView(element);
  }
}
