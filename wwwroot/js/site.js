document.addEventListener("DOMContentLoaded", () => {

    // ✅ Use event delegation so it works for dynamically added items too
    document.addEventListener("click", async (event) => {

        const button = event.target.closest(".favorite-btn");
        if (!button) return;

        event.preventDefault();
        event.stopPropagation();

        const listingId = button.dataset.listingId;

        try {
            const response = await fetch(`/Favorites/toggle/${listingId}`, {
                method: "POST",
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            if (!response.ok) {
                alert("You must be logged in to favorite listings.");
                return;
            }

            const result = await response.json();

            button.textContent = result.isFavorited
                ? "★ Favorited"
                : "☆ Favorite";

        } catch (error) {
            console.error("Favorite error:", error);
            alert("Something went wrong.");
        }
    });

});