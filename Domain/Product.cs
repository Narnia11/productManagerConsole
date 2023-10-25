using System.ComponentModel.DataAnnotations;
//using Microsoft.EntityFrameworkCore;

namespace ProductManager.Domain;

//[Index(nameof(SerialNum), IsUnique = true)]
class Product
{
    public int Id { get; set; }

    
    [MaxLength(50)]
    public required string ProductName { get; set; }

   
    [MaxLength(10)]
    public required string SerialNum { get; set; }


    [MaxLength(50)]
    public required string ProductDesc { get; set; }

   
    [MaxLength(100)]
    public required string ImageUrl { get; set; }

    public int Price { get; set; }
}




