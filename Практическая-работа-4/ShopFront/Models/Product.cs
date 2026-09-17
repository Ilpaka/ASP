namespace ShopFront.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public decimal? OldPrice { get; set; }      // старая цена — для расчёта скидки
    public int Stock { get; set; }              // 0 => «Нет в наличии»
    public bool IsHit { get; set; }             // «Хит продаж»
    public string Category { get; set; } = "";  // тег: Электроника / Одежда / Книги
    public string Image { get; set; } = "placeholder.svg";

    // Процент скидки; 0, если старой цены нет
    public int DiscountPercent =>
        OldPrice.HasValue && OldPrice.Value > 0 && OldPrice.Value > Price
            ? (int)Math.Round((OldPrice.Value - Price) / OldPrice.Value * 100)
            : 0;

    public bool InStock => Stock > 0;
}

public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Sum => Price * Quantity;
}

public class CartViewModel
{
    public List<CartItem> Items { get; set; } = new();
    public decimal TotalAmount => Items.Sum(i => i.Sum);
    public int TotalCount => Items.Sum(i => i.Quantity);
}

// Сообщение для Bootstrap-alert (_AlertPartial)
public class AlertMessage
{
    public string Type { get; set; } = "info";   // success / warning / danger / info
    public string Text { get; set; } = "";
}
