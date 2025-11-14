using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(new[]
        {
            new { Id = 1, Name = "Laptop", Price = 50000 },
            new { Id = 2, Name = "Mobile", Price = 20000 }
        });
    }
}
