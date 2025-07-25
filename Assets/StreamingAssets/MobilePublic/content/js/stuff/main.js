const loadingPage = document.querySelector("#loadingPage");
const pageContent = document.querySelector("#pageContent");

let loadingPageMessages = [
    "You Make The Game.",
    "Welcome to NUGLOX!",
    "Eat, sleep, NUGLOX!",
    "Grass? What's that? Is it a new NUGLOX game?",
    "Rendering pixels... with love.",
    "NUGLOX: Where the fun never logs off.",
    "Your imagination called — it wants to build.",
    "BRB, loading awesome.",
    "Warning: May cause uncontrollable creativity.",
    "Polishing virtual bricks...",
    "Loading fun... please stand by.",
    "Launching you into creativity orbit!",
    "Built different. Built NUGLOX.",
    "This might take a sec... or 0.0035 sec.",
    "NUGLOX isn't just a game. It's a vibe.",
    "Loading your daily dose of chaos.",
    "Generating blocky brilliance...",
    "Crafting worlds, one line of code at a time.",
    "Your world is almost ready. Get hyped!",
    "Glitch-free since... never. Just kidding."
];

// variables that sync thru iframes (all pages) and index and everything so you can use in all js files (very good for arguments for pages)
let syncVariables = {};

if(window.self === window.top) {
      function getSyncVariable(key) {
        return syncVariables[key];
    }

    async function setSyncVariable(key, value) {
        syncVariables[key] = value;
    }

    async function deleteSyncVariable(key) {
        delete syncVariables[key];
    }

    window.addEventListener("message", async e => {
        if(e.data.type === "getSyncVariable" && e.data.key)
            e.source.postMessage({ type: "getSyncVariable", value: await getSyncVariable(e.data.key) }, "*");

        else if(e.data.type === "setSyncVariable" && e.data.key && e.data.value) {
            await setSyncVariable(e.data.key, e.data.value);
            e.source.postMessage({ type: "setSyncVariable" }, "*");

        } else if(e.data.type === "deleteSyncVariable" && e.data.key) {
            await deleteSyncVariable(e.data.key);
            e.source.postMessage({ type: "deleteSyncVariable" }, "*");
        }
    });
} else {
    function getSyncVariable(key) {
        return new Promise(resolve => {
            window.parent.postMessage({ type: "getSyncVariable", key }, "*");

            window.addEventListener("message", e => {
                if(e.data.type === "getSyncVariable")
                    resolve(e.data.value);
            });
        });
    }

    function setSyncVariable(key, value) {
        return new Promise(resolve => {
            window.parent.postMessage({ type: "setSyncVariable", key, value }, "*");

            window.addEventListener("message", e => {
                if(e.data.type === "setSyncVariable")
                    resolve();
            });
        });
    }

    function deleteSyncVariable(key) {
        return new Promise(resolve => {
            window.parent.postMessage({ type: "deleteSyncVariable", key }, "*");

            window.addEventListener("message", e => {
                if(e.data.type === "deleteSyncVariable")
                    resolve();
            });
        });
    }
}

let loadingWanters = 0;
function showLoadingPage() {
    if(window.self === window.top) {
        loadingWanters++;
        if(loadingWanters === 1)
            loadingPage.querySelector("span").innerText = loadingPageMessages[Math.floor(Math.random() * loadingPageMessages.length)];

        loadingPage.style.display = "flex";
    } else
        window.parent.postMessage({ type: "showLoadingPage" }, "*");
}
function hideLoadingPage() {
    if(window.self === window.top) {
        loadingWanters--;

        if(loadingWanters < 1)
            loadingPage.style.display = "none";
    } else
        window.parent.postMessage({ type: "hideLoadingPage" }, "*");
}

if(window.self === window.top) {
    window.addEventListener("message", e => {
        if(e.data.type === "showLoadingPage")
            showLoadingPage();
        else if(e.data.type === "hideLoadingPage")
            hideLoadingPage();
    });
}

let currentPage = null;
function loadPage(name) {
    if(currentPage === name) return;
    if(accInfos && accInfos.banned && currentPage === "banned") return;
    if(accInfos && accInfos.banned)
        name = "banned";

    currentPage = name;

    return new Promise(async (resolve, reject) => {
        if(window.self === window.top) {
            let i = null;

            if(name === "home")
                i = 0;
            else if(name === "games")
                i = 1;
            else if(name === "character")
                i = 2;
            else if(name === "bux")
                i = 3;
            else if(name === "more")
                i = 4;

            bottomBarButtons.forEach(el => el.classList.remove("selected"));
            if(i !== null)
                bottomBarButtons[i].classList.add("selected");

            showLoadingPage();

            if(typeof RefreshAccInfos === "function")
                await RefreshAccInfos();

            pageContent.onload = () => {
                if(
                    name !== "login" &&
                    name !== "signup"
                ) {
                    bottomBar.style.display = null;
                    bottomBar.style.animation = "bottomBarSpawn 1s ease-out";
                    bottomBarButtons.forEach((el, i) => {
                        el.style.animation = `bottomBarButtonSpawn 1s ease-out`;
                        el.style.animationDelay = `${i * 0.25}s`;
                        el.addEventListener("animationstart", () => el.style.opacity = 1);
                    });
                }

                hideLoadingPage();
                resolve();
            };
            pageContent.src = "./content/pageContents/" + name + ".html";
        } else {
            window.parent.postMessage({ type: "loadPage", name }, "*");
            resolve();
        }
    });
}
window.addEventListener("message", e => {
    if(e.data.type === "loadPage" && e.data.name)
        loadPage(e.data.name);
});

function urlToBlob(url) {
    return new Promise(async (resolve, reject) => {
        const req = await fetch(url);
        const res = await req.blob();
        resolve(res);
    });
}

function loadJS(url, type = "text/javascript") {
    return new Promise((resolve, reject) => {
        const el = document.createElement("script");

        el.src = url;
        el.type = type;
        el.defer = true;
        el.async = false;
        el.onload = () => resolve();
        el.onerror = () => reject();

        document.head.appendChild(el);
    });
}

function loadCSS(url) {
    return new Promise((resolve, reject) => {
        const el = document.createElement("link");

        el.href = url;
        el.rel = "stylesheet";
        el.onload = () => resolve();
        el.onerror = () => reject();

        document.head.appendChild(el);
    });
}

async function requestAPI(path, paramsdata = {}) {
    const data = new FormData();
    if(authtoken)
        data.append("authtoken", authtoken);

    for(const key in paramsdata) {
        if(paramsdata.hasOwnProperty(key)) {
            data.append(key, paramsdata[key]);
        }
    }

    const req = await fetch("https://testsite1.nuglox.xyz/" + path, {
        method: "POST",
        body: data
    });
    const res = await req.json();

    return res;
}

if(window.self !== window.top) {
    function confettiSpawn() {
        window.parent.postMessage({ type: "confettiSpawn" }, "*");
    }

    function reloadApp() {
        window.parent.postMessage({ type: "reloadApp" }, "*");
    }

    function loginAs(token) {
        window.parent.postMessage({ type: "loginAs", token }, "*");
    }

    function logOut() {
        window.parent.postMessage({ type: "logOut" }, "*");
    }
} else {
    function reloadApp() {
        location.reload();
    }

    function loginAs(token) {
        window.location = "uniwebview://SetAuthToken?token=" + token;
    }

    async function logOut() {
        await requestAPI("v1/mobile/logout");

        loginAs(null);
    }

    window.addEventListener("message", async e => {
        if(e.data.type === "reloadApp")
            reloadApp();
        else if(e.data.type === "loginAs" && e.data.token)
            loginAs(e.data.token);
        else if(e.data.type === "logOut")
            await logOut();
    });
}

if(window.self !== window.top && !document.querySelector(".party-background")) {
    setSyncVariable("canEnablePartyBG", true);

    setTimeout(() => {
        let frameCount = 0;
        let lastTime = performance.now();

        function checkFPS() {
            frameCount++;
            const now = performance.now();
            const delta = now - lastTime;

            if (delta >= 1000) {
                const fps = (frameCount / delta) * 1000;
                if(fps < 25 && document.querySelector(".party-background")) {
                    setSyncVariable("canEnablePartyBG", false);
                    document.querySelector(".party-background").remove();
                }

                frameCount = 0;
                lastTime = now;
            }

            requestAnimationFrame(checkFPS);
        }

        requestAnimationFrame(checkFPS);
    }, 100);

    if(getSyncVariable("canEnablePartyBG")) {
        const el = document.createElement("div");
        el.className = "party-background";
        document.body.appendChild(el);

        const partybgcolors = ["#ff0055", "#00ffaa", "#ffaa00", "#5500ff", "#00aaff", "#ff00aa"];

        for (let i = 0; i < 25; i++) {
            const circle = document.createElement("div");
            const size = Math.random() * 80 + 40;
            const x = (Math.random() - 0.5) * 500 + "px";
            const y = (Math.random() - 0.5) * 500 + "px";
            const color = partybgcolors[Math.floor(Math.random() * partybgcolors.length)];
            const duration = Math.random() * 20 + 10;

            circle.classList.add("party-circle");
            circle.style.width = size + "px";
            circle.style.height = size + "px";
            circle.style.backgroundColor = color;
            circle.style.top = Math.random() * 100 + "%";
            circle.style.left = Math.random() * 100 + "%";
            circle.style.setProperty('--x', x);
            circle.style.setProperty('--y', y);
            circle.style.animationDuration = duration + "s";

            document.querySelector(".party-background").appendChild(circle);
        }
    }
}