import { html } from "../utils/library.js";

const loginTemplate = (submitAction) => {
    return html`
    <section class="loginPage">
        <h2>Login</h2>
        <form @submit=${submitAction}>
            <label>
                Email
                <input type="email" name="email" />
            </label>
            <label>
                Password
                <input type="password" name="password" />
            </label>
            <button type="submit">Login</button>
        </form>
    </section>
    `;
};

export function loginView(context) {
    context.render(loginTemplate(submitAction));

    function submitAction(event) {
        event.preventDefault();
    }
}
