using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MVVMBase
{
    public abstract class ObservableBase : INotifyPropertyChanged
    {
        #region Constructors
        public ObservableBase()
        {
            // key = independent property, value = list of properties to raise when key is raised
            PropertyRaiseChain = new Dictionary<string, List<string>>();

            // Reflect on self, getting all properties with [INPCDepends()] and
            // using the constructor arguments as keys for the new chain dictionary
            foreach (var dependentProp in this.GetType().GetProperties())
            {
                var propAttributes = dependentProp.GetCustomAttributes(typeof(INPCDependsAttribute), false);

                // should only ever be one or zero entries in propAttributes, but this is easier to write and less smelly
                foreach (INPCDependsAttribute attribute in propAttributes)
                {
                    foreach (string independentPropName in attribute.TriggerProperties)
                    {
                        if (!PropertyRaiseChain.ContainsKey(independentPropName))
                            PropertyRaiseChain.Add(independentPropName, new List<string>());
                        PropertyRaiseChain[independentPropName].Add(dependentProp.Name);
                    }
                }
            }

            // todo work this out at compile time?? probably not possible (yet)
            foreach (string key in PropertyRaiseChain.Keys)
                CycleCheck(key);
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Methods
        private void CycleCheck(string input, HashSet<string> currentCycle = null)
        {
            currentCycle = currentCycle == null ? new HashSet<string>() : currentCycle;

            try
            {
                if (currentCycle.Contains(input))
                {
                    throw new Exception("cycle detected, printing chain to console");
                }
                if (PropertyRaiseChain.ContainsKey(input))
                {
                    currentCycle.Add(input);
                    foreach (string entry in PropertyRaiseChain[input])
                        CycleCheck(entry, currentCycle);
                    currentCycle.Remove(input);
                }
            }
            catch (Exception)
            {
                Console.WriteLine(input);
                throw;
            }
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (Comparison<T>.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            this.VerifyPropertyName(propertyName);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

                if (PropertyRaiseChain.ContainsKey(propertyName))
                {
                    foreach (string dependentProp in PropertyRaiseChain[propertyName])
                    {
                        RaisePropertyChanged(dependentProp);
                    }
                }
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;
                if (this.ThrowOnInvalidPropertyName)
                {
                    throw new Exception(msg);
                }
                else
                {
                    Debug.Fail(msg);
                }
            }
        }
        #endregion

        #region Fields
        private bool ThrowOnInvalidPropertyName = true;
        private Dictionary<string, List<string>> PropertyRaiseChain;
        #endregion
    }
}
