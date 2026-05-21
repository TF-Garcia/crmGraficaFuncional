using Microsoft.EntityFrameworkCore;
using PrintFlowApi.Model;

namespace PrintFlowApi.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PrintFlowDbContext>();
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new User
                {
                    Name = "Admin PrintFlow",
                    Email = "admin@printflowpro.com.br",
                    Phone = "(11) 98888-2026",
                    Role = UserRole.Admin,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123456")
                },
                new User
                {
                    Name = "Studio Bella",
                    Email = "contato@studiobella.com.br",
                    Phone = "(11) 98888-4211",
                    Document = "12.345.678/0001-90",
                    Address = "Av. Primavera, 123",
                    Role = UserRole.Client,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cliente@123456")
                });
        }

        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(
                Product("cartoes-visita", "Cartoes de visita", "Papelaria corporativa", "Cartoes profissionais com opcoes de laminacao, papel premium e impressao frente e verso.", "https://images.unsplash.com/photo-1589831377283-33cb1cc6bd5d?auto=format&fit=crop&w=900&q=80", 78, 3, false,
                    [100, 250, 500, 1000],
                    [("9x5 cm", 0, 0), ("8,8x4,8 cm", 8, 0)],
                    [("Couche 250g", 0, 0), ("Couche 300g", 18, 1), ("Reciclato 240g", 26, 1)],
                    [("Frente colorida", 0, 0), ("Frente e verso colorido", 32, 0)],
                    [("Refile", 0, 0), ("Laminacao fosca", 42, 1), ("Cantos arredondados", 35, 1)]),
                Product("panfletos", "Panfletos", "Divulgacao", "Panfletos para campanhas locais, eventos, ofertas e comunicacao promocional.", "https://images.unsplash.com/photo-1586953208448-b95a79798f07?auto=format&fit=crop&w=900&q=80", 96, 4, true,
                    [250, 500, 1000, 2500],
                    [("A6", 0, 0), ("A5", 36, 0), ("A4", 88, 1)],
                    [("Couche 90g", 0, 0), ("Couche 115g", 22, 0), ("Couche 150g", 48, 1)],
                    [("Frente colorida", 0, 0), ("Frente e verso colorido", 54, 0)],
                    [("Sem acabamento", 0, 0), ("Dobra", 38, 1), ("Corte especial", 72, 2)]),
                Product("banners", "Banners e lonas", "Comunicacao visual", "Banners, faixas e lonas com ilhos, bastao ou acabamento reforcado.", "https://images.unsplash.com/photo-1581092160607-ee22621dd758?auto=format&fit=crop&w=900&q=80", 115, 3, false,
                    [1, 2, 5, 10],
                    [("60x90 cm", 0, 0), ("80x120 cm", 48, 0), ("100x150 cm", 96, 1)],
                    [("Lona brilho 280g", 0, 0), ("Lona fosca 340g", 35, 1), ("Lona frontlight", 58, 1)],
                    [("Frente colorida", 0, 0)],
                    [("Ilhos", 18, 0), ("Bastao e cordao", 28, 1), ("Reforco de borda", 34, 1)]),
                Product("adesivos", "Adesivos", "Rotulos e etiquetas", "Adesivos em vinil, etiquetas, rotulos e recortes para embalagens e vitrines.", "https://images.unsplash.com/photo-1605648916319-cf082f7524a9?auto=format&fit=crop&w=900&q=80", 64, 3, false,
                    [100, 250, 500, 1000],
                    [("5x5 cm", 0, 0), ("8x8 cm", 24, 0), ("10x15 cm", 58, 1)],
                    [("Vinil branco", 0, 0), ("Vinil transparente", 36, 1), ("BOPP brilho", 48, 1)],
                    [("Frente colorida", 0, 0)],
                    [("Meio corte", 18, 0), ("Laminacao", 36, 1), ("Corte especial", 58, 1)]));
        }

        if (!await db.Products.AnyAsync(product => product.Slug == "produto-teste-mercado-pago"))
        {
            db.Products.Add(Product("produto-teste-mercado-pago", "Produto teste Mercado Pago", "Teste", "Produto de R$ 0,50 para validar pagamentos Pix e cartao em ambiente de teste do Mercado Pago.", "https://images.unsplash.com/photo-1554224155-6726b3ff858f?auto=format&fit=crop&w=900&q=80", 0.50m, 1, true,
                [1],
                [("Padrao", 0, 0)],
                [("Teste", 0, 0)],
                [("Digital", 0, 0)],
                [("Sem acabamento", 0, 0)]));
        }

        if (!await db.InventoryItems.AnyAsync())
        {
            db.InventoryItems.AddRange(
                new InventoryItem { Name = "Papel couche 250g", Category = "Papel", Available = 2400, Unit = "folhas", Minimum = 1000, Supplier = "Papelaria Max", UnitCost = 0.32m },
                new InventoryItem { Name = "Tinta CMYK", Category = "Insumo", Available = 18, Unit = "%", Minimum = 30, Supplier = "Ink Prime", UnitCost = 460m },
                new InventoryItem { Name = "Lona brilho", Category = "Lona", Available = 12, Unit = "m", Minimum = 25, Supplier = "Visual Pack", UnitCost = 22m },
                new InventoryItem { Name = "Bobina adesiva", Category = "Vinil", Available = 44, Unit = "m", Minimum = 20, Supplier = "Print Suprimentos", UnitCost = 31m });
        }

        if (!await db.SystemSettings.AnyAsync())
        {
            db.SystemSettings.Add(new SystemSettings
            {
                CompanyName = "Vera Grafica Digital",
                CompanyEmail = "atendimento@printflowpro.com.br",
                CompanyPhone = "(11) 98888-2026",
                AutoStockDeductionEnabled = false,
                StockDeductionTriggerStatus = OrderStatus.InProduction,
                RequireAdminPasswordForSensitiveActions = false
            });
        }

        await db.SaveChangesAsync();
    }

    private static Product Product(
        string slug,
        string name,
        string category,
        string description,
        string imageUrl,
        decimal basePrice,
        int baseDeadline,
        bool allowPickupPayment,
        int[] quantities,
        (string Name, decimal Price, int Days)[] sizes,
        (string Name, decimal Price, int Days)[] materials,
        (string Name, decimal Price, int Days)[] printModes,
        (string Name, decimal Price, int Days)[] finishings)
    {
        var product = new Product
        {
            Slug = slug,
            Name = name,
            Category = category,
            Description = description,
            ImageUrl = imageUrl,
            BasePrice = basePrice,
            BaseDeadlineDays = baseDeadline,
            AllowPickupPayment = allowPickupPayment
        };

        product.Quantities = quantities.Select(quantity => new ProductQuantity { Product = product, Quantity = quantity }).ToList();
        product.Options.AddRange(sizes.Select(item => Option(product, "size", item)));
        product.Options.AddRange(materials.Select(item => Option(product, "material", item)));
        product.Options.AddRange(printModes.Select(item => Option(product, "printMode", item)));
        product.Options.AddRange(finishings.Select(item => Option(product, "finishing", item)));
        return product;
    }

    private static ProductOption Option(Product product, string type, (string Name, decimal Price, int Days) item)
    {
        return new ProductOption
        {
            Product = product,
            Type = type,
            Name = item.Name,
            PriceDelta = item.Price,
            DeadlineDeltaDays = item.Days
        };
    }
}
