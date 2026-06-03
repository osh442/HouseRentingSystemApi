import { homeView } from "./views/homeView.js";
import { loginView } from "./views/loginView.js";
import { render, page } from "./utils/library.js";
import { navigationView } from "./views/navigationView.js";

const mainElement = document.querySelector("main");

const insertContext = function (context, next) {
    context.render = (content) => render(content, mainElement);
    next();
};

page(insertContext);
page(navigationView);
page("/", homeView);
page("/login", loginView);

page.start();