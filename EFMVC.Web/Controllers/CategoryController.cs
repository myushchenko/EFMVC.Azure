﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using EFMVC.Web.ViewModels;
using EFMVC.Domain.Commands;
using EFMVC.Core.Common;
using EFMVC.Web.Core.Extensions;
using EFMVC.CommandProcessor.Dispatcher;
using EFMVC.Data.Repositories;
using EFMVC.Web.Core.ActionFilters;
using EFMVC.Model;
using Microsoft.ApplicationServer.Caching;
using System;
using EFMVC.Web.Caching;
namespace EFMVC.Web.Controllers
{
    [CompressResponse]
    public class CategoryController : Controller
    {
        private readonly ICommandBus commandBus;
        private readonly ICategoryRepository categoryRepository;
        private readonly ICacheProvider cache;
        public CategoryController(ICommandBus commandBus, ICategoryRepository categoryRepository,ICacheProvider cache)
        {
            this.commandBus = commandBus;
            this.categoryRepository = categoryRepository;
            this.cache = cache;
        }       
        public ActionResult Index()
        {
            IEnumerable<Category> categories;
            var cachedCategories=cache.Get("categories");

            if (cachedCategories != null)
            {
                categories = cachedCategories as IEnumerable<Category>;
            }
            else
            {
                categories = categoryRepository.GetAll();
                cache.Put("categories", categories);
            }            
            return View(categories);
        }      
        
        public ActionResult Details(int id)
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {

            var category = categoryRepository.GetById(id);
            var viewModel = new CategoryFormModel(category);
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(CategoryFormModel form)
        {
            if(ModelState.IsValid)
            {
                var command = new CreateOrUpdateCategoryCommand(form.CategoryId,form.Name, form.Description);
               IEnumerable<ValidationResult> errors=  commandBus.Validate(command);
                ModelState.AddModelErrors(errors);              
               if (ModelState.IsValid)
               {
                   var result = commandBus.Submit(command);
                   if (result.Success)
                   {
                       //updating data to Cache
                       var categories = categoryRepository.GetAll();
                       cache.Put("categories", categories);
                       return RedirectToAction("Index");
                   }
               }                
            }   
            //if fail
            if (form.CategoryId == 0)
                return View("Create",form);
            else
                return View("Edit", form);
        }         
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var command = new DeleteCategoryCommand { CategoryId = id };
            var result = commandBus.Submit(command);           
            var categories = categoryRepository.GetAll();
            return PartialView("_CategoryList", categories);      
        }       
    }
}
