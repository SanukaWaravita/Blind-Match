document.addEventListener("DOMContentLoaded", () => {
    const roleSelector = document.getElementById("roleSelector");
    const sidebarToggle = document.querySelector("[data-sidebar-toggle]");
    const sidebar = document.getElementById("appSidebar");

    const syncRoleSections = () => {
        const isSupervisor = roleSelector && roleSelector.value === "Supervisor";
        document.querySelectorAll(".student-only").forEach(x => x.classList.toggle("d-none", isSupervisor));
        document.querySelectorAll(".supervisor-only").forEach(x => x.classList.toggle("d-none", !isSupervisor));
    };

    if (roleSelector) {
        roleSelector.addEventListener("change", syncRoleSections);
        syncRoleSections();
    }

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener("click", () => {
            sidebar.classList.toggle("is-open");
        });

        document.addEventListener("click", event => {
            if (!sidebar.classList.contains("is-open")) {
                return;
            }

            const clickedInsideSidebar = sidebar.contains(event.target);
            const clickedToggle = sidebarToggle.contains(event.target);
            if (!clickedInsideSidebar && !clickedToggle) {
                sidebar.classList.remove("is-open");
            }
        });
    }

    document.querySelectorAll(".confirm-action").forEach(button => {
        button.addEventListener("click", event => {
            const message = button.getAttribute("data-confirm-message") || "Are you sure?";
            if (!window.confirm(message)) {
                event.preventDefault();
            }
        });
    });
});
