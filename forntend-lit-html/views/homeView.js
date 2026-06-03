import { html } from "../utils/library.js";

const homTemplate = () => {
    return html `
    <div class="homePage">home page</div>
    `
}

export function homeView(context){
    context.render(homTemplate());
}