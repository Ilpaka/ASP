(() => {
    const viewButtons = document.querySelectorAll("[data-dashboard-view]");
    if (!viewButtons.length) return;

    const views = {
        cards: [
            document.getElementById("cardSectionTitle"),
            document.getElementById("cardGrid")
        ],
        list: [
            document.getElementById("compactSectionTitle"),
            document.getElementById("compactList")
        ]
    };

    function setView(name, save = true) {
        const selected = views[name] ? name : "cards";

        Object.entries(views).forEach(([viewName, elements]) => {
            elements.forEach(element => {
                if (element) element.hidden = viewName !== selected;
            });
        });

        viewButtons.forEach(button => {
            const active = button.dataset.dashboardView === selected;
            button.classList.toggle("active", active);
            button.setAttribute("aria-pressed", String(active));
        });

        if (save) {
            try { localStorage.setItem("dashboard-view", selected); } catch { }
        }
    }

    viewButtons.forEach(button => {
        button.addEventListener("click", () => setView(button.dataset.dashboardView));
    });

    let initialView = "cards";
    try { initialView = localStorage.getItem("dashboard-view") || initialView; } catch { }
    setView(initialView, false);
})();
