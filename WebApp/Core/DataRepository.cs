using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Core
{
    public class DataRepository
    {
        private ApplicationDbContext ApplicationDbContext { get; }

        public DataRepository(ApplicationDbContext applicationDbContext)
        {
            ApplicationDbContext = applicationDbContext;
        }

        public void Add(MdnData data)
        {
            ApplicationDbContext.Items.AddRange(data.Items);
            ApplicationDbContext.MdnData.Add(data);

            ApplicationDbContext.SaveChanges();
        }

        public void Remove()
        {
            var items = ApplicationDbContext.Items;
            ApplicationDbContext.Items.RemoveRange(items);
            var mdnData = ApplicationDbContext.MdnData;
            ApplicationDbContext.MdnData.RemoveRange(mdnData);

            ApplicationDbContext.SaveChanges();
        }

        public MdnData GetLast()
        {
            var mdnData = ApplicationDbContext.MdnData.Include(mdn => mdn.Items).FirstOrDefault();
            return mdnData ?? new MdnData();
        }

        public bool Exists(DateTime dateTime)
        {
            var key = default(DateTime);
            var mdnData =
                ApplicationDbContext.MdnData.FirstOrDefault(mdn => DateTime.Compare(mdn.Timestamp, dateTime) == 0);
            if (mdnData != null && mdnData.Count > 0)
            {
                key = mdnData.Timestamp;
            }

            return key != default;
        }

        public int Count()
        {
            return ApplicationDbContext.Items.Count();
        }
    }
}
