const bottomBar = document.querySelector("#bottomBar");
const bottomBarButtons = bottomBar.querySelectorAll("button");

bottomBarButtons.forEach((btn, i) => {
    const icon = btn.querySelector("i");
    const name = btn.querySelector("span");
    btn.addEventListener("click", () => {
        if(i === 0)
            loadPage("home");
        else if(i === 1)
            loadPage("games");
        else if(i === 2)
            loadPage("character");
        else if(i === 3)
            loadPage("bux");
        else if(i === 4)
            loadPage("more");
        else
            loadPage("home");
    });
});