using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Osint.Source.One.Db;

namespace Osint.Source.One.Controllers;

[ApiController]
[Route("[controller]")]
public class CarController(CarDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public Car Get()
    {
        var carEntity = dbContext.Cars
            .AsNoTracking()
            .First();
        
        return new Car(carEntity.Owner, carEntity.Plate);
    }
}