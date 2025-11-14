using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(new[]
        {
            new { Id = 1, User = "Ashay", Product = "Laptop" },
            new { Id = 2, User = "Pusdekar", Product = "Mobile" }
        });
    }
}
