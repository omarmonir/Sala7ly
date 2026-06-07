using Sala7ly.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sala7ly.DAL.Repositories.Abstraction
{
    public interface IServiceCategoryRepository : IGenericRepository<ServiceCategory>
    {
        // inherits Add, Delete, Update, GetAll, GetById
        // override GetById and GetAll to include SubCategories
    }
}
