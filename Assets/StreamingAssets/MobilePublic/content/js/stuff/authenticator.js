let loggedin = false;
let authtoken;
let accInfos;
let authSet = false;

window.GetAuthToken = function() {
    if(!authtoken) return null;

    return authtoken;
}

if(window.self === window.top) {
    showLoadingPage();

    let setauthtokencalled = false;
    setTimeout(() => { if(!setauthtokencalled) location.reload(); }, 3500);

    // unity will call that
    window.RefreshAccInfos = () => {
        return new Promise(async resolve => {
            const res = await requestAPI("v1/mobile/getinfo");

            if(res.success) {
                loggedin = true;
                authSet = true;
                accInfos = res.data;
                return resolve(accInfos);
            }

            loggedin = false;
            authSet = true;
            authtoken = null;
            resolve(null);
        });
    }

    window.SetAuthToken = async function(newtoken) {
        setauthtokencalled = true;

        authtoken = newtoken;

        if(await RefreshAccInfos()) {
            loggedin = true;
            authSet = true;

            if(!accInfos.banned)
                loadPage("home");
            else
                loadPage("banned");

            return hideLoadingPage();
        } else {
            loggedin = false;
            authSet = true;

            loadPage("signup");
            return hideLoadingPage();
        }
    }

    window.addEventListener("message", async e => {
        if(e.data.type === "getAuth") {
            let getAuthInterval = setInterval(() => {
                e.source.postMessage({ type: "getAuth", authtoken, accInfos, loggedin }, "*");
                clearInterval(getAuthInterval);
            }, 100);
        } else if(e.data.type === "refreshAccInfos") {
            await RefreshAccInfos();
            e.source.postMessage({ type: "refreshAccInfos", accInfos }, "*");
        }
    });
} else {
    window.RefreshAccInfos = async () => {
        return new Promise((resolve, reject) => {
            window.parent.postMessage({ type: "refreshAccInfos" }, "*");
            window.addEventListener("message", e => {
                if(e.data.type === "refreshAccInfos") {
                    loggedin = true;
                    authSet = true;
                    accInfos = e.data.accInfos;
                    return resolve(e.data.accInfos);
                }
            });

            setTimeout(() => reject(false), 10000);
        });
    }

    window.parent.postMessage({ type: "getAuth" }, "*");
    window.addEventListener("message", e => {
        if(authSet) return;

        if(e.data.type === "getAuth") {
            loggedin = e.data.loggedin;
            authtoken = e.data.authtoken;
            accInfos = e.data.accInfos;
            authSet = true;

            showLoadingPage();
            try {
                if(loggedin && accInfos) {
                    let placeholders;
                    if(!accInfos.banned) {
                        placeholders = {
                            "user.username": accInfos?.username || "?"
                        };
                    } else {
                        placeholders = {
                            "ban.bantype": accInfos?.bantype || "?",
                            "ban.banreason": accInfos?.banreason || "?",
                            "ban.unbantime": accInfos?.unbantime || "?"
                        };
                    }

                    function replacePlaceholders(node) {
                        if(node.nodeType === Node.TEXT_NODE) {
                            let text = node.nodeValue;

                            Object.keys(placeholders).forEach(key => {
                                const placeholder = `{{${key}}}`;
                                if(text.includes(placeholder)) {
                                    text = text.replace(new RegExp(placeholder.replace(/[.*+?^${}()|[\]\\]/g, '\\$&'), "g"), placeholders[key]);
                                }
                            });

                            node.nodeValue = text;
                        } else if(node.nodeType === Node.ELEMENT_NODE) {
                            node.childNodes.forEach(child => replacePlaceholders(child));
                        }
                    }

                    replacePlaceholders(document.body);
                }

                hideLoadingPage();

                if(accInfos.banned && currentPage !== "banned")
                    loadPage("banned");
            } catch (err) {
                console.error("Could not access iframe content:", err);
                hideLoadingPage();
            }
        }
    });
}