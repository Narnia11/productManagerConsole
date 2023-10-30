namespace ProductManager.DTO;

public class CreateProductRequest
{
    public string ProductName { get; set; }

    public string SerialNum { get; set; }

    public string ProductDesc  { get; set; }

    public string ImageUrl { get; set; }

    public int Price { get; set; }
}