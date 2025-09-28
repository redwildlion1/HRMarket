namespace HRMarket.Configuration.UniversalExtensions;

public interface IOrderable
{
    public int Order { get; set; }
}

public static class OrderExtensions
{
    public static void CheckOrderedList<T>(this List<T> items) where T : IOrderable
    {
        var ordered = items.OrderBy(i => i.Order).ToList();
        
        // Throw if there are duplicate orders
        if (ordered.Select(i => i.Order).Distinct().Count() != ordered.Count)
            throw new ArgumentException("Duplicate orders found");

        // Throw if there are gaps in the orders
        if (ordered.Where((t, i) => t.Order != i).Any())
            throw new ArgumentException("Gaps in orders found");
        
    }
}