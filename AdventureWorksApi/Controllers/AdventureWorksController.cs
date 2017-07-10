using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AdventureWorksApi.Models;

namespace AdventureWorksApi.Controllers
{
    //[Route("api/[controller]")]
    public class AdventureWorksController : Controller
    {
        private readonly AdventureWorks2014Context _dbContext;
        public AdventureWorksController(AdventureWorks2014Context dbContext)
        {
            _dbContext = dbContext;
        }

        [Route("api/adventureworks")]
        [HttpGet]
        public IActionResult GetTopFive()
        {
            var bikeSales =
                            (from b in _dbContext.Product
                             where b.ProductSubcategory.ProductCategory.ProductCategoryId == 1
                             join s in _dbContext.SalesOrderDetail
                             on b.ProductId equals s.ProductId
                             select new
                             {
                                 Bike = b,
                                 Sale = s
                             });

            var topFive =
                            (from bS in bikeSales
                             group bS by bS.Bike.ProductModel into grpModels
                             orderby grpModels.Count() descending
                             select new
                             {
                                 ModelName = grpModels.Key.Name,
                                 ModelCount = grpModels.Count(),
                                 Product = (from g in grpModels
                                            group g by g.Bike into grpBikes
                                            orderby grpBikes.Count() descending
                                            select new
                                            {
                                                Bikes = grpBikes.Key.Name,
                                                Id = grpBikes.Key.ProductId,
                                                Count = grpBikes.Count()
                                            }).FirstOrDefault()
                             }
                            ).Take(5);

            return new ObjectResult(topFive);
        }


        // GET api/adventureworks/details/id
        [Route("api/adventureworks/details/{id}")]
        [HttpGet]
        public IActionResult GetDetails(int id)
        {
            var productDescriptionsAndPhoto = from p in _dbContext.Product
                                              join dc in _dbContext.ProductModelProductDescriptionCulture
                                              on p.ProductModelId equals dc.ProductModelId
                                              join d in _dbContext.ProductDescription
                                              on dc.ProductDescriptionId equals d.ProductDescriptionId
                                              join ppPh in _dbContext.ProductProductPhoto
                                              on p.ProductId equals ppPh.ProductId
                                              join ph in _dbContext.ProductPhoto
                                              on ppPh.ProductPhotoId equals ph.ProductPhotoId
                                              where dc.CultureId == "en"
                                              select new
                                              {
                                                  Product = p,
                                                  Description = d,
                                                  Photo = ph
                                              };

            var details = from p in productDescriptionsAndPhoto
                          where p.Product.ProductId == id
                          select new
                          {
                              Name = p.Product.Name,
                              Number = p.Product.ProductNumber,
                              Price = p.Product.ListPrice,
                              Photo = p.Photo,
                              Description = p.Description.Description,
                              Model = p.Product.ProductModel.Name,
                              OtherBikesOfSameModel = p.Product.ProductModel.Product
                          };

            return new ObjectResult(details);
        }

        // GET api/adventureworks/search/str
        [Route("api/adventureworks/search/{str}")]
        [HttpGet]
        public IActionResult GetSearchResults(string str)
        {
            var searchResults = from p in _dbContext.Product
                                join dc in _dbContext.ProductModelProductDescriptionCulture
                                on p.ProductModelId equals dc.ProductModelId
                                join d in _dbContext.ProductDescription
                                on dc.ProductDescriptionId equals d.ProductDescriptionId
                                where dc.CultureId == "en" &&
                                (p.Name.ToLower().Contains(str.ToLower()) || d.Description.ToLower().Contains(str.ToLower()))
                                select new
                                {
                                    Product = p,
                                    Description = d
                                };
            return new ObjectResult(searchResults);
        }
    }
}
