using System;
using System.Collections.Generic;
using System.Text;

namespace TTerminal.DataSets
{
    public class UsingDSAttribute : Attribute
    {
        public Type[] DSArr;

        public UsingDSAttribute(Type[] DSArr)
        {
            this.DSArr = DSArr;
        }
    }
    public abstract class TTDataSet
    {
        public virtual Type BaseType => null;
        protected virtual bool RunOnce => false;

        private Dictionary<string, object> SavedDataSet = null;
        
        public Dictionary<string, object> GetData()
        {
            if (SavedDataSet != null)
                return SavedDataSet;

            Dictionary<string, object> genDataSet = GenerateData();

            if (RunOnce)
                SavedDataSet = genDataSet;

            return genDataSet;
        }

        protected abstract Dictionary<string, object> GenerateData();
    }
}
