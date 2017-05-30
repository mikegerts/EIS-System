using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EIS.SystemJobApp.Models;

namespace EIS.SystemJobApp.Workers
{
    public class BigCommerceSuggestedCategoriesWorker : JobWorker
    {
        public BigCommerceSuggestedCategoriesWorker(SystemJob job) : base(job)
        {

        }

        protected override void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void DoPostWorkCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
