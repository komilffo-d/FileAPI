using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Interfaces
{
    public interface IDbInitializer<T> where T: ModelBuilder
    {
        public IDbInitializer<T> Seed();

        public IDbInitializer<T> CreateThirdParty();
    }
}
