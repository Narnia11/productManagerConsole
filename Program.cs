using System.Net;
using System.Text;
using System.Text.Json;
using ProductManager.Domain;
using static System.Console;

namespace ProductManager;
class Program
{
    static readonly HttpClient httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7000")
    };

    static void Main()
    {
        ShowMainMenu();
    }

    private static void ShowMainMenu()
    {
        CursorVisible = false;
        Title = "Product-Manager";

        while (true)
        {
            WriteLine("1. Ny produkt");
            WriteLine("2. Sök produkt");
            WriteLine("3. Avsluta");

            var keyPressed = ReadKey(intercept: true);

            Clear();

            switch (keyPressed.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:

                    ShowRegisterProduct();

                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:

                    ShowSearchProduct();

                    break;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:

                    Environment.Exit(0);

                    return;
            }

            Clear();
        }
    }
    private static void ShowRegisterProduct()
    {
        bool returnToDisplay = false;

        do
        {
            var productName = GetUserInput("       Namn");

            var serialNum = GetUserInput("        SKU");

            var productDesc = GetUserInput("Beskrivning");

            var imageUrl = GetUserInput(" Bild (URL)");

            var price = GetUserInputForInt("       Pris");

            WriteLine("Är detta korrekt? (J)a (N)ej");
            var yesOrNoKey = ReadKey(intercept: true);

            if (yesOrNoKey.Key == ConsoleKey.J)
            {
                try
                {
                    var product = new Product
                    {
                        ProductName = productName,
                        SerialNum = serialNum,
                        ProductDesc = productDesc,
                        ImageUrl = imageUrl,
                        Price = price
                    };
                    Clear();

                    try
                    {
                        SaveProduct(product);

                        WriteLine("Product sparad");
                    }
                    catch
                    {
                        WriteLine("Product redan registrerad");
                    }

                    Thread.Sleep(2000);
                    returnToDisplay = false;
                }
                catch (Exception ex)
                {
                    WriteLine(ex.Message);
                    Thread.Sleep(2000);
                    Clear();
                    returnToDisplay = true;
                }
            }
            else if (yesOrNoKey.Key == ConsoleKey.N)
            {
                Clear();
                returnToDisplay = true;
            }
        } while (returnToDisplay);
    }

    private static void SaveProduct(Product product)
    {
        // 1 - Serialisera product (alltså från product-objektet, skapa motsvarande JSON-objekt)
        // { "make": "Tesla", model: "X", ... }
        var json = JsonSerializer.Serialize(product);

        // 2 - Skicka JSON till web API:et
        var body = new StringContent(
          json,
          Encoding.UTF8,
          // Beskriver formatet på data
          "application/json");

        // HTTP POST https://localhost:7000/products
        var response = httpClient.PostAsync("products", body).Result;

        // 3 - Om det gick bra (2xx - i detta fallet "201 Created"), avsluta metoden, annars (400 Bad Request) 
        //     kasta en exception

        // Kommer kasta en exception om statuskoden inte är 2xx
        response.EnsureSuccessStatusCode();
    }

    private static string GetUserInput(string label)
    {
        CursorVisible = true;

        Write($"{label}: ");

        return ReadLine() ?? "";
    }

    private static int GetUserInputForInt(string label)
    {
        int result;
        bool validInput = false;

        do
        {
            Write($"{label}: ");
            string userInput = ReadLine();

            if (int.TryParse(userInput, out result))
            {
                validInput = true;
            }
            else
            {
                WriteLine("Priset måste anges som ett nummer!");
            }
        } while (!validInput);

        return result;
    }

    private static void ShowSearchProduct()
    {
        Write("SKU: ");

        string serialNum = ReadLine();

        Clear();

        var product = SearchProduct(serialNum);

        if (product is not null)
        {
            bool returnToDisplay = false;

            do
            {
                WriteLine($"Namn: {product.ProductName}");
                WriteLine($"SKU: {product.SerialNum}");
                WriteLine($"Beskrivning: {product.ProductDesc}");
                WriteLine($"Bild (URL): : {product.ImageUrl}");
                WriteLine($"Pris: {product.Price}");
                WriteLine("(R)adera");
                var deleteKey = ReadKey(intercept: true);

                if (deleteKey.Key == ConsoleKey.R)
                {
                    Clear();
                    WriteLine($"Namn: {product.ProductName}");
                    WriteLine($"SKU: {product.SerialNum}");
                    WriteLine($"Beskrivning: {product.ProductDesc}");
                    WriteLine($"Bild (URL): : {product.ImageUrl}");
                    WriteLine($"Pris: {product.Price}");
                    WriteLine("Radera produkt? (J)a (N)ej");

                    var yesOrNoKey = ReadKey(intercept: true);

                    if (yesOrNoKey.Key == ConsoleKey.J)
                    {
                        //context.Product.Remove(product);
                        //context.SaveChanges();
                        //WriteLine("Produkt raderad");
                        //Thread.Sleep(2000);

                        DeleteProduct(product);
                        returnToDisplay = false;
                    }
                    else if (yesOrNoKey.Key == ConsoleKey.N)
                    {
                        Clear();
                        returnToDisplay = true; // Return to display product details
                    }
                }
                EscapeKeyPressed(ConsoleKey.Escape);
            } while (returnToDisplay);
        }
        else
        {
            WriteLine("Produkt finns ej");
            Thread.Sleep(20000);
        }
    }

    private static Product? SearchProduct(string serialNum)
    {
 try
    {
        var response = httpClient.GetAsync($"https://localhost:7000/products?serialNum={serialNum}")
            .Result;

        if (response.IsSuccessStatusCode)
        {
            var json = response.Content.ReadAsStringAsync().Result;
            try
            {
                var product = JsonSerializer.Deserialize<Product>(json);
                if (product != null)
                {
                    return product;
                }
            }
            catch (JsonException ex)
            {
                WriteLine($"Error deserializing the response: {ex.Message}");
                WriteLine("Response content: " + json);
            }
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            WriteLine("Product not found");
        }
        else
        {
            WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            WriteLine("Response content: " + response.Content.ReadAsStringAsync().Result);
        }
    }
    catch (Exception ex)
    {
        WriteLine("An error occurred: " + ex.Message);
    }

    return null;

        // try
        // {
        //     //HttpResponseMessage response = httpClient.GetAsync($"https://localhost:7000/products?serialNum={serialNum}")
        //     var response = httpClient.GetAsync($"https://localhost:7000/products?serialNum={serialNum}")
        //          .Result;

        //     if (response.IsSuccessStatusCode)
        //     {
        //         var json = response.Content.ReadAsStringAsync().Result;
        //         var product = JsonSerializer.Deserialize<Product>(json);

        //         if (product != null)
        //         {
        //             return product;
        //         }
        //     }
        // }
        // catch (Exception ex)
        // {
        //     WriteLine("An error occurred: " + ex.Message);
        // }

        // return null;
    }

    private static void EscapeKeyPressed(ConsoleKey key)
    {
        Clear();
        ShowMainMenu();

        //بجای دوخط کد بالا میتوان مثل زیر نوشت 
        //یعنی تاوقتی یک دستوری را انجام بده که دکمه فشرده شده مخالف آن دکمه ای باشد که 
        // داخل پرانتز جلوی نام فانکشن هنگام فراخوانی اش مینویسیم.
        //مثل خط 209 که دکمه اسکیپ را داخل پرانتز نوشتیم
        // یعنی تا اسکیپ را نزدیم کارهای بالای خط209 را انجام بده اگر اسکیپ زدیم خط بعدی کد را اجرا کن

        //while (ReadKey(true).Key != key) ; 
    }

    private static void DeleteProduct(Product product)
    {
        //HttpResponseMessage response = httpClient.DeleteAsync($"products/{product.Id}").Result;
        var response = httpClient.DeleteAsync($"products/{product.Id}").Result;

        if (response.IsSuccessStatusCode)
        {
            WriteLine("Produkt raderad");
        }
        else
        {
            WriteLine("Fel vid radering av produkten");
        }
        Thread.Sleep(2000);
    }
}