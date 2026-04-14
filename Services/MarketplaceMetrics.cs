using Prometheus;

public static class MarketplaceMetrics
{
    public static readonly Counter ListingsCreated = Metrics.CreateCounter(
        "etsu_marketplace_listings_created_total",
        "Total number of listings created.");

    public static readonly Counter BugReportsSubmitted = Metrics.CreateCounter(
        "etsu_marketplace_bug_reports_submitted_total",
        "Total number of bug reports submitted.");

    public static readonly Histogram ListingCreateDuration = Metrics.CreateHistogram(
        "etsu_marketplace_listing_create_duration_seconds",
        "Time spent creating a listing.");
}