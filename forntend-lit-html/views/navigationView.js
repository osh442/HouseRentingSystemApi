import {html,render} from "../utils/library.js"

const headerElement = document.querySelector('header');
const navigationTemplate = () => {
    return html`
    <nav>
        <ul>
            <li><a href="/">Home</a></li>
            <li><a href="/register">Register</a></li>
            <li><a href="/login">Login</a></li>
        </ul>
    </nav>
    `
}

export function navigationView(context, next){
    render(navigationTemplate(), headerElement);
    next();
}