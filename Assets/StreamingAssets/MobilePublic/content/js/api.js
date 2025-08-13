const API_BASE = "http://localhost/";

async function makeApiRequest(endpoint, method = "GET", body = null) {
    const url = API_BASE + endpoint;
    const options = {
        method: method,
        headers: {
            "Content-Type": "application/json"
        }
    };
    if (body) {
        options.body = JSON.stringify(body);
    }
    try {
        const response = await fetch(url, options);
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }
        return await response.json();
    } catch (err) {
        console.error("API Request Failed:", err);
        return null;
    }
}

async function signIn(username, password) {
  const response = await makeApiRequest("v1/auth/signin.php", "POST", { username, password });
  if (response && response.success) {
    localStorage.setItem("auth_token", response.token);
    window.location.href = "/home.html";
  } else {
    alert(response?.error || "Login failed");
  }
}

async function checkLogin() {
  const token = localStorage.getItem("auth_token");
  const data = await makeApiRequest("v1/mobile/getinfo.php", "GET", null, token);
  if (data && data.loggedIn) {
    window.location.href = "/home.html";
  } else {
    window.location.href = "/signin.html";
  }
}
