using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMBase
{
    public abstract class ViewModelBase : ObservableBase
    {
        #region Properties and Fields
        private string _DisplayName;
        public string DisplayName
        {
            get { return _DisplayName; }
            set { SetField(ref _DisplayName, value); }
        }
        #endregion
    }
}
