document.addEventListener("DOMContentLoaded", () => {

    // Attach a single click listener to the document (event delegation)
    // This allows dynamically added favorite buttons to work without re-binding events
    document.addEventListener("click", async (event) => {

        // Find the closest element with the class "favorite-btn"
        const button = event.target.closest(".favorite-btn");

        // If the click was not on a favorite button, exit
        if (!button) return;

        // Prevent link navigation and stop event bubbling
        event.preventDefault();
        event.stopPropagation();

        // Get the listing ID from the button's data attribute
        const listingId = button.dataset.listingId;

        try {
            // Send AJAX request to toggle favorite status
            const response = await fetch(`/Favorites/toggle/${listingId}`, {
                method: "POST",
                headers: {
                    // Identifies this request as AJAX
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            // If request failed (e.g., user not logged in)
            if (!response.ok) {
                alert("You must be logged in to favorite listings.");
                return;
            }

            // Parse JSON response from server
            const result = await response.json();

            // Update button text based on favorite state
            button.textContent = result.isFavorited
                ? "★ Favorited"
                : "☆ Favorite";

        } catch (error) {
            // Handle network or server errors
            console.error("Favorite error:", error);
            alert("Something went wrong.");
        }
    });

});