using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class ProductController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {

            List<Product> productList = _unitOfWork.product.GetAll().ToList();
            return View(productList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.category.GetAll().Select(u => new SelectListItem()
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };

            if (id==null || id == 0)
            {

                //Create
            return View(productVM);

            } else
            {
                //Update
                productVM.Product = _unitOfWork.product.Get(u => u.Id == id);
                return View(productVM);
            }

        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {


            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);  //this will give us a random name of a file.
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.Image))
                    {
                        //delete old image

                        var oldImgPath = Path.Combine(wwwRootPath, productVM.Product.Image.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImgPath))
                        {
                            System.IO.File.Delete(oldImgPath);
                        }

                        using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }
                        productVM.Product.Image = @"\images\product\" + fileName;

                        if (productVM.Product.Id == 0)
                        {
                            _unitOfWork.product.Add(productVM.Product);
                        }
                        else
                        {
                            _unitOfWork.product.Update(productVM.Product);
                        }

                        
                    }

                    _unitOfWork.Save();
                    TempData["success"] = "Product added successfully";
                }

            }
                return RedirectToAction("Index");
        }


        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? Product = _unitOfWork.product.Get(x => x.Id == id);

            if (Product == null)
            {
                return NotFound();
            }
            return View(Product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {

            Product? obj = _unitOfWork.product.Get(x => x.Id == id);

            if (obj == null)
            {
                return NotFound();
            }
            _unitOfWork.product.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully";

            return RedirectToAction("Index");

        }


    }
}

